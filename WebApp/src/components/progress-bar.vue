<style lang="less" scoped>
@import url(../settings.less);

.progress-bar {
  width: 100%;
  height: 16px;

  .slot {
    position: relative;
    width: 100%;
    height: 100%;
    background-color: @bg-color;
    .inner-shadow(2px, @primary-color);

    .current {
      position: absolute;
      left: 0;
      top: 0;
      height: 100%;
      background-color: fadeout(@h-color, 50%);

      transition: width 200ms;
    }
  }
}
</style>

<template>
  <div class="progress-bar">
    <div class="slot">
      <div class="current" :style="{ width: percent }"></div>
    </div>
  </div>
</template>

<script>
module.exports = {
  props: ['progress'],
  computed: {
    percent() {
      var percent = +this.progress * 100
      if (percent > 100) percent = 100
      if (percent < 0) percent = 0
      return percent + '%'
    }
  }
}
</script>