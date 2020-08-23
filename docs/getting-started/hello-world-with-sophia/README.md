# TUTORIAL: How to create a Sophia smart contract in three(3) steps

## Overview
This tutorial will quite aid the beginners to understand the fundamentals of smart contract and how to write your first smart contract with Sophia ML in 3 steps using the AEstudio online editor.

## Prerequisites
All you will need for this tutorial is an **Internet Connection** to visit the AEstudio at [https://studio.aepps.com](https://studio.aepps.com)

## Let's get started
### Step One(1) - Declaring the contract and it's state
The first step to writing a Sophia SmartContract is to give the contract a name which in our case will be **HelloWorld**.
```aes
contract HelloWorld =
```
The next thing is to give the contract its state. Take a ```state``` to be the container that keeps the information of our declared smart contract. In our case, below is a sample:
```aes
  record state = { word : string }
```

### Step Two(2) - Initializing the contract
The second step is to initialize our smart contract which simply means setting our smart contract starting point/state. This step is where we will give value to our smart contract state object. In our case, we will make our word variable equal to an empty string. See example:
```aes
  entrypoint init() = { word = "" }
```

### Step Three(3) - Utilizing the contract
The last step is to utilize our contract writing functions to either perform logic tasks, add, remove, or read from our contract state. See example of how to add to our contract state:
```aes
  public stateful entrypoint say_hello(name : string) : string = 
    let new_word = String.concat("Hello, ", name)
    put(state{word = new_word})
    new_word
```

## Deploying and Testing our contract
Now that our smart contract code is ready, let's copy the full code and head to [AEstudio](https://studio.aepps.com) to paste the code so we can test our contract by deploying the contract to the testnet network in other to get a value of **Hello, æternal** or **Hello, world** depending on the name given as a parameter to our ```say_hello``` function.
```aes
contract HelloWorld =

  record state = { word : string }

  entrypoint init() = { word = "" }

  public stateful entrypoint say_hello(name : string) : string = 
    let new_word = String.concat("Hello, ", name)
    put(state{word = new_word})
    new_word
```

You should have an output simplier to the below image:
<p align="center"><img src="https://ipfs.io/ipfs/QmbkBu3PQqRWUmBpRhFtJsAqoc7RADJxuokf6pYmigpeQ8"></p>

## Conclusion
It is fairly simple to create an æternity smart contract using Sophia ML. It even gets easier with time if you familiarize yourself with the language. In case you encounter any problems feel free to contact us through the [æternity Forum](https://forum.aeternity.com/c/development).
