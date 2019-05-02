#!/bin/bash
#
# Script to test the middleware. Must be used against an empty DB (use
# the reset-database.sh script, not `diesel database reset`
#
# This script should be run from the root dir of the project, and requires
# the

# Populate the DB with the minimum number of key blocks and their
# attendant micro blocks and transactions (and names and contracts and
# and and)

./reset-database.sh

RUST_LOG=info AESOPHIA_URL=https://compiler.aepps.com NODE_URL=https://sdk-mainnet.aepps.com RUST_BACKTRACE=full cargo run --  -H26093,28153
# spawn a server middleware, but do not permit it to contact mainnet...
RUST_LOG=info AESOPHIA_URL=https://compiler.aepps.com NODE_URL=https://sdk-testnet.aepps.com RUST_BACKTRACE=full cargo run --  -s >& afile &
ID=$!
curl -q https://sdk-mainnet.aepps.com/v2/generations/height/26093 > a.json
curl -q http://localhost:8000/v2/generations/height/26093 > b.json

echo "Comparing generations (should be no output)"
json-diff a.json b.json

curl -q https://sdk-mainnet.aepps.com/v2/transactions/th_2PY4qj1uAoBU5fyw5k7F5uhM6aNfQsj4hz9zNqJFZuV43zSpsb > a.json
curl -q http://localhost:8000/v2/transactions/th_2PY4qj1uAoBU5fyw5k7F5uhM6aNfQsj4hz9zNqJFZuV43zSpsb > b.json

echo "Comparing transactions (should be no output)"
json-diff a.json b.json


# cleanup!!!
rm a.json b.json
kill $ID
