<template>
  <div class="app-transactions">
    <PageHeader
      title="Contracts Transactions"
      :has-crumbs="true"
      :page="{to: '/Contracts', name: 'Contracts'}"
      :subpage="{to: `/contracts/transactions/${$route.params.id}`, name: 'Contract Transactions'}"
    />
    <TransactionDetails
      v-for="tx of transactions"
      :key="tx.hash"
      :data="tx"
    />
  </div>
</template>

<script>

import PageHeader from '../../../components/PageHeader'
import TransactionDetails from '../../../partials/transactionDetails'

export default {
  name: 'ChannelTransactions',
  components: {
    TransactionDetails,
    PageHeader
  },
  data () {
    return {
      contract: '',
      transactions: []
    }
  },
  async asyncData ({ store, params }) {
    let transactions = await store.dispatch('contracts/getContractTx', params.id)
    const calls = await store.dispatch('contracts/getContractCalls', params.id)
    for (const tx of transactions) {
      const call = calls.find(x => x.transaction_id === tx.hash)
      if (call) {
        tx.arguments = call.arguments
        tx.callinfo = call.callinfo
        if (call.result) {
          tx.result = call.result
        }
      }
    }
    return { contract: params.id, transactions }
  }
}
</script>
