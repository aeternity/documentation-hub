#![feature(plugin)]
#![feature(slice_concat_ext)]
#![feature(custom_attribute)]
#![feature(proc_macro_hygiene, decl_macro)]
#![feature(try_trait)]
#![feature(try_from)]
extern crate backtrace;
extern crate base58;
extern crate base58check;
extern crate base64;
extern crate bigdecimal;
extern crate blake2;
extern crate blake2b;
extern crate byteorder;
extern crate chashmap;
extern crate chrono;
extern crate clap;
extern crate crypto;
extern crate curl;
extern crate daemonize;
#[macro_use]
extern crate diesel;
#[macro_use]
extern crate diesel_migrations;
extern crate dotenv;
extern crate env_logger;
extern crate flexi_logger;
extern crate futures;
extern crate hex;
extern crate itertools;
#[macro_use]
extern crate lazy_static;
#[macro_use]
extern crate log;
extern crate r2d2;
extern crate r2d2_diesel;
extern crate r2d2_postgres;
extern crate rand;
extern crate regex;
extern crate reqwest;
#[macro_use]
extern crate rocket;
#[macro_use]
extern crate rocket_contrib;
extern crate rocket_cors;
extern crate rust_base58;
extern crate rust_decimal;
#[macro_use]
extern crate serde_derive;
extern crate postgres;
extern crate serde_json;
extern crate ws;

extern crate aepp_middleware;

use std::thread;
use std::thread::JoinHandle;

use clap::{App, Arg, SubCommand};

use std::env;

pub mod coinbase;
pub mod compiler;
pub mod hashing;
pub mod loader;
pub mod middleware_result;
pub mod node;
pub mod schema;
pub mod server;
pub mod websocket;

pub use bigdecimal::BigDecimal;
use loader::BlockLoader;
use middleware_result::MiddlewareResult;
use server::MiddlewareServer;
pub mod models;

use daemonize::Daemonize;

const VERSION: &'static str = env!("CARGO_PKG_VERSION");

embed_migrations!("migrations/");

use loader::PGCONNECTION;

/*
 * This function does two things--initially it asks the DB for the
* heights not present between 0 and the height returned by
* /generations/current.  After it has queued all of them it spawns the
* detect_forks thread, then it starts the blockloader, which does not
* return.
*/

fn fill_missing_heights(url: String, _tx: std::sync::mpsc::Sender<i64>) -> MiddlewareResult<bool> {
    debug!("In fill_missing_heights()");
    let node = node::Node::new(url.clone());
    let top_block = node::key_block_from_json(node.latest_key_block().unwrap()).unwrap();
    let missing_heights = node.get_missing_heights(top_block.height)?;
    for height in missing_heights {
        debug!("Adding {} to load queue", &height);
        match loader::queue(height as i64, &_tx) {
            Ok(_) => (),
            Err(x) => {
                error!("Error queuing block to send: {}", x);
                BlockLoader::recover_from_db_error();
            }
        };
    }
    _tx.send(loader::BACKLOG_CLEARED)?;
    Ok(true)
}

fn main() {
    match env::var("LOG_DIR") {
        Ok(x) => {
            flexi_logger::Logger::with_env()
                .log_to_file()
                .directory(x)
                .start()
                .unwrap();
            ()
        }
        Err(_x) => env_logger::Builder::from_default_env()
            .target(env_logger::Target::Stdout)
            .init(),
    }
    let matches = App::new("æternity middleware")
        .version(VERSION)
        .author("John Newby <john@newby.org>")
        .about("----")
        .arg(
            Arg::with_name("server")
                .short("s")
                .long("server")
                .help("Start server")
                .takes_value(false),
        )
        .arg(
            Arg::with_name("populate")
                .short("p")
                .long("populate")
                .help("Populate DB")
                .takes_value(false),
        )
        .arg(
            Arg::with_name("daemonize")
                .short("-d")
                .long("daemonize")
                .help("Daemonize process")
                .takes_value(false),
            )
        .arg(
            Arg::with_name("verify")
                .short("v")
                .long("verify")
                .help("Verify DB integrity against chain, values separated by comma, ranges with from-to accepted. To verify all the blocks in the database just say 'all' (without quotes)")
                .takes_value(true),
        )
        .arg(
            Arg::with_name("heights")
                .short("H")
                .long("heights")
                .help("Load specific heights, values separated by comma, ranges with from-to accepted")
                .takes_value(true),
            )
        .arg(
            Arg::with_name("websocket")
                .short("w")
                .long("websocket")
                .help("Activate websocket (only valid when -p (populate) option also set")
                .requires("populate")
                .takes_value(false),
            )
        .get_matches();

    let url = env::var("NODE_URL")
        .expect("NODE_URL must be set")
        .to_string();

    let populate = matches.is_present("populate");
    let serve = matches.is_present("server");
    let verify = matches.is_present("verify");
    let heights = matches.is_present("heights");
    let daemonize = matches.is_present("daemonize");
    let websocket = matches.is_present("websocket");


    if daemonize {
        let daemonize = Daemonize::new();
        if let Ok(x) = env::var("PID_FILE") {
            daemonize.pid_file(x).start().unwrap();
        } else {
            daemonize.start().unwrap();
        }
    }

    if verify {
        debug!("Verifying");
        let loader = BlockLoader::new(url.clone());
        let verify_flags = String::from(matches.value_of("verify").unwrap()).to_lowercase();
        if &verify_flags == "all" {
            match loader.verify_all() {
                Ok(_) => (),
                Err(x) => error!("Blockloader::verify() returned an error: {}", x),
            };
            return;
        } else {
            for height in range(&verify_flags) {
                loader.verify_height(height);
            }
        }
    }

    // Run migrations if populate or heights set
    if populate || heights {
        let connection = PGCONNECTION.get().unwrap();
        let mut migration_output = Vec::new();
        let migration_result =
            embedded_migrations::run_with_output(&*connection, &mut migration_output);
        for line in migration_output.iter() {
            info!("migration out: {}", line);
        }
        migration_result.unwrap();
    }

    /*
     * The `heights` argument is of this form: 1,10-15,1000 which
     * would cause blocks 1, 10,11,12,13,14,15 and 1000 to be loaded.
     */
    if heights {
        let to_load = matches.value_of("heights").unwrap();
        let loader = BlockLoader::new(url.clone());
        for h in range(&String::from(to_load)) {
            loader.load_blocks(h).unwrap();
        }
    }

    let mut populate_thread: Option<JoinHandle<()>> = None;

    /*
     * We start 3 populate processes--one queries for missing heights
     * and works through that list, then exits. Another polls for
     * new blocks to load, then sleeps and does it again, and yet
     * another reads the mempool (if available).
     */
    if populate {
        let url = url.clone();
        let loader = BlockLoader::new(url.clone());
        match fill_missing_heights(url.clone(), loader.tx.clone()) {
            Ok(_) => (),
            Err(x) => error!("fill_missing_heights() returned an error: {}", x),
        };
        populate_thread = Some(thread::spawn(move || {
            loader.start();
        }));
        if websocket {
            websocket::start_ws();
        }
    }

    if serve {
        let ms: MiddlewareServer = MiddlewareServer {
            node: node::Node::new(url.clone()),
            dest_url: url.to_string(),
            port: 3013,
        };
        ms.start();
        loop {
            // just to stop main() thread exiting.
            thread::sleep(std::time::Duration::new(40, 0));
        }
    }
    if !populate && !serve && !heights {
        warn!("Nothing to do!");
    }

    /*
     * If we have a populate thread running, wait for it to exit.
     */
    match populate_thread {
        Some(x) => {
            x.join().unwrap();
            ()
        }
        None => (),
    }
}

// takes args of the form X,Y-Z,A and returns a vector of the individual numbers
// ranges in the form X-Y are INCLUSIVE
fn range(arg: &String) -> Vec<i64> {
    let mut result = vec!();
    for h in arg.split(',') {
        let s = String::from(h);
        match s.find("-") {
            Some(_) => {
                let fromto: Vec<String> = s.split('-').map(|x| String::from(x)).collect();
                for i in fromto[0].parse::<i64>().unwrap()..fromto[1].parse::<i64>().unwrap()+1 {
                    result.push(i);
                }
            }
            None => {
                result.push(s.parse::<i64>().unwrap());
            }
        }
    }
    result
}

#[test]
fn test_range() {
    assert_eq!(range(&String::from("1")), vec!(1));
    assert_eq!(range(&String::from("2-5")), vec!(2,3,4,5));
    assert_eq!(range(&String::from("1,2-5,10")), vec!(1,2,3,4,5,10));
}
