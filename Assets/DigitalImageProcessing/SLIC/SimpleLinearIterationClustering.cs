using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using static DIP.ImageProcessing;

public class SimpleLinearIterationClustering : MonoBehaviour
{
    [SerializeField] Texture2D tex;
    [SerializeField] Button btn;
    Texture2D output;
    // Start is called before the first frame update
    void Start()
    {
        output = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);

        btn.onClick.AddListener(delegate
        {
            Vector2[,] nhtes = GetNeighborhoodPos(1, 1, 1);

            
        });

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SLIC(Texture2D tex, int nsp)
    {
        int M = tex.width;
        int N = tex.height;
        float ntp = M * N;
        float s =Sqrt(ntp / nsp);

        List<Vector2> spCenters = new List<Vector2>();

        float x = s;
        float y = s;


        for (int i = 0; i < nsp; i++)
        {
            
        }

        ////Spixels Center
        

        ///// adjust to min grand pos

        for (int i = 0; i < spCenters.Count; i++)
        {
            //3*3 neighborhood
            Vector2[,] nhs = GetNeighborhoodPos((int)spCenters[i].x,(int)spCenters[i].y, 1);
        }
    }
}
