-- Your SQL goes here

alter table contract_calls add column callinfo jsonb default null;
delete from key_blocks where height in (select block_height from transactions where tx_type='ContractCallTx');
