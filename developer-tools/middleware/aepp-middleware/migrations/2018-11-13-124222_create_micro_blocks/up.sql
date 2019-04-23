-- Your SQL goes here

CREATE TABLE micro_blocks (
       id SERIAL PRIMARY KEY,
       key_block_id INT NOT NULL REFERENCES key_blocks(id) ON DELETE CASCADE,
       hash VARCHAR(55) NOT NULL,
       pof_hash VARCHAR(55) NOT NULL,
       prev_hash VARCHAR(55) NOT NULL,
       prev_key_hash VARCHAR(55) NOT NULL,
       signature VARCHAR(255) NOT NULL,
       time_ BIGINT,
       state_hash VARCHAR(255) NOT NULL,
       txs_hash VARCHAR(255) NOT NULL,
       version INT NOT NULL
 );

CREATE INDEX micro_blocks_hash on micro_blocks(hash);
create index micro_blocks_key_block_id ON micro_blocks (key_block_id);
