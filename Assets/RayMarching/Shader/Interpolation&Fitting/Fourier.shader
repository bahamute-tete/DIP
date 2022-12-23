Shader "RayMarch/Fourier"
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

			float sdCircle(float2 p,float2 cs,float r)
			{
				float d =length(p-cs)-r;
				return d;
			}

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
                o.uv = v.uv;
               
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
               
				float4 col = 0;
				float e = 1.0/_ScreenParams.x;
				float2 uv  = (2*i.uv-1)*_ScreenParams/_ScreenParams.y;
				uv *= 6;


				float t = _Time.y;
				float w = 1;

				float2 p0= 0;
				float2 p1 =0;
				float pi = UNITY_PI;
				
				float pix = 0;
				float dc= 0;
				
				#define NUM  6
				
				float2 data[NUM];
				data[0]= float2(0,0.5);
				data[1]= float2(pi/3,1);
				data[2]= float2(pi/2,1.3);
				data[3]= float2(pi/5,0.75);
				data[4]= float2(pi/7,0.35);
				data[5]= float2(pi/4,1);

				float2 path[38];
				path[ 0] = float2( 0.098, 0.062);
				path[ 1] = float2( 0.352, 0.073);
				path[ 2] = float2( 0.422, 0.136);
				path[ 3] = float2( 0.371, 0.085);
				path[ 4] = float2( 0.449, 0.140);
				path[ 5] = float2( 0.352, 0.187);
				path[ 6] = float2( 0.379, 0.202);
				path[ 7] = float2( 0.398, 0.202);
				path[ 8] = float2( 0.266, 0.198);
				path[ 9] = float2( 0.318, 0.345);
				path[10] = float2( 0.402, 0.359 );
				path[11] = float2( 0.361, 0.425 );
				path[12] = float2( 0.371, 0.521 );
				path[13] = float2( 0.410, 0.491 );
				path[14] = float2( 0.410, 0.357 );
				path[15] = float2( 0.502, 0.482 );
				path[16] = float2( 0.529, 0.435 );
				path[17] = float2( 0.426, 0.343 );
				path[18] = float2( 0.449, 0.343 );
				path[19] = float2( 0.504, 0.335 );
				path[20] = float2( 0.664, 0.355 );
				path[21] = float2( 0.748, 0.208 );
				path[22] = float2( 0.738, 0.277 );
				path[23] = float2( 0.787, 0.308 );
				path[24] = float2( 0.748, 0.183 );
				path[25] = float2( 0.623, 0.081 );
				path[26] = float2( 0.557, 0.099 );
				path[27] = float2( 0.648, 0.116 );
				path[28] = float2( 0.598, 0.116 );
				path[29] = float2( 0.566, 0.195 );
				path[30] = float2( 0.584, 0.228 );
				path[31] = float2( 0.508, 0.083 );
				path[32] = float2( 0.457, 0.140 );
				path[33] = float2( 0.508, 0.130 );
				path[34] = float2( 0.625, 0.071 );
				path[35] = float2( 0.818, 0.093 );
				path[36] = float2( 0.951, 0.066 );
				path[37] = float2( 0.547, 0.081 );
				

				float2 d2 = 1000;
				for(float j =0 ;j <37; j++)
				{
					float2 a = path[j+0]*10-float2(1,1);
					float2 b= path[j+1]*10-float2(1,1);
					d2 = min(d2,float2(sdLine(uv,a,b),sdPoint(uv,a)));
				}
				d2.y = min(d2.y,sdPoint(uv,path[37]));
				col = lerp(col ,float4(0.2,0.2,0,1),1-smoothstep(0,0.1,d2.x));


				float2 fcsX[20];
				float2 fcsY[20];
				for( int k=0; k<20; k++ )
				{
					float2 fcx = 0;
					float2 fcy = 0;
					for( int m=0; m<38; m++ )
					{
						float an = -2*pi*float(k)*float(m)/38.0;
						float2  ex = float2(cos(an), sin(an) );      // an*cos(nx)+bn*sin(nx)
						fcx += (path[m].x)*ex;
						fcy += (path[m].y)*ex;
					}
				fcsX[k] = fcx;
				fcsY[k] = fcy;
				}


				float ani = min( fmod((12.0+_Time.y)/10,1.3),1.0 );
				float d = 1000.0;
				float2 oq, fq;
				for( int j=0; j<256; j++ )
				{
					float h = ani*float(j)/256.0;
					float2 q = 0;
					for( int k=0; k<20; k++ )
					{
						float w = (k==0||k==19)?1.0:2.0;
						float an = -2*pi*float(k)*h;
						float2  ex = float2( cos(an), sin(an) );
						q.x += w*dot(fcsX[k],ex)/38.0;
						q.y += w*dot(fcsY[k],ex)/38.0;
					}
					if( j==0 ) fq=q; else d = min( d,sdLine( uv,q, oq) );
					oq = q;
				}
				col = lerp( col, float4(0.8,0.9,0.2,1),1- smoothstep(0,0.1,d) );
				col *= 0.75 + 0.25*smoothstep( 0.0, 0.13, d );

				
                return col;
            }
            ENDCG
        }
    }
}
