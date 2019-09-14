-- Your SQL goes here

CREATE INDEX transactions_tx_sender_idx on transactions(((tx ->> 'sender_id')::text));
CREATE INDEX transactions_tx_account_idx on transactions(((tx ->> 'account_id')::text));
CREATE INDEX transactions_tx_recipient_idx on transactions(((tx ->> 'recipient_id')::text));
CREATE INDEX transactions_tx_owner_idx on transactions(((tx ->> 'owner_id')::text));
