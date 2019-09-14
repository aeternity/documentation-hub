const pkg = require('./package')

module.exports = {
  mode: 'universal',

  /*
  ** Headers of the page
  */
  head: {
    title: 'æpp-middleware',
    meta: [
      { charset: 'utf-8' },
      { name: 'viewport', content: 'width=device-width, initial-scale=1' },
      { hid: 'description', name: 'description', content: pkg.description }
    ],
    link: [
      { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' }
    ]
  },

  /*
  ** Customize the progress-bar color
  */
  loading: {
    continuous: true,
    color: '#FF0D6A'
  },

  /*
  ** Global CSS
  */
  css: [
    { src: 'styles/index.scss', lang: 'scss' },
    {
      src: 'vue-multiselect/dist/vue-multiselect.min.css',
      lang: 'css'
    }
  ],
  env: {
    middlewareURL: process.env.NUXT_APP_NODE_URL || 'https://testnet.mdw.aepps.com',
    middlewareWS: process.env.NUXT_APP_NODE_WS || 'wss://testnet.mdw.aepps.com/websocket',
    networkName: process.env.NUXT_APP_NETWORK_NAME || 'TEST NET',
    swaggerHub: process.env.NUXT_APP_SWAGGER_HUB || 'https://app.swaggerhub.com/apis-docs/sshekhar/aepp-middleware/1.0',
    faucetNetwork: process.env.NUXT_APP_FAUCET_NETWORK || 'ae_uat',
    faucetAPI: process.env.NUXT_APP_FAUCET_API || 'https://testnet.faucet.aepps.com/account'
  },
  /*
  ** Plugins to load before mounting the App
  */
  plugins: [
    { src: '~/plugins/directives/copyToClipboard.js' },
    { src: '~/plugins/directives/removeSpacesOnCopy.js' }
  ],
  /*
    ** Router config
    */
  router: {
    linkActiveClass: 'active-link',
    linkExactActiveClass: 'exact-active-link'
  },
  /*
  ** Nuxt.js modules
  */
  modules: [
    // Doc: https://github.com/nuxt-community/axios-module#usage
    '@nuxtjs/axios',
    '@nuxtjs/svg-sprite'
  ],
  /*
  ** Axios module configuration
  */
  axios: {
    // See https://github.com/nuxt-community/axios-module#options
  },

  /*
  ** Build configuration
  */
  build: {
    /*
    ** You can extend webpack config here
    */

    postcss: {
      plugins: {
        autoprefixer: {}
      }
    },
    extend (config, ctx) {
      // Run ESLint on save
      if (ctx.isDev && ctx.isClient) {
        config.module.rules.push({
          enforce: 'pre',
          test: /\.(js|vue)$/,
          loader: 'eslint-loader',
          exclude: /(node_modules)/
        })
      }
    }
  }
}
