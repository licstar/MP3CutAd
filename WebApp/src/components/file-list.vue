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
}
</style>

<template>
<div class="file-list">
  <div class="control">
    <!-- <input type="button" :value="expandButtonText" @click="expand = !expand" /> -->
  </div>
  <ul class="list" v-show="expand">
    <li v-for="file in files" class="item v-box">
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
var TrackViewer = require('./track-viewer.vue')

module.exports = {
  props: ['files', 'type-count', 'selected-ad'],
  data() {
    return {
      expand: true
    }
  },
  computed: {
    expandButtonText() {
      return this.expand ? '-' : '+'
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
    TrackViewer
  }
}
</script>