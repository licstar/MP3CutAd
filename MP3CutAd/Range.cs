using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3CutAd {
    class Range {
        public int begin;
        public int end;
        public int count;

        public Range(int begin, int end, int count) {
            this.begin = begin;
            this.end = end;
            this.count = count;
        }

        public Range(int begin, int end) {
            this.begin = begin;
            this.end = end;
            this.count = 1;
        }
    }
}
