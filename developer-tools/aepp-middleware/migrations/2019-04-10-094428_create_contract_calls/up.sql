-- Your SQL goes here

CREATE TABLE contract_calls (
       id SERIAL PRIMARY KEY,
       transaction_id INTEGER NOT NULL REFERENCES transactions(id) ON DELETE CASCADE,
       contract_id VARCHAR(55) NOT NULL,
       caller_id VARCHAR(55) NOT NULL,
       arguments JSONB NOT NULL
);

DELETE FROM transactions WHERE tx_type = 'ContractCallTx';

CREATE INDEX contract_calls_transaction_id_index ON contract_calls(transaction_id);
CREATE INDEX contract_calls_contract_id ON contract_calls(contract_id);
