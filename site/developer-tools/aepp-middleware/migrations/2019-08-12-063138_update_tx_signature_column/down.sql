-- This file should undo anything in `up.sql`
alter table transactions alter column signatures set NOT NULL;