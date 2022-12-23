Shader "RayMarch/StarField"
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

			float2x2 Rot(float a)
			{
				float s = sin(a);
				float c = cos(a);
				return float2x2(c,-s,s,c);
			}

			float2 hash22(float2 p)
			{
				float3 a= frac(p.xyy*float3(356.23,786.12,543.36));
				a+=dot(a,a.yzx+34.56);
				return  frac( float2(a.x*a.y,a.y*a.z));
			}

			float Star (float2 p,float flar)
			{
			    float m =0;
				float d = length(p);
				float glow = 0.03/d;
				m+=glow;
				float ray =max(0,1- abs(p.x*p.y*1000));
				m  +=ray*flar;
				p = mul(Rot(UNITY_PI/4),p);
				float ray2 =max(0,1- abs(p.x*p.y*1000));
				m  +=ray2*0.2*flar;

				m *= smoothstep(0.8,0.3,d);
				return m;
			}

			float4 StarLayer(float2 uv)
			{
				float2 gv = frac(uv)-0.5;
				float2 id = floor(uv);
				float4 starCol =0;
				float4 sl=0;
				for (int y =-1;y<=1;y++)
				{
					for(int x= -1;x<=1;x++)
					{
						float2 uvoff = float2(x,y);
						float2 n = hash22(id+uvoff);
						float size = n.y*1;
					    starCol = (sin(float4(0.2,0.3,0.9,1)*n.x*156))*0.5+0.5;
						starCol = starCol*float4(0.75,0.40,0.8+size,1);
						starCol *= sin(_Time.y*2+ dot(n,float2(123.35,459.32)))*0.5+0.5;//splash star
						sl += Star(gv-uvoff-n,smoothstep(0.9,1,size*0.95))*size*1.2*starCol;
					}
				}
				return sl;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv-0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float2 uv = i.uv*_ScreenParams/_ScreenParams.y;
				uv*=1;
				uv  = mul (Rot(_Time.x*1),uv);
                float4 col =0;
				float star =0;

				float4 sl =0;
				for(float j =0.0 ;j<1.0;j+=1.0/10)
				{  
					float depth = frac(j+_Time.x*2);//make layer move
					float scale = lerp(20,0.5,depth);
					float fade = depth*smoothstep(1,0.9,depth);//make depth smooth if from 1 to 0
					sl += StarLayer(uv*scale+j*525)*fade;//offset layer
				}

				//col+=star*starCol;
				col =sl;
                return col;
            }
            ENDCG
        }
    }
}
