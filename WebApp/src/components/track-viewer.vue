<style lang="less" scoped>
@import url(../settings.less);

.track-viewer {
  @control-size: 32px;
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
      z-index: 20;
      opacity: @o-xl;
      cursor: pointer;
      &.selected, &.active {
        opacity: 1.0;
      }
      &.ignored:not(:hover) {
        //background-color: @o-d-color !important;
        opacity: @o-xxxl;
      }

      .control {
        visibility: hidden;
        position: absolute;
        top: 50%;
        left: 50%;
        width: @control-size * 2;
        height: @control-size;
        margin-top: -@control-size / 2;
        margin-left: -@control-size;
        text-align: center;
        z-index: 100;

        >i {
          font-size: @control-size;
          cursor: pointer;
          color: @l-color;
          &:hover {
            color: @d-color;
          }
        }
      }
      &:hover .control, .control.active {
        visibility: visible;
      }
    }
  }
}
</style>

<template>
  <div class="track-viewer">
    <div class="track" v-el:track>
      <div class="segment"
        v-if="file.ads.length > 0"
        v-for="ad in file.ads"
        :class="{ active: ad.gid === activeGroup, ignored: ad.ignored, selected: ad === selectedAd }"
        :style="ad | segmentStyle file groupCount"
        @mouseover="mouseover(ad)"
        @mouseout="mouseout(ad)"
        @click="select(ad)">
        <!-- <div class="control">
          <i class="play fa fa-play" @click="playSegment(ad)"></i>
          <i class="remove fa" :class="[(ad.ignored ? 'fa-check' : 'fa-times')]" @click="tagAd(ad)"></i>
        </div> -->
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
  if (isNaN(time)) {
    return '--:--'
  }
  var ms = time % 1000
  time = _.floor(time / 1000)
  var s = time % 60
  time = _.floor(time / 60)
  var m = time % 60
  time = _.floor(time / 60)
  var h = time
  var text = ''
  if (h > 0) text += h + ':'
  if (!showMs || m > 0) text += (text ? _.padding(m, 2, '0') : m) + ':'
  text += (text ? _.padding(s, 2, '0') : s)
  if (showMs) text += ':' + _.padding(ms, 3, '0')

  return text
})

Vue.filter('segmentStyle', function(ad, file, groupCount) {
  var style = {
    top: 0,
    bottom: 0
  }
  style.left = (ad.start / file.length) * 100 + '%'
  style.right = (1 - ad.end / file.length) * 100 + '%'
  if (groupCount > 0) {
    var h = ad.gid / groupCount * 350 + 10
    var [r, g, b] = _.hsv2rgb(h, 0.8, 0.9)
    var hex = _.rgb2hex(r, g, b)
    style.backgroundColor = hex
  }
  return style
})

module.exports = {
  props: ['file', 'group-count', 'selected-ad'],
  data: () => {
    return {
      activeGroup: null
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
    },
    tagAd(ad) {
      ad.ignored = !ad.ignored
    },
    playSegment(seg) {
      console.log('playSegment', seg)
    }
  },
  events: {
    'ad-active': function(ad) {
      this.activeGroup = ad.gid
    },
    'ad-deactive': function(ad) {
      this.activeGroup = null
    }
  }
}
</script>