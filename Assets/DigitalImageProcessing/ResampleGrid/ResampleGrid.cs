using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using static DIP.ImageProcessing;
using System;

public class ResampleGrid : MonoBehaviour
{

    [SerializeField] RawImage image0, image1, image2;
    [SerializeField] Texture2D texture;

    [SerializeField] int grid = 10;

    [SerializeField] Button button, button2;
    Texture2D sampleTex;
    int M ;
    int N ;

    [SerializeField] bool stop = false;
    [SerializeField]List<Vector2> coordinate = new List<Vector2>();

    private void Awake()
    {
        M = texture.width;
        N = texture.height;
        sampleTex = new Texture2D(M, N, TextureFormat.RGB24, false);

       
    }
    // Start is called before the first frame update
    void Start()
    {


        texture = Im2bw(Rgb2Gray(texture),0.5f);
        image0.texture = texture;

        //Vector2 pos = Vector2.zero;





        Vector2 c = Vector2.zero;
        Vector2 b = Vector2.zero;
        Vector2 first = Vector2.zero;


        first = GetConner(texture,Conner.LBottom);
        b = GetConner(texture,Conner.LBottom);
        Vector2 offset = new Vector2(-1, 0);


        //////////////////////////////////////////////////////////////

        int[,] SE = new int[3, 3] {   {1,1,1 },
                                      {1,1,1 },
                                      {1,1,1 }};

        //Color[] black = InitialColor(M, N, Color.black);
        //Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
        //output.SetPixels(0, 0, M, N, black);
        //Texture2D erroTex = new Texture2D(M, N, TextureFormat.RGB24, false);
        //erroTex.SetPixels(0, 0, M, N, black);

        int[,] id = new int[M, N];
        for (int n = 0; n < N; n++)
        {
            for (int m = 0; m < M; m++)
            {
                id[m, n] = 0;
            }
        }

        int mark = 1;

        Texture2D output = new Texture2D(M, N, TextureFormat.RGB24, false);
        //bool isEnd = false;

        //int sm = 0;
        //int sn = 0;
        //int Loop = 0;
        //for (int n = 0; n < N; n++)
        //{
        //    for (int m = 0; m < M; m++)
        //    {
        //        if (texture.GetPixel(m, n).grayscale == 1 && Loop<1)
        //        {
        //            sm = m;
        //            sn = n;
        //            Loop++;
        //        }

        //    }
        //}

        //output.SetPixel(sm, sn, Color.white);
        //id[sm, sn] = fgMark;

        //erroTex = MulTex(texture, output);
        //int whitePixelNum = 0;
        //for (int n = 0; n < N; n++)
        //{
        //    for (int m = 0; m < M; m++)
        //    {
        //        if (erroTex.GetPixel(m, n) == Color.white)
        //            whitePixelNum++;
        //    }
        //}

        //image2.texture = erroTex;


        ///////////////////////////////////////////////////////////////////////////////
        //button.onClick.AddListener(delegate
        //{

        //    //coordinate.Clear();
        //    //coordinate = MooreTracing(texture, SortDirection.CW);


        //    //sampleTex = BWLable8(texture);
        //    //sampleTex = BWLable4(texture);

        //    if (!isEnd)
        //    {
        //        int whitePixelNum_Dilation = 0;

        //        output = ImMorphology(output, SE, Image_Type.BlackWhite, Morphological_Operator.Dilation);
        //        output = MulTex(texture, output);


        //        Debug.Log("whitePixelNum =" + whitePixelNum);
        //        for (int n = 0; n < N; n++)
        //        {
        //            for (int m = 0; m < M; m++)
        //            {

        //                if (output.GetPixel(m, n) == Color.white)
        //                {
        //                    id[m, n] = fgMark;
        //                    whitePixelNum_Dilation++;
        //                    Debug.Log("whitePixelNum_Dilation =" + whitePixelNum_Dilation);
        //                }
        //            }
        //        }



        //        if (whitePixelNum != whitePixelNum_Dilation)
        //        {
        //            whitePixelNum = whitePixelNum_Dilation;
        //        }
        //        else
        //        {
        //            fgMark++;
        //            isEnd = true;
        //            Debug.Log("End!!! ");

        //        }
        //    }



        //    //if (isEnd)
        //    //{
        //    //    Texture2D checkTex = SubTex(texture, output);
        //    //    Color[] colors = checkTex.GetPixels(0, 0, M, N);
        //    //    if (Array.Exists(colors, (x) => x.Equals(Color.white)))
        //    //    {

        //    //    }
        //    //}

        //    //image1.texture = output;

        //});

        button2.onClick.AddListener(delegate
        {
            id = ConnectedComponentLable(texture,Neighborhood_type.Four);
            int[] id_1D = new int[texture.width * texture.height];
            

            for (int n = 0,i =0; n < N; n++)
            {
                for (int m = 0; m < M; m++,i++)
                {
                    id_1D[i] = id[m, n];  
                }
            }

            int max = Max(id_1D);
            Color[] colors = new Color[max];
            for (int i = 0; i < max; i++)
            {
                colors[i] = UnityEngine.Random.ColorHSV(0f, 1f);
            }


            for (int n = 0; n < N; n++)
            {
                for (int m = 0; m < M; m++)
                {
                    for (int i = 0; i < max; i++)
                    {
                        if (id[m, n] == i + 1)
                            output.SetPixel(m, n, colors[i]);
                    }
                }
            }
            output.Apply();
            image1.texture = output;
        });

    }
















   

}
