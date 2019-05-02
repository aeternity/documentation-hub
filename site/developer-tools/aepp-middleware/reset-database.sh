#!/bin/bash
#
# This script backs up src/schema.rs, resets the DB and then restores
# src/schema.rs. This is necessary because the generated file requires
# modification in order to compile.

cp src/schema.rs src/schema.rs.backup
diesel database reset
mv -f src/schema.rs.backup src/schema.rs
