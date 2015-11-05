using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace MP3CutAd {

    /// <summary>
    /// 快速傅立叶变换(Fast Fourier Transform)。 
    /// </summary>
    public class TWFFT {
        private TWFFT() {
        }
        private static void bitrp(float[] xreal, float[] ximag, int n) {
            // 位反转置换 Bit-reversal Permutation
            int i, j, a, b, p;
            for (i = 1, p = 0; i < n; i *= 2) {
                p++;
            }
            for (i = 0; i < n; i++) {
                a = i;
                b = 0;
                for (j = 0; j < p; j++) {
                    b = b * 2 + a % 2;
                    a = a / 2;
                }
                if (b > i) {
                    float t = xreal[i];
                    xreal[i] = xreal[b];
                    xreal[b] = t;
                    t = ximag[i];
                    ximag[i] = ximag[b];
                    ximag[b] = t;
                }
            }
        }
        public static int FFT(float[] xreal, float[] ximag) {
            //n值为2的N次方
            int n = 2;
            while (n <= xreal.Length) {
                n *= 2;
            }
            n /= 2;
            // 快速傅立叶变换，将复数 x 变换后仍保存在 x 中，xreal, ximag 分别是 x 的实部和虚部
            float[] wreal = new float[n / 2];
            float[] wimag = new float[n / 2];
            float treal, timag, ureal, uimag, arg;
            int m, k, j, t, index1, index2;
            bitrp(xreal, ximag, n);
            // 计算 1 的前 n / 2 个 n 次方根的共轭复数 W'j = wreal [j] + i * wimag [j] , j = 0, 1, ... , n / 2 - 1
            arg = (float)(-2 * Math.PI / n);
            treal = (float)Math.Cos(arg);
            timag = (float)Math.Sin(arg);
            wreal[0] = 1.0f;
            wimag[0] = 0.0f;
            for (j = 1; j < n / 2; j++) {
                wreal[j] = wreal[j - 1] * treal - wimag[j - 1] * timag;
                wimag[j] = wreal[j - 1] * timag + wimag[j - 1] * treal;
            }
            for (m = 2; m <= n; m *= 2) {
                for (k = 0; k < n; k += m) {
                    for (j = 0; j < m / 2; j++) {
                        index1 = k + j;
                        index2 = index1 + m / 2;
                        t = n * j / m;    // 旋转因子 w 的实部在 wreal [] 中的下标为 t
                        treal = wreal[t] * xreal[index2] - wimag[t] * ximag[index2];
                        timag = wreal[t] * ximag[index2] + wimag[t] * xreal[index2];
                        ureal = xreal[index1];
                        uimag = ximag[index1];
                        xreal[index1] = ureal + treal;
                        ximag[index1] = uimag + timag;
                        xreal[index2] = ureal - treal;
                        ximag[index2] = uimag - timag;
                    }
                }
            }
            return n;
        }
        public static int IFFT(float[] xreal, float[] ximag) {
            //n值为2的N次方
            int n = 2;
            while (n <= xreal.Length) {
                n *= 2;
            }
            n /= 2;
            // 快速傅立叶逆变换
            float[] wreal = new float[n / 2];
            float[] wimag = new float[n / 2];
            float treal, timag, ureal, uimag, arg;
            int m, k, j, t, index1, index2;
            bitrp(xreal, ximag, n);
            // 计算 1 的前 n / 2 个 n 次方根 Wj = wreal [j] + i * wimag [j] , j = 0, 1, ... , n / 2 - 1
            arg = (float)(2 * Math.PI / n);
            treal = (float)(Math.Cos(arg));
            timag = (float)(Math.Sin(arg));
            wreal[0] = 1.0f;
            wimag[0] = 0.0f;
            for (j = 1; j < n / 2; j++) {
                wreal[j] = wreal[j - 1] * treal - wimag[j - 1] * timag;
                wimag[j] = wreal[j - 1] * timag + wimag[j - 1] * treal;
            }
            for (m = 2; m <= n; m *= 2) {
                for (k = 0; k < n; k += m) {
                    for (j = 0; j < m / 2; j++) {
                        index1 = k + j;
                        index2 = index1 + m / 2;
                        t = n * j / m;    // 旋转因子 w 的实部在 wreal [] 中的下标为 t
                        treal = wreal[t] * xreal[index2] - wimag[t] * ximag[index2];
                        timag = wreal[t] * ximag[index2] + wimag[t] * xreal[index2];
                        ureal = xreal[index1];
                        uimag = ximag[index1];
                        xreal[index1] = ureal + treal;
                        ximag[index1] = uimag + timag;
                        xreal[index2] = ureal - treal;
                        ximag[index2] = uimag - timag;
                    }
                }
            }
            for (j = 0; j < n; j++) {
                xreal[j] /= n;
                ximag[j] /= n;
            }
            return n;
        }
    }
}
