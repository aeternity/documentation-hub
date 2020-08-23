# TUTORIAL: Deploying a smart contract on testnet network with "AEproject"
## Overview
This tutorial will walk you through the process of deploaying Sophia ML Smart contract on aeternity testnet network with the ```aeproject``` tool. We will install ```aecli``` tool, create an account, fund the account using **Faucet** and finally deploy our **HelloWorld** contract to æternity testnet network.

## Prerequisites
Before we go any further, please make sure you have followed the [Coding Locally with AEproject](../coding-locally-with-aeproject/README.md) tutorial

## Installing AECLI
**AECLI** is a command Line Interface for the æternity blockchain. The package is available for installation from the npm global repository. You will be able to install it via the following command:
```
npm install -g @aeternity/aepp-cli
```

Now, you have a global command - ```aecli```. With ```aecli -h``` command you have a quick reference list of all commands:

```bash
Usage: aecli [options] [command]

Options:
  -V, --version  output the version number
  -h, --help     output usage information

Commands:
  config         Print the client configuration
  chain          Interact with the blockchain
  inspect        Get information on transactions, blocks,...
  account        Handle wallet operations
  contract       Compile contracts
  name           AENS system
  tx             Transaction builder
  crypto         Crypto helpers
  help [cmd]     display help for [cmd]
```

## Creating a secure account
Let's create our new æternity account:
```bash
aecli account create my-ae-wallet --password 12345
```
Expected output:
```bash
Address____________ ak_2K2qr83kVeQZzQ74CpoL5E7mUxA1QMNxAhCr1iyoYNyLnset56
Path_______________ ~/deploying-on-testnet/wallet/my-ae-wallet
```
The above command generates a wallet file with name - my-ae-wallet, located where the command was executed. The content of wallet file looks like this:
```json
{"name":"my-ae-wallet","version":1,"public_key":"ak_2K2qr83kVeQZzQ74CpoL5E7mUxA1QMNxAhCr1iyoYNyLnset56","id":"c3502253-a4e0-4f1d-8bf6-2e33a0319451","crypto":{"secret_type":"ed25519","symmetric_alg":"xsalsa20-poly1305","ciphertext":"c6a575dda6d15e0fc34cccabbb8d946ef73fc3cd3496a4f9de255a3becd29c7371d0a52bdfe671d087b5b080a20be328b52b4a5d3c1817320b512136ceb51a73d6091283fa3abe8b630cea00c7229a7c","cipher_params":{"nonce":"5a40a900562642e9bdd79ab7b856e9ffcfd4d0380e3ed3f4"},"kdf":"argon2id","kdf_params":{"memlimit_kib":65536,"opslimit":3,"parallelism":1,"salt":"143bf0809f654f574a8235e1f97f72c6"}}}
```
In order to get the address and secret key of an account one can use the following command:
```bash
aecli account address my-ae-wallet --privateKey
```
Type your password in and you will see output similar to this:
```bash
Address________ ak_2K2qr83kVeQZzQ74CpoL5E7mUxA1QMNxAhCr1iyoYNyLnset56
✔ Are you sure you want print your secret key? … yes
Secret Key_____ 0ef6ce64e4cb01532a0097a8f08aaf8e81e06dc920a4182d5a464cd5bce67386aca3b0cd045fad53e984237a0797c29ebcae48cb4035d8a5fab5d89fafc30c00
```

## Getting Tokens
Since we have an account and we are develping on the testnet network, we can copy our wallet address and head to æternity's faucet æpp at [https://faucet.aepps.com](https://faucet.aepps.com) to fund our wallet with 5AE. 

You should have an output simplier to the below image:
<p align="center"><img src="https://ipfs.io/ipfs/QmVcVoQxGz3LpXixXhiDVN6XzTnvy59Z3yLaKs7T6ckveW"></p>

Finally, let's check the balance of wallet on sdk-testnet:
```bash
aecli account balance ./my-ae-wallet -u https://sdk-testnet.aepps.com
```
and we have it:
```bash
Balance________________ 5000000000000000000
ID_____________________ ak_2K2qr83kVeQZzQ74CpoL5E7mUxA1QMNxAhCr1iyoYNyLnset56
Nonce__________________ 1
```

*Note:* If you try to get the account balance of our newly created account without funding it first with AE you will get the below error:
```bash
API ERROR: Account not found
```

## Deploying HelloWorld contract on the testnet network
The ```aeproject deploy``` command helps developers run their deployment scripts for their æpp. The sample deployment script is scaffolded in deployment folder. Before we run the deploy command we have to edit our deployment script which we got at the [Coding Locally with AEproject](../coding-locally-with-aeproject/README.md) tutorial. Here is how our new deploy script looks like:
```js
const Deployer = require('aeproject-lib').Deployer;

const deploy = async (network, privateKey, compiler, networkId) => {
  let deployer = new Deployer(network, privateKey, compiler, networkId)

  await deployer.deploy("./contracts/HelloWorld.aes")
};

module.exports = {
  deploy
};
```

Now Let's deploy our **HelloWorld** contract to the testnet network using the secret key of our newly created and funded account with the following command:
```bash
aeproject deploy -n testnet -s 0ef6ce64e4cb01532a0097a8f08aaf8e81e06dc920a4182d5a464cd5bce67386aca3b0cd045fad53e984237a0797c29ebcae48cb4035d8a5fab5d89fafc30c00
```
The expected result should be similar to this:
```bash
===== Contract: HelloWorld.aes has been deployed at ct_rxP8txFLPaK21ApZfNNix3mVzxRCQxxS1YoZhZ2fRRAjLztod =====
Your deployment script finished successfully!
```

If you get an error saying Cannot find module 'aeproject-utils', execute the below command on your project directory and re-execute the deploy command.
```bash
npm install aeproject-utils prompts aeproject-logger
```

### Deployment Confirmation
You can recheck your account ballance to verify if the SmartContract was deployed using your account with the previous command. The output should be similar to this:
```bash
Balance____________ 4999839654000000000
ID_________________ ak_2K2qr83kVeQZzQ74CpoL5E7mUxA1QMNxAhCr1iyoYNyLnset56
Nonce______________ 3
```

## Conclusion
Deploying smart contracts to the æternity testnet network is nice and easy. In just a few minutes and few commands, one can deploy their desired contracts on the network. If you encounter any problems please contact us through the [æternity dev Forum category](https://forum.aeternity.com/c/development).