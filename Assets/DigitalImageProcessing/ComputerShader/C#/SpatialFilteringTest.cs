using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using DIP;

public class SpatialFilteringTest : MonoBehaviour
{

    [SerializeField] Texture2D texture;
    [SerializeField] RawImage original,kernelTex,kernelSum, kenelTexH, kenelTexV;
    [SerializeField] RawImage  firstPassTex, blurResult_Gpu, blurResult_Cpu,laplacianResult;
    [SerializeField] Button btn_GPU, btn_CPU;
    [SerializeField] ComputeShader csSpatialFiltering;
    [Range(3, 141), Min(3)]
    [SerializeField] int kernelSize = 3;


    private void Start()
    {
        float[] r1 = new float[9] {1.0f,1.0f,1.0f,1.0f,-8.0f,1.0f,1.0f,1.0f,1.0f };
        float[] laplacianK = new float[36];

        for (int i = 0,a=0; i < r1.Length; i++,a+=4)
        {
            laplacianK[a] = r1[i];
        }

 

        original.texture = texture;

        btn_GPU.onClick.AddListener(delegate
        {
            int LPFSize = kernelSize;
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            int kernel_SF = csSpatialFiltering.FindKernel("SpatialFiltering");

            RenderTexture rt_Laplacian_kernel = new RenderTexture(3, 3, 32, RenderTextureFormat.ARGB32);
            rt_Laplacian_kernel.enableRandomWrite = true;
            rt_Laplacian_kernel.Create();

            RenderTexture rt_Laplacian = new RenderTexture(texture.width, texture.height, 32, RenderTextureFormat.ARGB32);
            rt_Laplacian.enableRandomWrite = true;
            rt_Laplacian.Create();


            csSpatialFiltering.SetFloats(Shader.PropertyToID("lpKernel"), laplacianK);

            csSpatialFiltering.SetTexture(kernel_SF, Shader.PropertyToID("LaplacianLPF"), rt_Laplacian_kernel);
            csSpatialFiltering.SetTexture(kernel_SF, Shader.PropertyToID("Input"), texture);
            csSpatialFiltering.SetTexture(kernel_SF, Shader.PropertyToID("LaplacianResult"), rt_Laplacian);


            csSpatialFiltering.Dispatch(kernel_SF, CeilToInt(texture.width / 8.0f), CeilToInt(texture.height / 8.0f), 1);
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            int kernel_KernelCreate = csSpatialFiltering.FindKernel("GaussianKernelCreate");

            float[] gaussainK = new float[LPFSize*4];


            RenderTexture rt_Kernel = new RenderTexture(LPFSize, LPFSize, 32, RenderTextureFormat.ARGB32);
            rt_Kernel.enableRandomWrite = true;
            rt_Kernel.Create();

            RenderTexture rt_Kernel_H = new RenderTexture(LPFSize, 1, 32, RenderTextureFormat.ARGB32);
            rt_Kernel_H.enableRandomWrite = true;
            rt_Kernel_H.Create();
            csSpatialFiltering.SetTexture(kernel_KernelCreate, Shader.PropertyToID("KernelHor"), rt_Kernel_H);

            RenderTexture rt_Kernel_V = new RenderTexture(1, LPFSize, 32, RenderTextureFormat.ARGB32);
            rt_Kernel_V.enableRandomWrite = true;
            rt_Kernel_V.Create();

            RenderTexture rt_Kernel_Sum = new RenderTexture(LPFSize, LPFSize, 32, RenderTextureFormat.ARGB32);
            rt_Kernel_Sum.enableRandomWrite = true;
            rt_Kernel_Sum.Create();


            csSpatialFiltering.SetTexture(kernel_KernelCreate, Shader.PropertyToID("KernelVer"), rt_Kernel_V);

            csSpatialFiltering.SetInt(Shader.PropertyToID("kernelSize"), LPFSize);

            csSpatialFiltering.SetTexture(kernel_KernelCreate, Shader.PropertyToID("LPF"), rt_Kernel);
            csSpatialFiltering.SetTexture(kernel_KernelCreate, Shader.PropertyToID("sumShow"), rt_Kernel_Sum);

            csSpatialFiltering.Dispatch(kernel_KernelCreate, CeilToInt(LPFSize / 8.0f), CeilToInt(LPFSize / 8.0f), 1);

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            int kernel_SFH = csSpatialFiltering.FindKernel("SpatialFilteringH");


            RenderTexture rt3 = new RenderTexture(texture.width, texture.height, 32, RenderTextureFormat.ARGB32);
            rt3.enableRandomWrite = true;
            rt3.Create();

            RenderTexture rt_FirstPass = new RenderTexture(texture.width, texture.height, 32, RenderTextureFormat.ARGB32);
            rt_FirstPass.enableRandomWrite = true;
            rt_FirstPass.Create();

            RenderTexture rt_SecondPass = new RenderTexture(texture.width, texture.height, 32, RenderTextureFormat.ARGB32);
            rt_SecondPass.enableRandomWrite = true;
            rt_SecondPass.Create();

            csSpatialFiltering.SetTexture(kernel_SFH, Shader.PropertyToID("Input"), texture);
            csSpatialFiltering.SetTexture(kernel_SFH, Shader.PropertyToID("LPF"), rt_Kernel);
            csSpatialFiltering.SetTexture(kernel_SFH, Shader.PropertyToID("KernelHor"), rt_Kernel_H);
            csSpatialFiltering.SetTexture(kernel_SFH, Shader.PropertyToID("FirstPass"), rt_FirstPass);

            csSpatialFiltering.Dispatch(kernel_SFH, CeilToInt(texture.width / 8.0f), CeilToInt(texture.height / 8.0f), 1);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            int kernel_SFV = csSpatialFiltering.FindKernel("SpatialFilteringV");

            csSpatialFiltering.SetTexture(kernel_SFV, Shader.PropertyToID("LPF"), rt_Kernel);
            csSpatialFiltering.SetTexture(kernel_SFV, Shader.PropertyToID("KernelVer"), rt_Kernel_V);
            csSpatialFiltering.SetTexture(kernel_SFV, Shader.PropertyToID("FirstPass"), rt_FirstPass);
            csSpatialFiltering.SetTexture(kernel_SFV, Shader.PropertyToID("SecondPass"), rt_SecondPass);

            csSpatialFiltering.Dispatch(kernel_SFV, CeilToInt(texture.width / 8.0f), CeilToInt(texture.height / 8.0f), 1);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            kernelTex.texture = rt_Laplacian_kernel;
            kernelSum.texture = rt_Kernel_Sum;
            kenelTexH.texture = rt_Kernel_H;
            kenelTexV.texture = rt_Kernel_V;
            firstPassTex.texture = rt_FirstPass;
            blurResult_Gpu.texture = rt_SecondPass;
            laplacianResult.texture = rt_Laplacian;

        }
        );

        btn_CPU.onClick.AddListener(delegate
        {
            Texture2D outputeTexture = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
            float[,] gaussianKernel = new float[kernelSize, kernelSize];
            List<float[]> sepreatKernels = new List<float[]>();
            Color[,] horCol = new Color[texture.width, texture.height];
            Color[,] filterRes = new Color[texture.width, texture.height];

            gaussianKernel = ImageProcessing.FSpecial(ImageProcessing.KernelType.gaussian, kernelSize, kernelSize, 1.0f);
            sepreatKernels = Sepreate(gaussianKernel, out float sum);


            int midr = (kernelSize - 1) / 2;
            int midc = (kernelSize - 1) / 2;

            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {

                    for (int i = -midr; i <= midr; i++)
                    {
                        if (m + i >= 0 && m + i <= texture.width - 1)
                            horCol[m, n] += texture.GetPixel(m + i, n) * sepreatKernels[0][i + midr];
                    }
                }
            }

            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {

                    for (int i = -midc; i <= midc; i++)
                    {
                        if (n + i >= 0 && n + i <= texture.height - 1)
                            filterRes[m, n] += horCol[m, n + i] * sepreatKernels[1][i + midc];
                    }
                }
            }

            for (int m = 0; m < texture.width; m++)
            {
                for (int n = 0; n < texture.height; n++)
                {
                    Color fc = filterRes[m, n] / sum;
                    outputeTexture.SetPixel(m, n, fc);
                }
            }
            outputeTexture.Apply();

            //outputeTexture = GaussianBlur(texture, kernelSize, kernelSize, kernelSize/6.0f, Boundary_Option.zero);
            blurResult_Cpu.texture = outputeTexture;
        });
    }


    void Update()
    {
    }


    static List<float[]> Sepreate(float[,] f, out float sum)
    {

        int r = f.GetUpperBound(0) + 1;
        int c = f.GetUpperBound(1) + 1;

        List<float[]> kernels = new List<float[]>();
        kernels.Clear();
        float[] horizon = new float[r];
        float[] vertical = new float[c];

        int midr = (r - 1) / 2;
        int midc = (c - 1) / 2;

        sum = 0;

        for (int s = 0; s < r; s++)
        {
            for (int t = 0; t < c; t++)
            {
                sum += f[s, t];
            }
        }

        for (int s = 0; s < r; s++)
        {
            horizon[s] = f[s, midc];
            Debug.Log("horizon[" + s + "]== " + horizon[s]);
        }

        for (int t = 0; t < c; t++)
        {
            vertical[t] = f[midr, t];
            Debug.Log("vertical[" + t + "]== " + vertical[t]);
        }

        kernels.Add(horizon); kernels.Add(vertical);
        Debug.Log("sum = " + sum);
        return kernels;
    }

}

 
