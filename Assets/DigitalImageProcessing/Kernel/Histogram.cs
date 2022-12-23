using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using System;

namespace DIP
{
    public static partial class ImageProcessing
    {
        #region Histogram

        public static void HistogramStatistics(float[] datas, out float mean, out float sig)
        {
            // mean`s range :[0,255]
            mean = 0f; sig = 0f;
            float powerSig = 0f;
            for (int i = 0; i < datas.Length; i++)
            {
                mean += i * datas[i];
            }

            for (int i = 0; i < datas.Length; i++)
            {
                powerSig += (i - mean) * (i - mean) * datas[i];
            }

            sig = Sqrt(powerSig);

        }
        public static float[] Histogram(Texture2D tex, TextureChannel channel)
        {
            int L = 256;
            int M = tex.width, N = tex.height;
            Color[] texCol = new Color[M * N];
            texCol = tex.GetPixels(0, 0, M, N);

            int[] hist = new int[L];
            float[] histNormal = new float[L];

            int[] grayLevel = new int[M * N];
            float[] grayscale = new float[M * N];


            for (int i = 0; i < L; i++)
            {
                hist[i] = 0;
            }

            for (int i = 0; i < texCol.Length; i++)
            {
                switch ((int)channel)
                {
                    case 0:
                        grayscale[i] = texCol[i].r;
                        break;

                    case 1:
                        grayscale[i] = texCol[i].g;
                        break;

                    case 2:
                        grayscale[i] = texCol[i].b;
                        break;

                    case 3:
                        grayscale[i] = texCol[i].grayscale;
                        break;
                }
            }

            for (int i = 0; i < grayscale.Length; i++)
            {
                grayLevel[i] = (int)(grayscale[i] * (L - 1) + 0.5f);
            }

            for (int i = 0; i < grayLevel.Length; i++)
            {
                hist[grayLevel[i]] += 1;
            }

            for (int i = 0; i < hist.Length; i++)
            {
                histNormal[i] = hist[i] / (float)(M * N);
                //Debug.Log("HistNormal[" + i + "]== " + histNormal[i]);
            }

            return histNormal;
        }
        public static void Histogram(Texture2D tex, TextureChannel channel, out Texture2D histTex)
        {
            float[] hist = Histogram(tex, channel);
            float max = Max(hist);
            int[] vertical = ToInt(hist, (int)(100f / max + 0.5f));

            histTex = new Texture2D(256, 100, TextureFormat.ARGB32, false);

            Color hisColor;
            switch ((int)channel)
            {
                case 0:
                    hisColor = Color.red;
                    break;

                case 1:
                    hisColor = Color.green;
                    break;

                case 2:
                    hisColor = Color.blue;
                    break;

                case 3:
                    hisColor = Color.gray;
                    break;
                default:
                    hisColor = new Color(0.1411f, 0.1411f, 0.1411f, 1f);
                    break;
            }

            for (int m = 1; m < 256; m++)
            {
                for (int n = 0; n < 100; n++)
                {
                    histTex.SetPixel(m, n, new Color(0.1411f, 0.1411f, 0.1411f, 1f));
                }
            }

            for (int m = 1; m < 256; m++)
            {
                for (int n = 0; n < vertical[m]; n++)
                {
                    histTex.SetPixel(m, n, hisColor);
                }
            }
            histTex.Apply();
        }

        #endregion

        #region HistogramEqualization

        static int[] CDF(float[] levels, int M, int N)
        {
            int L = 256;
            float[] sum = new float[L];

            int[] res = new int[L];

            for (int i = 0; i < L; i++)
            {
                for (int j = 0; j < i + 1; j++)
                {
                    sum[i] += (L - 1) * levels[j];
                    int index = (int)(sum[i] + 0.5f);
                    res[i] = index;
                }
            }
            return res;
        }
        static int[] Match(int[] s, int[] g)
        {

            for (int i = 0; i < s.Length; i++)
            {

                if (Array.Exists(g, (x) => x.Equals(s[i])))
                {
                    int index = Array.FindIndex(g, (x) => x.Equals(s[i]));
                    s[i] = index;
                }
                else
                {
                    int[] error = new int[s.Length];
                    for (int j = 0; j < g.Length; j++)
                    {
                        int temp = Abs(s[i] - g[j]);
                        error[j] = temp;
                    }
                    int min = Min(error);
                    int indexError = Array.FindIndex(error, (x) => x == min);
                    s[i] = indexError;
                }

            }

            return s;

        }
        public static float[,] Histeq(Texture2D texture, TextureChannel channel)
        {
            int L = 256;
            int M = texture.width;
            int N = texture.height;
            int[,] textureGray = new int[M, N];

            float value = 0;
            for (int m = 0, i = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++, i++)
                {
                    //float gray = texture.GetPixel(m, n).grayscale;
                    switch ((int)channel)
                    {
                        case 0:
                            value = texture.GetPixel(m, n).r;
                            break;

                        case 1:
                            value = texture.GetPixel(m, n).g;
                            break;

                        case 2:
                            value = texture.GetPixel(m, n).b;
                            break;

                        case 3:
                            value = texture.GetPixel(m, n).grayscale;
                            break;
                    }
                    textureGray[m, n] = (int)((L - 1) * value + 0.5);
                }
            }

            int[] s = new int[L];
            s = CDF(Histogram(texture, channel), M, N);

            float[,] singleColorChannel = new float[M, N];

            for (int m = 0, i = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++, i++)
                {

                    int gl = textureGray[m, n];

                    int res = s[gl];

                    singleColorChannel[m, n] = res / 255f;
                }
            }

            return singleColorChannel;
        }
        public static float[,] Histeq(Texture2D Src, Texture2D Dst, TextureChannel channel)
        {
            int L = 256;
            int M = Src.width;
            int N = Src.height;
            int[,] textureGray = new int[M, N];
            //float gray = 0;
            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    float gray = Src.GetPixel(m, n).grayscale;
                    float r = Src.GetPixel(m, n).r;
                    float g = Src.GetPixel(m, n).g;
                    float b = Src.GetPixel(m, n).b;
                    switch ((int)channel)
                    {
                        case 0:
                            textureGray[m, n] = (int)((L - 1) * r + 0.5);
                            break;

                        case 1:
                            textureGray[m, n] = (int)((L - 1) * g + 0.5);
                            break;

                        case 2:
                            textureGray[m, n] = (int)((L - 1) * b + 0.5);
                            break;

                        case 3:
                            textureGray[m, n] = (int)((L - 1) * gray + 0.5);
                            break;
                    }
                    //textureGray[m, n] = (int)((L - 1) * gray + 0.5);
                }
            }
            int[] s = new int[L];
            s = CDF(Histogram(Src, channel), M, N);


            int M2 = Dst.width;
            int N2 = Dst.height;
            int[] Gz = new int[L];
            Gz = CDF(Histogram(Dst, channel), M2, N2);


            s = Match(s, Gz);


            float[,] singleColorChannel = new float[M, N];
            for (int m = 0, i = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++, i++)
                {
                    int gl = textureGray[m, n];
                    int res = s[gl];
                    singleColorChannel[m, n] = res / 255f;
                }
            }

            return singleColorChannel;
        }

        #endregion
    }

}
