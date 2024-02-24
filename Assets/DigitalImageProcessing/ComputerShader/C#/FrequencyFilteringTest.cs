using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using static UnityEngine.UI.Image;

public class FrequencyFilteringTest : MonoBehaviour
{

    [SerializeField] Texture2D texture;
    [SerializeField] RawImage original;
    [SerializeField] RawImage convertData, DFT_Gpu, DFT_Cpu;
    [SerializeField] Button btn_GPU, btn_CPU;
    [SerializeField] ComputeShader csFrequencyFiltering;

    int kernel_DFT;

    // Start is called before the first frame update
    void Start()
    {

        original.texture = texture;


        btn_GPU.onClick.AddListener(delegate
        {
            RenderTexture rt_convert_data_Test = CreateRenderTexture(texture.width, texture.height);

            kernel_DFT = csFrequencyFiltering.FindKernel("DFT");

            csFrequencyFiltering.SetTexture(kernel_DFT, Shader.PropertyToID("input"), texture);
            csFrequencyFiltering.SetTexture(kernel_DFT, Shader.PropertyToID("Result"), rt_convert_data_Test);

            csFrequencyFiltering.Dispatch(kernel_DFT, CeilToInt(texture.width / 8.0f), CeilToInt(texture.height / 8.0f), 1);

           
            convertData.texture = rt_convert_data_Test;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    RenderTexture CreateRenderTexture(int width, int height)
    {
        RenderTexture rt = new RenderTexture(width, height, 32, RenderTextureFormat.ARGB32);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }
}
