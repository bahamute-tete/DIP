using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;
using static DIP.ImageProcessing;
using UnityEngine.EventSystems;

public class LineRasterization : MonoBehaviour
{
    [SerializeField] RawImage screen;
    int ScreenW => Screen.width;
    int ScreenH => Screen.height;
    Texture2D drawTex;
   
    Rect rec;
    Vector2 minPos;
    float w ;
    float h;

    int pointCount = 0;
    Vector2 startPos;
    Vector2 originalPos;
    Vector2 currentPos;
    bool isConnect = false;
    //[SerializeField] Image pointerCir;
    [Min(3),SerializeField] int lineWidth=5;

    [SerializeField] Slider lineSlider;
    private void OnEnable()
    {
        //pointerCir.gameObject.SetActive(false);
    }
    private void Awake()
    {
        drawTex = new Texture2D(ScreenW, ScreenH, TextureFormat.ARGB32, false);

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
        //screen.rectTransform.anchorMin = Vector2.zero;
        //screen.rectTransform.anchorMax = Vector2.zero;
        //screen.rectTransform.anchoredPosition= new Vector2(Screen.width / 2f, Screen.height / 2);

        //  w= screen.rectTransform.sizeDelta.x / 2;
        //  h = screen.rectTransform.sizeDelta.y / 2;

        //minPos= screen.rectTransform.anchoredPosition - screen.rectTransform.sizeDelta / 2f;
        //rec = new Rect(minPos, screen.rectTransform.sizeDelta);

        //lineSlider.value = lineWidth;
        //lineSlider.maxValue = 25f;
        //lineSlider.minValue = 3f;
        

        //lineSlider.onValueChanged.AddListener(delegate 
        //{
        //    lineWidth =(int) lineSlider.value;
        //    if ((lineWidth & 1) != 1)
        //        lineWidth -=1;
        //});


    }

    private void Start()
    {
       


    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        //pointerCir.gameObject.SetActive(false);

            if (rec.Contains(mousePos))
            {
                Vector2 mousePosInTex = mousePos - minPos;
                int m = FloorToInt(mousePosInTex.x);
                int n = FloorToInt(mousePosInTex.y);

                if (pointCount >= 2)
                {
                    currentPos = new Vector2(m, n);
                    float d = Vector2.SqrMagnitude(currentPos - originalPos);
                    //Debug.Log("d ==" + Sqrt(d));
                    if (d < 100f)
                    {
                        currentPos = originalPos;
                        isConnect = true;
                        //pointerCir.gameObject.SetActive(true);
                        //pointerCir.rectTransform.anchoredPosition = mousePos;
                        //Debug.Log("Connect"+ isConnect);
                    }
                }

                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit = new RaycastHit();
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    pointCount++;

                    if (pointCount == 1)
                    {
                        startPos = new Vector2(m, n);
                        originalPos = new Vector2(m, n);
                        //Debug.Log("startPos==" + startPos);
                    }

                    if (pointCount > 1)
                    {
                        if (!isConnect)
                            currentPos = new Vector2(m, n);

                   
                        drawTex.SetPixel(m, n, Color.white);
                        List<Vector2> drawPosSets = new List<Vector2>();
                        drawPosSets.Clear();
                        drawPosSets = ConnetcPoints(startPos, currentPos);
                        foreach (var pos in drawPosSets)
                        {
                            for (int s = -(lineWidth - 1) / 2; s <= (lineWidth - 1) / 2; s++)
                            {
                                for (int t = -(lineWidth - 1) / 2; t <= (lineWidth - 1) / 2; t++)
                                {
                                    int X = (int)(pos.x + s);
                                    int Y = (int)(pos.y + t);
                                    if (X >= 0 && X < drawTex.width && Y >= 0 && Y < drawTex.height)
                                        drawTex.SetPixel(X, Y, Color.white);
                                }
                            }
                        }

                        if (isConnect)
                        {
                            isConnect = false;
                            pointCount = 0;
                        }
                    }

                    drawTex.Apply();
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (pointCount > 1)
                    {
                        startPos = currentPos;
                        currentPos = Vector2.zero;
                    }
                }

                screen.texture = drawTex;
            } 
    }



}
