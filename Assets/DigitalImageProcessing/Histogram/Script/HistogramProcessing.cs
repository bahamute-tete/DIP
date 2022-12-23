using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;
using System;
using UnityEngine.UI;
using TMPro;
using static DIP.ImageProcessing;


public class HistogramProcessing : MonoBehaviour
{
   

    [SerializeField] Texture2D inputTexture, matchTexture;
    [SerializeField] GameObject barPrefab;
    [SerializeField] RawImage inputTex, channelOut;
    [SerializeField] RawImage inputTex2, channel2Out;
    [SerializeField] RawImage inputTex3, channel3Out;
    [SerializeField] RawImage matchTex, channel4Out, matchOutput;

    enum TextureChannel {R=0,G,B,Gay};
    TextureChannel channel = 0;
    TextureChannel channel2 = 0;



    
    [SerializeField] TMP_Dropdown dropdown;
    [SerializeField] TMP_Dropdown dropdown2;
    [SerializeField] TMP_Dropdown dropdown3;
    [SerializeField] TMP_Dropdown dropdown4;

    float mean0=0f, sig0=0f;
    [SerializeField] Text text_mean, text_sig;
    float mean1 = 0f, sig1 = 0f;
    [SerializeField] Text text_mean_res, text_sig_res;
    float mean2 = 0f, sig2 = 0f;
    [SerializeField] Text text_mean_res1, text_sig_res1;
    private void Awake()
    {
        DropDownSet(dropdown,"Red","Green","Blue");
        DropDownSet(dropdown2,"Hue","Saturation","Value");
        DropDownSet(dropdown3, "None", "V", "S");
        DropDownSet(dropdown4, "H", "S", "V");

    }
    
    void Start()
    {
       
        GameObject content = new GameObject("Content");
        GameObject content2 = new GameObject("Content2");
        GameObject content3 = new GameObject("Content3");

        int M = inputTexture.width; int N = inputTexture.height;

        ///////RGB
        Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);
        Texture2D outputeq = new Texture2D(M, N, TextureFormat.ARGB32, false);

        channel = TextureChannel.R;
        ChannelDatasVisualize(output, channel, content, mean0, sig0, -130f, 70f);
        ChannelDatasHisteqVisualize(outputeq, channel, content3, mean2, sig2, -130f, -170f);

        /////// Equalization single channel
        dropdown.onValueChanged.AddListener(delegate
        {
            ClearBar(content.transform);
            ClearBar(content3.transform);

            switch (dropdown.value)
            {
                case 0:
                    channel = TextureChannel.R;
                    ChannelDatasVisualize( output, channel, content, mean0, sig0, -130f, 70f);
                    ChannelDatasHisteqVisualize(outputeq, channel, content3, mean2, sig2, -130f, -170f);
                    break;
                case 1:
                    channel = TextureChannel.G;
                    ChannelDatasVisualize(output, channel, content, mean0, sig0, -130f,70f);
                    ChannelDatasHisteqVisualize(outputeq, channel, content3, mean2, sig2, -130f, -170f);
                    break;
                case 2:
                    channel = TextureChannel.B;
                    ChannelDatasVisualize(output, channel, content, mean0, sig0, -130f, 70f);
                    ChannelDatasHisteqVisualize(outputeq, channel, content3, mean2, sig2, -130f, -170f);
                    break;
            }
        });

        ///////HSV
        Texture2D outputHSV = new Texture2D(M, N, TextureFormat.ARGB32, false);
        Texture2D outputHSVChannel = new Texture2D(M, N, TextureFormat.ARGB32, false);

        channel2 = TextureChannel.R;
        Vector3[,] HsvColors = new Vector3[M, N];
        float[,] Hvalue = new float[M, N];
        float[,] Svalue = new float[M, N];
        float[,] Vvalue = new float[M, N];

        HsvColors = RgbToHsv(inputTexture);

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {

                Color rgb = inputTexture.GetPixel(m, n);
                float h = 0f;
                float s = 0f;
                float v = 0f;
                Color.RGBToHSV(rgb, out h, out s, out v);
                outputHSV.SetPixel(m, n, new Color(h,s,v));
            }
        }
        outputHSV.Apply();

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                Hvalue[m, n] = HsvColors[m, n].x;
                Svalue[m, n] = HsvColors[m, n].y;
                Vvalue[m, n] = HsvColors[m, n].z;
            }
        }
        Imshow(outputHSV, Hvalue, Svalue, Vvalue);
        Imshow(outputHSVChannel, Hvalue, Hvalue, Hvalue);
        ImHist(outputHSV, content2.transform, channel2, -130f, -60f);

        dropdown2.onValueChanged.AddListener(delegate
        {
            ClearBar(content2.transform);

            switch (dropdown2.value)
            {
                case 0:
                    channel2 = TextureChannel.R;
                    Imshow(outputHSVChannel, Hvalue, Hvalue, Hvalue);
                    ImHist(outputHSV, content2.transform, channel2, -130f, -60f);
                    Statistics(Histogram(outputHSV, channel2),out mean2,out sig2);
                    text_mean_res.text = mean2.ToString("mean= #.##");
                    text_sig_res.text = sig2.ToString("sig= #.##");
                    break;
                case 1:
                    channel2 = TextureChannel.G;
                    Imshow(outputHSVChannel, Svalue, Svalue, Svalue);
                    ImHist(outputHSV, content2.transform, channel2, -130f, -60f);
                    Statistics(Histogram(outputHSV, channel2), out mean2, out sig2);
                    text_mean_res.text = mean2.ToString("mean= #.##");
                    text_sig_res.text = sig2.ToString("sig= #.##");
                    break;
                case 2:
                    channel2 = TextureChannel.B;
                    Imshow(outputHSVChannel, Vvalue, Vvalue, Vvalue);
                    ImHist(outputHSV, content2.transform, channel2, -130f, -60f);
                    Statistics(Histogram(outputHSV, channel2), out mean2, out sig2);
                    text_mean_res.text = mean2.ToString("mean= #.##");
                    text_sig_res.text = sig2.ToString("sig= #.##");
                    break;
            }
        });


        inputTex.texture = inputTexture;
        channelOut.texture = output;

        inputTex2.texture = outputHSV;
        channel2Out.texture = outputHSVChannel;

        channel3Out.texture = outputeq;

        ///////Equalization one of hsv channels 
        float[,] eqV = new float[inputTexture.width, inputTexture.height];
        float[,] eqS = new float[inputTexture.width, inputTexture.height];
        eqV = Histeq(outputHSV, TextureChannel.B);
        eqS = Histeq(outputHSV, TextureChannel.G);

        Texture2D outputHSV2RGB = new Texture2D(M, N, TextureFormat.ARGB32, false);
        Color[,] resColor = new Color[M, N];
        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {

                Vector3 hsveqRes = new Vector3(outputHSV.GetPixel(m, n).r, outputHSV.GetPixel(m, n).g, outputHSV.GetPixel(m, n).b);

               
                Color rgbColor = Color.HSVToRGB(hsveqRes.x,hsveqRes.y,hsveqRes.z);
                outputHSV2RGB.SetPixel(m, n, rgbColor);

                //Vector3 rgb = HSV2RGB(hsveqRes);
                //outputHSV2RGB.SetPixel(m, n, new Color(rgb.x, rgb.y, rgb.z));
            }
        }
        outputHSV2RGB.Apply();

        dropdown3.onValueChanged.AddListener(delegate
        {

            switch (dropdown3.value)
            {
                case 0:

                    for (int m = 0; m < M; m++)
                    {
                        for (int n = 0; n < N; n++)
                        {
                            Vector3 hsveqRes = new Vector3(outputHSV.GetPixel(m, n).r, outputHSV.GetPixel(m, n).g, outputHSV.GetPixel(m, n).b);
                            //Vector3 rgb = HSV2RGB(hsveqRes);
                            //outputHSV2RGB.SetPixel(m, n, new Color(rgb.x, rgb.y, rgb.z));

                            Color rgbColor = Color.HSVToRGB(hsveqRes.x, hsveqRes.y, hsveqRes.z);
                            outputHSV2RGB.SetPixel(m, n, rgbColor);
                        }
                    }
                    outputHSV2RGB.Apply();
                    break;
                case 1:
                    for (int m = 0; m < M; m++)
                    {
                        for (int n = 0; n < N; n++)
                        {
                            Vector3 hsveqRes = new Vector3(outputHSV.GetPixel(m, n).r, outputHSV.GetPixel(m, n).g, eqV[m, n]);
                            //Vector3 rgb = HSV2RGB(hsveqRes);
                            //outputHSV2RGB.SetPixel(m, n, new Color(rgb.x, rgb.y, rgb.z));

                            Color rgbColor = Color.HSVToRGB(hsveqRes.x, hsveqRes.y, hsveqRes.z);
                            outputHSV2RGB.SetPixel(m, n, rgbColor);
                        }
                    }
                    outputHSV2RGB.Apply();
                    break;
                case 2:
                    for (int m = 0; m < M; m++)
                    {
                        for (int n = 0; n < N; n++)
                        {
                            Vector3 hsveqRes = new Vector3(outputHSV.GetPixel(m, n).r, eqS[m,n], outputHSV.GetPixel(m, n).b);
                            //Vector3 rgb = HSV2RGB(hsveqRes);
                            //outputHSV2RGB.SetPixel(m, n, new Color(rgb.x, rgb.y, rgb.z));

                            Color rgbColor = Color.HSVToRGB(hsveqRes.x, hsveqRes.y, hsveqRes.z);
                            outputHSV2RGB.SetPixel(m, n, rgbColor);
                        }
                    }
                    outputHSV2RGB.Apply();
                    break;
            }
        });

        inputTex3.texture = outputHSV2RGB;

        ///////Equalization (Match)
        int M1 = matchTexture.width; int N1 = matchTexture.height;
        Texture2D match = new Texture2D(M1,N1,TextureFormat.ARGB32, false);
        matchTex.texture = matchTexture;

        Texture2D hsvMatchTex = new Texture2D(M1, N1, TextureFormat.ARGB32, false);
        Vector3[,] hsvMatch = new Vector3[M1, N1];
        float[,] hMatchvalue = new float[M1, N1];
        float[,] sMatchvalue = new float[M1, N1];
        float[,] vMatchvalue = new float[M1, N1];

        hsvMatch = RgbToHsv(matchTexture);

        for (int m = 0; m < M1; m++)
        {
            for (int n = 0; n < N1; n++)
            {

                Color rgb = matchTexture.GetPixel(m, n);
                float h = 0f;
                float s = 0f;
                float v = 0f;
                Color.RGBToHSV(rgb, out h, out s, out v);
                hsvMatchTex.SetPixel(m, n, new Color(h, s, v));
            }
        }
        hsvMatchTex.Apply();
        channel4Out.texture = hsvMatchTex;

        Texture2D outputeMatch = new Texture2D(inputTexture.width, inputTexture.height, TextureFormat.ARGB32, false);

        float[,] maH = new float[inputTexture.width, inputTexture.height];
        maH = Histeq(outputHSV, hsvMatchTex, TextureChannel.R);
        float[,] maS = new float[inputTexture.width, inputTexture.height];
        maS = Histeq(outputHSV, hsvMatchTex, TextureChannel.G);
        float[,] maV = new float[inputTexture.width, inputTexture.height];
        maV = Histeq(outputHSV, hsvMatchTex, TextureChannel.B);

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                Vector3 hsveqRes = new Vector3(maH[m, n], outputHSV.GetPixel(m, n).g, outputHSV.GetPixel(m, n).b);
                Color rgbColor = Color.HSVToRGB(hsveqRes.x, hsveqRes.y, hsveqRes.z);
                outputeMatch.SetPixel(m, n, rgbColor);
            }
        }
        outputeMatch.Apply();

        dropdown4.onValueChanged.AddListener(delegate
        {

            switch (dropdown4.value)
            {
                case 0:

                    for (int m = 0; m < M; m++)
                    {
                        for (int n = 0; n < N; n++)
                        {
                            Vector3 hsveqRes = new Vector3(maH[m,n], outputHSV.GetPixel(m, n).g, outputHSV.GetPixel(m, n).b);
                            Color rgbColor = Color.HSVToRGB(hsveqRes.x, hsveqRes.y, hsveqRes.z);
                            outputeMatch.SetPixel(m, n, rgbColor);
                        }
                    }
                    outputeMatch.Apply();
                    break;
                case 1:
                    for (int m = 0; m < M; m++)
                    {
                        for (int n = 0; n < N; n++)
                        {
                            Vector3 hsveqRes = new Vector3(outputHSV.GetPixel(m, n).r, maS[m,n], outputHSV.GetPixel(m, n).b);
                            Color rgbColor = Color.HSVToRGB(hsveqRes.x, hsveqRes.y, hsveqRes.z);
                            outputeMatch.SetPixel(m, n, rgbColor);
                        }
                    }
                    outputeMatch.Apply();
                    break;
                case 2:
                    for (int m = 0; m < M; m++)
                    {
                        for (int n = 0; n < N; n++)
                        {
                            Vector3 hsveqRes = new Vector3(outputHSV.GetPixel(m, n).r, outputHSV.GetPixel(m, n).g, maV[m,n]);
                            Color rgbColor = Color.HSVToRGB(hsveqRes.x, hsveqRes.y, hsveqRes.z);
                            outputeMatch.SetPixel(m, n, rgbColor);
                        }
                    }
                    outputeMatch.Apply();
                    break;
            }
        });
        matchOutput.texture = outputeMatch;


    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void ChannelDatasVisualize(Texture2D output,TextureChannel channel,GameObject parent,float mean,float sig,float xOffset,float yOffset)
    {
        mean = sig = 0f;

        float[,] channelColor = new float[inputTexture.width, inputTexture.height];
        channelColor = GetChannelValue(inputTexture, channel);


        Imshow(output, channelColor, channelColor, channelColor);
        ImHist(output, parent.transform, channel, xOffset, yOffset);
        Statistics(Histogram(output, channel), out mean, out sig);
        text_mean.text = mean.ToString("mean= #.##");
        text_sig.text = sig.ToString("sig= #.##");
    }
    void ChannelDatasHisteqVisualize(Texture2D output, TextureChannel channel, GameObject parent, float mean, float sig, float xOffset, float yOffset)
    {
        mean = sig = 0f;

        float[,] eqcolor = new float[inputTexture.width, inputTexture.height];
        eqcolor = Histeq(inputTexture, channel);
        Imshow(output, eqcolor, eqcolor, eqcolor);
        ImHist(output, parent.transform, channel, xOffset, yOffset);
        Statistics(Histogram(output, channel), out mean, out sig);
        text_mean_res1.text = mean.ToString("mean= #.##");
        text_sig_res1.text = sig.ToString("sig= #.##");
    }

    private void DropDownSet( TMP_Dropdown dropdown,string c1,string c2,string c3)
    {
        TMP_Dropdown.OptionData optionData0 = new TMP_Dropdown.OptionData();
        optionData0.text = c1;
        TMP_Dropdown.OptionData optionData1 = new TMP_Dropdown.OptionData();
        optionData1.text = c2;
        TMP_Dropdown.OptionData optionData2 = new TMP_Dropdown.OptionData();
        optionData2.text = c3;

        List<TMP_Dropdown.OptionData> optionDatas = new List<TMP_Dropdown.OptionData>();
        optionDatas.Add(optionData0); optionDatas.Add(optionData1); optionDatas.Add(optionData2);

        dropdown.AddOptions(optionDatas);

        dropdown.options[0].text = c1;
        dropdown.options[1].text = c2;
        dropdown.options[2].text = c3;
        dropdown.value = 0;
    }
    void ClearBar(Transform content)
    {
        if (content != null && content.childCount != 0)
        {
            for (int i = 0; i < content.childCount; i++)
            {
                GameObject temp = content.GetChild(i).gameObject;
                Destroy(temp);
            }
        }
    }

    void Statistics(float[] datas,out float mean, out float sig)
    {
        mean = 0f;sig = 0f;
        float powerSig = 0f;
        for (int i = 0; i < datas.Length; i++)
        {
            mean += i * datas[i];
        }
       
        for (int i = 0; i <datas.Length; i++)
        {
            powerSig += (i - mean) * (i - mean) * datas[i];
        }

        sig = Sqrt(powerSig);

    }

    float[] Histogram(Texture2D tex,TextureChannel channel)
    {
        int L = 256;
        int M = tex.width, N = tex.height;
        int[] hist = new int[L];
        float[] histNormal = new float[L];

        int[] grayLevel = new int[M*N];
        float[] grayscale = new float[M * N];

       
        for (int i = 0; i < L; i++)
        {
            hist[i] = 0;
        }

        for (int m = 0,i=0; m < M; m++)
        {
            for (int n = 0; n < N; n++,i++)
            {
               
                float gray = tex.GetPixel(m, n).grayscale;
                float r = tex.GetPixel(m, n).r;
                float g = tex.GetPixel(m, n).g;
                float b = tex.GetPixel(m, n).b;
                switch ((int)channel)
                {
                    case 0:
                        grayscale[i] = r;
                        break;

                    case 1:
                        grayscale[i] = g;
                        break;

                    case 2:
                        grayscale[i] = b;
                        break;

                    case 3:
                        grayscale[i] = gray;
                        break;
                }
                //grayscale[i] = value;
            }
        }

        for (int i = 0; i < grayscale.Length; i++)
        {
            grayLevel[i] = (int)(grayscale[i] * (L - 1) + 0.5f); 
        }

        for (int i = 0; i < grayLevel.Length; i++)
        {
            hist[grayLevel[i]] += 1;
        }

        for (int i = 0; i < hist.Length; i++)
        {
            histNormal[i] = hist[i] / (float)(M * N);
            //Debug.Log("HistNormal[" + i + "]== " + histNormal[i]);
        }

        return histNormal;
    }
    void ImHist(Texture2D tex,Transform panrent, TextureChannel channel,float x_offset=0f ,float y_offset =0f, float scale = 2550f)
    {
        int L = 256;
        GameObject[] bars = new GameObject[L];
        int M = tex.width;
        int N = tex.height;

        for (int i = 0; i < L; i++)
        {

            GameObject temp = Instantiate(barPrefab, Vector3.zero, Quaternion.identity, panrent);
            temp.transform.position = new Vector3(i, 0, 0);
            float lineWidth = 0.8f;
            LrSet(temp, 2, lineWidth, Color.gray);

            bars[i] = temp;
        }

        float[] levels = new float[L];

        levels = Histogram(tex,channel);

        for (int i = 0; i < L; i++)
        {
            SetPos(bars[i], i, levels[i] * scale, x_offset, y_offset);
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
    void SetPos(GameObject bar, float horizonShift, float verticalShift,float x_offset =0f,float y_offset= 0f)
    {
        bar.GetComponent<LineRenderer>().SetPosition(0, new Vector3(horizonShift+ x_offset, y_offset, 0));
        bar.GetComponent<LineRenderer>().SetPosition(1, new Vector3(horizonShift+ x_offset, verticalShift+ y_offset, 0));
    }

    int[] CDF(float[] levels,int M ,int N)
    {
        int L = 256;
        float[] sum = new float[L];
       
        int[] res = new int[L];

        for (int i = 0; i < L; i++)
        {
            for (int j = 0; j < i + 1; j++)
            {
                sum[i] += (L-1)* levels[j];
                int index = (int)(sum[i] + 0.5f);
                res[i] = index;
            }
        }
        return res;
    }
    int[] Match(int[] s, int[] g)
    {

        for (int i = 0; i < s.Length; i++)
        {

            if (Array.Exists(g, (x) => x.Equals(s[i])))
            {
                int index = Array.FindIndex(g, (x) => x.Equals(s[i]));
                s[i] = index;
            }
            else
            {
                int[] error = new int[s.Length];
                for (int j = 0; j < g.Length; j++)
                {
                    int temp = Abs(s[i] - g[j]);
                    error[j] = temp;
                }
                int min = Min(error);
                int indexError = Array.FindIndex(error, (x) => x == min);
                s[i] = indexError;
            }

        }

        return s;

    }
    float[,] Histeq(Texture2D texture, TextureChannel channel)
    {
        int L = 256;
        int M = texture.width;
        int N = texture.height;
        int[,] textureGray = new int[M, N];

        float value = 0;
        for (int m = 0, i = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++, i++)
            {
                //float gray = texture.GetPixel(m, n).grayscale;
                switch ((int)channel)
                {
                    case 0:
                        value = texture.GetPixel(m, n).r;
                        break;

                    case 1:
                        value = texture.GetPixel(m, n).g;
                        break;

                    case 2:
                        value = texture.GetPixel(m, n).b;
                        break;

                    case 3:
                        value = texture.GetPixel(m, n).grayscale;
                        break;
                }
                textureGray[m, n] = (int)((L - 1) * value + 0.5);
            }
        }

        int[] s = new int[L];
        s=CDF( Histogram(texture,channel),M,N);

        float[,] singleColorChannel = new float[M, N];

        for (int m = 0, i = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++, i++)
            {

                int gl = textureGray[m, n];

                int res = s[gl];

                singleColorChannel[m, n] = res/255f;
            }
        }

        return singleColorChannel;
    }
    float[,] Histeq(Texture2D Src,Texture2D Dst, TextureChannel channel)
    {
        int L = 256;
        int M = Src.width;
        int N = Src.height;
        int[,] textureGray = new int[M, N];
        //float gray = 0;
        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                float gray = Src.GetPixel(m, n).grayscale;
                float r = Src.GetPixel(m, n).r;
                float g = Src.GetPixel(m, n).g;
                float b = Src.GetPixel(m, n).b;
                switch ((int)channel)
                {
                    case 0:
                        textureGray[m, n] = (int)((L - 1) * r + 0.5);
                        break;

                    case 1:
                        textureGray[m, n] = (int)((L - 1) * g + 0.5);
                        break;

                    case 2:
                        textureGray[m, n] = (int)((L - 1) * b + 0.5);
                        break;

                    case 3:
                        textureGray[m, n] = (int)((L - 1) * gray + 0.5);
                        break;
                }
            }
        }
        int[] s = new int[L];
        s = CDF(Histogram(Src, channel), M, N);


        int M2 = Dst.width;
        int N2= Dst.height;
        int[] Gz = new int[L];
        Gz = CDF(Histogram(Dst, channel), M2, N2);


        s = Match(s, Gz);


        float[,] singleColorChannel = new float[M, N];
        for (int m = 0, i = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++, i++)
            {
                int gl = textureGray[m, n];
                int res = s[gl];
                singleColorChannel[m, n] = res / 255f;
            }
        }

        return singleColorChannel;
    }

    float[,] GetChannelValue(Texture2D texture,TextureChannel channel)
    {
        int M = texture.width;
        int N = texture.height;

        float[,] singleColorChannel = new float[M, N];
        float value=0;
        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                switch ((int)channel)
                {
                    case 0:
                         value = texture.GetPixel(m, n).r;
                         break;

                    case 1:
                        value = texture.GetPixel(m, n).g;
                        break;

                    case 2:
                        value = texture.GetPixel(m, n).b;
                        break;

                    case 3:
                        value = texture.GetPixel(m, n).grayscale;
                        break;
                }
                //float value = texture.GetPixel(m, n).grayscale;
                 
                singleColorChannel[m, n] = value;
            } 
        }
        return singleColorChannel;
    }

    void Imshow(Texture2D outpute,float[,] channelR,float[,] channelG,float[,] channleB)
    {
        int M = outpute.width;
        int N = outpute.height;

        for (int m = 0, i = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++, i++)
            {

                Color color = new Color(channelR[m, n], channelG[m, n], channleB[m, n], 1f);
                outpute.SetPixel(m, n, color);
            }
        }

        outpute.Apply();
    }

    float Rgb2Gray(Color rgba) => Vector4.Dot(rgba, new Vector4(0.305306011f, 0.682171111f, 0.012522878f, 0f));
    int[] ToInt(float[] f)
    {
        int[] r = new int[f.Length];
        for (int i = 0; i < f.Length; i++)
        {
            r[i] = (int)(f[i] + 0.5f);
        }
        return r;
    }



    Vector3 RGB2HSV(Color rgb)
    {
        Vector3 HSV = new Vector3(0f, 0f, 0f);

        float r = rgb.r;
        float g = rgb.g;
        float b = rgb.b;

        float num = 0.5f * ((r - g) + (r - b));
        float den = Sqrt(Pow(r - g,2) + (r - b) * (g - b));
        float H = Acos(num / (den + float.Epsilon));
        if (b > g)
            H = 2 * PI - H;
        H /= 2 * PI;

        float den2 = r + g + b;
        if (den2 == 0)
            den2 = float.Epsilon;
        float S = 1f - 3f / den2 * Min(Min(r, g), b);
        //float S = 0;
        //if (Max(Max(r, g), b) != 0)
        //    S = 1f - Min(Min(r, g), b) / Max(Max(r, g), b);


        if (S == 0)
            H = 0;

        float V = (r + g + b)/3f;
         //float V = Max(Max(r, g), b);

        HSV = new Vector3(H, S, V);
        return HSV;
    }
    Vector3 HSV2RGB(Vector3 hsv)
    {
        
        float H = hsv.x;
        float S = hsv.y;
        float V = hsv.z;

        H *= 2f * PI;

        float r=0f, g=0f, b=0f;
        if (H >= 0f && H < 2f / 3f * PI)
        {
             r = V * (1f + S * Cos(H) / Cos( PI/3f - H ));
             b = V * (1 - S);
             g = 3f * V - (r + b);
        }

        if (H >= 2f / 3f * PI && H < 4f / 3f * PI)
        {
             g = V * (1f + S * Cos(H- 2f / 3f * PI) / Cos( PI - H));
             r = V * (1 - S);
             b = 3f * V - (r + g);
        }

        if (H >= 4f / 3f * PI && H <= 2*PI)
        {
             b = V * (1f + S * Cos(H - 4f / 3f * PI) / Cos(5f/3f*PI - H));
             g = V * (1 - S);
             r = 3f * V - (b + g);
        }

        Vector3 rgb = new Vector3(r, g, b);

        Vector3.Max(Vector3.Min(rgb, Vector3.one), Vector3.zero);

        return new Vector3(rgb.x,rgb.y,rgb.z);
    }
    Vector3[,] RgbToHsv(Texture2D texture)
    {
        int M = texture.width;
        int N = texture.height;
        float[,] H = new float[M, N];
        float[,] S = new float[M, N];
        float[,] V = new float[M, N];

        Vector3[,] HSVColor = new Vector3[M, N];

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                //H[m, n] = RGB2HSV(texture.GetPixel(m, n)).x;
                //S[m, n] = RGB2HSV(texture.GetPixel(m, n)).y;
                //V[m, n] = RGB2HSV(texture.GetPixel(m, n)).z;
                Color.RGBToHSV(texture.GetPixel(m, n),out H[m, n], out S[m, n],out V[m, n]);
              
                HSVColor[m, n] = new Vector3(H[m, n], S[m, n], V[m, n]);
            }
        }
        return HSVColor;
    }
    Vector3[,] Hsv2Rgb(Texture2D texture)
    {
        int M = texture.width;
        int N = texture.height;
        float[,] R = new float[M, N];
        float[,] G = new float[M, N];
        float[,] B = new float[M, N];

        Vector3[,] rgbColor = new Vector3[M, N];

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                Vector3 hsv = new Vector3(texture.GetPixel(m, n).a, texture.GetPixel(m, n).g, texture.GetPixel(m, n).b);

                rgbColor[m, n] = HSV2RGB(hsv);
            }
        }
        return rgbColor;
    }

    Vector3 RGB2HSV2(Color rgb)
    {
        Vector3 HSV = new Vector3(0f, 0f, 0f);

        float r = rgb.r;
        float g = rgb.g;
        float b = rgb.b;

        float num = 0.5f * ((r - g) + (r - b));
        float den = Sqrt(Pow(r - g, 2) + (r - b) * (g - b));
        float H = Acos(num / (den + float.Epsilon));
        if (b > g)
            H = 2 * PI - H;
        H /= 2 * PI;


        float S = 0;
        if (Max(Max(r, g), b) != 0)
            S = 1f - Min(Min(r, g), b) / Max(Max(r, g), b);


        if (S == 0)
            H = 0;
        float V = Max(Max(r, g), b);

        HSV = new Vector3(H, S, V);
        return HSV;
    }
    Vector3 HSV2RGB2(Vector3 hsv)
    {

        float H = hsv.x;
        float S = hsv.y;
        float V = hsv.z;

        H *= 2f * PI;

        float C = V * S;
        float m = V - C;
        float X = C * (1 - Abs((PI * H / 3f) % 2 - 1));

        float r = 0f, g = 0f, b = 0f;
        if (H >= 0f && H < 1f / 3f * PI)
        {
            r = C;
            g = X;
            b = 0f;
        }
        if (H >= 1f / 3f * PI && H < 2f / 3f * PI)
        {
            r = X;
            g = C;
            b = 0f;
        }


        if (H >= 2f / 3f * PI && H < PI)
        {
            r = 0;
            g = C;
            b = X;
        }
        if (H >= PI && H < 4f / 3f * PI)
        {
            r = 0;
            g = X;
            b = C;
        }

        if (H >= 4f / 3f * PI && H < 5f / 3f * PI)
        {
            r = X;
            g = 0;
            b = C;
        }
        if (H >= 5f / 3f * PI && H <= 2 * PI)
        {
          
            r = C;
            g = 0;
            b = X;
        }

        Vector3 rgb = new Vector3(r+m, g+m, b+m);

        Vector3.Max(Vector3.Min(rgb, Vector3.one), Vector3.zero);

        return new Vector3(rgb.x, rgb.y, rgb.z);
    }
    Vector3[,] RgbToHsv2(Texture2D texture)
    {
        int M = texture.width;
        int N = texture.height;
        float[,] H = new float[M, N];
        float[,] S = new float[M, N];
        float[,] V = new float[M, N];

        Vector3[,] HSVColor = new Vector3[M, N];

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                H[m, n] = RGB2HSV2(texture.GetPixel(m, n)).x;
                S[m, n] = RGB2HSV2(texture.GetPixel(m, n)).y;
                V[m, n] = RGB2HSV2(texture.GetPixel(m, n)).z;
                HSVColor[m, n] = new Vector3(H[m, n], S[m, n], V[m, n]);
            }
        }
        return HSVColor;
    }
    Vector3[,] Hsv2Rgb2(Texture2D texture)
    {
        int M = texture.width;
        int N = texture.height;
        float[,] R = new float[M, N];
        float[,] G = new float[M, N];
        float[,] B = new float[M, N];

        Vector3[,] rgbColor = new Vector3[M, N];

        for (int m = 0; m < M; m++)
        {
            for (int n = 0; n < N; n++)
            {
                Vector3 hsv = new Vector3(texture.GetPixel(m, n).a, texture.GetPixel(m, n).g, texture.GetPixel(m, n).b);

                rgbColor[m, n] = HSV2RGB2(hsv);
            }
        }
        return rgbColor;
    }
}