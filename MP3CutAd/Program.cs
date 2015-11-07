using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
//using TgTransform;

namespace MP3CutAd.Core {

    class TimeLogger {
        private DateTime last;
        public TimeLogger() {
            last = DateTime.Now;
        }
        public void Log(TextWriter sw, string format) {
            sw.Write(format, (DateTime.Now - last).TotalSeconds);
            last = DateTime.Now;
        }
    }

    class Program {
        //static int fft_len = 128;

        static void Main(string[] args) {
            var mp3Dir = args[0];
            var outDir = args[1];
            var tmpDir = outDir + "\\tmp\\";

            Directory.CreateDirectory(tmpDir);

            List<string> fileList = new List<string>();
            List<string> mp3FileList = new List<string>();
            List<string> outFileList = new List<string>();


            List<double[,]> ffts = new List<double[,]>();
            List<List<int>> hashs = new List<List<int>>();
            var ranges = new List<List<RangeType>>();

            int size = 20;
            LSH.init(size, MyFFT.len / 2);
            foreach (var f in new DirectoryInfo(mp3Dir).GetFiles()) {
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
                mp3FileList.Add(f.FullName);
                outFileList.Add(outDir + "\\" + f.Name);

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

                ranges.Add(new List<RangeType>());

                for (int i = 0; i < fileList.Count - 1; i++) {
                    int j = fileList.Count - 1;
                    var ranges_i = new List<RangeType>();
                    var ranges_j = new List<RangeType>();
                    var lst = CheckSame(ffts[i], ffts[j], hashs[i], hashs[j], size);
                    FineTune(ffts[i], ffts[j], hashs[i], hashs[j], ranges_i, ranges_j, size, lst);

                    //TODO 加上反向的看看效果会不会有变化

                    ranges_i = CompresssRange(ranges_i);
                    ranges_j = CompresssRange(ranges_j);
                    ranges[i] = CombineToRanges(ranges[i], ranges_i);
                    ranges[j] = CombineToRanges(ranges[j], ranges_j);
                }
                //for (int j = 0; j < fileList.Count; j++) {
                //    ranges[j] = CompresssRange(ranges[j]);
                //}

                //存储广告位置
                for (int i = 0; i < fileList.Count; i++) {
                    using (StreamWriter sw = new StreamWriter(fileList[i] + ".range")) {
                        //sw.WriteLine(ranges[i].Count);
                        foreach (var r in ranges[i]) {
                            sw.WriteLine("{0} {1} {2}", r.begin, r.end, r.count);
                        }
                    }
                }

                log.Log(Console.Out, "\t{0:F1}\n");

            }

            //2. fft+hash
            //Console.WriteLine("开始fft+hash {0}", DateTime.Now);
            //
            //

            //var a1 = MyFFT.ProcessWavArr(fileList[0]);
            //var a2 = MyFFT.ProcessWavArr(fileList[1]);
            //using (StreamWriter sw = new StreamWriter(tmpDir + "1.txt")) {
            //    sw.WriteLine("00\t1\t{0}", LSH.hash(a2, 14400));
            //    for (int i = 0; i + size < a1.GetLength(0); i++) {
            //        sw.WriteLine("{0}\t{1}\t{2}", i, LSH.sim(a1, a2, i, 14400, size), LSH.hash(a1, i));
            //    }
            //}

            //return;



            //读取临时文件


            //3. 识别ads
            //for (int i = 0; i < fileList.Count; i++) {
            //    for (int j = 0; j < fileList.Count; j++) {
            //        if (i == j) continue;
            //        var lst = CheckSame(ffts[i], ffts[j], hashs[i], hashs[j], size);
            //        FineTune(ffts[i], ffts[j], hashs[i], hashs[j], ranges[i], ranges[j], size, lst);
            //        Console.WriteLine("{0} {1} {2}", i, j, DateTime.Now);
            //    }
            //}

            //for (int j = 0; j < fileList.Count; j++) {
            //    ranges[j] = CompresssRange(ranges[j]);
            //}

            ////存储广告位置
            //for (int i = 0; i < fileList.Count; i++) {
            //    using (StreamWriter sw = new StreamWriter(fileList[i] + ".range")) {
            //        //sw.WriteLine(ranges[i].Count);
            //        foreach (var r in ranges[i]) {
            //            sw.WriteLine("{0} {1}", r.begin, r.end);
            //        }
            //    }
            //}


            //读取广告位置，并且报告出现次数
            for (int i = 0; i < fileList.Count; i++) {
                var range_file = fileList[i] + ".range";
                using (StreamReader sr = new StreamReader(range_file)) {
                    var range = new List<RangeType>();
                    while (!sr.EndOfStream) {
                        var x = sr.ReadLine().Split(' ');
                        if (x.Length != 3) continue;
                        range.Add(new RangeType(int.Parse(x[0]), int.Parse(x[1]), int.Parse(x[2])));
                    }
                    ranges[i] = ReverseRange(range, ffts[i].GetLength(0));
                }
            }

            //5. 提取、拼接 正文剩下部分
            for (int i = 0; i < fileList.Count; i++) {
                CutAndCombine(mp3FileList[i], outFileList[i], fileList[i], ranges[i]);
            }


            //0.1s的精度。22.05khz

        }

        class RangeType {
            public int begin;
            public int end;
            public int count;

            public RangeType(int begin, int end, int count) {
                this.begin = begin;
                this.end = end;
                this.count = count;
            }

            public RangeType(int begin, int end) {
                this.begin = begin;
                this.end = end;
                this.count = 1;
            }
        }

        private static List<RangeType> CombineToRanges(List<RangeType> range, List<RangeType> add) {
            for (int i = 0; i < add.Count; i++) {
                add[i].count = 1;
            }
            range.AddRange(add);
            return CompresssRange(range);
        }

        private static List<RangeType> ReverseRange(List<RangeType> range, int len) {
            var ret = new List<RangeType>();
            range = range.FindAll(x => x.count > 1);
            range.Sort((a, b) => a.begin.CompareTo(b.begin));
            if (range.Count != 0) {
                if (range[0].begin != 0) {
                    ret.Add(new RangeType(0, range[0].begin));
                }
                for (int i = 1; i < range.Count; i++) {
                    ret.Add(new RangeType(range[i - 1].end, range[i].begin));
                }
                if (range.Last().end != len) {
                    ret.Add(new RangeType(range.Last().end, len));
                }
            } else {
                ret.Add(new RangeType(0, len));
            }

            int sum = 0;
            foreach (var o in range) {
                sum += o.end - o.begin;
            }
            Console.WriteLine("{0}\t{1}\t{2}", range.Count, sum, len);
            return ret;
        }

        private static void CutAndCombine(string mp3, string output, string wav, List<RangeType> range) {

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
        private static void FineTune(double[,] fft1, double[,] fft2,
             List<int> hash1, List<int> hash2,
             List<RangeType> range1, List<RangeType> range2,
            int size, List<KeyValuePair<int, int>> sames) {

            int len1 = fft1.GetLength(0);
            int len2 = fft2.GetLength(0);

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
                CreateRange(range1, len1, new RangeType(p1 + prev, p1 + next));
                CreateRange(range2, len2, new RangeType(p2 + prev, p2 + next));
            }
        }

        private static void CreateRange(List<RangeType> range, int len, RangeType add) {
            if (add.begin <= 50) add.begin = 0;
            if (add.end >= len - 50) add.end = len;
            if (add.begin >= add.end - 20) return; //至少要两秒

            bool cross = false;
            for (int i = 0; i < range.Count; i++) {
                var r = range[i];
                if (r.begin > add.end || r.end < add.begin) //不相交
                    continue;
                cross = true;
                var r2 = new RangeType(Math.Min(r.begin, add.begin), Math.Max(r.end, add.end), add.count + r.count);
                range[i] = r2;
            }
            if (!cross) {
                range.Add(add);
            }
        }

        private static List<RangeType> CompresssRange(List<RangeType> range) {
            var ret = new List<RangeType>();
            foreach (var r in range) {
                CreateRange(ret, int.MaxValue, r);
            }
            return ret;
        }

        private static bool InRange(List<RangeType> range, int value) {
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

    class LSH {
        public static double sim(double[,] a, double[,] b, int offsetA, int offsetB, int size) {
            double f = 0, na = 0, nb = 0;

            for (int i = offsetA, j = offsetB; i < offsetA + size; i++, j++) {
                for (int k = 0; k < a.GetLength(1); k++) {
                    f += a[i, k] * b[j, k];
                    na += a[i, k] * a[i, k];
                    nb += b[j, k] * b[j, k];
                }
            }
            return f / Math.Sqrt(na * nb + 1e-8);
        }

        static List<double[,]> vectors = null;

        public static void init(int s1, int s2) { //初始化随机向量
            Random r = new Random(1);
            int n = 12;
            vectors = new List<double[,]>();
            for (int id = 0; id < n; id++) {
                var vector = new double[s1, s2];
                for (int i = 0; i < s1; i++) {
                    for (int j = 0; j < s2; j++) {
                        vector[i, j] = r.NextDouble() - 0.5;
                    }
                }
                vectors.Add(vector);
            }
        }

        public static int hash(double[,] a, int offsetA) {
            int ret = 0;
            for (int i = 0; i < vectors.Count; i++) {
                double v = sim(a, vectors[i], offsetA, 0, vectors[i].GetLength(0));
                ret <<= 1;
                if (v <= 0) {
                    ret |= 1;
                }
            }
            return ret;
        }
    }

    class FFMpeg {
        private static string path = @"D:\tools\ffmpeg-20151101-git-dee7440-win64-static\bin\ffmpeg";

        private static void run(string argument) {
            Process p = new Process();
            p.StartInfo.FileName = path;
            p.StartInfo.Arguments = argument;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();

            p.StandardError.ReadToEnd();
            p.StandardOutput.ReadToEnd();
            p.WaitForExit();
        }

        public static void Mp3toWav(string mp3, string wav) {
            run(string.Format("-i \"{0}\" -ac 1 -ar 22050 -f wav \"{1}\"", mp3, wav));
        }

        public static void Split(string mp3, string output, double from, double to) {
            run(string.Format("-i \"{0}\" -c copy -ss {2} -to {3} \"{1}\"",
                mp3, output, from, to));
        }

        public static void Concat(string list, string output) {
            run(string.Format("-f concat -i \"{0}\" -c copy \"{1}\"", list, output));
        }
    }
}
