# aetmodel
Threat model documentation.

## List of acronyms
**AEVM** Aeternity Virtual Machine<br />
**API** Application Programming Interface

**BGP** Border Gateway Protocol

**CORS** Cross-Origin Resource Sharing

**DSL** Domain Specific Language<br />
**DB** Database

**EoP** Elevation of Privilege<br />
**ETH** Ethereum (Blockchain)

**HTTP** Hypertext Transfer Protocol

**ISP** Internet Service Provider

**MitM** Man-in-the-Middle (attack)<br />

**NTP** Network Time Protocol<br />
**N/A** No Answer

**OS** Operating System<br />
**OOS** Out Of Scope

**PRNG** Pseudo-Random Number Generator<br />
**p2p** Peer-to-Peer

**STRIDE** Spoofing, Tampering, Repudiation, Information Disclosure, Elevation of Privilege

**TTL** Time-To-Live<br />
**TBD** To Be Decided

**VM** Virtual Machine

**XSS** Cross-Site Scripting (exploit)
## Definitions

**Client Node** is an Aeternity node with no mining capability.

**Miner Node** is an Aeternity node with mining capability.

**Noise protocol** [Crypto protocol based on Diffie-Hellman key agreement](http://noiseprotocol.org/noise.html) that we use with [specific handshake](https://github.com/aeternity/protocol/blob/master/SYNC.md) (**XK**) and encryption (ChaCHaPoly).

**Node** (aka **Epoch node**) umbrella term for Aeternity protocol participant; includes miner nodes, client nodes, peers, etc.
Identified by a URI consisting of the protocol 'aenode://', the public key, an '@' character, the hostname or IP number, a ':' character and the Noise port number.

**Connection** is a communication channel between two nodes peers. Two arbitrary peers can have zero or many connections.

**Peer Node** [is a node participating in a channel](https://github.com/Aeternity/protocol/tree/master/channels#terms).
**Penetration testing** (aka ***pentesting***) authorized simulated attack on a computer system, performed to evaluate the security of the target system.
The test aims to identify the target's strengths and vulnerabilities, including the potential for unauthorized parties to gain access to the system's software and data.
**Predefined Epoch Node** This is a peer that is automatically connected to upon node startup.

**Spoofing** is an attack in which a person or program successfully masquerades as another by falsifying data, to gain an illegitimate advantage.

**State Channel** [is an off-chain method for two peers to exchange state updates](https://github.com/Aeternity/protocol/tree/master/channels#terms), each node can have multiple state channels and a pair of nodes can also have multiple channels between each other, which should be multiplexed over one connection. Epoch nodes come with a state channel web-service API as a reference implementation.

**Transactions** A transaction is an artefact that you post to the blockchain to alter its state. There are many kinds of transactions, e.g. to transfer tokens from one account to another, to create a contract, to query an oracle, etc.
If a transaction is syntactically incorrect it will just be ignored.
Syntactically correct transactions can be classified in 3 groups:
 * **Invalid** transactions are rejected by the validation algorithm. A reason could be that the nonce of a spend transaction is already used on chain, that the TTL (time-to-live) is less than the present height of the chain, etc.
	If the validation algorithm rejects a transaction, it is invalid.
 * **Unusable** transactions are rejected by the validation algorithm, because they cannot be used at the moment. However, they can potentially be used in the future.
	For example, a transaction that spends more tokens than it has in the account is unusable, but can become usable a few blocks later if another transaction transfers money to it.
 * **Valid** transactions are accepted by the validation algorithm.
	They can be part of the next generated block. A miner is not forced to use a valid transaction in a generated block; miners are free to pick any number of valid transactions they prefer (e.g. depending on fees connected to them).


## System Model

The **system model** describes the high level view of the system and the context in which it is used.
It abstracts the details and allows to define the trust boundaries and state changes relevant to security.

Aeternity is a general blockchain, allowing whatever actions on the blockchain.
It is different from Bitcoin in that it has many more features and that it introduces oracles, name registration, contracts, state-channels and governance.
Higher transaction throughput possible than in Bitcoin, faster in 3 ways:

		1. Faster block rate
		2. Bitcoin-NG technology with key-blocks and micro-blocks
		3. Off-chain state channels (micro-payments per second)

High-level features (a non-exhaustive list, to be completed):

1. **Account**
	* **Public key on the chain + private key** - the blockchain does not handle the private keys of the users. Users are assumed to take care of their own keys; we trust the key generation code;
	* **Tokens** - each account holds a positive amount of tokens (aeons);
	* **Nonce** is a counter that increases upon every transaction.
2. **Contract language** a DSL for writing contracts.
3. **Oracle mechanism** ~ An oracle operator scans the blockchain for query transactions and posts answers to those queries to the chain.
4. **Naming service** - allows to claim a name; specification described [here](https://github.com/aeternity/protocol/blob/master/AENS.md)
5. **State channels** - allow off-chain transactions;
	* Opening and closing the channel [incurs fees](https://github.com/aeternity/protocol/blob/master/channels/README.md#incentives).
6. **Transaction fees**
Aeternity is a community blockchain, minimum fees are agreed upon within the governance process.
Paying more than a minimum fee is possible and expected to be steered by the market.
7. **Governance**
There is a set of parameters, such as minimal transaction fee, that may be modified over the lifetime of the blockchain. Changes must be agreed upon by the majority of the community.
The governance mechanism allows to cast votes on the chain in favour or against changes.
However, the governance mechanism does not include an automated process to modify parameters.

=============================================

![Overview System Diagram](https://github.com/Aeternity/aetmodel/blob/master/epoch-system-diagram.jpeg)

=============================================

## Assets
**Assets** describe are the valuable data that the business cares about
1. **Private Keys** are of paramount importance, the "golden nuggets"; they uniquely identify epoch nodes and used to authenticate transactions.
2. **Password for key encryption** used to encrypt keypair files stored to disk (under investigation if ***both*** keypair files are encrypted - only private key is enough).
3. **Communication on state channels for cooperating nodes** - this is potentially an asset (according to [issue#2](https://github.com/ThomasArts/aetmodel/issues/2), but is unconfirmed and needs further investigation.
4. **Tokens** are an expression of value in the system.
Control over tokens that belong to an account should be unconditionally linked to the respective account's private key.
5. **Computational power** - we consider Tokens and computational power equivalent in the context of the Aeternity blockchain.

## Assumptions

The following list contains assumptions at the core of the threat model for the Aeternity blockchain.
The list is **not** exhaustive and should be extended with additional assumptions about the functionality and structure of the Aeternity blockchain.
The purpose of the list is - once complete - to explicitly list all assumptions that impact the security of the Aeternity blockchain and thus facilitate future work on improving the security and robustness of the blockchain.

1. **The user model is completely flat**, there is only one type of users in the system, all users have equal privileges.

2. **Security of Epoch nodes** relies on the security of the toolchain for compilation, building and dependency management.

3. **Security of Epoch nodes** relies on the absence of malicious Erlang nodes running on the same platform.
This is since an arbitrary Erlang node can connect to a target node running on the same host, assuming knowledge of the target node's *magic cookie*.
The target nodes's default magic cookie is known to be set to 'epoch_cookie'

4. **A node's private key** is **not** the only data that must remain secret at all times.
Messages exchanged in a state channel should be private as long as peers cooperate (See [issue #2](https://github.com/ThomasArts/aetmodel/issues/2)).

5. **Code does not run in the same privilege ring**, since code on the epoch nodes runs on different privilege levels (See [issue #3](https://github.com/ThomasArts/aetmodel/issues/3)).
The AEVM executes untrusted code and EoP should not be possible.


## Threat Model

The threat model described in this document is based on three artifacts:

### 1. The **STRIDE** model:

STRIDE is a mnemonic for things that go wrong in computer and network systems security [1],[2].
It stands for Spoofing, Tampering, Repudiation, Information Disclosure, Denial of Service, and Elevation of Privilege.
We base the threat model described in this document on an adaptation of the STRIDE methodology.
A virtualization of the threat trees will be added in the future if necessary.

* **(1) Spoofing** - Impersonating something or someone else.
* **(2) Tampering** - Modifying data (transaction content?) or code.
* **(3) Repudiation** - Claiming	to	have not performed an action.
* **(4) Information disclosure** - Exposing information to someone not authorized to see it.
* **(5) Denial of service** - Deny or	degrade service to users.
* **(6) Elevation of privilege** - Gain capabilities without proper authorization

### 2. Earlier threat model work on Bitcoin
Earlier work has been done on the [Bitcoin threat model](https://github.com/JWWeatherman/bitcoin_security_threat_model), list of [Bitcoin Weaknesses](https://en.bitcoin.it/wiki/Weaknesses) and [Bitcoin Vulnerabilities and Exposures](https://en.bitcoin.it/wiki/Common_Vulnerabilities_and_Exposures).
We have reviewed and adapted the parts that were considered relevant to Aeternity.

### 3. Earlier threat model work on Aeternity
Earlier work has been done on a [thread model for Aeternity](https://github.com/Aeternity/protocol/blob/master/SYNC.md#threat-model).
We revised the updated information and relevant aspects and included them into the current threat model.

=============================================

"(1.1.1)" -> Details provided in tables<br />
"[1.1.1]" -> Details NOT provided in tables


### (1) Spoofing: Spoof user actions


	(1.1) Obtain private keys
		(1.1.1) At generation time.
			(1.1.1.1) Use of Cryptographically Weak Pseudo-Random Number Generator (PRNG)
			(1.1.1.2) Flawed implementation of key generation code
					(1.1.1.2.1) Flawed Libsodium implementation of key generation code
					(1.1.1.2.2) Flawed Erlang implementation of key generation code
		(1.1.2) At rest / in storage.
			(1.1.2.1) From local storage.
			(1.1.2.2) Third-party storage (e.g. on-line wallets).
			(1.1.2.3) Exploit cross-site scripting vulnerabilities browser-based wallets.
			(1.1.2.4) Malicious neighbours on shared hardware platform.
				(1.1.2.4.1) Malicious neighbours on shared operating system
				(1.1.2.4.2) Malicious neighbours on operating system virtualized platform (in containers).
				(1.1.2.4.3) Malicious neighbours on hardware virtualized platform (in virtual machines)
			(1.1.2.5) By operator of virtualized infrastructure.
			(1.1.2.6) By malicious apps on mobile devices.
				(1.1.2.6.1) By malicious colocated apps on mobile devices
				(1.1.2.6.2) By malicious wallet implementations on mobile devices
				(1.1.2.6.2) Through cloud back-up of application data on mobile devices
		(1.1.3) Node run time.
			(1.1.3.1) Via external interfaces.
			(1.1.3.2) By obtaining access to the node.
		(1.1.4) At logging time.
		(1.1.5) In error messages.
			(1.1.5.1) Errors caused by arbitrary corruption of files on file system.
			(1.1.5.2) Errors caused by invalid program state
			(1.1.5.3) Memory dump caused by an Erlang VM crash

	(1.2) Exploit vulnerabilities in authentication code
		(1.2.1) Incomplete or otherwise flawed signature verification
		(1.2.2) Incomplete or otherwise flawed transaction validation

	(1.3) Exploit vulnerabilities in network communication
		(1.3.1) Packet spoofing
			(1.3.1.1) On-path packet injection
			(1.3.1.2) Blind packet injection
		(1.3.2) Exploit DNS & BGP vulnerabilities to redirect traffic to an impersonated wallet web service;

	(1.4) Vulnerabilities in node API
		(1.4.1) Exploiting CORS to run arbitrary code on node
		(1.4.2) Exploiting the state channel API
		(1.4.3) Exploiting the HTTP API
		(1.4.4) Executing a fun though an external API

 * **Past attacks**
* [A1.1 | 2012 | Generic | Ron was wrong, Whit is right | iacr eprint](https://eprint.iacr.org/2012/064.pdf)
* [A1.2 |2012 | Generic | Mining Your Ps and Qs: Detection of Widespread Weak Keys in Network Devices | Usenix Security](https://www.usenix.org/system/files/conference/usenixsecurity12/sec12-final228.pdf)
* [A1.3 |2016 | Generic | Weak Keys Remain Widespread in Network Devices | IMC'16](https://dl.acm.org/ft_gateway.cfm?id=2987486&type=pdf)
* [A1.4 |2013 | Bitcoin | Weak crypto on Android](https://arstechnica.com/information-technology/2013/08/google-confirms-critical-android-crypto-flaw-used-in-5700-bitcoin-heist/).
* [A1.5 |2011 | Bitcoin | Private keys stolen from wallet](https://bitcointalk.org/index.php?topic=16457.msg214423#msg214423)
* [A1.6 |2017 | Bitcoin | MtGox wallet.dat file stolen (e.g. through exploit, rogue employee, back-up theft)](https://blog.wizsec.jp/2017/07/breaking-open-mtgox-1.html)
* [A1.7 |2017 | Ethereum | Malicious wallet Providers](https://mybroadband.co.za/news/banking/214178-ethereum-wallet-provider-steals-account-keys-and-cashes-out.html)
* [A1.8 |2017 | Ethereum | Exploit in Parity wallet](https://thehackernews.com/2017/07/ethereum-cryptocurrency-hacking.html)
* [A1.9 |2017 | Ethereum | Bug in Parity wallet](https://www.theguardian.com/technology/2017/nov/08/cryptocurrency-300m-dollars-stolen-bug-ether)
* [A1.10 |2018 | Ethereum | Bug/misconfiguration in client node](https://thehackernews.com/2018/06/ethereum-geth-hacking.html)
* [A1.11 |2018 | Ethereum | Conrail wallet exploit](https://mashable.com/2018/06/11/coinrail-exchange-hack/?europe=true)
* [A1.12 |2014 | Bitcoin | XSS wallet vulnerability](https://www.reddit.com/r/Bitcoin/comments/1n57uj/im_attempting_to_reach_a_security_contact_at/)
* [A1.13 |2017 | Generic | Signature verification flaw 1](https://www.cvedetails.com/cve/CVE-2014-9934/)
* [A1.14 |2017 | Generic | Signature verification flaw 2](https://www.cvedetails.com/cve/CVE-2017-2898/)
* [A1.15 |2018 | Ethereum | BGP hijacking](https://www.theverge.com/2018/4/24/17275982/myetherwallet-hack-bgp-dns-hijacking-stolen-ethereum)

### (2) Tampering
Tampering is closely related to spoofing and information disclosure.

		(2.1) Connection tampering
			(2.1.1) No connection integrity
			(2.1.2) Weak connection integrity;
			(2.1.3) Connection security compromise;
		(2.2) Tampering with message integrity
			(2.2.1) No message integrity
			(2.2.2) Weak message integrity;
		(2.3) Tampering with block integrity
			(2.3.1) Tampering with the ordering of transactions included in a block
			(2.3.2) Tampering the timestamp in mined blocks
		(2.4) Tampering with block validity
			(2.4.1) No verification of block validity
			(2.4.2) Weak verification of block validity
		(2.5) Tampering with transaction validity
			(2.5.1) No verification of transaction validity
			(2.5.2) Weak verification of transaction validity
			(2.5.3) Violation of transaction integrity by a node prior to including in a block
		(2.6) Tampering with keys of epoch nodes
			(2.6.1) Replacing private keys of miner nodes
			[2.6.2] Replacing public key of miner beneficiary
		(2.7) Tampering with the persistent copy of the blockchain database (see Note 2.1)
			(2.7.1) Tampering the genesis blocks
			(2.7.2) Tampering blocks
		(2.8) Tampering with code (see Note 2.2)
			(2.8.1) Tampering with code in the Epoch code repository
			 	(2.8.1.1) Hiding malicious code in a commit (see Note 2.3);
			 	(2.8.1.2) Performing an insider attack;
			 	(2.8.1.3) Tampering with code using hijacked privileged accounts (see Note 2.4)
			(2.8.2) Tampering with code in a library built into the Epoch binary
			(2.8.3) Tampering with code in a shared dependency used by Epoch (See note 2.5)
			(2.8.4) Tampering with code during compilation (e.g. via build software, see Note 2.6)
			(2.8.5) Tampering from Erlang nodes on the same platform (see Note 2.7)

* **Notes**
* *Note 2.1: on (2.7) Database tampering*:
Epoch stores a persistent copy of the blockchain on some storage. Clearly this storage is hard to get to, but if stored on some cloud machine, it may be tampered with.

* *Note 2.2: on (2.8) Code Tampering*:
 The epoch node software is open source and constructed using other open source components or libraries.
* *Note 2.3: on (2.8.1.1) Tampering w. code in Epoch code repository*:
 * Example of a [similar past attack](https://getmonero.org/2017/05/17/disclosure-of-a-major-bug-in-cryptonote-based-currencies.html)

* *Note 2.4: on (2.8.1.2) Tampering w. code using hijacked privileged accounts*:
 * Example of a [similar past attack (1) Gentoo](https://wiki.gentoo.org/wiki/Project:Infrastructure/Incident_Reports/2018-06-28_Github)
 * Example of a [similar past attack (2) Kernel.org](https://pastebin.com/BKcmMd47)

* *Note 2.5: on (2.8.3) Reliance on the trusted computing base*:
The threats described under 2.8.1 apply also to all of the libraries that the Epoch nodes rely on.
See also note 2.6.

* *Note 2.6: on (2.8.4) Reflections on Trusting Trust*:
Based on [Ken Thomson's Turing award lecture](http://delivery.acm.org/10.1145/360000/358210/reflections.pdf?ip=85.227.246.214&id=358210&acc=OPEN&key=4D4702B0C3E38B35%2E4D4702B0C3E38B35%2E4D4702B0C3E38B35%2E6D218144511F3437&__acm__=1531250618_7cde18b767d136d90f839b6c196eeaae):
"*To what extent should one trust a statement that a program is free of Trojan horses?
Perhaps it is more important to trust the people who wrote the software.*"

* *Note 2.7: on (2.8.5) Colocated Erlang nodes*:
***Any*** Erlang node on the same platform can interact with the Epoch nodes

* **Related info**
	* [Unchecked block validity](https://github.com/Aeternity/protocol/blob/master/SYNC.md#incentives)




### (3) Repudiation
To be extended once the implementation of bitcoin-NG is stable.

	(3.1) Repudiating a future commitment
	(3.2) Repudiating a past transaction
		(3.2.1) Repudiating a past off-chain transaction
		(3.2.2) Repudiating a past on-chain transaction
				(3.2.2.1) Repudiating timely reception of an oracle response
				(3.2.2.2) Repudiating late submission of an oracle response

### (4) Information Disclosure

Considering that all information added to the blockchain is public, the scope of information disclosure is significantly reduced.

The working assumption is that the only data that must remain secret at all times are the private keys of nodes (see Assumptions above) and the private keys of the accounts, oracles, and contracts.
The threats to the confidentiality and integrity of the node private keys are listed in the ***Spoofing*** threat tree.

Hence, if the assumption is correct, the information disclosure threat tree is a subtree of the ***Spoofing*** threat tree

Update 2018-07-02, based on [issue#2](https://github.com/ThomasArts/aetmodel/issues/2) ***The messages exchanged in a state channel should be private — as long as peers cooperate —, i.e. MitM should not be possible***, i.e. assumption 1 is false.

Threat tree for threat vector (4): Information Disclosure.

	(4.1) Disclosure of messages in a state channel.
		(4.1.1) Adversary performs a MitM attack on the state channel to breach communication confidentiality and integrity;
		(4.1.2) Forcing early arbitration to breach communication confidentiality;


### (5) Denial of service

##### 1. Overloading with transactions
Creating and posting a transaction is a computationally cheap action for an adversary. A valid transaction is a transaction that can potentially be included in a future block and that a miner receives a fee for.
Validation of a transaction is computational cheap, but having to validate many transactions that cannot be included in a block, is a computational overhead for a node.
If an adversary could post enormous amounts of transactions to the network, it could potentially impact the rate in which correct transactions are accepted.
Transactions may validate but nevertheless not be possible to include in a block. For example, an adversary could post a spend-transaction including more tokens than the from account contains. This transaction is then kept in the transaction pool for a while and *check this* validated for each new block candidate. <br />
By posting enormous amounts of transactions to the network, the pool of transactions kept to be included in the next block could grow beyond memory capacity causing the node to crash or possible valid transactions being pruned.
Additionally the network capacity could be overloaded and therefore distribution of possible valid transactions be impacted as propagation of these may be delayed or stopped.

	(5.1) Posting invalid transactions.
	(5.2) Posting unusable transactions
			(5.2.1) Resubmitting unusable transactions directly to a node
			(5.2.2) Gossiping unusable transactions through the p2p network (related to 5.4.2.1)
	(5.3) Exploiting memory limitations
		(5.3.1) Memory leaks in cleaning transaction pool
		(5.3.2) Overloading memory with atoms
		(5.3.3) Overloading memory with non-garbage-collected processes
	(5.4) Exploiting network or communication vulnerabilities to degrade or deny service
		(5.4.1) Launch Eclipse attacks against a node or a set of nodes
			(5.4.1.1) Eclipse by connection monopolization
			(5.4.1.2) Eclipse by owning the table
			(5.4.1.3) Eclipse by manipulating time
			(5.4.1.4) Obtain node 'secret' used to determine peer selection from unverified pool
		(5.4.2) Network-wide attacks against the Aeternity network
			(5.4.2.1) Attacks to slow down the Aeternity network (See note 5.1)
			(5.4.2.2) Flooding the network with unresponsive nodes
		(5.4.3) Denial of Service against predefined peer nodes
			(5.4.3.1) Denial of Service using API functionality
			(5.4.3.2) Denial of Service using generic DoS methods
	(5.5) Exploiting software vulnerabilities to degrade or deny service
		(5.5.1) Improper check for unusual or exceptional condition
	(5.6) Exploiting epoch protocol vulnerabilities to degrade or deny service.
		(5.6.1) Refusing to cooperate after having opened the channel;
		(5.6.2) Refusing to sign a multi-party transaction;
		(5.6.3) Open channels up to the full capacity of the node;
		(5.6.4) Dropping messages on a state channel;
		(5.6.5) Exploiting errors in the contract language to run contracts without gas;
	(5.7) Exploiting code flaw in validation to create coins
	(5.8) Exploiting vulnerabilities in used libraries (e.g. cuckoo cycle validation code, degrading PoW check)
	(5.9) Exploiting code flaws to force hash collisions


 * **Notes**
 * *Note 5.1 on 5.4.2.1 Attacks to slow down the Aeternity network*: ISPs can delay block propagation between nodes even without being detected, using a so-called 'delay attack' (see A5.3 under "past attacks" below). While A5.3 has been demonstrated for Bitcoin, it is worth investigating how robust is Aeternity to such attacks.

 * **Past attacks**
 * [A5.1 | 2018 | Ethereum | Low-Resource Eclipse Attacks on Ethereum’s Peer-to-Peer Network (iacr eprint)](https://www.cs.bu.edu/~goldbe/projects/eclipseEth.pdf)
 * [A5.3 |2018 | Ethereum | Unhandled exception vulnerability exists in Ethereum API](https://nvd.nist.gov/vuln/detail/CVE-2017-12119)
 * [A5.3 |2017 | Bitcoin | Hijacking Bitcoin: routing attacks on cryptocurrencies | IEEE S&P](https://btc-hijack.ethz.ch/)

* **Background information**
	 * [2018 | Aeternity state channel incentives](https://github.com/Aeternity/protocol/tree/master/channels#incentives)
	 * [2018 | Aeternity state channel fees](https://github.com/Aeternity/protocol/tree/master/channels#fees)


### (6) Elevation of privilege
The working assumption is that the user model is flat, i.e. there is no difference between the privileges of any two nodes.
Hence, if the assumption is correct, the elevation of privilege threat tree only applies to underlying environment and is orthogonal to the software developed in this project.

**Update 2018-07-02** Assumption is FALSE, since [the AEVM executes untrusted code](https://github.com/ThomasArts/aetmodel/issues/3)

**Discuss:** As long as the network is small, there is a concept of Aeternity owned nodes that would be more "trustable" than other nodes. In the beginning it might be important to prevent a small different subset of nodes to take the role as trusted set to connect to. This falls under the threat of so-called ["altcoin infanticide"](https://bitcointalk.org/index.php?topic=56675.0).


	(6.1) EoP on the epoch node.
		(6.1.1)	Exploitable vulnerabilities in AEVM leading to EoP
		(6.1.2) Exploit Erlang distribution to get access to node
	(6.2) EoP in p2p network
		(6.2.1) EoP of an arbitrary node to status of trusted node
				(6.2.1.1) EoP though exploitabtion of API vulnerabilities;
				(6.2.1.2) EoP through forged Epoch node distributions;

## STRIDE Threat Trees

For each threat tree, the tables below describe the ***leaf*** nodes AND parent nodes with ***one*** leaf.
As a rule, when a leaf node becomes a parent it is replaced by one or more leaf node entries, except when justified by generic mitigation strategies (to avoid repetition).

### 1. (Node) Spoofing

|  Tree Node |Explanation   | Developer Mitigation   | Operational Mitigation   | Notes   | Actions | Priority |
|---|---|---|---|---|---|---|
| 1.1.1.1  | Use of weak or flawed PRNGs leading to generation of keys that are predictable or brute-forceable  | Ensure best-practice PRNG is used | [Libsodium PRNG](https://download.libsodium.org/doc/generating_random_data/) is used | relevant for mobile devices - past attacks exist | - | low priority (unlikely) |
| 1.1.1.2  | Vulnerabilities in key generation implementation leading to generation of keys that are predictable or brute-forceable  | Verify Key generation implementation and use keys of sufficient length | N/A | Private keys are 256 bits: both for P2P connections as well as for signing transactions. relevant for mobile devices - past attacks exist  | TODO: verify that the user cannot accidentally use a key with less than 256 bits;  | low priority (unlikely)|
| 1.1.1.2.1  | Vulnerabilities in the crypto library implementation of key generation implementation leading to generation of keys that are predictable or brute-forceable  | Extensive testing of the underlying crypto library | Short patching cycle | -  |  - | low priority (unlikely)|
| 1.1.1.2.2  | Vulnerabilities in the Epoch crypto functionality implementation leading to generation of keys that are predictable or brute-forceable | Extensive testing of the Epoch crypto functionality | Short patching cycle | -  |  - | medium priority |
|  1.1.2.1 | Vulnerabilities in the client platform, exploitable through trojans or viruses |  N/A | N/A  | Out of scope (OOS) | - | TBD |
|  1.1.2.2    | Vulnerabilities in 3rd party wallets and applications | N/A  |  N/A | OOS; NOTE: Risk of multiple account compromise   | - |TBD |
|1.1.2.3     |  Vulnerabilities in web services allowing an adversary to execute code on nodes to reveal the wallet| Security Testing  |  N/A | OOS; NOTE: Risk of multiple account compromise   | | TBD |
|1.1.2.4.1  | Malicious processes running on a shared operating system (with process isolation), leaking keys of neighbour nodes through operating system vulnerabilities or side-channel attacks | API for storing keys in a hardware enclave / on external device | Erlang ports should be closed; Regular OS and hypervisor patching;| - | - | TBD |
|1.1.2.4.2  | Malicious processes running on a shared hardware platform (with kernel isolation), leaking keys of neighbour nodes through operating system vulnerabilities or side-channel attacks | API for storing keys in a hardware enclave / on external device | Erlang ports should be closed; Regular OS patching; | - | -  | TBD |
|1.1.2.4.3  | Malicious processes running on a shared hardware platform (with hypervisor isolation), leaking keys of neighbour nodes through operating system vulnerabilities or side-channel attacks | API for storing keys in a hardware enclave / on external device | Erlang ports should be closed; Regular VM and hypervisor patching; | - | - | TBD |
|1.1.2.5  | Operators of virtualized infrastructure obtaining keys of nodes in virtual containers by reading files stored on disk | API for storing keys in a hardware enclave |  N/A |  Low bar | -| TBD|
|1.1.2.6.1  | Malicious mobile applications colocated with an Epoch node or wallet potentially leaking the private key | Leverage hardware-supported features  (e.g. ARM TrustZone) to protect private key |  N/A | - | - | TBD |
|1.1.2.6.2  | Malicious wallet implementations *leaking* Epoch node private keys | Develop applications that leverage hardware-supported features  (e.g. ARM TrustZone) to maintain key security while providing the necessary crypto services to e.g. wallets and Epoch nodes without revealing keys |  N/A | - |- |TBD |
|1.1.2.6.3  | Malicious wallet implementations *using* private keys to sign arbitrary transactions | N/A |  N/A | OOS | -| TBD|
|1.1.2.6.4  | Applications leaking individual private keys through cloud back-ups | N/A |  Disable  cloud back-ups for relevant applications | - |- | TBD|
|  1.1.3.1 | Exploiting external interfaces | Penetration testing of  external interfaces of application: http, web services and noise | N/A  | - | TODO: Define penetration testing | TBD|
|  1.1.3.2 | Misuse of node node functionality by obtaining access to the node  | N/A | Standard unix ports and Erlang distribution daemon blocked for incoming requests | - | TODO:specify what needs to be closed?? | TBD |
| 1.1.4  | Client implementation exposing private keys in logs and memory dumps | a. Ensure code never logs private key; b. User private keys are not handled by node (peer key and mining key are); c. Never send client logs/memory dumps unencrypted over public network; | Ensure secure access to monitoring software (datalog) | - | TODO: check encrypted submission to datalog | priority low |
| 1.1.5  | Error messages exposing private keys directly to a user or in logs and memory dumps | a. Ensure code never raises an error with  private key as argument; b. User private keys are not handled by node (peer key and mining key are); c. Never send client logs/memory dumps unencrypted over public network; | Ensure secure access to monitoring software (datalog) | - | TODO: check error messages | priority medium |
| 1.1.5.1  |  Exposing sensitive information - such as private keys - through arbitrary corruption of files | Ensure data considered security sensitive not exposed in logs unless explicitly unusable | Ensure secure access to monitoring software (datalog) | Example: aec_keys:setup_sign_keys/2; aec_keys:setup_peer_keys/2 | - | priority medium |
| 1.1.5.2  |  Exposing sensitive information - such as private keys - through logs and crash dumps | Ensure data considered security sensitive not exposed in logs unless explicitly unusable | Ensure secure access to monitoring software (datalog) | - | Example: none yet | priority medium |
| 1.1.5.3  |  Exposing sensitive information - such as private keys - through the Erlang VM crash dump | Minimize or eradicate vulnerabilities leading to Erlang VM crashes | Rapid patching of identified vulnerabilities | - | Example: none yet | priority medium |
|  1.2.1 | Exploiting code flaws in signature verification to spoof user actions | Thoroughly and continuously test signature verification code;  | Exclude/ignore outdated clients (?)  | -  | TODO: review robustness of signing | TBD|
|  1.2.2 |  Exploiting code flaw in transaction validation to spoof user actions | A binary serialization of each transactions is signed with the private key of the accounts that may get their balances reduced.  |  N/A | Signing is performed using NaCL cryptographic signatures (implemented in LibSodium). Forging a signature is considered extremely difficult. The LibSodium library has an active user community (*has it been certified?*). LibSodium is connected via the Erlang enacl library (*version ...*), which has been reviewed for security violations.  | TODO: Check libsodium guarantees and update to latest version of enacl |TBD |
|  1.3.1.1 |  Adversary observing the normal packet flow and inserting own packets. | Enforce transport integrity  |  N/A | - | Prevented using the Noise protocol with specific handshake and encryption | TBD  |
|  1.3.1.2 |  Adversary inserting own arbitrary packets without observing the packet flow. | Enforce transport integrity  | Transport layer security  | - | Prevented using the Noise protocol |  TBD |
|  1.3.2 |  DNS attack rerouting users to a scam site collecting user's login credentials | N/A  | N/A  | OOS  | - | TBD  |
|  1.4.1 |  Web service with malicious code exploiting internal node HTTP APIs  | Enforce strict origin policy  | N/A  | Needs further investigation  | -| TBD  |
|  1.4.2 |  Adversary exploiting the state channel HTTP API  | Security testing of the API  | N/A  | Needs further investigation  | - | TBD  |
|  1.4.3 |  Adversary exploiting the node's HTTP APIs  | Security testing of the API  | N/A  | Needs further investigation  | - |  TBD |
|  1.4.4 |  Adversary externally executing a fun over the nodes API  | Security testing of the API  | N/A  | Needs further investigation  | |  High (devastating consequences) |


### 2. Tampering
|  Tree Node |Explanation   | Developer Mitigation   | Operational Mitigation   | Notes   | Actions | Priority |
|---|---|---|---|---|---|---|
| 2.1.1  | Failing to implement connection integrity | Ensure channel integrity |  N/A |   Prevented using the Noise protocol with specific handshake and encryption |  Verify correct implementation using a QuickCheck model |-|
| 2.1.2  | Using weak algorithms to ensure connection integrity | Use cryptographically strong and well tested crypto algorithms and implementations  | N/A  | Prevented using the Noise protocol with specific handshake and encryption |   Verify correct implementation using a QuickCheck model| -  |
| 2.1.3  | Compromising connection security by nonce wrap back | Enforce key rotation | N/A | Nonce wraps back after 2^64 - 1 messages, long over channel lifetime | Needs further investigation; potentially enforce key rotation |  TBD |
|  2.2.1 | Failing to verify message integrity  | Ensure message integrity  |  N/A | Prevented using the Noise protocol with specific handshake and encryption | Verify correct implementation using a QuickCheck model  |TBD|
|  2.2.2 | Failing to correctly implement message integrity verification | Use cryptographically strong and well tested crypto algorithms and implementations   |  N/A |   Prevented using the Noise protocol with specific handshake and encryption |  Verify correct implementation using a QuickCheck model |TBD|
|  2.3.1 | Modifying order of transactions included in a block (due to a bug or malicious intent) | N/A |  N/A |  - |  Potentially a threat; Needs further investigation |  TBD |
|  2.3.2 | Modifying the timestamp in mined blocks (due to a bug or malicious intent) | N/A | N/A  |  - |  Discuss whether this is a threat | TBD  |
|  2.4.1 | Nodes failing to verify block validity before adding it to the blockchain  | Correct implementation of block validity verification in node implementation |  Strong incentives for nodes to validate blocks | -  |  Verify correct implementation using a QuickCheck model |  TBD |
|  2.4.2 | Nodes verifying block validity, but using an incomplete or flawed verification implementation | Correct implementation of block validity verification in node implementation |   N/A |  - |  Verify correct implementation using a QuickCheck model |   TBD|
|  2.5.1 | Nodes failing to verify transaction validity  | Correct implementation of transaction validity verification in node implementation |  (a) Protocol incentives for nodes to validate blocks; (b) Penalise nodes relaying invalid blocks/transactions |  Needs further investigation |  Verify correct implementation using a QuickCheck model |  TBD |
|  2.5.2 | Nodes verifying transaction validity, but using an incomplete or flawed verification implementation | Correct implementation of transaction validity verification in node implementation |  N/A  | -  |  Verify correct implementation using a QuickCheck model | TBD  |
|  2.5.3 | Nodes modifying transactions prior to including it in a block  | Ensure correct implementation of the signature scheme | N/A  |  - |  Verify correct implementation of the signature scheme using a QuickCheck model | TBD  |
|  2.6.1 | Tampering with the keys of miner nodes to obtain rewards from mining | Prevent run-time substitution of keys | Needs further investigation |  - | Review once protocol implementation stable  |  TBD |
|  2.7.1 | Tampering the genesis block in persistent DB | A node is isolated if genesis block differs, no communication with other epochs possible  | Ensure that database runs in protected area | -  |  - |  no issue |
|  2.7.2 | Tampering a block in persistent DB | DB is read at startup and all blocks are validated again, tampering will be noticed in block-hash that does not fit. If new consecutive hashes have been computed, then DB is considered a fork and tampered part is removed while syncing with other nodes |  Ensure that database runs in protected area |- |  - | no issue  |
|  2.8.1.1 | Hiding (potentially obfuscated) malicious code in a commit | N/A |  Conduct security reviews of external pull requests | |  - | low priority  |
|  2.8.1.2 | Abusing position of trusted insider (e.g. developer) to tamper with code integrity | N/A |  Perform background checks of developers; periodically re-evaluate potential personal vulnerabilities of developers (debts, addictions, vulnerable personal situation, etc.) | -| - | low priority  |
|  2.8.1.3 | Hijacking a privileged account (e.g. developer, release manager, etc.) to tamper with code integrity | N/A |  (a) Use strong, 2-factor authentication for code repository; (b) ensure passwords are not reused; (c) ensure security of 2nd factor; (d) ensure security of authentication gateway  | -|  - | low priority  |
|  2.8.2 | Tampering with code in a library built into the epoch binary | N/A |  (a) Bind releases to whitelisted release tags of dependency libraries   (b) Epoch security review and testing whenever release tag changes   (c) Lock checksum of dependencies used to build  | -|   -| low priority  |
|  2.8.3 | Tampering with code in the Epoch trusted computing base (incl. dependencies) | N/A |  (a) Bind releases to whitelisted release tags of dependency libraries  (b) Epoch security review and testing whenever release tag changes  | -|  - | low priority  |
|  2.8.4 | Tampering with code via build software prior to compilation |  N/A	 | Provide recommended toolchains for most common platforms |- | -| low priority  |
|  2.8.5 | Tampering with the Epoch node over another Erlang node running on the same platform |  N/A	 | OOS; run Epoch on a dedicated host (physical or virtual) |- |- | low priority  |


### 3. Repudiation
|  Tree Node |Explanation   | Developer Mitigation   | Operational Mitigation   | Notes   | Actions | Priority |
|---|---|---|---|---|---|---|
|  3.1 |  An Epoch node  repudiating a future commitment (e.g. as oracle) | N/A  |  N/A | Can someone "announce" a victim node X as oracle without node X's its consent? motivation: to "damage" a nodes' reputation as oracle;  Needs further investigation | -  |  TBD |
|  3.2.1 | Epoch node repudiating a past transaction that is not on the chain | N/A  | N/A  | OOS; Since a transaction on the chain is signed with private keys, only possible due to loss of private keys; safeguarding private keys is responsibility of the node  | -  |  TBD |
|  3.2.2 | Epoch node repudiating a past transaction that is on the chain | N/A |  N/A | Needs further investigation   |  - |  TBD |
|  3.2.2.1 |  Epoch node repudiating timely reception of oracle response (within originally posted TTL)  |  N/A | N/A  |  Needs further investigation | -  | TBD  |
|  3.2.2.2 | Oracle node repudiating late submission of a query response  |  N/A | adjust miner incentives  | Needs further investigation; since the oracle has no control (?) over when the transaction enters the chain, it can claim that it has posted an oracle response transaction "on time", but no miner picked it up;  | -  |   TBD|

### 4. Information Disclosure
|  Tree Node |Explanation   | Developer Mitigation   | Operational Mitigation   | Notes   | Actions | Priority |
|---|---|---|---|---|---|---|
| 4.1.1  |  Performing a MitM attack on the communication over a state channel  | In the [Aeternity naming system](https://github.com/aeternity/protocol/blob/master/AENS.md) - implement reliable mapping between peer names and keypairs; correct implementation of Noise protocol with specific handshake and encryption | N/A  |  - |  - |   TBD|
| 4.1.2  |  Performing a selective DoS attack on the state channel to force peer to revert to arbitration and (partly) disclose state channel content | Ensure arbitration requires minimum information about the messages exchanged on the state channel  | N/A  |  - |   -| TBD  |

### 5. Denial of service
|  Tree Node |Explanation   | Developer Mitigation   | Operational Mitigation   | Notes   | Actions | Priority |
|---|---|---|---|---|---|---|
| 5.1  | Posting invalid transactions  | The node that receives a transaction validates this transaction. Invalid transactions are rejected and never propagated to other nodes.  | Handling the http request is more work than validating the transaction. By standard http load balancing the number of posted transactions is the limiting factor, rejecting the transactions is cheap. |  - | Verify that indeed all invalid transactions are rejected using a QuickCheck model  | medium |
| 5.2  | Posting unusable transactions  | Validation is light-weight and ensures that if the transaction is accepted in a block candidate fee and gas can be paid.  | Valid transactions have a configurable TTL that determines how long a transaction may stay in the memory pool. By default a node is configured to have a transaction in the pool for at most 256 blocks.  |  - |  - |  TBD |
| 5.2.1  | Nodes resubmitting unusable transactions to a arbitrary node to cause a DoS  | Needs further investigation.  | Needs further investigation.  |  - |  - |   TBD|
| 5.2.2  | Nodes (both malicious and benign) flooding the the p2p network by continuously gossiping unusable transactions | Implement a scoring system for the peers; Needs further investigation.  | Needs further investigation. |  - |  - |   TBD|
| 5.3.1  | Exploiting memory leaks in cleaning transaction pool  | Erlang is a garbage collected language and additional garbage collection is implemented for invalid transactions.  | N/A  | Erlang does not garbage collect atoms. Transactions that are potentially able to create new atoms from arbitrary binaries (e.g. name claim transactions) should be reviewed | TODO: check for binary_to_atom in transaction handling. Verify memory constraints on transaction pool | low |
| 5.3.2  | Exceeding the limit of atoms to cause a node crash. | Ensure atoms are not created arbitrarily; ensure atoms are not created based on API input.  | N/A  |   On Erlang nodes, atoms are stored once for each unique atom in the atom table. Erlang does not garbage collect atoms. Transactions that are potentially able to create new atoms from arbitrary binaries (e.g. name claim transactions) should be reviewed | TODO: check for binary_to_atom in transaction handling. Verify memory constraints on transaction pool | low |
| 5.3.3  |  Causing a node crash by exceeding the limit of non-garbage-collected processes.  | Correct implementation of pid creation on nodes; ensure limits on number of processes created based on API input.  |  N/A | On Erlang nodes, process identifier refers into a process table and a node table. | TODO: check code spawning new processes on nodes | low |
| 5.4.1.1  | Monopolizing incoming node connections |  Needs further investigation | Needs further investigation  |  - | Adversary waits until the victim reboots (or deliberately forces the victim to reboot), and then immediately initiates incoming connections to victim from each of its adversary nodes. Attack shown for ETH - investigate relevance see [Persistence](https://github.com/Aeternity/F0kker50#Macrotocol/blob/master/GOSSIP.md#persistence) | TBD  |
|  5.4.1.2 |  Monopolizing outgoing node connections |  Needs further investigation |  Needs further investigation |  - | Adversary probabilistically forces the victim to form all outgoing connection to the adversary, combined with unsolicited incoming connection requests. Attack shown for ETH - investigate relevance; see [Peer Maintenance](https://github.com/Aeternity/protocol/blob/master/GOSSIP.md#peers-maintenance)|TBD |
|  5.4.1.3 | Eclipsing node by skewing time, e.g. by manipulating the network time protocol (NTP) used by the host |  Needs further investigation | Configure host to use secure/trusted NTP (esp. relevant for peers)  | -|Attack shown for ETH - investigate relevance|TBD |
|  5.4.1.4 | Eclipsing node by influencing peer selection from unverified pool; assumes obtaining 'secret' used for peer selection |  Needs further investigation | Needs further investigation  | -|Secret generation, storage and usage is [undocumented](https://github.com/Aeternity/protocol/blob/master/GOSSIP.md#bucket-selection) |TBD |
| 5.4.2  | Slowing down or disrupting the Aeternity network by tampering network traffic | N/A   |  N/A | Discuss whether in scope  | - | TBD  |
| 5.4.2.1  | Slowing down the Aeternity network by tampering with the outgoing and incoming messages of a subset of nodes  | Ensure message integrity   |  N/A | -  | Attack shown for Bitcoin - investigate relevance  | TBD  |
| 5.4.2.2  | Slowing down the Aeternity network by flooding the network with unresponsive nodes  | Score nodes, detect and remove disruptive ones |  N/A |  - | -  |  TBD |
| 5.4.3.1  | Flooding predefined peer nodes with requests on the Chain WebSocket API  |  Check request signature   | Throttle requests from same origin  |  - |  - |   TBD|
| 5.4.3.2  | Flooding predefined peer nodes with packets using DoS techniques on the TCP (SYN flood) or Epoch protocol level  |  N/A  | N/A  |  - | Investigate feasibility  |   TBD |
|  5.5 |  Exploiting API vulnerabilities to launch a DoS attack on either individual nodes or targeted groups of nodes  | Security testing of the API  |  N/A |  - | Verify that indeed all invalid transactions are rejected using a QuickCheck model (?) |  High |
|  5.5.1 | Using specially crafted JSON requests to cause an unhandled exception resulting in DoS | Security testing of the API  |  N/A |  - | Verify that indeed all invalid transactions are rejected using a QuickCheck model (?) |  High |
|  5.6.1 | Locking up other nodes' coins in transactions | N/A  |  Discouraged through incentives (?); Users are expected to act rationally. |  Opening a channel with a peer and subsequently refusing to cooperate, [locking up coins](https://github.com/Aeternity/protocol/tree/master/channels#incentives) and making the peer pay the channel closing fees.  | - |   TBD|
|  5.6.2 | Making a transaction unusable | N/A  |  Halt interactions if on-chain fees reach the point, where the fees required to timely close a channel approach the balance of the channel; Discouraged through incentives | Refusing to sign a transaction when the channel holds significant funds and the account sending the transaction does not have sufficient funds to close the channel. Needs further investigation |  -|  TBD|
|  5.6.3 | Locking up coins on multiple channels | Discouraged through incentives  |  Implement deterring incentives in protocol |  Opening multiple channels with a peer (up to the capacity of the WebSocket and subsequently refusing to cooperate, locking up coins and making the peer pay the channel closing fees. Needs further investigation |  -| TBD  |
|  5.6.4 | Dropping arbitrary packets on a state channel to disrupt or degrade communication between two peers. | N/A  |  Discouraged through incentives |  Needs further investigation |  -|  High |
|  5.7 | Exploiting code flaw in validation to create coins (cf. CVE-2010-5139) | Verify code for possible overflows to be handled as expected  |  N/A |  Needs further investigation |  -|  High |
|  5.8 | Exploiting vulnerabilities in used libraries (e.g. cuckoo cycle validation code, degrading PoW check) | Check for security audits on used libraries  |  N/A |  Needs further investigation |  -|  TBD |
|  5.9 | Exploiting code flaws to force hash collisions (cf. CVE-2012-2459) | Check for implantation of hash ingredients and tree library used |  N/A |  Needs further investigation |  -|  TBD |


### 6. Elevation of privilege
|  Tree Node |Explanation   | Developer Mitigation   | Operational Mitigation   | Notes | Actions | Priority |
|---|---|---|---|---|---|---|
| 6.1.1  | Running malicious code embedded in contacts | Sanity checks for code in smart contracts?  | N/A  |  Malicious code embedded in the contracts can be run to exploit vulnerabilities in AEVM and lead to elevation of privilege on the epoch node or disclosure of information | Correct implementation and security testing of the AEVM | TBD  |
| 6.1.2 | Exploiting known Erlang cookie to connect to another Erlang node | Node is started with -sname which disallows access from different IP address | Erlang daemon only listens to localhost  |  Erlang daemon accepts incoming connection from other Erlang node (default cookie is epoch_cookie) |  - | low |
| 6.2.1.1  | Exploit Epoch node API vulnerabilities to obtain status of trusted node |  Security testing of Epoch node APIs | N/A  | -  |   |  TBD |
| 6.2.1.2  | Creating custom distribution of Epoch node code with a modified set of trusted nodes	 | N/A  | Encourage use of "genuine" epoch nodes |  Discuss potential as "existential" risk to the network |  - |    TBD|


## Questions and concerns

1. Reusing cryptographic keys for different functions is considered bad practice but [commonly done in public key cryptography](https://crypto.stanford.edu/RealWorldCrypto/slides/kenny.pdf).
Considering that the "Private Keys" (see **Assets**) are used to both authenticate nodes and authorize transactions, it is **essential** to review the security of this reuse pattern.

* The ***privilege levels*** is the system must be documented to add further details to the threat model.

* Password for keypair protection stored in CONFIG file OR as an environment variable is NOT a good practice (example in aec_keys:start_worker/0; config in epoch_config_schema.json)

* In epoch_config_schema.json: "Password used to encrypt the peer key-pair files - if left blank `password` will be used." ***Such defaults provide a false sense of security and should not be used.***
* In epoch_config_schema.json: "used to encrypt the peer key-pair files" - it does not make sense to encrypt the public key file (investigate if that is actually done).

	"peer_password" : {
			"description" :
			"Password used to encrypt the peer key-pair files - if left blank `password` will be used",
			"type" : "string"
		}
* **[Discussion]** In aec_peers, '-type peer\_id(): What is the consideration behind using the public key (and not e.g. a hash of it) as peer id?

* **[Discussion]** In epoch_config_schema.json: ***Is it intended that the default contradicts the comment?***
	"extra_args" : { "description" : "Extra arguments to pass to the miner executable binary. The safest choice is specifying no arguments i.e. empty string.",
		                                    "type" : "string",
		                                    "default": "-t 5"
		                                },

* **[Discussion]** In epoch_config_schema.json: ***consider placing such controls in a separate file - otherwise there is a high risk of deliberately misleading users to make damaging changes, this can damage availability.***
		"node_bits" : {
		"description" : "Number of bits used for representing a node in the Cuckoo Cycle problem. It affects both PoW generation (mining) and verification. WARNING: Changing this makes the node incompatible with the chain of other nodes in the network, do not change from the default unless you know what you are doing.",
		                                    "type": "integer",
		                                    "default": 28


## Next steps
 1. **[Developers]** Review threats, describe additional developer mitigations - potential or in place.
 2. **[Developers]** Review threat prioritization; re-assign priority level where relevant.
 * **[Security researchers]** Complete threat trees with additional threat vectors.
 * **[Security researchers]** Review code to check if developer mitigations are in place.
 * **[Security researchers+Developers]** Implement threat mitigations where missing;
 * **[Security researchers]** Review code to identify potential security vulnerabilities in the implementation.
 * **[Security researchers]** penetration testing of an arbitrary Aeternity node;
 * **[Security researchers]** penetration testing of Aeternity trusted nodes;
 * **[Contributors]** The threat model continuously evolves together with the feature and the security landscape.
Therefore, this document must be periodically revised and updated.

## Conclusions
This document describes a snapshot of the threat model for the Aeternity blockchain, following the STRIDE threat modelling approach.
To the best of our knowledge, this is a first publicly available systematic threat model of a blockchain project.
We have described threat trees in 6 categories: spoofing, tampering, repudiation, information disclosure, denial of service and elevation of privilege
We detailed the high-level threat trees in tables.
Along with a threat identifier, each table entry contains an explanation and (where applicable) developer mitigation, operational mitigation, notes, actions and priority.
Threat descriptions shall be updated with further details once the Aeternity codebase stabilizes and threats are better understood.
Threat priority should be periodically revised if new (and potentially unforeseen) usage models emerge.
We encourage collaborators to contribute and improve this threat model.

### Threats to be mitigated
To be completed after a review of how the threats are addressed in the codebase.

### Threats to be eliminated
To be completed after a review of how the threats are addressed in the codebase.

### Threats to be transferred
To be completed after a review of how the threats are addressed in the codebase.

### Accepted risks
To be completed after a review of how the threats are addressed in the codebase.


## References

[1] P. Torr, "Demystifying the Threat-Modelling Process," in IEEE Security & Privacy, vol. 3, no. , pp. 66-70, 2005.
[doi](10.1109/MSP.2005.119), [url](doi.ieeecomputersociety.org/10.1109/MSP.2005.119)<br />
[2] A. Shostack "Threat Modelling: Designing for Security", ISBN: 978-1-118-80999-0, Feb 2014
