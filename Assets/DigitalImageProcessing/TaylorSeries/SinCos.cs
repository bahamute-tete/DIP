using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinCos : MonoBehaviour
{
    [SerializeField] float f;

    float curf = 0;
    // Start is called before the first frame update
    void Start()
    {
        curf = f;
    }

    // Update is called once per frame
    void Update()
    {
        if (curf != f)
        {
           float a = Mathf.Abs(f) * 256 / 2 / Mathf.PI;
            int  i = Mathf.FloorToInt(a);
            Debug.Log("a " + a);
            Debug.Log("i " + (i));
            Debug.Log("i & 255 =" + (i & 255));
            curf = f;                                                                                   

        }

         

        
    }


    Vector2 SineCosine(float f)
    {

        Vector2 res = new Vector2();
        float a = Mathf.Abs(f) * 256 / 2 / Mathf.PI;
        int i = Mathf.FloorToInt(a);

        float b = (a - i) * 2 * Mathf.PI / 256;

        Vector2[] alphaCosSin = new Vector2[i & 255];

        float b2 = b * b;
        float sin_beta = b - b * b2 * (0.166666667f - b2 * 0.0083333333f);
        float cos_beta = 1f - b2 * (0.5f - b2 * 0.0416666667f);

        //float sine=

        return res;
    }
}
