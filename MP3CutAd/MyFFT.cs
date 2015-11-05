using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TgTransform;

namespace MP3CutAd {
    class MyFFT {

        public const int len = 128; //对多少个采样点做fft（记得保持 len <= step，否则会浪费）
        public const int step = 2205; //采样时间步长（22050Hz，所以2205大约0.1s）
        

        private static double[] doFFT(double[] a) {
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

        private static void normalization(double[] f) {
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

        public static Bitmap ProcessWavBmp(string file) {
            byte[] data = File.ReadAllBytes(file);
            List<double> vals = new List<double>();
            for (int i = 44; i < data.Length; i += 2) {
                int val = data[i] + data[i + 1] * 256;
                if (val >= 32768)
                    val = val - 65536;
                vals.Add((val / 32768.0));
            }

            //int len = fft_len;
            //int step = 2205; //大约0.1s


            int width = 0;
            for (int i = 0; i + len < vals.Count; i += step, width++) ;

            Bitmap bmp = new Bitmap(1000, len / 4);

            List<double> allValue = new List<double>();
            for (int i = 1000, cnt = 0; i + len < vals.Count && cnt < 1000; i += step, cnt++) {
                double[] v = new double[len];
                for (int j = 0; j < len; j++) {
                    v[j] = vals[i + j];
                }
                v = doFFT(v);
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


        public static double[,] ProcessWavArr(string file) {
            byte[] data = File.ReadAllBytes(file);
            List<double> vals = new List<double>();
            for (int i = 44; i < data.Length; i += 2) {
                int val = data[i] + data[i + 1] * 256;
                if (val >= 32768)
                    val = val - 65536;
                vals.Add((val / 32768.0));
            }
            
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
                v = doFFT(v);
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



    }
}
