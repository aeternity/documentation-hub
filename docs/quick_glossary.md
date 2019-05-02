Quick Glossary Proposal
=====
This quick glossary contains many of the terms used in relation to aeternity.

### *Account*
An object containing an address, balance, nonce, and optional storage and code. An account can be a contract account or an externally owned account (EOA).

### *AirGap*
A project specializing in crypto wallets and designed to be installed on your everyday smartphone. It is connected to the æternity blockchain and therefore requires an internet connection and deals only with public data. On the other hand, the AirGap Vault is designed to be installed on an air gapped smartphone to safeguard your private keys.

### *AENS*
The æternity Naming System enables user-friendly identities for blockchain entities, such as user accounts, oracles, contracts, etc.

### *æpp*
A decentralized application built on æternity blockchain. At a minimum, it is a smart contract and a web user interface. More broadly, an æpp is a web application that is built in top of open, decentralized, peer-to-peer infrastructure services.

### *ættos*
An ætto is the smallest unit of AE.
````
1 AE = 1 ætto x 10^18 = 1000000000000000000 ættos
````
Therefore:
```
1 ætto = 0.000000000000000001 AE
```

### *Address*
Most generally, this represents an EOA or contract that can receive (destination address) or send (source address) transactions on the blockchain. More specifically, it is the rightmost 160 bits of a Keccak hash of an ECDSA public key.

### *BitcoinNG*
An enhanced version of the Bitcoin protocol. The “NG” in the name stands for “Next Generation”. It uses the Nakamoto consensus but distinguishes between leader election and block production in that there are two  different kinds of blocks - key blocks and micro blocks. Key blocks are used for leader election and do not contain transactions. Once the Proof-of-Work puzzle is solved, a leader can produce multiple micro blocks. These micro blocks contain transactions without solving the Proof-of-Work puzzle, rather they require to be signed cryptographically by the current leader. This aids immensely in terms of transaction throughput.

### *Block*
A collection of required information (a block header) about the comprised transactions, and a set of other block headers  known as ommers. Blocks are added to the æternity network by miners.

### *Blockchain*
In æternity, a sequence of blocks validated by the proof-of-work system, each linking to its predecessor all the way to the genesis block. This varies from the Bitcoin protocol in that it does not have a block size limit; it instead uses varying gas limits.

### *Cuckoo Cycle*
A memory-bound graph-theoretic Proof-of-Work algorithm. It is the mining algorithm used by the æternity blockchain.

### *DAO*
Decentralized Autonomous Organization. A company or other organization that operates without hierachical management. Also may refer to a contract names "The DAO" launched on April 30, 2016, which was then hacked in June 2016; this ultimately motivated a hard fork (codenamed DAO) at block #1,192,000, which reversed the hacked DAO contract and caused Ethereum and Ethereum Classic to split into two competing systems.

### *Faucet*
A service that dispenses funds in the form of free test ættos that can be used on a testnet.

### *forgAE*
An æternity framework for creating and setting up a project. It provides commands to run a local node and to compile, deploy, and test smart contracts.

### *Fork*
A change in protocol causing the creation of an alternative chain, or a temporal divergence in two potential block paths during mining.

### *Fortuna release*
The release codename for the third live implementation of the æternity network and will include æternity’s ERC-20 AE tokens on the Ethereum blockchain that migrate during Phase 2 of the æternity blockchain mainnet migration between February 15th 2019 and May 2019. This release will kick off Phase 3 whereby tokens that migrate between May 2019 and September 2019 will be available in the next release.

### *Gas*
A virtual fuel used in Ethereum to execute smart contracts. The aesophia uses accounting mechanism to measure the consumption of gas and limit the consumption of computing resources.

### *Gas limit*
The maximum amount of gas a tranction or block may consume.

### *Genesis block*
The first block in a blockchain, used to initialize a particular network and its cryptocurrency.

### *Hard fork*
A permanent divergence in the blockchain; also known as a hard-forking change. One commonly occurs when nonupgrated nodes can not validate blocks created by upgraded nodes that follow newer consensus rules. Not to be confused with a fork, soft fork, software fork, or Git fork.

### *Hash*
A fixed-length fingerprint of variable-size input, produced by a hash function.

### *IDE*
Integrated Development Environment. A user interface that typically combines a code editor, compiler, runtime, and debugger.

### *microAE*
In the metric system, micro is a millionth of a unit.

As such, a microAE is a millionth of an AE.
````
1 AE = 1 microAE x 10^6 = 1000000 microAE
````
Therefore:
```
1 microAE = 0.000001 AE
```

### *Miner*
A network node that finds valid proof of work for new blocks, by repeated hashing.

### *Minerva release*
The release codename for the second live implementation of the æternity network and included æternity’s ERC-20 AE tokens on the Ethereum blockchain that were migrated during Phase 1 of the æternity blockchain mainnet migration between November 25th 2018 and February 15th 2019. This release kicks off Phase 2 whereby tokens that migrate between February 15th 2019 and May 2019 will be available in the next release codenamed Fortuna.

### *Pre-Roma release*
The release codename before the first live implementation of the æternity network. This release kicked off Phase 0 of the æternity blockchain mainnet migration. Migration of æternity’s ERC-20 AE tokens on the Ethereum blockchain that happened before November 25th 2018 would be available in the next release codenamed Roma.

### *Roma release*
The release codename for the first live implementation of the æternity network and included æternity’s ERC-20 AE tokens on the Ethereum blockchain that were migrated during Phase 0 of the æternity blockchain mainnet migration before November 25th 2018. This release kicked off Phase 1 whereby tokens migrated between November 25th 2018 and February 15th 2019 would be available in the next release codenamed Minerva.

### *SDK*
Software development kit is typically a set of software development tools that allows the creation of applications for a certain software package, software framework, hardware platform, computer system, video game console, operating system, or similar development platform. To enrich applications with advanced functionalities, advertisements, push notifications, and more, most app developers implement specific software development kits. æternity’s SDKs (JavaScript, Python and Golang) are critical for developing an æpp.

### *Sophia*
A new functional programming language developed by æternity for the purpose of writing smart contracts which run on the æternity Virtual Machine and the Ethereum Virtual Machine. It is in the same family of languages such as Reason.

### *Smart Contract*
A program that executes on the æternity computing infrastructure.

### *Testnet*
Short for "test network", a network used to simulate the behavior of the main æternity network.

### *Transcaction*
Data committed to the æternity Blockchain signed by an originating account, targeting a specific address. The transaction contains metadata such as the gas limit for that transaction.

### *Wallet*
Software that holds secret keys. Used to access and control æternity accounts and interact with smart contracts. Keys need not be stored in a wallet, and can instead be retrieved from offline storage (e.g., a memory card or paper) for improved security. Despite the name, wallets never store the actual coins or tokens.

### *Turing complete*
A concept named after English mathematician and computer scientist Alan Turing; a system of data-manipulation rules (such as a computer's instruction set, a programming language, or a cellular automaton) is said to be "Turing complete" or "computationally universal" if it can be used to simulate any Turing machine.

### *Varna*
A new programming language being developed by the æternity team that will be used for writing smart contracts. It is anticipated to be similar to Bitcoin’s Script language. Since Varna contracts will not contain any loops and the gas cost for calls are bounded at compile time, this will help tremendously in mitigating the risks associated with accidentally draining an excessive amount of funds when interacting with the contract.
