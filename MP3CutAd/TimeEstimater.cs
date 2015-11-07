using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MP3CutAd.Core {
    /// <summary>
    /// 估计运行时间用的工具
    /// </summary>
    class TimeEstimater {
        Dictionary<string, TimeSpan> doneTime;
        Dictionary<string, int> totalCount;
        Dictionary<string, int> doneCount;
        Dictionary<string, DateTime> startTime;

        public TimeEstimater() {
            startTime = new Dictionary<string, DateTime>();
            doneTime = new Dictionary<string, TimeSpan>();
            totalCount = new Dictionary<string, int>();
            doneCount = new Dictionary<string, int>();
        }

        public void StartTimer(string typeId) {
            if (!totalCount.ContainsKey(typeId)) {
                throw new Exception("这个类型没有初始化");
            }
            startTime[typeId] = DateTime.Now;
        }

        public TimeSpan EndTimer(string typeId) {
            TimeSpan ts = new TimeSpan(0);
            if (!startTime.ContainsKey(typeId)) {
                throw new Exception("这个类型没有初始化");
            }
            ts = DateTime.Now - startTime[typeId];
            doneTime[typeId] += ts;
            doneCount[typeId]++;
            return ts;
        }

        //初始化一个类型的计时器
        public void InitType(string typeId, int count) {
            startTime[typeId] = DateTime.Now;
            doneTime[typeId] = new TimeSpan(0);
            totalCount[typeId] = count;
            doneCount[typeId] = 0;
        }

        //返回预计剩余时间
        public TimeSpan EstimateTime() {
            TimeSpan ret = new TimeSpan(0);
            foreach (var o in doneTime) {
                if (doneCount[o.Key] == 0) {
                    return new TimeSpan(1, 0, 0, 0, 0);
                }
                ret = ret.Add(new TimeSpan(o.Value.Ticks * (totalCount[o.Key] - doneCount[o.Key]) / doneCount[o.Key]));
            }
            return ret;
        }

        //返回已经使用的时间
        public TimeSpan GetTimeUsed() {
            TimeSpan ret = new TimeSpan(0);
            foreach (var o in doneTime) {
                ret = ret.Add(o.Value);
            }
            return ret;
        }

        //返回当前进度
        public float EstimateProgress() {
            var used = GetTimeUsed();
            var left = EstimateTime();
            return 1.0f * used.Ticks / (used.Ticks + left.Ticks);
        }

    }
}
