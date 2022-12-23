using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

namespace DIP
{
    public static partial class ImageProcessing
    {
        #region Nosise

        public static Texture2D PepperSaltNoise(Texture2D texture, float a = 0.05f, float b = 0.05f)
        {
            int M = texture.width; int N = texture.height;
            float ps = Clamp01(a);
            float pp = Clamp01(b);


            Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);

            Color sault = Color.white;
            Color pepper = Color.black;

            if (ps + pp < 1f)
            {
                for (int m = 0; m < M; m++)
                {
                    for (int n = 0; n < N; n++)
                    {

                        float u = UnityEngine.Random.Range(0f, 1f);
                        Color value = (u <= pp) ? pepper : (u > pp && u <= pp + ps) ? sault : texture.GetPixel(m, n);
                        output.SetPixel(m, n, value);
                    }
                }
            }
            else
            {
                Debug.LogError("The sum of  probility of salt and pepper must be less than 1.0 ");
            }

            output.Apply();
            return output;
        }
        public static Texture2D ExponentialNoise(Texture2D texture, float a = 10f)
        {
            int M = texture.width; int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);

            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Color value = texture.GetPixel(m, n);
                    float u = UnityEngine.Random.Range(0f, 1f);
                    float x = -1f / a * Log(1f - u);
                    Color exponetNoise = new Color(x, x, x);
                    output.SetPixel(m, n, value + exponetNoise);
                }
            }
            output.Apply();
            return output;
        }
        public static Texture2D GaussianNoise(Texture2D texture, float a = 0f, float b = 0.01f)
        {
            int M = texture.width; int N = texture.height;

            Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Color value = texture.GetPixel(m, n);
                    float g = a + Randn() * b;
                    Color guassianNoise = new Color(g, g, g);
                    output.SetPixel(m, n, value + guassianNoise);
                }
            }
            output.Apply();
            return output;
        }
        public static Texture2D UniformNoise(Texture2D texture, float a = 0f, float b = 0.1f)
        {
            int M = texture.width; int N = texture.height;

            Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Color value = texture.GetPixel(m, n);
                    float g = a + UnityEngine.Random.Range(0, 1f) * (b - a);
                    Color uniformNoise = new Color(g, g, g);
                    output.SetPixel(m, n, value + uniformNoise);
                }
            }
            output.Apply();
            return output;
        }
        public static Texture2D LogNormalNoise(Texture2D texture, float a = 1f, float b = 50f)
        {
            int M = texture.width; int N = texture.height;

            Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Color value = texture.GetPixel(m, n);
                    float g = Exp(a + b * Randn());
                    Color logNormalNoise = new Color(g, g, g);
                    output.SetPixel(m, n, value + logNormalNoise);
                }
            }
            output.Apply();
            return output;
        }
        public static Texture2D RayleighNosie(Texture2D texture, float a = 0f, float b = 0.01f)
        {
            int M = texture.width; int N = texture.height;

            Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    Color value = texture.GetPixel(m, n);
                    float g = a + Sqrt(-b * Log(1 - UnityEngine.Random.Range(0f, 1f)));
                    Color rayleighNosie = new Color(g, g, g);
                    output.SetPixel(m, n, value + rayleighNosie);
                }
            }
            output.Apply();
            return output;
        }
        public static Texture2D ErlandNosie(Texture2D texture, float a = 100f, float b = 5f)
        {
            int M = texture.width; int N = texture.height;
            Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);
            int B = RoundToInt(b);

            float k = -1f / a;
            if (b <= 0)
            {
                Debug.Log("b must be a positive integer for erland");
            }
            else
            {
                for (int m = 0; m < M; m++)
                {
                    for (int n = 0; n < N; n++)
                    {
                        float g = 0;
                        Color value = texture.GetPixel(m, n);

                        for (int j = 0; j < B; j++)
                        {
                            float u = UnityEngine.Random.Range(0f, 1f);
                            float x = k * Log(1f - u);
                            g = g + x;
                        }
                        Color erlandNosie = new Color(g, g, g);
                        output.SetPixel(m, n, value + erlandNosie);
                    }
                }
            }
            output.Apply();
            return output;
        }
        public static Texture2D ImNoise(int M, int N, Nosise_Type nosise_Type, float a = 0, float b = 1f)
        {

            Texture2D output = new Texture2D(M, N, TextureFormat.ARGB32, false);
            float g = 0;


            for (int m = 0; m < M; m++)
            {
                for (int n = 0; n < N; n++)
                {
                    float U = UnityEngine.Random.Range(0f, 1f);

                    switch (nosise_Type)
                    {
                        case Nosise_Type.uniform:
                            g = a + U * (b - a);
                            break;
                        case Nosise_Type.guassian:
                            g = a + Randn() * b;
                            break;
                        case Nosise_Type.pepperSalt:
                            g = (U <= a) ? 0f : (U > b && U <= a + b) ? 1f : 0.5f;
                            break;
                        case Nosise_Type.logNormal:
                            g = Exp(a + b * Randn());
                            break;
                        case Nosise_Type.exponential:
                            g = -1f / a * Log(1f - U);
                            break;
                        case Nosise_Type.rayleigh:
                            g = a + Sqrt(-b * Log(1f - U));
                            break;
                        case Nosise_Type.erland:
                            int B = RoundToInt(b);
                            for (int j = 0; j < B; j++)
                            {
                                float U2 = UnityEngine.Random.Range(0f, 1f);
                                float x = -1f / a * Log(1f - U2);
                                g = g + x;
                            }
                            break;
                    }
                    Color noiseCol = new Color(g, g, g);
                    output.SetPixel(m, n, noiseCol);
                }
                output.Apply();
            }
            return output;
        }

        #endregion
    }

}
