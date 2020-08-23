# TUTORIAL: Deploying a smart contract on main network with "AEproject"
## Overview
This tutorial will walk you through the process of deploaying Sophia ML Smart contract on aeternity mainnet network with the ```aeproject``` tool. We will an fund an account and finally deploy our **HelloWorld** contract to æternity mainnet network.

## Prerequisites
Before we go any further, please make sure you have followed the [Deploying on testnet](../deploying-on-testnet/README.md) tutorial

## Getting Tokens
Since we have created an account at the [Coding Locally with AEproject](../coding-locally-with-aeproject/README.md) tutorial, we will look at seperate ways we can fund our account with tokens before we deploy our contract.

### Superhero
**Superhero** is a P2P social platform that elevates the impact that sharing can have. You can earn AE easily on Superhero by creating and publishing good content on the web. Learn more about superhero at [https://superhero.com](https://superhero.com).

### Exchanges
Another way in which you can fund your wallet with AE tokens is through exchanges plaforms where you can trade from Fiat currenty to AE tokens or from other cryptocurrencies to AEtoken. Check out CoinMarketCap at [https://coinmarketcap.com/currencies/aeternity/markets](https://coinmarketcap.com/currencies/aeternity/markets) for more details on exchanges.

### Local P2P
You can also purchase AE locally from an individual who is willing to sell. For the purpose of this tutorial, I will make use of this method to fund our previously created account with 1AE.

### Checking our balance
Finally, let's check the balance of wallet on sdk-mainnet:
```bash
aecli account balance ./my-ae-wallet -u https://sdk-mainnet.aepps.com
```
and we have it:
```bash
Balance________________ 1000000000000000000
ID_____________________ ak_2K2qr83kVeQZzQ74CpoL5E7mUxA1QMNxAhCr1iyoYNyLnset56
Nonce__________________ 1
```

*Note:* If you try to get the account balance of our newly created account without funding it first with AE you will get the below error:
```bash
API ERROR: Account not found
```

## Deploying HelloWorld contract on the mainnet network
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

Now Let's deploy our **HelloWorld** contract to the mainnet network using the secret key of our previously created and funded account with the following command:
```bash
aeproject deploy -n mainnet -s 0ef6ce64e4cb01532a0097a8f08aaf8e81e06dc920a4182d5a464cd5bce67386aca3b0cd045fad53e984237a0797c29ebcae48cb4035d8a5fab5d89fafc30c00
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
Balance____________ 999832437000000000
ID_________________ ak_2K2qr83kVeQZzQ74CpoL5E7mUxA1QMNxAhCr1iyoYNyLnset56
Nonce______________ 3
```

## Conclusion
Deploying smart contracts to the æternity mainnet network is nice and easy. In just a few minutes and few commands, one can deploy their desired contracts on the network. If you encounter any problems please contact us through the [æternity dev Forum category](https://forum.aeternity.com/c/development).