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

  .range {
    margin-left: 8px;
    font-size: 14px;
    align-items: flex-end;
  }

  .control {
    justify-content: flex-end;
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
    margin: 0 10px;
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
      <div class="range v-box">
        <span class="time">{{rangeStart}}</span>
        <span class="time">{{rangeEnd}}</span>
      </div>
      <div class="progress">
        <progress-bar :progress="playProgress"></progress-bar>
      </div>
      <div class="control h-box">
        <span class="time h-box">{{played}} / {{duration}}</span>
        <i class="play fa fa-play" :class="[(playing ? 'fa-stop' : 'fa-play')]" @click="togglePlay"></i>
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
    rangeStart() {
      if (!this.ad) return '--:--'
      return _.formatDuration(this.ad.start, false)
    },
    rangeEnd() {
      if (!this.ad) return '--:--'
      return _.formatDuration(this.ad.end, false)
    },
    played() {
      if (!this.ad) return '--:--'
      if (!this.playing) return '0:00'
      var time = this.playTime - this.ad.start
      return _.formatDuration(time, false)
    },
    duration() {
      if (!this.ad) return '--:--'
      var time = this.ad.end - this.ad.start
      return _.formatDuration(time, false)
    },
    tagButtonText() {
      return (this.ad && this.ad.ignored) ? '确是广告' : '并非广告'
    },
    playProgress() {
      if (!this.ad || !this.file) return 0
      if (!this.playing) return 0
      if (this.playTime <= 0) return 0
      //return this.playTime / this.file.length // 绝对进度
      return (this.playTime - this.ad.start) / (this.ad.end - this.ad.start) // 相对进度
    }
  },
  created() {
    this.$watch('ad', function() {
      this.stop()
    })
  },
  methods: {
    stop() {
      clearInterval(this.playTimeer)
      this.playing = false
      this.playTime = 0
      bound.stop()
    },
    play() {
      this.playing = true
      var freq = 100
      var beginTime = _.timestamp()
      this.playTime = this.ad.start
      this.playTimeer = setInterval(() => {
        this.playTime = this.ad.start + _.timestamp() - beginTime
      }, freq)

      bound.play(this.file.fullname, this.ad.start, this.ad.end, () => {
        clearInterval(this.playTimeer)
        this.playing = false
        this.playTime = 0
      })
    },
    togglePlay() {
      if (!this.playing) {
        this.play()
      } else {
        this.stop()
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