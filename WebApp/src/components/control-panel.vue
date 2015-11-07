<style lang="less" scoped>
@import url(../settings.less);

.control-panel {
  position: relative;
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
  padding-left: 10px;
  align-items: center;
  flex: 1;

  i {
    margin: 0 10px;
    cursor: pointer;
  }
}
.ad-control {
}
</style>

<template>
  <div class="control-panel h-box">
    <div class="player h-box">
      <i class="play fa fa-play" @click="play"></i>
      <div class="progress-bar">你就当我是进度条吧</div>
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
  }
}
</script>