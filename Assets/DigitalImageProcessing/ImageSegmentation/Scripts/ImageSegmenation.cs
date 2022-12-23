using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using static DIP.ImageProcessing;
using TMPro;
using static UnityEngine.Mathf;

public class ImageSegmenation : MonoBehaviour
{

    [SerializeField] RawImage image0, image1, image2, image3, image4;
    [SerializeField] Texture2D testTexture;

    Texture2D contentTexture, smoothTex, laplacianTex;
    Texture2D thresholdTex, houghTex;
    Texture2D hisTex, maskTex, maskResTex;

    [SerializeField, Range(1f, 30f)] float a = 10f;
    [SerializeField, Range(0.01f, 1.5f)] float b = 0.1f;

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        smoothTex = GaussianBlur(testTexture, 3, 3, 0.5f, Boundary_Option.zero);
        //laplacianTex = LaplacianAbsNormal(Rgb2Gray(smoothTex));

        image2.texture = Rgb2Gray(testTexture);
        image2.SetNativeSize();

        //float Tmask = OtsuThresh(laplacianTex);
        //float Tmask = GrayThresh(laplacianTex);
        //maskTex = Im2bw(laplacianTex, Tmask / 255f);
        // maskTex = Im2bw(laplacianTex, 0.57f);

        //maskResTex = TextureMul(Rgb2Gray(testTexture), maskTex);
        //maskResTex.Apply();
        //image3.texture = maskResTex;
        //image3.SetNativeSize();


        //float T = OtsuThresh(maskResTex);
        //image0.texture = Im2bw(Rgb2Gray(testTexture), T / 255f);
        image0.texture = testTexture;
        image0.SetNativeSize();

        //Histogram(testTexture, TextureChannel.Gay, out hisTex);
        //image4.texture = hisTex;
        //image4.SetNativeSize();


        //image1.texture = GaussianBlur( LocalThresh(testTexture, 3, 3, a, b),3,3,0.5f,Boundary_Option.zero);
        image1.texture = K_Mean_Clustering(smoothTex, 3);
        image1.SetNativeSize();


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
