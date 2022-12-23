

Shader "RayMarch/CubemapToEquirectangular" {
  Properties {
        _MainTex ("Cubemap (RGB)", CUBE) = "" {}
    }

    Subshader {
        Pass {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }      

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                //#pragma fragmentoption ARB_precision_hint_fastest
                #pragma fragmentoption ARB_precision_hint_nicest
                #include "UnityCG.cginc"

                #define PI    UNITY_PI
                #define TAO 2*UNITY_PI

				struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

                struct v2f {
                    float4 pos : POSITION;
                    float2 uv : TEXCOORD0;
                };

                samplerCUBE _MainTex;

                v2f vert( appdata v )
                {
                    v2f o;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv*float2(TAO,PI);
                    return o;
                }

                fixed4 frag(v2f i) : COLOR 
                {
                    float t = i.uv.y;
                    float p = i.uv.x;
                    float3 pos = float3(0,0,0);

                    pos.x = sin(p) * sin(t) ;
                    pos.y = cos(t);
                    pos.z =- cos(p) * sin(t) ;

                    return texCUBE(_MainTex, pos);
                }
            ENDCG
        }
    }
    Fallback Off
}