# TUTORIAL: How to build a To-do list Æpp - Part Three(3)

## Overview
This tutorial will teach you how to develop the communication process between the client side and the *ToDoManager* smart contract. We will setup the basic infrastructure in order to interact with the smart contract from the frontend.

## Prerequisites
- Before we go any further, please make sure you have followed the <a href="https://github.com/aeternity/tutorials/blob/master/build-to-do-list-aepp-1.md" target="_blank">first part</a> of *How to build a To-do list Æpp*.

## Plan
I have published the simple frontend project on Github. We will start with cloning the github repository and I will explain the most important segments of the *Advance To-do list Æpp*.

## Getting started 
1. Clone the Github repository and open your IDE at the directory:
```
git clone https://github.com/aekiti/advance-todo-aepp.git
```
2. Install the required dependencies on the aepp:
```
npm install
```
3. Start the aepp:
```
npm run serve
```
The ToDo List Æpp runs at http://localhost:8080.

## Applicatrion Structure
In this tutorial we will focus on the **account.js**, **contractDetails.js**, and **ToDo.vue** files. These files contain the essence of our project. Тhe other files are specific to the configuration of the frontend framework you have chosen - in our case - *Vue.js*. 

Here is the structure of the *src* directory for the cloned project:
```
.
├── assets
│   └── logo.png
│   └── logo-small.png
├── components
│   └── Header.vue
│   └── ToDo.vue
├── config
│   └── account.js
│   └── contractDetails.js
├── store
│   └── store.js
│   └── toDo.module.js
├── App.vue
├── main.js
```

This tutorial will not deal with creation of the ui components and styling them.

*Мost of the code is crammed into the ```ToDo.vue``` file. That is so, as for the purpose of this tutorial, good separation is not essential. Please do follow the VUE best practices once you start developing outside of this tutorial.*

## Application flow
### account.js file
The ```account.js``` file contains the configuration settings for the aepp in ```~/advance-todo-aepp/src/config/account.js```. For example:
```js
export default {
  pub: 'ak_2mwRmUeYmfuW93ti9HMSUJzCk1EYcQEfikVSzgo6k2VghsWhgU',
  priv: 'bb9f0b01c8c9553cfbaf7ef81a50f977b1326801ebf7294d1c2cbccdedf27476e9bbf604e611b5460a3b3999e9771b6f60417d73ce7c5519e12f7e127a1225ca',
  nodeUrl: 'https://sdk-testnet.aepps.com',
  compilerUrl: 'https://compiler.aepps.com',
}
```
The ```account.js``` file contains the information for:
- the public key;
- the private key;
- the node we want to connect to;
- the compiler we want to connect to;

We want to test our *To-do list Æpp* on the testnet newtork. The nodeUrl property should be ```https://sdk-testnet.aepps.com```, the compilerUrl property is ```https://compiler.aepps.com``` while the pub and priv property is the wallet keypair.

### contractDetails.js file
The ```contractDetails.js``` file contains the information for the contract and the deployed contract address in ```~/advance-todo-aepp/src/config/contractDetails.js```. For instance:

```js
export default {
  contractAddress: 'ct_28HXSRkYyQDr4nRzYuUTDyRxy9NGJRYuTpi5LUiyvb7oG9tJaj',
  contractSource: `contract ToDoManager =

  record todo = {
    name: string,
    is_completed: bool}

  record state = {
    map_user_todos: map(address, map(int, todo)),
    map_user_todo_count: map(address, int),
    map_user_todo_id: map(address, int)}


  stateful entrypoint init() =
    { map_user_todos = {},
      map_user_todo_count = {},
      map_user_todo_id = {}}

  entrypoint get_todo_count(user: address) : int =
    Map.lookup_default(user, state.map_user_todo_count, 0)

  private function get_todo_id(user: address) : int =
    Map.lookup_default(user, state.map_user_todo_id, 0)

  stateful entrypoint add_todo(todo_name: string) : int =
    let new_todo : todo = {
      name = todo_name,
      is_completed = false}

    let count = get_todo_count(Call.caller) + 1
    let id = get_todo_id(Call.caller) + 1

    put(state{map_user_todos[Call.caller = {}][id] = new_todo})
    put(state{map_user_todo_count[Call.caller] = count})
    put(state{map_user_todo_id[Call.caller] = id})

    id

  stateful entrypoint edit_todo_state(todo_id: int, is_completed: bool) =

    let current_todo : todo = get_todo_by_id'(Call.caller, todo_id)
    let edited_todo : todo = {
      name = current_todo.name,
      is_completed = is_completed}

    put(state{map_user_todos[Call.caller][todo_id] = edited_todo})

  stateful entrypoint edit_todo_name(todo_id: int, todo_name: string) =

    let current_todo : todo = get_todo_by_id'(Call.caller, todo_id)
    let edited_todo : todo = {
      name = todo_name,
      is_completed = current_todo.is_completed}

    put(state{map_user_todos[Call.caller][todo_id] = edited_todo})

  stateful entrypoint delete_todo(todo_id: int) =
    let todos: map(int,todo) = Map.lookup_default(Call.caller, state.map_user_todos, {})
    let updated_todos = Map.delete(todo_id, todos)

    put(state{map_user_todos[Call.caller] = updated_todos})

    let count = get_todo_count(Call.caller) - 1
    put(state{map_user_todo_count[Call.caller] = count})

  entrypoint get_todo_by_id(id: int) : todo =
    let todos: map(int,todo) = Map.lookup_default(Call.caller, state.map_user_todos, {})
    let result = switch(Map.lookup(id, todos))
      // None => {}
      Some(x) => x

    result

  entrypoint get_todos() =

    let user_todos = Map.lookup_default(Call.caller, state.map_user_todos, {})
    let todos = Map.to_list(user_todos)
    todos

  private function convert_bool_to_string(expression: bool) : string =
    switch(expression)
      true => "true"
      false => "false"

  private function get_todo_by_id'(user: address, id: int) : todo =
    let todos: map(int,todo) = Map.lookup_default(user, state.map_user_todos, {})

    let result = switch(Map.lookup(id, todos))
      // None => {}
      Some(x) => x

    result`
}
```

In the <a href="https://github.com/aeternity/tutorials/blob/master/build-to-do-list-aepp-1.md" target="_blank">previous tutorial</a> we have deployed the ```ToDoManager.aes``` contract to the testnet, we will use the deployed contract address as a ```contractAddress``` property.

### Todo.vue file
The application flow starts with the authentication step. The code placed in the ```getClient``` lifecycle hook provides client and contractInstance in ```~/advance-todo-aepp/src/components/ToDo.vue```.

The contractInstance will use the ```.call``` method to execute a contract funtion that either reads from the contract without chaning the state or a contract function the changes the state of the contract after getting the client.

```js
async getClient() {
  try {
    this.client = await Universal({
      nodes: [{ 
        name: 'testnet', 
        instance: await Node({ 
          url: account.nodeUrl, 
          internalUrl: account.nodeUrl
        })
      }],
      accounts: [
        MemoryAccount({ 
          keypair: { 
            secretKey: account.priv, 
            publicKey: account.pub 
          } 
        })
      ],
      compilerUrl: account.compilerUrl,
    });

    this.$store.dispatch('setAccount', this.client);
    this.balance = await this.client.balance(account.pub).catch(() => 0);
    this.contractInstance = await this.client.getContractInstance(contractDetails.contractSource, { contractAddress: contractDetails.contractAddress });
  } catch (err) {
    console.log(err);
  }
},
```

#### Get all tasks
The first step after successful authentication is to take all the tasks in the contract, if any. Тhis is achieved by calling the ```get_todos``` function of our ```ToDoManager``` contract. Finally we will store the results in the ```toDos``` array and visualize it in the frontend template.

```js
  async getContractTasks() {
    const allToDosResponse = await this.contractInstance.call("get_todos", []);
    const allToDos = await allToDosResponse.decode();
    const parsedToDos = this.convertSophiaListToTodos(allToDos);
    
    this.$store.dispatch('setToDos', parsedToDos);
  },
  convertToTODO(data) {
    return {
      title: data.name,
      isCompleted: data.is_completed
    }
  },

  convertSophiaListToTodos(data) {
    let tempCollection = [];
    let taskId;

    for (let dataIndex in data) {
      let todoInfo = data[dataIndex];

      taskId = todoInfo[0];
      let todo = this.convertToTODO(todoInfo[1]);
      todo.id = taskId;

      tempCollection.push(todo);
    }

    return tempCollection;
  }
```

#### Create a task 
Creating a task is done by calling the ```add_todo``` function of our ```ToDoManager``` contract. Here we will use the ```.call``` to change the contract state:
```js
  async addTodo() {
    this.$store.dispatch('toggleLoading');
    try {
      const value = this.newTodo && this.newTodo.trim()
      if (!value) {
        return
      }

      const allToDos = this.allToDos;

      allToDos.push({
        id: allToDos.length > 0 ? allToDos.length : 0,
        title: value,
        completed: false
      });

      await this.contractInstance.call('add_todo', [value]);

      await this.getContractTasks();

      this.$store.dispatch('toggleLoading');
      this.newTodo = ''
    } catch (e) {
      console.log(e);
      this.$store.dispatch('toggleLoading');
    }
  },
```

#### Change task status
Changing a task status is done by calling the ```edit_todo_state``` function of our ```ToDoManager``` contract.
```js
  async toggleTaskStatus(key) {
    this.$store.dispatch('toggleLoading');

    try {
      await this.contractInstance.call('edit_todo_state', [this.allToDos[key - 1].id, !this.allToDos[key - 1].isCompleted]);
      this.$store.dispatch('toggleTaskStatus', key);
      this.$store.dispatch('toggleLoading');
    } catch (err) {
      this.$store.dispatch('toggleLoading');
      console.log(err);
    }
  },
```

## Conclusion
Let’s briefly recap what we did during this tutorial series. 
In the <a href="https://github.com/aeternity/tutorials/blob/master/build-to-do-list-aepp-1.md" target="_blank">first part</a> we created a basic ```ToDoManager.aes``` contract that can:

- create a new task;
- read a specific task;
- edit a specific task;
- delete a specific task;
- get all tasks;
- get total tasks count;
- get task by index;

We wrote basic unit tests for the contract. Compile it with ```testnet``` network selected and deploy the contract to the ```testnet```.
**This part** shows us how to communicate with the previously deployed contract.
The included features are:

- connecting the account with testnet;
- getting the account's balance;
- getting all the task and visualize them in the browser;
- creating a task by typing name in a input field and press enter;
- chaning a task status by a button click;

You can now create a own awesome **Æpp**.

The æternity team will keep this tutorial updated. If you encounter any problems please contract us through the [æternity dev Forum category](https://forum.aeternity.com/c/development).