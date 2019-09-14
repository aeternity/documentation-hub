-- This file should undo anything in `up.sql`

DROP INDEX transactions_tx_sender_idx;
DROP INDEX transactions_tx_account_idx;
DROP INDEX transactions_tx_recipient_idx;
DROP INDEX transactions_tx_owner_idx;
