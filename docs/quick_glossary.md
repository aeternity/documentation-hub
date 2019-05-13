Quick Glossary Proposal
=====
This quick glossary contains many of the terms used in relation to æternity.

### *AE*
The æternity token (AE) is used as payment for any resources that users consume on the platform, e.g. sending payments, using oracles, etc. The distribution of AE in the genesis block will be determined by a smart contract hosted on Ethereum. More AE will be created via mining.

### *æternity*
æternity is a decentralized, public blockchain that uses the GHOST consensus protocol, with Proof-of-Work (PoW) for security and Proof-of-Stake (PoS) for governance. æternity is an account-based system, like Ethereum, and does not use Bitcoin-style unspent transaction outputs (UTXO).

Real-world data can interface with smart contracts through decentralized “oracles”. Scalability and trustless Turing-complete state channels set æternity apart from other Blockchain 2.0 projects.

[Source](https://github.com/aeternity/aeternity-reimagined/blob/master/overview.md)

### *Account*
An object containing an address, balance, nonce, and optional storage and code. An account can be a contract account or an externally owned account.

[Source](https://github.com/ethereumbook/ethereumbook)

### *AirGap*
A crypto wallet system that let's you secure cypto assets with one secret on an offline device like a smartphone. The AirGap Wallet application is installed on an everyday smartphone, which is connected to the æternity blockchain and therefore requires an internet connection. AirGap Vault is installed on a dedicated or old smartphone that has no connection to any network, thus it is air gapped.

### *AENS*
The Aeternity Naming System (AENS) enables user-friendly identities for blockchain entities, such as user accounts, oracles, contracts, etc.

### *æpp*
A decentralized application built on æternity blockchain. At a minimum, it is a smart contract and a web user interface. More broadly, an æpp is a web application that is built in top of open, decentralized, peer-to-peer infrastructure services.

### *ættos*
An ætto is the smallest unit of an æternity token (AE).
````
1 AE = 1 ætto x 10^18 = 1000000000000000000 ættos
````
As a result:
```
1 ætto = 0.000000000000000001 AE
```

### *AEXs*
The Aeternity Expansions (AEXs), or æxpansions, are standards proposed by the community at large, i.e. everyone. Some of them can be mandatory in a specific context, e.g. AEX-1 describes the set of rules governing this repository, but are restricted to the application layer.

[Source](https://github.com/aeternity/AEXs/blob/master/README.md)

### *Address*
Most generally, this represents an EOA or contract that can receive (destination address) or send (source address) transactions on the blockchain. More specifically, it is the rightmost 160 bits of a Keccak hash of an ECDSA public key.

### *Application-specific integrated circuit (ASIC)*
An application-specific integrated circuit (ASIC) /ˈeɪsɪk/, is an integrated circuit (IC) customized for a particular use, rather than intended for general-purpose use. For example, a chip designed to run in a digital voice recorder or a high-efficiency Bitcoin miner is an ASIC. Application-specific standard products (ASSPs) are intermediate between ASICs and industry standard integrated circuits like the 7400 series or the 4000 series.

[Source](https://en.wikipedia.org/wiki/Application-specific_integrated_circuit)

### *BitcoinNG*
An enhanced version of the Bitcoin protocol. The “NG” in the name stands for “Next Generation”. It uses the Nakamoto consensus but distinguishes between leader election and block production in that there are two  different kinds of blocks - key blocks and micro blocks. Key blocks are used for leader election and do not contain transactions. Once the Proof-of-Work puzzle is solved, a leader can produce multiple micro blocks. These micro blocks contain transactions without solving the Proof-of-Work puzzle, rather they require to be signed cryptographically by the current leader. This aids immensely in terms of transaction throughput.

### *Block*
A collection of required information (a block header) about the comprised transactions, and a set of other block headers  known as ommers. Blocks are added to the æternity network by miners.

### *Blockchain*
A block chain is a transaction database shared by all nodes participating in a system based on the Bitcoin protocol. A full copy of a currency’s block chain contains every transaction ever executed in the currency. With this information, one can find out how much value belonged to each address at any point in history.

This ledger of past transactions is called the block chain as it is a chain of blocks. The block chain serves to confirm transactions to the rest of the network as having taken place.

[Source](https://en.bitcoin.it/wiki/Block_chain) [Source](https://www.bitcoinmining.com/)

### *Consensus*
When several nodes (usually most nodes on the network) all have the same blocks in their locally-validated best block chain.

[Source](https://bitcoin.org/en/glossary/consensus)

### *Cuckoo Cycle*
The first graph-theoretic proof-of-work, and the most memory bound, yet with instant verification.

æternity uses Cuckoo Cycle as the ASIC-resistant mining algorithm to avoid centralizing mining power in the hands of a few large mining pools.
From this point of view, Cuckoo Cycle is a very simple PoW, requiring hardly any code, time, or memory to verify.

Finding a 42-cycle, on the other hand, is far from trivial, requiring considerable resources, and some luck (for a given header, the odds of its graph having a L-cycle are about 1 in L). Its large memory requirements make single-chip ASICs economically infeasable for Cuckoo Cycle.

### *DAO*
Decentralized Autonomous Organization. A company or other organization that operates without hierachical management. Also may refer to a contract names "The DAO" launched on April 30, 2016, which was then hacked in June 2016; this ultimately motivated a hard fork (codenamed DAO) at block #1,192,000, which reversed the hacked DAO contract and caused Ethereum and Ethereum Classic to split into two competing systems.

### *Faucet*
Bitcoin faucets are a reward system, in the form of a website or app, that dispenses rewards in the form of a satoshi, which is a hundredth of a millionth BTC, for visitors to claim in exchange for completing a captcha or task as described by the website. There are also faucets that dispense alternative cryptocurrencies.

æternity's faucets are a service that dispenses funds in the form of free test ættos that can be used on a testnet.

### *forgAE*
An æternity framework which helps with setting up a project. It simplifies the development of smart contracts in the aeternity network. It provides commands for compilation, deployment of smart contracts, running a local node and unit testing the contracts.

### *Fork*
A change in protocol causing the creation of an alternative chain, or a temporal divergence in two potential block paths during mining.

### *Gas*
A virtual fuel used in Ethereum to execute smart contracts. The aesophia uses accounting mechanism to measure the consumption of gas and limit the consumption of computing resources.

### *Gas limit*
The maximum amount of gas a tranction or block may consume.

### *Genesis block*
The first block of a block chain. Modern versions of Bitcoin number it as block 0, though very early versions counted it as block 1. The genesis block is almost always hardcoded into the software of the applications that utilize its block chain. It is a special case in that it does not reference a previous block, and for Bitcoin and almost all of its derivatives, it produces an unspendable subsidy.

[Source](https://en.bitcoin.it/wiki/Genesis_block)

### *Hard fork*
A permanent divergence in the blockchain; also known as a hard-forking change. One commonly occurs when nonupgrated nodes can not validate blocks created by upgraded nodes that follow newer consensus rules. Not to be confused with a fork, soft fork, software fork, or Git fork.

### *Hash*
A fixed-length fingerprint of variable-size input, produced by a hash function.

### *Hashcash PoW*
Hashcash is a proof-of-work algorithm that requires a selectable amount of work to compute, but the proof can be verified efficiently. For email uses, a textual encoding of a hashcash stamp is added to the header of an email to prove the sender has expended a modest amount of CPU time calculating the stamp prior to sending the email. In other words, as the sender has taken a certain amount of time to generate the stamp and send the email, it is unlikely that they are a spammer. The receiver can, at negligible computational cost, verify that the stamp is valid. However, the only known way to find a header with the necessary properties is brute force, trying random values until the answer is found; though testing an individual string is easy, if satisfactory answers are rare enough it will require a substantial number of tries to find the answer.

### *IDE*
Integrated Development Environment. A user interface that typically combines a code editor, compiler, runtime, and debugger.

### *Merkle tree*
In cryptography and computer science, a Merkle tree or hash tree is a tree in which every leaf node is labelled with the hash of a data block and every non-leaf node is labelled with the cryptographic hash of the labels of its child nodes. Hash trees allow efficient and secure verification of the contents of large data structures. Hash trees are a generalization of hash lists and hash chains.

Hash trees can be used to verify any kind of data stored, handled and transferred in and between computers. They can help ensure that data blocks received from other peers in a peer-to-peer network are received undamaged and unaltered, and even to check that the other peers do not lie and send fake blocks.

### *microAE*
In the metric system, micro is a millionth of a unit.

As such, a microAE is a millionth of an AE.
````
1 AE = 1 microAE x 10^6 = 1000000 microAE
````
As a result:
```
1 microAE = 0.000001 AE
```
### *Miner*
A network node that finds valid proof of work for new blocks, by repeated hashing.

### *Minerva release*
The release codename for the second live implementation of the æternity network and included æternity’s ERC-20 AE tokens on the Ethereum blockchain that were migrated during Phase 1 of the æternity blockchain mainnet migration between November 25th 2018 and February 15th 2019. This release kicks off Phase 2 whereby tokens that migrate between February 15th 2019 and May 2019 will be available in the next release codenamed Fortuna.

### *ReasonML*
Reason lets you write simple, fast and quality type safe code while leveraging both the JavaScript & OCaml ecosystems.

[Source](https://reasonml.github.io/)

### *SDK*
Software development kit is typically a set of software development tools that allows the creation of applications for a certain software package, software framework, hardware platform, computer system, video game console, operating system, or similar development platform. To enrich applications with advanced functionalities, advertisements, push notifications, and more, most app developers implement specific software development kits. æternity’s SDKs (JavaScript, Python and Golang) are critical for developing an æpp.

### *Sophia*
æternity's own functional programming language, a dialect of ReasonML, which is customized for smart contracts and can be published to a blockchain (the æternity blockchain). Thus some unnecessary features of Reason (and OCaml - since Reason is closely related to OCaml) have been removed, and some blockchain specific primitives, constructions and types have been added.

The Functional Typed Warded Virtual Machine is used to efficiently and safely execute contracts written in the Sophia language.

[Source](https://github.com/aeternity/protocol/blob/master/contracts/sophia.md) [Source](https://github.com/aeternity/protocol/blob/master/contracts/aevm.md)

### *Smart Contract*
A program that executes on the æternity computing infrastructure.

### *Testnet*
Short for "test network", a network used to simulate the behavior of the main æternity network.

### *Transaction*
Data committed to the æternity Blockchain signed by an originating account, targeting a specific address. The transaction contains metadata such as the gas limit for that transaction.

### *Wallet*
Software that holds secret keys. Used to access and control æternity accounts and interact with smart contracts. Keys need not be stored in a wallet, and can instead be retrieved from offline storage (e.g., a memory card or paper) for improved security. Despite the name, wallets never store the actual coins or tokens.

### *Turing complete*
A concept named after English mathematician and computer scientist Alan Turing; a system of data-manipulation rules (such as a computer's instruction set, a programming language, or a cellular automaton) is said to be "Turing complete" or "computationally universal" if it can be used to simulate any Turing machine.
