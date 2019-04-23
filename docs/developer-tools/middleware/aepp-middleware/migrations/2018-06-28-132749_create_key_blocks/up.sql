CREATE TABLE key_blocks (
       id SERIAL PRIMARY KEY,
       hash VARCHAR(55) UNIQUE,
       height BIGINT UNIQUE,
       miner VARCHAR(55),
       beneficiary VARCHAR(55),
       nonce numeric(20,0),
       pow TEXT,
       prev_hash VARCHAR(55),
       prev_key_hash VARCHAR(55),
       state_hash VARCHAR(55),
       target BIGINT,
       time_ BIGINT,
       version INTEGER);

CREATE INDEX key_blocks_beneficiary ON key_blocks(beneficiary);
CREATE INDEX key_blocks_height ON key_blocks(height);
