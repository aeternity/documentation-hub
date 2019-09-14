<template>
  <div class="app-generation-details">
    <PageHeader
      title="Generation Details"
      :has-crumbs="true"
      :has-nav="true"
      :page="{to: '/generations', name: 'Generations'}"
      :subpage="{to: `/generations/${$route.params.generation}`, name: 'Generation Details'}"
      :prev="prev"
      :next="next"
    />
    <GenerationDetails
      :data="generation"
      :dynamic-data="height"
    />
    <MicroBlocks>
      <MicroBlock
        v-for="(microBlock, number) in generation.micro_blocks"
        :key="number"
        :data="microBlock"
      >
        <TXListItem
          v-for="(transaction, index) in microBlock.transactions"
          :key="index"
          :data="transaction"
        />
      </MicroBlock>
    </MicroBlocks>
  </div>
</template>

<script>

import GenerationDetails from '../../../partials/generationDetails'
import MicroBlocks from '../../../partials/microBlocks'
import MicroBlock from '../../../partials/microBlock'
import PageHeader from '../../../components/PageHeader'
import TXListItem from '../../../partials/transactions/txListItem'

export default {
  name: 'AppGenerationDetails',
  components: {
    PageHeader,
    GenerationDetails,
    MicroBlocks,
    MicroBlock,
    TXListItem
  },
  data () {
    return {
      height: 0,
      prev: '',
      next: '',
      generation: null
    }
  },
  async asyncData ({ store, params }) {
    let generation = null
    const current = Number(params.generation)
    const height = await store.dispatch('height')
    if (store.generations && store.generations.generations[current]) {
      generation = store.generations.generations[current]
    } else {
      const generations = await store.dispatch('generations/getGenerationByRange', { start: current - 1, end: current + 1 })
      generation = generations[current]
    }
    const prev = current < 1 ? '' : `/generations/${current - 1}`
    const next = height === current ? '' : `/generations/${current + 1}`
    return { generation, prev, next, height }
  }
}
</script>
