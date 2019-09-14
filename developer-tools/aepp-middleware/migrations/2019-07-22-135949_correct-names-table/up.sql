delete from key_blocks where height in (select block_height from transactions where hash in (select tx_hash from names));
delete from names;

alter table names add column transaction_id integer not null references transactions(id) on delete cascade;
