<style lang="less" scoped>
@import url(../settings.less);

.filter-control {
  padding: 4px 8px;
  justify-content: flex-end;

  .title {
    font-weight: bold;
  }

  .rules {
    .rule {
      margin-left: 20px;
      align-items: center;
    }
    input[type=text] {
      height: 16px;
      font-size: 14px;
      margin: 0 2px;
    }

    input[type=text].number {
      text-align: right;
    }

    .minLength {
      input {
        width: 2em;
      }
    }

    .minCount {
      input {
        width: 2em;
      }
    }
  }

  .spinner {
    justify-content: center;
    margin-left: 2px;

    i {
      font-size: 10px;
      cursor: pointer;
      color: @l-color;
      &:hover {
        color: @primary-color;
      }
    }
  }
}
</style>

<template>
  <div class="filter-control h-box">
    <div class="title">过滤规则</div>
    <div class="rules h-box">
      <label class="rule minLength h-box">
        <span>最小长度</span>
        <input type="text" v-model="minLength" class="number" number debounce="500" />
        <span>秒</span>
        <span class="v-box spinner">
          <i class="fa fa-plus" @click="incMinLength"></i>
          <i class="fa fa-minus" @click="decMinLength"></i>
        </span>
      </label>

      <label class="rule minCount h-box">
        <span>至少出现</span>
        <input type="text" v-model="minCount" class="number" number debounce="500" />
        <span>次</span>
        <span class="v-box spinner">
          <i class="fa fa-plus" @click="incMinCount"></i>
          <i class="fa fa-minus" @click="decMinCount"></i>
        </span>
      </label>
    </div>
  </div>
</template>

<script>

module.exports = {
  // props: {
  //   minLength: {
  //     type: Number,
  //     default: 5,
  //     twoWay: true
  //   }
  // },
  data() {
    return {
      minLength: 5,
      minCount: 3
    }
  },
  methods: {
    incMinLength() {
      this.minLength++
    },
    decMinLength() {
      this.minLength--
      if (this.minLength < 0) this.minLength = 0
    },
    incMinCount() {
      this.minCount++
    },
    decMinCount() {
      this.minCount--
      if (this.minCount < 2) this.minCount = 2
    }
  }
}
</script>