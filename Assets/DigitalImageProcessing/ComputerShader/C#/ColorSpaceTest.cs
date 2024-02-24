using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using DIP;


public class ColorSpaceTest : MonoBehaviour
{
    [SerializeField] Texture2D texture;
    [SerializeField] RawImage original, result;
    [SerializeField] Button btn_GPU, btn_CPU;
    [SerializeField] ComputeShader csColorSpace;

    // Start is called before the first frame update
    void Start()
    {

        original.texture = texture;

        btn_GPU.onClick.AddListener(delegate
        {
            RenderTexture rt = new RenderTexture(texture.width,texture.height,32,RenderTextureFormat.ARGB32);
            rt.enableRandomWrite = true;
            rt.Create();

            int kernel = csColorSpace.FindKernel("ColorSpaceTransform");
            csColorSpace.SetTexture(kernel, Shader.PropertyToID("Input"), texture);
            csColorSpace.SetTexture(kernel, Shader.PropertyToID("Result"), rt);
            csColorSpace.Dispatch(kernel, CeilToInt(texture.width / 8.0f), CeilToInt(texture.height / 8.0f), 1);


            result.texture = rt;
        }
        );

        btn_CPU.onClick.AddListener(delegate
        {
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    
}
