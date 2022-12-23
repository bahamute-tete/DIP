Shader "RayMarch/TwisedToroid"
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

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =v.uv;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {

                float4 col = 0;
				float2 uv = (i.uv-.5)*_ScreenParams/_ScreenParams.y;
				

				float t = _Time.y*0.2;
				uv = mul(Rot(t),uv);

				float3 ro = float3(0,0,-1);
				ro.x = (ro.x+1)*cos(t*4);
				ro.z = ro.z*sin(t*2);
				//float3 lookat =lerp ( float3 (0,0,0),float3(-0.5,0,0),sin(t*5)*0.5+0.5);
				float3 lookat= 0;
				float  zoom = lerp(0.4,0.5,sin(t*5)*0.5+0.5);
				float3 f = normalize(lookat - ro);
				float3 r = cross(float3(0,1,0),f);
				float3 u =cross(f,r);
				float3 c = ro + zoom*f;
				float3 ic = c+ uv.x*r+uv.y*u;
				float3 rd = normalize( ic-ro);

				float ds=0,d0=0;
				float3 p =0;
				for(int j = 0 ;j<100;j++)
				{
					 p = ro+rd*d0;
					 ds =-(length(float2 (length(p.xz)-1,p.y))-0.75);
					if(ds<1e-3||ds>100) break;
					d0+=ds;
				}

				if (ds<0.01)
				{
					float x =atan2(p.z,p.x); //-pi to pi
					x+=t;
					//float x =atan(p.x/p.z)/(UNITY_PI/2)*0.5+0.5;//0-1
					float y = atan2(length(p.xz)-1,p.y);

					float band1 =sin(y*10+x*20);
					float band2 = sin((x*10-y*30)*3)*0.5+0.5;
					float wave = sin(x*2-y*6+t*20);

					float c1 = smoothstep(-0.1,0.1,band1);
					float c2 = smoothstep(-0.1,0.1,band1-0.8);
					float m = c1*(1-c2);
					m = max(m,band2*c2*max(0,wave));
					m += max(0,wave*1*c2);
					
					float4 col1 =max(col,float4(0.259434,0.880329,1,1)*wave*c2*1.2);
					
					float4 col2 =c2*float4(1,0.380329,0.6,1)*(1-wave);
					float4 col3 = float4(0.65,0.21,0.98,1)*c1;
					col+=lerp(col1,col2,smoothstep(0.0,0.8 ,sin(x*1+t)*0.5+0.5));
					col =lerp(col,col3,0.35);
					//col +=min(col,float4(1,0.308,1,1)*c2);
				}
                return col;
            }
            ENDCG
        }
    }
}
