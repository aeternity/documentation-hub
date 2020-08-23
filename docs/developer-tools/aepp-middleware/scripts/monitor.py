#!/usr/bin/env python
import requests
import argparse
import subprocess
import logging as log

# if the seconds_since_last_block from the
# api are greater than this value the
# url will be cosidered dead
# 0 to disable
MAX_SECONDS_SINCE_LAST_BLOCK = 1000

# if the seconds_since_last_block from the
# api are greater than this value the
# url will be cosidered dead
# 0 to disable
MAX_QUEUE_LENGTH = 1000

## list of monitors to check
monitors = {
    "testnet-loader": {
        "service_name": "mdw-testnet-loader",
        "url": "https://testnet.mdw.aepps.com/middleware/status"
    },
    "mainnet-loader": {
        "service_name": "mdw-loader",
        "url": "https://mdw.aepps.com/middleware/status"
    },
}

def restart_service(service_name):
    """
    :param service_name:
    """
    cmd = ["systemctl", service_name, "restart"]
    subprocess.run(cmd)

def main(args):
    # default auth
    for monitor, data in monitors.items():
        resp = requests.get(data.get("url"))
        if resp.status_code >= 400:
            print(f"monitor {monitor} response {resp.status_code}")
            # monitor offline restart
            restart_service(data.get("service_name"))
            continue
        # get the response data
        resp_data = resp.json()
        seconds_since_last_block = resp_data.get("seconds_since_last_block")
        if MAX_SECONDS_SINCE_LAST_BLOCK > 0 and seconds_since_last_block > MAX_SECONDS_SINCE_LAST_BLOCK:
            # monitor offline restart
            print(f"monitor {monitor} seconds_since_last_block {seconds_since_last_block} (> {MAX_SECONDS_SINCE_LAST_BLOCK})")
            signal_process(data.get("pid_file"))
            continue
        queue_length = resp_data.get("queue_length")
        if MAX_QUEUE_LENGTH > 0 and queue_length > MAX_QUEUE_LENGTH:
            print(f"monitor {monitor} seconds_since_last_block {queue_length} (> {MAX_QUEUE_LENGTH})")
            # monitor offline restart
            signal_process(data.get("pid_file"))
            continue

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    # parse the arguments
    args = parser.parse_args()
    # call the command with our args
    # ret = getattr(sys.modules[__name__], 'cmd_{0}'.format(args.command.replace('-', '_')))(args)
    main(args)
