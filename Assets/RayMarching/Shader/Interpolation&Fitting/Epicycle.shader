Shader "RayMarch/Epicycle"
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

            float4 frag (v2f i) : SV_Target
            {
               
                float4 col = 0;            
				float e = 1.0/_ScreenParams.x;
				float2 uv  = (2*i.uv-1)*_ScreenParams/_ScreenParams.y;
				uv *= 2;


				float t = fmod((_Time.y*1)/5.0,1.2);
				float w = 1;
				float pi = UNITY_PI;
				
				
				//float2 data[20];
				//data[0]= float2(0,0.5);
				//data[1]= float2(pi/3,1);
				//data[2]= float2(pi/2,1.3);
				//data[3]= float2(pi/5,0.75);
				//data[4]= float2(pi/7,0.35);
				//data[5]= float2(pi/4,1);

            
			 //  #define NUM  38
			 //  float2 path[NUM];
				//path[ 0] = float2( 0.098, 0.062);
				//path[ 1] = float2( 0.352, 0.073);
				//path[ 2] = float2( 0.422, 0.136);
				//path[ 3] = float2( 0.371, 0.085);
				//path[ 4] = float2( 0.449, 0.140);
				//path[ 5] = float2( 0.352, 0.187);
				//path[ 6] = float2( 0.379, 0.202);
				//path[ 7] = float2( 0.398, 0.202);
				//path[ 8] = float2( 0.266, 0.198);
				//path[ 9] = float2( 0.318, 0.345);
				//path[10] = float2( 0.402, 0.359 );
				//path[11] = float2( 0.361, 0.425 );
				//path[12] = float2( 0.371, 0.521 );
				//path[13] = float2( 0.410, 0.491 );
				//path[14] = float2( 0.410, 0.357 );
				//path[15] = float2( 0.502, 0.482 );
				//path[16] = float2( 0.529, 0.435 );
				//path[17] = float2( 0.426, 0.343 );
				//path[18] = float2( 0.449, 0.343 );
				//path[19] = float2( 0.504, 0.335 );
				//path[20] = float2( 0.664, 0.355 );
				//path[21] = float2( 0.748, 0.208 );
				//path[22] = float2( 0.738, 0.277 );
				//path[23] = float2( 0.787, 0.308 );
				//path[24] = float2( 0.748, 0.183 );
				//path[25] = float2( 0.623, 0.081 );
				//path[26] = float2( 0.557, 0.099 );
				//path[27] = float2( 0.648, 0.116 );
				//path[28] = float2( 0.598, 0.116 );
				//path[29] = float2( 0.566, 0.195 );
				//path[30] = float2( 0.584, 0.228 );
				//path[31] = float2( 0.508, 0.083 );
				//path[32] = float2( 0.457, 0.140 );
				//path[33] = float2( 0.508, 0.130 );
				//path[34] = float2( 0.625, 0.071 );
				//path[35] = float2( 0.818, 0.093 );
				//path[36] = float2( 0.951, 0.066 );
				//path[37] = float2( 0.547, 0.081 );

				#define NUM  10
				float2 path[NUM];
				path[0] = float2(1, 0.2);
				path[1] = float2(0.55, 0.4);
				path[2] = float2(0.25, 0.8);
				path[3] = float2(0.3,1.4);
				path[4] = float2( 0.8, 1.5);
				path[5] = float2(1.0, 1.2);
				path[6] = float2(1.2, 1.5);
				path[7] = float2(1.7, 1.4);
				path[8] = float2(1.75, 0.8);
				path[9] = float2(1.45, 0.4);

				//#define NUM  10
				//float2 path[NUM];
				//path[0] = float2(1.0, 1.0);
				//path[1] = float2(1.1, 1.1);;
				//path[2] = float2(1.2, 1.2);;
				//path[3] = float2(1.3, 1.3);;
				//path[4] = float2(1.4, 1.4);;
				//path[5] = float2(1.5, 1.5);;
				//path[6] = float2(1.6, 1.6);;
				//path[7] = float2(1.7, 1.7);;
				//path[8] = float2(1.8, 1.8);;
				//path[9] = float2(1.9, 1.9);;



				//#define NUM  15
				//float2 path[NUM];
				//path[0] = float2(0.5, 0.5);
				//path[1] = float2(0.5, 0.8);
				//path[2] = float2(0.7, 1.0);
				//path[3] = float2(1.2,1);
				//path[4] = float2( 1.4, 0.8);
				//path[5] = float2(1.9, 0.8);
				//path[6] = float2(1.9, 0.5);
				//path[7] = float2(1.8, 0.6);
				//path[8] = float2(1.5, 0.6);
				//path[9] = float2(1.4, 0.5);
			 //   path[10] = float2(1, 0.5);
				//path[11] = float2(0.9, 0.6);
				//path[12] = float2(0.7, 0.6);
				//path[13] = float2(0.6, 0.5);
				//path[14] = float2(0.5, 0.5);


				float2 d2 = 1000;
				for(float j =0 ;j <NUM-1; j++)
				{
					float2 a = path[j+0];
					float2 b=  path[j+1];
					d2 = min(d2,float2(sdLine(uv,a,b),sdPoint(uv,a)));
				}
				d2.y = min(d2.y,sdPoint(uv,path[NUM-1]));
				col = lerp(col ,float4(0.2,0.2,0.1,1),smoothstep(0.02,0.01,d2.y));
				//col = lerp(col ,float4(0.2,0.2,0,1),smoothstep(0.005,0.004,d2.x));

				
				#define ENum 6
				float2 fcsX[ENum];
				float2 fcsY[ENum];

				float2 mcsX[ENum];
				float  rr[ENum];

				float3 epicycleData[ENum];
				float2 epicyclePos[ENum];

				for(int k  = 0  ; k < ENum; k ++)//DFT
				{
					float2 fcx =0;
					float2 fcy =0;
					float2 sum =0;
					float real = 0;
					float ima = 0;

					float a =0;
					float b =0 ;
					for (int j = 0; j < NUM ;j++)
					{
						float an = -(2*pi/float(NUM))*float(k)* float(j);
						float2 ex = float2 (cos(an),sin(an));

						float an2 = 2*pi* float(j)*float(k)/float(NUM);
						//sum += cmul(path[j],float2(cos(an2), -sin(an2)));
						a += path[j].x*cos(an2)+path[j].y*sin(an2);
						b += path[j].y*cos(an2)-path[j].x*sin(an2);
						sum = float2(a,b);

						fcx +=  path[j].x * ex;
						fcy +=  path[j].y * ex;
					}
					fcsX[k]= fcx;
					fcsY[k]= fcy;

					mcsX[k]= fcx/NUM;
					rr[k] = length(fcx/NUM);

					sum /=NUM;
					epicycleData[k].x = length(sum);
					epicycleData[k].y = k;
					epicycleData[k].z = atan2(sum.y,sum.x);
					epicyclePos[k] = sum;

				}
			

				float ani = min(t,1.0);
				
				float2 op=0 ;
				float2 fp =0;
				float d = 1000 ;
				float h  = 0;
				
				#define SAMPLE_NUM 128
				for(int j = 0  ;j <SAMPLE_NUM;j++)
				{
					h  = ani*float(j)/float(SAMPLE_NUM);
					float2 p = 0 ;

/*
实数 N 个离散点DFT将产生 N/2+1 个频率点，频率的序号是从0--N/2，频谱带宽是N/2 ，每个频率点占的带宽是 2/N ，
所以每个频率的实际幅值需要用DFT后的幅值乘以2/N 。
频率序号为0和N/2 的两个点带宽只占中间频率点的一半，也就是占 1/N的带宽
*/
					for(int k= 0 ;k <ENum ;k++)
					{	
						#if 1
						float w  = (k==0||k==ENum-1)?1.0:2.0;
						float an = 2*pi*float(k)*h;
						float2 ex = float2(cos(an),sin(an));
						p.x +=w* cmul(fcsX[k],ex)/float(NUM);
						p.y +=w* cmul(fcsY[k],ex)/float(NUM);

						#else
						
						float an = 2*pi*float(k)*h+epicycleData[k].z;
						float2 ex = float2(cos(an),sin(an));
						p  +=   epicycleData[k].x*ex;
						#endif

					}
					if( j==0 ) fp=p; else d = min( d,sdLine( uv,p, op) );
					op = p ;

				}
				col = lerp(col ,float4(0.968,0.501,0.49,1),1-smoothstep(0,0.01,d));



				float2 p = 0 ;
				
				float2 mp =0;
				
				float dl = 0;
				
				float pix = 0;
				
				for(int  k =  0  ;k < ENum; k++)
				{

						#if 1
					    float w  = (k==0||k==ENum-1)?1.0:2.0;					
						float an = 2*pi*float(k)*h;
						float2 ex = float2(cos(an),sin(an));
						p.x +=w*cmul(fcsX[k],ex)/float(NUM);
						p.y +=w*cmul(fcsY[k],ex)/float(NUM);
						//p.x  += w*dot(fcsX[k],ex)/float(NUM);
						//p.y  += w*dot(fcsY[k],ex)/float(NUM);

						float dc = sdCircle(uv,mp,length(p-mp));
						float pix = fwidth(dc);
						float dp = sdPoint(uv,p);
						float dl = sdLine(uv,mp,p);
						mp=p;

						#else

						float w  = (k==0||k==ENum-1)?1.0:2.0;	
						float an = 2*pi*float(k)*h;
						float2 ex = float2(cos(an),sin(an));
						p += cmul(epicyclePos[k],ex);
						float dp = sdPoint(uv,p);
						float dc = sdCircle(uv,mp,epicycleData[k].x);
						float pix = fwidth(dc);
						float dl = sdLine(uv,mp,p);
						mp =p;
						
				
					
						#endif

						col+=smoothstep(0.03,0.01,dp)*float4(0.8,0.2,0.6,1);
						
						col+=smoothstep(2*pix,0,dl)*float4(0.4,0.2,0.6,1)*0.5;
						
						col += smoothstep(4*pix,0,abs(dc))*0.2;
				}
				
				
				 
				
              return col;
            }
            ENDCG
        }
    }
}
