# TUTORIAL: How to create a Sophia smart contract in three(3) steps

## Overview
This tutorial will aid you as a beginner to understand the fundamentals of **Sophia** smart contract by writing your first smart contract in 3 steps using the **AE Studio** online editor.

## Let's get started
### Step One(1) - Declaring the contract and its state
The first step to writing a Sophia smart contract is to give the contract a name which in our case will be **HelloWorld**.
```aes
contract HelloWorld =
```
Then we give the contract its state; take `state` to be the container that keeps the information of our declared smart contract. In our case, below is a sample:
```aes
  record state = { word : string }
```

### Step Two(2) - Initializing the contract
The second step is to initialize our smart contract. This simply means setting our smart contract starting point. This step is where we will give value to our smart contract state object. In this case, we will make our word variable equal to an empty string. See sample:
```aes
  entrypoint init() = { word = "" }
```

### Step Three(3) - Utilizing the contract
The last step is to utilize our contract by writing functions to either perform logic tasks, add, remove, update, or read from our contract state. See a sample of how to update our contract state:
```aes
  public stateful entrypoint say_hello(name : string) : string = 
    let new_word = String.concat("Hello, ", name)
    put(state{word = new_word})
    new_word
```

## Deploying and Testing our contract
Now that our smart contract is ready, let's copy the full code, head to **AE Studio** at <a href="https://studio.aepps.com" target="_blank">https://studio.aepps.com</a>, paste the code, deploy our contract to the testnet network, and finally test our contract. The final result of our ```say_hello``` function will be **Hello, æternal**, or **Hello, world** depending on the name given as a parameter to the function.
```aes
contract HelloWorld =

  record state = { word : string }

  entrypoint init() = { word = "" }

  public stateful entrypoint say_hello(name : string) : string = 
    let new_word = String.concat("Hello, ", name)
    put(state{word = new_word})
    new_word
```

You should have an output similar to the image below:
<p align="center"><img src="https://ipfs.io/ipfs/QmbkBu3PQqRWUmBpRhFtJsAqoc7RADJxuokf6pYmigpeQ8"></p>

## Helpful Links
1. <a href="https://www.youtube.com/watch?v=ZUccuEaFBq8&list=PLVz98HTQCJzRmy8naIh49mAW306kGyGXA" target="_blank">Tutorial Video</a>
2. <a href="https://github.com/aeternity/aesophia/blob/lima/docs/sophia.md" target="_blank">Sophia Documentation</a>

## Conclusion
It is fairly simple to create an æternity smart contract using Sophia. It gets even easier with time if you familiarize yourself with the language. In case you encounter any problems feel free to contact us through the <a href="https://forum.aeternity.com/c/development" target="_blank">æternity dev Forum category</a>.
