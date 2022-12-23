using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
using static UnityEngine.Mathf;

namespace DIP
{
    public static partial class ImageProcessing
    {
        #region FourierTransform
        public static Vector2[,] Convert2Complex(Texture2D texture, bool isShift = true, bool isPadding = true)
        {
            int M = texture.width;
            int N = texture.height;

            int M_n = (int)Pow(2f, CeilToInt(Log(M, 2)));
            int N_n = (int)Pow(2f, CeilToInt(Log(N, 2)));

            int P = M_n;
            int Q = N_n;

            float[,] data = new float[M, N];

            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < M; x++)
                {
                    data[x, y] = texture.GetPixel(x, y).grayscale;
                }
            }


            Vector2[,] res;
            if (isPadding)
            {
                res = PaddingDatas(data, isShift, P, Q);
            }
            else
            {
                res = PaddingDatas(data, isShift);
            }

            return res;
        }
        static Vector2[,] PaddingDatas(float[,] f, bool isShift=false, params int[] p)
        {
            int M = f.GetUpperBound(0) + 1;
            int N = f.GetUpperBound(1) + 1;
            int n = p.Length;
            ///////////////////////////////////////////////////
            int P = 0; int Q = 0;
            if (n == 0)
            {
                P = M;
                Q = N;
            }
            else if (n == 1)
            {
                P = Q = p[0];
            }
            else if (n == 2)
            {
                P = p[0];
                Q = p[1];
            }
            else
                Debug.Log("Counts of parameters must be less than 3");

            //////////////////////////////////////////////////
            Vector2[,] res = new Vector2[P, Q];

            for (int y = 0; y < Q; y++)
            {
                for (int x = 0; x < P; x++)
                {
                    if (x < M && y < N)
                    {
                        f[x, y] = (isShift) ? f[x, y] * Pow(-1, x + y) : f[x, y];
                        res[x, y] = new Vector2(f[x, y], 0f);
                    }
                    else
                    {
                        res[x, y] = new Vector2(0, 0f);
                    }
                }
            }

            return res;
        }
        public static Texture2D ImFFT2(Vector2[,] F)
        {
            int M = F.GetUpperBound(0) + 1;
            int N = F.GetUpperBound(1) + 1;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);


            for (int v = 0; v < N; v++)
            {
                for (int u = 0; u < M; u++)
                {
                    float magnitude = Sqrt(Vector2.SqrMagnitude(F[u, v]));
                    float normal = 1f / (M * N) * magnitude;
                    Color c = new Color(normal, normal, normal);
                    output.SetPixel(u, v, N * c);
                }
            }
            output.Apply();
            return output;
        }
        /// <summary>
        /// Image for invers FFT2
        /// </summary>
        /// <param name="F"> DFT datas</param>
        /// <param name="M">original texture width</param>
        /// <param name="N">original texture height</param>
        /// <returns></returns>
        public static Texture2D ImIFFT2(Vector2[,] F, int M, int N)
        {
            int P = F.GetUpperBound(0) + 1;
            int Q = F.GetUpperBound(1) + 1;

            Color[,] colors = new Color[P, Q];
            for (int v = 0, i = 0; v < Q; v++)
            {
                for (int u = 0; u < P; u++, i++)
                {
                    //float magnitude = Sqrt(Vector2.SqrMagnitude(F[u, v]));
                    float magnitude = F[u, v].x * Pow(-1, u + v);
                    Color c = new Color(magnitude, magnitude, magnitude);
                    colors[u, v] = c;
                }
            }

            M = Min(P, M);
            N = Min(Q, N);
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);

            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < M; x++)
                {
                    output.SetPixel(x, y, colors[x, y]);
                }
            }


            output.Apply();
            return output;
        }
        public static Vector2[,] FFT2(Vector2[,] f)
        {
            int M = f.GetUpperBound(0) + 1;
            int N = f.GetUpperBound(1) + 1;


            Vector2[,] res = new Vector2[M, N];
            //Horizon
            for (int x = 0; x < M; x++)
            {

                Vector2[] cx = new Vector2[N];
                Vector2[] fftres = new Vector2[N];
                for (int y = 0; y < N; y++)
                {
                    cx[y] = f[x, y];
                }

                fftres = FFT(cx);

                for (int y = 0; y < N; y++)
                {
                    res[x, y] = fftres[y];
                }
            }
            //Vertical
            for (int y = 0; y < N; y++)
            {

                Vector2[] cy = new Vector2[M];
                Vector2[] fftres = new Vector2[M];
                for (int x = 0; x < M; x++)
                {
                    cy[x] = res[x, y];
                }

                fftres = FFT(cy);

                for (int x = 0; x < M; x++)
                {
                    res[x, y] = fftres[x];
                }
            }
            return res;
        }
        public static Vector2[,] IFFT2(Vector2[,] F)
        {
            int M = F.GetUpperBound(0) + 1;
            int N = F.GetUpperBound(1) + 1;

            for (int v = 0; v < N; v++)
            {
                for (int u = 0; u < M; u++)
                {
                    F[u, v] = F[u, v] * new Vector2(1f, -1f);
                }
            }


            Vector2[,] res = new Vector2[M, N];
            //Horizon
            for (int x = 0; x < M; x++)
            {

                Vector2[] cx = new Vector2[N];
                Vector2[] fftres = new Vector2[N];
                for (int y = 0; y < N; y++)
                {
                    cx[y] = F[x, y];
                }

                fftres = FFT(cx);

                for (int y = 0; y < N; y++)
                {
                    res[x, y] = fftres[y];
                }
            }
            //Vertical
            for (int y = 0; y < N; y++)
            {

                Vector2[] cy = new Vector2[M];
                Vector2[] fftres = new Vector2[M];
                for (int x = 0; x < M; x++)
                {
                    cy[x] = res[x, y];
                }

                fftres = FFT(cy);

                for (int x = 0; x < M; x++)
                {
                    res[x, y] = 1f / (M * N) * fftres[x];
                }
            }

            return res;
        }
        public static Vector2[] FFT(Vector2[] P)
        {
            int n = P.Length;
            if (n == 1)
                return P;

            int half = n / 2;
            float a = -2.0f * PI / n;

            Vector2[] Pe = new Vector2[half];
            Vector2[] Po = new Vector2[half];
            for (int i = 0; i < half; i++)
            {
                Pe[i] = P[2 * i];
                Po[i] = P[2 * i + 1];
            }

            Vector2[] ye = FFT(Pe);
            Vector2[] yo = FFT(Po);

            for (int i = 0; i < half; i++)
            {
                Vector2 ex = new Vector2(Cos(a * i), Sin(a * i));
                P[i] = ye[i] + Cmul(ex, yo[i]);
                P[i + half] = ye[i] - Cmul(ex, yo[i]);

            }

            return P;
        }
        public static Vector2[] IFFT(Vector2[] P)
        {
            for (int i = 0; i < P.Length; i++)
            {
                P[i] = P[i] * new Vector2(1, -1);
            }
            int n = P.Length;
            //if (n == 1)
            //    return P;

            P = FFT(P);

            for (int i = 0; i < P.Length; i++)
            {
                P[i] = P[i] * (1f / n);
            }

            return P;
        }
        public static Complex[] FFT(Complex[] P)
        {
            int n = P.Length;
            if (n == 1)
                return P;

            int half = n / 2;
            float a = -2.0f * PI / n;

            Complex[] Pe = new Complex[half];
            Complex[] Po = new Complex[half];
            for (int i = 0; i < half; i++)
            {
                Pe[i] = P[2 * i];
                Po[i] = P[2 * i + 1];
            }

            Complex[] ye = FFT(Pe);
            Complex[] yo = FFT(Po);

            for (int i = 0; i < half; i++)
            {
                Complex ex = Complex.Exp(new Complex(0, a * i));
                P[i] = ye[i] + yo[i] * ex;
                P[i + half] = ye[i] - yo[i] * ex;

            }

            return P;
        }
        public static Complex[] IFFT(Complex[] P)
        {
            for (int i = 0; i < P.Length; i++)
            {
                P[i] = Complex.Conjugate(P[i]);
            }

            int n = P.Length;
            //if (n == 1)
            //    return P;

            P = FFT(P);

            for (int i = 0; i < P.Length; i++)
            {
                P[i] = P[i] * (1f / n);
            }


            return P;
        }
        public static Vector2[,] FilterMul(Vector2[,] a, Vector2[,] b)
        {
            int P = a.GetUpperBound(0) + 1;
            int Q = b.GetUpperBound(1) + 1;
            Vector2[,] res = new Vector2[P, Q];

            if (a.Length == b.Length)
            {
                for (int x = 0; x < P; x++)
                {
                    for (int y = 0; y < Q; y++)
                    {
                        res[x, y] = Cmul(a[x, y], b[x, y]);
                    }
                }


            }
            else
            {
                Debug.LogError("Must have the same size with two parameters");
            }

            return res;
        }
        public static Vector2[,] FilterMul(Vector2[,] a, float[,] b)
        {
            int P = a.GetUpperBound(0) + 1;
            int Q = b.GetUpperBound(1) + 1;
            Vector2[,] res = new Vector2[P, Q];

            if (a.Length == b.Length)
            {
                for (int x = 0; x < P; x++)
                {
                    for (int y = 0; y < Q; y++)
                    {
                        res[x, y] = Cmul(a[x, y], new Vector2(b[x, y], 0f));
                    }
                }
            }
            else
            {
                Debug.LogError("Must have the same size with two parameters");
            }

            return res;
        }
        public static Vector2[,] ILPF_F(float percent, int P, int Q)
        {
            int min = Min(P, Q);
            int D = FloorToInt(min * Clamp01(percent));
            Vector2[,] ILPF = new Vector2[P, Q];
            for (int x = 0; x < P; x++)
            {
                for (int y = 0; y < Q; y++)
                {
                    if (Pow(x - P / 2, 2) + Pow(y - Q / 2, 2) <= Pow(D, 2))
                        ILPF[x, y] = new Vector2(1f, 0f);
                    else
                        ILPF[x, y] = new Vector2(0f, 0f);
                }
            }

            return ILPF;
        }
        public static Vector2[,] IHPF_F(float percent, int P, int Q)
        {
            int min = Min(P, Q);
            int D = FloorToInt(min * Clamp01(percent));
            Vector2[,] IHPF = new Vector2[P, Q];
            for (int x = 0; x < P; x++)
            {
                for (int y = 0; y < Q; y++)
                {
                    if (Pow(x - P / 2, 2) + Pow(y - Q / 2, 2) <= Pow(D, 2))
                        IHPF[x, y] = Vector2.zero;
                    else
                        IHPF[x, y] = new Vector2(1f, 0f);
                }
            }
            return IHPF;
        }
        public static Vector2[,] GLPF_F(float percent, int P, int Q)
        {
            int min = Min(P, Q);
            float sig = percent * min;
            Vector2[,] GLPF = new Vector2[P, Q];
            for (int x = 0; x < P; x++)
            {
                for (int y = 0; y < Q; y++)
                {
                    float dis = Pow(x - P / 2, 2) + Pow(y - Q / 2, 2);

                    GLPF[x, y] = new Vector2(Exp(-dis / (2f * sig * sig)), 0f);
                }
            }

            return GLPF;
        }
        public static Vector2[,] GHPF_F(float percent, int P, int Q)
        {
            int min = Min(P, Q);
            float sig = percent * min;
            Vector2[,] GHPF = new Vector2[P, Q];
            for (int x = 0; x < P; x++)
            {
                for (int y = 0; y < Q; y++)
                {
                    float dis = Pow(x - P / 2, 2) + Pow(y - Q / 2, 2);

                    GHPF[x, y] = new Vector2(1f, 0) - new Vector2(Exp(-dis / (2f * sig * sig)), 0f);
                }
            }

            return GHPF;
        }
        public static Vector2[,] BLPF_F(float percent, float n, int P, int Q)
        {
            int min = Min(P, Q);
            float sig = percent * min;
            Vector2[,] GLPF = new Vector2[P, Q];
            for (int x = 0; x < P; x++)
            {
                for (int y = 0; y < Q; y++)
                {
                    float dis = Sqrt(Pow(x - P / 2, 2) + Pow(y - Q / 2, 2));

                    GLPF[x, y] = new Vector2(1f / (1 + Pow(dis / sig, 2 * n)), 0f);
                }
            }

            return GLPF;
        }
        public static Vector2[,] BHPF_F(float percent, float n, int P, int Q)
        {
            int min = Min(P, Q);
            float sig = percent * min;
            Vector2[,] BHPF = new Vector2[P, Q];
            for (int x = 0; x < P; x++)
            {
                for (int y = 0; y < Q; y++)
                {
                    float dis = Sqrt(Pow(x - P / 2, 2) + Pow(y - Q / 2, 2));

                    BHPF[x, y] = new Vector2(1f / (1 + Pow(sig / dis, 2 * n)), 0f);
                }
            }

            return BHPF;
        }
        public static Vector2[,] Laplacian_F(int P, int Q)
        {
            int min = Min(P, Q);

            Vector2[,] Lap_L = new Vector2[P, Q];
            //float[] laps = new float[P * Q];
            for (int x = 0, i = 0; x < P; x++)
            {
                for (int y = 0; y < Q; y++, i++)
                {
                    float dis = Pow(x - P / 2, 2) + Pow(y - Q / 2, 2);
                    Lap_L[x, y] = new Vector2(-4f * PI * PI * dis, 0f);
                    //laps[i] = -4f * PI * PI * dis;
                }
            }

            return Lap_L;
        }
        public static Vector2[,] Laplacian_F_Normal(Vector2[,] H_mul_F)
        {
            int P = H_mul_F.GetUpperBound(0) + 1;
            int Q = H_mul_F.GetUpperBound(0) + 1;

            float[] lap_1D = new float[H_mul_F.Length];
            for (int x = 0, i = 0; x < P; x++)
            {
                for (int y = 0; y < Q; y++, i++)
                {
                    lap_1D[i] = H_mul_F[x, y].x;

                }
            }
            float max = Max(lap_1D);

            for (int x = 0; x < P; x++)
            {
                for (int y = 0; y < Q; y++)
                {
                    H_mul_F[x, y] /= max;
                }
            }

            return H_mul_F;
        }
        static Vector2 Cmul(Vector2 a, Vector2 b)
        {
            Vector2 res = Vector2.zero;

            res = new Vector2((a.x * b.x - a.y * b.y), (a.x * b.y + a.y * b.x));
            return res;
        }
        static Texture2D DFT(Texture2D texture)
        {
            int M = texture.width;
            int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);

            #region BruteForce
            //for (int v = 0; v < N; v++)
            //{
            //    for (int u = 0; u < M; u++)
            //    {
            //        Vector2 complex = Vector2.zero;
            //        for (int y = 0; y < N; y++)
            //        {
            //            for (int x = 0; x < M; x++)
            //            {
            //                float c = texture.GetPixel(x, y).grayscale * Pow(-1, x + y);
            //                float w = -2f * PI * (u * (float)x / M + v * (float)y / N);
            //                Vector2 ex = new Vector2(Cos(w), Sin(w));
            //                complex += c * ex;
            //            }
            //        }

            //        float fc = 1f / M * Sqrt(Vector2.SqrMagnitude(complex));

            //        float cc = 50f * Log(1 + Abs(fc));
            //        // Color nc = new Color(cc, cc, cc);
            //        Color nc = new Color(fc, fc, fc);
            //        Debug.Log(nc);
            //        output.SetPixel(u, v, nc);

            //    }
            //}
            #endregion


            Vector2[,] FH = new Vector2[M, N];
            float[,] shift = new float[M, N];
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < M; x++)
                {
                    shift[x, y] = texture.GetPixel(x, y).grayscale * Pow(-1, x + y);
                }
            }


            /////Horizon
            for (int x = 0; x < M; x++)
            {
                for (int v = 0; v < N; v++)
                {

                    Vector2 complex = Vector2.zero;
                    for (int y = 0; y < N; y++)
                    {

                        float w = -2.0f * PI * ((float)v * y / N);
                        Vector2 ex = new Vector2(Cos(w), Sin(w));

                        complex += shift[x, y] * ex;

                    }
                    FH[x, v] = complex;
                }
            }
            /////Vertical
            for (int v = 0; v < N; v++)
            {
                for (int u = 0; u < M; u++)
                {
                    Vector2 complex = Vector2.zero;

                    for (int x = 0; x < M; x++)
                    {

                        float w = -2f * PI * ((float)u * x / M);
                        Vector2 ex = new Vector2(Cos(w), Sin(w));


                        complex += FH[x, v] * ex;
                    }


                    float fc = (float)N / (M * N) * Sqrt(Vector2.SqrMagnitude(complex));
                    Color nc = new Color(fc, fc, fc);
                    output.SetPixel(u, v, nc);
                }
            }

            output.Apply();
            return output;
        }
        #endregion
    }

}
