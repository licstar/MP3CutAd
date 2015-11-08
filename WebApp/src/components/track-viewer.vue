<style lang="less" scoped>
@import url(../settings.less);

.track-viewer {
  @control-size: 18px;
  height: @control-size * 2;

  .time {
    position: absolute;
    right: 0;
    bottom: 0;
    padding: 2px 4px;
    background-color: @o-xl-color;
    font-size: 12px;
    z-index: 10;
  }

  >.remove {
    position: absolute;
    right: 2px;
    top: 2px;
    opacity: @o-xl;
    cursor: pointer;
    z-index: 30;
    &:hover {
      opacity: 1.0;
    }
  }

  .track {
    position: relative;
    width: 100%;
    height: 100%;

    >.segment {
      position: absolute;
      height: 100%;
      align-items: center;
      justify-content: center;
      z-index: 20;
      opacity: @o-xl;
      cursor: pointer;

      &.selected, &.active {
        opacity: 1.0 !important;
      }
      &.selected, &:hover {
        .inner-shadow(2px, lighten(@h-color, 20%));
        //border: 2px solid lighten(@h-color, 20%);
      }
      &.ignored:not(:hover) {
        //background-color: @o-d-color !important;
        opacity: @o-xxxl;
      }

      >.ignored {
        font-size: 12px;
      }
    }
  }
}
</style>

<template>
  <div class="track-viewer">
    <div class="track" v-el:track>
      <div class="segment h-box"
        v-if="file.ads.length > 0"
        v-for="ad in filteredAds"
        :class="{ active: ad.type === activeType, ignored: ad.ignored, selected: ad === selectedAd }"
        :style="ad | segmentStyle file typeCount"
        @mouseover="mouseover(ad)"
        @mouseout="mouseout(ad)"
        @click="select(ad)">
          <i class="ignored fa fa-times" v-show="ad.ignored"></i>
      </div>
    </div>

    <span class="time">
      {{file.length | musicTime true}}
    </span>

    <i class="remove fa fa-times" @click="removeFile"></i>
  </div>
</template>

<script>
var Vue = require('vue')
var _ = require('../utils')

Vue.filter('musicTime', function(time, showMs = false) {
  return _.formatDuration(time, showMs)
})

Vue.filter('segmentStyle', function(ad, file, typeCount) {
  var style = {
    top: 0,
    bottom: 0
  }
  style.left = (ad.start / file.length) * 100 + '%'
  style.right = (1 - ad.end / file.length) * 100 + '%'
  if (typeCount > 0) {
    var h = ad.type / typeCount * 350 + 10
    var [r, g, b] = _.hsv2rgb(h, 0.8, 0.9)
    var hex = _.rgb2hex(r, g, b)
    style.backgroundColor = hex
  }
  return style
})

module.exports = {
  props: ['file', 'type-count', 'selected-ad', 'filter'],
  data: () => {
    return {
      activeType: null
    }
  },
  computed: {
    filteredAds() {
      if (!this.file || !this.file.ads || this.file.ads.length === 0) return []
      var rule = ad => {
        var ok = true
        if (this.filter && this.filter.minLength > 0) ok = ok && (ad.end - ad.start) >= this.filter.minLength * 1000
        if (this.filter && this.filter.minCount > 1) ok = ok && ad.count >= this.filter.minCount
        return ok
      }
      return this.file.ads.filter(rule)
    }
  },
  methods: {
    removeFile(e) {
      this.$dispatch('remove', this.file)
    },
    mouseover(ad) {
      this.$dispatch('ad-mouseover', ad)
    },
    mouseout(ad) {
      this.$dispatch('ad-mouseout', ad)
    },
    select(ad) {
      this.$dispatch('select-ad', this.file, ad)
      this.selectedAd = ad 
    }
  },
  events: {
    'ad-active': function(ad) {
      this.activeType = ad.type
    },
    'ad-deactive': function(ad) {
      this.activeType = null
    }
  }
}
</script>