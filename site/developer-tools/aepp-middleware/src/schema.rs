table! {
    channel_identifiers (id) {
        id -> Int4,
        channel_identifier -> Nullable<Varchar>,
        transaction_id -> Int4,
    }
}

table! {
    contract_identifiers (id) {
        id -> Int4,
        contract_identifier -> Nullable<Varchar>,
        transaction_id -> Int4,
    }
}

table! {
    key_blocks (id) {
        id -> Int4,
        hash -> Varchar,
        height -> Int8,
        miner -> Varchar,
        beneficiary -> Varchar,
        pow -> Varchar,
        nonce -> Numeric,
        prev_hash -> Varchar,
        prev_key_hash -> Varchar,
        state_hash -> Varchar,
        target -> Int8,
        #[sql_name="time_"]
        time -> Int8,
        version -> Int4,
        info -> Varchar,
    }
}

table! {
    micro_blocks (id) {
        id -> Int4,
        key_block_id -> Int4,
        hash -> Varchar,
        pof_hash -> Varchar,
        prev_hash -> Varchar,
        prev_key_hash -> Varchar,
        signature -> Varchar,
        #[sql_name="time_"]
        time -> Int8,
        state_hash -> Varchar,
        txs_hash -> Varchar,
        version -> Int4,
    }
}

table! {
    names (id) {
        id -> Int4,
        name -> Varchar,
        name_hash -> Varchar,
        tx_hash -> Varchar,
        created_at_height -> Int8,
        owner -> Varchar,
        expires_at -> Int8,
        pointers -> Nullable<Jsonb>,
        transaction_id -> Int4,
    }
}

table! {
    oracle_queries (id) {
        id -> Int4,
        oracle_id -> Nullable<Varchar>,
        query_id -> Nullable<Varchar>,
        transaction_id -> Int4,
    }
}

table! {
    transactions (id) {
        id -> Int4,
        micro_block_id -> Nullable<Int4>,
        block_height -> Int4,
        block_hash -> Varchar,
        hash -> Varchar,
        signatures -> Nullable<Text>,
        tx_type -> Varchar,
        tx -> Jsonb,
        fee -> Numeric,
        size -> Int4,
        valid -> Bool,
        encoded_tx -> Nullable<Varchar>,
    }
}

table! {
    contract_calls (id) {
        id -> Int4,
        transaction_id -> Int4,
        contract_id -> Varchar,
        caller_id -> Varchar,
        arguments -> Jsonb,
        callinfo -> Nullable<Jsonb>,
        result -> Nullable<Jsonb>,
    }
}

joinable!(channel_identifiers -> transactions (transaction_id));
joinable!(contract_calls -> transactions (transaction_id));
joinable!(contract_identifiers -> transactions (transaction_id));
joinable!(micro_blocks -> key_blocks (key_block_id));
joinable!(names -> transactions (transaction_id));
joinable!(oracle_queries -> transactions (transaction_id));
joinable!(transactions -> micro_blocks (micro_block_id));

allow_tables_to_appear_in_same_query!(
    channel_identifiers,
    contract_calls,
    contract_identifiers,
    key_blocks,
    micro_blocks,
    names,
    oracle_queries,
    transactions,
);
