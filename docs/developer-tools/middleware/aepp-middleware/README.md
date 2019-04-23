# æternity middleware

## Overview

This is a caching layer for æternity. It reads the chain and records key- and micro-blocks, and transactions in a PostgreSQL database.

## How to use

- Install a postgresql DB somewhere. Works with versions >= 9.5, at least.
- as the admin user, execute `scripts/prepare-db.sql` which will create the DB and user
- copy 'Rocket.example.toml' to 'Rocket.toml'
- copy `.env.example` to `.env`
- if you want to use a different DB name, edit `scripts/prepare-db.sql`, `.env` and `Rocket.toml`

## How to build

You need a nightly rust build

`rustup default nightly`

then

`cargo build`

and to install the database

```
cargo install diesel_cli
diesel database reset
```

NB: The diesel framework causes many many compiler warnings. It can be good to suppress them with
`export RUSTFLAGS="-Aproc-macro-derive-resolution-fallback"`so that you can see what's actually going on.

## How to run

`cargo run -- ` + flags below

```
FLAGS:
        --help        Prints help information
    -p, --populate    Populate DB
    -s, --server      Start server
    -v, --verify      Check the DB against the chain
    -V, --version     Prints version information

OPTIONS:
    -h, --start <START_HASH>    Hash to start from.
    -u, --url <URL>             URL of æternity node.

```

## Environment variables

`NODE_URL` - the URL of the æternity node
`AESOPHIA_URL` - if present, the middleware will attempt to use this to decode contract calls, storing the function called, and its parameters

## Supported queries
```
GET /middleware/channels/active
GET /middleware/channels/transactions/address/<address>
GET /middleware/contracts/all
GET /middleware/contracts/calls/address/<address>
GET /middleware/contracts/transactions/address/<address>
GET /middleware/names/active?<limit>&<page>
GET /middleware/oracles/all?<limit>&<page>
GET /middleware/reward/height/<height>
GET /middleware/size/current
GET /middleware/size/height/<height>
GET /middleware/transactions/account/<account>/count
GET /middleware/transactions/account/<sender>/to/<receiver>
GET /middleware/transactions/account/<account>?<limit>&<page>
GET /middleware/transactions/<hash>
GET /middleware/transactions/interval/<from>/<to>?<limit>&<page>
GET /middleware/transactions/rate/<from>/<to>

GET /v2/generations/current
GET /v2/generations/height/<height>
GET /v2/key-blocks/current/height
GET /v2/key-blocks/hash/<hash>
GET /v2/key-blocks/height/<height>
GET /v2/micro-blocks/hash/<hash>/header
GET /v2/micro-blocks/hash/<hash>/transactions
GET /v2/micro-blocks/hash/<hash>/transactions/count
```

## Websocket support

The websocket is exposed by default on 0.0.0.0:3020 but can be overridden with the environment variable `WEBSOCKET_ADDRESS`.

Message format:
```
{
"op": "<operation to perform>",
"payload": "<message payload>"
}
```
### Supported ops:
- subscribe
- unsubscribe

### Supported payload:
- key_blocks
- micro_blocks
- transactions
- tx_update

### Returned data

Subscriptions return the array of subscriptions (possibly empty):
```
{"op":"subscribe", "payload": "key_blocks"}
["key_blocks"]
{"op":"subscribe", "payload": "micro_blocks"}
["key_blocks","micro_blocks"]
```

Actual chain data is wrapped in a JSON structure identifying the subscription to which it relates:
```
{"payload":{"beneficiary":"ak_nv5B93FPzRHrGNmMdTDfGdd5xGZvep3MVSpJqzcQmMp59bBCv","hash":"kh_rTQLY9ymNL9MSop3RJtySjxPreuowhejHJYu4KSPBfSSZTerh","height":38263,"miner":"ak_bCCis9P7hTGCjfhDPZh3KQUSDXd5V7thyxYAznFcsr1iLn1Uv","nonce":15041902164601590529,"pow":[11019073,17901996,22830222,28569083,49447859,49462425,49519772,87145865,99866878,102237553,123920118,132218818,155018387,180362216,198154991,201321139,222409093,280306327,281351614,283186475,285893935,287852747,323975163,324943621,371752985,397769028,399438380,400871099,414481284,419637507,437183279,446297926,458204825,460525976,468647042,479097364,479884030,491013280,503534491,505981207,522008475,527020581],"prev_hash":"kh_dxbyza4hSY3Ly5U7HJnfn748Po5pJ1rv3dSgisrexcQ5Nna6p","prev_key_hash":"kh_dxbyza4hSY3Ly5U7HJnfn748Po5pJ1rv3dSgisrexcQ5Nna6p","state_hash":"bs_2mnjZYzxN23QpUx7MT2f5RRx8sXcZqvEq7GtqE28LCowq91k4u","target":503824559,"time":1550244690796,"version":1},"subscription":"key_blocks"}
```


### Testing the websocket

Here is some magic you can run in your Javascript console
```
var exampleSocket = new WebSocket("ws://127.0.0.1:3020");
exampleSocket.onopen = function (event) {  // when connection is open, send a subscribe request
    exampleSocket.send('{"op":"subscribe", "payload": "key_blocks"}');
    //to unsubscribe: exampleSocket.send('{"op":"unsubscribe", "payload": "key_blocks"}')
}

exampleSocket.onmessage = function (event) {
   	console.log(event.data); // you get data here when it arrives
}

```
