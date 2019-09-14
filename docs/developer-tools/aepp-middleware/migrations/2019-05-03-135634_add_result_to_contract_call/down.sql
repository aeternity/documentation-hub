-- This file should undo anything in `up.sql`

alter table contract_calls drop column callinfo;
