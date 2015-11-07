<style lang="less">
@import url(./settings.less);
@import url(./global.less);

body {
  font: 100% @font-stack;
  color: @primary-color;
}
#wrap {
  position: absolute;
  width: 100%;
  height: 100%;
  min-height: 320px;
}
#body {
  width: 100%;
  flex: 1;
}
#footer {
  width: 100%;
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
}
</style>

<template>
  <div id="wrap" class="v-box">
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
          :group-count="groupCount"></file-list>
      </div>
    </div>
    <div id="footer">
      <control-panel :ad="selectedAd" :file="selectedFile"></control-panel>
    </div>
  </div>

  <div id="loading" v-show="loading" class="v-box">
    <i class="fa fa-spinner spinner"></i>
    <div>玩命计算中</div>
  </div>
</template>

<script>
var _ = require('./utils')

module.exports = {
  data() {
    return {
      selectedFiles: [],
      groupCount: 0,
      selectedFile: null,
      selectedAd: null,
      loading: false
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
      this.loading = true
      bound.detectAD(JSON.stringify(this.selectedFiles.map(f => f.fullname)), (err, result) => {
        this.loading = false
        result = JSON.parse(result)

        var groups = [];
        result.forEach((ret, i) => {
          var file = this.selectedFiles[i]
          file.length = ret.length
          var ads = ret.ads
          ads.forEach((ad, j) => {
            var gid = ad.gid
            if (groups.indexOf(gid) === -1) {
              groups.push(gid)
            }
            ad.ignored = false
          })
          file.ads = ads
        })
        this.groupCount = groups.length
      })
    },

    cut() {
      bound.selectDirectory((err, path) => {
        path = JSON.parse(path)
        if (path) {
          this.loading = true
          bound.cut(JSON.stringify(this.selectedFiles), path, (err, result) => {
            alert(JSON.stringify(['cut', err, result, path], null, '  '))
            this.loading = false
          })
        }
      })
    },

    showDevTools() {
      bound.showDevTools()
    }
  },
  components: {
    FileList: require('./components/file-list.vue'),
    ControlPanel: require('./components/control-panel.vue')
  }
}
</script>