using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using System;


namespace DIP
{
    public static partial class ImageProcessing
    {
                          

        #region Private

        static float[,] Average(float r, float c)
        {
            int m = (int)r;
            int n = (int)c;

            float[,] LPF = new float[m,n];
            for (int s = 0; s < m; s++)
            {
                for (int t = 0; t < n; t++)
                {
                    LPF[s, t] = 1f;
                }
            }
            return LPF;
        }
        static float[,] Gaussian(float r, float c, float sig )
        {
            int m = (int)r;
            int n = (int)c;

            float[,] LPF = new float[m, n];
            int midr = (m - 1) / 2;
            int midc = (n - 1) / 2;
            //float K = 1f / (2f * PI * Pow(sig, 2));
            float K = 1f;

            for (int s = 0; s < m; s++)
            {
                for (int t = 0; t < n; t++)
                {
                    float squareR = Pow(s - midr, 2) + Pow(t - midc, 2);
                    float index = -squareR / (2f * Pow(sig, 2));
                    LPF[s, t] = K * Exp(index);

                }
            }
            return LPF;
        }
        static float[,] Laplacian0(float alpha )
        {
            float a = Clamp01(alpha);
            float[,] HPF ={
                           { a / (1f + a ), (1f-a) / (1f + a), a / (1f + a) },
                           {(1f-a) / (1f + a), -4f / (1f + a), (1f-a) / (1f + a) },
                           { a / (1f + a), (1f-a) / (1f + a), a / (1f + a) }
                          };
            return HPF;
        }
        static float[,] Laplacian1()
        {
            float[,] f = new float[3, 3] { { 1f, 1f, 1f }, { 1f, -8f, 1f }, { 1f, 1f, 1f } };
            return f;
        }

        static float KernelSum(float[,] f)
        {
            int r = f.GetUpperBound(0) + 1; int c = f.GetUpperBound(1) + 1;
            float sum = 0;
            for (int s = 0; s < r; s++)
            {
                for (int t = 0; t < c; t++)
                {
                    sum += f[s, t];
                }
            }

            return sum;
        }
        static List<float[]> Sepreate(float[,] f, out float sum)
        {

            int r = f.GetUpperBound(0) + 1;
            int c = f.GetUpperBound(1) + 1;

            List<float[]> kernels = new List<float[]>();
            kernels.Clear();
            float[] horizon = new float[r];
            float[] vertical = new float[c];

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            sum = 0;

            for (int s = 0; s < r; s++)
            {
                for (int t = 0; t < c; t++)
                {
                    sum += f[s, t];
                }
            }

            for (int s = 0; s < r; s++)
            {
                horizon[s] = f[s, midc];
                //Debug.Log("horizon[" + s + "]== " + horizon[s]);
            }

            for (int t = 0; t < c; t++)
            {
                vertical[t] = f[midr, t];
                //Debug.Log("vertical[" + t + "]== " + vertical[t]);
            }

            kernels.Add(horizon); kernels.Add(vertical);
            //Debug.Log("sum = " + sum);
            return kernels;
        }

        static Texture2D Zero(Texture2D texture, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;

            int M = texture.width + r - 1; int N = texture.height + c - 1;
            Texture2D padTex = new Texture2D(M, N, TextureFormat.ARGB32, false);


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {

                    padTex.SetPixel(m, n, Color.black);
                }
            }


            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    Color col = texture.GetPixel(m, n);
                    padTex.SetPixel(m + midr, n + midc, col);
                }
            }

            padTex.Apply();
            return padTex;
        }
        static Vector4[,] Zero(Vector4[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            Vector4[,] padDatas = new Vector4[M, N];


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    padDatas[m, n] = Vector4.zero;
                }
            }


            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    Vector4 col = datas[m, n];
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            return padDatas;
        }
        static Vector2[,] Zero(Vector2[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            Vector2[,] padDatas = new Vector2[M, N];


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    padDatas[m, n] = Vector2.zero;
                }
            }


            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    var col = datas[m, n];
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            return padDatas;
        }
        static float[,] Zero(float[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            float[,] padDatas = new float[M, N];


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    padDatas[m, n] = 0;
                }
            }


            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    float col = datas[m, n];
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            return padDatas;
        }
        static int[,] Zero(int[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            int[,] padDatas = new int[M, N];


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    padDatas[m, n] = 0;
                }
            }


            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    float col = datas[m, n];
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            return padDatas;
        }

        static Texture2D Replcate(Texture2D texture, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;

            int M = texture.width + r - 1; int N = texture.height + c - 1;
            Texture2D padTex = new Texture2D(M, N, TextureFormat.ARGB32, false);

            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    Color col = texture.GetPixel(m, n);
                    padTex.SetPixel(m + midr, n + midc, col);
                }
            }


            Color leftdown_Conner = texture.GetPixel(0, 0);
            Color leftup_Conner = texture.GetPixel(0, texture.height - 1);
            Color rightdown_Conner = texture.GetPixel(texture.width - 1, 0);
            Color rightup_Conner = texture.GetPixel(texture.width - 1, texture.height - 1);


            for (int m = 0; m < midr; m++)
            {
                for (int n = 0; n < midc; n++)
                {
                    padTex.SetPixel(m, n, leftdown_Conner);
                    padTex.SetPixel(m, n + texture.height + midc, leftup_Conner);
                    padTex.SetPixel(m + texture.width + midr, n, rightdown_Conner);
                    padTex.SetPixel(m + texture.width + midr, n + texture.height + midc, rightup_Conner);
                }
            }


            for (int m = 0; m < texture.width; m++)
            {
                Color lowBoundary = texture.GetPixel(m, 0);
                Color upBoundary = texture.GetPixel(m, texture.height - 1);

                for (int i = 0; i < midc; i++)
                {
                    padTex.SetPixel(m + midr, i, lowBoundary);
                    padTex.SetPixel(m + midr, texture.height + midc + i, upBoundary);
                }

            }
            for (int n = 0; n < texture.height; n++)
            {
                Color leftBoundary = texture.GetPixel(0, n);
                Color rightBoundary = texture.GetPixel(texture.width - 1, n);
                for (int i = 0; i < midr; i++)
                {
                    padTex.SetPixel(i, n + midc, leftBoundary);
                    padTex.SetPixel(texture.width + midr + i, n + midc, rightBoundary);
                }
            }

            padTex.Apply();
            return padTex;
        }
        static Vector4[,] Replcate(Vector4[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            Vector4[,] padDatas = new Vector4[M, N];

            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            Vector4 leftDown_Conner = datas[0, 0];
            Vector4 leftUp_Conner = datas[0, oN - 1];
            Vector4 rightdown_Conner = datas[oM - 1, 0];
            Vector4 rightup_Conner = datas[oM - 1, oN - 1];


            for (int m = 0; m < midr; m++)
            {
                for (int n = 0; n < midc; n++)
                {
                    padDatas[m, n] = leftDown_Conner;
                    padDatas[m, n + oN + midc] = leftUp_Conner;
                    padDatas[m + oM + midr, n] = rightdown_Conner;
                    padDatas[m + oM + midr, n + oN + midc] = rightup_Conner;
                }
            }


            for (int m = 0; m < oM; m++)
            {
                Vector4 lowBoundary = datas[m, 0];
                Vector4 upBoundary = datas[m, oN - 1];

                for (int i = 0; i < midc; i++)
                {
                    padDatas[m + midr, i] = lowBoundary;
                    padDatas[m + midr, oN + midc + i] = upBoundary;
                }

            }
            for (int n = 0; n < oN; n++)
            {
                Vector4 leftBoundary = datas[0, n];
                Vector4 rightBoundary = datas[oM - 1, n];
                for (int i = 0; i < midr; i++)
                {
                    padDatas[i, n + midc] = leftBoundary;
                    padDatas[oM + midr + i, n + midc] = rightBoundary;
                }
            }


            return padDatas;
        }
        static Vector2[,] Replcate(Vector2[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            Vector2[,] padDatas = new Vector2[M, N];

            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            var leftDown_Conner = datas[0, 0];
            var leftUp_Conner = datas[0, oN - 1];
            var rightdown_Conner = datas[oM - 1, 0];
            var rightup_Conner = datas[oM - 1, oN - 1];


            for (int m = 0; m < midr; m++)
            {
                for (int n = 0; n < midc; n++)
                {
                    padDatas[m, n] = leftDown_Conner;
                    padDatas[m, n + oN + midc] = leftUp_Conner;
                    padDatas[m + oM + midr, n] = rightdown_Conner;
                    padDatas[m + oM + midr, n + oN + midc] = rightup_Conner;
                }
            }


            for (int m = 0; m < oM; m++)
            {
                var lowBoundary = datas[m, 0];
                var upBoundary = datas[m, oN - 1];

                for (int i = 0; i < midc; i++)
                {
                    padDatas[m + midr, i] = lowBoundary;
                    padDatas[m + midr, oN + midc + i] = upBoundary;
                }

            }
            for (int n = 0; n < oN; n++)
            {
                var leftBoundary = datas[0, n];
                var rightBoundary = datas[oM - 1, n];
                for (int i = 0; i < midr; i++)
                {
                    padDatas[i, n + midc] = leftBoundary;
                    padDatas[oM + midr + i, n + midc] = rightBoundary;
                }
            }


            return padDatas;
        }
        static float[,] Replcate(float[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            float[,] padDatas = new float[M, N];

            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            float leftDown_Conner = datas[0, 0];
            float leftUp_Conner = datas[0, oN - 1];
            float rightdown_Conner = datas[oM - 1, 0];
            float rightup_Conner = datas[oM - 1, oN - 1];


            for (int m = 0; m < midr; m++)
            {
                for (int n = 0; n < midc; n++)
                {
                    padDatas[m, n] = leftDown_Conner;
                    padDatas[m, n + oN + midc] = leftUp_Conner;
                    padDatas[m + oM + midr, n] = rightdown_Conner;
                    padDatas[m + oM + midr, n + oN + midc] = rightup_Conner;
                }
            }


            for (int m = 0; m < oM; m++)
            {
                float lowBoundary = datas[m, 0];
                float upBoundary = datas[m, oN - 1];

                for (int i = 0; i < midc; i++)
                {
                    padDatas[m + midr, i] = lowBoundary;
                    padDatas[m + midr, oN + midc + i] = upBoundary;
                }

            }
            for (int n = 0; n < oN; n++)
            {
                float leftBoundary = datas[0, n];
                float rightBoundary = datas[oM - 1, n];
                for (int i = 0; i < midr; i++)
                {
                    padDatas[i, n + midc] = leftBoundary;
                    padDatas[oM + midr + i, n + midc] = rightBoundary;
                }
            }


            return padDatas;
        }
        static int[,] Replcate(int[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            int[,] padDatas = new int[M, N];

            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            int leftDown_Conner = datas[0, 0];
            int leftUp_Conner = datas[0, oN - 1];
            int rightdown_Conner = datas[oM - 1, 0];
            int rightup_Conner = datas[oM - 1, oN - 1];

            


            for (int m = 0; m < midr; m++)
            {
                for (int n = 0; n < midc; n++)
                {
                    padDatas[m, n] = leftDown_Conner;
                    padDatas[m, n+oN +midc] = leftUp_Conner;
                    padDatas[m + oM+midr, n] = rightdown_Conner;
                    padDatas[m + oM+midr, n + oN+midc] = rightup_Conner;
                }
            }
            

            for (int m = 0; m < oM; m++)
            {
                int lowBoundary = datas[m, 0];
                int upBoundary = datas[m, oN - 1];

                for (int i = 0; i < midc; i++)
                {
                    padDatas[m + midr, i] = lowBoundary;
                    padDatas[m + midr, oN + midc + i] = upBoundary;
                }

            }
            for (int n = 0; n < oN; n++)
            {
                int leftBoundary = datas[0, n];
                int rightBoundary = datas[oM - 1, n];
                for (int i = 0; i < midr; i++)
                {
                    padDatas[i, n + midc] = leftBoundary;
                    padDatas[oM + midr + i, n + midc] = rightBoundary;
                }
            }


            return padDatas;
        }

        static Texture2D Symmetric(Texture2D texture, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;

            int M = texture.width + r - 1; int N = texture.height + c - 1;
            Texture2D padTex = new Texture2D(M, N, TextureFormat.ARGB32, false);

            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    Color col = texture.GetPixel(m, n);
                    padTex.SetPixel(m + midr, n + midc, col);
                }
            }


            for (int m = 0; m < midr; m++)
            {
                for (int n = 0; n < midc; n++)
                {

                    Color leftdownConner = texture.GetPixel(m, n);
                    padTex.SetPixel(midr - m - 1, midc - n - 1, leftdownConner);

                    Color leftupConner = texture.GetPixel(m, texture.height - 1 - n);
                    padTex.SetPixel(midr - m - 1, texture.height + midc + n, leftupConner);

                    Color rightdownConner = texture.GetPixel(texture.width - 1 - m, n);
                    padTex.SetPixel(texture.width + midr + m, midc - n - 1, rightdownConner);

                    Color rightupConner = texture.GetPixel(texture.width - 1 - m, texture.height - n - 1);
                    padTex.SetPixel(texture.width + midr + m, texture.height + midc + n, rightupConner);

                }
            }



            for (int m = 0; m < texture.width; m++)
            {
                Color[] lowBoundary = new Color[midc];
                Color[] upBoundary = new Color[midc];

                for (int i = 0; i < midc; i++)
                {
                    lowBoundary[i] = texture.GetPixel(m, i);
                    upBoundary[i] = texture.GetPixel(m, texture.height - 1 - i);
                }


                for (int i = 0; i < midc; i++)
                {
                    padTex.SetPixel(m + midr, midc - i - 1, lowBoundary[i]);
                    padTex.SetPixel(m + midr, texture.height + midc + i, upBoundary[i]);
                }

            }
            for (int n = 0; n < texture.height; n++)
            {
                Color[] leftBoundary = new Color[midr];
                Color[] rightBoundary = new Color[midr];

                for (int i = 0; i < midc; i++)
                {
                    leftBoundary[i] = texture.GetPixel(i, n);
                    rightBoundary[i] = texture.GetPixel(texture.width - 1 - i, n);
                }

                for (int i = 0; i < midr; i++)
                {
                    padTex.SetPixel(midr - i - 1, n + midc, leftBoundary[i]);
                    padTex.SetPixel(texture.width + midr + i, n + midc, rightBoundary[i]);
                }
            }

            padTex.Apply();
            return padTex;
        }
        static Vector4[,] Symmetric(Vector4[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            Vector4[,] padDatas = new Vector4[M, N];

            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            for (int m = 0; m < midr; m++)
            {
                for (int n = 0; n < midc; n++)
                {

                    Vector4 leftdownConner = datas[m, n];
                    padDatas[midr - m - 1, midc - n - 1] = leftdownConner;

                    Vector4 leftupConner = datas[m, oN - 1 - n];
                    padDatas[midr - m - 1, oN + midc + n] = leftupConner;

                    Vector4 rightdownConner = datas[oM - 1 - m, n];
                    padDatas[oM + midr + m, midc - n - 1] = rightdownConner;

                    Vector4 rightupConner = datas[oM - 1 - m, oN - n - 1];
                    padDatas[oM + midr + m, oN + midc + n] = rightupConner;

                }
            }



            for (int m = 0; m < oM; m++)
            {
                Vector4[] lowBoundary = new Vector4[midc];
                Vector4[] upBoundary = new Vector4[midc];

                for (int i = 0; i < midc; i++)
                {
                    lowBoundary[i] = datas[m, i];
                    upBoundary[i] = datas[m, oN - 1 - i];
                }


                for (int i = 0; i < midc; i++)
                {
                    padDatas[m + midr, midc - i - 1] = lowBoundary[i];
                    padDatas[m + midr, oN + midc + i] = upBoundary[i];
                }

            }
            for (int n = 0; n < oN; n++)
            {
                Vector4[] leftBoundary = new Vector4[midr];
                Vector4[] rightBoundary = new Vector4[midr];

                for (int i = 0; i < midc; i++)
                {
                    leftBoundary[i] = datas[i, n];
                    rightBoundary[i] = datas[oM - 1 - i, n];
                }

                for (int i = 0; i < midr; i++)
                {
                    padDatas[midr - i - 1, n + midc] = leftBoundary[i];
                    padDatas[oM + midr + i, n + midc] = rightBoundary[i];
                }
            }

            return padDatas;
        }
        static Vector2[,] Symmetric(Vector2[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            Vector2[,] padDatas = new Vector2[M, N];

            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            for (int m = 0; m < midr; m++)
            {
                for (int n = 0; n < midc; n++)
                {

                    var leftdownConner = datas[m, n];
                    padDatas[midr - m - 1, midc - n - 1] = leftdownConner;

                    var leftupConner = datas[m, oN - 1 - n];
                    padDatas[midr - m - 1, oN + midc + n] = leftupConner;

                    var rightdownConner = datas[oM - 1 - m, n];
                    padDatas[oM + midr + m, midc - n - 1] = rightdownConner;

                    var rightupConner = datas[oM - 1 - m, oN - n - 1];
                    padDatas[oM + midr + m, oN + midc + n] = rightupConner;

                }
            }



            for (int m = 0; m < oM; m++)
            {
                var lowBoundary = new Vector2[midc];
                var upBoundary = new Vector2[midc];

                for (int i = 0; i < midc; i++)
                {
                    lowBoundary[i] = datas[m, i];
                    upBoundary[i] = datas[m, oN - 1 - i];
                }


                for (int i = 0; i < midc; i++)
                {
                    padDatas[m + midr, midc - i - 1] = lowBoundary[i];
                    padDatas[m + midr, oN + midc + i] = upBoundary[i];
                }

            }
            for (int n = 0; n < oN; n++)
            {
                var leftBoundary = new Vector2[midr];
                var rightBoundary = new Vector2[midr];

                for (int i = 0; i < midc; i++)
                {
                    leftBoundary[i] = datas[i, n];
                    rightBoundary[i] = datas[oM - 1 - i, n];
                }

                for (int i = 0; i < midr; i++)
                {
                    padDatas[midr - i - 1, n + midc] = leftBoundary[i];
                    padDatas[oM + midr + i, n + midc] = rightBoundary[i];
                }
            }

            return padDatas;
        }
        static float[,] Symmetric(float[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            float[,] padDatas = new float [M, N];

            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            for (int m = 0; m < midr; m++)
            {
                for (int n = 0; n < midc; n++)
                {

                    float leftdownConner = datas[m, n];
                    padDatas[midr - m - 1, midc - n - 1] = leftdownConner;

                    float leftupConner = datas[m, oN - 1 - n];
                    padDatas[midr - m - 1, oN + midc + n] = leftupConner;

                    float rightdownConner = datas[oM - 1 - m, n];
                    padDatas[oM + midr + m, midc - n - 1] = rightdownConner;

                    float rightupConner = datas[oM - 1 - m, oN - n - 1];
                    padDatas[oM + midr + m, oN + midc + n] = rightupConner;

                }
            }



            for (int m = 0; m < oM; m++)
            {
                float[] lowBoundary = new float[midc];
                float[] upBoundary = new float[midc];

                for (int i = 0; i < midc; i++)
                {
                    lowBoundary[i] = datas[m, i];
                    upBoundary[i] = datas[m, oN - 1 - i];
                }


                for (int i = 0; i < midc; i++)
                {
                    padDatas[m + midr, midc - i - 1] = lowBoundary[i];
                    padDatas[m + midr, oN + midc + i] = upBoundary[i];
                }

            }
            for (int n = 0; n < oN; n++)
            {
                float[] leftBoundary = new float[midr];
                float[] rightBoundary = new float[midr];

                for (int i = 0; i < midc; i++)
                {
                    leftBoundary[i] = datas[i, n];
                    rightBoundary[i] = datas[oM - 1 - i, n];
                }

                for (int i = 0; i < midr; i++)
                {
                    padDatas[midr - i - 1, n + midc] = leftBoundary[i];
                    padDatas[oM + midr + i, n + midc] = rightBoundary[i];
                }
            }

            return padDatas;
        }
        static int[,] Symmetric(int[,] datas, int r, int c)
        {

            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int M = datas.GetUpperBound(0) + r; int N = datas.GetUpperBound(1) + c;
            int oM = datas.GetUpperBound(0) + 1; int oN = datas.GetUpperBound(1) + 1;

            int[,] padDatas = new int[M, N];

            for (int m = 0; m < oM; m++)
            {
                for (int n = 0; n < oN; n++)
                {
                    if (m + midr < M && n + midc < N)
                        padDatas[m + midr, n + midc] = datas[m, n];
                }
            }


            for (int m = 0; m < midr; m++)
            {
                for (int n = 0; n < midc; n++)
                {

                    int leftdownConner = datas[m, n];
                    padDatas[midr - m - 1, midc - n - 1] = leftdownConner;

                    int leftupConner = datas[m, oN - 1 - n];
                    padDatas[midr - m - 1, oN + midc + n] = leftupConner;

                    int rightdownConner = datas[oM - 1 - m, n];
                    padDatas[oM + midr + m, midc - n - 1] = rightdownConner;

                    int rightupConner = datas[oM - 1 - m, oN - n - 1];
                    padDatas[oM + midr + m, oN + midc + n] = rightupConner;

                }
            }



            for (int m = 0; m < oM; m++)
            {
                int[] lowBoundary = new int[midc];
                int[] upBoundary = new int[midc];

                for (int i = 0; i < midc; i++)
                {
                    lowBoundary[i] = datas[m, i];
                    upBoundary[i] = datas[m, oN - 1 - i];
                }


                for (int i = 0; i < midc; i++)
                {
                    padDatas[m + midr, midc - i - 1] = lowBoundary[i];
                    padDatas[m + midr, oN + midc + i] = upBoundary[i];
                }

            }
            for (int n = 0; n < oN; n++)
            {
                int[] leftBoundary = new int[midr];
                int[] rightBoundary = new int[midr];

                for (int i = 0; i < midc; i++)
                {
                    leftBoundary[i] = datas[i, n];
                    rightBoundary[i] = datas[oM - 1 - i, n];
                }

                for (int i = 0; i < midr; i++)
                {
                    padDatas[midr - i - 1, n + midc] = leftBoundary[i];
                    padDatas[oM + midr + i, n + midc] = rightBoundary[i];
                }
            }

            return padDatas;
        }

        static Texture2D MedianFilter(Texture2D texture, int r, int c, Boundary_Option option = Boundary_Option.symmetric)
        {
            int M = texture.width; int N = texture.height;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            Texture2D paddingTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D filterTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D outpute = new Texture2D(M, N, TextureFormat.ARGB32, false);
            //float[] f = new float[r*c];
            Color[] fColor = new Color[r * c];

            switch (option)
            {
                case Boundary_Option.zero:
                    paddingTex = Zero(texture, r, c);
                    break;
                case Boundary_Option.replicate:
                    paddingTex = Replcate(texture, r, c);
                    break;
                case Boundary_Option.symmetric:
                    paddingTex = Symmetric(texture, r, c);
                    break;
            }

            for (int m = 0; m < paddingTex.width; m++)
            {
                for (int n = 0; n < paddingTex.height; n++)
                {
                    for (int s = 0, i = 0; s < r; s++)
                    {
                        for (int t = 0; t < c; t++, i++)
                        {
                            if (m + s < paddingTex.width && n + t < paddingTex.height)
                            {
                                //f[i] = Rgb2Gray(paddingTex.GetPixel(m + s, n + t));
                                fColor[i] = paddingTex.GetPixel(m + s, n + t);
                            }


                        }
                    }

                    // Array.Sort(f);
                    //float median = f[(f.Length - 1) / 2];
                    //Color mc = new Color(median, median, median);

                    Array.Sort(fColor, (x, y) => Rgb2Gray(x).CompareTo(Rgb2Gray(y)));
                    Color medianCol = fColor[(fColor.Length - 1) / 2];

                    //Debug.Log("m =" + mc);
                    filterTex.SetPixel(m + midr, n + midc, medianCol);
                }
            }
            filterTex.Apply();

            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    Color fc = filterTex.GetPixel(m + midr, n + midc);
                    outpute.SetPixel(m, n, fc);
                }
            }

            outpute.Apply();

            return outpute;
        }
        static Texture2D MaxFilter(Texture2D texture, int r, int c, Boundary_Option option = Boundary_Option.symmetric)
        {
            int M = texture.width; int N = texture.height;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            Texture2D paddingTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D filterTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D outpute = new Texture2D(M, N, TextureFormat.ARGB32, false);
            //float[] f = new float[r*c];
            Color[] fColor = new Color[r * c];

            switch (option)
            {
                case Boundary_Option.zero:
                    paddingTex = Zero(texture, r, c);
                    break;
                case Boundary_Option.replicate:
                    paddingTex = Replcate(texture, r, c);
                    break;
                case Boundary_Option.symmetric:
                    paddingTex = Symmetric(texture, r, c);
                    break;
            }

            for (int m = 0; m < paddingTex.width; m++)
            {
                for (int n = 0; n < paddingTex.height; n++)
                {
                    for (int s = 0, i = 0; s < r; s++)
                    {
                        for (int t = 0; t < c; t++, i++)
                        {
                            if (m + s < paddingTex.width && n + t < paddingTex.height)
                            {
                                //f[i] = Rgb2Gray(paddingTex.GetPixel(m + s, n + t));
                                fColor[i] = paddingTex.GetPixel(m + s, n + t);
                            }


                        }
                    }

                    // Array.Sort(f);
                    //float median = f[(f.Length - 1) / 2];
                    //Color mc = new Color(median, median, median);

                    Array.Sort(fColor, (x, y) => Rgb2Gray(x).CompareTo(Rgb2Gray(y)));
                    Color maxCol = fColor[(fColor.Length - 1)];

                    //Debug.Log("m =" + mc);
                    filterTex.SetPixel(m + midr, n + midc, maxCol);
                }
            }
            filterTex.Apply();

            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    Color fc = filterTex.GetPixel(m + midr, n + midc);
                    outpute.SetPixel(m, n, fc);
                }
            }

            outpute.Apply();

            return outpute;
        }
        static Texture2D MinFilter(Texture2D texture, int r, int c, Boundary_Option option = Boundary_Option.symmetric)
        {
            int M = texture.width; int N = texture.height;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            Texture2D paddingTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D filterTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D outpute = new Texture2D(M, N, TextureFormat.ARGB32, false);
            //float[] f = new float[r*c];
            Color[] fColor = new Color[r * c];

            switch (option)
            {
                case Boundary_Option.zero:
                    paddingTex = Zero(texture, r, c);
                    break;
                case Boundary_Option.replicate:
                    paddingTex = Replcate(texture, r, c);
                    break;
                case Boundary_Option.symmetric:
                    paddingTex = Symmetric(texture, r, c);
                    break;
            }

            for (int m = 0; m < paddingTex.width; m++)
            {
                for (int n = 0; n < paddingTex.height; n++)
                {
                    for (int s = 0, i = 0; s < r; s++)
                    {
                        for (int t = 0; t < c; t++, i++)
                        {
                            if (m + s < paddingTex.width && n + t < paddingTex.height)
                            {
                                //f[i] = Rgb2Gray(paddingTex.GetPixel(m + s, n + t));
                                fColor[i] = paddingTex.GetPixel(m + s, n + t);
                            }


                        }
                    }

                    // Array.Sort(f);
                    //float median = f[(f.Length - 1) / 2];
                    //Color mc = new Color(median, median, median);

                    Array.Sort(fColor, (x, y) => Rgb2Gray(x).CompareTo(Rgb2Gray(y)));
                    Color minCol = fColor[0];

                    //Debug.Log("m =" + mc);
                    filterTex.SetPixel(m + midr, n + midc, minCol);
                }
            }
            filterTex.Apply();

            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    Color fc = filterTex.GetPixel(m + midr, n + midc);
                    outpute.SetPixel(m, n, fc);
                }
            }

            outpute.Apply();

            return outpute;
        }
        static Texture2D MidPointFilter(Texture2D texture, int r, int c, Boundary_Option option = Boundary_Option.symmetric)
        {
            int M = texture.width; int N = texture.height;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            Texture2D paddingTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D filterTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D outpute = new Texture2D(M, N, TextureFormat.ARGB32, false);
            //float[] f = new float[r*c];
            Color[] fColor = new Color[r * c];

            switch (option)
            {
                case Boundary_Option.zero:
                    paddingTex = Zero(texture, r, c);
                    break;
                case Boundary_Option.replicate:
                    paddingTex = Replcate(texture, r, c);
                    break;
                case Boundary_Option.symmetric:
                    paddingTex = Symmetric(texture, r, c);
                    break;
            }

            for (int m = 0; m < paddingTex.width; m++)
            {
                for (int n = 0; n < paddingTex.height; n++)
                {
                    for (int s = 0, i = 0; s < r; s++)
                    {
                        for (int t = 0; t < c; t++, i++)
                        {
                            if (m + s < paddingTex.width && n + t < paddingTex.height)
                            {
                                //f[i] = Rgb2Gray(paddingTex.GetPixel(m + s, n + t));
                                fColor[i] = paddingTex.GetPixel(m + s, n + t);
                            }


                        }
                    }

                    Array.Sort(fColor, (x, y) => Rgb2Gray(x).CompareTo(Rgb2Gray(y)));

                    Color maxCol = fColor[(fColor.Length - 1)];
                    Color minCol = fColor[0];
                    Color resC = (maxCol + minCol) / 2f;

                    if (m + midr < paddingTex.width && n + midc < paddingTex.height)
                        filterTex.SetPixel(m + midr, n + midc, resC);
                }
            }
            filterTex.Apply();

            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    Color fc = filterTex.GetPixel(m + midr, n + midc);
                    outpute.SetPixel(m, n, fc);
                }
            }

            outpute.Apply();

            return outpute;
        }
        static Texture2D AlphaTrim(Texture2D texture, int r, int c, int d, Boundary_Option option = Boundary_Option.symmetric)
        {
            int M = texture.width; int N = texture.height;
            Texture2D outpute = new Texture2D(M, N, TextureFormat.ARGB32, false);
            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            if (d <= 0 || d / 2 != RoundToInt(d / 2) || d >= r * c)
            {
                Debug.LogError("d must be a positive ,even integer, less than Neighborhood length");
            }
            else
            {
                Texture2D paddingTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
                Texture2D filterTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);

                //float[] f = new float[r*c];
                Color[] fColor = new Color[r * c];

                switch (option)
                {
                    case Boundary_Option.zero:
                        paddingTex = Zero(texture, r, c);
                        break;
                    case Boundary_Option.replicate:
                        paddingTex = Replcate(texture, r, c);
                        break;
                    case Boundary_Option.symmetric:
                        paddingTex = Symmetric(texture, r, c);
                        break;
                }

                for (int m = 0; m < paddingTex.width; m++)
                {
                    for (int n = 0; n < paddingTex.height; n++)
                    {
                        for (int s = 0, i = 0; s < r; s++)
                        {
                            for (int t = 0; t < c; t++, i++)
                            {
                                if (m + s < paddingTex.width && n + t < paddingTex.height)
                                {
                                    fColor[i] = paddingTex.GetPixel(m + s, n + t);
                                }
                            }
                        }

                        Array.Sort(fColor, (x, y) => Rgb2Gray(x).CompareTo(Rgb2Gray(y)));

                        Color resC = Color.black;

                        for (int i = 0; i < d / 2; i++)
                        {
                            fColor[i] = Color.black;
                            fColor[r * c - 1 - i] = Color.black;

                        }

                        for (int i = 0; i < fColor.Length; i++)
                        {
                            resC += 1f / (r * c - d) * fColor[i];
                        }

                        if (m + midr < paddingTex.width && n + midc < paddingTex.height)
                            filterTex.SetPixel(m + midr, n + midc, resC);
                    }
                }

                filterTex.Apply();

                for (int m = 0; m < texture.width; m++)
                {
                    for (int n = 0; n < texture.height; n++)
                    {
                        Color fc = filterTex.GetPixel(m + midr, n + midc);
                        outpute.SetPixel(m, n, fc);
                    }
                }
            }
            outpute.Apply();
            return outpute;
        }
        static Texture2D GMean(Texture2D texture, int r, int c)
        {
            int M = texture.width; int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
            float[,] w = FSpecial(KernelType.average, r, c);


            Vector4[,] outData = new Vector4[M, N];

            Vector4[,] colDates = new Vector4[M, N];
            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Color g = texture.GetPixel(m, n);
                    colDates[m, n] = new Vector4(Log(g.r), Log(g.g), Log(g.b), 1f);
                }
            }

            outData = ImFilter(colDates, w, Boundary_Option.replicate);

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Vector3 col = outData[m, n];
                    Color resCol = new Color(Pow(Exp(col.x), 1f / r / c), Pow(Exp(col.y), 1f / r / c), Pow(Exp(col.z), 1f / r / c), 1f);
                    output.SetPixel(m, n, resCol);
                }
            }

            output.Apply();
            return output;

        }
        static Texture2D HarMean(Texture2D texture, int r, int c)
        {
            int M = texture.width; int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
            float[,] w = FSpecial(KernelType.average, r, c);

            Vector4[,] outData = new Vector4[M, N];
            Vector4[,] colDates = new Vector4[M, N];


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Color g = texture.GetPixel(m, n) + new Color(Epsilon, Epsilon, Epsilon);
                    Vector4 transCol = new Vector4(1f / g.r, 1f / g.g, 1f / g.b, 1.0f);
                    colDates[m, n] = transCol;
                }
            }

            outData = ImFilter(colDates, w, Boundary_Option.replicate);

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Vector4 col = outData[m, n];
                    Color resCol = new Color(r * c / col.x, r * c / col.y, r * c / col.z, 1f);
                    output.SetPixel(m, n, resCol);
                }
            }

            output.Apply();
            return output;

        }
        static Texture2D CHarMean(Texture2D texture, int r, int c, float q)
        {
            int M = texture.width; int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
            float[,] w = FSpecial(KernelType.average, r, c);


            Vector4[,] outData0 = new Vector4[M, N];
            Vector4[,] outData1 = new Vector4[M, N];
            Vector4[,] colDates0 = new Vector4[M, N];
            Vector4[,] colDates1 = new Vector4[M, N];
            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Color g = texture.GetPixel(m, n);
                    Vector4 transCol0 = new Vector4(Pow(g.r, q + 1), Pow(g.g, q + 1), Pow(g.b, q + 1), 1f);
                    Vector4 transCol1 = new Vector4(Pow(g.r, q), Pow(g.g, q), Pow(g.b, q), 1f);
                    colDates0[m, n] = transCol0;
                    colDates1[m, n] = transCol1;
                }
            }

            outData0 = ImFilter(colDates0, w, Boundary_Option.replicate);
            outData1 = ImFilter(colDates1, w, Boundary_Option.replicate);

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Vector4 col0 = outData0[m, n];
                    Vector4 col1 = outData1[m, n];
                    Color resCol = new Color(col0.x / (col1.x + Epsilon), col0.y / (col1.y + Epsilon), col0.z / (col1.z + Epsilon), 1f);
                    output.SetPixel(m, n, resCol);
                }
            }

            output.Apply();
            return output;

        }

        #endregion

        #region Public

        public static float[,] FSpecial(KernelType filter, params float[] param)
        {

            float r = 0 ;
            float c = 0;
            float sig = 0;
            float alpha = 0;

            switch (filter)
            {
                case KernelType.average:
                    #region 1
                    if (param.Length == 0)
                    {
                        r = c = 3;
                    }

                    if (param.Length == 1)
                    {
                        
                        if (param[0] < 3)
                        {
                            Debug.LogError("min size must be 3");
                            return null;
                        }
                        else
                        {
                            r = c = param[0];
                        }
                    }

                    if (param.Length == 2)
                    {
                        r = param[0];
                        c = param[1];
                    }

                    if (param.Length > 2)
                    {
                        Debug.LogError("max param number must be 2");
                        return null;
                    }
                    #endregion
                    break;

                case KernelType.gaussian:
                    #region 2
                    if (param.Length == 0)
                    {
                        r = c = 3f;
                        sig = 0.5f;
                    }

                    if (param.Length == 1)
                    {
                        sig = param[0];
                        r = c = 6f * sig;
                    }

                    if (param.Length == 2)
                    {
                        if (param[0] < 3)
                        {
                            Debug.LogError("min size must be 3");
                            return null;
                        }
                        else
                        {
                            r = c = param[0];
                        }
                       
                        sig = param[1];
                    }

                    if (param.Length ==3)
                    {
                        if (param[0] < 3 || param[1]<3)
                        {
                            Debug.LogError("min size must be 3");
                            return null;
                        }
                        else
                        {
                            r = param[0];
                            c = param[1];
                        }
                        sig = param[2];
                    }

                    if (param.Length > 3)
                    {
                        Debug.LogError("max param number must be 3");
                        return null;
                    }
                    #endregion
                    break;

                case KernelType.laplacian:
                    #region 3
                    if (param.Length == 0)
                    {
                        alpha = 0.2f;
                    }

                    if (param.Length == 1)
                    {
                        alpha = param[0];
                    }

                    if (param.Length > 2)
                    {
                        Debug.LogError("max param number must be 1");
                    }
                    #endregion
                    break;
            }

            float[,] f = new float[(int)r, (int)c];

            switch (filter)
            {
                case KernelType.average:
                    f = Average(r, c);
                    break;

                case KernelType.gaussian:
                    f = Gaussian(r, c, sig);
                    break;

                case KernelType.laplacian:
                    f = Laplacian0(alpha);
                    break;

                case KernelType.prewitt:
                    f = new float[3, 3] { {-1f,0f,1f }, {-1f,0f,1f }, {-1f,0f,1f} };
                    break;

                case KernelType.sobel:
                    f = new float[3, 3] { { -1f, 0f, 1f }, { -2f, 0f, 2f }, { -1f, 0f, 1f } };//Vertical
                    break;
            }

            return f;
        }
        public static float[,] NormalFilter(float[,] f)
        {

            int r = f.GetUpperBound(0) + 1;
            int c = f.GetUpperBound(1) + 1;

            float sum = KernelSum(f);

            for (int s = 0; s < r; s++)
            {
                for (int t = 0; t < c; t++)
                {
                    f[s, t] /= sum;
                }
            }
            return f;
        }
        public static Texture2D ImFilter(Texture2D texture, float[,] filter, Boundary_Option option = Boundary_Option.zero)
        {

            int r = filter.GetUpperBound(0) + 1; int c = filter.GetUpperBound(1) + 1;
            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int srcM = texture.width; int srcN = texture.height;
            int M = srcM + r - 1; int N = srcN + c - 1;

            Texture2D paddingTex = new Texture2D(M, N, TextureFormat.ARGB32, false);
            Texture2D filterTex = new Texture2D(M, N, TextureFormat.ARGB32, false);
            Texture2D resTex = new Texture2D(srcM, srcN, TextureFormat.ARGB32, false);

            #region Padding

            paddingTex = PaddingDatas(texture, r, c, option);
           
            #endregion

            #region Convolution
            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {

                    Color resPixel = Color.black;

                    for (int s = -midr; s <= midc; s++)
                    {
                        for (int t = -midc; t <= midr; t++)
                        {
                            resPixel += paddingTex.GetPixel(m + s, n + t) * filter[s + midr, t + midc];
                        }
                    }

                    filterTex.SetPixel(m, n, resPixel);
                }
            }
          
            #endregion

            #region TextureClip
            for (int m = 0; m < srcM; m++)
            {
                for (int n = 0; n < srcN; n++)
                {

                    Color col = filterTex.GetPixel(m + midc, n + midr);

                    resTex.SetPixel(m, n, col);
                }
            }
            resTex.Apply();
            #endregion
            return resTex;
        }
        public static Vector4[,] ImFilter(Vector4[,] colDatas, float[,] filter, Boundary_Option option = Boundary_Option.zero)
        {

            int r = filter.GetUpperBound(0) + 1; int c = filter.GetUpperBound(1) + 1;
            int midr = (r - 1) / 2; int midc = (c - 1) / 2;
            int srcM = colDatas.GetUpperBound(0) + 1; int srcN = colDatas.GetUpperBound(1) + 1;
            int M = srcM + r - 1; int N = srcN + c - 1;

            Vector4[,] paddingDatas = new Vector4[M, N];
            Vector4[,] filterCv = new Vector4[M, N];
            Vector4[,] resCv = new Vector4[srcM, srcN];

            #region Padding
            paddingDatas = PaddingDatas(colDatas, r, c, option);
            #endregion

            #region Convolution
            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {

                    Vector4 res = Vector4.zero;

                    for (int s = -midr; s <= midc; s++)
                    {
                        for (int t = -midc; t <= midr; t++)
                        {
                            if (m + s > 0 && n + t > 0 && m + s < M && n + t < N)
                            {
                                Vector4 color = paddingDatas[m + s, n + t];
                                Vector4 cv4 = new Vector4(color.x, color.y, color.z, 1f);
                                res += cv4 * filter[s + midr, t + midc];
                            }

                        }
                    }

                    filterCv[m, n] = res;
                }
            }

            #endregion

            #region Clip
            for (int m = 0; m < srcM; m++)
            {
                for (int n = 0; n < srcN; n++)
                {

                    Vector4 res = filterCv[m + midc, n + midr];

                    resCv[m, n] = res;
                }
            }

            #endregion
            return resCv;
        }

        public static Texture2D GaussianBlur(Texture2D texture, int r, int c, float sig = 0.5f, Boundary_Option option = Boundary_Option.zero)
        {


            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            int M = texture.width; int N = texture.height;
            Texture2D paddingTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D filterTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D res = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            ////////////////////////Padding Texture
            
            paddingTex = PaddingDatas(texture, r, c, option);
            
            ///////////////////////Create Guassian Kernel
            float[,] gaussianKernel = new float[r, c];
            List<float[]> sepreatKernels = new List<float[]>();
            Color[,] horCol = new Color[paddingTex.width, paddingTex.height];
            Color[,] filterRes = new Color[paddingTex.width, paddingTex.height];

            gaussianKernel = FSpecial(KernelType.gaussian, r, c, sig);
            sepreatKernels = Sepreate(gaussianKernel, out float sum);
            //Debug.Log(sum);


            ////////////////////////Horizon 
            for (int m = 0; m < paddingTex.width; m++)
            {
                for (int n = 0; n < paddingTex.height; n++)
                {

                    for (int i = -midr; i <= midr; i++)
                                                                                                                                                                                                                                                                                          {
                        if (m + i >= 0 && m + i <= paddingTex.width - 1)
                            horCol[m, n] += paddingTex.GetPixel(m + i, n) * sepreatKernels[0][i + midr];
                    }
                }
            }
            ////////////////////////Vertical 
            for (int m = 0; m < paddingTex.width; m++)
            {
                for (int n = 0; n < paddingTex.height; n++)
                {

                    for (int i = -midc; i <= midc; i++)
                    {
                        if (n + i >= 0 && n + i <= paddingTex.height - 1)
                            filterRes[m, n] += horCol[m, n + i] * sepreatKernels[1][i + midc];
                    }
                }
            }
            //////////////////////Clip to original Size
            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    Color fc = filterRes[m + midr, n + midc] / sum;
                    res.SetPixel(m, n, fc);
                }
            }

            res.Apply();

            return res;
        }
        public static Texture2D AverageBlur(Texture2D texture, int r, int c, Boundary_Option option = Boundary_Option.zero)
        {


            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            int M = texture.width; int N = texture.height;
            Texture2D paddingTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D filterTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D res = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            ////////////////////////Padding Texture
            paddingTex = PaddingDatas(texture, r, c, option);
           
            ///////////////////////Create Guassian Kernel
            float[,] boxKernel = new float[r, c];
            List<float[]> sepreatKernels = new List<float[]>();
            Color[,] horCol = new Color[paddingTex.width, paddingTex.height];
            Color[,] filterRes = new Color[paddingTex.width, paddingTex.height];
            boxKernel = FSpecial(KernelType.average, r, c);
            sepreatKernels = Sepreate(boxKernel, out float sum);


            ////////////////////////Horizon 
            for (int m = 0; m < paddingTex.width; m++)
            {
                for (int n = 0; n < paddingTex.height; n++)
                {

                    for (int i = -midr; i <= midr; i++)
                    {
                        if (m + i >= 0 && m + i <= paddingTex.width - 1)
                            horCol[m, n] += paddingTex.GetPixel(m + i, n) * sepreatKernels[0][i + midr];
                    }
                }
            }
            ////////////////////////Vertical 
            for (int m = 0; m < paddingTex.width; m++)
            {
                for (int n = 0; n < paddingTex.height; n++)
                {

                    for (int i = -midc; i <= midc; i++)
                    {
                        if (n + i >= 0 && n + i <= paddingTex.height - 1)
                            filterRes[m, n] += horCol[m, n + i] * sepreatKernels[1][i + midc];
                    }
                }
            }
            //////////////////////Clip to original Size
            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    Color fc = filterRes[m + midr, n + midc] / sum;
                    res.SetPixel(m, n, fc);
                }
            }

            res.Apply();

            return res;
        }
        

        public static Texture2D SpFilt(Texture2D texture, FilterType type, int r, int c, params float[] param)
        {
            int M = texture.width; int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
            switch (type)
            {
                case FilterType.Arithemetic:
                    float[,] w = NormalFilter( FSpecial(KernelType.average, r, c));
                    output = ImFilter(texture, w, Boundary_Option.replicate);
                    break;

                case FilterType.Geometric:
                    output = GMean(texture, r, c);
                    break;

                case FilterType.Contraharmonic:
                    output = CHarMean(texture, r, c, param[0]);
                    break;

                case FilterType.Harmonic:
                    output = HarMean(texture, r, c);
                    break;

                case FilterType.Median:
                    output = MedianFilter(texture, r, c, Boundary_Option.symmetric);
                    break;

                case FilterType.Max:
                    output = MaxFilter(texture, r, c, Boundary_Option.symmetric);
                    break;

                case FilterType.Min:
                    output = MinFilter(texture, r, c, Boundary_Option.symmetric);
                    break;

                case FilterType.MidPoint:
                    output = MidPointFilter(texture, r, c, Boundary_Option.symmetric);
                    break;

                case FilterType.AlphaTrim:
                    output = AlphaTrim(texture, r, c, (int)param[0], Boundary_Option.symmetric);
                    break;
            }


            return output;
        }
        #endregion
    }

}


