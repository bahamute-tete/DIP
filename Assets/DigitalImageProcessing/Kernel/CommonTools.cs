using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEngine.Mathf;
using Random = UnityEngine.Random;

namespace DIP
{
    public static partial class ImageProcessing
    {
        #region Common Tools
        public static float[,] GetTexturePixel(Texture2D texture, TextureChannel channel)
        {
            int M = texture.width;
            int N = texture.height;

            float[,] singleColorChannel = new float[M, N];
            float value = 0;
            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
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
                    //float value = texture.GetPixel(m, n).grayscale;

                    singleColorChannel[m, n] = value;
                }
            }
            return singleColorChannel;
        }
        public static Texture2D GetTexturePixel(Texture2D texture, TextureChannel channel, bool isTex)
        {
            int M = texture.width;
            int N = texture.height;
            Texture2D outpute = new Texture2D(M, N, TextureFormat.ARGB32, false);


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    switch ((int)channel)
                    {
                        case 0:
                            float red = texture.GetPixel(m, n).r;
                            Color colr = new Color(red, red, red);
                            outpute.SetPixel(m, n, colr);
                            break;

                        case 1:
                            float green = texture.GetPixel(m, n).g;
                            Color colg = new Color(green, green, green);
                            outpute.SetPixel(m, n, colg);
                            break;

                        case 2:
                            float blue = texture.GetPixel(m, n).b;
                            Color colb = new Color(blue, blue, blue);
                            outpute.SetPixel(m, n, colb);
                            break;

                        case 3:
                            float gray = texture.GetPixel(m, n).grayscale;
                            Color colgray = new Color(gray, gray, gray);
                            outpute.SetPixel(m, n, colgray);
                            break;
                    }
                }
            }

            outpute.Apply();
            return outpute;
        }
        public static Texture2D SetPixelPos(List<Vector2> pos, Color col)
        {
            Vector2 starPos = pos[0];
            Vector2 massCenter = Vector2.zero;


            float maxD = 0;
            for (int i = 0; i < pos.Count; i++)
            {
                massCenter += pos[i] / pos.Count;
                float sqrD = Vector2.SqrMagnitude(pos[i] - massCenter);
                if (sqrD > maxD)
                {
                    maxD = sqrD;
                }
            }

            int M = CeilToInt(Sqrt(maxD));
            Texture2D output = new Texture2D(M, M, TextureFormat.RGB24, false);
            output = InitialColor(output, Color.black);
            for (int i = 0; i < pos.Count; i++)
            {
                output.SetPixel((int)pos[i].x, (int)pos[i].y, col);
            }

            output.Apply();
            return output;
        }

        public static Vector2[,] GetNeighborhoodPos(int x, int y, int size)
        {
            int M = 2 * size + 1;

            int half = size;

            Vector2[,] nhPos = new Vector2[M, M];

            for (int t = -half; t <= half; t++)
            {

                for (int s = -half; s <= half; s++)
                {
                    int tempx = x + s;
                    int tempy = y + t;
                    nhPos[tempx, tempy] = new Vector2(tempx, tempy);
                }
            }

            return nhPos;
        }

        public static void Imshow(Texture2D outpute, float[,] channelR, float[,] channelG, float[,] channleB)
        {
            int M = outpute.width;
            int N = outpute.height;

            for (int m = 0, i = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++, i++)
                {

                    Color color = new Color(channelR[m, n], channelG[m, n], channleB[m, n], 1f);
                    outpute.SetPixel(m, n, color);
                }
            }

            outpute.Apply();
        }
        public static Texture2D Im2bw(Texture2D texture, float threshold)
        {
            int M = texture.width;
            int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);

            if (threshold >= 0f && threshold <= 1f)
            {
                Color[] thresholdCol = texture.GetPixels(0, 0, M, N);

                for (int i = 0; i < thresholdCol.Length; i++)
                {
                    if (thresholdCol[i].grayscale > threshold)
                        thresholdCol[i] = Color.white;
                    else
                        thresholdCol[i] = Color.black;
                }

                output.SetPixels(0, 0, M, N, thresholdCol);

            }
            else
            {
                Debug.LogError("Threshold Value must be on range[0,1]");
            }

            output.Apply();

            return output;
        }
        public static int[] ToInt(float[] f, int L)
        {
            int[] r = new int[f.Length];
            for (int i = 0; i < f.Length; i++)
            {
                r[i] = (int)(f[i] * L + 0.5f);
            }
            return r;
        }

        public static List<Vector2> ConnetcPoints(Vector2 start, Vector2 end)
        {
            List<Vector2> posSets = new List<Vector2>();
            posSets.Clear();

            float x0 = start.x; float y0 = start.y;
            float x1 = end.x; float y1 = end.y;


            Color lineCol = Color.white;
            float m = (y1 - y0) / (x1 - x0);

            float y = y0;
            float x = x0;
            int j = FloorToInt(y0);
            int k = FloorToInt(x0);

            float d = 0;
            d = (y0 - y1) * (x0 + 1f) + (x1 - x0) * (y + 0.5f) + x0 * y1 - x1 * y0;

            if (m <= 1f && m > -1f)
            {
                if (x0 > x1)
                {
                    for (int i = FloorToInt(x0); i > FloorToInt(x1); i--)
                    {
                        if (m > 0 && m <= 1f)
                        {
                            if (d < 0)
                            {
                                j = j - 1;
                                d = d - (x1 - x0) - (y0 - y1);
                            }
                            else
                            {
                                d = d - (y0 - y1);
                            }

                            posSets.Add(new Vector2(i, j));
                        }
                        else
                        {
                            if (d > 0)
                            {
                                j = j + 1;
                                d = d + (x1 - x0) - (y0 - y1);
                            }
                            else
                            {
                                d = d - (y0 - y1);
                            }
                            posSets.Add(new Vector2(i, j));

                        }
                    }
                }
                else
                {
                    for (int i = FloorToInt(x0); i < FloorToInt(x1); i++)
                    {
                        if (m > 0 && m <= 1f)
                        {
                            if (d < 0)
                            {
                                j = j + 1;
                                d = d + (x1 - x0) + (y0 - y1);
                            }
                            else
                            {
                                d = d + (y0 - y1);
                            }
                            posSets.Add(new Vector2(i, j));
                        }
                        else
                        {
                            if (d > 0)
                            {
                                j = j - 1;
                                d = d - (x1 - x0) + (y0 - y1);
                            }
                            else
                            {
                                d = d + (y0 - y1);
                            }
                            posSets.Add(new Vector2(i, j));
                        }
                    }
                }

            }
            else
            {

                if (y0 < y1)
                {
                    for (int n = FloorToInt(y0); n < FloorToInt(y1); n++)
                    {

                        if (m > 1f)
                        {
                            if (d < 0)
                            {
                                k = k + 1;
                                d = d - (x1 - x0) - (y0 - y1);
                            }
                            else
                            {
                                d = d - (x1 - x0);
                            }
                            posSets.Add(new Vector2(k, n));
                        }
                        else
                        {
                            if (d < 0)
                            {
                                k = k - 1;
                                d = d + (x1 - x0) - (y0 - y1);
                            }
                            else
                            {
                                d = d + (x1 - x0);
                            }
                            posSets.Add(new Vector2(k, n));
                        }

                    }
                }
                else
                {
                    for (int n = FloorToInt(y0); n > FloorToInt(y1); n--)
                    {
                        if (m < -1f)
                        {
                            if (d < 0)
                            {
                                k = k + 1;
                                d = d - (x1 - x0) + (y0 - y1);
                            }
                            else
                            {
                                d = d - (x1 - x0);
                            }
                            posSets.Add(new Vector2(k, n));
                        }
                        else
                        {
                            if (d > 0)
                            {
                                k = k - 1;
                                d = d - (x1 - x0) - (y0 - y1);
                            }
                            else
                            {
                                d = d - (x1 - x0);
                            }
                            posSets.Add(new Vector2(k, n));
                        }

                    }
                }
            }
            return posSets;
        }
        public static List<Vector2> ConnetcPoints(List<Vector2> point)
        {

            List<Vector2> perPos = new List<Vector2>();

            for (int i = 0; i < point.Count; i++)
            {
                List<Vector2> drawPosSets = new List<Vector2>();
                if (i == point.Count - 1)
                    drawPosSets = ConnetcPoints(point[point.Count - 1], point[0]);
                else
                    drawPosSets = ConnetcPoints(point[i], point[i + 1]);

                perPos.AddRange(drawPosSets);

            }

            return perPos;
        }

        public static Texture2D MulTex(Texture2D src, Texture2D dst)
        {
            int M = src.width;
            int N = src.height;

            Texture2D resTex = new Texture2D(M, N, TextureFormat.RGB24, false);
            Color[] srcCol = src.GetPixels(0, 0, M, N);
            Color[] dstCol = dst.GetPixels(0, 0, M, N);
            Color[] resCol = new Color[M * N];
            for (int i = 0; i < M * N; i++)
            {
                resCol[i] = srcCol[i] * dstCol[i];
            }

            resTex.SetPixels(0, 0, M, N, resCol);
            resTex.Apply();
            return resTex;
        }
        public static Texture2D AddTex(Texture2D src, Texture2D dst)
        {
            int M = src.width;
            int N = src.height;

            Texture2D resTex = new Texture2D(M, N, TextureFormat.RGB24, false);
            Color[] srcCol = src.GetPixels(0, 0, M, N);
            Color[] dstCol = dst.GetPixels(0, 0, M, N);
            Color[] resCol = new Color[M * N];
            for (int i = 0; i < M * N; i++)
            {
                resCol[i] = srcCol[i] + dstCol[i];
            }

            resTex.SetPixels(0, 0, M, N, resCol);
            resTex.Apply();
            return resTex;
        }
        public static Texture2D SubTex(Texture2D src, Texture2D dst)
        {
            int M = src.width;
            int N = src.height;

            Texture2D resTex = new Texture2D(M, N, TextureFormat.RGB24, false);
            Color[] srcCol = src.GetPixels(0, 0, M, N);
            Color[] dstCol = dst.GetPixels(0, 0, M, N);
            Color[] resCol = new Color[M * N];
            for (int i = 0; i < M * N; i++)
            {
                float gray = Abs(srcCol[i].grayscale - dstCol[i].grayscale);
                resCol[i] = new Color(gray, gray, gray, 1f);
                //resCol[i] = srcCol[i] - dstCol[i];
            }

            resTex.SetPixels(0, 0, M, N, resCol);
            resTex.Apply();
            return resTex;
        }
        public static Texture2D ComplementTex(Texture2D tex)
        {
            int M = tex.width;
            int N = tex.height;

            Texture2D resTex = new Texture2D(M, N, TextureFormat.RGB24, false);
            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Color res = Color.white - tex.GetPixel(m, n);
                    resTex.SetPixel(m, n, res);
                }
            }

            resTex.Apply();
            return resTex;
        }

        public static Color[] InitialColor(int M, int N, Color color)
        {

            Color[] col = new Color[M * N];
            for (int i = 0; i < col.Length; i++)
            {
                col[i] = color;
            }

            return col;
        }
        public static Texture2D InitialColor(Texture2D tex, Color color)
        {
            int M = tex.width;
            int N = tex.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);

            Color[] col = new Color[M * N];
            col = InitialColor(M, N, color);
            output.SetPixels(col);
            return output;
        }
        public static Texture2D RotateTexture(Texture2D texture, float a)
        {
            int M = texture.width;
            int N = texture.height;

            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);

            for (int x = 0; x < M; x++)
            {
                for (int y = 0; y < N; y++)
                {
                    output.SetPixel(x, y, Color.black);

                    Color c = texture.GetPixel(x, y);
                    int xn = FloorToInt((x - M / 2) * Cos(a) - (y - N / 2) * Sin(a));
                    int yn = FloorToInt((x - M / 2) * Sin(a) + (y - N / 2) * Cos(a));

                    output.SetPixel(xn + M / 2, yn + N / 2, c);
                }
            }
            output.Apply();
            return output;
        }

        public static Texture2D Checkerboard(params int[] p)
        {
            ////////////////////////////////////////////////////////
            //size = block size; r = row count; c = column count;//
            ///////////////////////////////////////////////////////
            int size = 0; int r = 0; int c = 0;
            int a = p.Length;
            if (a == 0)
            {
                size = 10;
                r = c = 8;
            }

            if (a == 1)
            {
                size = p[0];
                r = c = 8;
            }

            if (a == 2)
            {
                size = p[0];
                r = c = p[1];
            }

            if (a == 3)
            {
                size = p[0];
                r = p[1];
                c = p[2];
            }
            //////////////////////////////////////////////////////
            Texture2D output = new Texture2D(size * r, size * c, TextureFormat.RGB24, false);

            if (p.Length <= 3)
            {


                for (int m = 0; m < output.width; m++)
                {
                    for (int n = 0; n < output.height; n++)
                    {
                        int x = FloorToInt(m / size);
                        int y = FloorToInt(n / size);
                        int i = (x + y) % 2;
                        if (m < output.width / 2)
                            output.SetPixel(m, n, Color.white * i);
                        else
                            output.SetPixel(m, n, Color.white * i + Color.gray);

                    }
                }

                output.Apply();
            }
            else
            {
                Debug.LogError("Count of Parameters must less than 3");
            }



            return output;
        }

        public static float[,] Transpos(float[,] f)
        {
            float[,] res = new float[f.GetUpperBound(0) + 1, f.GetUpperBound(1) + 1];

            for (int y = 0; y <= res.GetUpperBound(1); y++)
            {
                for (int x = 0; x <= res.GetUpperBound(0); x++)
                {
                    res[y, x] = f[x, y];
                }
            }

            return res;
        }
        public static float[,] CenterMirror(float[,] f)
        {
            int r = f.GetUpperBound(0) + 1;
            int c = f.GetUpperBound(1) + 1;
            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            float[,] res = new float[r, c];
            for (int t = -midc; t <= midc; t++)
            {
                for (int s = -midr; s <= midr; s++)
                {
                    res[midr - s, midc - t] = f[midr + s, midr + t];
                }
            }
            return res;
        }
        public static int[,] Transpos(int[,] f)
        {
            int[,] res = new int[f.GetUpperBound(0) + 1, f.GetUpperBound(1) + 1];

            for (int y = 0; y <= res.GetUpperBound(1); y++)
            {
                for (int x = 0; x <= res.GetUpperBound(0); x++)
                {
                    res[y, x] = f[x, y];
                }
            }

            return res;
        }
        public static int[,] CenterMirror(int[,] f)
        {
            int r = f.GetUpperBound(0) + 1;
            int c = f.GetUpperBound(1) + 1;
            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            int[,] res = new int[r, c];
            for (int t = -midc; t <= midc; t++)
            {
                for (int s = -midr; s <= midr; s++)
                {
                    res[midr - s, midc - t] = f[midr + s, midr + t];
                }
            }
            return res;
        }

        public static bool IsBinaryImage(Texture2D tex)
        {
            Color[] c = tex.GetPixels();
            return !Array.Exists(c, x => x.grayscale > 0f && x.grayscale < 1f);
        }

        /// NormalDistribution
        public static float Randn()
        {

            //if R^2 is disttibution  of exponet with mean=2,
            //and theta is distribution of uniform on (0,2pi) 
            //and the  X,Y  is  guassian distribution with mean=0 sig=1
            float U1 = Random.Range(0f, 1f);
            float U2 = Random.Range(0f, 1f);
            float R = Sqrt(-2f * Log(U1));
            float thi = 2f * PI * U2;
            float X = R * Cos(thi);
            float Y = R * Sin(thi);

            return X;
        }
        public static float Randn2()
        {
            float v1, v2, s;
            do
            {
                v1 = 2.0f * Random.Range(0f, 1f) - 1.0f;
                v2 = 2.0f * Random.Range(0f, 1f) - 1.0f;
                s = v1 * v1 + v2 * v2;
            } while (s >= 1.0f || s == 0f);
            s = Sqrt((-2.0f * Log(s)) / s);
            return v1 * s;
        }
        

        public static Texture2D PaddingDatas(Texture2D texture, int r, int c, Boundary_Option option = Boundary_Option.zero)
        {
            int M = texture.width; int N = texture.height;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            Texture2D paddingTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);

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

            return paddingTex;
        }
        public static Vector4[,] PaddingDatas(Vector4[,] datas, int r, int c, Boundary_Option option = Boundary_Option.zero)
        {
            int M = datas.GetUpperBound(0) + 1; int N = datas.GetUpperBound(1) + 1;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            Vector4[,] paddingDatas = new Vector4[M + r - 1, N + c - 1];

            switch (option)
            {
                case Boundary_Option.zero:
                    paddingDatas = Zero(datas, r, c);
                    break;
                case Boundary_Option.replicate:
                    paddingDatas = Replcate(datas, r, c);
                    break;
                case Boundary_Option.symmetric:
                    paddingDatas = Symmetric(datas, r, c);
                    break;
            }

            return paddingDatas;
        }
        public static float[,] PaddingDatas(float[,] datas, int r, int c, Boundary_Option option = Boundary_Option.zero)
        {
            int M = datas.GetUpperBound(0) + 1; int N = datas.GetUpperBound(1) + 1;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            float[,] paddingDatas = new float[M + r - 1, N + c - 1];

            switch (option)
            {
                case Boundary_Option.zero:
                    paddingDatas = Zero(datas, r, c);
                    break;
                case Boundary_Option.replicate:
                    paddingDatas = Replcate(datas, r, c);
                    break;
                case Boundary_Option.symmetric:
                    paddingDatas = Symmetric(datas, r, c);
                    break;
            }

            return paddingDatas;
        }
        public static int[,] PaddingDatas(int[,] datas, int r, int c, Boundary_Option option = Boundary_Option.zero)
        {
            int M = datas.GetUpperBound(0) + 1; int N = datas.GetUpperBound(1) + 1;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            int[,] paddingDatas = new int[M + r - 1, N + c - 1];

            switch (option)
            {
                case Boundary_Option.zero:
                    paddingDatas = Zero(datas, r, c);
                    break;
                case Boundary_Option.replicate:
                    paddingDatas = Replcate(datas, r, c);
                    break;
                case Boundary_Option.symmetric:
                    paddingDatas = Symmetric(datas, r, c);
                    break;
            }

            return paddingDatas;
        }
        public static Vector2[,] PaddingDatas(Vector2[,] datas, int r, int c, Boundary_Option option = Boundary_Option.zero)
        {
            int M = datas.GetUpperBound(0) + 1; int N = datas.GetUpperBound(1) + 1;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            Vector2[,] paddingDatas = new Vector2[M + r - 1, N + c - 1];

            switch (option)
            {
                case Boundary_Option.zero:
                    paddingDatas = Zero(datas, r, c);
                    break;
                case Boundary_Option.replicate:
                    paddingDatas = Replcate(datas, r, c);
                    break;
                case Boundary_Option.symmetric:
                    paddingDatas = Symmetric(datas, r, c);
                    break;
            }

            return paddingDatas;
        }

        #endregion
    }

}
