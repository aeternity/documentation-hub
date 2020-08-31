# TUTORIAL: Coding locally with "AEproject"
## Overview
This tutorial will walk you through the process of writing Sophia ML Smart contract with the **AEproject** tool. We will install AEproject, initialize a folder with AEproject, and update an HelloWorld smart contract.

## Prerequisites
- Installed node.js and npm (node package manager)
- Installed docker and docker-compose. Installation instructions can be found <a href="https://docs.docker.com/compose/install/" target="_blank">here</a>
- Installed Visual Studio Code
- Completed the <a href="../hello-world-with-sophia" target="_blank">HelloWorld with Sophia</a> tutorial

## Installing AEproject
**AEproject** is an æternity framework which helps with setting up an æpp project. The framework makes the development of smart contracts in the æternity network very easy. It provides commands for compilation of smart contracts, running a local æternity node, unit testing and deployment of smart contracts.

The package is available for installation from the npm global repository. You will be able to install it via the following command:
```
npm install -g aeproject
```

### After installing
Now, you have a global command ```aeproject```, with ```aeproject -h``` command you have a quick reference list of all commands:

```bash
Usage: aeproject [options] [command]

Options:
  -V, --version            output the version number
  -h, --help               output usage information

Commands:
  init [options]           Initialize AEproject
  compile [options]        Compile contracts
  test [options]           Running the tests
  env [options]            Running a local network. Without any argument node will be run with --start argument
  node [options]           Running a local node. Without any argument node will be run with --start argument
  compiler [options]       Running a local compiler. Without any arguments compiler will be run with --start argument
  deploy [options]         Run deploy script
  history [options]        Show deployment history info
  contracts [options]      Running a Contract web aepp locally and connect it to the spawned aeproject node.
  shape <type> [type]      Initialize a web Vue project.
  export-config [options]  Export miner account, few funded accounts  and default node configuration.
  inspect [options] <tx>   Unpack and verify transaction (verify nonce, ttl, fee, account balance)
  fire-editor [options]    Download, install and run locally the Fire Editor
  compatibility [options]  Start env with latest versions and test the current project for compatibility
```

## Generating the project structure
First, we need to do is create a project folder:
```bash
mkdir ~/coding_locally
```

Go to the newly created folder ```cd ~/coding_locally``` and initialize the folder with:
```bash
aeproject init
```

You will see an output similar to this:
```bash
===== Initializing AEproject =====
===== Installing aepp-sdk =====
===== Installing AEproject locally =====
===== Creating project file & dir structure =====
===== Creating contracts directory =====
===== Creating tests directory =====
===== Creating integrations directory =====
===== Creating deploy directory =====
===== Creating docker directory =====
==== Adding additional files ====
===== AEproject was successfully initialized! =====
```

The init command creates an æpp structure with several folders and scripts:
```
.
├── contracts
│   └── ExampleContract.aes
├── deployment
│   └── deploy.js
├── docker
├── integrations
├── node_modules
├── test
│   └── exampleTest.js
```

* contracts - directory in which the developer can create contracts
    + `ExampleContract.aes` - an examplary smart contract
* deployment - directory that contains the deployment scripts
    + `deploy.js` - an examplary deployment script
* test - directory that contains the unit test scripts
    + `exampleTest.js` - an examplary unit test script

## Creating the ```HelloWorld.aes``` smart contract
Following up on our previously written **HelloWorld** smart contract, we make the following chages to the contract: add a wallet address to the `state`, update the `say_hello` function, and create `get_info` function to retrieve our smart contract data. 

The first step is to create the `HelloWorld.aes` file:
```bash
touch ./contracts/HelloWorld.aes
```
Now we can write Sophia codes in our newly created file. The entire code of our ```HelloWorld.aes``` is:
```aes
contract HelloWorld =

  record state = { 
    name : string,
    user : address }

  stateful entrypoint init() = { 
    name = "",
    user = Call.caller }

  public stateful entrypoint say_hello(name' : string) : string = 
    let greetings = String.concat("Hello, ", name')
    put(state{name = name'})
    greetings

  public entrypoint get_info() =
    state
```
Our contract now stores `name`(as a string) and `user`(as an address) in its state. The `user` address is the account(public address) that deployed the contract. The ```say_hello``` functions stores the name inserted as a paramenter then returns a greeting with the name. Finally, the ```get_info``` function returns the whole `state` data as an object.

## Helpful Links
1. <a href="https://www.youtube.com/watch?v=1SveDRKBVho&list=PLVz98HTQCJzRmy8naIh49mAW306kGyGXA&index=2" target="_blank">Tutorial Video</a>
2. <a href="https://youtu.be/JZO89dtc5uI" target="_blank">Video Introducing AEproject</a>
3. <a href="https://youtu.be/7MwTuo70g5w" target="_blank">Video Installing AEproject on Ubuntu</a>
4. <a href="https://youtu.be/ELE24MDuGC8" target="_blank">Video Installing AEproject on Windows</a>

## Conclusion
Writing smart contracts on the æternity blockchain is nice and easy. In just a few minutes and few commands, one can initilize a folder using the aeproject tool. If you encounter any problems installing AEproject, contact us through the <a href="https://forum.aeternity.com/c/development" target="_blank">æternity dev Forum category</a>.
