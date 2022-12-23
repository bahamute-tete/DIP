Shader "RayMarch/MandelbrotSet"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MapTex("_MapTex",2D) ="white"{}
		_Angle("_Angle",Range(-3.1415,3.1415))=0
		_Scale ("_Scale",float)=1
		_Pos ("_Pos",Vector)=(1,1,1,1)
		_ColorRange ("_ColorRange",Range(0,1))=0.5
		_Repeat("_Repeat",float) =1
		_Speed("_Speed",float) =0
		sy ("sy",Range(0,1)) =1
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
			#define _MaxStep 255
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
			 float _ColorRange;
			 float _Repeat;

			float _Angle;
			float _Scale;
			float4 _Pos;
			float _Speed;
			float sy;

			float2x2 Rot (float a)
			{
				float s = sin(a);
				float c = cos(a);
				return float2x2(c,-s,s,c);
			}



			float InMandelbrotSet( float2 p)
			{
				//(a+bi)*(c+di) = (ac-bd)+(bc+ad)i
				float d =0;

				float  cx =p.x;
				float  cy =p.y;

				float iter =0;
				for( iter =0 ;iter <255 ; iter++)
				{
					
						float resultX = (p.x*p.x - p.y*p.y)+cx;
						float resultY = (p.y*p.x + p.x*p.y)+cy;

						d = length(float2(resultX,resultY));
						if (d >=2.0)
						{
							break;	
						}
						p.x = resultX; 
						p.y = resultY;	
				}
				return  d;
			}


			float MandelbrotSet2(float2 p, out float fracIter)
			{
				float2 z =0,pz=0;
				float iter =0;
				float d =0;
				float r =20.0;
				float r2 =r*r;
				fracIter =0;
				for (iter =0 ;iter<_MaxStep;iter++)
				{
					pz =z;//priew Z 
					z =float2(z.x*z.x- z.y*z.y,2*z.x*z.y)+p;
					d = length(z);
					if(dot (z,pz)>=r2) break;
					//if(d>r) break;
				}
				/*
				linear interpolation ===>>(y-y0)/(x-x0) = (y1-y0)/(x1-x0)===>>(x-x0)/(x1-x0)=(y-y0)/(y1-y0) =K ===>>remap();
				log a (x) = log b (x)/log b (a) 对数换底


				对于 set外的点 他们的逃脱距离是是在2 以外 的  所以他们的 distance  在  R<distance< R*R (因为Z=Z*Z+C? )
				
				logR(R)=1 loaR(R*R)=2  so  1<log R (distance)<2 ====>>0<log R (distance)-1<1
				log R (distance) 返回线性  ===》 log2(log R (distance))  （因为Z*Z? )
				*/
					d = length(z);
					fracIter = (d-r)/(r2-r);//linear interpolation
					fracIter = log2(log(d)/log(r))-1;//double exponential interpolation
					//iter -=fracIter;
					return iter/_MaxStep;

			}


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
				float d=0;
				float m =0;
				float fracIter =0;

				float2 rotPivot = float2 (_Pos.x,_Pos.y);

				float scale =max(0.0001,_Pos.z);

				float2 uv = (i.uv-0.5)*_ScreenParams/_ScreenParams.y;
				uv =abs(uv);
				uv =mul (Rot(0.25*3.1415),uv);
				uv = abs(uv);
				uv = lerp((i.uv-0.5)*_ScreenParams/_ScreenParams.y,uv ,sy);
				uv=float2(_Pos.x,_Pos.y)+uv*scale;
				
		
				uv -=rotPivot;
				uv =mul (Rot(_Angle),uv);
				uv+=rotPivot;
					
				if (MandelbrotSet2(uv,d)*_MaxStep >=_MaxStep)
				return col =0;

				
				m =sqrt(MandelbrotSet2(uv,fracIter));
				
				
				//col =sin(float4(0.3,0.4,0.72,1.0)*30*m)*0.5+0.5;
				col = tex2D (_MapTex,float2(m*_Repeat+_Time.y*_Speed, _ColorRange));
				col *=smoothstep (1,0,fracIter);
                return col;
            }
            ENDCG
        }
    }
}
