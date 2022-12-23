using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using System;

namespace DIP
{
    public static partial class ImageProcessing
    {
        #region Segmenation
         static Vector2[,] DxDy_GrayScale(Texture2D texture,Edge_Operator op,CheckDir dir =CheckDir.Both)
            {
                int M = texture.width + 2;
                int N = texture.height + 2;

                Vector2[,] grad = new Vector2[texture.width, texture.height];
                float[,] gradx = new float[M, N];
                float[,] grady = new float[M, N];

                float[,] f_hor = new float[3, 3];
                float[,] f_ver = new float[3, 3];

                switch (op)
                {
                    case Edge_Operator.Sobel:
                        f_ver = FSpecial(KernelType.sobel);
                        f_hor = Transpos(f_ver);
                        break;

                    case Edge_Operator.Prewitt:
                        f_ver = FSpecial(KernelType.prewitt);
                        f_hor = Transpos(f_ver);
                        break;
                }
            

                #region Padding
                Texture2D paddingTex = new Texture2D(M, N, TextureFormat.ARGB32, false);
                paddingTex = Zero(texture, 3, 3);
                #endregion

                for (int n = 0; n < N; n++)
                {
                    for (int m = 0; m < M; m++)
                    {
                        for (int t = -1; t <= 1; t++)
                        {
                            for (int s = -1; s <= 1; s++)
                            {
                                if (m + s > 0 && n + t > 0 && m + s < M && n + t < N)
                                {
                                    switch (dir)
                                    {
                                        case CheckDir.Horizontal:
                                            grady[m, n] = 0;
                                            gradx[m, n] += paddingTex.GetPixel(m + s, n + t).grayscale * f_hor[s + 1, t + 1];
                                            break;

                                        case CheckDir.Vertical:
                                            grady[m, n] += paddingTex.GetPixel(m + s, n + t).grayscale * f_ver[s + 1, t + 1];
                                            gradx[m, n] = 0;
                                            break;

                                        case CheckDir.Both:
                                            grady[m, n] += paddingTex.GetPixel(m + s, n + t).grayscale * f_ver[s + 1, t + 1];
                                            gradx[m, n] += paddingTex.GetPixel(m + s, n + t).grayscale * f_hor[s + 1, t + 1];
                                            break;
                                    }
                                }   
                            }
                        }
                    }
                }

                for (int m = 0; m < texture.width; m++)
                {
                    for (int n = 0; n < texture.height; n++)
                    {
                        if (m + 1 < texture.width && n + 1 < texture.height)
                            grad[m, n] = new Vector2(gradx[m + 1, n + 1], grady[m + 1, n + 1]);
                    }
                }
                return grad;
            }
         static float[,] GradAmplitude(Vector2[,] gradient , bool isNormal = true, Accuracy ac = Accuracy.Approximate)
            {
                int M = gradient.GetUpperBound(0) + 1;
                int N = gradient.GetUpperBound(1) + 1;

                float[,] amps = new float[M,N];

                float maxGrad = 0f;
                for (int y = 0; y < N; y++)
                {
                    for (int x = 0; x < M; x++)
                    {
                        switch (ac)
                        {
                            case Accuracy.Precision:
                                amps[x, y] = Sqrt(gradient[x, y].x * gradient[x, y].x + gradient[x, y].y * gradient[x, y].y);
                                break;

                            case Accuracy.Approximate:
                                amps[x, y] = Abs(gradient[x, y].x) + Abs(gradient[x, y].y);
                                break;
                        }

                        if (amps[x, y] > maxGrad)
                            maxGrad = amps[x, y];
                    }
                }

                if (isNormal == true)
                {
                    for (int y = 0; y < N; y++)
                    {
                        for (int x = 0; x < M; x++)
                        {
                            amps[x, y] /= maxGrad;
                        }
                    }
                }
            

                return amps;
            }
         static float[,] GradDirection(Vector2[,] gradient)
        {
            int M = gradient.GetUpperBound(0) + 1;
            int N = gradient.GetUpperBound(1) + 1;

            float[,] dirs = new float[M, N];


            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < M; x++)
                {
                    dirs[x, y] = Atan2(gradient[x, y].y, gradient[x, y].x);
                    dirs[x, y] = dirs[x, y] * Rad2Deg;
                }
            }

            return dirs;
        }
         public static Texture2D ImLaplacian(Texture2D texture, bool isScale = false, Laplacian_Display ld = Laplacian_Display.Abs, Boundary_Option option = Boundary_Option.zero)
        {
            int r = 3;
            int c = 3;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            int M = texture.width; int N = texture.height;
            Texture2D paddingTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            Texture2D filterTex = new Texture2D(M, N, TextureFormat.ARGB32, false);
            ////////////////////////Padding Texture
            ///
            paddingTex = PaddingDatas(texture, r, c, option);
            //float[,] f = new float[3, 3] { { 0f, 1f, 0f }, { 1f, -4f, 1f }, { 0f, 1f, 0f } };
            float[,] f = new float[3, 3] { { 1f, 1f, 1f }, { 1f, -8f, 1f }, { 1f, 1f, 1f } };

            float[,] paddingRes = new float[paddingTex.width, paddingTex.height];

            float maxAbs = 0;
            float min = 0;
            float max = 0;

            for (int m = 0; m < paddingTex.width; m++)
            {
                for (int n = 0; n < paddingTex.height; n++)
                {

                    float res = 0;

                    for (int s = -midr; s <= midc; s++)
                    {
                        for (int t = -midc; t <= midr; t++)
                        {
                            if (m + s >= 0 && n + t >= 0 && m + s < paddingTex.width && n + t < paddingTex.height)
                                res += paddingTex.GetPixel(m + s, n + t).grayscale * f[s + midr, t + midc];
                        }
                    }

                    if (maxAbs < Abs(res))
                        maxAbs = Abs(res);

                    if (max < res)
                        max = res;

                    if (min > res)
                        min = res;

                    paddingRes[m, n] = res;
                }
            }

            // Debug.Log("maxABS =" + maxAbs);

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    float outRes = 0;
                    float scaleValue = 0;//[-1,1]
                    switch (ld)
                    {
                        case Laplacian_Display.Abs:
                            scaleValue = paddingRes[m + midr, n + midc] / (max - min);
                            outRes = (isScale) ? Abs(scaleValue) : Abs(paddingRes[m + midr, n + midc]);
                            break;

                        case Laplacian_Display.Shift:
                            scaleValue = paddingRes[m + midr, n + midc] / (max - min);
                            outRes = 0.5f * scaleValue + 0.5f;//[0,1]
                            break;

                        case Laplacian_Display.Positive:
                            scaleValue = paddingRes[m + midr, n + midc] / (max - min);
                            outRes = (isScale) ? scaleValue : paddingRes[m + midr, n + midc];
                            break;

                    }

                    Color color = new Color(outRes, outRes, outRes);
                    filterTex.SetPixel(m, n, color);
                }
            }

            filterTex.Apply();
            return filterTex;
        }
         static float[,] Laplacian(Texture2D texture, bool isScale = false, Boundary_Option option = Boundary_Option.zero)
        {
            int r = 3;
            int c = 3;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            int M = texture.width; int N = texture.height;
            Texture2D paddingTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            float[,] laplacian = new float[M, N];
            ////////////////////////Padding Texture
            ///
            paddingTex = PaddingDatas(texture, r, c, option);

            //float[,] f = new float[3, 3] { { 0f, 1f, 0f }, { 1f, -4f, 1f }, { 0f, 1f, 0f } };
            float[,] f = new float[3, 3] { { 1f, 1f, 1f }, { 1f, -8f, 1f }, { 1f, 1f, 1f } };

            float[,] resScale = new float[paddingTex.width, paddingTex.height];

            float maxAbs = 0;
            float max = 0;
            float min = 0;

            for (int m = 0; m < paddingTex.width; m++)
            {
                for (int n = 0; n < paddingTex.height; n++)
                {

                    float res = 0;

                    for (int s = -midr; s <= midr; s++)
                    {
                        for (int t = -midc; t <= midc; t++)
                        {
                            if (m + s >= 0 && n + t >= 0 && m + s < paddingTex.width && n + t < paddingTex.height)
                                res += paddingTex.GetPixel(m + s, n + t).grayscale * f[s + midr, t + midc];
                        }
                    }

                    if (maxAbs < Abs(res))
                        maxAbs = Abs(res);

                    if (max < res)
                        max = res;

                    if (min > res)
                        min = res;

                    resScale[m, n] = res;
                }
            }

            //Debug.Log("maxABS =" + maxAbs);
            Debug.Log("max =" + max);
            Debug.Log("min =" + min);

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    float scaleValue = resScale[m + midr, n + midc] / (max - min);
                    float outRes = (isScale) ? resScale[m + midr, n + midc] / maxAbs : resScale[m + midr, n + midc];
                    laplacian[m, n] = outRes;

                }
            }

            return laplacian;
        }

         static Texture2D LoG(Texture2D texture, float T, float sig)
        {   /////////////////////////
            ///---MarrHildreth---////
            /////////////////////////
            float c = 6f * sig;
            int n = CeilToInt(c);
            n = ((n & 1) == 1) ? n : n + 1;

            int M = texture.width;
            int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);

            Texture2D blurTex = GaussianBlur(texture, n, n, sig, Boundary_Option.replicate);
            float[,] lapValues = Laplacian(blurTex);
            float[,] lapPadding = PaddingDatas(lapValues, 3, 3);

            Color[] labColors = new Color[M * N];
            for (int y = 0, i=0; y < N; y++)
            {
                for (int x = 0; x < M; x++,i++)
                {
                    float f = lapValues[x, y];

                    float Hdiff = 0;
                    float Vdiff = 0;
                    float DLdiff = 0;
                    float DRdiff = 0;

                    output.SetPixel(x, y, Color.black);

                    if (x < M+ 1 && y < N + 1 && x >= 1 && y >= 1)
                    {
                        float signV = Sign(lapPadding[x, y + 1] * lapPadding[x, y - 1]);
                        Vdiff = Abs(lapPadding[x, y + 1] - lapPadding[x, y - 1]);

                        float signH = Sign(lapPadding[x + 1, y] * lapPadding[x - 1, y]);
                        Hdiff = Abs(lapPadding[x + 1, y] - lapPadding[x - 1, y]);

                        float signDL = Sign(lapPadding[x - 1, y + 1] * lapPadding[x + 1, y - 1]);
                        DLdiff = Abs(lapPadding[x - 1, y + 1] - lapPadding[x + 1, y - 1]);

                        float signDR = Sign(lapPadding[x + 1, y + 1] * lapPadding[x - 1, y - 1]);
                        DRdiff = Abs(lapPadding[x + 1, y + 1] - lapPadding[x - 1, y - 1]);

                        if ((signV < 0 && Vdiff > T) || (signH < 0 && Hdiff > T) ||
                            (signDL < 0 && DLdiff > T) || (signDR < 0 && DRdiff > T))
                        {
                            output.SetPixel(x, y, Color.white);
                        }
                    }



                    labColors[i] = new Color(f,f,f);

                    //Debug.Log(labColors[i]);
                }
            }
            //output.SetPixels(labColors);
            output.Apply();
            return output;

        }
         static Texture2D Sobel(Texture2D texture, float T ,float sig, CheckDir dir = CheckDir.Both)
        {   ///////////////////
            ///---Sobel---////
            //////////////////
            ///
          
            int M = texture.width;
            int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
            Texture2D blurTex = new Texture2D(M, N, TextureFormat.RGB24, false);

            if (sig != 0)
            {
                float c = 6f * sig;
                int n = CeilToInt(c);
                n = ((n & 1) == 1) ? n : n + 1;
                blurTex = GaussianBlur(texture, n, n, sig, Boundary_Option.replicate);
            }
            else
            {
                blurTex = texture;
            }
           
            Vector2[,] grad = new Vector2[M, N];
            float[,] amplitude = new float[M, N];

            grad = DxDy_GrayScale(blurTex, Edge_Operator.Sobel,dir);
            amplitude = GradAmplitude(grad);

            for (int x = 0; x < M; x++)
            {
                for (int y = 0; y < N; y++)
                {
                    output.SetPixel(x, y, new Color(amplitude[x, y], amplitude[x, y], amplitude[x, y]));
                }
            }

            output = Im2bw(output, T);

            return output;

        }
         static Texture2D Prewitt(Texture2D texture, float T, float sig, CheckDir dir = CheckDir.Both)
        {   ////////////////////
            ///---Prewitt---////
            ////////////////////

            int M = texture.width;
            int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
            Texture2D blurTex = new Texture2D(M, N, TextureFormat.RGB24, false);

            if (sig != 0)
            {
                float c = 6f * sig;
                int n = CeilToInt(c);
                n = ((n & 1) == 1) ? n : n + 1;
                blurTex = GaussianBlur(texture, n, n, sig, Boundary_Option.replicate);
            }
            else
            {
                blurTex = texture;
            }

            Vector2[,] grad = new Vector2[M, N];
            float[,] amplitude = new float[M, N];

            grad = DxDy_GrayScale(texture, Edge_Operator.Prewitt,dir);
            amplitude = GradAmplitude(grad);

            for (int x = 0; x < M; x++)
            {
                for (int y = 0; y < N; y++)
                {
                    output.SetPixel(x, y, new Color(amplitude[x, y], amplitude[x, y], amplitude[x, y]));
                }
            }
            output.Apply();

            output = Im2bw(output, T);

            return output;

        }
         public static Texture2D Canny(Texture2D texture, float TL, float TH,float sig)
         {  

            int M = texture.width;
            int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
            Texture2D blurTex = new Texture2D(M, N, TextureFormat.RGB24, false);

            if (sig != 0)
            {
                float c = 6f * sig;
                int n = CeilToInt(c);
                n = ((n & 1) == 1) ? n : n + 1;
                blurTex = GaussianBlur(texture, n, n, sig, Boundary_Option.replicate);
            }
            else
            {
                blurTex = texture;
            }

            Vector2[,] grad = new Vector2[M, N];
            float[,] amplitude = new float[M, N];
            float[,] dirs = new float[M, N];

            grad = DxDy_GrayScale(blurTex, Edge_Operator.Sobel);
            amplitude = GradAmplitude(grad);
            dirs = GradDirection(grad);

            //////Non maximum suppressio
            ///
            float[,] edgesStrong = new float[M, N];
            float[,] edgesWeak = new float[M, N];
            float[,] edges = new float[M, N]; 
            for (int s = 0; s < M; s++)
            {
                for (int t = 0; t < N; t++)
                {
                    if (s - 1 >= 0 && s + 1 < M && t - 1 >= 0 && t + 1 < N)
                    {
                        if ((dirs[s, t] >= -22.5f && dirs[s, t] < 22.5f) || (dirs[s, t] >= -67.5f && dirs[s, t] < -22.5f))
                        {
                            //-90 or 90
                            if (amplitude[s, t] > amplitude[s + 1, t] && amplitude[s, t] > amplitude[s - 1, t])
                                edges[s, t] = amplitude[s, t];
                            else
                                edges[s, t] = 0;
                        }

                        if ((dirs[s, t] >= 22.5f && dirs[s, t] < 67.5f) || (dirs[s, t] >= -112.5f && dirs[s, t] < -67.5f))
                        {
                            //-45 or 135
                            if (amplitude[s, t] > amplitude[s + 1, t + 1] && amplitude[s, t] > amplitude[s - 1, t - 1])
                                edges[s, t] = amplitude[s, t];
                            else
                                edges[s, t] = 0;
                        }

                        if ((dirs[s, t] >= 67.5f && dirs[s, t] < 112.5f) || (dirs[s, t] >= -157.5f && dirs[s, t] < -112.5f))
                        {
                            //0 or 180
                            if (amplitude[s, t] > amplitude[s, t + 1] && amplitude[s, t] > amplitude[s, t - 1])
                                edges[s, t] = amplitude[s, t];
                            else
                                edges[s, t] = 0;
                        }

                        if ((dirs[s, t] >= 112.5f && dirs[s, t] < 157.5f) || (dirs[s, t] >= -180f && dirs[s, t] < -157.5f && dirs[s, t] >= 157.5f && dirs[s, t] < 180f))
                        {
                            //45 or -135
                            if (amplitude[s, t] > amplitude[s - 1, t + 1] && amplitude[s, t] >amplitude[s + 1, t - 1])
                                edges[s, t] = amplitude[s, t];
                            else
                                edges[s, t] = 0;
                        }

                      
                    }
                }
            }

            /////two-threshold

            for (int s = 0; s < M; s++)
            {
                for (int t = 0; t < N; t++)
                {
                    if (edges[s, t] >= TH)
                        edgesStrong[s, t] = 1;
                    else
                        edgesStrong[s, t] = 0;

                    if (edges[s, t] >= TL && edges[s,t]<TH)
                        edgesWeak[s, t] = 1;
                    else
                        edgesWeak[s, t] = 0;
                }
            }

            /////Connectivity
            
            for (int x = 0; x < M; x++)
            {
                for (int y = 0; y < N; y++)
                {

                    output.SetPixel(x, y, new Color(edgesStrong[x, y], edgesStrong[x, y], edgesStrong[x, y]));

                    if (edgesStrong[x, y] == 1)
                    {
                        for (int s = -1; s <= 1; s++)
                        {
                            for (int t = -1; t <= 1; t++)
                            {
                                if (edgesWeak[x + s, y + t] == 1 && x + s >= 0 && x + s < M && y + t >= 0 && y + t < N)
                                    output.SetPixel(x, y, Color.white);

                            }
                        }
                    }

                }
            }

           
            output.Apply();

            return output;

         }

        public static Texture2D Edge(Texture2D texture, Edge_Method method, params float[] param)
        {
            int M = texture.width;
            int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);

            Vector2[,] grad = new Vector2[M, N];
            float[,] amplitude = new float[M, N];

            float t = 0;
            float tH = 0;
            float sig = 0;

            switch (method)
            {
                case Edge_Method.Sobel:
                    if (param.Length == 0)
                    {
                        t = 0.1f;
                        sig = 0;
                    }

                    if (param.Length == 1)
                    {
                        t = Clamp01(param[0]);
                        sig = 0;
                    }

                    if (param.Length == 2)
                    {
                        t = Clamp01(param[0]);
                        sig = param[1];
                    }

                    output = Sobel(texture, t, sig);
                    break;

                case Edge_Method.Prewitt:
                    if (param.Length == 0)
                    {
                        t = 0.1f;
                        sig = 0;
                    }

                    if (param.Length == 1)
                    {
                        t = Clamp01(param[0]);
                        sig = 0;
                    }

                    if (param.Length == 2)
                    {
                        t = Clamp01(param[0]);
                        sig = param[1];
                    }

                    output = Prewitt(texture, t, sig);
                    break;

                case Edge_Method.LoG:
                    if (param.Length == 0)
                    {
                        t = 0.01f;
                        sig = 0.5f;
                    }

                    if (param.Length == 1)
                    {
                        t = Clamp01(param[0]);
                        sig = 0.5f;
                    }

                    if (param.Length == 2)
                    {
                        t = Clamp01(param[0]);
                        sig = param[1];
                    }

                    output = LoG(texture, t, sig);
                    break;

                case Edge_Method.Canny:
                    if (param.Length == 0)
                    {
                        t = 0.05f;
                        tH = 0.1f;
                        sig = 0.5f;
                    }

                    if (param.Length == 1)
                    {
                        t = 0.05f;
                        tH = 0.1f;
                        sig = param[0];
                    }

                    if (param.Length == 2)
                    {
                        t = Clamp01(param[0]);
                        tH = Clamp01(param[1]);
                        if (param[1] < param[0])
                        {
                            Debug.LogError("Threshold2 must great than Threshold1");
                            return null;
                        }
                        sig = 0.5f;
                    }

                    if (param.Length == 3)
                    {
                        t = Clamp01(param[0]);
                        tH = Clamp01(param[1]);
                        if (param[1] < param[0])
                        {
                            Debug.LogError("Threshold2 must great than Threshold1");
                            return null;
                        }
                        sig = param[2];
                    }

                    output = Canny(texture, t, tH,sig);
                    break;
            }

           
            return output;
        }

        public static Texture2D Hough(int M, int N, float step = 0.01f, params Vector2[] pointPos)
        {
            int W = FloorToInt(PI / step);
            int H = FloorToInt(2 * Sqrt(2) * 100) + 2;

            Texture2D output = new Texture2D(W, H, TextureFormat.RGB24, false);

            for (int m = 0; m < W; m++)
            {
                for (int n = 0; n < H; n++)
                {
                    output.SetPixel(m, n, Color.black);

                }
            }

            int count = pointPos.Length;

            for (int i = 0; i < count; i++)
            {
                float u = pointPos[i].x / (M - 1);
                float v = pointPos[i].y / (N - 1);

                List<int> rhos = new List<int>();

                for (float theta = -0.5f * PI; theta <= 0.5f * PI; theta += step)
                {
                    float rho = u * Cos(theta) + v * Sin(theta);

                    int rhoInt = FloorToInt(rho * 100);
                    rhos.Add(rhoInt);
                }

                for (int j = 0; j < rhos.Count; j++)
                {
                    output.SetPixel(j, rhos[j] + H / 2, Color.white);

                }
            }

            output.Apply();

            return output;
        }
        public static Texture2D Hough(Texture2D texture, float step = 0.01f)
        {
            int W = FloorToInt(PI / step);
            int H = FloorToInt(2 * Sqrt(2) * 100) + 2;
            int M = texture.width;
            int N = texture.height;

            Texture2D output = new Texture2D(W, H, TextureFormat.RGB24, false);

            for (int m = 0; m < W; m++)
            {
                for (int n = 0; n < H; n++)
                {
                    output.SetPixel(m, n, Color.black);

                }
            }

            Color[] textureCol = texture.GetPixels(0, 0, M, N);

            for (int i = 0; i < textureCol.Length; i++)
            {
                if (textureCol[i] == Color.white)
                {
                    int y = i % N;
                    int x = (i - y) / N;

                    float u = x / (float)(M - 1);
                    float v = y / (float)(N - 1);

                    List<int> rhos = new List<int>();

                    for (float theta = -0.5f * PI; theta <= 0.5f * PI; theta += step)
                    {
                        float rho = u * Cos(theta) + v * Sin(theta);

                        int rhoInt = FloorToInt(rho * 100);
                        rhos.Add(rhoInt);
                    }

                    for (int j = 0; j < rhos.Count; j++)
                    {
                        output.SetPixel(j, rhos[j] + H / 2, Color.white);
                    }
                }
            }

            output.Apply();

            return output;
        }
       
        public static float GrayThresh(Texture2D texture)
        {
            int count = 0;
            float[] his = Histogram(texture, TextureChannel.Gay);


            float T = Mean(his);
            int Tmin = FloorToInt(T);
            float Tnext = 0;

            bool done = false;

            while (!done)
            {
                count++;
                //Debug.Log("Count ==" + count);
                List<float> g1 = new List<float>();
                List<float> g2 = new List<float>();
                for (int i = 0; i <= Tmin; i++)
                {
                    g1.Add(his[i]);
                }

                for (int i = Tmin + 1; i < his.Length; i++)
                {
                    g2.Add(his[i]);
                }


                Tnext = 0.5f * (Mean(g1.ToArray()) + Mean(g2.ToArray()));
                done = Abs(T - Tnext) < 0.5f;
                T = Tnext;
                Tmin = FloorToInt(T);

                //Debug.Log("g1.cout==" + g1.Count);
            }

            return T / 255f;

        }
        public static float OtsuThresh(Texture2D texture)
        {

            float[] his = Histogram(texture, TextureChannel.Gay);
            float T = 0;
            List<float> pis = new List<float>();
            List<float> ms = new List<float>();
            float mg = 0f;

            for (int K = 0; K < 256; K++)
            {
                float pi = 0f;
                float m = 0f;

                mg += K * his[K];

                for (int i = 0; i <= K; i++)
                {
                    pi += his[i];
                    m += i * his[i];

                }

                pis.Add(pi);
                ms.Add(m);
            }

            List<float> sigSquars = new List<float>();
            List<float> sigSquarsCopy = new List<float>();

            for (int K = 0; K < 256; K++)
            {
                float sigSquarValue = 0;
                sigSquarValue = Pow(mg * pis[K] - ms[K], 2) / (pis[K] * (1f - pis[K]) + Epsilon);
                sigSquars.Add(sigSquarValue);
                sigSquarsCopy.Add(sigSquarValue);
            }

            float max = Max(sigSquars.ToArray());
            bool done = false;

            List<int> indexes = new List<int>();
            while (!done)
            {
                if (sigSquarsCopy.Contains(max))
                {
                    int index = sigSquarsCopy.FindIndex(x => x.Equals(max));
                    sigSquarsCopy[index] = max + 1000f;
                    indexes.Add(index);
                }
                else
                {
                    done = true;
                }
            }

            for (int i = 0; i < indexes.Count; i++)
            {

                T += indexes[i] / indexes.Count;
            }

            // Debug.Log("T ==" + T);


            float sigSquarG = 0;
            for (int i = 0; i < 256; i++)
            {
                sigSquarG += Pow(i - mg, 2) * his[i];
            }

            float ita = 0;
            ita = sigSquars[FloorToInt(T)] / sigSquarG;
            //Debug.Log("ita ==" + ita);

            return T / 255f;

        }
        public static Texture2D LocalThresh(Texture2D texture, int r, int c, float a, float b)
        {
            int M = texture.width;
            int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);

            float[,] nhood = new float[r, c];
            for (int i = 0; i < r; i++)
            {
                for (int j = 0; j < c; j++)
                {
                    nhood[i, j] = 1f / (r * c);
                }
            }

            Texture2D localMean = ImFilter(texture, nhood, Boundary_Option.replicate);
            Color[] localMeanCol = localMean.GetPixels(0, 0, M, N);

            Texture2D paddingTex = new Texture2D(M + r - 1, N + c - 1, TextureFormat.ARGB32, false);
            paddingTex = Replcate(texture, r, c);
            int midr = (r - 1) / 2; int midc = (c - 1) / 2;

            float[] localsigs = new float[M * N];

            for (int m = 0, i = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++, i++)
                {
                    float sig = 0;
                    for (int s = 0; s < r; s++)
                    {
                        for (int t = 0; t < c; t++)
                        {
                            sig += Pow(paddingTex.GetPixel(m + s, n + t).grayscale - localMean.GetPixel(m, n).grayscale, 2) / (r * c);
                            localsigs[i] = Sqrt(sig);
                        }

                    }
                }
            }

            //Debug.Log(localsigs.Length);


            for (int m = 0, i = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++, i++)
                {
                    if (texture.GetPixel(m, n).grayscale > a * localsigs[i] && texture.GetPixel(m, n).grayscale > b * localMeanCol[i].grayscale)
                    {
                        output.SetPixel(m, n, Color.white);
                    }
                    else
                    {
                        output.SetPixel(m, n, Color.black);
                    }
                }

            }

            output.Apply();
            return output;




        }
        public static Texture2D K_Mean_Clustering(Texture2D texture, int k)
        {
            //////////////////////////////////////////////////////////////////Initialize
            int M = texture.width;
            int N = texture.height;

            Texture2D ouput = new Texture2D(M, N, TextureFormat.RGB24, false);
            int[] identification = new int[M * N];
            Color[] pixels = texture.GetPixels(0, 0, M, N);
            Color[] pixelsCopy = texture.GetPixels(0, 0, M, N);

            bool stop = false;
            int iteration = 0;
            float[] mean = new float[k];
            float[] lastMean = new float[k];

            for (int i = 0; i < k; i++)
            {
                mean[i] = ((1f / k) * (2 * i + 1)) / 2f;
            }


            for (int i = 0; i < identification.Length; i++)
            {
                identification[i] = -1;
            }

            //////////////////////////////////////////////////////////////Iteration

            while (!stop)
            {
                iteration++;
                //Debug.Log("iteration ==" + iteration);

                for (int i = 0; i < mean.Length; i++)
                {
                    lastMean[i] = mean[i];
                }

                ///////////////////////////////////////////////Find min(distance) and clustering 
                for (int a = 0; a < pixels.Length; a++)
                {
                    float[] dist = new float[mean.Length];

                    for (int i = 0; i < mean.Length; i++)
                    {
                        dist[i] = Pow(pixels[a].grayscale - mean[i], 2);
                    }

                    float minDis = Min(dist);

                    for (int i = 0; i < mean.Length; i++)
                    {
                        if (minDis == dist[i])
                            identification[a] = i;
                    }
                }

                //////////////////////////////////////////////Update mean
                for (int i = 0; i < mean.Length; i++)
                {
                    mean[i] = 0f;
                }


                for (int i = 0; i < mean.Length; i++)
                {
                    int count = 0;
                    for (int j = 0; j < pixels.Length; j++)
                    {
                        if (identification[j] == i)
                        {
                            count++;
                        }
                    }

                    for (int j = 0; j < pixels.Length; j++)
                    {

                        if (identification[j] == i)
                        {
                            mean[i] += pixels[j].grayscale / count;
                        }
                    }
                }

                //////////////////////////////////////////// Verify convergence
                bool[] over = new bool[mean.Length];
                for (int i = 0; i < mean.Length; i++)
                {
                    over[i] = (Abs(mean[i] - lastMean[i]) < 0.001f) ? true : false;
                }

                if (!Array.Exists(over, (a) => a.Equals(false)))
                {
                    stop = true;
                    // Debug.Log("K_Mean_Clustering is Over & Iteration ="+iteration);
                }
            }

            ///////////////////////////////////////////////////////////////SetPixels
            for (int i = 0; i < pixels.Length; i++)
            {
                for (int j = 0; j < mean.Length; j++)
                {
                    if (identification[i] == j)
                    {
                        //pixelsCopy[i] = new Color(mean[j], mean[j], mean[j]);

                        float gray = (float)j / (k - 1);
                        pixelsCopy[i] = new Color(gray, gray, gray);
                    }
                }

            }


            ouput.SetPixels(0, 0, M, N, pixelsCopy);
            ouput.Apply();
            return ouput;


        }
        #endregion
    }

}
