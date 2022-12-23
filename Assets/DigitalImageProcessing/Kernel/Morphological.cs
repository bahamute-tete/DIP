using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using System;

namespace DIP
{
    public static partial class ImageProcessing
    {
        #region Morphological
        static int[,] AND(int[,] SE, int[,] T)
        {
            int r = SE.GetUpperBound(0) + 1;
            int c = SE.GetUpperBound(1) + 1;
            int[,] id = new int[r, c];
            if (T.Length != SE.Length)
            {
                Debug.LogError("T(image stencil mask) must have the same length as SE");
            }
            else
            {

                for (int s = 0; s < r; s++)
                {
                    for (int t = 0; t < c; t++)
                    {
                        id[s, t] = SE[s, t] & T[s, t];
                    }
                }
            }
            return id;
        }
        static int[,] OR(int[,] SE, int[,] T)
        {
            int r = SE.GetUpperBound(0) + 1;
            int c = SE.GetUpperBound(1) + 1;
            int[,] id = new int[r, c];

            if (T.Length != SE.Length)
            {
                Debug.LogError("T(image stencil mask) must have the same length as SE");
            }
            else
            {

                for (int s = 0; s < r; s++)
                {
                    for (int t = 0; t < c; t++)
                    {
                        id[s, t] = SE[s, t] | T[s, t];
                    }
                }
            }
            return id;
        }
        static int[,] XOR(int[,] SE, int[,] T)
        {
            int r = SE.GetUpperBound(0) + 1;
            int c = SE.GetUpperBound(1) + 1;
            int[,] id = new int[r, c];

            if (T.Length != SE.Length)
            {
                Debug.LogError("T(image stencil mask) must have the same length as SE");
            }
            else
            {

                for (int s = 0; s < r; s++)
                {
                    for (int t = 0; t < c; t++)
                    {
                        id[s, t] = SE[s, t] ^ T[s, t];
                    }
                }
            }
            return id;
        }
        static int[,] NOT(int[,] res)
        {
            int r = res.GetUpperBound(0) + 1;
            int c = res.GetUpperBound(1) + 1;
            int[,] id = new int[r, c];

            for (int s = 0; s < r; s++)
            {
                for (int t = 0; t < c; t++)
                {
                    id[s, t] = ~res[s, t];
                }
            }

            return id;
        }
        static Color[,] GetPaddingColors(Texture2D texture, Image_Type type, int r, int c)
        {
            Texture2D paddingTex = new Texture2D(texture.width + r - 1, texture.height + c - 1, TextureFormat.RGB24, false);
            switch (type)
            {
                case Image_Type.BlackWhite:
                    paddingTex = Zero(texture, r, c);
                    break;

                case Image_Type.Grayscale:
                    paddingTex = Replcate(Rgb2Gray(texture), r, c);
                    break;
            }

            Color[,] outputColor = new Color[paddingTex.width, paddingTex.height];

            for (int m = 0; m < paddingTex.width; m++)
            {
                for (int n = 0; n < paddingTex.height; n++)
                {
                    outputColor[m, n] = paddingTex.GetPixel(m, n);
                }
            }
            return outputColor;
        }
        static Color[,] Erosion(Color[,] colors, int[,] SE)
        {
            int r = SE.GetUpperBound(0) + 1;
            int c = SE.GetUpperBound(1) + 1;
            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;


            int M = colors.GetUpperBound(0) + 1;
            int N = colors.GetUpperBound(1) + 1;


            Color[,] outputColor = new Color[M, N];

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    int[,] nhood = new int[r, c];
                    bool inner = false;
                    int[,] id = new int[r, c];
                    int count = 0;

                    if (FloorToInt(colors[m, n].grayscale) == 1)
                    {

                        for (int s = -midr; s <= midr; s++)
                        {
                            for (int t = -midc; t <= midc; t++)
                            {
                                if (m + s >= 0 && n + t >= 0 && m + s < M && n + t < N)
                                {
                                    nhood[s + midr, t + midc] = FloorToInt(colors[m + s, n + t].grayscale);
                                    id = AND(SE, nhood);
                                }

                            }
                        }

                        for (int s = 0; s < r; s++)
                        {
                            for (int t = 0; t < c; t++)
                            {
                                if (id[s, t] == SE[s, t])
                                    count++;
                            }
                        }
                        inner = (count == r * c) ? true : false;
                    }

                    if (inner)
                    {
                        outputColor[m, n] = Color.white;
                    }
                }
            }

            return outputColor;
        }
        static Color[,] Erosion_Gray(Color[,] colors, int[,] SE)
        {
            int r = SE.GetUpperBound(0) + 1;
            int c = SE.GetUpperBound(1) + 1;
            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            int M = colors.GetUpperBound(0) + 1;
            int N = colors.GetUpperBound(1) + 1;

            Color[,] paddingCol = new Color[M, N];

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    paddingCol[m, n] = Color.black;
                }
            }

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    float[,] nhood = new float[r, c];

                    List<float> maslRes = new List<float>();


                    for (int s = -midr; s <= midr; s++)
                    {
                        for (int t = -midc; t <= midc; t++)
                        {
                            if (m + s >= 0 && n + t >= 0 && m + s < M && n + t < N)
                            {
                                nhood[s + midr, t + midc] = colors[m + s, n + t].grayscale;
                                if (SE[s + midr, t + midc] == 1)
                                {
                                    maslRes.Add(nhood[s + midr, t + midc]);
                                }
                            }
                        }
                    }



                    float min = Min(maslRes.ToArray());

                    //if (m + midr < M && n + midc < N)
                    paddingCol[m, n] = new Color(min, min, min, 1.0f);


                }
            }
            return paddingCol;
        }
        static Color[,] Dilation(Color[,] colors, int[,] SE)
        {
            int r = SE.GetUpperBound(0) + 1;
            int c = SE.GetUpperBound(1) + 1;
            int M = colors.GetUpperBound(0) + 1;
            int N = colors.GetUpperBound(1) + 1;
            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            int[,] refSE = new int[r, c];

            for (int s = 0; s < r; s++)
            {
                for (int t = 0; t < c; t++)
                {
                    refSE[s, t] = SE[r - 1 - s, c - 1 - t];
                }
            }

            Color[,] outputColor = new Color[M, N];

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    int[,] nhood = new int[r, c];
                    int[,] id = new int[r, c];

                    if (FloorToInt(colors[m, n].grayscale) == 1)
                    {
                        //Debug.Log("m =" + m + "    n =" + n);
                        for (int s = -midr; s <= midr; s++)
                        {
                            for (int t = -midc; t <= midc; t++)
                            {
                                nhood[s + midr, t + midc] = FloorToInt(colors[m + s, n + t].grayscale);

                                id = OR(refSE, nhood);
                            }
                        }

                        for (int s = -midr; s <= midr; s++)
                        {
                            for (int t = -midc; t <= midc; t++)
                            {
                                if (id[s + midr, t + midc] == 1)
                                {
                                    outputColor[m + s, n + t] = Color.white;
                                }

                            }
                        }
                    }
                }
            }

            return outputColor;
        }
        static Color[,] Dilation_Gray(Color[,] colors, int[,] SE)
        {
            int r = SE.GetUpperBound(0) + 1;
            int c = SE.GetUpperBound(1) + 1;
            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;

            int[,] refSE = new int[r, c];
            for (int s = 0; s < r; s++)
            {
                for (int t = 0; t < c; t++)
                {
                    refSE[s, t] = SE[r - 1 - s, c - 1 - t];
                }
            }

            int M = colors.GetUpperBound(0) + 1;
            int N = colors.GetUpperBound(1) + 1;

            Color[,] paddingCol = new Color[M, N];

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    paddingCol[m, n] = Color.black;
                }
            }

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    float[,] nhood = new float[r, c];

                    List<float> maslRes = new List<float>();


                    for (int s = -midr; s <= midr; s++)
                    {
                        for (int t = -midc; t <= midc; t++)
                        {
                            if (m + s >= 0 && n + t >= 0 && m + s < M && n + t < N)
                            {
                                nhood[s + midr, t + midc] = colors[m + s, n + t].grayscale;
                                if (refSE[s + midr, t + midc] == 1)
                                {
                                    maslRes.Add(nhood[s + midr, t + midc]);
                                }
                            }
                        }
                    }



                    float max = Max(maslRes.ToArray());

                    //if (m + midr < M && n + midc < N)
                    paddingCol[m, n] = new Color(max, max, max, 1.0f);


                }
            }
            return paddingCol;
        }
        static Color[,] HMT(Color[,] colors, int[,] SE)
        {
            int r = SE.GetUpperBound(0) + 1;
            int c = SE.GetUpperBound(1) + 1;
            int M = colors.GetUpperBound(0) + 1;
            int N = colors.GetUpperBound(1) + 1;
            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;
            Color[,] outputColor = new Color[M, N];




            int[,] tPos_SE = new int[r, c];
            tPos_SE = Transpos(SE);

            ////Check Not care element
            List<Vector2> notCaresPos = new List<Vector2>();
            for (int t = 0; t < c; t++)
            {
                for (int s = 0; s < r; s++)
                {
                    if (tPos_SE[s, t] != 1 && tPos_SE[s, t] != 0)
                    {
                        notCaresPos.Add(new Vector2(s, t));
                        //Debug.Log("s,t =" + s + "," + t);
                    }
                }
            }


            for (int n = 0; n < N; n++)
            {
                for (int m = 0; m < M; m++)
                {
                    int[,] nhood = new int[r, c];
                    bool match = false;
                    int[,] id = new int[r, c];
                    int count = 0;

                    for (int t = -midc; t <= midc; t++)
                    {
                        for (int s = -midr; s <= midr; s++)
                        {
                            if (m + s > 0 && n + t > 0 && m + s < M && n + t < N)
                            {
                                nhood[s + midr, t + midc] = FloorToInt(colors[m + s, n + t].grayscale);
                            }
                        }
                    }

                    for (int t = 0; t < c; t++)
                    {
                        for (int s = 0; s < r; s++)
                        {
                            if (notCaresPos.Count != 0)
                            {
                                for (int i = 0; i < notCaresPos.Count; i++)
                                {
                                    tPos_SE[(int)notCaresPos[i].x, (int)notCaresPos[i].y] = nhood[(int)notCaresPos[i].x, c - (int)notCaresPos[i].y - 1];
                                }
                            }

                            if (nhood[s, t] == tPos_SE[s, c - t - 1])
                                count++;

                        }
                    }

                    match = (count == r * c) ? true : false;

                    if (match)
                    {
                        outputColor[m, n] = Color.white;
                    }
                }
            }


            return outputColor;
        }
        static Texture2D GeodesicDilation(Texture2D signatures, Texture2D mask)
        {
            int M = signatures.width;
            int N = signatures.height;
            int P = mask.width;
            int Q = mask.height;



            Texture2D tex = Im2bw(signatures, 0.5f);
            int[,] SE = new int[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
            bool stop = false;
            int itertation = 0;

            if (M == P && N == Q)
            {
                float lastMean = 0;
                for (int m = 0; m < mask.width; m++)
                {
                    for (int n = 0; n < tex.height; n++)
                    {
                        lastMean += tex.GetPixel(m, n).grayscale / (tex.width * tex.height);
                    }
                }

                while (!stop)
                {
                    itertation++;

                    tex = ImMorphology(tex, SE, 0, Morphological_Operator.Dilation);
                    tex = MulTex(tex, mask);

                    float currentMean = 0;
                    for (int m = 0; m < tex.width; m++)
                    {
                        for (int n = 0; n < tex.height; n++)
                        {
                            currentMean += tex.GetPixel(m, n).grayscale / (tex.width * tex.height);
                        }
                    }


                    if (Abs(currentMean - lastMean) < 0.001f)
                    {
                        stop = true;
                        Debug.Log("iteration =" + itertation);
                    }
                    else
                    {
                        lastMean = currentMean;
                    }


                }
            }
            else
            {
                Debug.LogError("signature Texture and mask Texture must have the same size!");
            }



            tex.Apply();
            return tex;
        }
        static Texture2D GetConnection(Texture2D texture, Neighborhood_type nt, int[,] id, int mark)
        {
            int M = texture.width;
            int N = texture.height;

            int[,] SE = new int[3, 3];
            switch (nt)
            {
                case Neighborhood_type.Eight:
                    SE = new int[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
                    break;

                case Neighborhood_type.Four:
                    SE = new int[3, 3] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };
                    break;

                default:
                    SE = new int[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
                    break;

            }

            int sm = 0;
            int sn = 0;
            int Loop = 0;
            for (int n = 0; n < N; n++)
            {
                for (int m = 0; m < M; m++)
                {
                    if (texture.GetPixel(m, n).grayscale == 1 && Loop < 1)
                    {
                        sm = m;
                        sn = n;
                        Loop++;
                    }
                }
            }

            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
            Color[] black = InitialColor(M, N, Color.black);
            output.SetPixels(0, 0, M, N, black);
            output.SetPixel(sm, sn, Color.white);
            id[sm, sn] = mark;

            int wpCount = 0;
            for (int n = 0; n < N; n++)
            {
                for (int m = 0; m < M; m++)
                {
                    if (output.GetPixel(m, n) == Color.white)
                        wpCount++;
                }
            }

            bool stop = false;
            while (!stop)
            {

                output = ImMorphology(output, SE, Image_Type.BlackWhite, Morphological_Operator.Dilation);
                output = MulTex(texture, output);

                int wpCount_Dilation = 0;
                for (int n = 0; n < N; n++)
                {
                    for (int m = 0; m < M; m++)
                    {
                        if (output.GetPixel(m, n) == Color.white)
                        {
                            id[m, n] = mark;
                            wpCount_Dilation++;
                            //Debug.Log("whitePixelNum_Dilation =" + wpCount_Dilation);
                        }
                    }
                }

                if (wpCount != wpCount_Dilation)
                {
                    wpCount = wpCount_Dilation;
                }
                else
                {
                    stop = true;
                    // Debug.Log("Stop!!!");
                }
            }


            return output;
        }


        public static Texture2D ImMorphology(Texture2D texture, int[,] SE, Image_Type type, Morphological_Operator Operator)
        {
            int r = SE.GetUpperBound(0) + 1;
            int c = SE.GetUpperBound(1) + 1;

            int M = texture.width + r - 1;
            int N = texture.height + c - 1;

            int midr = (r - 1) / 2;
            int midc = (c - 1) / 2;
            Color[,] resColors = new Color[M, N];
            Texture2D output = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            if (type == Image_Type.BlackWhite)
            {
                switch (Operator)
                {
                    case Morphological_Operator.Erosion:
                        resColors = Erosion(GetPaddingColors(texture, type, r, c), SE);
                        break;

                    case Morphological_Operator.Dilation:
                        resColors = Dilation(GetPaddingColors(texture, type, r, c), SE);
                        break;

                    case Morphological_Operator.Opening:
                        resColors = Erosion(GetPaddingColors(texture, type, r, c), SE);
                        resColors = Dilation(resColors, SE);
                        break;

                    case Morphological_Operator.Closing:
                        resColors = Dilation(GetPaddingColors(texture, type, r, c), SE);
                        resColors = Erosion(resColors, SE);
                        break;

                    case Morphological_Operator.HMT:
                        resColors = HMT(GetPaddingColors(texture, type, r, c), SE);
                        break;
                }
            }
            else
            {
                switch (Operator)
                {
                    case Morphological_Operator.Erosion:
                        resColors = Erosion_Gray(GetPaddingColors(texture, type, r, c), SE);
                        break;

                    case Morphological_Operator.Dilation:
                        resColors = Dilation_Gray(GetPaddingColors(texture, type, r, c), SE);
                        break;

                    case Morphological_Operator.Opening:
                        resColors = Erosion_Gray(GetPaddingColors(texture, type, r, c), SE);
                        resColors = Dilation_Gray(resColors, SE);
                        break;

                    case Morphological_Operator.Closing:
                        resColors = Dilation_Gray(GetPaddingColors(texture, type, r, c), SE);
                        resColors = Erosion_Gray(resColors, SE);
                        break;
                }
            }


            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    output.SetPixel(m, n, Color.black);
                    output.SetPixel(m, n, resColors[m + midr, n + midc]);
                }
            }
            output.Apply();
            return output;
        }
        public static int[,] ConnectedComponentLable(Texture2D texture, Neighborhood_type nt = Neighborhood_type.Eight)
        {
            Texture2D output;
            bool end = false;
            int iter = 0;
            int lable = 1;

            int M = texture.width;
            int N = texture.height;
            int[,] id = new int[M, N];
            for (int n = 0; n < N; n++)
            {
                for (int m = 0; m < M; m++)
                {
                    id[m, n] = 0;
                }
            }


            output = GetConnection(texture, nt, id, lable);
            while (!end)
            {
                iter++;
                texture = SubTex(texture, output);
                Color[] restCols = texture.GetPixels(0, 0, M, N);
                if (!Array.Exists(restCols, (a) => a.Equals(Color.white)))
                {
                    end = true;
                }
                else
                {
                    lable++;
                    //Debug.Log("mark =" + lable);
                    output = GetConnection(texture, nt, id, lable);
                }
            }
            //Debug.Log("iter =" + iter);
            return id;
        }
        public static Texture2D ImFillHole(Texture2D texture)
        {
            int M = texture.width;
            int N = texture.height;


            Texture2D tex = Im2bw(texture, 0.5f);
            Texture2D masktex = ComplementTex(tex);
            Texture2D signature = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            int[,] SE = new int[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };

            bool stop = false;
            int itertation = 0;

            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    signature.SetPixel(m, n, Color.black);
                    signature.SetPixel(0, n, Color.white);
                    signature.SetPixel(texture.width - 1, n, Color.white);
                    signature.SetPixel(m, texture.height - 1, Color.white);
                    signature.SetPixel(m, 0, Color.white);
                }
            }

            float lastMean = 0;
            for (int m = 0; m < signature.width; m++)
            {
                for (int n = 0; n < signature.height; n++)
                {
                    lastMean += signature.GetPixel(m, n).grayscale / (signature.width * signature.height);
                }
            }

            while (!stop)
            {
                itertation++;

                signature = ImMorphology(signature, SE, 0, Morphological_Operator.Dilation);
                signature = MulTex(signature, masktex);

                float currentMean = 0;
                for (int m = 0; m < signature.width; m++)
                {
                    for (int n = 0; n < signature.height; n++)
                    {
                        currentMean += signature.GetPixel(m, n).grayscale / (signature.width * signature.height);
                    }
                }


                if (Abs(currentMean - lastMean) < 0.001f)
                {
                    stop = true;
                    Debug.Log("iteration =" + itertation);
                }
                else
                {
                    lastMean = currentMean;
                }
            }

            signature = ComplementTex(signature);
            //signature = MulTex(signature, masktex);
            signature = AddTex(signature, texture);
            signature.Apply();
            return signature;
        }
        public static Texture2D ImClearBoundary(Texture2D texture, int pixel)
        {
            int M = texture.width;
            int N = texture.height;


            Texture2D tex = Im2bw(texture, 0.5f);
            Texture2D signature = new Texture2D(M, N, TextureFormat.RGB24, false);

            int[,] SE = new int[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    signature.SetPixel(m, n, Color.black);

                    signature.SetPixel(0, n, Color.white);
                    signature.SetPixel(texture.width - 1, n, Color.white);
                    signature.SetPixel(m, texture.height - 1, Color.white);
                    signature.SetPixel(m, 0, Color.white);
                }
            }

            for (int i = 0; i < pixel; i++)
            {
                signature = ImMorphology(signature, SE, 0, Morphological_Operator.Dilation);
                signature = MulTex(signature, tex);
            }

            signature = SubTex(tex, signature);
            signature.Apply();
            return signature;
        }
        public static Texture2D ImSkeleton(Texture2D texture)
        {
            int M = texture.width;
            int N = texture.height;


            Texture2D tex = Im2bw(texture, 0.5f);
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
            Color[] bgCol = new Color[M * N];
            for (int i = 0; i < bgCol.Length; i++)
            {
                bgCol[i] = Color.black;
            }
            output.SetPixels(0, 0, M, N, bgCol);

            List<Texture2D> sks = new List<Texture2D>();

            int[,] SE = new int[3, 3] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
            bool stop = false;
            int iteration = 0;
            Texture2D er = tex;

            while (!stop)
            {
                iteration++;
                Texture2D op = ImMorphology(er, SE, 0, Morphological_Operator.Opening);
                Texture2D sk = SubTex(er, op);
                sks.Add(sk);
                er = ImMorphology(er, SE, 0, Morphological_Operator.Erosion);

                Color[] erColors = er.GetPixels(0, 0, M, N);
                if (!Array.Exists(erColors, (c) => c.Equals(Color.white)))
                {
                    stop = true;
                    Debug.Log("iteration =" + iteration);
                }

            }


            foreach (var t in sks)
            {
                output = AddTex(output, t);
            }
            output.Apply();


            return output;
        }
        public static Texture2D ConvexHullSimple(Texture2D texture)
        {
            /////////////////////initial
            int M = texture.width;
            int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
            Color[] blacks = InitialColor(M, N, Color.black);
            output.SetPixels(blacks);
            //////////////////////Structure Element
            int[,] SE0 = new int[3, 3]{{1,2,2},
                                       {1,0,2},
                                       {1,2,2}};

            int[,] SE1 = new int[3, 3]{{1,1,1},
                                       {2,0,2},
                                       {2,2,2}};

            int[,] SE2 = new int[3, 3]{{2,2,1},
                                       {2,0,1},
                                       {2,2,1}};

            int[,] SE3 = new int[3, 3]{{2,2,2},
                                       {2,0,2},
                                       {1,1,1}};


            List<int[,]> SEs = new List<int[,]>();
            SEs.Add(SE0); SEs.Add(SE1); SEs.Add(SE2); SEs.Add(SE3);
            /////////////////////// Create Sets which greater than Convex Hull
            Texture2D[] ResTexs = new Texture2D[SEs.Count];
            for (int i = 0; i < ResTexs.Length; i++)
            {
                Texture2D HMTTex = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
                ResTexs[i] = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
                ResTexs[i] = texture;

                bool Stop = false;

                while (!Stop)
                {

                    HMTTex = ImMorphology(ResTexs[i], SEs[i], Image_Type.BlackWhite, Morphological_Operator.HMT);

                    Color[] colors = HMTTex.GetPixels();
                    if (!Array.Exists(colors, x => x.grayscale == 1.0f))
                    {
                        Stop = true;
                    }
                    else
                    {
                        ResTexs[i] = AddTex(ResTexs[i], HMTTex);
                    }
                }
                output = AddTex(output, ResTexs[i]);
            }
            /////////////////////// Clamp to Convex Hull
            int minX = 0, maxX = 0, minY = 0, maxY = 0;
            Vector2 startPos = GetConner(texture, Conner.LBottom);
            minX = maxX = FloorToInt(startPos.x);
            minY = maxY = FloorToInt(startPos.y);
            ///Get AABB Boundary
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < M; x++)
                {
                    if (texture.GetPixel(x, y) == Color.white)
                    {
                        int currentX = x, currentY = y;

                        minX = (currentX < minX) ? currentX : minX;
                        maxX = (currentX > maxX) ? currentX : maxX;

                        minY = (currentY < minY) ? currentY : minY;
                        maxY = (currentY > maxY) ? currentY : maxY;
                    }
                }
            }
            ///Clamp
            for (int y = 0; y < N; y++)
            {
                for (int x = 0; x < M; x++)
                {
                    if (x < minX || x > maxX || y < minY || y > maxY)
                        output.SetPixel(x, y, Color.black);
                }
            }
            ///////////////////// Extract Edge
            List<Vector2> convexHulls = new List<Vector2>();
            convexHulls = MooreTracing(output);
            ///////////////////// Draw Edge
            output.SetPixels(blacks);
            for (int i = 0; i < convexHulls.Count; i++)
            {
                output.SetPixel((int)convexHulls[i].x, (int)convexHulls[i].y, Color.white);
            }
            output.Apply();
            return output;
        }
        public static Texture2D Thinning(Texture2D texture)
        {
            int M = texture.width;
            int N = texture.height;
            Texture2D start = new Texture2D(M, N, TextureFormat.RGB24, false);

            #region SE
            int[,] SE1 = new int[3, 3] {    {0,0,0},
                                            {2,1,2},
                                            {1,1,1}};

            int[,] SE2 = new int[3, 3] {    {2,0,0},
                                            {1,1,0},
                                            {1,1,2}};

            int[,] SE3 = new int[3, 3] {    {1,2,0},
                                            {1,1,0},
                                            {1,2,0}};

            int[,] SE4 = new int[3, 3] {    {1,1,2},
                                            {1,1,0},
                                            {2,0,0}};

            int[,] SE5 = new int[3, 3] {    {1,1,1},
                                            {2,1,2},
                                            {0,0,0}};

            int[,] SE6 = new int[3, 3] {    {2,1,1},
                                            {0,1,1},
                                            {0,0,2}};

            int[,] SE7 = new int[3, 3] {    {0,2,1},
                                            {0,1,1},
                                            {0,2,1}};

            int[,] SE8 = new int[3, 3] {    {0,0,2},
                                            {0,1,1},
                                            {2,1,1}};
            #endregion
            List<int[,]> SEs = new List<int[,]>();
            SEs.Add(SE1); SEs.Add(SE2); SEs.Add(SE3); SEs.Add(SE4); SEs.Add(SE5); SEs.Add(SE6); SEs.Add(SE7); SEs.Add(SE8);

            start = texture;

            bool stop = false;

            while (!stop)
            {

                int count = 0; //the count which  se has no effected on texture 
                for (int i = 0; i < SEs.Count; i++)
                {
                    Texture2D hmt = new Texture2D(M, N, TextureFormat.RGB24, false);
                    hmt = ImMorphology(start, SEs[i], Image_Type.BlackWhite, Morphological_Operator.HMT);
                    Color[] colors = hmt.GetPixels();
                    if (!Array.Exists(colors, x => x.grayscale == 1.0f))
                    {
                        count++;
                    }
                    start = SubTex(start, hmt);
                }

                if (count == SEs.Count)
                {
                    stop = true;
                }
            }



            return start;
        }
        #endregion
    }

}
