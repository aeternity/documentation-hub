drop schema if exists middleware;
create schema middleware authorization middleware;
drop user if exists middleware;
create user middleware login password 'middleware' noinherit createdb;
