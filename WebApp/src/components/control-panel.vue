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
    margin-left: 20px;
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
      <div class="progress">
        <progress-bar :progress="playProgress"></progress-bar>
      </div>
      <div class="control h-box">
        <span class="time h-box">片段时长：{{duration}}</span>
        <i class="play fa fa-play" :class="[(playing ? 'fa-stop' : 'fa-play')]" @click="play"></i>
      </div>
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
  data() {
    return {
      playing: false,
      playTime: 0,
    }
  },
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
    },
    playProgress() {
      if (!this.ad || !this.file) return 0
      if (!this.playing) return 0
      if (this.playTime <= 0) return 0
      return this.playTime / this.file.length
    }
  },
  methods: {
    play() {
      if (!this.playing) {
        this.playing = true
        var freq = 200
        var beginTime = _.timestamp()
        this.playTime = this.ad.start
        this.playingTimer = setInterval(() => {
          this.playTime = this.ad.start + _.timestamp() - beginTime
        }, freq)

        bound.play(this.file.fullname, this.ad.start, this.ad.end, () => {
          clearInterval(this.playTimeer)
          this.playing = false
          this.playTime = 0
        })
      } else {
        clearInterval(this.playTimeer)
        this.playing = false
        this.playTime = 0
        bound.stop()
      }
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