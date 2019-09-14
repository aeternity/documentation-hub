import axios from 'axios'
export const state = () => ({
  names: []
})

export const mutations = {
  setNames (state, names) {
    state.names = [...state.names, ...names]
  }
}

export const actions = {
  getNames: async function ({ rootState: { nodeUrl }, commit }, { page, limit }) {
    try {
      const url = `${nodeUrl}/middleware/names?limit=${limit}&page=${page}`
      const names = await axios.get(url)
      console.info('MDW 🔗 ' + url)
      commit('setNames', names.data)
      return names.data
    } catch (e) {
      console.log(e)
      commit('catchError', 'Error', { root: true })
    }
  },
  searchNames: async function ({ rootState: { nodeUrl }, commit }, query) {
    try {
      const url = `${nodeUrl}/middleware/names/${query}`
      const names = await axios.get(url)
      console.info('MDW 🔗 ' + url)
      return names.data
    } catch (e) {
      console.log(e)
      commit('catchError', 'Error', { root: true })
    }
  }
}
