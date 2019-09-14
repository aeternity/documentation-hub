
CREATE INDEX IF NOT EXISTS transactions_tx_initiator_idx on transactions(((tx ->> 'initiator_id')::text));
CREATE INDEX IF NOT EXISTS transactions_tx_responder_idx on transactions(((tx ->> 'responder_id')::text));
CREATE INDEX IF NOT EXISTS transactions_tx_ga_idx on transactions(((tx ->> 'ga_id')::text));
CREATE INDEX IF NOT EXISTS transactions_tx_from_idx on transactions(((tx ->> 'from_id')::text));
CREATE INDEX IF NOT EXISTS transactions_tx_to_idx on transactions(((tx ->> 'to_id')::text));
CREATE INDEX IF NOT EXISTS transactions_tx_caller_idx on transactions(((tx ->> 'caller_id')::text));
