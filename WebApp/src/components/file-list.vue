<style lang="less" scoped>
@import url(../settings.less);

.file-list {
  ul {
    list-style: none;
    margin: 0;
    padding: 0;

    >li.item {
      position: relative;
      margin-bottom: 4px;
      background-color: @xxxl-color;

      >header {
        position: absolute;
        left: 0;
        top: 0;
      }
    }
  }

  /* always present */
  .file-list-expand-transition {
    transition: height .2s ease, opacity .2s ease;
    height: 36px;
    overflow: hidden;
  }
  /* .file-list-expand-enter defines the starting state for entering */
  /* .file-list-expand-leave defines the ending state for leaving */
  .file-list-expand-enter, .file-list-expand-leave {
    height: 0;
    opacity: 0;
  }
}

</style>

<template>
<div class="file-list">
  <ul class="list" v-show="expand">
    <li v-for="file in files" class="item v-box" transition="file-list-expand">
      <header class="h-box">
        <span>{{file.filename}}</span>
      </header>
      <track-viewer
        :file="file"
        :type-count="typeCount"
        :selected-ad="selectedAd"
        @ad-mouseover="adMouseover"
        @ad-mouseout="adMouseout"></track-viewer>
    </li>
  </ul>
</div>
</template>

<script>
var Vue = require('vue')
Vue.transition('file-list-expand', {
})

module.exports = {
  props: ['files', 'type-count', 'selected-ad'],
  data() {
    return {
      expand: true
    }
  },
  methods: {
    adMouseover(ad) {
      this.$broadcast('ad-active', ad)
    },
    adMouseout(ad) {
      this.$broadcast('ad-deactive', ad)
    }
  },
  components: {
    TrackViewer: require('./track-viewer.vue')
  }
}
</script>