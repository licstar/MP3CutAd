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
            this.type = 0;
        }

        public Range(int begin, int end) {
            this.begin = begin;
            this.end = end;
            this.count = 1;
            this.type = 0;
        }

        public bool InRange(int p) {
            if (begin <= p && p <= end)
                return true;
            else
                return false;
        }
    }

    class Link {
        public int f1;
        public int f2;
        public int p1;
        public int p2;
        public Link(int f1, int f2, int p1, int p2) {
            this.f1 = f1;
            this.f2 = f2;
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
