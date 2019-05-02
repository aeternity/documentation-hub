-- Your SQL goes here
CREATE TABLE names (
       id SERIAL PRIMARY KEY,
       name VARCHAR(255) NOT NULL,
       name_hash VARCHAR(255) NOT NULL,
       created_at_height BIGINT NOT NULL,
       owner VARCHAR(255),
       expires_at BIGINT NOT NULL,
       pointers JSONB NULL);

DELETE FROM transactions WHERE tx_type in ('NameClaimTx', 'NameRevokeTx', 'NameTransferTx', 'NameUpdateTx');

CREATE INDEX names_name_hash_index ON names(name_hash);
CREATE INDEX names_pointers_index ON names(pointers);
