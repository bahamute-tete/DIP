///////////////////////////////////////////
/// Some usefull tools for Digital Image///
///////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using System;


namespace DIP
{
    public static partial class ImageProcessing
    {
       

        #region Statistics
       
        public static float Mean(float[] histDatas)
        {
            float mean = 0;
            for (int i = 0; i < histDatas.Length; i++)
            {
                mean += i * histDatas[i];
            }

            return mean;

        }
        #endregion

        

        #region ColorSpace

        static Vector3 RGB2HSV(Color rgb)
        {
        Vector3 HSV = new Vector3(0f, 0f, 0f);

        float r = rgb.r;
        float g = rgb.g;
        float b = rgb.b;

        float num = 0.5f * ((r - g) + (r - b));
        float den = Sqrt(Pow(r - g, 2) + (r - b) * (g - b));
        float H = Acos(num / (den + float.Epsilon));
        if (b > g)
            H = 2 * PI - H;
        H /= 2 * PI;

        float den2 = r + g + b;
        if (den2 == 0)
            den2 = float.Epsilon;
        float S = 1f - 3f / den2 * Min(Min(r, g), b);
        //float S = 0;
        //if (Max(Max(r, g), b) != 0)
        //    S = 1f - Min(Min(r, g), b) / Max(Max(r, g), b);


        if (S == 0)
            H = 0;

        float V = (r + g + b) / 3f;
        //float V = Max(Max(r, g), b);

        HSV = new Vector3(H, S, V);
        return HSV;
        }
        static Vector3 HSV2RGB(Vector3 hsv)
        {

        float H = hsv.x;
        float S = hsv.y;
        float V = hsv.z;

        H *= 2f * PI;

        float r = 0f, g = 0f, b = 0f;
        if (H >= 0f && H < 2f / 3f * PI)
        {
            r = V * (1f + S * Cos(H) / Cos(PI / 3f - H));
            b = V * (1 - S);
            g = 3f * V - (r + b);
        }

        if (H >= 2f / 3f * PI && H < 4f / 3f * PI)
        {
            g = V * (1f + S * Cos(H - 2f / 3f * PI) / Cos(PI - H));
            r = V * (1 - S);
            b = 3f * V - (r + g);
        }

        if (H >= 4f / 3f * PI && H <= 2 * PI)
        {
            b = V * (1f + S * Cos(H - 4f / 3f * PI) / Cos(5f / 3f * PI - H));
            g = V * (1 - S);
            r = 3f * V - (b + g);
        }

        Vector3 rgb = new Vector3(r, g, b);

        Vector3.Max(Vector3.Min(rgb, Vector3.one), Vector3.zero);

        return new Vector3(rgb.x, rgb.y, rgb.z);
        }
        static Vector3[,] RGB2HSV(Texture2D texture)
        {
        int M = texture.width;
        int N = texture.height;
        float[,] H = new float[M, N];
        float[,] S = new float[M, N];
        float[,] V = new float[M, N];

        Vector3[,] HSVColor = new Vector3[M, N];

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                H[m, n] = RGB2HSV(texture.GetPixel(m, n)).x;
                S[m, n] = RGB2HSV(texture.GetPixel(m, n)).y;
                V[m, n] = RGB2HSV(texture.GetPixel(m, n)).z;
                HSVColor[m, n] = new Vector3(H[m, n], S[m, n], V[m, n]);
            }
        }
        return HSVColor;
        }
        static Vector3[,] HSV2RGB(Texture2D texture)
        {
        int M = texture.width;
        int N = texture.height;
        float[,] R = new float[M, N];
        float[,] G = new float[M, N];
        float[,] B = new float[M, N];

        Vector3[,] rgbColor = new Vector3[M, N];

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                Vector3 hsv = new Vector3(texture.GetPixel(m, n).a, texture.GetPixel(m, n).g, texture.GetPixel(m, n).b);

                rgbColor[m, n] = HSV2RGB(hsv);
            }
        }
        return rgbColor;
        }

        static Vector3 RGB2HSV2(Color rgb)
        {
        Vector3 HSV = new Vector3(0f, 0f, 0f);

        float r = rgb.r;
        float g = rgb.g;
        float b = rgb.b;

        float num = 0.5f * ((r - g) + (r - b));
        float den = Sqrt(Pow(r - g, 2) + (r - b) * (g - b));
        float H = Acos(num / (den + float.Epsilon));
        if (b > g)
            H = 2 * PI - H;
        H /= 2 * PI;


        float S = 0;
        if (Max(Max(r, g), b) != 0)
            S = 1f - Min(Min(r, g), b) / Max(Max(r, g), b);


        if (S == 0)
            H = 0;
        float V = Max(Max(r, g), b);

        HSV = new Vector3(H, S, V);
        return HSV;
        }
        static Vector3 HSV2RGB2(Vector3 hsv)
        {

        float H = hsv.x;
        float S = hsv.y;
        float V = hsv.z;

        H *= 2f * PI;

        float C = V * S;
        float m = V - C;
        float X = C * (1 - Abs((PI * H / 3f) % 2 - 1));

        float r = 0f, g = 0f, b = 0f;
        if (H >= 0f && H < 1f / 3f * PI)
        {
            r = C;
            g = X;
            b = 0f;
        }
        if (H >= 1f / 3f * PI && H < 2f / 3f * PI)
        {
            r = X;
            g = C;
            b = 0f;
        }


        if (H >= 2f / 3f * PI && H < PI)
        {
            r = 0;
            g = C;
            b = X;
        }
        if (H >= PI && H < 4f / 3f * PI)
        {
            r = 0;
            g = X;
            b = C;
        }

        if (H >= 4f / 3f * PI && H < 5f / 3f * PI)
        {
            r = X;
            g = 0;
            b = C;
        }
        if (H >= 5f / 3f * PI && H <= 2 * PI)
        {

            r = C;
            g = 0;
            b = X;
        }

        Vector3 rgb = new Vector3(r + m, g + m, b + m);

        Vector3.Max(Vector3.Min(rgb, Vector3.one), Vector3.zero);

        return new Vector3(rgb.x, rgb.y, rgb.z);
        }
        static Vector3[,] RGB2HSV2(Texture2D texture)
        {
        int M = texture.width;
        int N = texture.height;
        float[,] H = new float[M, N];
        float[,] S = new float[M, N];
        float[,] V = new float[M, N];

        Vector3[,] HSVColor = new Vector3[M, N];

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                H[m, n] = RGB2HSV2(texture.GetPixel(m, n)).x;
                S[m, n] = RGB2HSV2(texture.GetPixel(m, n)).y;
                V[m, n] = RGB2HSV2(texture.GetPixel(m, n)).z;
                HSVColor[m, n] = new Vector3(H[m, n], S[m, n], V[m, n]);
            }
        }
        return HSVColor;
        }
        static Vector3[,] HSV2RGB2(Texture2D texture)
        {
        int M = texture.width;
        int N = texture.height;
        float[,] R = new float[M, N];
        float[,] G = new float[M, N];
        float[,] B = new float[M, N];

        Vector3[,] rgbColor = new Vector3[M, N];

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                Vector3 hsv = new Vector3(texture.GetPixel(m, n).a, texture.GetPixel(m, n).g, texture.GetPixel(m, n).b);

                rgbColor[m, n] = HSV2RGB2(hsv);
            }
        }
        return rgbColor;
        }

        public static float Rgb2Gray(Color rgba) => Vector4.Dot(rgba, new Vector4(0.2989f, 0.5870f, 0.1140f, 0f));
        public static Texture2D Rgb2Gray(Texture2D texture)
        {
            int M = texture.width; int N = texture.height;

            Texture2D grayTex = new Texture2D(M, N, TextureFormat.ARGB32, false);
            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {

                    Color color = texture.GetPixel(m, n);
                    float gray = Rgb2Gray(color);
                    Color grayColor = new Color(gray, gray, gray, 1f);
                    grayTex.SetPixel(m, n, grayColor);
                }
            }
            grayTex.Apply();
            return grayTex;
        }
        public static float Rgb2Luminance(Color rgba) => Vector4.Dot(rgba, new Vector4(0.305306011f, 0.682171111f, 0.012522878f, 0f));
        public static Texture2D Rgb2Luminance(Texture2D texture)
        {
            int M = texture.width; int N = texture.height;

            Texture2D grayTex = new Texture2D(M, N, TextureFormat.ARGB32, false);
            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {

                    Color color = texture.GetPixel(m, n);
                    float L = Rgb2Luminance(color);
                    Color LColor = new Color(L, L, L, 1f);
                    grayTex.SetPixel(m, n, LColor);
                }
            }
            grayTex.Apply();
            return grayTex;
        }
        #endregion

        #region ImageEnhance
        public static Texture2D StretchTransform(Texture2D texture, float mean, float E = 4.0f)
        {
            int M = texture.width;
            int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);
            Color[] texCol = new Color[M * N];
            float[] gray = new float[M * N];
            Color[] resCol = new Color[M * N];

            if (texture.isReadable)
            {

                texCol = texture.GetPixels(0, 0, M, N, 0);
                for (int i = 0; i < texCol.Length; i++)
                {
                    gray[i] = 1f / (1f + Pow((mean / Rgb2Gray(texCol[i])), E));
                    resCol[i] = new Color(gray[i], gray[i], gray[i], 1f);
                }
                output.SetPixels(0, 0, M, N, resCol);
            }

            output.Apply();
            return output;
        }
        public static Texture2D LogTransform(Texture2D texture, float C = 1f)
        {
            int M = texture.width;
            int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);
            Color[] texCol = new Color[M * N];
            float[] gray = new float[M * N];
            Color[] resCol = new Color[M * N];
            if (texture.isReadable)
            {

                texCol = texture.GetPixels(0, 0, M, N, 0);
                for (int i = 0; i < texCol.Length; i++)
                {
                    gray[i] = C * Log(1f + Rgb2Gray(texCol[i]));
                    resCol[i] = new Color(gray[i], gray[i], gray[i], 1f);
                }
                output.SetPixels(0, 0, M, N, resCol);
            }
            output.Apply();
            return output;
        }
        #endregion

    }


}
 


