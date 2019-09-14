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

pub mod coinbase;
pub mod compiler;
pub mod hashing;
pub mod loader;
pub mod middleware_result;
pub mod models;
pub mod node;
pub mod schema;
pub mod server;
pub mod websocket;

pub fn test() {
    println!("test");
}
