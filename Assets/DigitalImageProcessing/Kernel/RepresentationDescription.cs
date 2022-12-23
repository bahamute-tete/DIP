using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using System;

namespace DIP
{
    public static partial class ImageProcessing
    {
        #region Feature
        public static Texture2D Resample(Texture2D tex, int M, int N)
        {
            tex = Im2bw(Rgb2Gray(tex), 0.5f);

            Texture2D output = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
            Color[] bgCol = new Color[tex.width * tex.height];
            for (int i = 0; i < bgCol.Length; i++)
            {
                bgCol[i] = Color.black;
            }
            output.SetPixels(0, 0, tex.width, tex.height, bgCol);

            float stepx = tex.width / M;
            float stepy = tex.height / N;

            for (int m = 0; m < tex.width; m++)
            {
                for (int n = 0; n < tex.height; n++)
                {
                    if (tex.GetPixel(m, n) == Color.white)
                    {
                        //int x = (int)(m / stepx+0.5f);
                        //int y = (int)(n / stepy+0.5f);

                        int x = RoundToInt(m / stepx);
                        int y = RoundToInt(n / stepy);

                        output.SetPixel(x * (int)stepx, y * (int)stepy, Color.red);
                    }

                }
            }

            output.Apply();

            return output;
        }
        public static Vector2 GetConner(Texture2D tex, Conner conner = Conner.LBottom)
        {

            int M = tex.width;
            int N = tex.height;
            int count = 0;
            Vector2 pos = Vector2.zero;

            Vector2 mark0 = Vector2.zero, mark1 = Vector2.zero;
            switch (conner)
            {
                case Conner.LBottom:
                    mark0 = new Vector2(0f, 1f);
                    mark1 = new Vector2(1f, 0f);
                    break;

                case Conner.LUp:
                    mark0 = new Vector2(0f, 1f);
                    mark1 = new Vector2(1f, 2f);
                    break;

                case Conner.RUp:
                    mark0 = new Vector2(1f, 2f);
                    mark1 = new Vector2(2f, 1f);
                    break;

                case Conner.RBottom:
                    mark0 = new Vector2(2f, 1f);
                    mark1 = new Vector2(1f, 0f);
                    break;
            }

            for (int n = 0; n < N; n++)
            {
                for (int m = 0; m < M; m++)
                {
                    if (count < 1)
                    {
                        if (tex.GetPixel(m, n) == Color.white)
                        {
                            int[,] nhood = new int[3, 3];
                            int sum = 0;
                            for (int s = -1; s <= 1; s++)
                            {
                                for (int t = -1; t <= 1; t++)
                                {
                                    nhood[s + 1, t + 1] = (int)tex.GetPixel(m + s, n + t).grayscale;
                                    sum += nhood[s + 1, t + 1];
                                }
                            }

                            if (sum != 1)
                            {
                                if (nhood[(int)mark0.x, (int)mark0.y] == 0 && nhood[(int)mark1.x, (int)mark1.y] == 0)
                                {
                                    pos = new Vector2(m, n);
                                    count++;
                                }
                            }
                        }
                    }
                }
            }

            return pos;
        }
        public static Vector2[] NhoodSort(Vector2 coordinate, Neighborhood_type nht = Neighborhood_type.Eight, SortDirection sd = SortDirection.CCW)
        {
            Vector2 i0 = new Vector2(-1, -1);
            Vector2 i1 = new Vector2(-1, 0);
            Vector2 i2 = new Vector2(-1, 1);
            Vector2 i3 = new Vector2(0, 1);
            Vector2 i4 = new Vector2(1, 1);
            Vector2 i5 = new Vector2(1, 0);
            Vector2 i6 = new Vector2(1, -1);
            Vector2 i7 = new Vector2(0, -1);

            if (nht == Neighborhood_type.Eight)
            {
                Vector2[] res = new Vector2[8];
                Vector2[] cw = new Vector2[8] { i0, i1, i2, i3, i4, i5, i6, i7 };
                Vector2[] ccw = new Vector2[8] { i0, i7, i6, i5, i4, i3, i2, i1 };
                int mark = 0;
                for (int s = 0; s < 8; s++)
                {
                    res[s] = Vector2.zero;
                }
                switch (sd)
                {
                    case SortDirection.CW:
                        for (int i = 0; i < 8; i++)
                        {
                            if (coordinate == cw[i])
                            {
                                mark = i;
                            }
                        }
                        for (int i = 0, s = mark; i < 8 - mark; i++, s++)
                        {
                            res[i] = cw[s];

                        }
                        for (int i = 0, t = 0; i < mark; i++, t++)
                        {
                            res[8 - mark + i] = cw[t];
                        }
                        break;

                    case SortDirection.CCW:
                        for (int i = 0; i < 8; i++)
                        {
                            if (coordinate == ccw[i])
                            {
                                mark = i;
                            }
                        }
                        for (int i = 0, s = mark; i < 8 - mark; i++, s++)
                        {
                            res[i] = ccw[s];

                        }
                        for (int i = 0, t = 0; i < mark; i++, t++)
                        {
                            res[8 - mark + i] = ccw[t];
                        }
                        break;

                }

                return res;
            }
            else
            {
                Vector2[] res = new Vector2[4];
                Vector2[] cw = new Vector2[4] { i1, i3, i5, i7 };
                Vector2[] ccw = new Vector2[4] { i1, i7, i5, i3 };
                for (int s = 0; s < 4; s++)
                {
                    res[s] = Vector2.zero;
                }
                int mark = 0;

                switch (sd)
                {
                    case SortDirection.CW:
                        for (int i = 0; i < 4; i++)
                        {
                            if (coordinate == cw[i])
                            {
                                mark = i;
                            }
                        }
                        for (int i = 0, s = mark; i < 4 - mark; i++, s++)
                        {
                            res[i] = cw[s];

                        }
                        for (int i = 0, t = 0; i < mark; i++, t++)
                        {
                            res[4 - mark + i] = cw[t];
                        }
                        break;

                    case SortDirection.CCW:
                        for (int i = 0; i < 4; i++)
                        {
                            if (coordinate == ccw[i])
                            {
                                mark = i;
                            }
                        }
                        for (int i = 0, s = mark; i < 4 - mark; i++, s++)
                        {
                            res[i] = ccw[s];

                        }
                        for (int i = 0, t = 0; i < mark; i++, t++)
                        {
                            res[4 - mark + i] = ccw[t];
                        }
                        break;

                }
                return res;
            }
            //int count = 0;
            //foreach (var v in res)
            //{

            //    Debug.Log("res[" + count + "] = " + v);
            //    count++;
            //}
        }
        public static List<Vector2> MooreTracing(Texture2D tex, Neighborhood_type nht = Neighborhood_type.Eight, SortDirection sd = SortDirection.CCW)
        {
            List<Vector2> coordinates = new List<Vector2>();

            Vector2 start = Vector2.zero;
            Vector2 b = Vector2.zero;

            if (!IsBinaryImage(tex))
            {
                Debug.LogError("Texture must be binary image");
                return coordinates;
            }

            start = b = GetConner(tex, Conner.LBottom);
            Vector2 offset = new Vector2(-1, 0);

            #region 1
            bool stop = false;
            int count = 0;
            while (!stop)
            {
                count++;

                Vector2[] sortOffset = NhoodSort(offset, nht, sd);


                for (int i = 0; i < sortOffset.Length; i++)
                {
                    Vector2 t = b + sortOffset[i];
                    if (tex.GetPixel((int)t.x, (int)t.y) == Color.white)
                    {
                        b = new Vector2(t.x, t.y);

                        if (i == 0)
                            offset = sortOffset[sortOffset.Length - 1 - i];
                        else
                            offset = sortOffset[i - 1];

                        coordinates.Add(b);
                        break;
                    }
                }

                if (b == start)
                {
                    stop = true;
                }
            }
            #endregion

            Vector2 first = coordinates[coordinates.Count - 1];
            coordinates.RemoveAt(coordinates.Count - 1);
            coordinates.Insert(0, first);

            return coordinates;
        }
        public static float ShapePerimeter(Texture2D shape, Neighborhood_type nht = Neighborhood_type.Eight)
        {
            float P = 0;
            float d = Sqrt(2f);
            if (!IsBinaryImage(shape))
            {
                Debug.LogError("Texture must be binary image");
                return 0;

            }

            List<Vector2> edges = MooreTracing(shape, nht);
            List<int> fCode = FChainEncode(edges);

            if (nht == Neighborhood_type.Eight)
            {
                for (int i = 0; i < fCode.Count; i++)
                {
                    if ((fCode[i] & 1) == 1)
                        P += d;
                    else
                        P += 1f;
                }
            }
            else
            {
                for (int i = 0; i < fCode.Count; i++)
                {
                    P += 1f;
                }
            }

            return P;
        }
        public static float ShapePerimeter(List<Vector2> edgePos)
        {
            float P = 0;
            float d = Sqrt(2f);
            int type = 0; //0:four connect 1: eight connect

            for (int i = 0; i < edgePos.Count; i++)
            {
                Vector2 offset = Vector2.zero;
                if (i != edgePos.Count - 1)
                    offset = edgePos[i + 1] - edgePos[i];
                else
                    offset = edgePos[0] - edgePos[i];

                if (offset == new Vector2(1f, 1f) || offset == new Vector2(-1f, 1f) || offset == new Vector2(1f, -1f) || offset == new Vector2(-1f, -1f))
                {
                    type = 1;
                    break;
                }
            }



            List<int> fCode = FChainEncode(edgePos);

            if (type == 1)
            {
                for (int i = 0; i < fCode.Count; i++)
                {
                    if ((fCode[i] & 1) == 1)
                        P += d;
                    else
                        P += 1f;
                }
            }
            else
            {
                for (int i = 0; i < fCode.Count; i++)
                {
                    P++;
                }
            }

            return P;
        }
        public static float ShapeArea(Texture2D shape)
        {
            float A = 0;

            if (IsBinaryImage(shape))
            {
                Color[] c = shape.GetPixels();
                for (int i = 0; i < c.Length; i++)
                {
                    A += c[i].grayscale;
                }

            }
            else
            {
                Debug.LogError("Texture must be binary image");
            }
            return A;
        }
        public static List<int> FChainEncode(List<Vector2> pixelCoord)
        {
            List<int> fCode = new List<int>();
            int mark = 0;
            int type = 0; //0:four connect 1: eight connect

            for (int i = 0; i < pixelCoord.Count; i++)
            {
                Vector2 offset = Vector2.zero;
                if (i != pixelCoord.Count - 1)
                    offset = pixelCoord[i + 1] - pixelCoord[i];
                else
                    offset = pixelCoord[0] - pixelCoord[i];

                if (offset == new Vector2(1f, 1f) || offset == new Vector2(-1f, 1f) || offset == new Vector2(1f, -1f) || offset == new Vector2(-1f, -1f))
                {
                    type = 1;
                    break;
                }
            }

            if (type == 1)
            {
                for (int i = 0; i < pixelCoord.Count; i++)
                {
                    Vector2 offset = Vector2.zero;
                    if (i != pixelCoord.Count - 1)
                        offset = pixelCoord[i + 1] - pixelCoord[i];
                    else
                        offset = pixelCoord[0] - pixelCoord[i];


                    if (offset.x == 1f && offset.y == 0f)
                        mark = 0;
                    else if (offset.x == 1f && offset.y == 1f)
                        mark = 1;
                    else if (offset.x == 0f && offset.y == 1f)
                        mark = 2;
                    else if (offset.x == -1f && offset.y == 1f)
                        mark = 3;
                    else if (offset.x == -1f && offset.y == 0f)
                        mark = 4;
                    else if (offset.x == -1f && offset.y == -1f)
                        mark = 5;
                    else if (offset.x == 0f && offset.y == -1f)
                        mark = 6;
                    else if (offset.x == 1f && offset.y == -1f)
                        mark = 7;

                    fCode.Add(mark);
                }
            }
            else
            {
                for (int i = 0; i < pixelCoord.Count; i++)
                {
                    Vector2 offset = Vector2.zero;
                    if (i != pixelCoord.Count - 1)
                        offset = pixelCoord[i + 1] - pixelCoord[i];
                    else
                        offset = pixelCoord[0] - pixelCoord[i];


                    if (offset.x == 1f && offset.y == 0f)
                        mark = 0;
                    else if (offset.x == 0f && offset.y == 1f)
                        mark = 1;
                    else if (offset.x == -1f && offset.y == 0f)
                        mark = 2;
                    else if (offset.x == 0f && offset.y == -1f)
                        mark = 3;


                    fCode.Add(mark);
                }
            }

            return fCode;
        }
        public static List<Vector2> FChainnDecode(List<int> fCode, Vector2 startPos)
        {
            List<Vector2> pixelPos = new List<Vector2>();

            Vector2 offset0 = new Vector2(1f, 0f);
            Vector2 offset1 = new Vector2(1f, 1f);
            Vector2 offset2 = new Vector2(0f, 1f);
            Vector2 offset3 = new Vector2(-1f, 1f);
            Vector2 offset4 = new Vector2(-1f, 0f);
            Vector2 offset5 = new Vector2(-1f, -1f);
            Vector2 offset6 = new Vector2(0f, -1f);
            Vector2 offset7 = new Vector2(1f, -1f);

            int type = 0;

            pixelPos.Add(startPos);

            for (int i = 0; i < fCode.Count; i++)
            {
                if (fCode[i] > 3)
                {
                    type = 1;
                    break;
                }
            }

            if (type == 1)
            {
                for (int i = 0; i < fCode.Count; i++)
                {
                    Vector2 nextPos = Vector2.zero;
                    if (fCode[i] == 0)
                        nextPos = startPos + offset0;
                    else if (fCode[i] == 1)
                        nextPos = startPos + offset1;
                    else if (fCode[i] == 2)
                        nextPos = startPos + offset2;
                    else if (fCode[i] == 3)
                        nextPos = startPos + offset3;
                    else if (fCode[i] == 4)
                        nextPos = startPos + offset4;
                    else if (fCode[i] == 5)
                        nextPos = startPos + offset5;
                    else if (fCode[i] == 6)
                        nextPos = startPos + offset6;
                    else if (fCode[i] == 7)
                        nextPos = startPos + offset7;

                    pixelPos.Add(nextPos);
                    startPos = nextPos;

                }
            }
            else
            {
                for (int i = 0; i < fCode.Count; i++)
                {
                    Vector2 nextPos = Vector2.zero;
                    if (fCode[i] == 0)
                        nextPos = startPos + offset0;
                    else if (fCode[i] == 1)
                        nextPos = startPos + offset2;
                    else if (fCode[i] == 2)
                        nextPos = startPos + offset4;
                    else if (fCode[i] == 3)
                        nextPos = startPos + offset6;

                    pixelPos.Add(nextPos);
                    startPos = nextPos;

                }
            }
            return pixelPos;
        }
        #endregion
    }

}
