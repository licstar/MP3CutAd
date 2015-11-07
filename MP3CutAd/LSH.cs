using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MP3CutAd.Core {
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
}
