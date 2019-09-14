#!/bin/bash
set -e
#
# Script to install a release binary and to ensure that everything is set up and
# permissions are correct. Run in root crate directory.
#
# You'll almost certainly want to run this as root
#
# ENV VARIABLES, modify as you wish
USER=aeternity
GROUP=aeternity
PIDDIR=/var/run/aeternity
LOGDIR=/var/log/aeternity

cargo build --release
if [ $? -ne 0 ]
then
   echo "Cargo didn't build, exiting"
   exit $?
fi

if ! [ -d $PIDDIR ]
then
    mkdir -p $PIDDIR
    chown $USER:$GROUP $PIDDIR
fi

if ! [ -d $LOGDIR ]
then
    mkdir -p $LOGDIR
    chown $USER:$GROUP $LOGDIR
fi

cp target/release/mdw /usr/local/mdw/bin
