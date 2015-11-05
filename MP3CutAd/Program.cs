using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using TgTransform;

namespace MP3CutAd {





    class Program {
        static int fft_len = 128;

        static double[] MyFFT(double[] a) {
            int dataSize = a.Length;
            var fftData = new Complex[dataSize];
            for (int i = 0; i < dataSize; i++) {
                fftData[i] = new Complex(a[i], 0);
            }
            FFT fft = new FFT(dataSize);
            fft.Forward(fftData);

            double[] ret = new double[dataSize];
            for (int i = 0; i < dataSize; i++) {
                ret[i] = fftData[i].Magnitude;
            }
            return ret;
        }

        static void normalization(double[] f) {
            double min = double.MaxValue;
            double max = double.MinValue;
            for (int i = 0; i < f.Length; i++) {
                if (f[i] == 0)
                    continue;
                if (f[i] < min)
                    min = f[i];
            }
            for (int i = 0; i < f.Length; i++)
                if (f[i] == 0)
                    f[i] = min;

            for (int i = 0; i < f.Length; i++) {
                //if (f[i] == 0) f[i] = 0.0001;
                f[i] = Math.Pow(f[i], 1.0 / 3);
                //f[i] = f[i] * f[i];
                if (f[i] > max)
                    max = f[i];
                if (f[i] < min)
                    min = f[i];
            }
            //min = -11.343299590295221;
            //max = 2.8883186764638418;
            for (int i = 0; i < f.Length; i++) {
                //f[i] = (f[i] - (max + min)/2) / (max - min)*2;
                f[i] = (f[i] - min) / (max - min);
            }
        }

        static Bitmap ProcessWav(string file) {
            byte[] data = File.ReadAllBytes(file);
            List<double> vals = new List<double>();
            for (int i = 44; i < data.Length; i += 2) {
                int val = data[i] + data[i + 1] * 256;
                if (val >= 32768)
                    val = val - 65536;
                vals.Add((val / 32768.0));
            }

            int len = fft_len;
            int step = 2205; //大约0.1s


            int width = 0;
            for (int i = 0; i + len < vals.Count; i += step, width++) ;

            Bitmap bmp = new Bitmap(1000, len / 4);

            List<double> allValue = new List<double>();
            for (int i = 1000, cnt = 0; i + len < vals.Count && cnt < 1000; i += step, cnt++) {
                double[] v = new double[len];
                for (int j = 0; j < len; j++) {
                    v[j] = vals[i + j];
                }
                v = MyFFT(v);
                allValue.AddRange(v);

            }
            double[] arrAllValue = allValue.ToArray();
            normalization(arrAllValue);
            for (int i = 1000, cnt = 0, offset = 0; i + len < vals.Count && cnt < 1000; i += step, cnt++) {

                double[] v = new double[len];
                for (int j = 0; j < len; j++, offset++) {
                    v[j] = arrAllValue[offset];
                }
                for (int j = 0; j < len / 4; j++) {
                    int val = (int)(v[j] * 256);
                    if (val < 0) val = 0;
                    if (val > 255) val = 255;
                    byte t = (byte)(255 - val);
                    bmp.SetPixel(cnt, j, Color.FromArgb(255, t, t, t));//
                }
            }

            return bmp;
        }


        static double[,] ProcessWavArr(string file) {
            byte[] data = File.ReadAllBytes(file);
            List<double> vals = new List<double>();
            for (int i = 44; i < data.Length; i += 2) {
                int val = data[i] + data[i + 1] * 256;
                if (val >= 32768)
                    val = val - 65536;
                vals.Add((val / 32768.0));
            }

            int len = fft_len;
            int step = 2205; //大约0.1s

            int width = 0;
            for (int i = 0; i + len < vals.Count; i += step, width++) ;

            //Bitmap bmp = new Bitmap(width, len / 4);
            double[,] bmp = new double[width, len / 4];

            List<double> allValue = new List<double>();
            for (int i = 0, cnt = 0; i + len < vals.Count; i += step, cnt++) {
                double[] v = new double[len];
                for (int j = 0; j < len; j++) {
                    v[j] = vals[i + j];
                }
                v = MyFFT(v);
                allValue.AddRange(v);

            }
            double[] arrAllValue = allValue.ToArray();
            for (int i = 0; i < arrAllValue.Length; i++)
                arrAllValue[i] = Math.Pow(arrAllValue[i], 1.0 / 3);
            //normalization(arrAllValue);
            for (int i = 0, cnt = 0, offset = 0; i + len < vals.Count; i += step, cnt++) {
                double[] v = new double[len];
                for (int j = 0; j < len; j++, offset++) {
                    v[j] = arrAllValue[offset];
                }
                for (int j = 0; j < len / 4; j++) {
                    /*int val = (int)(v[j] * 256);
                    if (val < 0) val = 0;
                    if (val > 255) val = 255;
                    byte t = (byte)(255 - val);*/
                    bmp[cnt, j] = v[j];// Color.FromArgb(255, t, t, t));//
                }
            }

            return bmp;
        }




        static void Main(string[] args) {
            var mp3Dir = args[0];
            var outDir = args[1];
            var tmpDir = outDir + "\\tmp\\";

            Directory.CreateDirectory(tmpDir);

            List<string> fileList = new List<string>();
            List<string> mp3FileList = new List<string>();
            List<string> outFileList = new List<string>();

            //1. 转wav
            Console.WriteLine("开始转wav {0}", DateTime.Now);

            foreach (var f in new DirectoryInfo(mp3Dir).GetFiles()) {
                if (f.Extension.ToLower() == ".mp3") {
                    var outfile = tmpDir + f.Name + ".wav";
                    if (!File.Exists(outfile))
                        FFMpeg.Mp3toWav(f.FullName, outfile);
                    fileList.Add(outfile);
                    mp3FileList.Add(f.FullName);
                    outFileList.Add(outDir + "\\" + f.Name);
                }
            }
           
            //2. fft+hash
            Console.WriteLine("开始fft+hash {0}", DateTime.Now);
            int size = 50;
            LSH.init(size, fft_len / 4);

            foreach (var s in fileList) {
                var fft_file = s + ".fft";
                var hash_file = s + ".hash";
                if (File.Exists(fft_file))
                    continue;

                Console.WriteLine("{0} {1}", s, DateTime.Now);
                var a = ProcessWavArr(s);
                WriteArrayToFile(a, fft_file);

                if (File.Exists(hash_file))
                    continue;
                using (StreamWriter sw = new StreamWriter(hash_file)) {
                    for (int i = 0; i + size < a.GetLength(0); i++) {
                        int h = LSH.hash(a, i);
                        sw.WriteLine(h);
                    }
                }
            }




            //读取临时文件
            List<double[,]> ffts = new List<double[,]>();
            List<List<int>> hashs = new List<List<int>>();
            List<List<KeyValuePair<int, int>>> ranges = new List<List<KeyValuePair<int, int>>>();
            foreach (var s in fileList) {
                var fft_file = s + ".fft";
                var hash_file = s + ".hash";
                ffts.Add(ReadArrayFromFile(fft_file));

                List<int> hash = new List<int>();
                using (StreamReader sr = new StreamReader(hash_file)) {
                    while (!sr.EndOfStream) {
                        hash.Add(int.Parse(sr.ReadLine()));
                    }
                }
                hashs.Add(hash);
                ranges.Add(new List<KeyValuePair<int, int>>());
            }

            //3. 识别ads
            for (int i = 0; i < fileList.Count; i++) {
                for (int j = 0; j < fileList.Count; j++) {
                    if (i == j) continue;
                    var lst = CheckSame(ffts[i], ffts[j], hashs[i], hashs[j], size);
                    FineTune(ffts[i], ffts[j], hashs[i], hashs[j], ranges[i], ranges[j], size, lst);
                    Console.WriteLine("{0} {1} {2}", i, j, DateTime.Now);
                }
            }

            for (int j = 0; j < fileList.Count; j++) {
                ranges[j] = CompresssRange(ranges[j]);
            }

            //存储广告位置
            for (int i = 0; i < fileList.Count; i++) {
                using (StreamWriter sw = new StreamWriter(fileList[i] + ".range")) {
                    //sw.WriteLine(ranges[i].Count);
                    foreach (var r in ranges[i]) {
                        sw.WriteLine("{0} {1}", r.Key, r.Value);
                    }
                }
            }


            //读取广告位置，并且报告出现次数
            for (int i = 0; i < fileList.Count; i++) {
                var range_file = fileList[i] + ".range";
                using (StreamReader sr = new StreamReader(range_file)) {
                    var range = new List<KeyValuePair<int, int>>();
                    while (!sr.EndOfStream) {
                        var x = sr.ReadLine().Split(' ');
                        if (x.Length != 2) continue;
                        range.Add(new KeyValuePair<int, int>(int.Parse(x[0]), int.Parse(x[1])));
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

        private static List<KeyValuePair<int, int>> ReverseRange(List<KeyValuePair<int, int>> range, int len) {
            var ret = new List<KeyValuePair<int, int>>();
            range.Sort((a, b) => a.Key.CompareTo(b.Key));
            if (range.Count != 0) {
                if (range[0].Key != 0) {
                    ret.Add(new KeyValuePair<int, int>(0, range[0].Key));
                }
                for (int i = 1; i < range.Count; i++) {
                    ret.Add(new KeyValuePair<int, int>(range[i - 1].Value, range[i].Key));
                }
                if (range.Last().Value != len) {
                    ret.Add(new KeyValuePair<int, int>(range.Last().Value, len));
                }
            } else {
                ret.Add(new KeyValuePair<int, int>(0, len));
            }

            int sum = 0;
            foreach (var o in range) {
                sum += o.Value - o.Key;
            }
            Console.WriteLine("{0}\t{1}\t{2}", range.Count, sum, len);
            return ret;
        }

        private static void CutAndCombine(string mp3, string output, string wav, List<KeyValuePair<int, int>> range) {

            string listFile = wav + ".list";

            using (StreamWriter sw = new StreamWriter(listFile)) {
                for (int i = 0; i < range.Count; i++) {
                    var r = range[i];
                    var file = wav + i + ".mp3";
                    FFMpeg.Split(mp3, file, r.Key / 10.0, r.Value / 10.0);
                    sw.WriteLine("file '{0}'", file);
                }
            }
            FFMpeg.Concat(listFile, output);

        }

        //输入候选，输出时间区间
        private static void FineTune(double[,] fft1, double[,] fft2,
             List<int> hash1, List<int> hash2,
             List<KeyValuePair<int, int>> range1, List<KeyValuePair<int, int>> range2,
            int size, List<KeyValuePair<int, int>> sames) {

            int len1 = fft1.GetLength(0);
            int len2 = fft2.GetLength(0);

            foreach (var same in sames) {
                int p1 = same.Key;
                int p2 = same.Value;

                //if (InRange(range1, p1) && InRange(range2, p2)) { //两个区域都已经识别过
                //                                                  //    continue;
                //}

                //错位选择最佳位置
                double best = 0;
                int bp = 0;
                for (int i = Math.Max(0, p1 - 10); i < Math.Min(p1 + 10, len1 - size); i++) {
                    double s = LSH.sim(fft1, fft2, i, p2, size);
                    if (s > best) {
                        best = s;
                        bp = i;
                    }
                }
                p1 = bp;

                //二分的版本
                double threshold = 0.88;
                //int next = size;
                //int prev = -size;
                //for (; p1 + next + size < len1 && p2 + next + size < len2; next += size) {
                //    //记得处理结尾的情况
                //    double s = LSH.sim(fft1, fft2, p1 + next, p2 + next, size);
                //    if (s < threshold) {
                //        next = BinarySearch(fft1, fft2, p1, p2, next - size, next, size, threshold, true);
                //        break;
                //    }
                //}

                //for (; p1 + prev >= 0 && p2 + prev >= 0; prev -= size) {
                //    //记得处理结尾的情况
                //    double s = LSH.sim(fft1, fft2, p1 + prev, p2 + prev, size);
                //    if (s < threshold) {
                //        prev = BinarySearch(fft1, fft2, p1, p2, prev, prev + size, size, threshold, false);
                //        break;
                //    }
                //}

                int step = 2;//切割精度
                int next = 0;
                int prev = 0;
                int size2 = size;
                List<double> ss = new List<double>();
                for (; p1 + next + size2 < len1 && p2 + next + size2 < len2; next += step) {
                    double s = LSH.sim(fft1, fft2, p1 + next, p2 + next, size2);
                    ss.Add(s);
                    if (s < threshold) {
                        // double sum2 = ss.Sum();
                        double sum = 0;
                        int cnt = 0;
                        bool ok = false;
                        for (int i = 0; i + 1 < ss.Count; i++) {
                            sum += ss[i];
                            cnt++;
                            double v = (sum / cnt) / ss[i + 1];
                            if (v > 1.02) {
                                next = i * step;
                                ok = true;
                                break;
                            }
                        }
                        if (!ok) {
                            Console.WriteLine(s); //TODO 这里可能需要适当处理
                        }
                        break;
                    }
                }

                ss.Clear();
                for (; p1 + prev >= 0 && p2 + prev >= 0; prev -= step) {
                    double s = LSH.sim(fft1, fft2, p1 + prev, p2 + prev, size2);
                    ss.Add(s);
                    if (s < threshold) {
                        // double sum2 = ss.Sum();
                        double sum = 0;
                        int cnt = 0;
                        bool ok = false;
                        for (int i = 0; i + 1 < ss.Count; i++) {
                            sum += ss[i];
                            cnt++;
                            double v = (sum / cnt) / ss[i + 1];
                            if (v > 1.02) {
                                prev = -i * step;
                                ok = true;
                                break;
                            }
                        }
                        if (!ok) {
                            Console.WriteLine(s);
                        }
                        break;
                    }
                }

                // range1.Add(new KeyValuePair<int, int>(p1 + prev + size, p1 + next - size));
                //range2.Add(new KeyValuePair<int, int>(p2 + prev + size, p2 + next - size));
                CreateRange(range1, len1, p1 + prev, p1 + next);
                CreateRange(range2, len2, p2 + prev, p2 + next);
            }
        }

        private static void CreateRange(List<KeyValuePair<int, int>> range, int len, int begin, int end) {
            if (begin <= 50) begin = 0;
            if (end >= len - 50) end = len;

            bool cross = false;
            for (int i = 0; i < range.Count; i++) {
                var r = range[i];
                if (r.Key >= end || r.Value <= begin) //不相交
                    continue;
                cross = true;
                var r2 = new KeyValuePair<int, int>(Math.Min(r.Key, begin), Math.Max(r.Value, end));
                range[i] = r2;
            }
            if (!cross) {
                range.Add(new KeyValuePair<int, int>(begin, end));
            }
        }

        private static List<KeyValuePair<int, int>> CompresssRange(List<KeyValuePair<int, int>> range) {
            List<KeyValuePair<int, int>> ret = new List<KeyValuePair<int, int>>();
            foreach (var r in range) {
                CreateRange(ret, int.MaxValue, r.Key, r.Value);
            }
            return ret;
        }

        private static int BinarySearch(double[,] fft1, double[,] fft2, int p1, int p2,
            int begin, int end, int size, double threshold, bool next) {
            for (int i = 0; i < 5; i++) {
                int mid = (begin + end) / 2;
                double s = LSH.sim(fft1, fft2, p1 + mid, p2 + mid, size);

                if (s > threshold) {
                    if (next)
                        begin = mid;
                    else
                        end = mid;
                } else {
                    if (next)
                        end = mid;
                    else
                        begin = mid;
                }
            }
            return begin;
        }

        private static bool InRange(List<KeyValuePair<int, int>> range, int value) {
            foreach (var r in range) {
                if (value >= r.Key && value < r.Value) {
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
                    if (best > 0.95)
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
            int n = 24;
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
            run(string.Format("-ac 1 -i \"{0}\" -f wav \"{1}\"", mp3, wav));
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
