<template>
  <div class="app-transactions">
    <PageHeader
      title="Channel Transactions"
      :has-crumbs="true"
      :page="{to: '/channels', name: 'Channels'}"
      :subpage="{to: `/channels/transactions/${$route.params.id}`, name: 'Channel Transactions'}"
    />
    <TxList>
      <TXListItem
        v-for="tx of transactions"
        :key="tx.hash"
        :data="tx"
      />
    </TxList>
  </div>
</template>

<script>

import TxList from '../../../partials/transactions/txList'
import TXListItem from '../../../partials/transactions/txListItem'
import PageHeader from '../../../components/PageHeader'

export default {
  name: 'ChannelTransactions',
  components: {
    TxList,
    TXListItem,
    PageHeader
  },
  data () {
    return {
      transactions: []
    }
  },
  async asyncData ({ store, params }) {
    const transactions = await store.dispatch('channels/getChannelTx', params.id)
    return { transactions }
  }
}
</script>
