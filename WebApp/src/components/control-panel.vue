<style lang="less" scoped>
@import url(../settings.less);

.control-panel {
  position: relative;
  height: 45px;
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
    justify-content: flex-end;
    flex: 1;
    padding-right: 4px;

    .time {
      align-items: center;
      margin-right: 8px;
    }

    i {
      font-size: 30px;
      margin: 0 4px;
      cursor: pointer;
      // color: @l-color;
      &:hover {
        color: @d-color;
      }
    }
  }
  .progress {
    flex: 1;
  }
}
.ad-control {
  padding-right: 4px;
  align-items: center;
}
</style>

<template>
  <div class="control-panel h-box">
    <div class="player h-box">
      <div class="control h-box">
        <span class="time h-box">片段时长：{{duration}}</span>
        <i class="play fa fa-play" @click="play"></i>
      </div>
      <!-- <div class="progress">
        <progress-bar :progress="0.5"></progress-bar>
      </div> -->
    </div>

    <div class="ad-control h-box">
      <button class="pure-button pure-button-warning" v-show="ad" @click="tagAd">{{tagButtonText}}</button>
    </div>

    <div class="mask" v-show="disable"></div>
  </div>
</template>

<script>
var _ = require('../utils')

module.exports = {
  props: ['ad', 'file'],
  computed: {
    disable() {
      return !(this.ad && this.file)
    },
    duration() {
      if (!this.ad) return '--:---'
      var time = this.ad.end - this.ad.start
      return _.formatDuration(time, false)
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
      console.log(this.file.fullname, this.ad.start, this.ad.end)
      var audio = new Audio

      audio.src = 'file:///' + this.file.fullname.replace(/\\/g, '/')
      audio.play()
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