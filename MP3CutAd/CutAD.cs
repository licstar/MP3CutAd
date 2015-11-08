using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3CutAd.Core {
    public partial class CutAD {

        const string tmpdirName = "mp3cutad";
        const bool saveTmpFile = false; //会不会输出临时文件。输出的话，第二次计算会快；不输出当然就省空间了

        /// <summary>
        /// 检测一系列MP3文件包含的广告（重复出现的部分）
        /// </summary>
        /// <param name="mp3Files"></param>
        /// <param name="notify">在需要的时候调用notify()更新进度，参数是[0, 1]的浮点数</param>
        /// <returns>对应各个MP3的广告区间</returns>
        public static List<KeyValuePair<List<Range>, int>> DetectAD(string[] mp3Files, Action<int, int> notify) {

            var tmpDir = Path.Combine(Path.GetTempPath(), tmpdirName);

            Directory.CreateDirectory(tmpDir);

            List<string> fileList = new List<string>();

            List<double[,]> ffts = new List<double[,]>();
            List<List<int>> hashs = new List<List<int>>();
            var ranges = new List<List<Range>>();

            int size = 20;
            LSH.init(size, MyFFT.len / 2);

            List<Link> links = new List<Link>();

            TimeEstimater log = new TimeEstimater();
            log.InitType("wav", mp3Files.Length);
            log.InitType("fft", mp3Files.Length);
            log.InitType("hash", mp3Files.Length);
            log.InitType("compare", mp3Files.Length * (mp3Files.Length - 1) / 2);

            foreach (var fn in mp3Files) {
                var f = new FileInfo(fn);
                if (f.Extension.ToLower() != ".mp3")
                    continue;

                Console.Write("{0}", f.Name);

                var wavFile = Path.Combine(tmpDir, f.Name + ".wav");
                var fftFile = wavFile + ".fft";

                log.StartTimer("wav");

                ///
                /// 1. 转wav
                /// 
                if (!File.Exists(wavFile) && !File.Exists(fftFile)) //wav只是为了生成fft，如果fft已经有了，就不用wav了
                    FFMpeg.Mp3toWav(f.FullName, wavFile);
                fileList.Add(wavFile);

                log.EndTimer("wav");
                notify((int)log.GetTimeUsed().TotalSeconds, (int)log.EstimateTime().TotalSeconds);
                log.StartTimer("fft");

                /// 
                /// 2. fft
                /// 
                if (!File.Exists(fftFile)) {
                    var a = MyFFT.ProcessWavArr(wavFile);
                    if (saveTmpFile)
                        WriteArrayToFile(a, fftFile);
                    ffts.Add(a);
                    File.Delete(wavFile); //生成fft之后就可以删除wav了
                } else {
                    ffts.Add(ReadArrayFromFile(fftFile));
                }

                log.EndTimer("fft");
                notify((int)log.GetTimeUsed().TotalSeconds, (int)log.EstimateTime().TotalSeconds);
                log.StartTimer("hash");

                /// 
                /// 3. hash
                /// 
                var hashFile = wavFile + ".hash";
                List<int> hash = new List<int>();
                if (!File.Exists(hashFile)) {
                    var a = ffts.Last();

                    for (int i = 0; i + size < a.GetLength(0); i++) {
                        int h = LSH.hash(a, i);
                        hash.Add(h);
                    }

                    if (saveTmpFile) {
                        using (StreamWriter sw = new StreamWriter(hashFile)) {
                            foreach (var h in hash) {
                                sw.WriteLine(h);
                            }
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

                log.EndTimer("hash");
                notify((int)log.GetTimeUsed().TotalSeconds, (int)log.EstimateTime().TotalSeconds);

                //log.Log(Console.Out, "\t{0:F1}");

                ranges.Add(new List<Range>());

                for (int i = 0; i < fileList.Count - 1; i++) {
                    int j = fileList.Count - 1;

                    log.StartTimer("compare");

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

                    log.EndTimer("compare");
                    notify((int)log.GetTimeUsed().TotalSeconds, (int)log.EstimateTime().TotalSeconds);
                }




                //存储广告位置
                //for (int i = 0; i < fileList.Count; i++) {
                //    using (StreamWriter sw = new StreamWriter(fileList[i] + ".range")) {
                //        //sw.WriteLine(ranges[i].Count);
                //        foreach (var r in ranges[i]) {
                //            sw.WriteLine("{0} {1} {2}", r.begin, r.end, r.count);
                //        }
                //    }
                //}

                //log.Log(Console.Out, "\t{0:F1}\n");

            }
            for (int j = 0; j < ranges.Count; j++) {
                ranges[j] = CompresssRange(ranges[j]);
            }
            CalcRangeTypes(ranges, links);
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


        public class CutPara {
            public List<Range> range;
            public int length;
            public CutPara(int length, List<Range> range) {
                this.range = range;
                this.length = length;
            }
        }

        public static void Cut(Dictionary<string, CutPara> files, string path, Action<int, int> notify) {
            var tmpDir = Path.Combine(Path.GetTempPath(), tmpdirName);

            Directory.CreateDirectory(tmpDir);

            DateTime start = DateTime.Now;
            int calced = 0;
            foreach (var file in files) {
                var f = new FileInfo(file.Key);

                Console.Write("{0}", f.Name);

                var wavFile = Path.Combine(tmpDir, f.Name + ".wav");
                CutAndCombine(f.FullName, Path.Combine(path, f.Name), wavFile, ReverseRange(file.Value.range, file.Value.length));

                calced++;
                var used = (DateTime.Now - start).TotalSeconds;
                var left = used / calced * (files.Count - calced);
                notify((int)used, (int)left);
            }
        }

        private static void CutAndCombine(string mp3, string output, string wav, List<Range> range) {

            string listFile = wav + ".list";
            var fileList = new List<string>();
            using (StreamWriter sw = new StreamWriter(listFile)) {
                for (int i = 0; i < range.Count; i++) {
                    var r = range[i];
                    var file = wav + i + ".mp3";
                    FFMpeg.Split(mp3, file, r.begin / 10.0, r.end / 10.0);
                    sw.WriteLine("file '{0}'", file);
                    fileList.Add(file);
                }
            }
            FFMpeg.Concat(listFile, output);

            foreach(var f in fileList) {
                File.Delete(f);
            }
            File.Delete(listFile);
        }

        //输入候选，输出时间区间
        private static List<KeyValuePair<Range, Range>> FineTune(double[,] fft1, double[,] fft2,
             List<int> hash1, List<int> hash2,
             List<Range> range1, List<Range> range2,
            int size, List<KeyValuePair<int, int>> sames) {

            int len1 = fft1.GetLength(0);
            int len2 = fft2.GetLength(0);
            var ret = new List<KeyValuePair<Range, Range>>();

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
                double threshold = 0.75;// LSH.sim(fft1, fft2, p1, p2, size2) / change - 0.01;

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


                var r1 = CreateRange(range1, len1, new Range(p1 + prev, p1 + next));
                var r2 = CreateRange(range2, len2, new Range(p2 + prev, p2 + next));
                if (r1 != null && r2 != null)
                    ret.Add(new KeyValuePair<Range, Range>(r1, r2));

            }
            return ret;
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

