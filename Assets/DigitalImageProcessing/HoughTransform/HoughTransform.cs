using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using static DIP.ImageProcessing;
using TMPro;
using static UnityEngine.Mathf;

public class HoughTransform : MonoBehaviour
{

    [SerializeField] RawImage image0, image1, image2, image3;
    [SerializeField] Texture2D testTexture;
    Texture2D contentTexture, smoothTex, laplacianTex;
    Texture2D thresholdTex,houghTex;
    Texture2D hisTex, maskTex, maskResTex;

    LineRenderer lr;

    [SerializeField] int x = 50, y = 50;
    [SerializeField] int M = 101, N = 101;
    [SerializeField] Vector2 p0, p1, p2, p3, p4;
    [SerializeField,Range(0.005f,0.01f)] float step = 0.01f;



    float[] test = new float[8] { 0.25f, 0.84f, 0.458f, 0.25f, 0.31f, 0.84f, 0.26f, 0.84f };
    List<float> testL = new List<float>();
    private void Awake()
    {
        //for (int i = 0; i < test.Length; i++)
        //{
        //    testL.Add(test[i]);
        //}

        //float max = Max(testL.ToArray());
        //bool done = false;

        //List<int> indexs = new List<int>();
        //while (!done)
        //{
        //    if (testL.Contains(max))
        //    {
        //        int index = testL.FindIndex(x => x.Equals(max));
        //        testL[index] = 1f;

        //        Debug.Log(index);
        //        indexs.Add(index);
        //    }
        //    else
        //    {
        //        done = true;
        //    }
        //}

        //foreach (var i in indexs)
        //{
        //    Debug.Log("indexs[]==" + i);
        //}







        // Debug.Log("index==" + index);

    }
    // Start is called before the first frame update
    void Start()
    {
      
        image0.texture = testTexture;

        image1.texture = Hough( Im2bw(testTexture,0.5f));

        image0.SetNativeSize();
        image1.SetNativeSize();
    }

    // Update is called once per frame
    void Update()
    {
    

    }

    private void OnValidate()
    {
      

    }
}
