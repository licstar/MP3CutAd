<style lang="less" scoped>
@import url(../settings.less);

.control-panel {
  position: relative;
  height: 40px;
}
.mask {
  position: absolute;
  width: 100%;
  height: 100%;
  top: 0;
  left: 0;
  background: @o-l-color;
}
.player {
  align-items: center;
  flex: 1;

  .control {
    padding-left: 4px;
    i {
      margin: 0 4px;
      cursor: pointer;
    }
  }
  .progress {
    flex: 1;
  }
}
.ad-control {
  padding-right: 4px;
}
</style>

<template>
  <div class="control-panel h-box">
    <div class="player h-box">
      <div class="control h-box">
        <i class="play fa fa-play" @click="play"></i>
      </div>
      <div class="progress">
        <!-- <progress-bar :progress="0.5"></progress-bar> -->
      </div>
    </div>

    <div class="ad-control">
      <button class="pure-button pure-button-warning" v-show="ad" @click="tagAd">{{tagButtonText}}</button>
    </div>

    <div class="mask" v-show="disable"></div>
  </div>
</template>

<script>

module.exports = {
  props: ['ad', 'file'],
  computed: {
    disable() {
      return !(this.ad && this.file)
    },
    tagButtonText() {
      if (!this.ad) return ''
      return this.ad.ignored ? '这是广告' : '不是广告'
    }
  },
  methods: {
    play() {
      alert('播放：' + JSON.stringify({
        fullname: this.file.fullname,
        start: this.ad.start,
        end: this.ad.end
      }, null, '  '))
    },
    tagAd() {
      this.ad.ignored = !this.ad.ignored
    }
  },
  components: {
    ProgressBar: require('./progress-bar.vue')
  }
}
</script>