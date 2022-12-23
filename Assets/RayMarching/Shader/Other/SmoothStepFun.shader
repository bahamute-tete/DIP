Shader "RayMarch/SmoothStepFun"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;

                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

//////////////////////////////////////////////////////////////////////////
			float mix(float a,float b,float c)
			{
				return a*(1-c)+b*c;
			}

			float2 mix(float2 a,float2 b,float c)
			{
				return a*(1-c)+b*c;
			}

			float3 mix(float3 a,float3 b,float c)
			{
				return a*(1-c)+b*c;
			}

			float4 mix(float4 a,float4 b,float c)
			{
				return a*(1-c)+b*c;
			}
/////////////////////////////////////////////////////////////////////
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col = 0;
				float2 uv = (i.uv-0.5)*_ScreenParams/_ScreenParams.y; 
				float fork =smoothstep(0.0,0.5,uv.y);
				float s1 = smoothstep(mix(0.05,0.02,fork),0.00,abs(abs(uv.x)-0.1*fork));
				
                
				col =s1;
                return col;
            }
            ENDCG
        }
    }
}
