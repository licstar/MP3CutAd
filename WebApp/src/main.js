var Vue = require('vue')
var App = require('./app.vue')
window.Vue = Vue

Vue.config.debug = true;

new Vue({
  el: 'body',
  components: {
    app: App
  }
})