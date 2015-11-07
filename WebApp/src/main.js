var Vue = require('vue')
var App = require('./app.vue')
window.Vue = Vue

if (process.env.NODE_ENV !== 'production') {
  Vue.config.debug = true;
}

new Vue({
  el: 'body',
  components: {
    app: App
  }
})