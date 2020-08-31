# TUTORIAL: Deploying a smart contract on mainnet network with "AEproject"
## Overview
This tutorial will walk you through the process of deploying Sophia ML Smart contract on æternity mainnet network with the **AEproject** tool. We will an fund an account and finally deploy a **HelloWorld** contract.

## Prerequisites
Before we go any further, please make sure you have followed the <a href="../deploying-on-testnet" target="_blank">Deploying on Testnet</a> tutorial.

## Getting Tokens
Since we have created an account at the **Deploying on Testnet** tutorial, we will look at seperate ways we can fund our account with tokens before we deploy our contract to the æternity mainnet network.

### Superhero
**Superhero** is a P2P social platform that elevates the impact that sharing can have. You can earn AE easily on Superhero by publishing good content on the web. Learn more about Superhero at <a href="https://superhero.com" target="_blank">https://superhero.com</a>.

### Exchanges
Another way in which you can fund your account with AE tokens is through exchange plaforms. Here you can trade from Fiat currencies to AE tokens or from other cryptocurrencies to AE tokens. Check out <a href="https://coinmarketcap.com/currencies/aeternity/markets" target="_blank">CoinMarketCap</a> for more details on exchanges.

### Local P2P
You can also purchase AE locally from an individual who is willing to sell. For the purpose of this tutorial, I will make use of this method to fund our previously created account with 1AE.

### Checking our balance
Let's check the balance of our account on the mainnet network with the following command in our account directory:
```bash
aecli account balance ./my-ae-account -u https://sdk-mainnet.aepps.com
```
and we have it:
```bash
✔ Enter your password … *****
Balance__________ 1000000000000000000
ID_______________ ak_2rBj4fHW6CEcP4w7gZn1ALsyFifrt8wiDECt7n2mpmefRYfnyH
Nonce____________ 1
```

## Deploying HelloWorld contract on the mainnet network
Now Let's deploy our **HelloWorld** contract to the mainnet network using the secret key of our previously created and funded account with the following command:
```bash
aeproject deploy -n mainnet -s 336e2063e02d7886925aa9a106f85bc952e39ac974e40fa7490c0aab12b551c2f3606032c111eaacb784aa21f35534642ca1d198866285dc37832e1721ea1e47
```
The expected result should be similar to this:
```bash
===== Contract: HelloWorld.aes has been deployed at ct_27YfPckio77U6RJh9yisxidQZtegWmYQUxL5A8o8biQka7N4f1 =====
Your deployment script finished successfully!
```

## Deployment Confirmation
### AECLI
You can recheck your account balance to verify the deployment of the smart contract with the previous command. The output should be similar to this:
```bash
✔ Enter your password … *****
Balance_________ 999919987000000000
ID______________ ak_2rBj4fHW6CEcP4w7gZn1ALsyFifrt8wiDECt7n2mpmefRYfnyH
Nonce___________ 2
```

### AE Studio
You can also verify the contract deployment on the mainnet network using the might AE Studio with the following steps:

1. Head to <a href="https://studio.aepps.com" target="_blank">AE Studio</a>.
2. Copy the **HelloWorld** Contract codes, and paste it as a new contract.
3. Click the **MAINNET** button, and complete the authentication system with Superhero.
4. Copy the contract address given to you after executing the `aeproject deploy` command.
5. Paste the contract address at the provided bar beside the **Use at address:** button, and click the button.
6. Enter a name for the `say_hello` function, and click the **Send Transaction** button.
7. Click the **Call Locally** button for the `get_info` function.   

## Conclusion
Deploying smart contracts to the æternity mainnet network is nice and easy. In just a few minutes and few commands, one can deploy their desired contracts on the network. If you encounter any problems please check out the <a href="https://www.youtube.com/watch?v=uoF4LWo4624&list=PLVz98HTQCJzRmy8naIh49mAW306kGyGXA&index=4" target="_blank">video tutorial</a> on YouTube or contact us through the <a href="https://forum.aeternity.com/c/development" target="_blank">æternity dev Forum category</a>.