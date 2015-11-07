using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3CutAd.Core {
    public class Range {
        public int begin;
        public int end;
        public int count; //通过另外的方式计数
        public int type;

        public Range(int begin, int end, int count) {
            this.begin = begin;
            this.end = end;
            this.count = count;
            this.type = -1;
        }

        public Range(int begin, int end) {
            this.begin = begin;
            this.end = end;
            this.count = 1;
            this.type = -1;
        }

        public bool InRange(int p) {
            if (begin <= p && p <= end)
                return true;
            else
                return false;
        }
        public bool InRange(Range r) {
            if (begin <= r.begin && r.end <= end)
                return true;
            else
                return false;
        }
    }

    class Link {
        public int f1;
        public int f2;
        public int target;
        public Range r1;
        public Range r2;
        public Link(int f1, int f2, Range p1, Range p2) {
            this.f1 = f1;
            this.f2 = f2;
            this.r1 = p1;
            this.r2 = p2;
        }
        public Link(int f2, Range p2) {
            this.f1 = -1;
            this.f2 = f2;
            this.r1 = new Range(0, 0);
            this.r2 = p2;
        }
        public Link(int f2, int target) {
            this.f1 = -1;
            this.f2 = f2;
            this.r1 = new Range(0, 0);
            this.r2 = new Range(0, 0);
            this.target = target;
        }
    }


    partial class CutAD {

        private static int SetRangeType(List<List<Range>> ranges, List<Link>[][] rangeLinks, int i, int j, int typeId) {
            ranges[i][j].type = typeId;
            int ret = 1;
            foreach (var link in rangeLinks[i][j]) {
                if (ranges[link.f2][link.target].type == -1)
                    ret += SetRangeType(ranges, rangeLinks, link.f2, link.target, typeId);
            }
            return ret;
        }

        private static void CalcRangeTypes(List<List<Range>> ranges, List<Link> links) {

            List<Link>[][] rangeLinks = new List<Link>[ranges.Count][]; //储存link的图
            //rangeLinks[i][j]对应ranges[i][j]，表示这个区间和那些别的区间有联系

            for (int i = 0; i < rangeLinks.Length; i++) {
                rangeLinks[i] = new List<Link>[ranges[i].Count];
                for (int j = 0; j < rangeLinks[i].Length; j++) {
                    rangeLinks[i][j] = new List<Link>();
                }
            }

            foreach (var link in links) {
                //把link从时间刻度对应到range上
                int i1 = ranges[link.f1].FindIndex(r => r.InRange(link.r1));
                int i2 = ranges[link.f2].FindIndex(r => r.InRange(link.r2));

                //图中添加边
                var ri1 = ranges[link.f1][i1];
                var ri2 = ranges[link.f2][i2];
                if ((ri1.end - ri1.begin) < (link.r1.end - link.r1.begin) * 2 &&
                    (ri2.end - ri2.begin) < (link.r2.end - link.r2.begin) * 2) {
                    rangeLinks[link.f1][i1].Add(new Link(link.f2, i2));
                    rangeLinks[link.f2][i2].Add(new Link(link.f1, i1));
                }

            }

            int typeId = 0;
            List<int> counts = new List<int>();
            for (int i = 0; i < ranges.Count; i++) {
                for (int j = 0; j < ranges[i].Count; j++) {
                    var r = ranges[i][j];
                    if (r.type == -1) {
                        counts.Add(SetRangeType(ranges, rangeLinks, i, j, typeId));
                        typeId++;
                    }
                }
            }

            //可以加入统计一下每个type都有几个区间，如果只有1个的话，直接删掉
            for (int i = 0; i < ranges.Count; i++) {
                var good = new List<Range>();
                for (int j = 0; j < ranges[i].Count; j++) {
                    var r = ranges[i][j];
                    if (counts[r.type] > 1) {
                        r.count = counts[r.type];
                        good.Add(r);
                    }
                }
                ranges[i] = good;
            }
        }



        private static List<Range> CombineToRanges(List<Range> range, List<Range> add) {
            for (int i = 0; i < add.Count; i++) {
                add[i].count = 1;
            }
            range.AddRange(add);
            return CompresssRange(range);
        }

        private static List<Range> ReverseRange(List<Range> range, int len) {
            var ret = new List<Range>();
            range.Sort((a, b) => a.begin.CompareTo(b.begin));
            if (range.Count != 0) {
                if (range[0].begin != 0) {
                    ret.Add(new Range(0, range[0].begin));
                }
                for (int i = 1; i < range.Count; i++) {
                    ret.Add(new Range(range[i - 1].end, range[i].begin));
                }
                if (range.Last().end != len) {
                    ret.Add(new Range(range.Last().end, len));
                }
            } else {
                ret.Add(new Range(0, len));
            }

            int sum = 0;
            foreach (var o in range) {
                sum += o.end - o.begin;
            }
            Console.WriteLine("{0}\t{1}\t{2}", range.Count, sum, len);
            return ret;
        }


        //返回一个在区间内的点
        private static Range CreateRange(List<Range> range, int len, Range add) {
            if (add.begin <= 50) add.begin = 0;
            if (add.end >= len - 50) add.end = len;
            if (add.begin >= add.end - 20) return null; //至少要两秒

            bool cross = false;
            for (int i = 0; i < range.Count; i++) {
                var r = range[i];
                if (r.begin > add.end || r.end < add.begin) //不相交
                    continue;
                cross = true;
                var r2 = new Range(Math.Min(r.begin, add.begin), Math.Max(r.end, add.end), add.count + r.count);
                range[i] = r2;
            }
            if (!cross) {
                range.Add(add);
            }
            return add;
        }

        private static List<Range> CompresssRange(List<Range> range) {
            var ret = new List<Range>();
            foreach (var r in range) {
                CreateRange(ret, int.MaxValue, r);
            }
            return ret;
        }

        private static bool InRange(List<Range> range, int value) {
            foreach (var r in range) {
                if (value >= r.begin && value < r.end) {
                    return true;
                }
            }
            return false;
        }
    }

}
