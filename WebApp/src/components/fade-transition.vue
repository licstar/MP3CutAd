<style lang="less">
/* always present */
.fade-transition {
  transition: opacity .3s ease;
  overflow: hidden;
}

/* .fade-enter defines the starting state for entering */
/* .fade-leave defines the ending state for leaving */
.fade-enter, .fade-leave {
  opacity: 0;
}
</style>

<script>
var Vue = require('vue')

Vue.transition('fade', {
  // beforeEnter: function (el) {
  //   el.textContent = 'beforeEnter'
  // },
  // enter: function (el) {
  //   el.textContent = 'enter'
  // },
  // afterEnter: function (el) {
  //   el.textContent = 'afterEnter'
  // },
  // enterCancelled: function (el) {
  //   // handle cancellation
  // },

  // beforeLeave: function (el) {
  //   el.textContent = 'beforeLeave'
  // },
  // leave: function (el) {
  //   el.textContent = 'leave'
  // },
  // afterLeave: function (el) {
  //   el.textContent = 'afterLeave'
  // },
  // leaveCancelled: function (el) {
  //   // handle cancellation
  // }
})
</script>