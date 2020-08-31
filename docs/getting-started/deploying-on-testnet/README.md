# TUTORIAL: Deploying a smart contract on testnet network with "AEproject"
## Overview
This tutorial will walk you through the process of deploying Sophia ML Smart contract on æternity testnet network with the **AEproject** tool. We will install the **AECLI** tool, create an æternity account, fund the account using **Faucet** and finally deploy a **HelloWorld** contract.

## Prerequisites
Before we go any further, please make sure you have followed the <a href="../coding-locally-with-aeproject" target="_blank">Coding Locally with AEproject</a> tutorial.

## Installing AECLI
**AECLI** is a command Line Interface for the æternity blockchain. The package is available for installation from the npm global repository. You will be able to install it via the following command:
```
npm install -g @aeternity/aepp-cli
```

Now, you have a global command ```aecli```, with ```aecli -h``` command you have a quick reference list of all commands:

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
First, let's create an `account` folder in our **coding_locally** project folder:

```bash
mkdir ~/coding_locally/account
```

Go to the newly created folder ```cd ~/coding_locally/account``` and create a new æternity account with:
```bash
aecli account create my-ae-account --password 12345
```

Expected output:
```bash
Address____________ ak_2rBj4fHW6CEcP4w7gZn1ALsyFifrt8wiDECt7n2mpmefRYfnyH
Path_______________ ~/coding_locally/account/my-ae-account
```

The above command generates an account file with the name `my-ae-account`, located at the directory the command was executed. The content of account file looks like this:
```json
{"name":"my-ae-account","version":1,"public_key":"ak_2rBj4fHW6CEcP4w7gZn1ALsyFifrt8wiDECt7n2mpmefRYfnyH","id":"217c1716-fe58-4145-a80d-a7e12d6f1381","crypto":{"secret_type":"ed25519","symmetric_alg":"xsalsa20-poly1305","ciphertext":"ff1c6ba1dd48df09e536c4ffbbfff92c5a72a38ba41ac6f4a96501b323d2c9380fccdda798f53b2105d4e37346ae514cb6efc980930b542dccc0f7017d66bde6526ea269e843757d4627a91c4700eb81","cipher_params":{"nonce":"311d045274e9326cb2145722a029212c71affe007a38bc0b"},"kdf":"argon2id","kdf_params":{"memlimit_kib":65536,"opslimit":3,"parallelism":1,"salt":"517ce31f0fe994ee54b32860dafd591a"}}}
```

In order to get the public address and secret key of our account, let's execute the following command in our account directory:
```bash
aecli account address my-ae-account --privateKey
```
Type your password and you will see an output similar to this:
```bash
✔ Enter your password … *****
Address_______________ ak_2rBj4fHW6CEcP4w7gZn1ALsyFifrt8wiDECt7n2mpmefRYfnyH
✔ Are you sure you want print your secret key? … yes
Secret Key____________ 336e2063e02d7886925aa9a106f85bc952e39ac974e40fa7490c0aab12b551c2f3606032c111eaacb784aa21f35534642ca1d198866285dc37832e1721ea1e47
```

## Getting Tokens
Since we have an account and we are developing on the testnet network, we can copy our account public address, head to æternity's faucet æpp at <a href="https://faucet.aepps.com" target="_blank">https://faucet.aepps.com</a> to fund our account with 5AE. 

You should have an output simpliar to the below image:
<p align="center"><img src="https://ipfs.io/ipfs/QmTMWAx7j7Z5eGaXK5xG47Qwfm2j3ahFt1NgS9Kb8g4ASb"></p>

Finally, let's check the balance of the account on the testnet network with the following command in our account directory:
```bash
aecli account balance ./my-ae-account -u https://sdk-testnet.aepps.com
```
and we have it:
```bash
✔ Enter your password … *****
Balance_____________ 5000000000000000000
ID__________________ ak_2rBj4fHW6CEcP4w7gZn1ALsyFifrt8wiDECt7n2mpmefRYfnyH
Nonce_______________ 1
```

*Note:* If you try to get the account balance of a newly created account without funding it with AE you will get the below error:
```bash
API ERROR: Account not found
```

## Deploying HelloWorld contract on the testnet network
The ```aeproject deploy``` command helps developers run their deployment scripts for their æpp. The sample deployment script is scaffolded in deployment folder. Before we run the deploy command we have to edit our deployment script in our `coding_locally` project folder. Here is how our new deploy script looks like:
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

Now Let's deploy our **HelloWorld** contract to the testnet network using the secret key of our newly created and funded account with the following command in our coding_locally directory:
```bash
aeproject deploy -n testnet -s 336e2063e02d7886925aa9a106f85bc952e39ac974e40fa7490c0aab12b551c2f3606032c111eaacb784aa21f35534642ca1d198866285dc37832e1721ea1e47
```
The expected result should be similar to this:
```bash
===== Contract: HelloWorld.aes has been deployed at ct_27YfPckio77U6RJh9yisxidQZtegWmYQUxL5A8o8biQka7N4f1 =====
Your deployment script finished successfully!
```

If you get an error; *Cannot find module 'aeproject-utils'*, execute the below command on your coding_locally directory and re-execute the deploy command.
```bash
npm install aeproject-utils prompts aeproject-logger
```

## Deployment Confirmation
### AECLI
You can recheck your account balance to verify the deployment of the smart contract with the previous command. The output should be similar to this:
```bash
Balance_______________ 4999919987000000000
ID____________________ ak_2rBj4fHW6CEcP4w7gZn1ALsyFifrt8wiDECt7n2mpmefRYfnyH
Nonce_________________ 2
```

### AE Studio
You can also verify the contract deployment on the testnet network using the might AE Studio with the following steps:

1. Head to <a href="https://studio.aepps.com" target="_blank">AE Studio</a>.
2. Copy the **HelloWorld** Contract codes, and paste it as a new contract.
3. Copy the contract address given to you after executing the `aeproject deploy` command.
4. Paste the contract address at the provided bar beside the **Use at address:** button, and click the button.
5. Enter a name for the `say_hello` function, and click the **Send Transaction** button.
6. Click the **Call Locally** button for the `get_info` function. 

## Conclusion
Deploying smart contracts to the æternity testnet network easy. In just a few minutes and few commands, one can deploy their desired contracts on the network. If you encounter any problems please check out the <a href="https://www.youtube.com/watch?v=IoFlZHWbhX4&list=PLVz98HTQCJzRmy8naIh49mAW306kGyGXA&index=3" target="_blank">video tutorial</a> on YouTube or contact us through the <a href="https://forum.aeternity.com/c/development" target="_blank">æternity dev Forum category</a>.