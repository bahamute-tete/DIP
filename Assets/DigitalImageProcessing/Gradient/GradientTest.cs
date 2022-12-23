using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DIP.ImageProcessing;
using static UnityEngine.Mathf;
using UnityEngine.UI;
using System;

public class GradientTest : MonoBehaviour
{
    [SerializeField] RawImage image0, image1, image2;
    [SerializeField] Texture2D tex;
    Texture2D blurTex, amplitudeTex, dirctionTex, testValueTex;

    [SerializeField] Button btn;

    int[,] SE0 = new int[5, 5] {    {2,3,3,3,2},
                                    {4,1,1,1,6},
                                    {4,1,1,1,6},
                                    {4,1,1,1,6},
                                    {2,5,5,5,2}};

    int[,] SE1 = new int[3, 3] { {1,1,1},
                                 {1,1,1},
                                 {1,1,1}};

    [SerializeField,Min(0f)] float T0 = 0;
    [SerializeField, Min(0f)] float T1 = 0;
    [SerializeField,Min(0.1f)] float sig = 1f;

     enum EdgeCheck { Sobel = 0, Prewitt, LoG, Canny }
    [SerializeField] EdgeCheck method;
    
    int r = 5;
    int c = 5;

    //private void Awake()
    //{
    //    int[,] padd = PaddingDatas(SE0, r, c,Boundary_Option.replicate);

    //    for (int y = 0; y <= padd.GetUpperBound(1); y++)
    //    {
    //        for (int x = 0; x <= padd.GetUpperBound(0); x++)
    //        {
    //            Debug.Log("padd[" + x + "][" + y + "] " + padd[x, y]);
    //        }
    //    }
    //}


    // Start is called before the first frame update
    void Start()
    {

      

        image0.texture = tex;
        image0.SetNativeSize();
       // amplitudeTex = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
        //dirctionTex = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
       // testValueTex = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);

        float[,] lapValues = new float[tex.width, tex.height];
        float[,] lapValuespad = new float[tex.width+2, tex.height+2];

       

        btn.onClick.AddListener(delegate
        {
            Texture2D outtex = new Texture2D(tex.width, tex.height);

            switch (method)
            {
                case EdgeCheck.Sobel:
                     outtex = Edge(tex, Edge_Method.Sobel, T0,sig);
                    break;

                case EdgeCheck.Prewitt:
                    outtex = Edge(tex, Edge_Method.Prewitt, T0, sig);
                    break;

                case EdgeCheck.LoG:
                    outtex = Edge(tex, Edge_Method.LoG, T0, sig);
                    break;

                case EdgeCheck.Canny:
                    outtex = Edge(tex, Edge_Method.Canny, T0,T1, sig);
                    break;
            }


           
            //for (int y = 0; y < amp.GetUpperBound(1)+1; y++)
            //{
            //    for (int x = 0; x < amp.GetUpperBound(0)+1; x++)
            //    {
            //        Color c = new Color(amp[x, y], amp[x, y], amp[x, y]);
            //        sobelTex.SetPixel(x, y,c );
            //    }
            //}
            //sobelTex.Apply();
            //Texture2D prewittlTex = Edge(blurTex, Edge_Method.Prewitt,T0);
            
            //Texture2D LoGTex = Edge(tex, Edge_Method.LoG, T0,sig);





            image1.texture = outtex;
            image1.SetNativeSize();

            //image2.texture = sobelTex;
            //image2.SetNativeSize();



            //DebugFilter(dir);
        });

    }

    private static void DebugFilter(float[,] f)
    {
        for (int y = 0; y <= f.GetUpperBound(1); y++)
        {
            for (int x = 0; x <= f.GetUpperBound(0); x++)
            {
                Debug.Log("f[" + x + "][" + y + "]= " + f[x, y]);
            }
        }
    }

    private static void DebugFilter(Vector2[,] f)
    {
        for (int y = 0; y <= f.GetUpperBound(1); y++)
        {
            for (int x = 0; x <= f.GetUpperBound(0); x++)
            {
                //Debug.Log("f[" + x + "][" + y + "]= " + f[x, y]);
                Debug.Log("f[x,y] =" + f[x, y]);
            }
        }
    }




}
