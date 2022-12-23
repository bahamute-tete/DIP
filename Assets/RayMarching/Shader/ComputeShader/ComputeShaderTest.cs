using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComputeShaderTest : MonoBehaviour
{

    [SerializeField] Texture tex;
    [SerializeField] ComputeShader _cs;

    public RawImage image;

    [SerializeField]Vector2[] uvs;
    private ComputeBuffer csbuffer;

    // Start is called before the first frame update
    void Start()
    {
        int length = tex.width * tex.height;
        uvs = new Vector2[length];
        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                uvs[i * tex.height + j] = new Vector2(i,j);
            }
        }

        csbuffer = new ComputeBuffer(length, 8);
        csbuffer.SetData(uvs);


#if UNITY_EDITOR
        Debug.LogFormat("cs start time = {0}", Time.realtimeSinceStartup);
#endif
        RenderTexture rt = new RenderTexture(tex.width, tex.height, 24);
        rt.enableRandomWrite = true;
        rt.Create();
        image.texture = rt;
        image.SetNativeSize();


        int kernel = _cs.FindKernel("CSMain");
        _cs.SetBuffer(kernel, "uvs", csbuffer);
        _cs.SetInt( "width", tex.width);
        _cs.SetInt("height", tex.height);
        _cs.SetTexture(kernel, "Inpute", tex);
        _cs.SetTexture(kernel, "Result", rt);
        _cs.Dispatch(kernel, tex.width / 8, tex.height / 8, 1);


#if UNITY_EDITOR
        Debug.LogFormat("cs start time = {0}", Time.realtimeSinceStartup);
#endif
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
