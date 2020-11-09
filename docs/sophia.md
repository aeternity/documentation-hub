# What are Smart Contracts ?

- A smart **Contract** is a program on the blockchain that lives in the **contract state tree** in a full node.
- A contract runs on a virtual machine.
- A contract is owned by an **contract owner**.
- The contract owner creates a contract through posting a **create contract transaction** on the chain.
- Alternatively an owner can attach a contract to an existing account through an **attach contract transaction**.
- The contract creation transaction register an account as a contract. (One account - one contract)
- No one will have the private key for accounts created with create contract.
- Any user can call an exported function in a contract by posting a  **contract call transaction** on the chain.
- Contract can be written in a high level language which is compiled to the VM byte code.

# How do I write them ?

Hop over to the [sophia documentation](http://aeternity-sophia.readthedocs.io/) and learn to write and deploy your first contract. 
Use [AEstudio](http://https://studio.aepps.com/), the free web-IDE for writing and testing smart contracts on Aeternity!