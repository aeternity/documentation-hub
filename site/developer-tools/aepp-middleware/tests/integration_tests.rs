extern crate aepp_middleware;
extern crate diesel;
#[macro_use]
extern crate assert_json_diff;
#[macro_use]
extern crate serde_json;

use aepp_middleware::loader::BlockLoader;
use aepp_middleware::loader::PGCONNECTION;
use aepp_middleware::models::*;
use aepp_middleware::schema::key_blocks;
use aepp_middleware::schema::key_blocks::dsl::*;
use diesel::query_dsl::filter_dsl::FilterDsl;
use diesel::sql_query;
use diesel::BelongingToDsl;
use diesel::ExpressionMethods;
use diesel::RunQueryDsl;
use std::env;

fn get_blockloader() -> BlockLoader {
    let url = env::var("NODE_URL")
        .expect("NODE_URL must be set")
        .to_string();
    BlockLoader::new(url)
}

fn load_blocks(list: Vec<i64>) {
    let loader = get_blockloader();
    for block in list {
        loader.load_blocks(block).unwrap();
    }
}

/**
 * Load contract and one call via block heights, and test the call data against what it
 * ought to be
 */
#[test]
pub fn test_contract_call() {
    load_blocks(vec![7132, 7139]);
    let conn = &*PGCONNECTION.get().unwrap();
    let calls: Vec<ContractCall> = sql_query("SELECT CC.* FROM contract_calls cc, transactions t WHERE cc.transaction_id=t.id AND t.block_height=7139".to_string()).load(conn).unwrap();
    assert_eq!(calls.len(), 1);
    let args = json!({"function": "createProof",
                      "arguments": [{"type": "string", "value": "testtest"}]});
    let result = json!({"data": {"type": "tuple", "value": []}});
    let cc = &calls[0];
    assert_json_eq!(cc.arguments.clone(), args);
    assert_json_eq!(cc.result.clone().unwrap(), result);

    // some more tests we can do quickly, to check everything looks OK
    let key_block = key_blocks::table
        .filter(height.eq(7132))
        .first::<KeyBlock>(conn)
        .unwrap();
    let micro_blocks: Vec<MicroBlock> = MicroBlock::belonging_to(&key_block).load(conn).unwrap();
    assert_eq!(micro_blocks.len(), 1);
    let transactions: Vec<Transaction> = Transaction::belonging_to(&micro_blocks[0])
        .load(conn)
        .unwrap();
    assert_eq!(transactions.len(), 1);
}

#[test]
pub fn test_oracle_queries() {
    load_blocks(vec![34424]);
    let conn = &*PGCONNECTION.get().unwrap();
}
