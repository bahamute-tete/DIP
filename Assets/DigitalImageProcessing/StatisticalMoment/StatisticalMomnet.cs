using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using static DIP.ImageProcessing;

public class StatisticalMomnet : MonoBehaviour
{

    [SerializeField] RawImage image0, image1, image2, image3, image4;
    [SerializeField] Button btn;
    [SerializeField] Texture2D tex0, tex1, tex2, tex3;
    Texture2D output0, output1, output2, chekerboard;
    int[,] se = new int[3, 3] { {1, 1, 1},{ 1, 1, 1 },{ 1, 1, 1 } };
    private void Awake()
    {
        output0 = new Texture2D(tex0.width, tex0.height, TextureFormat.RGB24, false);
        //output0 = AverageBlur(tex0, 9, 9,Boundary_Option.replicate);
        //float t = OtsuThresh(output0);
        output0 = Im2bw(tex0, 0.5f);
        image0.texture = Im2bw(tex0, 0.9f);
        image0.SetNativeSize();
        image1.texture =ComplementTex(output0);
        image1.SetNativeSize();
    }
    // Start is called before the first frame update
    void Start()
    {
        output1 = new Texture2D(output0.width, output0.height, TextureFormat.RGB24, false);
        output2 = new Texture2D(output0.width, output0.height, TextureFormat.RGB24, false);
        #region 1
        //btn.onClick.AddListener(delegate
        //{



        //    output1= ImMorphology(output0, se, Image_Type.BlackWhite, Morphological_Operator.Erosion);
        //    output1 = SubTex(output0, output1);

        //    List<Vector2> pos = MooreTracing(output0, SortDirection.CW);
        //    for (int n = 0; n < output1.height; n++)
        //    {
        //        for (int m = 0; m < output1.width; m++)
        //        {
        //            output1.SetPixel(m, n, Color.black);
        //        }
        //    }

        //    for (int i = 0; i < pos.Count; i++)
        //    {

        //       // Debug.Log("pos[" + i + "]=" + pos[i]);
        //        output1.SetPixel((int) pos[i].x, (int) pos[i].y, Color.white);
        //    }

        //    output1.Apply();

        //    image2.texture = output1;
        //    image2.SetNativeSize();


        //    int row = 11;
        //    int column = 11;
        //    float sizeX = (float)output1.width / row;
        //    float sizeY = (float)output1.height / column;

        //    chekerboard = Checkerboard((int)sizeX, 11);
        //    image4.texture = chekerboard;
        //    image4.SetNativeSize();

        //    for (int m = 0; m < output2.width; m++)
        //    {
        //        for (int n = 0; n < output2.height; n++)
        //        {
        //            output2.SetPixel(m, n, Color.black);
        //        }
        //    }


        //    List<Vector2> resample = new List<Vector2>();

        //    for (int i = 0; i < pos.Count; i++)
        //    {
        //        float mincellX = Floor( pos[i].x / sizeX)*sizeX;
        //        float maxcellX = Ceil( pos[i].x / sizeX)*sizeX;


        //        float mincellY = Floor(pos[i].y / sizeY)*sizeY;
        //        float maxcellY = Ceil(pos[i].y / sizeY)*sizeY;



        //        float disXmax = Abs(pos[i].x - maxcellX-1f);
        //        float disXmin = Abs(pos[i].x - mincellX);
        //        float nx = (disXmax > disXmin) ? mincellX : maxcellX;

        //        float disYmax = Abs(pos[i].y - maxcellY-1f);
        //        float disYmin = Abs(pos[i].y - mincellY);
        //        float ny = (disYmax > disYmin) ? mincellY : maxcellY;


        //        Vector2 resPos = new Vector2(nx, ny);
        //        resample.Add(resPos);

        //        output2.SetPixel((int)(resPos.x ), (int)(resPos.y ), Color.white);
        //    }


        //    output2.Apply();
        //    image3.texture = output2;
        //    image3.SetNativeSize();


        //});
        #endregion

        #region 2
        //btn.onClick.AddListener(delegate
        //{
        //    int[,] id = ConnectedComponentLable(ComplementTex(output0), Neighborhood_type.Four);
        //    int[] id_1D = new int[tex0.width * tex0.height];
        //    Texture2D output = new Texture2D(tex0.width, tex0.height, TextureFormat.RGB24, false);

        //    for (int n = 0, i = 0; n < tex0.height; n++)
        //    {
        //        for (int m = 0; m < tex0.width; m++, i++)
        //        {
        //            id_1D[i] = id[m, n];
        //        }
        //    }

        //    int max = Max(id_1D);
        //    Color[] colors = new Color[max];
        //    for (int i = 0; i < max; i++)
        //    {
        //        colors[i] = UnityEngine.Random.ColorHSV(0f, 1f);
        //    }


        //    for (int n = 0; n < tex0.height; n++)
        //    {
        //        for (int m = 0; m < tex0.width; m++)
        //        {
        //            for (int i = 0; i < max; i++)
        //            {
        //                if (id[m, n] == i + 1)
        //                    output.SetPixel(m, n, colors[i]);
        //            }
        //        }
        //    }
        //    output.Apply();
        //    image2.texture = output;
        //});
        #endregion

        btn.onClick.AddListener(delegate
        {
            int M = tex0.width;
            int N = tex0.height;

            float t = GrayThresh(tex0);

            List<Vector2> edges = MooreTracing(Im2bw(tex0, t), Neighborhood_type.Eight);
            List<int> fCode = FChainEncode(edges);
            List<Vector2> dCode = FChainnDecode(fCode, new Vector2(20f,20f));



            Texture2D  edgeTex = SetPixelPos(dCode, Color.white);
            image2.texture = edgeTex;
            image2.SetNativeSize();

            //float p = ShapePerimeter(edges);
            //float a = ShapeArea(Im2bw(tex0, t));

            //Debug.Log("per =" + p);
            //Debug.Log("are =" + a);

            //float compactness = Pow(p, 2f) / a;
            //Debug.Log("compactness =" + compactness);

            //float circularity = 4f * PI * a / Pow(p, 2f);
            //Debug.Log("circularity =" + circularity);


            
            //Debug.Log(IsBinaryImage(tex0));
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    float Moment(float[] datas, int rank)
    {
        float res = 0;
        int n = datas.Length;


        return res;
    }


}
