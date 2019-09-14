import Vue from 'vue'
import axios from 'axios'

export const state = () => ({
  contracts: []
})

export const mutations = {
  setContracts (state, contracts) {
    for (let contract of contracts) {
      Vue.set(state.contracts, contract.contract_id, contract)
    }
  }
}

export const actions = {
  getContracts: async function ({ rootState: { nodeUrl }, commit }, { page, limit }) {
    try {
      const url = `${nodeUrl}/middleware/contracts/all?limit=${limit}&page=${page}`
      const contracts = await axios.get(url)
      console.info('MDW 🔗 ' + url)
      commit('setContracts', contracts.data)
      return contracts.data
    } catch (e) {
      console.log(e)
      commit('catchError', 'Error', { root: true })
    }
  },

  getContractTx: async function ({ rootState: { nodeUrl }, commit }, contractId) {
    try {
      const url = `${nodeUrl}/middleware/contracts/transactions/address/${contractId}`
      const contractTx = await axios.get(url)
      console.info('MDW 🔗 ' + url)
      return contractTx.data.transactions
    } catch (e) {
      console.log(e)
      commit('catchError', 'Error', { root: true })
    }
  },
  getContractCalls: async function ({ rootState: { nodeUrl }, commit }, contractId) {
    try {
      const url = `${nodeUrl}/middleware/contracts/calls/address/${contractId}`
      const contractCalls = await axios.get(url)
      console.info('MDW 🔗 ' + url)
      return contractCalls.data
    } catch (e) {
      console.log(e)
      commit('catchError', 'Error', { root: true })
    }
  }
}
