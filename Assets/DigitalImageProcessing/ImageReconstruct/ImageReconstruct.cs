using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DIP.ImageProcessing;
using static UnityEngine.Mathf;


public class ImageReconstruct : MonoBehaviour
{

    [SerializeField] Texture2D tex, tex1;
    [SerializeField] RawImage image0, image1, image2;
    [SerializeField] Button btn0, btn1;

    [SerializeField] float sig = 1f, mean =0f;
    [SerializeField] float T = 1f, a = 0.1f, b = 0.1f;
    int M, N;
    // Start is called before the first frame update
    void Start()
    {
        M = tex.width;
        N = tex.height;

        image0.texture = tex;
        image0.SetNativeSize();


        //image1.texture =AddTex( tex1, ImNoise(M, N, Nosise_Type.guassian, mean, sig));
        //image1.SetNativeSize();
        btn0.onClick.AddListener(delegate
        {
            Vector2[,] fftDatas = Convert2Complex(tex);

            fftDatas = FFT2(fftDatas);
            Vector2[,] H = new Vector2[M, N];
            for (int u = 0; u < M; u++)
            {
                
                for (int v = 0; v < N; v++)
                {
                    float W = -1f * PI * (a * u  + b * v );
                    Vector2 ex = new Vector2(Cos(W), Sin(W));

                    float S = Sin(PI * (a * u + b * v));
                    float A = T / (PI * (a * u + b * v) + Epsilon);
                    Vector2 f = A * S * ex;

                    H[u, v] = f;
                }
            }

            //foreach (var h in H)
            //{
            //    Debug.Log(h);
            //}

            Vector2[,] res = FilterMul(fftDatas, H);

            Texture2D output = ImIFFT2(IFFT2(res), M, N);

            ////// Texture2D output = ImFFT2(fftDatas);
            image1.texture = output;
            image1.SetNativeSize();
        });

        btn1.onClick.AddListener(delegate
        {

            Texture2D res = SpFilt(tex1, FilterType.AlphaTrim, 3, 3, 6);

            res = SpFilt(res, FilterType.Geometric, 5, 5);
            //res = SpFilt(res, FilterType.Median, 3, 3);
            image2.texture = res;
            image2.SetNativeSize();

            //Texture2D output = Checkerboard(13,7,7);
            //image2.texture = output;
            //image2.SetNativeSize();
        });


       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
