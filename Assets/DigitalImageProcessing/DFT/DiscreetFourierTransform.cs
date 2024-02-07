using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DIP.ImageProcessing;
using static UnityEngine.Mathf;
using System;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using System.Numerics;


public class DiscreetFourierTransform : MonoBehaviour
{
    [SerializeField] RawImage image0, image1, image2;
    [SerializeField] Texture2D texture;
    [SerializeField] Button button;
    [SerializeField] RawImage camera;

    

  
    
    Vector2[] P4 = new Vector2[4] { new Vector2(1, 0), new Vector2(2, 0), new Vector2(4, 0), new Vector2(4, 0) };
    Vector2[] P8 = new Vector2[8] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(4, 0), new Vector2(9, 0),
    new Vector2(9, 0), new Vector2(4, 0), new Vector2(1, 0), new Vector2(0, 0)};

    Vector2[] P82 = new Vector2[8] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(4, 0), new Vector2(9, 0),
    new Vector2(5, 0), new Vector2(4, 0), new Vector2(3, 0), new Vector2(1, 0)};


    Vector2[] P2 = new Vector2[2] { new Vector2(1, 0), new Vector2(2, 0) };
    int M, N;


    Complex[] c4 = new Complex[4] { new Complex(1, 0), new Complex(2, 0), new Complex(4, 0), new Complex(4, 0) };


    [SerializeField,Range(0,180f)] float a = 0f;
    int[,] mat = new int[8, 8];

    [SerializeField, Range(1f, 4f)] float n = 1f;

    WebCamTexture web;
    public string dc;

    Texture2D getTex;

    public Resolution re;

    Texture2D contentTexture, smoothTex, laplacianTex, colorTex;
    Texture2D thresholdTex, houghTex;
    Texture2D hisTex, maskTex, maskResTex, ftex;

    int W = 256<<0;
    int H = 256<<0;
    private void Awake()
    {

   
        
    }

    // Start is called before the first frame update
    void Start()
    {
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            dc = devices[0].name;
            web = new WebCamTexture(dc,400,300,12);
            camera.texture = web;
            web.Play();  
        }

        


        //getTex = new Texture2D(256, 256, TextureFormat.RGBA32, false);
        //getTex.SetPixels(web.GetPixels());
        //getTex.Apply();



        //image0.texture = getTex;
        image0.SetNativeSize();

       
        getTex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        colorTex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        colorTex.SetPixels( InitialColor(getTex.width, getTex.height, Color.yellow));

        //button.onClick.AddListener(delegate
        // {
        //     Texture2D originTex = RotateTexture(getTex, a * Mathf.Deg2Rad);
        //     image0.texture = originTex;
        //     image0.SetNativeSize();


        //     Vector2[,] dft = GetDFTDatas(originTex);
        //     dft = FFT2(dft);

        //     int P = dft.GetUpperBound(0) + 1;
        //     int Q = dft.GetUpperBound(0) + 1;

        //     float[,] f = FSpecial(FilterType.gaussian, P, Q,false,24f);
        //     Vector2[,] f_ilpf = ILPF_F(0.2f,P,Q);
        //     Vector2[,] f_glpf = GLPF_F(0.1f, P, Q);
        //     Vector2[,] f_blpf = BLPF_F(0.17f, n, P, Q);

        //     Vector2[,] f_ihpf = IHPF_F(0.03f, P, Q);c
        //     Vector2[,] f_ghpf = GHPF_F(0.05f, P, Q);
        //     Vector2[,] f_bhpf = BHPF_F(0.117f, n, P, Q);

        //     Vector2[,] f_lapla = Laplacian_F(P, Q);

        //     dft = FilterMul(dft, f_lapla);

        //     Vector2[,] laplacians =Laplacian_F_Normal(IFFT2(dft));

        //     Texture2D output = ImIFFT2(laplacians, texture.width, texture.height);


        //     image1.texture = output;
        //     image1.SetNativeSize();

        //     image2.texture = ImFFT2(dft);
        //     image2.SetNativeSize();

        // });



    }


    //private void OnValidate()
    //{
    //    Texture2D originTex = RotateTexture(texture, a * Mathf.Deg2Rad);
    //    image0.texture = originTex;
    //    image0.SetNativeSize();


    //    Vector2[,] dft = GetDFTDatas(originTex);
    //    dft = FFT2(dft);

    //    Texture2D  output = ImIFFT2(IFFT2(dft), texture.width, texture.height);
    //    image1.texture = output;
    //    image1.SetNativeSize();

    //    image2.texture = ImFFT2(dft);
    //    image2.SetNativeSize();
    //}
    // Update is called once per frame
    void Update()
    {


        for (int w = 0; w <W; w++)
        {
            for (int h = 0; h < H; h++)
            {
                getTex.SetPixel(w, h, web.GetPixel(w+(web.width-W)/2,h+(web.height-H)/2));
                //Debug.Log("C =" + c);
            }
        }
        //getTex.SetPixels(web.GetPixels());
        // getTex.Apply();

        //image0.texture = getTex;
        //Debug.Log(getTex.width);
        //image0.SetNativeSize();



        Vector2[,] dft = Convert2Complex(getTex);
        dft = FFT2(dft);
        
        image0.texture = ImFFT2(dft);
        //image2.SetNativeSize();

        smoothTex = GaussianBlur(getTex, 5, 5, 1f, Boundary_Option.zero);
        laplacianTex = ImLaplacian(Rgb2Gray(smoothTex),true,Laplacian_Display.Abs);
        float Tmask = GrayThresh(laplacianTex);
        maskTex = Im2bw(laplacianTex, Tmask);
        maskResTex = MulTex(Rgb2Gray(getTex), maskTex);

        //thresholdTex = K_Mean_Clustering(getTex, 4)
        float T = OtsuThresh(maskResTex);
        thresholdTex = Im2bw(Rgb2Gray(getTex), T);
        ftex = AddTex(thresholdTex, MulTex(laplacianTex, colorTex));
        //float T = OtsuThresh(maskResTex);
        //image2.texture = Im2bw(Rgb2Gray(getTex), 0.32f);
        image2.texture = ftex;


        image1.texture =MulTex(laplacianTex, colorTex);
    }


}
