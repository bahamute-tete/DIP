Shader "RayMarch/Grid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_A("_Angle",Range(0,360)) = 0
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
            #define MOD3 float3(.1031,.11369,.13787)
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
			float _A;


			float smin( float a, float b, float k )
			{
				float h = clamp( 0.5 + 0.5*(b-a)/k, 0.0, 1.0 );
				return lerp( b, a, h ) - k*h*(1.0-h);
			}

            float hash31(float3 p3)
			{
				p3  = frac(p3 * MOD3);
				p3 += dot(p3, p3.yzx + 19.19);
				return -1.0 + 2.0 * frac((p3.x + p3.y) * p3.z);
			}

			float hash21(float2 p)
			{
				// return -1+2*frac(sin(dot(p,float2(100,6875)))*5637.56);
				             
				p = frac(p*float2(234.56,567.45));
				p += dot(p,p+34.56);
				return  frac(p.x*p.y);
			}


			float2x2 Rot (float degrees)
			{
				float rad = degrees * UNITY_PI / 180.0;
				float s = sin(rad);
				float c = cos(rad);
				return float2x2(c,-s,s,c);
			
			}

				float sdPlane (float3 pos,float h)
			{
				float d = pos.y -h ;
				return d;
			}


			float3 sdRec(float2 p,float2 size)
			{
				float2 w = abs(p)-size;
				float2 s = float2(p.x>0?1:-1 ,p.y>0? 1:-1);//sign
				float g = max(w.x,w.y);//  if  g>0 ===> out rec   g<0 ===> inner rec 
				float2 q= max(w,0);
				float l = length(q)-0.012;
				float2  m  = s*(g>0?q/l : (w.x>w.y)? float2(1,0):float2(0,1));//if  inner w.x>w.y==>the point at the  leftside of diagonal line else  at rightside 
				return float3 ( (g)>0?l:g, m);
			}


			float sdPoint (float3 ro,float3 rd,float3 p,float scale)
			{
				float3 po = p-ro;
				float d =length(cross(po,rd))/length(rd);  
				return scale*d;
			}

			float sdCapusle (float3 p ,float3 a,float3 b,float r)
			{
				float3 ap = p-a;
				float3 ab= b-a;

				float t = dot(ab,ap)/dot(ab,ab);
				t = clamp(t,0,1);
				float3 c = a +t*ab;

				float capsule = length(p-c)-r;
				return capsule;
			}

			float3 CameraRd (float3 ro ,float3 ta, float2 uv)
			{
				float3 f = normalize( ta - ro);
				float3 r = normalize( cross(f,float3(0,1,0)));
				float3 u = normalize( cross(r,f));
				float3 cc= ro+1*f;
				float3 ip = cc+ uv.x*r +uv.y*u;
				return normalize(ip - ro);
			}





			float map(float3 pos)//,float3 ro  ,float3 rd)
			{	
				float d=1000;
				

				float dCapusle1= 0 ;
				float dCapusle2= 0 ;
				float dpoint = 0;
				float lw = 0.02;
				float pw = 3;
				float d1= 0;
				float d2= 0;

				#define POINTNUM  5
				
				float3 sp = 0;
				float3 terrain[POINTNUM][POINTNUM];
				for(int k = 0 ; k <POINTNUM; k++)
				{
					for(int j = 0  ;j <POINTNUM ; j ++)
					{	
					   
						
			            float3 points =float3(j-(POINTNUM-1)*0.5,0,k-(POINTNUM-1)*0.5);

						float ny =1.0*sin( hash21(points.xz)*_Time.y*3)*0.5+0.5;
						terrain[j][k] = float3( points.x, ny , points.z);
						//dpoint = sdPoint(ro,rd,terrain[j][k] ,pw);



						int nj = (j== POINTNUM-1)? j-1:j+1;
						int nk = (k ==POINTNUM-1)? k-1:k+1;

					    dCapusle1 = sdCapusle(pos,terrain[j][k],terrain[nj][k],lw);
						dCapusle2 =  sdCapusle(pos,terrain[j][k],terrain[j][nk],lw);


						d = smin(d,smin(dCapusle1,dCapusle2,0.1),0.1);
						
					}
				}

			
				return d;
			}


			float CenterConnect (float3 pos,float3 cp )
			{
				float3 tp  = 0;
				float d  = 1000;
				float ds1 = 0;
				

				for(int z = -1 ; z <=1; z++)
				{
					for(int x  = -1; x <=1; x ++)
					{	
					 //   if (x==-1 && z==1 ) continue;
						//if (x==1 && z==-1) continue;

						if (x==z || x==-z) continue;
						
						float3 ofp = float3(x,0,z);
						tp.xz = cp.xz+ofp.xz;
						ds1 = sdCapusle(pos, cp, tp,0.01);
						d = min(d,ds1);      
					}
				}	
				return d;
			
			}

			float map2(float3 pos)
			{	
				float d  = 1000;
				float3 sp =0;
				float dl= 0;
				int cloumn = 1,row =1;
				float3 terrain[25];

				for(int k = 0 ; k <=cloumn; k++)
				{
					for(int j = 0  ;j <=row  ; j ++)
					{	
						sp = (fmod(k,2)==0)? float3(j,1,k) : float3(j,0,k);

						dl = CenterConnect(pos,sp);
						d = min(d,dl);
						
					}
				}
				return d;
			}

			float3 calcNormal (float3 pos,  float eps )
			{
				
				float2 e = float2(1.0,-1.0)*0.5773*eps;
				
				return normalize( e.xyy*map( pos + e.xyy ) + 
								  e.yyx*map( pos + e.yyx ) + 
								  e.yxy*map( pos + e.yxy ) + 
								  e.xxx*map( pos + e.xxx ) );
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
               
                float4 col =0;
				float2 uv  = (2*i.uv-1)*_ScreenParams/_ScreenParams.y;
				uv*= 1;

				float3 ro = float3(0,2,3);
				ro.xz= mul(Rot(_Time.y*_A),ro.xz);
				float3 ta = float3(0,0,0);
				float3 rd = 0;
				rd = CameraRd(ro,ta,uv);

				float3 lig = normalize (float3 (0,-1,0));

				float3 pos = 0;
				const float tmax = 1000.0;
				float t = 0.0;
				for( int i=0; i<120; i++ )
				{
					float3 pos = ro + rd*t;
					float h = map(pos);
					if(abs(h)<0.01 ) break;
					t += h;
					if( t> tmax ) break;
				}

				if (t<tmax)
				{    
					float3 pos = ro + t * rd;
					float3 nor = calcNormal(pos,0.001);
					float occ = nor.y*0.4+0.6;
					col =occ*float4(0.10,0.65,0.96,1);
				    //col = float4(0.2,0.3,0.9,1)*0.4;	
				}
			
				col = sqrt(col);
                return col;
            }
            ENDCG
        }
    }
}
