# aepp-faucet

Send Online Top-up. Instant Wallet Recharge

Recharge your wallet on the aeternity testnet https://sdk-testnet.aepps.com

## Configuration

Configuring Faucet application via enviornment variable:

- `TOPUP_AMOUNT` The amount of tokens that the faucet application will place into your account. (Default value 250)
- `FAUCET_ACCOUNT_PRIV_KEY` The account that faucet aepp will top off the account.
- `EPOCH_URL` URL of the node that the faucet aepp is using. (Default value 'https://sdk-testnet.aepps.com')
- `EPOCH_URL_DEBUG` URL of the node that the faucet aepp is using (debug endpoints). (Default value 'https://sdk-testnet.aepps.com')
- `TX_TTL` How many key blocks will live before it is mined (Default value 100)
- `EXPLORER_URL` Url of the explorer app (Default value 'https://explorer.aepps.com')
- `TX_PAYLOAD` Value to use to fill the payload for the transactions (Default value `Faucet Tx`)
- `NETWORK_ID` The network id (Default value `ae_devnet`)

### Telegram integration

- `TELEGRAM_ENABLED` enable telegram notifications
- `TELEGRAM_API_TOKEN` the token of the telegram bot
- `TELEGRAM_CHAT_ID` the chat id to send notifications to
