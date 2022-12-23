Shader "RayMarch/SpaceGIF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_GridNumber("_GridNumber",int) =15
		_WaveSpeed ("_WaveSpeed",float)=1.2
		_WaveShape ("_WaveShape",Range(0.1,2))=0.3
		_RotationSpeed ("_RotationSpeed",Range(0,1))=0
		_MinR ("_MinR",Range(0.1,0.5)) =0.3
		_MaxR ("_MinR",Range(0.6,2)) =1.5
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
			int _GridNumber;
			float _WaveSpeed;
			float _WaveShape;
			float _RotationSpeed;
			float _MinR;
			float _MaxR;


////////////////////////////////////////////////////////////////////////
			float remap (float a ,float b ,float c)
			{
				return (c-a)/(b-a);
			}

			float mix (float a , float b ,float c)
			{
				return a*(1-c)+b*c;
			}

			float2x2 M_rotation (float a)
			{
				float s = sin(a);
				float c = cos(a);
				return float2x2 (c ,-s ,s,c);
			}

			float Xor(float a ,float b)
			{ 
				//float m = saturate(a);
				//float n = saturate(b);
				//return m *(1-n)+n*(1-m);
				return a*(1-b)+b*(1-a);
			}
///////////////////////////////////////////////////////////////////////
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv= v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float4 col = 0;
				float  d = 0;
				float  circle= 0;
				float r = 0.6;
				float dist = 0; 
				float t =_Time.y*_WaveSpeed;

				float2 uv = (i.uv-0.5)*_ScreenParams/_ScreenParams.y;
				uv = mul (M_rotation(_Time.y*_RotationSpeed),uv);
				float2 gv = frac(uv*_GridNumber)-1;
				float2 gid = floor(uv*_GridNumber);

				

				for (float y =-1 ;y<1 ;y++)
				{
					for(float x = -1 ; x<1 ;x++)
					{
						float2 offs = float2 (x,y); 
						 d = length(gv-offs);
						 dist = length(gid+offs)*_WaveShape;
						 r = mix(_MinR,_MaxR,sin(dist-t)*0.5+0.5);
						 circle = Xor(circle, smoothstep(r,r*0.9,d)); 
					}
				}

				//col *= tex2D (_MainTex,float2 (circle,circle));

				//col.rg =gv; 
				//col +=fmod(circle,2);
				//float3 base = sin(t*2*float3(0.234,0.564,0.483))*0.4+0.6;
				col.rgb +=circle;//*base;
                return col;
            }
            ENDCG
        }
    }
}
