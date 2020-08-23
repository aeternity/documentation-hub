# TUTORIAL: Coding locally with "AEproject"
## Overview
This tutorial will walk you through the process of writing Sophia ML Smart contract with the ```aeproject``` tool. We will install **AEproject**, initialize a folder and go through the folder structure. Once this is done we will update our smartcontract.

## Prerequisites
- Installed node.js and npm (node package manager)
- Installed docker and docker-compose. Installation instructions can be found [here](https://docs.docker.com/compose/install/)
- Installed Visual Studio Code 2017 for Windows users
- Completed the [Hello World with Sophia](../hello-world-with-sophia/README.md) tutorial

## Installing AEproject
**AEproject** is an æternity framework which helps with setting up an æpp project. The framework makes the development of smart contracts in the æternity network very easy. It provides commands for compilation of smart contracts, running a local æternity node, unit testing and deployment of smart contracts.

### From npm global repository (recommended)

The package is available for installation from the npm global repository. You will be able to install it via the following command:
```
npm install -g aeproject
```

### After installing
Now, you have a global command - ```aeproject```
With ```aeproject -h``` command you have a quick reference list of all commands:

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

## Generating the folder project structure
The first thing we need to do is create a project folder and initialize its structure.

Let's create a folder for our project:
```
mkdir ~/codingLocally
```

Go to the newly created folder ```cd ~/codingLocally``` and initialize the folder with:
```
aeproject init
```

You will see output similar to this:
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

- contracts - directory in which the developer can create contracts
   - `ExampleContract.aes` -  a sample smart contract
- deployment - directory that contains the deployment scripts
   - `deploy.js` - an examplary deployment script
- docker - directory with docker configuration files
- integrations - directory with contract settions
- test - directory that contains the unit test scripts
    - `exampleTest.js` - an examplary test script coming with the init

## Creating the ```HelloWorld.aes``` smart contract
Following up on our previously written **HelloWorld** smart contract, we will update our contract to add a wallet address and also retrieve our smart contract informations. The first thing we will do is to create the `HelloWorld.aes` file using the code below:
```bash
touch ./contracts/HelloWorld.aes
```
Now we need to start coding our smart contract. The entire code of our ```HelloWorld.aes``` is:
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

  public entrypoint get_info() : string =
    let public_address = Address.to_str(state.user) 
    String.concat(state.name, public_address)
```
Our contract now stores a name as a string and a user as an address. The ```say_hello``` functions stores the name inserted as a paramenter then resurns a greeting with the name while the public address of the address initilizing the contract is been stored in the `user` state property. The ```get_info``` function then return the whole `state` data after conversting the address to a string.

## Conclusion
Writing smart contracts to the æternity network is nice and easy. In just a few minutes and few commands, one can initilize a folder using the aeproject tool. Check out the next tutorial to learn how to deploy your smart contract. If you encounter any problems installing AEproject, check out installing AEproject video on [Ubuntu](https://youtu.be/7MwTuo70g5w), [Windows](https://youtu.be/ELE24MDuGC8) or contact us through the [æternity dev Forum category](https://forum.aeternity.com/c/development).
