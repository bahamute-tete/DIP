using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DIP.ImageProcessing;
using System;
using TMPro;
using static UnityEngine.Mathf;

public class ImageEnhance : MonoBehaviour
{
   


    [SerializeField] Texture2D texture;
    Texture2D grayTex, stretchGrayTex, logGrayTex;
    int M => texture.width;
    int N => texture.height;

    [SerializeField, Range(0.1f, 10f)] float E = 4.0f;
    [SerializeField, Range(1f, 10f)] float C = 1f;
    float currentE, currentC;
    float mean = 0; float sig = 0;

    TextureChannel channel;
    [SerializeField] RawImage raw1, raw2, raw3, raw4;
    
    private void Awake()
    {
        grayTex = new Texture2D(M, N, TextureFormat.ARGB32, false);
        grayTex =Rgb2Gray(texture);



        currentE = E;
        channel = TextureChannel.Gay;
        HistogramStatistics(Histogram(texture, channel), out mean, out sig);
        stretchGrayTex = new Texture2D(M, N, TextureFormat.ARGB32, false);

        stretchGrayTex = StretchTransform(grayTex, mean/255f);
        logGrayTex = LogTransform(grayTex);

        raw1.texture = texture;
        raw2.texture = grayTex;
        raw3.texture = stretchGrayTex;
        raw4.texture = logGrayTex;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (E != currentE)
        {
            stretchGrayTex = StretchTransform(grayTex, mean / 255f, E);
            raw3.texture = stretchGrayTex;
            currentE = E;
        }

        if (C != currentC)
        {
            logGrayTex = LogTransform(grayTex,C);
            raw4.texture = logGrayTex;
            currentC = C;
        }

    }
}
