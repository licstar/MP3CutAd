<style lang="less">
@import url(./settings.less);
@import url(./global.less);

body {
  font: 100% @font-stack;
  color: @primary-color;
  background-color: @bg-color;
}
#wrap {
  position: absolute;
  width: 100%;
  height: 100%;
  min-height: 320px;
}
#header {
  width: 100%;
  z-index: 500;
  box-shadow: 0 0 10px #000;
}
#body {
  width: 100%;
  flex: 1;
}

#sidebar {
  width: 120px;
  padding: 10px 4px 0;
  align-items: center;

  button {
    width: 100%;
    margin-bottom: 10px;
  }
}
#main {
  min-width: 360px;
  flex: 1;
  overflow-x: hidden;
  overflow-y: auto;
}

#loading {
  position: absolute;
  width: 100%;
  height: 100%;
  align-items: center;
  justify-content: center;
  background: @o-l-color;
  z-index: 999;
  
  i {
    font-size: 120px;
  }
  div {
    font-size: 24px;
  }
  .progress {
    width: 80%;

    .time {
      text-align: center;
      font-size: 14px;
    }
  }
}
</style>

<template>
  <div id="wrap" class="v-box">
    <div id="header">
      <control-panel :ad="selectedAd" :file="selectedFile"></control-panel>
    </div>
    <div id="body" class="h-box">
      <div id="sidebar" class="v-box">
          <button class="pure-button pure-button-primary" @click="openFile">
            <i class="fa fa-file-o"></i>
            <span>添加文件</span>
          </button>
          <button class="pure-button pure-button-primary" @click="openDirectory">
            <i class="fa fa-folder-open-o"></i>
            <span>添加目录</span>
          </button>
          <button class="pure-button pure-button-secondary" :disabled="selectedFiles.length === 0" @click="detectAd">
            <i class="fa fa-search"></i>
            <span>算！</span>
          </button>
          <button class="pure-button pure-button-secondary" :disabled="selectedFiles.length === 0" @click="cut">
            <i class="fa fa-scissors"></i>
            <span>干！</span>
          </button>
          <button class="pure-button pure-button-warning" @click="showDevTools">
            <i class="fa fa-cog"></i>
            <span>DevTools</span>
          </button>
      </div>
      <div id="main" class="v-box">
        <file-list
          :files="selectedFiles"
          :selected-ad.sync="selectedAd"
          @remove="removeFile"
          @select-ad="selectAd"
          :type-count="typeCount"></file-list>
      </div>
    </div>
  </div>

  <div id="loading" v-show="loading" class="v-box">
    <i class="fa fa-spinner spinner"></i>
    <div>玩命计算中</div>
    <div class="progress">
      <progress-bar :progress="loadingProgress"></progress-bar>
      <div class="time">预计剩余时间：{{timeToGo}}</div>
    </div>
  </div>
</template>

<script>
var _ = require('./utils')

module.exports = {
  data() {
    return {
      selectedFiles: [],
      typeCount: 0,
      selectedFile: null,
      selectedAd: null,
      loading: false,
      timeUsed: 0,
      timeLeft: -1
    }
  },
  methods: {
    addFile(path) {
      if (!this.selectedFiles.some(f => f.fullname === path)) {
        var item = {
          length: NaN,
          ads: []
        }
        _.extend(item, _.resolveFileName(path))

        item.length = _.rand(123456)
        this.selectedFiles.push(item)
      }
    },
    openFile() {
      bound.openFile('添加文件', '音频文件(*.mp3)|*.mp3', (err, result) => {
        result = JSON.parse(result)
        result.forEach(this.addFile)
      })
    },
    openDirectory() {
      bound.openDirectory('*.mp3', (err, result) => {
        result = JSON.parse(result)
        result.forEach(this.addFile)
      })
    },
    removeFile(file) {
      this.selectedFiles.$remove(file)
      return false
    },
    selectAd(file, ad) {
      this.selectedFile = file
      this.selectedAd = ad
    },
    detectAd() {
      this.selectAd(null, null)
      this.loading = true
      this.timeUsed = 0
      this.timeLeft = -1
      bound.detectAD(JSON.stringify(this.selectedFiles.map(f => f.fullname)),
        (err, result) => {
          this.loading = false
          result = JSON.parse(result)

          var types = [];
          result.forEach((ret, i) => {
            var file = this.selectedFiles[i]
            file.length = ret.length
            var ads = ret.ads
            ads.forEach((ad, j) => {
              var type = ad.type
              if (types.indexOf(type) === -1) {
                types.push(type)
              }
              ad.ignored = false
            })
            file.ads = ads
          })
          this.typeCount = types.length
        },
        (used, left) => {
          this.timeUsed = used
          this.timeLeft = left
          this.loadingProgress = used / (used + left)
        }
      )
    },

    cut() {
      this.timeUsed = 0
      this.timeLeft = -1
      bound.selectDirectory((err, path) => {
        path = JSON.parse(path)
        if (path) {
          this.loading = true
          bound.cut(JSON.stringify(this.selectedFiles), path,
            (err, result) => {
              alert(JSON.stringify(['cut', err, result, path], null, '  '))
              this.loading = false
            },
            (used, left) => {
              this.timeUsed = used
              this.timeLeft = left
            }
          )
        }
      })
    },

    showDevTools() {
      bound.showDevTools()
    }
  },
  computed: {
    loadingProgress() {
      if (this.timeLeft < 0) return 0
      return this.timeUsed / (this.timeUsed + this.timeLeft)
    },
    timeToGo() {
      if (this.timeLeft < 0 || this.timeLeft > 10000) return '尚不明朗'
      return _.formatDuration(this.timeLeft * 1000, false)
    }
  },
  components: {
    FileList: require('./components/file-list.vue'),
    ControlPanel: require('./components/control-panel.vue'),
    ProgressBar: require('./components/progress-bar.vue')
  }
}
</script>