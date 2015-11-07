using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3CutAd.Core {
    public class CutAD {
        public static List<KeyValuePair<List<Range>, int>> DetectAD(string[] mp3Files, Action<float> notify) {
            // TODO: 在需要的时候调用notify()更新进度，参数是[0, 1]的浮点数


            //var mp3Dir = args[0];
            //var outDir = args[1];
            var tmpDir = Path.Combine(Path.GetTempPath(), "mp3cut");

            Directory.CreateDirectory(tmpDir);

            List<string> fileList = new List<string>();

            List<double[,]> ffts = new List<double[,]>();
            List<List<int>> hashs = new List<List<int>>();
            var ranges = new List<List<Range>>();

            int size = 20;
            LSH.init(size, MyFFT.len / 2);

            List<Link> links = new List<Link>();

            foreach (var fn in mp3Files) {
                var f = new FileInfo(fn);
                if (f.Extension.ToLower() != ".mp3")
                    continue;

                Console.Write("{0}", f.Name);

                var wavFile = tmpDir + f.Name + ".wav";
                var fftFile = wavFile + ".fft";

                TimeLogger log = new TimeLogger();
                //DateTime last = DateTime.Now;

                ///
                /// 1. 转wav
                /// 
                if (!File.Exists(wavFile) && !File.Exists(fftFile)) //wav只是为了生成fft，如果fft已经有了，就不用wav了
                    FFMpeg.Mp3toWav(f.FullName, wavFile);
                fileList.Add(wavFile);

                log.Log(Console.Out, "\t{0:F1}");

                /// 
                /// 2. fft
                /// 
                if (!File.Exists(fftFile)) {
                    var a = MyFFT.ProcessWavArr(wavFile);
                    WriteArrayToFile(a, fftFile);
                    ffts.Add(a);
                    File.Delete(wavFile); //生成fft之后就可以删除wav了
                } else {
                    ffts.Add(ReadArrayFromFile(fftFile));
                }

                log.Log(Console.Out, "\t{0:F1}");


                /// 
                /// 3. hash
                /// 
                var hashFile = wavFile + ".hash";
                List<int> hash = new List<int>();
                if (!File.Exists(hashFile)) {
                    var a = ffts.Last();
                    using (StreamWriter sw = new StreamWriter(hashFile)) {
                        for (int i = 0; i + size < a.GetLength(0); i++) {
                            int h = LSH.hash(a, i);
                            sw.WriteLine(h);
                            hash.Add(h);
                        }
                    }
                } else {
                    using (StreamReader sr = new StreamReader(hashFile)) {
                        while (!sr.EndOfStream) {
                            hash.Add(int.Parse(sr.ReadLine()));
                        }
                    }
                }
                hashs.Add(hash);

                log.Log(Console.Out, "\t{0:F1}");

                ranges.Add(new List<Range>());

                for (int i = 0; i < fileList.Count - 1; i++) {
                    int j = fileList.Count - 1;
                    var ranges_i = new List<Range>();
                    var ranges_j = new List<Range>();
                    var lst = CheckSame(ffts[i], ffts[j], hashs[i], hashs[j], size);
                    var tlink = FineTune(ffts[i], ffts[j], hashs[i], hashs[j], ranges_i, ranges_j, size, lst);

                    //TODO 加上反向的看看效果会不会有变化

                    //添加等价关系
                    foreach (var link in tlink) {
                        links.Add(new Link(i, j, link.Key, link.Value));
                    }

                    ranges_i = CompresssRange(ranges_i);
                    ranges_j = CompresssRange(ranges_j);
                    ranges[i] = CombineToRanges(ranges[i], ranges_i);
                    ranges[j] = CombineToRanges(ranges[j], ranges_j);
                }

                CalcRangeTypes(ranges, links);
                //for (int j = 0; j < fileList.Count; j++) {
                //    ranges[j] = CompresssRange(ranges[j]);
                //}

                //存储广告位置
                //for (int i = 0; i < fileList.Count; i++) {
                //    using (StreamWriter sw = new StreamWriter(fileList[i] + ".range")) {
                //        //sw.WriteLine(ranges[i].Count);
                //        foreach (var r in ranges[i]) {
                //            sw.WriteLine("{0} {1} {2}", r.begin, r.end, r.count);
                //        }
                //    }
                //}

                log.Log(Console.Out, "\t{0:F1}\n");

            }

            //读取广告位置，并且报告出现次数
            //for (int i = 0; i < fileList.Count; i++) {
            //    var range_file = fileList[i] + ".range";
            //    using (StreamReader sr = new StreamReader(range_file)) {
            //        var range = new List<Range>();
            //        while (!sr.EndOfStream) {
            //            var x = sr.ReadLine().Split(' ');
            //            if (x.Length != 3) continue;
            //            range.Add(new Range(int.Parse(x[0]), int.Parse(x[1]), int.Parse(x[2])));
            //        }
            //        ranges[i] = ReverseRange(range, ffts[i].GetLength(0));
            //    }
            //}

            var ret = new List<KeyValuePair<List<Range>, int>>();
            for (int i = 0; i < ranges.Count; i++) {
                ret.Add(new KeyValuePair<List<Range>, int>(ranges[i], ffts[i].GetLength(0)));
            }
            return ret;
        }


        private static void CalcRangeTypes(List<List<Range>> ranges, List<Link> links) {
            List<Link> rangeLinks = new List<Link>();
            //

            for (int i = 0; i < ranges.Count; i++) {

            }
        }

        public static void Cut(Dictionary<string, Range[]> files, string path, Action<float> notify) {
            // TODO: 在需要的时候调用notify()更新进度，参数是[0, 1]的浮点数
        }

        // 输入字典为<文件名，被判断为广告的所有区间>
        // 第二个参数是输出目录

        public static void Play(string filename, Range range) {

        }

        //// 播放一个文件的某个区间

        //public int GetLengthOfFile(string mp3Filename) {
        //    // 音频长度毫秒数
        //}

        //// 或者它的批量版本
        //public int[] GetLengthOfFiles(string[] mp3Files) {

        //}











        static void Mainxx(string[] args) {


            ////5. 提取、拼接 正文剩下部分
            //for (int i = 0; i < fileList.Count; i++) {
            //    CutAndCombine(mp3FileList[i], outFileList[i], fileList[i], ranges[i]);
            //}


            //0.1s的精度。22.05khz

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
            range = range.FindAll(x => x.count > 1);
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

        private static void CutAndCombine(string mp3, string output, string wav, List<Range> range) {

            string listFile = wav + ".list";

            using (StreamWriter sw = new StreamWriter(listFile)) {
                for (int i = 0; i < range.Count; i++) {
                    var r = range[i];
                    var file = wav + i + ".mp3";
                    FFMpeg.Split(mp3, file, r.begin / 10.0, r.end / 10.0);
                    sw.WriteLine("file '{0}'", file);
                }
            }
            FFMpeg.Concat(listFile, output);

        }

        //输入候选，输出时间区间
        private static List<KeyValuePair<int, int>> FineTune(double[,] fft1, double[,] fft2,
             List<int> hash1, List<int> hash2,
             List<Range> range1, List<Range> range2,
            int size, List<KeyValuePair<int, int>> sames) {

            int len1 = fft1.GetLength(0);
            int len2 = fft2.GetLength(0);
            var ret = new List<KeyValuePair<int, int>>();

            foreach (var same in sames) {
                int p1 = same.Key;
                int p2 = same.Value;

                if (InRange(range1, p1) && InRange(range2, p2)) { //两个区域都已经识别过
                                                                  // Console.WriteLine(".");
                    continue;
                }

                //错位选择最佳位置
                double best = 0;
                int bp = 0;
                for (int i = Math.Max(0, p1 - 5); i < Math.Min(p1 + 5, len1 - size); i++) {
                    double s = LSH.sim(fft1, fft2, i, p2, size);
                    if (s > best) {
                        best = s;
                        bp = i;
                    }
                }
                p1 = bp;

                int step = 2;//切割精度
                int next = 0;
                int prev = 0;
                int size2 = step * 2;

                // double change = 1.08; //改变量大于这个，就认为是不同的
                double threshold = 0.82;// LSH.sim(fft1, fft2, p1, p2, size2) / change - 0.01;

                List<double> ss = new List<double>();
                for (; p1 + next + size2 < len1 && p2 + next + size2 < len2; next += step) {
                    double s = LSH.sim(fft1, fft2, p1 + next, p2 + next, size2);
                    if (s < threshold) {
                        break;
                    }
                    ss.Add(s);
                }

                for (; p1 + prev >= 0 && p2 + prev >= 0; prev -= step) {
                    double s = LSH.sim(fft1, fft2, p1 + prev, p2 + prev, size2);
                    if (s < threshold) {
                        prev += size2 - 1;
                        break;
                    }
                    ss.Add(s);
                }

                //if (ss.Count > 0)
                //    Console.WriteLine("{0} {1} {2} {3}", p1 + prev, p1 + next, next - prev, ss.Average());

                if (next - prev >= 20) {
                    CreateRange(range1, len1, new Range(p1 + prev, p1 + next));
                    CreateRange(range2, len2, new Range(p2 + prev, p2 + next));
                    ret.Add(new KeyValuePair<int, int>(p1, p2));
                }
            }
            return ret;
        }

        private static void CreateRange(List<Range> range, int len, Range add) {
            if (add.begin <= 50) add.begin = 0;
            if (add.end >= len - 50) add.end = len;
            if (add.begin >= add.end - 20) return; //至少要两秒

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

        private static List<KeyValuePair<int, int>> CheckSame(double[,] fft1, double[,] fft2, List<int> hash1, List<int> hash2, int size) {
            Dictionary<int, List<int>> pos = new Dictionary<int, List<int>>();
            for (int i = 0; i + size < fft2.GetLength(0); i++) {
                int h = hash2[i];
                if (!pos.ContainsKey(h)) {
                    pos[h] = new List<int>();
                }
                pos[h].Add(i);
            }
            //Console.WriteLine(Environment.TickCount);

            //List<int> pos = new List<int>();
            var ret = new List<KeyValuePair<int, int>>();
            for (int i = 0; i + size < fft1.GetLength(0); i += size) {
                int h = hash1[i];
                if (pos.ContainsKey(h)) {
                    double best = 0;
                    int bp = 0;
                    foreach (var p in pos[h]) {
                        double s = LSH.sim(fft1, fft2, i, p, size);
                        if (s > best) {
                            best = s;
                            bp = p;
                        }
                    }
                    if (best > 0.85)
                        ret.Add(new KeyValuePair<int, int>(i, bp));
                    //sw.WriteLine("{0}\t{1}\t{2}", i, bp, best);
                }
                //pos.Add(a);
            }
            return ret;
        }

        static void WriteArrayToFile(double[,] a, string file) {
            using (var stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new BinaryWriter(stream)) {
                writer.Write((int)a.GetLength(0));
                writer.Write((int)a.GetLength(1));
                for (int i = 0; i < a.GetLength(0); i++) {
                    for (int j = 0; j < a.GetLength(1); j++) {
                        writer.Write(a[i, j]);
                    }
                }
            }
        }

        static double[,] ReadArrayFromFile(string file) {
            double[,] a = null;
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None))
            using (var reader = new BinaryReader(stream)) {
                int n = reader.ReadInt32();
                int m = reader.ReadInt32();
                a = new double[n, m];
                for (int i = 0; i < a.GetLength(0); i++) {
                    for (int j = 0; j < a.GetLength(1); j++) {
                        a[i, j] = reader.ReadDouble();
                    }
                }
            }
            return a;
        }

    }
}
