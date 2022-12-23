using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using TMPro;
using System;
using DIP;

public class GrayHistogram : MonoBehaviour
{
    static float pi = 3.1415926f;

    [SerializeField] Texture2D inputTexture, inputTexture2;
    Texture2D outputTextureR, outputTextureG, outputTextureB, outputTextureL, outputTextureEq;

    [SerializeField] int bit;
    

    

    [SerializeField] Material barMat, xAxieMat, yAxieMat;
    [SerializeField] Color rCol, gCol, bCol, lCol, equalizeCol;

    public enum ImageChannel {Red=0,Green,Blue,Luminance}
    [SerializeField] ImageChannel imageChannel;
   
    
    List<float[]>  imagelevels = new List<float[]>();
    List<float[,]> imageValues = new List<float[,]>();

    //RenderTexture rt;
    int index = 4;

    [SerializeField] RawImage inputImage;
    [SerializeField] RawImage outputImageR;
    [SerializeField] RawImage outputImageG;
    [SerializeField] RawImage outputImageB;
    [SerializeField] RawImage outputImageL;
    [SerializeField] RawImage outputImageEq;
    [SerializeField] TMP_Dropdown channel;

    [SerializeField] Text exTextR, varTextR;
    [SerializeField] Text exTextG, varTextG;
    [SerializeField] Text exTextB, varTextB;
    [SerializeField] Text exTextL, varTextL;
    [SerializeField] Text exTexteq, varTexteq;
    bool isChange =false;

    #region Histogram parameter

    uint[] histValuetest = new uint[256];

    uint[] histValueR = new uint[256];
    uint[] histValueG = new uint[256];
    uint[] histValueB = new uint[256];
    uint[] histValueL = new uint[256];

    float exR = 0f, exG = 0f, exB = 0f, exL = 0f, exEq = 0f;
    float sigR = 0f, sigG = 0f, sigB = 0f, sigL = 0f, sigEq = 0f;

    float extest = 0f;
    float sigtest = 0f;

    List<uint[]> ChannelDatas = new List<uint[]>();

    [SerializeField] GameObject barPrefab;
    GameObject[] bars;

    GameObject content;
    #endregion

    GameObject guassConten;
    LineRenderer guassLr;
    float[] twoGuassianRes = new float[256];
    int[] twoGuassianResInt = new int[256];
    const string formatter = "{0,25}{1,30}";
    void Start()
    {



        guassConten = new GameObject("GuassContent");
        twoGuassianRes = TwoModeGuassian(0.15f, 0.05f, 0.75f, 0.05f, 1f, 0.07f, 0.002f);
        
        foreach (var g in twoGuassianRes)
        {
           // Debug.Log("res ==" +ToInt(g));
        }
        DrawHistogram2(twoGuassianRes, guassConten.transform);
        //guassConten.SetActive(false);

        histValuetest = Historgram(inputTexture2, 3, out extest, out sigtest);

       content = new GameObject("Content");

        DropDownSet();

        outputTextureR = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.ARGB32, false);
        outputTextureG = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.ARGB32, false);
        outputTextureB = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.ARGB32, false);
        outputTextureL = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.ARGB32, false);
        outputTextureEq = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.ARGB32, false);


        #region  Histogram
        histValueR = Historgram(inputTexture, 0, out exR, out sigR);
        histValueG = Historgram(inputTexture, 1, out exG, out sigG);
        histValueB = Historgram(inputTexture, 2, out exB, out sigB);
        histValueL = Historgram(inputTexture, 3, out exL, out sigL);
        ChannelDatas.Add(histValueR); ChannelDatas.Add(histValueG); ChannelDatas.Add(histValueB); ChannelDatas.Add(histValueL);

        //DrawHistogram(histValueG, content.transform);


        exTextR.text = "E(x)= " + exR.ToString("#.##");
        varTextR.text = "SD(x)= " + sigR.ToString("#.##");

        exTextG.text = "E(x)= " + exG.ToString("#.##");
        varTextG.text = "SD(x)= " + sigG.ToString("#.##");

        exTextB.text = "E(x)= " + exB.ToString("#.##");
        varTextB.text = "SD(x)= " + sigB.ToString("#.##");

        exTextL.text = "E(x)= " + exL.ToString("#.##");
        varTextL.text = "SD(x)= " + sigL.ToString("#.##");

        #endregion


        #region Histeq

        float[,] eqColorR = new float[inputTexture.width, inputTexture.height];
        float[,] eqColorG = new float[inputTexture.width, inputTexture.height];
        float[,] eqColorB = new float[inputTexture.width, inputTexture.height];
        float[,] eqColorL = new float[inputTexture.width, inputTexture.height];
        //eqColorR = Histeq(inputTexture, 0);
        //eqColorG = Histeq(inputTexture, 1);
        //eqColorB = Histeq(inputTexture, 2);
        //eqColorL = Histeq(inputTexture, 3);

        //eqColorR = HisteqMap(inputTexture, twoGuassianRes, 0);
        //eqColorG = HisteqMap(inputTexture, twoGuassianRes, 1);
        //eqColorB = HisteqMap(inputTexture, twoGuassianRes,2);
        eqColorL = HisteqMap(inputTexture, twoGuassianRes, 3,10);
        //eqColorL = HisteqMap2(inputTexture, inputTexture2, 3);

        for (int m = 0; m < 64; m++)
        {
            for (int n = 0; n < 64; n++)
            {
                //Debug.Log(eqColorL[m, n]);
            }
        }

        Texture2D eqTexture = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.ARGB32, false);

        eqTexture = ImShow(outputTextureEq, eqColorL, eqColorL, eqColorL);

        uint[] histValueLEq = new uint[256];
        histValueLEq = Historgram(eqTexture, 0, out exEq, out sigEq);

        exTexteq.text = "E(x)= " + exEq.ToString("#.##");
        varTexteq.text = "SD(x)= " + sigEq.ToString("#.##");
        #endregion

        #region UI_dropdown

        channel.onValueChanged.AddListener(delegate
        {
            ClearBar();
            switch (channel.value)
            {
                case 0:
                    DrawHistogram(histValueR, content.transform);
                    break;
                case 1:
                    DrawHistogram(histValueG, content.transform);
                    break;
                case 2:
                    DrawHistogram(histValueB, content.transform);
                    break;
                case 3:
                    DrawHistogram(histValueL, content.transform);
                    break;
                case 4:
                    DrawHistogram(histValueR, content.transform, 0.001f);
                    DrawHistogram(histValueG, content.transform, 0.003f);
                    DrawHistogram(histValueB, content.transform, 0.005f);
                    break;
                case 5:
                    DrawHistogram(histValueLEq, content.transform);
                    break;
            }
        });
        #endregion

        #region Set RGBL and HistEq image

        float[,] cr = GetTextureValue(inputTexture, 0);
        float[,] cg = GetTextureValue(inputTexture, 1);
        float[,] cb = GetTextureValue(inputTexture, 2);
        float[,] cl = GetTextureValue(inputTexture, 3);

        for (int m = 0; m < inputTexture.width; m++)
        {
            for (int n = 0; n < inputTexture.height; n++)
            {
                float r = cr[m, n];
                float g = cg[m, n];
                float b = cb[m, n];
                float l = cl[m, n];

                Color rColor = new Color(r, r, r, 1f);
                Color gColor = new Color(g, g, g, 1f);
                Color bColor = new Color(b, b, b, 1f);
                Color lColor = new Color(l, l, l, 1f);

                outputTextureR.SetPixel(m, n, rColor);
                outputTextureG.SetPixel(m, n, gColor);
                outputTextureB.SetPixel(m, n, bColor);
                outputTextureL.SetPixel(m, n, lColor);
            }
        }

        outputTextureR.Apply();
        outputTextureG.Apply();
        outputTextureB.Apply();
        outputTextureL.Apply();

        inputImage.texture = inputTexture;
        outputImageR.texture = outputTextureR;
        outputImageG.texture = outputTextureG;
        outputImageB.texture = outputTextureB;
        outputImageL.texture = outputTextureL;
        outputImageEq.texture = eqTexture;
        #endregion
    }


   
    // Update is called once per frame
    void Update()
    {
       
    }


    void ClearBar()
    {
        if (content != null && content.transform.childCount != 0)
        {
            for (int i = 0; i < content.transform.childCount; i++)
            {
                GameObject temp = content.transform.GetChild(i).gameObject;
                Destroy(temp);
            }
        }
    }
    private void DropDownSet()
    {
        TMP_Dropdown.OptionData optionData0 = new TMP_Dropdown.OptionData();
        optionData0.text = "Gray";
        TMP_Dropdown.OptionData optionData1 = new TMP_Dropdown.OptionData();
        optionData1.text = "RGB";
        TMP_Dropdown.OptionData optionData2 = new TMP_Dropdown.OptionData();
        optionData2.text = "Eq";

        List<TMP_Dropdown.OptionData> optionDatas = new List<TMP_Dropdown.OptionData>();
        optionDatas.Add(optionData0); optionDatas.Add(optionData1); optionDatas.Add(optionData2);

        channel.AddOptions(optionDatas);

        channel.options[0].text = "Red";
        channel.options[1].text = "Green";
        channel.options[2].text = "Blue";
        channel.value = 1;
    }


    GameObject[] HistogramBarSpawn(Transform panrent, int L = 256)
    {
        GameObject[] bars = new GameObject[L];
     
        for (int i = 0; i < L; i++)
        {
            GameObject temp = Instantiate(barPrefab, Vector3.zero, Quaternion.identity, panrent);
            temp.transform.position = new Vector3(i, 0, 0);
            float lineWidth = (1f / 256f) * 0.8f;
            LrSet(temp, 2, lineWidth, Color.gray);
            bars[i] = temp;
        }
        return bars;
    }
    void DrawHistogram2(float[] p, Transform panrent, float offset = 0, int L = 256)
    {
       
        GameObject[] bars = new GameObject[L];

        for (int i = 0; i < L; i++)
        {
            GameObject temp = Instantiate(barPrefab, Vector3.zero, Quaternion.identity, panrent);
            temp.transform.position = new Vector3(i, 0, 0);
            float lineWidth = (1f/256f ) * 0.8f;
            LrSet(temp, 2, lineWidth, Color.gray);
            bars[i] = temp;
        }


        for (int i = 0; i < L; i++)
        {
            SetPos(bars[i], i/255f , p[i], offset);
        }

    }
    void DrawHistogram( uint[] levels, Transform panrent, float offset = 0,int L =256)
    {
        /// <summary>
        /// Draw Histogram  with channle 
        /// Panrent is a gameobject like a content 
        /// offset is a parameter that offsetZ axis
        /// </summary>
        /// 

        GameObject[] bars = new GameObject[L];

        for (int i = 0; i < L; i++)
        {
            GameObject temp = Instantiate(barPrefab, Vector3.zero, Quaternion.identity, panrent);
            temp.transform.position = new Vector3(i, 0, 0);
            float lineWidth = (1f / 256f) * 0.8f;
            LrSet(temp, 2, lineWidth, Color.gray);
            bars[i] = temp;
        }


        for (int i = 0; i < L; i++)
        {
            SetPos(bars[i], i /255f,  levels [i]/255f*(100f/256f), offset);
        }

    }
    void LrSet(GameObject bar, int count, float lineWidth, Color color)
    {
        LineRenderer lr = bar.GetComponent<LineRenderer>();
        lr.positionCount = count;
        lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        lr.startWidth = lr.endWidth = lineWidth;
        lr.startColor = lr.endColor = color;
    }
    void SetPos(GameObject bar, float horizonShift, float verticalShift, float depthShift = 0f)
    {
        bar.GetComponent<LineRenderer>().SetPosition(0, new Vector3(horizonShift, 0, depthShift));
        bar.GetComponent<LineRenderer>().SetPosition(1, new Vector3(horizonShift, verticalShift, depthShift));
    }


    Texture2D ImShow(Texture2D textureContent, float[ ,] channelR, float[,] channelG, float[,] channelB)
    {

        for (int m = 0; m < inputTexture.width; m++)
        {
            for (int n = 0; n < inputTexture.height; n++)
            {


                Color resColor = new Color(channelR[m, n], channelG[m, n], channelB[m, n], 1f);
                //Color rrr =new Color( Rgb2Gray(resColor), Rgb2Gray(resColor), Rgb2Gray(resColor));
                textureContent.SetPixel(m, n, resColor);

            }
        }

        textureContent.Apply();
        return textureContent;
    }

    uint[] Historgram(Texture2D texture,int index, out float ex ,out float sig)
    {
        /// <summary>
        /// Return  value of  histogram  a channel which you want.
        /// index [0,1,2,3] = [red ,green ,blue, gray]. 
        /// ex = Mean Value 
        /// sig = Standard Deviation
        /// </summary>


        int M = texture.width;
        int N = texture.height;
        int L = 256;
        ex = 0f;sig = 0f;

        float[] level_RChannel = new float[M * N];
        float[] level_GChannel = new float[M * N];
        float[] level_BChannel = new float[M * N];
        float[] level_LChannel = new float[M * N];

        for (int m = 0, i = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++, i++)
            {
                Color rgba = texture.GetPixel(m, n);

                level_RChannel[i] = rgba.r;
                level_GChannel[i] = rgba.g;
                level_BChannel[i] = rgba.b;
                level_LChannel[i] = Rgb2Gray(rgba);
            }
        }



        uint[] levelsR = new uint[L];
        uint[] levelsG = new uint[L];
        uint[] levelsB = new uint[L];
        uint[] levelsL = new uint[L];

        uint[] pixelValueR = new uint[M * N];
        uint[] pixelValueG = new uint[M * N];
        uint[] pixelValueB = new uint[M * N];
        uint[] pixelValueL = new uint[M * N];

        //initialize
        for (int i = 0; i < L; i++)
        {
            levelsR[i] = 0;
            levelsG[i] = 0;
            levelsB[i] = 0;
            levelsL[i] = 0;
        }

        for (int i = 0; i < M * N; i++)
        {
            pixelValueR[i] = (uint)(level_RChannel[i] * (L - 1) + 0.5f);
            pixelValueG[i] = (uint)(level_GChannel[i] * (L - 1) + 0.5f);
            pixelValueB[i] = (uint)(level_BChannel[i] * (L - 1) + 0.5f);
            pixelValueL[i] = (uint)(level_LChannel[i] * (L - 1) + 0.5f);
        }

        for (int i = 0; i < M * N; i++)
        {
            levelsR[pixelValueR[i]] += 1;
            levelsG[pixelValueG[i]] += 1;
            levelsB[pixelValueB[i]] += 1;
            levelsL[pixelValueL[i]] += 1;
        }


        float exR = 0f, exG = 0f, exB = 0f, exL = 0f;
        float sigR = 0f, sigG = 0f, sigB = 0f, sigL = 0f;
        float[] exes = new float[4];
        float[] sigs = new float[4];


        float[] fvalueR = new float[L];
        float[] fvalueG = new float[L];
        float[] fvalueB = new float[L];
        float[] fvalueL = new float[L];

        for (int i = 0; i < L; i++)
        {
           // Debug.Log("levelsR[" + i + "] = " + levelsR[i]);
            fvalueR[i] = levelsR[i] / (float)(M * N);
            fvalueG[i] = levelsG[i] / (float)(M * N);
            fvalueB[i] = levelsB[i] / (float)(M * N);
            fvalueL[i] = levelsL[i] / (float)(M * N);
        }

        for (int i = 0; i < L; i++)
        {
            exR += i * fvalueR[i];
            //Debug.Log("exR ==" + exR);
            exG += i * fvalueG[i];
            exB += i * fvalueB[i];
            exL += i * fvalueL[i];
        }
        exes[0] = exR; exes[1] = exG; exes[2] = exB; exes[3] = exL;
        ex = exes[index];

        for (int i = 0; i < L; i++)
        {
            sigR += (i - exR) * (i - exR) * fvalueR[i];
            sigG += (i - exG) * (i - exG) * fvalueG[i];
            sigB += (i - exB) * (i - exB) * fvalueB[i];
            sigL += (i - exL) * (i - exL) * fvalueL[i];
        }
        sigs[0] = sigR; sigs[1] = sigG;  sigs[2] = sigB; ; sigs[3] = sigL;
        sig = Sqrt( sigs[index]);


        List<uint[]> level_Channels = new List<uint[]>();
        level_Channels.Add(levelsR);
        level_Channels.Add(levelsG);
        level_Channels.Add(levelsB);
        level_Channels.Add(levelsL);

        return level_Channels[index];
    }

    float[,] Histeq(Texture2D texture,int index,float scale =1)
    {

        /// <summary>
        /// This funtion return one of 4 channels that is 2Dim Array Color[M,N] ( M = width,N = height) 
        /// which is the result of histogram equalization.
        /// channleValue is  value of  histogram  a channel which you want.
        /// index [0,1,2,3] = [red ,green ,blue, gray]. 
        /// </summary>

        int M = texture.width;
        int N = texture.height;
        uint[,] channelUint8 = new uint[M, N];
        channelUint8 = Im2Uint8(texture)[index];



        int[] s = new int[256];
       
        float[] cumsum = new float[256];

        uint[] channelValue = new uint[256];

        float ex = 0f, sig = 0f;
        channelValue = Historgram(texture, index, out ex, out sig);
        cumsum=CumulativeHistogram(M, N, channelValue);
        s=ToInt(cumsum);


        float[,] resColor = new float[M, N];
        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                uint res = 0;
                float fres = 0;

                for (int i = 0; i < 256; i++)
                {
                    uint graylevel = channelUint8[m, n];
                    res = (uint)s[graylevel];
                    fres = scale * (res / 255f);
                }
                resColor[m, n] = fres;
            }
        }
        return resColor;
    }
    float[,] HisteqMap(Texture2D texture,float[] mapDatas, int index, float scale = 1)
    {

        /// <summary>
        /// This funtion return one of 4 channels that is 2Dim Array Color[M,N] ( M = width,N = height) 
        /// which is the result of histogram equalization.
        /// channleValue is  value of  histogram  a channel which you want.
        /// index [0,1,2,3] = [red ,green ,blue, gray]. 
        /// </summary>

        int M = texture.width;
        int N = texture.height;
        uint[,] channelUint8 = new uint[M, N];
        channelUint8 = Im2Uint8(texture)[index];


        int[] s = new int[256];
        uint[] channelValue = new uint[256];
        float ex = 0f, sig = 0f;
        channelValue = Historgram(texture, index, out ex, out sig);

        float[] cumsum = new float[256];
        cumsum = CumulativeHistogram(M, N, channelValue);
        s = ToInt(cumsum);



        int[] gz = new int[256];
        uint[] mapValue = new uint[256];

        mapValue = ToUintNotNormalize(mapDatas);
        //foreach (var v in mapValue)
        //{
        //    Debug.Log("mapvalue =" + v);
        //}

        float[] cumsumMapdata = new float[256];
        cumsumMapdata = CumulativeHistogram(M, N, mapValue);
        gz = ToInt(cumsumMapdata);

        s = MapValues(s, gz);

        float[,] resColor = new float[M, N];
        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                uint res = 0;
                float fres = 0;

                for (int i = 0; i < 256; i++)
                {
                    uint graylevel = channelUint8[m, n];
                    res = (uint)s[graylevel];
                    fres = scale * (res / 255f);
                }
                resColor[m, n] = fres;
            }
        }
        return resColor;
    }
    float[,] HisteqMap2(Texture2D texture, Texture2D texture2, int index, float scale = 1)
    {

        /// <summary>
        /// This funtion return one of 4 channels that is 2Dim Array Color[M,N] ( M = width,N = height) 
        /// which is the result of histogram equalization.
        /// channleValue is  value of  histogram  a channel which you want.
        /// index [0,1,2,3] = [red ,green ,blue, gray]. 
        /// </summary>

        int M = texture.width;
        int N = texture.height;
        uint[,] channelUint8 = new uint[M, N];
        channelUint8 = Im2Uint8(texture)[index];


        int[] s = new int[256];
        uint[] channelValue = new uint[256];
        float ex = 0f, sig = 0f;
        channelValue = Historgram(texture, index, out ex, out sig);

        float[] cumsum = new float[256];
        cumsum = CumulativeHistogram(M, N, channelValue);
        s = ToInt(cumsum);



        int[] s2 = new int[256];
        uint[] channelValue2 = new uint[256];
        float ex2 = 0f, sig2 = 0f;
        channelValue2 = Historgram(texture2, index, out ex2, out sig2);

        float[] cumsum2 = new float[256];
        cumsum2 = CumulativeHistogram(M, N, channelValue2);
        s2 = ToInt(cumsum);



        int[] mapRes = new int[256];
        mapRes = MapValues(s, s2);

       
        float[,] resColor = new float[M, N];
        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                uint res = 0;
                float fres = 0;

                for (int i = 0; i < 256; i++)
                {
                    uint graylevel = channelUint8[m, n];
                    res = (uint)s2[graylevel];
                    
                    fres = scale * (res / 255f);
                  
                }
                resColor[m, n] = fres;
            }
        }
        return resColor;
    }

    private float[] CumulativeHistogram(int M, int N,  uint[] channelValue)
    {
        float[] res = new float[256];
        float[] normalizeData = new float[256]; 
        for (int i = 0; i < 256; i++)
        {
            normalizeData[i] = channelValue[i] / (float)(M * N);
        }

        for (int i = 0; i < 256; i++)
        {
            for (int j = 0; j < i + 1; j++)
            {
                res[i] += 255 * normalizeData[j];
            }
        }

        return res;
    }

    uint[] ToUint(float[] datas)
    {
        uint[] s = new uint[256];
        for (int i = 0; i < 256; i++)
        {
            s[i] = (uint)(datas[i] + 0.5f);
            //Debug.Log("s[" + i + "] =="+s[i]);
        }
        return s;
    }

    int[] ToInt(float[] datas)
    {
        int[] s = new int[256];
        for (int i = 0; i < 256; i++)
        {
            s[i] = (int)(datas[i] + 0.5f);
            //Debug.Log("s[" + i + "] =="+s[i]);
        }
        return s;
    }

    uint[] ToUintNotNormalize(float[] datas)
    {
        uint[] s = new uint[256];
        for (int i = 0; i < 256; i++)
        {
            s[i] = (uint)(255f*datas[i] + 0.5f);
            //Debug.Log("s[" + i + "] =="+s[i]);
        }
        return s;
    }

    List<uint[,]> Im2Uint8(Texture2D texture)
    {

        /// <summary>
        /// This funtion return a List which contents all channels (R,G,B,L) that has tranform to uint 
        /// type and Range all value  to [0,255].
        /// The order  is  R,G,B, L.
        /// </summary>
      

        List<uint[,]> uint8Image = new List<uint[,]>();
        uint[,] uintValue_RChannel = new uint[texture.width, texture.height];
        uint[,] uintValue_GChannel = new uint[texture.width, texture.height];
        uint[,] uintValue_BChannel = new uint[texture.width, texture.height];
        uint[,] uintValue_LChannel = new uint[texture.width, texture.height];

        for (int m = 0; m < texture.width; m++)
        {
            for (int n = 0; n < texture.height; n++)
            {
                Color rgba = texture.GetPixel(m, n);
                uintValue_RChannel[m, n] = (uint)(rgba.r * 255 + 0.5f);
                uintValue_GChannel[m, n] = (uint)(rgba.g * 255 + 0.5f);
                uintValue_BChannel[m, n] = (uint)(rgba.b * 255 + 0.5f);
                uintValue_LChannel[m, n] = (uint)(Rgb2Gray(rgba)*255+0.5f);

            }
        }

        uint8Image.Add(uintValue_RChannel);
        uint8Image.Add(uintValue_GChannel);
        uint8Image.Add(uintValue_BChannel);
        uint8Image.Add(uintValue_LChannel);

        return uint8Image;
    }

    float Rgb2Gray(Color rgba) => Vector4.Dot(rgba, new Vector4(0.2989f, 0.5870f, 0.1140f, 0f));

    float[,] GetTextureValue(Texture2D texture,int index)
    {
        int M = texture.width;
        int N = texture.height;

        List<float[,]> Channles = new List<float[,]>();
      

        float[,] RChannel = new float[M, N];
        float[,] GChannel = new float[M, N];
        float[,] BChannel = new float[M, N];
        float[,] LChannel = new float[M, N];

        for (int m = 0, i = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++, i++)
            {
                Color rgba = texture.GetPixel(m, n);

                RChannel[m,n] = rgba.r;
                GChannel[m,n] = rgba.g;
                BChannel[m,n] = rgba.b;
                LChannel[m,n] = Rgb2Gray(rgba);
            }
        }

        Channles.Add(RChannel); Channles.Add(GChannel); Channles.Add(BChannel); Channles.Add(LChannel);
        return Channles[index];
    }

    float[] TwoModeGuassian(float m1,float sig1,float m2,float sig2,float A1,float A2,float k)
    {
        float c1 = A1 * 1f / (Sqrt(2f * pi) * sig1);
        float c2=  A2 * 1f / (Sqrt(2f * pi) * sig2);
        float k1 = 2f * Pow(sig1, 2);
        float k2 = 2f * Pow(sig2, 2);
        float sum = 0;
        float maxValue=0;
        float[] res = new float[256]; 
        for (int i = 0; i < 256; i++)
        {
            float x = i / 255f;
            float p = k + c1 * Exp(-Pow(m1 - x, 2) / k1) + c2 * Exp(-Pow(m2 - x, 2) / k2);
            sum += p;
            res[i] = p;
            
        }
         

        for (int i = 0; i < 256; i++)
        {
            //maxValue = Max(res[i]);
           // Debug.Log("res==" + res[i]);
        }
        
        //Debug.Log("max==" + Max( res));
        //Debug.Log("sum==" + sum);

        for (int i = 0; i < 256; i++)
        {
            res[i] = res[i] / sum;
        }
        return res;
    }

    int[] MapValues(int[] s, int[] g)
    {
        int m = 0;
        int n = 0;
        int[] res = new int[s.Length];
        for (int i = 0; i < s.Length; i++)
        {
            m = 0;
            n = 0;
            for (int j = 0; j < g.Length; j++)
            {

                if (s[i] == g[j])
                {
                    s[i] = j;
                    //Debug.Log("s[" + i + "]== " + j);
                    break;
                }
                else
                {
                    m++;
                    //Debug.Log("m = "+m);
                }
            }

            if (m == g.Length)
            {
                m = 0;
                n++;
                //Debug.Log("n = " + n);
                for (int j = 0; j < g.Length; j++)
                {

                    if (Abs(s[i] - g[j]) == n)
                    {

                        s[i] = j;
                        //Debug.Log("s[" + i + "]== " + j);
                        break;
                    }
                    else
                    {
                        m++;
                        //Debug.Log("m = "+m);
                    }
                }
            }
        }
        return res;
    }

}
