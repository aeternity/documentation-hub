-- Your SQL goes here
delete from key_blocks k where height in (select created_at_height from names);
truncate names;

alter table names add column tx_hash varchar(255) not null;
create index names_tx_hash_index on names(tx_hash);
