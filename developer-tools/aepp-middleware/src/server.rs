use diesel::sql_query;

use coinbase::coinbase;
use compiler::*;
use models::*;
use node::Node;

use chrono::prelude::*;
use diesel::RunQueryDsl;
use loader::PGCONNECTION;
use loader::SQLCONNECTION;
use regex::Regex;
use rocket;
use rocket::http::{Header, Method, Status};
use rocket::response::Response;
use rocket::State;
use rocket_contrib::json::*;
use rocket_cors;
use rocket_cors::{AllowedHeaders, AllowedOrigins};
use rust_decimal::Decimal;
use serde_json;
use std::io::Cursor;
use std::path::PathBuf;

pub struct MiddlewareServer {
    pub node: Node,
    pub dest_url: String, // address to forward to
    pub port: u16,        // port to listen on
}

// SQL sanitizing method to prevent injection attacks.
fn sanitize(s: &String) -> String {
    s.replace("'", "\\'")
}

fn check_object(s: &str) -> () {
    lazy_static! {
        static ref OBJECT_REGEX: Regex = Regex::new(
            "[a-z][a-z]_[123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz]{38,60}"
        )
        .unwrap();
    }
    if !OBJECT_REGEX.is_match(s) {
        panic!("Invalid input"); // be paranoid
    };
}

/*
 * GET handler for Node
 */
#[get("/<path..>", rank = 6)]
fn node_get_handler(state: State<MiddlewareServer>, path: PathBuf) -> Response {
    let http_response = match state
        .node
        .get_naked(&String::from("/v2/"), &String::from(path.to_str().unwrap()))
    {
        Ok(x) => x,
        Err(e) => {
            info!("error response:\n{}", e.to_string());
            return Response::build()
                .status(Status::new(500, "An error occurred"))
                .finalize();
        }
    };
    debug!("http_response is {:?}", http_response);
    let mut response = Response::build();
    if let Some(status) = http_response.status {
        response.status(Status::from_code(status.parse::<u16>().unwrap()).unwrap());
    }
    for header in http_response.headers.keys() {
        response.raw_header(
            header.clone(),
            http_response.headers.get(header).unwrap().clone(),
        );
    }
    response.sized_body(Cursor::new(http_response.body.unwrap()));
    response.finalize()
}

fn node_get_json(state: State<MiddlewareServer>, path: PathBuf) -> Json<serde_json::Value> {
    Json(serde_json::from_str(&node_get_handler(state, path).body_string().unwrap()).unwrap())
}

/*
 * POST handler for Node
 */
#[post("/<path..>", format = "application/json", data = "<body>")]
fn node_post_handler(
    state: State<MiddlewareServer>,
    path: PathBuf,
    body: Json<serde_json::Value>,
) -> Response {
    let (headers, body) = state
        .node
        .post_naked(
            &String::from("/v2/"),
            &String::from(path.to_str().unwrap()),
            body.to_string(),
        )
        .unwrap();

    let mut response = Response::build();
    if let Some(status) = headers.get("status") {
        response.status(Status::from_code(status.parse::<u16>().unwrap()).unwrap());
    }
    for header in headers.keys() {
        if header.eq("status") {
            continue;
        }
        response.raw_header(header.clone(), headers.get(header).unwrap().clone());
    }
    response.sized_body(Cursor::new(body));
    response.finalize()
}

/*
 * Node's only endpoint which lives outside of /v2/...
 */
#[get("/")]
fn node_api_handler(state: State<MiddlewareServer>) -> Result<Json<serde_json::Value>, Status> {
    let http_response = state
        .node
        .get_naked(&String::from("/api"), &String::from(""))
        .unwrap();
    Ok(Json(
        serde_json::from_str(&http_response.body.unwrap()).unwrap(),
    ))
}

#[get("/generations/current", rank = 1)]
fn current_generation(state: State<MiddlewareServer>) -> Result<Json<serde_json::Value>, Status> {
    let _height = KeyBlock::top_height(&PGCONNECTION.get().unwrap()).unwrap();
    generation_at_height(state, _height)
}

#[get("/generations/height/<height>", rank = 1)]
fn generation_at_height(
    state: State<MiddlewareServer>,
    height: i64,
) -> Result<Json<serde_json::Value>, Status> {
    match JsonGeneration::get_generation_at_height(
        &SQLCONNECTION.get().unwrap(),
        &PGCONNECTION.get().unwrap(),
        height,
    ) {
        Some(x) => Ok(Json(
            serde_json::from_str(&serde_json::to_string(&x).unwrap()).unwrap(),
        )),
        None => {
            info!("Generation not found at height {}", height);
            let mut path = std::path::PathBuf::new();
            path.push(format!("generations/height/{}", height));
            return Ok(node_get_json(state, path));
        }
    }
}

#[get("/key-blocks/current/height", rank = 1)]
fn current_key_block(_state: State<MiddlewareServer>) -> Json<JsonValue> {
    let _height = KeyBlock::top_height(&PGCONNECTION.get().unwrap()).unwrap();
    Json(json!({
        "height" : _height,
    }))
}

#[get("/key-blocks/height/<height>", rank = 1)]
fn key_block_at_height(state: State<MiddlewareServer>, height: i64) -> Json<JsonValue> {
    let key_block = match KeyBlock::load_at_height(&PGCONNECTION.get().unwrap(), height) {
        Some(x) => x,
        None => {
            info!("Generation not found at height {}", height);
            return Json(
                serde_json::from_str(&serde_json::to_string(&state.node.get_generation_at_height(height).unwrap())
                    .unwrap()).unwrap(),
            );
        }
    };
    info!("Serving key block {} from DB", height);
    Json(serde_json::from_str(&serde_json::to_string(&JsonKeyBlock::from_key_block(&key_block)).unwrap()).unwrap())
}

#[catch(400)]
fn error400() -> Json<serde_json::Value> {
    Json(
        serde_json::from_str(
            r#"
{
  "reason": "Invalid input"
}"#,
        )
        .unwrap(),
    )
}

#[catch(404)]
fn error404() -> Json<serde_json::Value> {
    Json(
        serde_json::from_str(
            r#"
{
  "reason": "Not found"
}"#,
        )
        .unwrap(),
    )
}

#[get("/transactions/<hash>")]
fn transaction_at_hash(
    state: State<MiddlewareServer>,
    hash: String,
) -> Result<Json<JsonTransaction>, Status> {
    if let Some(tx) = Transaction::load_at_hash(&PGCONNECTION.get().unwrap(), &hash) {
        return Ok(Json(JsonTransaction::from_transaction(&tx)));
    }

    info!("Transaction not found at hash {}", &hash);
    let mut path = std::path::PathBuf::new();
    path.push(format!("transactions/{}", hash));
    let mut response = node_get_handler(state, path);
    if response.status() == Status::Ok {
        let body = response.body_string().unwrap();
        let jt: JsonTransaction = serde_json::from_str(&body).unwrap();
        return Ok(Json(jt));
    }
    Err(response.status())
}

#[get("/key-blocks/hash/<hash>", rank = 1)]
fn key_block_at_hash(
    state: State<MiddlewareServer>,
    hash: String,
) -> Result<Json<serde_json::Value>, Status> {
    let key_block = match KeyBlock::load_at_hash(&PGCONNECTION.get().unwrap(), &hash) {
        Some(x) => x,
        None => {
            info!("Key block not found at hash {}", &hash);
            let mut path = std::path::PathBuf::new();
            path.push(format!("/key-blocks/hash/{}", hash));
            return Ok(node_get_json(state, path));
        }
    };
    debug!("Serving key block {} from DB", hash);
    Ok(Json(
        serde_json::from_str(
            &serde_json::to_string(&JsonKeyBlock::from_key_block(&key_block)).unwrap(),
        )
        .unwrap(),
    ))
}

#[get("/micro-blocks/hash/<hash>/transactions", rank = 1)]
fn transactions_in_micro_block_at_hash(
    _state: State<MiddlewareServer>,
    hash: String,
) -> Json<JsonTransactionList> {
    check_object(&hash);
    let sql = format!("select t.* from transactions t, micro_blocks m where t.micro_block_id = m.id and m.hash = '{}'", sanitize(&hash));
    let transactions: Vec<Transaction> =
        sql_query(sql).load(&*PGCONNECTION.get().unwrap()).unwrap();

    let json_transactions = transactions
        .iter()
        .map(JsonTransaction::from_transaction)
        .collect();
    Json(JsonTransactionList {
        transactions: json_transactions,
    })
}

#[get("/micro-blocks/hash/<hash>/header", rank = 1)]
fn micro_block_header_at_hash(
    _state: State<MiddlewareServer>,
    hash: String,
) -> Result<Json<JsonValue>, Status> {
    let sql = "select m.hash, k.height, m.pof_hash, m.prev_hash, m.prev_key_hash, m.signature, m.state_hash, m.time_, m.txs_hash, m.version from micro_blocks m, key_blocks k where m.key_block_id=k.id and m.hash = $1";
    let rows = SQLCONNECTION.get().unwrap().query(sql, &[&hash]).unwrap();
    #[derive(Serialize)]
    struct JsonMicroBlock {
        hash: String,
        height: i64,
        pof_hash: String,
        prev_hash: String,
        prev_key_hash: String,
        signature: String,
        state_hash: String,
        time: i64,
        txs_hash: String,
        version: i32,
    };
    if rows.len() > 0 {
        let r = rows.get(0);
        let val = json!(JsonMicroBlock {
            hash: r.get(0),
            height: r.get(1),
            pof_hash: r.get(2),
            prev_hash: r.get(3),
            prev_key_hash: r.get(4),
            signature: r.get(5),
            state_hash: r.get(6),
            time: r.get(7),
            txs_hash: r.get(8),
            version: r.get(9),
        });
        Ok(Json(val))
    } else {
        Err(Status::new(404, "Block not in DB"))
    }
}

/*
 * Gets the amount spent, and number of transactions between specific dates,
 * broken up by day.
 *
 */
#[get("/transactions/rate/<from>/<to>")]
fn transaction_rate(_state: State<MiddlewareServer>, from: String, to: String) -> Json<JsonValue> {
    let from = NaiveDate::parse_from_str(&from, "%Y%m%d").unwrap();
    let to = NaiveDate::parse_from_str(&to, "%Y%m%d").unwrap();
    debug!("{:?} - {:?}", from, to);
    Json(json!(Transaction::rate(
        &SQLCONNECTION.get().unwrap(),
        from,
        to
    )
    .unwrap()))
}

/*
 * Gets this size of the chain at some height
 */
#[get("/size/height/<height>")]
fn size(_state: State<MiddlewareServer>, height: i32) -> Json<JsonValue> {
    let size = size_at_height(&SQLCONNECTION.get().unwrap(), height).unwrap();
    Json(json!({
        "size": size,
    }))
}

/*
 * return the current size of the DB
 */
#[get("/size/current")]
fn current_size(_state: State<MiddlewareServer>) -> Json<JsonValue> {
    let _height = KeyBlock::top_height(&PGCONNECTION.get().unwrap()).unwrap();
    size(_state, _height as i32)
}

/*
 * Gets this size of the chain at some height
 */
#[get("/count/height/<height>")]
fn count(_state: State<MiddlewareServer>, height: i32) -> Json<JsonValue> {
    let count = count_at_height(&SQLCONNECTION.get().unwrap(), height).unwrap();
    Json(json!({
        "count": count,
    }))
}

#[get("/count/current")]
fn current_count(_state: State<MiddlewareServer>) -> Json<JsonValue> {
    let _height = KeyBlock::top_height(&PGCONNECTION.get().unwrap()).unwrap();
    count(_state, _height as i32)
}

/*
 * Gets count of transactions for an account
 */
#[get("/transactions/account/<account>/count?<txtype>")]
fn transaction_count_for_account(
    _state: State<MiddlewareServer>,
    account: String,
    txtype: Option<String>,
) -> Json<JsonValue> {
    check_object(&account);
    let s_acc = sanitize(&account);
    let txtype_sql: String = match txtype {
        Some(txtype) => format!(" '{}') and tx_type ilike '{}' ", s_acc, sanitize(&txtype)),
        _ => format!(" '{}') ", s_acc),
    };
    let sql = format!(
        "select count(1) from transactions where ( \
         tx->>'sender_id'='{}' or \
         tx->>'account_id' = '{}' or \
         tx->>'recipient_id'='{}' or \
         tx->>'owner_id' = {} ",
        s_acc, s_acc, s_acc, txtype_sql
    );
    debug!("{}", sql);
    let rows = SQLCONNECTION.get().unwrap().query(&sql, &[]).unwrap();
    let count: i64 = rows.get(0).get(0);
    Json(json!({
        "count": count,
    }))
}

fn offset_limit(limit: Option<i32>, page: Option<i32>) -> (String, String) {
    let offset_sql;
    let limit_sql = match limit {
        None => {
            offset_sql = String::from(" 0 ");
            String::from(" all ")
        }
        Some(x) => {
            offset_sql = match page {
                None => String::from(" 0 "),
                Some(y) => format!(" {} ", (y - 1) * x),
            };
            format!(" {} ", x)
        }
    };
    (offset_sql, limit_sql)
}

/*
 * Gets all transactions for an account
 */
#[get("/transactions/account/<account>?<limit>&<page>&<txtype>")]
fn transactions_for_account(
    _state: State<MiddlewareServer>,
    account: String,
    limit: Option<i32>,
    page: Option<i32>,
    txtype: Option<String>,
) -> Json<Vec<JsonValue>> {
    check_object(&account);
    let s_acc = sanitize(&account);
    let (offset_sql, limit_sql) = offset_limit(limit, page);
    let txtype_sql: String = match txtype {
        Some(txtype) => format!(" '{}') and tx_type ilike '{}' ", s_acc, sanitize(&txtype)),
        _ => format!(" '{}') ", s_acc),
    };
    let sql = format!(
        "SELECT m.time_, t.* FROM transactions t, micro_blocks m WHERE \
         m.id = t.micro_block_id AND \
         (t.tx->>'sender_id'='{}' OR \
         t.tx->>'account_id' = '{}' OR \
         t.tx->>'ga_id' = '{}' OR \
         t.tx->>'caller_id' = '{}' OR \
         t.tx->>'recipient_id'='{}' OR \
         t.tx->>'initiator_id'='{}' OR \
         t.tx->>'responder_id'='{}' OR \
         t.tx->>'from_id'='{}' OR \
         t.tx->>'to_id'='{}' OR \
         t.tx->>'owner_id' = {}\
         order by m.time_ desc \
         limit {} offset {} ",
        s_acc,
        s_acc,
        s_acc,
        s_acc,
        s_acc,
        s_acc,
        s_acc,
        s_acc,
        s_acc,
        txtype_sql,
        limit_sql,
        offset_sql
    );
    info!("{}", sql);

    let mut results: Vec<JsonValue> = Vec::new();
    for row in &SQLCONNECTION.get().unwrap().query(&sql, &[]).unwrap() {
        let time: i64 = row.get(0);
        let block_height: i32 = row.get(3);
        let block_hash: String = row.get(4);
        let hash: String = row.get(5);
        let sig: Option<String> = row.get(6);
        let signatures = Transaction::deserialize_signatures(&sig);
        let tx: serde_json::Value = match row.get_opt(12).unwrap().unwrap() {
            Some(encoded_tx) => Transaction::decode_tx(&encoded_tx),
            _ => row.get(8),
        };
        results.push(json!({
            "time" : time,
            "block_height": block_height,
            "block_hash": block_hash,
            "hash": hash,
            "signatures": signatures,
            "tx": tx,
        }));
    }
    Json(results)
}

/*
 * Gets all transactions for an account to an account
 */
#[get("/transactions/account/<sender>/to/<receiver>")]
fn transactions_for_account_to_account(
    _state: State<MiddlewareServer>,
    sender: String,
    receiver: String,
) -> Json<JsonTransactionList> {
    check_object(&sender);
    check_object(&receiver);
    let s_acc = sanitize(&sender);
    let r_acc = sanitize(&receiver);
    let sql = format!(
        "select * from transactions where \
         tx->>'sender_id'='{}' and \
         tx->>'recipient_id' = '{}' \
         order by id desc",
        s_acc, r_acc
    );
    info!("{}", sql);
    let transactions: Vec<Transaction> =
        sql_query(sql).load(&*PGCONNECTION.get().unwrap()).unwrap();

    let json_transactions = transactions
        .iter()
        .map(JsonTransaction::from_transaction)
        .collect();
    Json(JsonTransactionList {
        transactions: json_transactions,
    })
}

/*
 * Gets transactions between blocks
 */
#[get("/transactions/interval/<from>/<to>?<limit>&<page>&<txtype>")]
fn transactions_for_interval(
    _state: State<MiddlewareServer>,
    from: i64,
    to: i64,
    limit: Option<i32>,
    page: Option<i32>,
    txtype: Option<String>,
) -> Json<JsonTransactionList> {
    let (offset_sql, limit_sql) = offset_limit(limit, page);
    let txtype_sql: String = match txtype {
        Some(txtype) => format!(" {}  and tx_type ilike '{}' ", to, sanitize(&txtype)),
        _ => to.to_string(),
    };
    let sql = format!(
        "select t.* from transactions t where t.block_height >= {} and t.block_height <= {} order by t.block_height desc, t.id desc limit {} offset {}",
        from, txtype_sql, limit_sql, offset_sql
    );
    let transactions: Vec<Transaction> =
        sql_query(sql).load(&*PGCONNECTION.get().unwrap()).unwrap();

    let json_transactions = transactions
        .iter()
        .map(JsonTransaction::from_transaction)
        .collect();
    Json(JsonTransactionList {
        transactions: json_transactions,
    })
}

#[get("/micro-blocks/hash/<hash>/transactions/count")]
/*
 * Gets count of transactions in a microblock
 */
fn transaction_count_in_micro_block(
    _state: State<MiddlewareServer>,
    hash: String,
) -> Json<JsonValue> {
    Json(json!({
        "count": MicroBlock::get_transaction_count(&SQLCONNECTION.get().unwrap(), &hash, ),
    }))
}

#[get("/contracts/transactions/address/<address>")]
fn transactions_for_contract_address(
    _state: State<MiddlewareServer>,
    address: String,
) -> Json<JsonTransactionList> {
    check_object(&address);
    let sql = format!(
        "select t.* from transactions t where \
         t.tx_type='ContractCallTx' and \
         t.tx->>'contract_id' = '{}' or \
         t.id in (select transaction_id from contract_identifiers where \
         contract_identifier='{}') ORDER by t.block_height ASC",
        sanitize(&address),
        sanitize(&address)
    );
    let transactions: Vec<Transaction> =
        sql_query(sql).load(&*PGCONNECTION.get().unwrap()).unwrap();

    let json_transactions = transactions
        .iter()
        .map(JsonTransaction::from_transaction)
        .collect();
    Json(JsonTransactionList {
        transactions: json_transactions,
    })
}

#[get("/contracts/calls/address/<address>")]
fn calls_for_contract_address(
    _state: State<MiddlewareServer>,
    address: String,
) -> Json<Vec<JsonValue>> {
    check_object(&address);
    let sql = "SELECT t.hash, contract_id, caller_id, arguments, callinfo, result FROM \
               contract_calls c join transactions t on t.id=c.transaction_id WHERE \
               contract_id = $1";
    let mut calls = Vec::new();
    for row in &SQLCONNECTION
        .get()
        .unwrap()
        .query(&sql, &[&address])
        .unwrap()
    {
        let transaction_id: String = row.get(0);
        let contract_id: String = row.get(1);
        let caller_id: String = row.get(2);
        let arguments: serde_json::Value = row.get(3);
        let callinfo: serde_json::Value = row.get(4);
        let result: serde_json::Value = row.get(5);
        calls.push(json!({
            "transaction_id": transaction_id,
            "contract_id": contract_id,
            "caller_id": caller_id,
            "arguments": arguments,
            "callinfo": callinfo,
            "result": result,
        }));
    }
    Json(calls)
}

// TODO: Lot of refactoring in the below method
#[get("/generations/<from>/<to>?<limit>&<page>")]
fn generations_by_range(
    _state: State<MiddlewareServer>,
    from: i64,
    to: i64,
    limit: Option<i32>,
    page: Option<i32>,
) -> Json<JsonValue> {
    let (offset, limit) = offset_limit(limit, page);
    let sql = format!(
        "select k.height, k.beneficiary, k.hash, k.miner, k.nonce::text, k.pow, \
         k.prev_hash, k.prev_key_hash, k.state_hash, k.target, k.time_, k.\"version\", \
         m.hash, m.pof_hash, m.prev_hash, m.prev_key_hash, m.signature, \
         m.state_hash, m.time_, m.txs_hash, m.\"version\", \
         t.block_hash, t.block_height, t.hash, t.signatures, t.tx, t.encoded_tx \
         from key_blocks k left join micro_blocks m on k.id = m.key_block_id \
         left join transactions t on m.id = t.micro_block_id \
         where k.height >={} and k.height <={} \
         order by k.height desc, m.time_ desc limit {} offset {}",
        from, to, limit, offset
    );
    let mut list = json!({});
    let mut mb_count = 0;
    let mut tx_count = 0;
    for row in &SQLCONNECTION.get().unwrap().query(&sql, &[]).unwrap() {
        let mut transaction = json!({"block_hash": ""});
        let mut micro_block = json!({"prev_key_hash":""});
        let mut key_block = json!({"height": ""});
        // check if tx is available for a given row
        if let Some(val) = row.get(21) {
            let block_hash: String = val;
            let block_height: i32 = row.get(22);
            let hash: String = row.get(23);
            let sig: Option<String> = row.get(24);
            let signatures = Transaction::deserialize_signatures(&sig);
            let tx_: serde_json::Value = match row.get_opt(26).unwrap().unwrap() {
                Some(encoded_tx) => Transaction::decode_tx(&encoded_tx),
                _ => row.get(25),
            };
            transaction = json!({
                "block_hash": block_hash,
                "block_height": block_height,
                "hash": hash,
                "signatures": signatures,
                "tx": tx_
            });
            tx_count += 1;
        }
        //check if micro_block is available for a given row
        if let Some(val) = row.get(15) {
            let prev_key_hash: String = val;
            let hash: String = row.get(12);
            let pof_hash: String = row.get(13);
            let prev_hash: String = row.get(14);
            let signature: String = row.get(16);
            let state_hash: String = row.get(17);
            let time: i64 = row.get(18);
            let txs_hash: String = row.get(19);
            let version: i32 = row.get(20);
            micro_block = json!({
                "hash": hash, "pof_hash": pof_hash, "prev_hash": prev_hash,
                "prev_key_hash": prev_key_hash, "signature": signature,
                "state_hash": state_hash, "time": time, "txs_hash": txs_hash,
                "version": version
            });
        }

        // get current key block
        if let Some(val) = row.get(0) {
            let height: i64 = val;
            let beneficiary: String = row.get(1);
            let hash: String = row.get(2);
            let miner: String = row.get(3);
            let nonce: String = row.get(4);
            let pow: String = row.get(5);
            let prev_hash: String = row.get(6);
            let prev_key_hash: String = row.get(7);
            let state_hash: String = row.get(8);
            let target: i64 = row.get(9);
            let time: i64 = row.get(10);
            let version: i32 = row.get(11);
            key_block = json!( {
                "height": height, "beneficiary": beneficiary, "hash": hash,
                "miner": miner, "nonce": nonce, "pow": pow, "prev_hash": prev_hash,
                "prev_key_hash": prev_key_hash, "state_hash": state_hash,
                "target": target, "time": time, "version": version,
                "micro_blocks": {}
            });
        }
        let block_height: i64 = serde_json::from_value(key_block["height"].clone()).unwrap();
        let key_height: String = block_height.to_string();
        if list[&key_height] != serde_json::json!(null) {
            if micro_block["prev_key_hash"] != "" {
                let mb_hash: String = serde_json::from_value(micro_block["hash"].clone()).unwrap();
                if list[&key_height]["micro_blocks"][&mb_hash] == serde_json::json!(null) {
                    list[&key_height]["micro_blocks"][&mb_hash] =
                        serde_json::to_value(micro_block).unwrap();
                    list[&key_height]["micro_blocks"][&mb_hash]["transactions"] =
                        serde_json::json!({});
                    mb_count += 1;
                }
                if transaction["block_hash"] != "" {
                    let hash: String = serde_json::from_value(transaction["hash"].clone()).unwrap();
                    list[&key_height]["micro_blocks"][mb_hash]["transactions"][hash] =
                        serde_json::to_value(transaction).unwrap();;
                }
            }
        } else {
            list[&key_height] = serde_json::to_value(key_block).unwrap();
            if micro_block["prev_key_hash"] != "" {
                let mb_hash: String = serde_json::from_value(micro_block["hash"].clone()).unwrap();
                list[&key_height]["micro_blocks"][&mb_hash] = serde_json::json!({});
                list[&key_height]["micro_blocks"][&mb_hash] =
                    serde_json::to_value(micro_block).unwrap();
                list[&key_height]["micro_blocks"][&mb_hash]["transactions"] = serde_json::json!({});
                mb_count += 1;
                if transaction["block_hash"] != "" {
                    let hash: String = serde_json::from_value(transaction["hash"].clone()).unwrap();
                    list[&key_height]["micro_blocks"][mb_hash]["transactions"][hash] =
                        serde_json::to_value(transaction).unwrap();;
                }
            }
        }
    }

    Json(json!({
        "total_transactions": tx_count,
        "total_micro_blocks": mb_count,
        "data": list
    }))
}

#[get("/channels/transactions/address/<address>")]
fn transactions_for_channel_address(
    _state: State<MiddlewareServer>,
    address: String,
) -> Json<JsonTransactionList> {
    check_object(&address);
    let sql = format!(
        "select t.* from transactions t where \
         t.tx->>'channel_id' = '{}' or \
         t.id in (select transaction_id from channel_identifiers where \
         channel_identifier='{}')",
        sanitize(&address),
        sanitize(&address)
    );
    debug!("{}", sql);
    let transactions: Vec<Transaction> =
        sql_query(sql).load(&*PGCONNECTION.get().unwrap()).unwrap();

    let json_transactions = transactions
        .iter()
        .map(JsonTransaction::from_transaction)
        .collect();
    Json(JsonTransactionList {
        transactions: json_transactions,
    })
}

#[get("/channels/active")]
fn active_channels(_state: State<MiddlewareServer>) -> Json<Vec<String>> {
    let sql = "select channel_identifier from channel_identifiers where \
               channel_identifier not in \
               (select tx->>'channel_id' from transactions where \
               tx_type in \
               ('ChannelCloseTx', 'ChannelCloseMutualTx', 'ChannelCloseSoloTx', 'ChannelSlashTx')) \
               order by id asc"
        .to_string();
    Json(
        SQLCONNECTION
            .get()
            .unwrap()
            .query(&sql, &[])
            .unwrap()
            .iter()
            .map(|x| x.get(0))
            .collect(),
    )
}

#[get("/contracts/all?<limit>&<page>")]
fn all_contracts(
    _state: State<MiddlewareServer>,
    limit: Option<i32>,
    page: Option<i32>,
) -> Json<Vec<JsonValue>> {
    let (offset_sql, limit_sql) = offset_limit(limit, page);
    let sql = format!(
        "SELECT ci.contract_identifier, t.hash, t.block_height \
         FROM contract_identifiers ci, transactions t WHERE \
         ci.transaction_id=t.id \
         ORDER BY block_height DESC LIMIT {} OFFSET {}",
        limit_sql, offset_sql
    );
    Json(
        SQLCONNECTION
            .get()
            .unwrap()
            .query(&sql, &[])
            .unwrap()
            .iter()
            .map(|x| {
                let contract_id: String = x.get(0);
                let transaction_hash: String = x.get(1);
                let block_height: i32 = x.get(2);
                json!({
                    "contract_id": contract_id,
                    "transaction_hash": transaction_hash,
                    "block_height": block_height,
                })
            })
            .collect(),
    )
}

#[get("/oracles/list?<limit>&<page>")]
fn oracles_all(
    _state: State<MiddlewareServer>,
    limit: Option<i32>,
    page: Option<i32>,
) -> JsonValue {
    let (offset_sql, limit_sql) = offset_limit(limit, page);
    let sql = format!(
        "SELECT REPLACE(tx->>'account_id', 'ak_', 'ok_'), hash, block_height, \
         CASE WHEN tx->'oracle_ttl'->>'type' = 'delta' THEN block_height + (tx->'oracle_ttl'->'value')::text::integer ELSE 0 END, \
         tx FROM transactions \
         WHERE tx_type='OracleRegisterTx' \
         ORDER BY block_height DESC \
         LIMIT {} OFFSET {}",
        limit_sql, offset_sql,
    );
    debug!("{}", sql);
    let mut res: Vec<JsonValue> = vec![];
    for row in &SQLCONNECTION.get().unwrap().query(&sql, &[]).unwrap() {
        let oracle_id: String = row.get(0);
        let hash: String = row.get(1);
        let block_height: i32 = row.get(2);
        let expires_at: i32 = row.get(3);
        let tx: serde_json::Value = row.get(4);
        res.push(json!({
            "oracle_id": oracle_id,
            "transaction_hash": hash,
            "block_height": block_height,
            "expires_at": expires_at,
               "tx": tx,
        }));
    }
    json!(res)
}

#[get("/oracles/<hash>?<limit>&<page>")]
fn oracle_requests_responses(
    _state: State<MiddlewareServer>,
    hash: String,
    limit: Option<i32>,
    page: Option<i32>,
) -> JsonValue {
    let (offset_sql, limit_sql) = offset_limit(limit, page);
    let sql = format!(
        "select oq.query_id, t1.tx, t2.tx, t1.hash, t2.hash, \
         m1.time_, m2.time_ from oracle_queries oq \
         join transactions t1 on oq.transaction_id=t1.id \
         inner join micro_blocks m1 on t1.micro_block_id = m1.id \
         left outer join transactions t2 on t2.tx->>'query_id' = oq.query_id \
         inner join micro_blocks m2 on t2.micro_block_id = m2.id \
         where oq.oracle_id='{}' \
         limit {} offset {} ",
        hash, limit_sql, offset_sql
    );
    let mut res: Vec<JsonValue> = vec![];
    for row in &SQLCONNECTION.get().unwrap().query(&sql, &[]).unwrap() {
        let query_id: String = row.get(0);
        let mut request: serde_json::Value = row.get(1);
        let request_hash: String = row.get(3);
        let request_timestamp: i64 = row.get(5);
        request["hash"] = serde_json::to_value(&request_hash).unwrap();
        request["timestamp"] = serde_json::to_value(&request_timestamp).unwrap();
        let data: Option<serde_json::Value> = row.get(2);
        let response = match data {
            Some(x) => {
                let mut response_value = x.clone();
                let response_hash: String = row.get(4);
                let response_timestamp: i64 = row.get(6);
                response_value["timestamp"] = serde_json::to_value(&response_timestamp).unwrap();
                response_value["hash"] = serde_json::to_value(&response_hash).unwrap();
                response_value
            }
            _ => serde_json::json!(null),
        };
        let result_set = json!({
            "query_id": query_id,
            "request": json!(request),
            "response": json!(response),
        });
        res.push(result_set);
    }
    json!(res)
}

#[get("/reward/height/<height>")]
fn reward_at_height(_state: State<MiddlewareServer>, height: i64) -> JsonValue {
    let coinbase: Decimal = (coinbase(height) as u64).into();
    let last_reward = KeyBlock::fees(&SQLCONNECTION.get().unwrap(), (height - 1) as i32);
    let this_reward = KeyBlock::fees(&SQLCONNECTION.get().unwrap(), height as i32);
    let key_block = KeyBlock::load_at_height(&PGCONNECTION.get().unwrap(), height).unwrap();
    let four: Decimal = 4.into();
    let six: Decimal = 6.into();
    let ten: Decimal = 10.into();
    let total_reward: Decimal = (last_reward * six / ten) + (this_reward * four / ten);
    json!({
        "height": height,
        "coinbase": coinbase,
        "beneficiary": key_block.beneficiary,
        "fees": total_reward,
        "total": coinbase + total_reward,
    })
}

#[get("/names/active?<limit>&<page>&<owner>")]
fn active_names(
    _state: State<MiddlewareServer>,
    limit: Option<i32>,
    page: Option<i32>,
    owner: Option<String>,
) -> Json<Vec<Name>> {
    let connection = PGCONNECTION.get().unwrap();
    let (offset_sql, limit_sql) = offset_limit(limit, page);
    let sql: String = match owner {
        Some(owner) => format!(
            "select * from \
             names where \
             expires_at >= {} and \
             owner = '{}' \
             order by expires_at desc \
             limit {} offset {} ",
            KeyBlock::top_height(&*connection).unwrap(),
            sanitize(&owner),
            limit_sql,
            offset_sql
        ),
        _ => format!(
            "select * from \
             names where \
             expires_at >= {} \
             order by created_at_height desc \
             limit {} offset {} ",
            KeyBlock::top_height(&*connection).unwrap(),
            limit_sql,
            offset_sql
        ),
    };
    let names: Vec<Name> = sql_query(sql).load(&*PGCONNECTION.get().unwrap()).unwrap();
    Json(names)
}

#[get("/names?<limit>&<page>&<owner>")]
fn all_names(
    _state: State<MiddlewareServer>,
    limit: Option<i32>,
    page: Option<i32>,
    owner: Option<String>,
) -> Json<Vec<Name>> {
    let (offset_sql, limit_sql) = offset_limit(limit, page);
    let sql: String = match owner {
        Some(owner) => format!(
            "select * from names \
             where owner = '{}' \
             order by expires_at desc \
             limit {} offset {} ",
            sanitize(&owner),
            limit_sql,
            offset_sql
        ),
        _ => format!(
            "select * from names \
             order by created_at_height desc \
             limit {} offset {} ",
            limit_sql, offset_sql
        ),
    };
    let names: Vec<Name> = sql_query(sql).load(&*PGCONNECTION.get().unwrap()).unwrap();
    Json(names)
}

#[get("/names/<query>")]
fn search_names(_state: State<MiddlewareServer>, query: String) -> Json<Vec<Name>> {
    let connection = PGCONNECTION.get().unwrap();
    let _name_query = format!("%{}%", query);
    let names = Name::find_by_name(&connection, &_name_query).unwrap();
    Json(names)
}

/*
 * Gets the names which point to something
 */
#[get("/names/reverse/<account>?<limit>&<page>")]
fn reverse_names(
    _state: State<MiddlewareServer>,
    account: String,
    limit: Option<i32>,
    page: Option<i32>,
) -> Json<Vec<Name>> {
    let connection = PGCONNECTION.get().unwrap();
    check_object(&account);
    let s_acc = sanitize(&account);
    let (offset_sql, limit_sql) = offset_limit(limit, page);
    let sql = format!(
        r#"SELECT * FROM names WHERE pointers @> '[{{"id": "{}"}}]' AND expires_at >= {} ORDER BY name limit {} offset {}"#,
        s_acc,
        KeyBlock::top_height(&*connection).unwrap(),
        limit_sql,
        offset_sql
    );
    debug!("{}", sql);
    let names: Vec<Name> = sql_query(sql).load(&*PGCONNECTION.get().unwrap()).unwrap();
    Json(names)
}

/**
 * Gets the chain height at a specific point in time
 */
#[get("/height/at/<millis_since_epoch>")]
fn height_at_epoch(
    _state: State<MiddlewareServer>,
    millis_since_epoch: i64,
) -> Result<Json<JsonValue>, Status> {
    match KeyBlock::height_at_epoch(&PGCONNECTION.get().unwrap(), millis_since_epoch).unwrap() {
        Some(x) => Ok(Json(json!({
            "height": x,
        }))),
        None => Err(rocket::http::Status::new(404, "Not found")),
    }
}

#[get("/status")]
fn status(_state: State<MiddlewareServer>) -> Response {
    let _height = KeyBlock::top_height(&PGCONNECTION.get().unwrap()).unwrap();
    let version = env!("CARGO_PKG_VERSION");
    let top_key_block = KeyBlock::load_at_height(&PGCONNECTION.get().unwrap(), _height).unwrap();
    let utc: DateTime<Utc> = Utc::now();
    let seconds_since_last_block = (utc.timestamp_millis() - top_key_block.time) / 1000;
    let max_seconds: i64 = std::env::var("STATUS_MAX_BLOCK_AGE")
        .unwrap_or("900".into())
        .parse::<i64>()
        .unwrap();
    let queue_length = crate::loader::queue_length();
    let max_queue_length: i64 = std::env::var("STATUS_MAX_QUEUE_LENGTH")
        .unwrap_or("2".into())
        .parse::<i64>()
        .unwrap();
    let ok: bool = true
        && (queue_length as i64 <= max_queue_length)
        && (seconds_since_last_block < max_seconds);
    let mut response = Response::build();
    response.status(Status::from_code(if ok { 200 } else { 503 }).unwrap());
    response.header(Header::new("content-type", "application/json"));
    response.sized_body(Cursor::new(
        json!({
            "queue_length": queue_length,
            "seconds_since_last_block": seconds_since_last_block,
            "OK": ok,
            "version": version,
        })
        .to_string(),
    ));
    response.finalize()
}

#[get("/api")]
fn swagger() -> JsonValue {
    let swagger_str = include_str!("../swagger/swagger.json");
    serde_json::from_str(swagger_str).unwrap()
}

#[get("/compilers")]
pub fn get_available_compilers() -> JsonValue {
    match supported_compiler_versions().unwrap() {
        Some(val) => json!({ "compilers": val }),
        _ => json!({
            "error": "no compiler available"
        }),
    }
}

#[post("/contracts/verify", format = "application/json", data = "<body>")]
pub fn verify_contract(
    _state: State<MiddlewareServer>,
    body: Json<ContractVerification>,
) -> JsonValue {
    if !validate_compiler(body.compiler.clone()) {
        return json!({
            "error": "invalid compiler version"
        });
    }
    match get_contract_bytecode(&body.contract_id).unwrap() {
        Some(create_bytecode) => {
            match compile_contract(body.source.clone(), body.compiler.clone()).unwrap() {
                Some(compiled_bytecode) => {
                    json!({
                            "verified": (create_bytecode == compiled_bytecode)
                    })
                }
                _ => json!({
                    "error": "unable to compile the contract"
                }),
            }
        }
        _ => json!({
            "error": "contract not found"
        }),
    }
}

impl MiddlewareServer {
    pub fn start(self) {
        let allowed_origins = AllowedOrigins::all();
        let options = rocket_cors::CorsOptions {
            allowed_origins,
            allowed_methods: vec![Method::Get].into_iter().map(From::from).collect(),
            allowed_headers: AllowedHeaders::some(&["Authorization", "Accept"]),
            allow_credentials: true,
            ..Default::default()
        }
        .to_cors()
        .unwrap(); // TODO

        rocket::ignite()
            .register(catchers![error400, error404])
            .mount("/middleware", routes![active_channels])
            .mount("/middleware", routes![active_names])
            .mount("/middleware", routes![all_names])
            .mount("/middleware", routes![all_contracts])
            .mount("/middleware", routes![calls_for_contract_address])
            .mount("/middleware", routes![get_available_compilers])
            .mount("/middleware", routes![current_count])
            .mount("/middleware", routes![current_size])
            .mount("/middleware", routes![generations_by_range])
            .mount("/middleware", routes![height_at_epoch])
            .mount("/middleware", routes![oracles_all])
            .mount("/middleware", routes![oracle_requests_responses])
            .mount("/middleware", routes![reverse_names])
            .mount("/middleware", routes![reward_at_height])
            .mount("/middleware", routes![search_names])
            .mount("/middleware", routes![size])
            .mount("/middleware", routes![status])
            .mount("/middleware", routes![swagger])
            .mount("/middleware", routes![transaction_rate])
            .mount("/middleware", routes![transactions_for_account])
            .mount("/middleware", routes![transactions_for_account_to_account])
            .mount("/middleware", routes![transactions_for_interval])
            .mount("/middleware", routes![transaction_count_for_account])
            .mount("/middleware", routes![transactions_for_channel_address])
            .mount("/middleware", routes![transactions_for_contract_address])
            .mount("/middleware", routes![verify_contract])
            .mount("/v2", routes![current_generation])
            .mount("/v2", routes![current_key_block])
            .mount("/v2", routes![generation_at_height])
            .mount("/v2", routes![key_block_at_height])
            .mount("/v2", routes![key_block_at_hash])
            .mount("/v2", routes![micro_block_header_at_hash])
            .mount("/v2", routes![node_get_handler])
            .mount("/v2", routes![node_post_handler])
            .mount("/api", routes![node_api_handler])
            .mount("/v2", routes![transaction_at_hash])
            .mount("/v2", routes![transaction_count_in_micro_block])
            .mount("/v2", routes![transactions_in_micro_block_at_hash])
            .attach(options)
            .manage(self)
            .launch();
    }
}
