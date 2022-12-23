using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DIP.ImageProcessing;
using static UnityEngine.Mathf;
using System;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using System.Numerics;

public class FrequencyDIP : MonoBehaviour
{

    [SerializeField] RawImage image0, image1, image2;
    [SerializeField] Texture2D texture;
    [SerializeField] Button button, btn2;
    Texture2D foutput, fftIm;

    [SerializeField] int W = 0, H = 0;
    // Start is called before the first frame update
    void Start()
    {
        foutput = new Texture2D(512, 512, TextureFormat.RGB24, false);

        image0.texture = texture;
        image0.SetNativeSize();


        Vector2[,] dft = Convert2Complex(texture);
        button.onClick.AddListener(delegate
        {
            
            dft = FFT2(dft);
            fftIm = ImFFT2(dft);
            image2.texture = fftIm;
            image2.SetNativeSize();
        });

        
        
        btn2.onClick.AddListener(delegate
        {
            int M = fftIm.width;
            int N = fftIm.height;

            Vector2[,] fres = new Vector2[M, N]; 
            for (int x = 0; x < M; x++)
            {
                for (int y = 0; y < N; y++)
                {
                    //foutput.SetPixel(x, y, fftIm.GetPixel(x, y));
                    fres[x, y] = dft[x, y];
                    if (x > M/2-W && x < M/2+W)
                    {
                        if ( y < N/2-H || y > N/2+H)
                        {
                            fres[x, y] = dft[x, y] * Vector2.zero;
                           // foutput.SetPixel(x, y, Color.black);
                        }

                    }
                }
            }

            foutput = ImIFFT2(IFFT2(fres), M, N);
            

            image1.texture = foutput;
            image1.SetNativeSize();
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
