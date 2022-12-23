Shader "TestShader/AnimatLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100



		Pass{
			HLSLPROGRAM 
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
			float2 _pos=0;
			float2 _pos2=0;
			
			float sdPoint (float2 p,float2 pc)
			{
				float2 d = p -pc;
				return length(d);
			}

			float sdLine (float2 p ,float2 a ,float2 b)
			{
				float2 pa = p-a;
				float2 ba = b-a;
				float h = clamp(dot(pa,ba)/dot(ba,ba),0,1);
				float2 d = pa -ba*h;
				return length(d);
			}


			float2 cmul (float2 a ,float2 b)
			{
				return float2 (a.x*b.x - a.y*b.y, a.x*b.y + a.y*b.x);
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv-0.5;
               
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
               
                float4 col = 0;
				float2 uv =i.uv*_ScreenParams/_ScreenParams.y; 
				float2 p = uv;

				
				float2 v= float2(1,0);
				float t = _Time.x;
				float dt =0;
				dt=unity_DeltaTime.z;
				
				p +=v*dt;


				float pd =sdPoint(uv,p);
				
				col +=smoothstep(0.05,0.04,pd)*float4 (1,0,0,1);

                return col;
            }
			ENDHLSL 
		}
    }
}
