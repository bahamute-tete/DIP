Shader "RayMarch/FourierSeries"
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
				float2 uv = (2*i.uv-1)*_ScreenParams/_ScreenParams.y; 
				uv*=8;
				float pi = UNITY_PI;


				float t = fmod(_Time.y*3,4*pi);
				float2 bias = float2(5,-4);
				float2 bias2 = float2(5,4);

				float2 cc = float2(0,0)-bias;
				float2 cc2 = float2(0,0)-bias2;

				float coefficient = 1.5;
				float  r = 1.0;
				float  r2 = 1.0;

				float dc = 0;
				float dc2 = 0;

				float pix = 0;

				float2 pc = 0;
				float2 pcc = 0;
				float2 pc2 = 0;

				float  dp = 0;
				float  dp2 = 0;

				float x = 0;
				float y = 0;

				float xtri = 0;
				float ytri = 0;

				float dl=0;
				float dl2 =0;

				float h=0;


				#define NUM 50
				float2 freP1[NUM];
				float2 freP2[NUM];
				for(int  k = 0  ; k<NUM ;k++)
				{
					
					float n= 2*k+1;
					r =coefficient* 4/(n*pi);
					x +=  r * cos(n*t);
					y +=  r * sin(n*t);

				
					float n1= (fmod(k+1,2)== 0)? k+1 :-(k+1);
					r2 =coefficient* 2/(n1*pi);
					xtri +=  r2 * cos(abs(n1)*t);
					ytri +=  r2 * sin(abs(n1)*t);


					pc = float2(x,y)-bias;
					pcc = float2(xtri,ytri)-bias2;

					freP1[k] = pc;
					freP2[k] = pcc;

					dp = sdPoint(uv,pc);
					dp2 = sdPoint(uv,pcc);

					dc = sdCircle(uv,cc,r);
					dc2 = sdCircle(uv,cc2,r);

					dl = sdLine(uv,cc,pc);
					dl2 = sdLine(uv,cc2,pcc);

					cc =pc;
					cc2 = pcc;

					pix = 2*fwidth(dc);
					float al = 0.8;
					float4 cyc =float4(0.6886792,0.4320488,0.6220629,1);
					col += smoothstep(pix,0,abs(dc))*al*cyc;
					col += smoothstep(0.06,0.05,dp)*float4(0.754717,0.6699519,0.2242791,1);
					col += smoothstep(pix,0,dl)*al*cyc;

					col += smoothstep(pix,0,abs(dc2))*al*cyc;
					col += smoothstep(0.06,0.05,dp2)*float4(0.2235294,0.5938153,0.7529412,1);
					col += smoothstep(pix,0,dl2)*al*cyc;
				
				}


				float2 npc =0;
				float2 npc2 =0;
				float2 sowave = 0;
				float2 cowave = 0 ;

				#define SAMPLE_NUM 128
				float2 swaveXY[SAMPLE_NUM];
				float2 cwaveXY[SAMPLE_NUM];

				
				float2 olp1 = 0-bias;
				float2 olp2 = 0-bias2;

				for (int n = 0;n<SAMPLE_NUM;n++)
				{
					h = t*n/SAMPLE_NUM;
					float nx =0;
				    float ny =0;
					float nxtri =0;
				    float nytri =0;
					for(int  k = 0  ; k<NUM ;k++)
					{
						float n= 2*k+1;
						r =coefficient* 4/(n*pi);
						nx +=  r * cos(n*h);
						ny +=  r * sin(n*h);

				
						float n1= (fmod(k+1,2)== 0)? k+1 :-(k+1);
						r2 =coefficient* 2/(n1*pi);
						nxtri +=  r2 * cos(abs(n1)*h);
						nytri +=  r2 * sin(abs(n1)*h);


						npc = float2(nx,ny)-bias;
						npc2 = float2(nxtri,nytri)-bias2;
					
						
					}
					
					float2 lp1 = npc;
					float dlp1= sdLine(uv,olp1,lp1);
					olp1 = lp1;
					col += smoothstep(pix,0,dlp1)*float4(0.754717,0.6699519,0.2242791,1);


					float2 lp2 = npc2;
					float dlp2 = sdLine(uv,olp2,lp2);
					olp2 = lp2;
					col += smoothstep(pix,0,dlp2)*float4(0.2235294,0.5938153,0.7529412,1);

					float2 nuv= float2( -uv.x+t,uv.y);
					
					float2 sinwave = float2(h,npc.y);			
					float2 coswave = float2(h,npc2.y);

					float dlswave = sdLine(nuv,sowave,sinwave);
					sowave = sinwave;

					float dlcwave = sdLine(nuv,cowave,coswave);
					cowave = coswave;

					swaveXY[n] = sinwave;
					cwaveXY[n] = coswave;

					col += smoothstep(pix,0,dlswave)*float4(0.754717,0.6699519,0.2242791,1);
					col += smoothstep(pix,0,dlcwave)*float4(0.2235294,0.5938153,0.7529412,1);


					
				
				}

				float dcp = sdPoint(uv,float2(swaveXY[0].x,swaveXY[SAMPLE_NUM-1].y));
				float dcl = sdLine(uv,pc,float2(swaveXY[0].x,swaveXY[SAMPLE_NUM-1].y));

				float cdcp = sdPoint(uv,float2(cwaveXY[0].x,cwaveXY[SAMPLE_NUM-1].y));
				float cdcl = sdLine(uv,pcc,float2(cwaveXY[0].x,cwaveXY[SAMPLE_NUM-1].y));


				col += smoothstep(pix,0,dcl)*float4(0.754717,0.6699519,0.2242791,1);
				col += smoothstep(pix,0,cdcl)*float4(0.2235294,0.5938153,0.7529412,1);

				col += smoothstep(0.1,0.09,dcp)*float4(0.754717,0.6699519,0.2242791,1);
				col += smoothstep(0.1,0.09,cdcp)*float4(0.2235294,0.5938153,0.7529412,1);

                return col;
            }
            ENDCG
        }
    }
}
