using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using static DIP.ImageProcessing;

public class ScreenDraw : MonoBehaviour
{
    [SerializeField] RawImage screen;
    int ScreenW => Screen.width;
    int ScreenH => Screen.height;
    Texture2D drawTex;
    [SerializeField] int size =55;
    [SerializeField] Texture2D texture;

    Rect rec;
    Vector2 minPos;
    float w ;
    float h;
    private void Awake()
    {
        drawTex = new Texture2D(256, 256, TextureFormat.ARGB32, false);

        for (int m = 0; m < drawTex.width; m++)
        {
            for (int n = 0; n < drawTex.height; n++)
            {
                Color bgCol = Color.black;
                drawTex.SetPixel(m, n, bgCol);
            }
        }
        drawTex.Apply();
        screen.texture = drawTex;
        screen.SetNativeSize();
        screen.rectTransform.anchorMin = Vector2.zero;
        screen.rectTransform.anchorMax = Vector2.zero;
        screen.rectTransform.anchoredPosition= new Vector2(Screen.width / 2f, Screen.height / 2);

          w= screen.rectTransform.sizeDelta.x / 2;
          h = screen.rectTransform.sizeDelta.y / 2;

        minPos= screen.rectTransform.anchoredPosition - screen.rectTransform.sizeDelta / 2f;
        rec = new Rect(minPos, screen.rectTransform.sizeDelta);
    }

    private void Start()
    {
       


    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = Input.mousePosition;

        if (rec.Contains(mousePos))
        {
            Vector2 mousePosInTex = mousePos - minPos;
            int m =FloorToInt( mousePosInTex.x);
            int n = FloorToInt(mousePosInTex.y);


            if (Input.GetMouseButtonDown(0))
            {
                drawTex.SetPixel(m, n, Color.white);

                RasterizationLine(new Vector2(w,h), mousePosInTex, drawTex);
              
                //drawTex =GussianBlur(drawTex, 3, 3, 1, Boundary_Option.zero);
                drawTex.Apply();
            }

            screen.texture = drawTex;
        }


        
    }


    void RasterizationLine(Vector2 start,Vector2 end,Texture2D tex)
    {
        float x0 = start.x;float y0 = start.y;
        float x1 = end.x;float y1 = end.y;


        Color lineCol = Color.white;
        float m = (y1 - y0) / (x1 - x0);
      
        float y =y0 ;
        float x =x0;
        int j = FloorToInt(y0);
        int k = FloorToInt(x0);

        float d = 0;
        d = (y0 - y1) * (x0 + 1f) + (x1 - x0) * (y + 0.5f) + x0 * y1 - x1 * y0;

        if (m <= 1f && m > -1f)
        {
            if (x0 > x1)
            {
                for (int i = FloorToInt(x0); i > FloorToInt(x1); i--)
                {
                    tex.SetPixel(i, j, Color.white);
                    if (m > 0 && m <= 1f)
                    {
                        if (d < 0)
                        {
                            j = j - 1;
                            d = d - (x1 - x0) - (y0 - y1);
                        }
                        else
                        {
                            d = d - (y0 - y1);
                        }
                    }
                    else 
                    {
                        if (d > 0)
                        {
                            j = j + 1;
                            d = d + (x1 - x0) - (y0 - y1);
                        }
                        else
                        {
                            d = d - (y0 - y1);
                        }
                    }

                }
            }
            else
            {
                for (int i = FloorToInt(x0); i < FloorToInt(x1); i++)
                {
                    tex.SetPixel(i, j, Color.white);
                    if (m > 0 && m <= 1f)
                    {
                        if (d < 0)
                        {
                            j = j + 1;
                            d = d + (x1 - x0) + (y0 - y1);
                        }
                        else
                        {
                            d = d + (y0 - y1);
                        }
                    }
                    else 
                    {
                        if (d > 0)
                        {
                            j = j - 1;
                            d = d - (x1 - x0) + (y0 - y1);
                        }
                        else
                        {
                            d = d + (y0 - y1);
                        }
                    }
                }
            }
            
        }
        else
        {

            if (y0 < y1)
            {
                for (int n = FloorToInt(y0); n < FloorToInt(y1); n++)
                {
                    tex.SetPixel(k, n, Color.white);
                    if (m > 1f)
                    {
                        if (d < 0)
                        {
                            k = k + 1;
                            d = d - (x1 - x0) - (y0 - y1);
                        }
                        else
                        {
                            d = d - (x1 - x0);
                        }
                    }
                    else
                    {
                        if (d < 0)
                        {
                            k = k - 1;
                            d = d + (x1 - x0) - (y0 - y1);
                        }
                        else
                        {
                            d = d + (x1 - x0);
                        }
                    }

                }
            }
            else
            {
                for (int n = FloorToInt(y0); n > FloorToInt(y1); n--)
                {
                    tex.SetPixel(k, n, Color.white);

                    if (m < -1f)
                    {
                        if (d < 0)
                        {
                            k = k + 1;
                            d = d - (x1 - x0) + (y0 - y1);
                        }
                        else
                        {
                            d = d - (x1 - x0);
                        }
                    }
                    else
                    {
                        if (d > 0)
                        {
                            k = k - 1;
                            d = d - (x1 - x0) - (y0 - y1);
                        }
                        else
                        {
                            d = d - (x1 - x0);
                        }
                    }

                }
            }
            //////////////////////////////////////////////////////
            #region 2
            //if (y0 < y1 && x0 < x1)
            //{
            //    for (int n = FloorToInt(y0); n < FloorToInt(y1); n++)
            //    {
            //        tex.SetPixel(k, n, Color.white);
            //        if (m > 1f)
            //        {
            //            if (d < 0)
            //            {
            //                k = k + 1;
            //                d = d - (x1 - x0) - (y0 - y1);
            //            }
            //            else
            //            {
            //                d = d - (x1 - x0);
            //            }
            //        }

            //    }
            //}


            //if (y0 < y1 && x0 > x1)
            //{
            //    for (int n = FloorToInt(y0); n < FloorToInt(y1); n++)
            //    {
            //        tex.SetPixel(k, n, Color.white);
            //        if (m < - 1f)
            //        {
            //            if (d < 0)
            //            {
            //                k = k - 1;
            //                d = d + (x1 - x0) - (y0 - y1);
            //            }
            //            else
            //            {
            //                d = d + (x1 - x0);
            //            }
            //        }

            //    }
            //}


            //if (y0 > y1 && x0 < x1)
            //{
                
            //    for (int n = FloorToInt(y0); n > FloorToInt(y1); n--)
            //    {
            //        tex.SetPixel(k, n, Color.white);
                    
            //        if (m <- 1f)
            //        {
            //            if (d < 0)
            //            {
            //                k = k + 1;
            //                d = d - (x1 - x0) + (y0 - y1);
            //            }
            //            else
            //            {
            //                d = d - (x1 - x0);
            //            }
            //        }
            //    }
            //}

            //if (y0 > y1 && x0 > x1)
            //{

            //    for (int n = FloorToInt(y0); n > FloorToInt(y1); n--)
            //    {

            //        tex.SetPixel(k, n, Color.white);
                    
            //        if (m > 1f)
            //        {
            //            if (d > 0)
            //            {
            //                k = k - 1;
            //                d = d - (x1 - x0) - (y0 - y1);
            //            }
            //            else
            //            {
            //                d = d - (x1 - x0);
            //            }
            //        }
            //    }
            //}

            #endregion
        }



    }

   


}
