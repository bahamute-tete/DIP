using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public class TaylorSeries : MonoBehaviour
{
    [SerializeField]int k = 4;
    [SerializeField] float x = 1;
    // Start is called before the first frame update
    void Start()
    {

        
        
        
    }


    private void OnValidate()
    {
        //float res = 0;
        //res = TaylorExp(x, k);
        //Debug.Log("exp(" + x + ") =" + res);

        //float resin = 0;
        //resin = TaylorSin(x, k);
        //Debug.Log("sin(" + x + ") =" + resin);

        float rescos = 0;
        rescos = TaylorCos(x, k);
        Debug.Log("cos(" + x + ") =" + rescos);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    float TaylorExp(float x,int k)
    {
        float res = 0;

        for (int i = 0; i < k; i++)
        {
            res += Power(x, i) / Factorial(i);
        }

        return res;
    }

    float TaylorSin(float x, int k)
    {
        float res = 0;

        for (int i = 0; i < k; i++)
        {
            res += Power(-1, i) * Power(x, 2 * i + 1) / Factorial(2 * i + 1);
        }

        return res;
    }

    float TaylorCos(float x, int k)
    {
        float res = 0;

        for (int i = 0; i < k; i++)
        {
            res += Power(-1, i) * Power(x, 2 * i) / Factorial(2 * i);
        }
        
        return res;
    }

    float Factorial(int k)
    {
        float res = 0;
        if (k == 0)
            return 1;

        for (int i = 0; i < k; i++)
        {
            float s = 1f;
            for (int j = 1; j <= i + 1; j++)
            {
                s *= j;
            }
            res = s;
        }

        return res;
    }

    float Power(float x,int k)
    {

        float powRes = x;

        if (k == 0)
            return 1;
        if (k == 1)
            return x;

        for (int i = 1; i < k; i++)
        {
            powRes *= x;
        }


        //Debug.LogError(powRes);
        return powRes;
    }
}
