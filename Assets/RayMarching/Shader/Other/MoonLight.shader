Shader "RayMarch/MoonLight"
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


			float N21(float2 p)
			{
				float2 a = frac(p * float2(324.56,756.78));
				a += dot (a, a.yx+314.45);
				return -1+2*frac( a.x *a.y );

			}

			float2 N22(float2 p)
			{
				float3 a = frac(p.xyy * float3(324.56,864.12,956.78));
				a += dot (a,a.yxz+34.45);
				return -1+2*frac(float2 (a.x *a.y ,a.y *a.z));

			}


			float PerlinNoisy (float2 p)
			{
				float2 gv = frac(p+float2(_Time.y*0.2,_Time.x*5));
				float2 id = floor(p);
				
				//gv = gv*gv*(3-2*gv);
				gv=gv*gv*gv*(gv*(gv*6-15)+10);
				
				float2 bottomleft =N22( id+float2(0,0));
				float2 bottomRight =N22( id+float2(1,0));
				float2 upLeft =N22 (id+float2(0,1));
				float2 upRight = N22(id+float2(1,1));
				
				

				float j = dot(bottomleft,gv-float2(0,0));
				float k = dot(bottomRight,gv-float2(1,0));
				float l = dot(upLeft,gv-float2(0,1));
				float h = dot(upRight,gv-float2(1,1));


				float a  = lerp(j,k ,gv.x);
				float b = lerp (l,h,gv.x);
				float c= lerp(a,b,gv.y);
				
				return c;
			}

			float FBM_PerlinNoisy(float2 p, float h)
			{
			    
				p = p*4;

				float G = exp2(-h);
				float f = 1.0;
				float a = 1.0;
				float t = 0.0;
				int numOctaves = 4;
				for( int i=0; i<numOctaves; i++ )
				{
					t +=  a*abs(PerlinNoisy(p*f));
					f *= 2.0;
					a *= G;
				}
				//t =0.2*sin((1-p.y)*3+t);
				return t;
			}

			float GetHeight(float2 p)
			{
				return sin(0.95*p.x)*0.4+sin(p.x)*0.1;
			}

			float TapperBox(float2 p ,float2 wbt,float2 dbt,float blur)
			{	
				float c = 0;
				float cb = smoothstep(-blur,blur,p.y-dbt.x);
				float ct = smoothstep(blur,-blur,p.y-dbt.y);
				p.x = abs(p.x);
				float k=lerp(wbt.x, wbt.y,(p.y-dbt.x)/(dbt.y - dbt.x));
				float cw = smoothstep(blur,-blur,p.x-k);
				c+=cb*ct*cw;
				return c;
			}

			float4 Tree (float2 p,float3 tc,float blur)
			{
				
				float m = 0;
				float shadowCol = 0;

				m+=TapperBox(p,float2(0.02,0.02),float2(-0.07,0.2),blur);//trunk
				m+=TapperBox(p,float2(0.25,0.124),float2(0.2,0.4),blur);
				m+=TapperBox(p,float2(0.2,0.1),float2(0.4,0.6),blur);
				m+=TapperBox(p,float2(0.15,0),float2(0.6,0.9),blur);

				shadowCol += TapperBox(p-float2(0.1,0),float2(0.1,0.15),float2(0.16,0.2),blur);
				shadowCol += TapperBox(p-float2(0.15,0),float2(0.1,0.3),float2(0.35,0.4),blur);
				shadowCol += TapperBox(p-float2(-0.14,0.2),float2(0.15,0.3),float2(0.365,0.4),blur);
				
				tc -=shadowCol*1;

				return float4(tc,m);
			}

			float4 Ground(float2 p,float3 gc,float blur)
			{
				float c =0;
				
				c=1- smoothstep(-blur,blur,p.y+GetHeight(p)+FBM_PerlinNoisy(p,1)*0.1);
				return float4(gc,c);
			}



			float4 Layer(float2 p,float blur)
			{
				float4 tree =0;
				float4 ground = 0;
				float4 layer =0;
				ground = Ground(p,float3(1,1,1),blur);
				

				float id = floor(p.x)+0.5;
				float n = N21(id)*0.2;
				float2 n_off = N22(id);
				float  y = -GetHeight(id+n);
				float2 offsetPos = float2(n,y);
				float2 scale = float2(1,1+n*0.6);
				p.x = frac(p.x)-0.5;	
				tree = Tree((p-offsetPos)*scale,float3(1,1,1),blur);

				layer = lerp(ground,tree,tree.a);
				layer.a = max(ground.a,tree.a);
				return  layer;
			}
///////////////////////////////////////////////////////////////
            v2f vert (appdata v)
            {
                v2f o= (v2f)0;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =v.uv-0.5;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float4 col = 0;
				
				float t = _Time.y*0.2;
				float4 layer = 0;

				float2  uv = i.uv*_ScreenParams/_ScreenParams.y;
				float2 vigintUV =(i.uv+0.5)*float2(0.5,1)*_ScreenParams/_ScreenParams.y;
				uv*= 1;
				
				//uv.x +=_Time.y*0.5;
				/////twinkle
				float2 z0 = length(sin(uv+t));
				float2 z1 = length(cos(uv*float2(22,6.7)-t*2));
				float dtw = z0*z1;
				dtw = sin(dtw*10)*0.5+0.5;
				/////
				float stars =pow(N21(uv),100)*dtw*smoothstep(0.09,0.1,uv.y+0.1);
				


				float moon =smoothstep(0.01,-0.01,length(uv-float2(0.4,0.25))-0.15);
				float moonlight =smoothstep(1,0,length(uv-float2(0.4,0.25))-0.15);
				moon *=smoothstep(-0.01,0.1,length(uv-float2(0.5,0.3))-0.15);

				float2 gruv = uv*15+float2(t,-0.5);
				float gh = sin(0.95*gruv.x)*0.5+sin(gruv.x)*0.1;
				float groundd=1- smoothstep(-1,0.3,gruv.y+gh);
				col +=groundd*0.8*float4(0.6,0.65,1,1);

				col +=moon*float4(0.85,0.9,1,1);
				
				for(float j = 0 ; j <1.0 ;j+=1.0/5.0)
				{
					float scale = lerp(15,1,j);
					float blur = lerp (0.03,0.01,j);
					layer = Layer(uv*scale+float2(t+j*240,j*2),blur);
					layer.rgb *=(1-j)*float3(0.85,0.9,1);

					col =lerp(col,layer,layer.a);
				}				
				
				layer = Layer(uv+float2(t,0.9),0.03);
				
				col =max(stars,col);
				col =lerp(col,layer*0.1,layer.a)+moonlight*float4(0.5,0.5,1,1)*0.25;

				col *= 0.5 + 0.5*pow( 16.0*vigintUV.x*vigintUV.y*(1-vigintUV.x)*(1-vigintUV.y),0.2);

                return col;
            }
            ENDCG
        }
    }
}
