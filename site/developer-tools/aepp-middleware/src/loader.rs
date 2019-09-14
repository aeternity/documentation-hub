use super::schema::key_blocks::dsl::*;
use super::schema::transactions::dsl::*;
use chashmap::*;
use diesel::pg::PgConnection;
use diesel::query_dsl::QueryDsl;
use diesel::sql_query;
use diesel::Connection;
use diesel::ExpressionMethods;
use diesel::RunQueryDsl;
use dotenv::dotenv;
use middleware_result::MiddlewareResult;
use middleware_result::*;
use models::*;
use node::*;
use r2d2::Pool;
use r2d2_diesel::ConnectionManager;
use r2d2_postgres::PostgresConnectionManager;
use serde_json;
use std::env;
use std::sync::mpsc;
use std::sync::mpsc::{Receiver, Sender};
use std::sync::Arc;
use std::thread;
use websocket::Candidate;

lazy_static! {
    pub static ref PGCONNECTION: Arc<Pool<ConnectionManager<PgConnection>>> = {
        dotenv().ok(); // Grabbing ENV vars
        let database_url = env::var("DATABASE_URL").expect("DATABASE_URL must be set");
        let manager = ConnectionManager::<PgConnection>::new(database_url);
        let pool = r2d2::Pool::builder()
            .max_size(20) // only used for emergencies...
            .build(manager)
            .expect("Failed to create pool.");
        Arc::new(pool)
    };
}

lazy_static! {
    pub static ref SQLCONNECTION: Arc<Pool<PostgresConnectionManager>> = {
        dotenv().ok(); // Grabbing ENV vars
        let database_url = env::var("DATABASE_URL").expect("DATABASE_URL must be set");
        let manager = PostgresConnectionManager::new
            (database_url, r2d2_postgres::TlsMode::None).unwrap();
        let pool = r2d2::Pool::builder()
            .max_size(3) // only used for emergencies...
            .build(manager)
            .expect("Failed to create pool.");
        Arc::new(pool)
    };
}

#[derive(PartialEq)]
pub enum ParanoiaLevel {
    Normal,
    High,
}

lazy_static! {
    pub static ref PARANOIA_LEVEL: ParanoiaLevel = {
        let paranoia_level = env::var("PARANOIA_LEVEL");
        match paranoia_level {
            Ok(x) => {
                if x.eq(&String::from("high")) {
                    ParanoiaLevel::High
                } else {
                    ParanoiaLevel::Normal
                }
            }
            _ => ParanoiaLevel::Normal,
        }
    };
}

use super::websocket;

pub struct BlockLoader {
    node: Node,
    rx: std::sync::mpsc::Receiver<i64>,
    pub tx: std::sync::mpsc::Sender<i64>,
}

pub static BACKLOG_CLEARED: i64 = -1;

lazy_static! {
    static ref TX_QUEUE: CHashMap<i64, bool> = CHashMap::<i64, bool>::new();
}

fn is_in_queue(_height: i64) -> bool {
    match TX_QUEUE.get(&_height) {
        None => false,
        _ => true,
    }
}

fn remove_from_queue(_height: i64) {
    info!("TX_QUEUE -> {}", _height);
    TX_QUEUE.remove(&_height);
    info!("TX_QUEUE len={}", TX_QUEUE.len());
}
fn add_to_queue(_height: i64) {
    info!("TX_QUEUE <- {}", _height);
    TX_QUEUE.insert(_height, true);
}

pub fn queue(
    _height: i64,
    _tx: &std::sync::mpsc::Sender<i64>,
) -> Result<(), std::sync::mpsc::SendError<i64>> {
    info!("TX_QUEUE len={}", TX_QUEUE.len());

    if is_in_queue(_height) {
        info!("TX_QUEUE already has {}", _height);
        return Ok(());
    }
    _tx.send(_height)?;
    add_to_queue(_height);
    Ok(())
}

pub fn queue_length() -> usize {
    TX_QUEUE.len()
}

/*
* You may notice the use of '_tx' as a variable name in this file,
* rather than the more natural 'tx'. This is because the macros which
* diesel generates from the code in models.rs cause every occurrence
* of their field names to be replaced, which obviously causes
* problems.
*/

impl BlockLoader {
    /*
     * Makes a new BlockLoader object, initializes its DB pool.
     */
    pub fn new(node_url: String) -> BlockLoader {
        let (_tx, rx): (Sender<i64>, Receiver<i64>) = mpsc::channel();
        let node = Node::new(node_url.clone());
        BlockLoader { node, rx, tx: _tx }
    }

    pub fn start_fork_detection(node: &Node, _tx: &std::sync::mpsc::Sender<i64>) {
        debug!("Entering fork detection");
        let _tx = _tx.clone();
        let node = node.clone();
        thread::spawn(move || loop {
            let node = node.clone();
            let _tx = _tx.clone();
            let handle = thread::spawn(move || match BlockLoader::detect_forks(&node, &_tx) {
                Ok(_) => {}
                Err(x) => error!("Error in fork detection {}", x),
            });
            match handle.join() {
                Ok(_) => {
                    error!("Thread exited, respawning");
                    continue;
                }
                Err(_) => {
                    error!("Error creating fork detection thread, exiting");
                    break;
                }
            }
        });
    }

    pub fn inner_detect_forks(
        node: &Node,
        _tx: &std::sync::mpsc::Sender<i64>,
    ) -> MiddlewareResult<bool> {
        let conn = PGCONNECTION.get()?;
        let mut fork_was_detected = false;
        let chain_length = KeyBlock::top_height(&conn)?;
        let mut current_height = chain_length;
        let mut blocks_since_last_fork = 0;
        loop {
            let mut in_fork = false;
            let gen_from_db: JsonGeneration = match JsonGeneration::get_generation_at_height(
                &*SQLCONNECTION.get()?,
                &conn,
                current_height,
            ) {
                Some(x) => x,
                None => {
                    error!("Couldn't load generation {} from DB", current_height);
                    break;
                }
            };

            let gen_from_server: JsonGeneration =
                serde_json::from_value(node.get_generation_at_height(current_height)?)?;
            if !gen_from_db.eq(&gen_from_server) {
                debug!("Generations don't match at height {}", current_height);
                fork_was_detected = true;
                in_fork = true;
            }

            if !in_fork {
                for i in 0..gen_from_db.micro_blocks.len() {
                    match BlockLoader::compare_micro_blocks(
                        &node,
                        &conn,
                        current_height,
                        gen_from_db.micro_blocks[i].clone(),
                        gen_from_db.micro_blocks[i].clone(),
                    ) {
                        Ok(differences) => {
                            if differences.len() != 0 {
                                info!("Microblocks differ: {:?}", differences);
                                fork_was_detected = true;
                                in_fork = true;
                            }
                        }
                        Err(e) => {
                            error!("Error in comparison of micro blocks");
                            fork_was_detected = true;
                            in_fork = true;
                        }
                    }
                }
            }

            if !in_fork {
                blocks_since_last_fork += 1;
                if blocks_since_last_fork >= 5 {
                    debug!("No fork found. Exiting loop");
                    break;
                }
            } else {
                blocks_since_last_fork = 0;
                BlockLoader::invalidate_block_at_height(current_height, &conn, &_tx)?;
                info!(
                    "{} detected at height {} with chain length {}\n\
                     db generation: {:?}\n\
                     chain generation: {:?}\n",
                    if current_height == chain_length {
                        "Fork"
                    } else {
                        "Micro Fork"
                    },
                    current_height,
                    chain_length,
                    gen_from_db,
                    gen_from_server
                );
            }
            current_height -= 1;
            thread::sleep(std::time::Duration::new(blocks_since_last_fork + 1, 0));
        }
        Ok(fork_was_detected)
    }

    pub fn detect_forks(node: &Node, _tx: &std::sync::mpsc::Sender<i64>) -> MiddlewareResult<()> {
        loop {
            BlockLoader::inner_detect_forks(node, _tx)?;
            thread::sleep(std::time::Duration::new(5, 0));
        }
        Ok(())
    }

    /*
     * Delete a key block at height (which causes the deletion of the
     * associated micro blocks and transaactions, and then adds the
     * height to the queue of heights to be loaded.
     */

    pub fn invalidate_block_at_height(
        _height: i64,
        _conn: &PgConnection,
        _tx: &std::sync::mpsc::Sender<i64>,
    ) -> MiddlewareResult<bool> {
        debug!("Invalidating block at height {}", _height);
        //        diesel::delete(key_blocks.filter(height.eq(&_height))).execute(conn)?;
        match queue(_height, _tx) {
            Ok(()) => (),
            Err(e) => {
                error!("Error queuing block at height {}: {:?}", _height, e);
                BlockLoader::recover_from_db_error();
            }
        };
        Ok(true)
    }

    /*
     * Load the mempool from the node--this will only be possible if
     * the debug endpoint is also available on the main URL. Otherwise
     * it harmlessly explodes.
     */
    pub fn load_mempool(&self, _node: &Node) -> MiddlewareResult<u64> {
        let conn = PGCONNECTION.get()?;
        let trans: JsonTransactionList =
            serde_json::from_value(self.node.get_pending_transaction_list()?)?;
        let mut hashes_in_mempool = vec![];
        for i in 0..trans.transactions.len() {
            match self.store_or_update_transaction(&conn, &trans.transactions[i], None) {
                Ok(_) => (),
                Err(x) => error!(
                    "Failed to insert transaction {} with error {}",
                    trans.transactions[i].hash, x
                ),
            }
            hashes_in_mempool.push(format!("'{}'", trans.transactions[i].hash));
        }
        let sql = format!(
            "UPDATE transactions SET VALID=FALSE WHERE \
             micro_block_id IS NULL AND \
             hash NOT IN ({})",
            hashes_in_mempool.join(", ")
        );
        Ok(SQLCONNECTION.get()?.execute(&sql, &[])?)
    }

    pub fn start_scan_thread(node: &Node, _tx: &std::sync::mpsc::Sender<i64>) {
        let _tx = _tx.clone();
        let node = node.clone();
        thread::spawn(move || loop {
            match BlockLoader::scan(&node, &_tx) {
                Ok(count) => debug!(
                    "BlockLoader::scan() added {} blocks to loading queue",
                    count
                ),
                Err(x) => debug!("BlockLoader::scan() returned an error: {}", x),
            };
            thread::sleep(std::time::Duration::new(5, 0));
        });
    }

    /*
     * this method scans the blocks from the heighest reported by the
     * node to the highest iairport n the DB, filling in the gaps.
     */
    pub fn scan(node: &Node, _tx: &std::sync::mpsc::Sender<i64>) -> MiddlewareResult<i32> {
        let connection = PGCONNECTION.get()?;
        let top_block_chain = key_block_from_json(node.latest_key_block()?)?;
        let top_block_db = KeyBlock::top_height(&connection)?;
        let mut blocks_changed = 0;
        if top_block_chain.height == top_block_db {
            trace!("Up-to-date");
            return Ok(0);
        }
        debug!(
            "Reading blocks {} to {}",
            top_block_db + 1,
            top_block_chain.height
        );
        let mut _height = top_block_chain.height;
        loop {
            if _height <= top_block_db {
                break;
            }
            if !KeyBlock::height_exists(&connection, _height) {
                debug!("Queuing block {}", _height);
                match queue(_height, _tx) {
                    Ok(x) => {
                        debug!("Success: {:?}", x);
                        blocks_changed += 1;
                    }
                    Err(e) => {
                        error!("Error queuing block at height {}: {:?}", _height, e);
                        BlockLoader::recover_from_db_error();
                    }
                };
            } else {
                info!("Block already in DB at height {}", _height);
            }
            _height -= 1;
        }
        Ok(blocks_changed)
    }

    /*
     * In fact there is no recovery currently--we just exit, and someone else can restart
     * the process.
     */
    pub fn recover_from_db_error() {
        error!("Quitting...");
        ::std::process::exit(-1);
    }

    /*
     * At this height, load the key block, using the generations call
     * to grab the block and all of its microblocks.
     */

    pub fn load_blocks(&self, _height: i64) -> MiddlewareResult<(i32, i32)> {
        let connection = PGCONNECTION.get()?;
        let result = connection.transaction::<(i32, i32), MiddlewareError, _>(|| {
            self.internal_load_block(&connection, _height)
        });
        result
    }

    fn internal_load_block(
        &self,
        connection: &PgConnection,
        _height: i64,
    ) -> MiddlewareResult<(i32, i32)> {
        let mut count = 0;
        // clear out the block at this height, and any with the same hash, to prevent key violations.
        diesel::delete(key_blocks.filter(height.eq(&_height))).execute(connection)?;
        let generation: JsonGeneration =
            serde_json::from_value(self.node.get_generation_at_height(_height)?)?;
        diesel::delete(
            key_blocks.filter(super::schema::key_blocks::dsl::hash.eq(&generation.key_block.hash)),
        )
        .execute(connection)?;
        let ib: InsertableKeyBlock =
            InsertableKeyBlock::from_json_key_block(&generation.key_block)?;
        let key_block_id = ib.save(&connection)? as i32;
        websocket::broadcast_ws(&Candidate {
            payload: WsPayload::object,
            data: serde_json::to_value(&generation.key_block)?,
        })?;
        websocket::broadcast_ws(&Candidate {
            payload: WsPayload::key_blocks,
            data: serde_json::to_value(&generation.key_block)?,
        })?; //broadcast key_block
        for mb_hash in &generation.micro_blocks {
            let mut mb: InsertableMicroBlock =
                serde_json::from_value(self.node.get_micro_block_by_hash(&mb_hash)?)?;
            mb.key_block_id = Some(key_block_id);
            websocket::broadcast_ws(&Candidate {
                payload: WsPayload::object,
                data: serde_json::to_value(&mb)?,
            })?;
            websocket::broadcast_ws(&Candidate {
                payload: WsPayload::micro_blocks,
                data: serde_json::to_value(&mb)?,
            })?;
            let _micro_block_id = mb.save(&connection)? as i32;
            let trans: JsonTransactionList =
                serde_json::from_value(self.node.get_transaction_list_by_micro_block(&mb_hash)?)?;
            for transaction in trans.transactions {
                let tx_id = self.store_or_update_transaction(
                    &connection,
                    &transaction,
                    Some(_micro_block_id),
                )?;
                BlockLoader::update_auxiliary_tables(&connection, tx_id, &transaction)?;
            }
            count += 1;
        }
        Ok((key_block_id, count))
    }

    /*
     * Does what is says on the tin: updates the contracts, oracles,
     * state channels, and names auxiliary tables.
     */
    fn update_auxiliary_tables(
        connection: &PgConnection,
        tx_id: i32,
        transaction: &JsonTransaction,
    ) -> MiddlewareResult<()> {
        if transaction.is_oracle_query() {
            InsertableOracleQuery::from_tx(tx_id, &transaction)?.save(&connection)?;
        } else if transaction.is_contract_call() {
            if let Ok(contract_url) = std::env::var("AESOPHIA_URL") {
                if let Some(icc) =
                    InsertableContractCall::request(&contract_url, &transaction, tx_id)?
                {
                    icc.save(connection)?;
                }
            }
        } else if transaction.is_contract_creation() {
            InsertableContractIdentifier::from_tx(tx_id, &transaction)?.save(&connection)?;
        } else if transaction.is_channel_creation() {
            InsertableChannelIdentifier::from_tx(tx_id, &transaction)?.save(&connection)?;
        } else if transaction.is_name_transaction() {
            Self::handle_name_transaction(connection, tx_id, transaction)?;
        }
        Ok(())
    }

    pub fn handle_name_transaction(
        connection: &PgConnection,
        tx_id: i32,
        transaction: &JsonTransaction,
    ) -> MiddlewareResult<()> {
        debug!("Name tx: {:?}", transaction);
        if let Some(ttype) = transaction.tx["type"].as_str() {
            match ttype {
                "NameClaimTx" => {
                    debug!("NameClaimTx: {:?}", transaction);
                    if let Some(name) = InsertableName::new_from_transaction(tx_id, transaction) {
                        name.save(connection)?;
                    }
                }
                "NameRevokeTx" => {
                    if let Some(name_id) = transaction.tx["name_id"].as_str() {
                        if let Some(name) = Name::load_for_hash(connection, name_id) {
                            name.delete(connection)?;
                        }
                    }
                }
                "NameTransferTx" => {
                    if let Some(name_id) = transaction.tx["name_id"].as_str() {
                        if let Some(recipient_id) = transaction.tx["recipient_id"].as_str() {
                            if let Some(mut name) = Name::load_for_hash(connection, name_id) {
                                name.owner = recipient_id.to_string();
                                name.update(connection)?;
                            }
                        }
                    }
                }
                "NameUpdateTx" => {
                    debug!("NameUpdateTx: {:?}", transaction);
                    if let Some(name_id) = transaction.tx["name_id"].as_str() {
                        if let Some(mut name) = Name::load_for_hash(connection, name_id) {
                            name.expires_at = transaction.tx["name_ttl"].as_i64()?
                                + transaction.block_height as i64;
                            name.pointers = Some(transaction.tx["pointers"].clone());
                            name.update(connection)?;
                        }
                    }
                }
                _ => (),
            }
        }
        Ok(())
    }
    /*
     * transactions in the mempool won't have a micro_block_id, so as we scan the chain we may
     * need to insert them, or update them with the id of the micro block with which they're
     * now associated. We may also need to move them to a different micro block, in the event
     * of a fork.
     */
    pub fn store_or_update_transaction(
        &self,
        conn: &PgConnection,
        trans: &JsonTransaction,
        _micro_block_id: Option<i32>,
    ) -> MiddlewareResult<i32> {
        let sql = format!(
            "select * from transactions where hash = '{}' limit 1",
            &trans.hash
        );
        debug!("{}", sql);
        websocket::broadcast_ws(&Candidate {
            payload: WsPayload::object,
            data: serde_json::to_value(trans)?,
        })?;
        let mut results: Vec<Transaction> = sql_query(sql).get_results(conn)?;
        match results.pop() {
            Some(x) => {
                debug!("Updating transaction with hash {}", &trans.hash);
                diesel::update(&x)
                    .set(micro_block_id.eq(_micro_block_id))
                    .execute(conn)?;
                websocket::broadcast_ws(&Candidate {
                    payload: WsPayload::tx_update,
                    data: serde_json::to_value(&x)?,
                })?; //broadcast updated transaction
                Ok(x.id)
            }
            None => {
                debug!("Inserting transaction with hash {}", &trans.hash);
                let _tx_type: String = from_json(&serde_json::to_string(&trans.tx["type"])?);
                let _tx: InsertableTransaction = match InsertableTransaction::from_json_transaction(
                    &trans,
                    _tx_type,
                    _micro_block_id,
                ) {
                    Ok(x) => x,
                    Err(e) => match *PARANOIA_LEVEL {
                        ParanoiaLevel::High => {
                            panic!("Error loading blocks, and paranoia level is high: {:?}", e)
                        }
                        _ => return Err(MiddlewareError::from(e)),
                    },
                };
                websocket::broadcast_ws(&Candidate {
                    payload: WsPayload::transactions,
                    data: serde_json::to_value(trans)?,
                })?; //broadcast updated transaction
                _tx.save(conn)
            }
        }
    }

    /*
     * The very simple function which pulls heights from the queue and
     * loads them into the DB
     */
    pub fn start(&self) {
        for b in &self.rx {
            debug!("Pulling height {} from queue for storage", b);
            if b == BACKLOG_CLEARED {
                debug!("Backlog cleared, now launching fork detection & scanning threads");
                BlockLoader::start_fork_detection(&self.node, &self.tx);
                BlockLoader::start_scan_thread(&self.node, &self.tx);
            } else {
                debug!("Loading block at height {} into DB", b);
                match self.load_blocks(b) {
                    Ok(x) => info!(
                        "Saved {} micro blocks in total at height {}, key block id is {} ",
                        x.1, b, x.0
                    ),
                    Err(x) => error!("Error loading blocks {}", x),
                };
                remove_from_queue(b);
            }
        }
        // if we fall through here something has gone wrong. Let's quit!
        error!("Failed to read from the queue, quitting.");
        BlockLoader::recover_from_db_error();
    }

    /*
     * Verify the DB, printing results to stdout.
     *
     * methodology:
     * pick the highest of DB height and height reported by node
     * load blocks from DB and from node
     * compare the microblocks and transactions in each
     * for each, log with - if missing from DB, + if present in DB but not on chain
     * - key block
     * - micro blocks
     * - transactions
     */
    pub fn verify_all(&self) -> MiddlewareResult<i64> {
        let top_chain = self.node.latest_key_block()?["height"].as_i64()?;
        let mut _verified: i64 = 0;
        let conn = PGCONNECTION.get()?;
        let top_db = KeyBlock::top_height(&conn)?;
        let top_max = std::cmp::max(top_chain, top_db);
        let mut _height = top_max;
        loop {
            self.verify_height(_height);
            _height -= 1;
            _verified += 1;
        }
    }

    pub fn verify_height(&self, _height: i64) -> MiddlewareResult<()>{
        let conn = PGCONNECTION.get()?;
        match self.compare_chain_and_db(_height, &conn) {
            Ok(_val) => println!("Height {} OK {}", _height, _val),
            Err(e) => println!("Height {} not OK: {}", _height, match e.to_string().lines().next() { Some(x) => x, None => "", }),
        }
        Ok(())
    }

    pub fn compare_chain_and_db(
        &self,
        _height: i64,
        conn: &PgConnection,
    ) -> MiddlewareResult<bool> {
        let block_chain = self.node.get_key_block_by_height(_height);
        let block_db = KeyBlock::load_at_height(conn, _height);
        match block_chain {
            Ok(x) => {
                match block_db {
                    Some(y) => {
                        return Ok(self.compare_key_blocks(conn, x, y)? == _height);
                    }
                    None => {
                        debug!("{} missing from DB", _height);
                        return Ok(false);
                    }
                };
            }
            Err(_) => {
                match block_db {
                    Some(_) => {
                        debug!("{} present in DB not in chain", _height);
                        return Ok(false);
                    }
                    None => {
                        debug!("Not found at either, something weird happened");
                        return Ok(false);
                    }
                };
            }
        };
    }

    pub fn compare_key_blocks(
        &self,
        conn: &PgConnection,
        block_chain: serde_json::Value,
        block_db: KeyBlock,
    ) -> MiddlewareResult<i64> {
        let chain_hash = block_chain["hash"].as_str()?;
        if !block_db.hash.eq(&chain_hash) {
            let err = format!(
                "{} Hashes differ: {} chain vs {} block",
                block_db.height, chain_hash, block_db.hash
            );
            return Err(MiddlewareError::new(&err));
        }
        let mut db_mb_hashes = MicroBlock::get_microblock_hashes_for_key_block_hash(
            &*SQLCONNECTION.get()?,
            &String::from(chain_hash),
        )?;
        db_mb_hashes.sort_by(|a, b| a.cmp(b));
        let chain_gen = self.node.get_generation_at_height(block_db.height)?;
        let mut chain_mb_hashes = chain_gen["micro_blocks"].as_array()?.clone();
        chain_mb_hashes.sort_by(|a, b| a.as_str().unwrap().cmp(b.as_str().unwrap()));
        if db_mb_hashes.len() != chain_mb_hashes.len() {
            let err = format!(
                "{} Microblock array size differs: {} chain vs {} db",
                block_db.height,
                chain_mb_hashes.len(),
                block_db.hash.len()
            );
            debug!("{}", err);
            return Err(MiddlewareError::new(&err));
        } else {
            for i in 0..db_mb_hashes.len() {
                let chain_mb_hash = String::from(chain_mb_hashes[i].as_str()?);
                let db_mb_hash = db_mb_hashes[i].clone();
                let differences = BlockLoader::compare_micro_blocks(
                    &self.node,
                    &conn,
                    block_db.height,
                    db_mb_hash,
                    chain_mb_hash,
                )?;
                if differences.len() != 0 {
                    let err = format!("Transactions differ: {:?}", differences);
                    debug!("{}", err);
                    return Err(MiddlewareError::new(&err));
                }
            }
        }
        debug!("{} OK", block_db.height);
        Ok(block_db.height)
    }

    pub fn compare_micro_blocks(
        node: &Node,
        conn: &PgConnection,
        _height: i64,
        db_mb_hash: String,
        chain_mb_hash: String,
    ) -> MiddlewareResult<Vec<String>> {
        let mut differences: Vec<String> = vec![];
        if db_mb_hash != chain_mb_hash {
            differences.push(format!(
                "{} micro block hashes don't match: {} chain vs {} db",
                _height, chain_mb_hash, db_mb_hash
            ));
        }
        let mut db_transactions = match Transaction::load_for_micro_block(conn, &db_mb_hash) {
            Some(x) => x,
            None => {
                error!("Couldn't load transaction list from DB {}", db_mb_hash);
                return Ok(differences);
            }
        };
        db_transactions.sort_by(|a, b| a.hash.cmp(&b.hash));
        // well this fails when Node barfs on us.
        let ct = match node.get_transaction_list_by_micro_block(&chain_mb_hash) {
            Ok(x) => x,
            Err(y) => {
                error!("Couldn't load transaction list from Node {}", y);
                return Ok(differences); // TODO should force reload?
            }
        };
        let mut chain_transactions = ct["transactions"].as_array()?.clone();
        chain_transactions
            .sort_by(|a, b| a["hash"].as_str().unwrap().cmp(b["hash"].as_str().unwrap()));
        if db_transactions.len() != chain_transactions.len() {
            differences.push(format!(
                "{} micro block {} transaction count differs: {} chain vs {} DB",
                _height,
                db_mb_hash,
                chain_transactions.len(),
                db_transactions.len()
            ));
        }
        let mut diffs: Vec<(usize, String, String)> = vec![];
        for i in 0..db_transactions.len() {
            let chain_tx_hash = chain_transactions[i]["hash"].as_str()?;
            if db_transactions[i].hash != chain_tx_hash {
                diffs.push((
                    i,
                    db_transactions[i].hash.clone(),
                    chain_tx_hash.to_string(),
                ));
            }
        }
        if diffs.len() != 0 {
            for i in 0..diffs.len() {
                differences.push(format!(
                    "{} micro block {} transaction {} of {} hashes differ: {} chain vs {} DB",
                    _height,
                    db_mb_hash,
                    diffs[i].0,
                    diffs.len(),
                    diffs[i].2,
                    diffs[i].1
                ));
            }
        }
        Ok(differences)
    }
}
