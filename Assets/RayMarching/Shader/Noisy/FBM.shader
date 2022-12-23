Shader "RayMarch/FBM"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Octaves("_Octaves",int) =1
		_Lacunarity("_Lacunarity",Range(2,10))=2 
		_Gain("_Gain",Range(0.5,10))=0.5

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
			int _Octaves;
			float _Lacunarity,_Gain;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv-0.5;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float4 col = 0;
				float2 uv = i.uv*_ScreenParams/_ScreenParams.y;

			    int octaves = _Octaves;
				float lacunarity = _Lacunarity;
				float gain =_Gain;

				float xpivot=smoothstep(0.01,-0.01,abs(uv.y));
				float ypivot=smoothstep(0.01,-0.01,abs(uv.x));
				col+=xpivot*float4(1,0,0,1);
				col+=ypivot*float4(0,1,0,1);

				uv *= 10;
				float  amplitude =1.0;
				float frequency =1.0;
				float t = 0;
				float y =0;
				//float y =amplitude* sin(uv.x*frequency+t);
				//y += sin(uv.x*frequency*2.1 + t)*4.5;
				//y += sin(uv.x*frequency*1.72 + t*1.121)*4.0;
				//y += sin(uv.x*frequency*2.221 + t*0.437)*5.0;
				//y += sin(uv.x*frequency*3.1122+ t*4.269)*2.5; 
				//y*=amplitude*0.05;
				//float c = smoothstep(0.1,0,abs(y-uv.y));

				for(int oc =0 ; oc<octaves ;oc++)
				{
					 //y +=amplitude* sin(uv.x*frequency);
					 //frequency *=lacunarity;
					 //amplitude *= gain;

					 frequency = pow( 2.0, float(oc) );
				     amplitude = pow( frequency, -gain );
					 y +=amplitude* sin(uv.x*frequency);
				}

				

				float c = smoothstep(0.1,0,abs(y-uv.y));

				col+=c;

                return col;
            }
            ENDCG
        }
    }
}
