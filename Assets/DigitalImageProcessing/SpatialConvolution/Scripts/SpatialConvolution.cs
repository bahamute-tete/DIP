using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using static DIP.ImageProcessing;
using TMPro;

public class SpatialConvolution : MonoBehaviour
{
    [SerializeField] Texture2D inputeTexture;
    Texture2D outputeTexture, outputTexture2, noiseTexture;

    [SerializeField] RawImage inputeImage;
    [SerializeField] RawImage outputImage;
    [SerializeField] RawImage outputImage2;
    [SerializeField] RawImage hisImage;


    float[,] LPF, HPF;
    [SerializeField, Range(0.4f,3f)] float sig = 0.5f;
    [SerializeField, Range(0f,1f)] float alpha = 0.2f;


    [SerializeField]int r =3;
    [SerializeField]int c = 3;

    [SerializeField] TMP_Dropdown dropFilterType;
    [SerializeField] Slider sigSlider;

    LineRenderer lr = new LineRenderer();
    //Texture2D hist;
    // Start is called before the first frame update
    private void Awake()
    {

        outputeTexture = new Texture2D(inputeTexture.width, inputeTexture.height, TextureFormat.ARGB32, false);
        outputTexture2 = new Texture2D(inputeTexture.width, inputeTexture.height, TextureFormat.ARGB32, false);
        noiseTexture = new Texture2D(inputeTexture.width, inputeTexture.height, TextureFormat.ARGB32, false);
        //hist = new Texture2D(258, 100, TextureFormat.ARGB32, false);
        DropDownSet(dropFilterType, "Average", "Guassian", "Laplacian");


        //GameObject content = new GameObject();
        //lr =content.AddComponent<LineRenderer>();
        //lr.material = new Material(Shader.Find("Particles/Standard Unlit"));
        //lr.startColor = lr.endColor = Color.green;
        //lr.startWidth = lr.endWidth = 0.01f;

        //List<Vector3> pos = new List<Vector3>();
        //for (float i = -2f; i < 2f; i += 0.01f)
        //{
        //    float N = Randn3();
        //    Vector3 v = new Vector3(i, N, 0f);
        //    pos.Add(v);
        //}
        //lr.positionCount = pos.Count;
        //lr.SetPositions(pos.ToArray());
        noiseTexture = ImNoise(256, 256, Nosise_Type.guassian, 0.5f, 0.1f);

        LPF = new float[r, c];
        LPF = FSpecial(KernelType.gaussian, 3, 3,0.5f);
        //foreach (var f in LPF)
        //{
        //    Debug.Log(f);
        //}
        inputeImage.texture =ImFilter(inputeTexture,LPF,Boundary_Option.zero);
        inputeImage.SetNativeSize();
        Histogram(noiseTexture, TextureChannel.Gay, out Texture2D hist);
        hisImage.texture = hist;
        hisImage.SetNativeSize();


        //LPF = new float[r, c];
        //HPF = new float[r, c];

        outputeTexture = AverageBlur(inputeTexture, r, c, Boundary_Option.symmetric);
        outputImage.texture = outputeTexture;
        outputImage.SetNativeSize();

        //sigSlider.maxValue = 3f;
        //sigSlider.minValue = 0.1f;
        //sigSlider.value = 1f;

        //sigSlider.onValueChanged.AddListener(delegate
        //{
        //    sig = sigSlider.value;
        //    outputeTexture = GussianBlur(inputeTexture, r, c, sig, Boundary_Option.symmetric);
        //    outputImage.texture = outputeTexture;
        //});

        //dropFilterType.onValueChanged.AddListener(delegate
        //{

        //    switch (dropFilterType.value)
        //    {
        //        case 0:
        //            outputeTexture = AverageBlur(inputeTexture, r, c, Boundary_Option.symmetric);
        //            sigSlider.gameObject.SetActive(false);
        //            break;
        //        case 1:
        //            outputeTexture = GussianBlur(inputeTexture, r, c, sig, Boundary_Option.symmetric);
        //            sigSlider.gameObject.SetActive(true);
        //            break;
        //        case 2:
        //            HPF = new float[r, c];
        //            HPF = FSpecial(FilterType.laplacian, r, c, alpha);
        //            outputeTexture = ImFilter(inputeTexture, HPF, Boundary_Option.replcate);
        //            sigSlider.gameObject.SetActive(false);
        //            break;
        //    }
        //    outputImage.texture = outputeTexture;
        //});

        //outputTexture2 = MedianFilter(inputeTexture, r, c, Boundary_Option.symmetric);

        //outputImage2.texture = outputTexture2;
        //outputImage2.SetNativeSize();

    }



    // Update is called once per frame
    void Update()
    {

       

       
    }

    private void DropDownSet(TMP_Dropdown dropdown, string c1, string c2, string c3)
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
}
