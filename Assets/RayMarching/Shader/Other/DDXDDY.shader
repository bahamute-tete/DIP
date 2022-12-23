Shader "RayMarch/DDXDDY"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MapTex ("_MapTex",2D) = "white" {}
		_Intensity("_Intensity",Range(0,0.1)) = 0
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
            // make fog work
            #pragma multi_compile_fog

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
			 sampler2D _MapTex;
            float4 _MapTex_ST;
			float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = v.uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
				float2 uv = i.uv *_ScreenParams/_ScreenParams.y;
                float4 col = 0;
				float4 lineCol=0;
				
				col =tex2D (_MapTex ,i.uv);
				float gray  = dot (col.rgb,float3 (0.2125,0.7154,0.0721));
				//float cwidth= ddx(gray)+ddy(gray);
				float cwidth=fwidth(gray);
				float lineff =1-smoothstep(0.00,0.1,cwidth);
				//if (cwidth-_Intensity>0.0)
				//lineCol=float4(0,0,1,1);
				
				col +=lineff*2;

                return col;
            }
            ENDCG
        }
    }
}
