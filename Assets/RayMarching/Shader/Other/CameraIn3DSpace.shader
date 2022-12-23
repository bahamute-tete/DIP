Shader "RayMarch/3DSpaceCamera"
{
    Properties
    {
        _CameraPosition("CameraPos",Vector)= (0,0,-5,0)
        _CameraZoom ("CameraZoom",Range(1,2)) =1
        _LookAtPoint("LookAtPoint",Vector)= (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"


float sdPoint (float3 ro,float3 rd,float3 p)
{
    float3 po = p-ro;
    float d =length(cross(po,rd))/length(rd);    
    return d;
}

float DrawPoint (float3 ro,float3 rd,float3 p)
{
    float d = sdPoint(ro,rd,p);
    d= smoothstep(0.06,0.05,d);
    return d;
}

float PointCube(float3 ro,float3 rd)
{
    float pd =0;
    pd = DrawPoint(ro,rd,float3(-1,-1,-1));
    pd +=DrawPoint(ro,rd,float3(1,-1,-1));
    pd +=DrawPoint(ro,rd,float3(1,-1,1));
    pd +=DrawPoint(ro,rd,float3(-1,-1,1));

    pd +=DrawPoint(ro,rd,float3(-1,1,-1));
    pd +=DrawPoint(ro,rd,float3(1,1,-1));
    pd +=DrawPoint(ro,rd,float3(1,1,1));
    pd +=DrawPoint(ro,rd,float3(-1,1,1));

    return pd;
}




            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _CameraPosition;
            float4 _LookAtPoint;
            float _CameraZoom;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv-0.5;
                o.uv = o.uv*_ScreenParams/_ScreenParams.y;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col =0;

                float3 targetPoint = _LookAtPoint.xyz;
                float zoom = _CameraZoom;


                float3 ro = _CameraPosition.xyz;
                ro.x = _CameraPosition.z*sin(_Time.y);
                ro.z =_CameraPosition.z*cos(_Time.y);
                

                float3 lookat =normalize(targetPoint- ro); 
                float3 right = cross(float3(0,1,0),lookat);
                float3 up = cross(lookat,right);
                float3 c = ro+zoom*lookat;
                float3 intersectionPoint =  c+ i.uv.x*right+i.uv.y*up;

                float3 rd = intersectionPoint-ro;


                float pd = 0;
                pd =PointCube(ro,rd);

                


                col.rgb = pd; 
                return col;
            }
            ENDCG
        }
    }
}
