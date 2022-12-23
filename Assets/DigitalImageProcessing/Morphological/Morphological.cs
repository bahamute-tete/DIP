using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DIP.ImageProcessing;
using static UnityEngine.Mathf;
using System;


public class Morphological : MonoBehaviour
{
    [SerializeField] RawImage image0, image1, image2;
    [SerializeField]Texture2D testTexture;
    [SerializeField] Texture2D testTexture2;
    int M => testTexture.width;
    int N => testTexture.height;

    Texture2D paddingTex;
    Texture2D outputTex;

    [SerializeField] Button btn, btn2;

    int[,] SE0 = new int[5, 5] {    {1,1,1,1,1},
                                    {1,1,1,1,1},
                                    {1,1,1,1,1},
                                    {1,1,1,1,1},
                                    {1,1,1,1,1}};


    int[,] SE1 = new int[3, 3] { {1,1,1},
                                 {1,1,1},
                                 {1,1,1}};


    int[,] SE2 = new int[3, 3] {  {1,1,1},
                                  {1,0,1},
                                  {1,1,1}};



    int[,] SE3 = new int[3, 3] {  {0,1,0},
                                  {1,1,1},
                                  {0,1,0}};

    int[,] SE4 = new int[3, 3] { {1,1,1},
                                 {1,0,1},
                                 {1,1,1}};

    int[,] SE5 = new int[3, 3] { {1,2,2},
                                 {1,0,2},
                                 {1,2,2}};

    int[,] SE6 = new int[3, 3] { {1,1,1},
                                 {2,0,2},
                                 {2,2,2}};

    int[,] SE7 = new int[3, 3] { {2,2,1},
                                 {2,0,1},
                                 {2,2,1}};

    int[,] SE8 = new int[3, 3] { {2,2,2},
                                 {2,0,2},
                                 {1,1,1}};

    int[,] SE9 = new int[3, 3] { {1,4,7},
                                 {2,5,8},
                                 {3,6,9}};



    private void Awake()
    {
     // Debug.Log(  AND(SE,T3));

    }

   
    // Start is called before the first frame update
    void Start()
    {
        float T = GrayThresh(testTexture);
        //Texture2D tex = Im2bw(testTexture, 0.5f);
        Texture2D tex =Rgb2Gray( testTexture);
        Texture2D masktex = ComplementTex(tex);
        Texture2D signature = new Texture2D(testTexture.width, testTexture.height, TextureFormat.RGB24, false);
        Texture2D sk = new Texture2D(testTexture.width, testTexture.height, TextureFormat.RGB24, false);


        for (int m = 0; m < testTexture.width; m++)
        {
            for (int n = 0; n < testTexture.height; n++)
            {
                signature.SetPixel(m, n, Color.black);
                signature.SetPixel(0, n, Color.white);
                signature.SetPixel(testTexture.width-1,n, Color.white);
                signature.SetPixel(m, testTexture.height-1, Color.white);
                signature.SetPixel(m, 0, Color.white);
            }
        }

        //signature.Apply();
        image0.texture = tex;
        image0.SetNativeSize();
        # region 1
        //btn.onClick.AddListener(delegate
        //{


        //    //Debug.Log("T ==" + T);
        //    // tex = ImMorphology(tex, SE1, Image_Type.BlackWhite, Morphological_Operator.Erosion);
        //    // tex = ImMorphology(tex, SE1,Image_Type.BlackWhite, Morphological_Operator.Dilation);
        //    //tex = ImMorphology(tex, SE1,Image_Type.BlackWhite, Morphological_Operator.Opening);
        //    //tex = ImMorphology(tex, SE1,Image_Type.BlackWhite, Morphological_Operator.Closing);
        //    //tex = ImMorphology(tex, SE4,Image_Type.BlackWhite, Morphological_Operator.HMT);

        //    //tex = GeodesicDilation(tex, testTexture2);

        //    //signature = ImMorphology(signature, SE1, Morphological_Operator.Dilation);
        //    //signature = MulTex(signature, masktex);
        //    //tex = AddTex(testTexture, testTexture2);
        //    // tex= ImFillHole(testTexture);
        //    //tex= ImClearBoundary(testTexture, 2);
        //    //Texture2D er0 = tex;
        //    //Texture2D op0 = ImMorphology(er0, SE1, Morphological_Operator.Opening);
        //    //Texture2D sk0 = SubTex(er0, op0);

        //    //Texture2D er1 = ImMorphology(er0, SE1, Morphological_Operator.Erosion);
        //    //Texture2D op1 = ImMorphology(er1, SE1, Morphological_Operator.Opening);
        //    //Texture2D sk1 = SubTex(er1, op1);

        //    //Texture2D er2 = ImMorphology(er1, SE1, Morphological_Operator.Erosion);
        //    //Texture2D op2 = ImMorphology(er2, SE1, Morphological_Operator.Opening);
        //    //Texture2D sk2 = SubTex(er2, op2);
        //    //Texture2D sk = AddTex(AddTex(sk0, sk1), sk2);


        //    //sk = ImSkeleton(testTexture);

        //    //tex = ImMorphology(tex, SE1,Image_Type.Grayscale, Morphological_Operator.Closing);

        //    Texture2D dilationTex = ImMorphology(tex, SE1, Image_Type.Grayscale, Morphological_Operator.Dilation);
        //    Texture2D erosionTex = ImMorphology(tex, SE1, Image_Type.Grayscale, Morphological_Operator.Erosion);
        //    Texture2D openingTex = ImMorphology(tex, SE1, Image_Type.Grayscale, Morphological_Operator.Opening);
        //    Texture2D closingTex = ImMorphology(tex, SE1, Image_Type.Grayscale, Morphological_Operator.Closing);
        //    Texture2D grand = SubTex(dilationTex, erosionTex);

        //    Texture2D That = SubTex(tex, openingTex);
        //    Texture2D Bhat = SubTex(closingTex, tex);

        //    image1.texture = That;
        //    image1.SetNativeSize();

        //    image2.texture = Bhat;
        //    image2.SetNativeSize();

        //});


        //btn2.onClick.AddListener(delegate
        //{
        //    Texture2D output;
        //    output = ImMorphology(testTexture, SE8, Image_Type.BlackWhite, Morphological_Operator.HMT);
        //    image1.texture = output;
        //    image1.SetNativeSize();


        //});

        //btn2.onClick.AddListener(delegate
        //{

        //    signature = ComplementTex(signature);
        //    signature = MulTex(signature, masktex);

        //    image2.texture = signature;
        //    image2.SetNativeSize();

        //});
      #endregion


        btn2.onClick.AddListener(delegate
        {
            Texture2D output=new Texture2D(testTexture.width, testTexture.height, TextureFormat.RGB24, false);

            output = ConvexHullSimple(testTexture);
            image1.texture = output;
            image1.SetNativeSize();


        });



        //outputTex = Im2bw(testTexture, 0.5f);
        //outputTex.Apply();


        //image0.texture = Im2bw(testTexture, 0.5f);


        btn.onClick.AddListener(delegate
        {
            Texture2D output = Thinning(testTexture);



            image1.texture = output;
            image1.SetNativeSize();

        });



    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void RotateF(int[,] f)
    {
        int r = f.GetUpperBound(0) + 1;
        int c = f.GetUpperBound(1) + 1;

        int[,] res = new int[r, c];

        //res = Transpos(f);//ccw90
         res = CenterSymmetry(f);//cc180
        //res = CenterSymmetry(res);//ccw270//cw90

        for (int y = 0; y < c; y++)
        {
            for (int x = 0; x < r; x++)
            {
                Debug.Log("res[" + x + "][" + y + "]= " + res[x, y]);
            }
        }


    }

    int[,] Transpos(int[,] f)
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
    int[,] CenterSymmetry(int[,] f)
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
}
