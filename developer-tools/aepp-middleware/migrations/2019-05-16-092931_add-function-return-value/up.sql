-- Your SQL goes here

ALTER TABLE contract_calls ADD column result JSONB;

DELETE FROM key_blocks where height in (SELECT block_height FROM transactions WHERE tx_type = 'ContractCallTx');
