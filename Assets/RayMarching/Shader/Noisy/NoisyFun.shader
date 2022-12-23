Shader "RayMarch/NoisyFun"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Map ("Texture", 2D) = "white" {}
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
			#define MOD3 float3(.1031,.11369,.13787)
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
			 sampler2D _Map;
            float4 _Map_ST;

			float2x2 Rot (float a)
			{
				float s = sin(a);
				float c =cos(a);
				return float2x2 (c,-s,s,c);
			}

			

			float hash31(float3 p3)
			{
				p3  = frac(p3 * MOD3);
				p3 += dot(p3, p3.yzx + 19.19);
				return -1.0 + 2.0 * frac((p3.x + p3.y) * p3.z);
			}

			float3 hash33(float3 p3)
			{
				p3 = frac(p3 * MOD3);
				p3 += dot(p3, p3.yxz+19.19);
				return -1.0 + 2.0 * frac(float3((p3.x + p3.y)*p3.z, (p3.x+p3.z)*p3.y, (p3.y+p3.z)*p3.x));
			}


			float N21(float2 p)
			{
				 return -1+2*frac(sin(dot(p,float2(100,6875)))*5637.56);
				             
				//p = frac(p*float2(234.56,567.45));
				//p += dot(p,p+34.56);
				//return  frac(p.x*p.y);
			}

			float2 N22(float2 p)
			{
				float3 a = frac(p.xyy * MOD3);
				a += dot (a,a.yxz+34.45);
				return -1+2*frac(float2 (a.x *a.y ,a.y *a.z));

				//p = float2(dot(p,float2(127.1,311.7)),dot(p,float2(269.5,183.3)));
				//return -1.0 + 2.0*frac(sin(p)*43758.5453123);
			}

			float ValueNoisy (float2 p)
			{
				float2 gv = frac(p);
				float2 id = floor(p);
				
				//gv = gv*gv*(3-2*gv);
				gv=gv*gv*gv*(gv*(gv*6-15)+10);
				
				float bottomleft = N21( id);
				float bottomRight = N21( id+float2(1,0));
				float upLeft = N21(id+float2(0,1));
				float upRight = N21(id+float2(1,1));
			   
				float a  = lerp(bottomleft,bottomRight,gv.x);
				float b = lerp (upLeft,upRight,gv.x);
				float c= lerp(a,b,gv.y);
				return c*0.5;
			}

			float PerlinNoisy (float2 p)
			{
				float2 gv = frac(p);
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
				
				return c+0.1;
			}

			float  VoronoiNoisy (float2 uv,out float2 cellID)
			{
				float2 gv = frac(uv);
				float2 id = floor (uv);
				float mindistance = 100;
				 cellID =0;
				for (int y =-1 ;y<=1;y++)
				{
					for(int x =-1 ;x<=1;x++)
					{
						float2 uvoffset = float2(x,y);
						float2 n = N22(id+uvoffset);
						float2 p =0.5*sin(n*_Time.y)+uvoffset+0.5;
						float d = length(gv -p);
						if (d<mindistance)
						{
							mindistance =d;
							cellID = id+uvoffset;
						}
					}
				}				
				return mindistance;
			}

			float FBM_ValueNoisy(float2 p,float h)
			{
			    p = p*4;
				float G = exp2(-h);
				float f = 1.0;
				float a = 1.0;
				float t = 0.0;
				int numOctaves = 6;
				for( int i=0; i<numOctaves; i++ )
				{
					t += a*abs(ValueNoisy(p*f));
					f *= 2.0;
					a *= G;
				}
				return t;
				
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
				//t =sin((1-p.y)*1+t);
				return t;
			}


            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =  v.uv; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float4 col = 0;
				float2 uv = 2*i.uv-1;
				uv =uv*_ScreenParams/_ScreenParams.y;
				
				uv*=2;
				//uv.x-=_Time.x;
				 float2 id = floor(uv);
				 float2 gv = frac(uv);
				 float checker = fmod((id.x+id.y),2)*2-1;
				 
				//uv = float2((uv.x+checker)+sin(_Time.y),(uv.y-checker)+cos(_Time.y));
				//float n_value = FBM_ValueNoisy(uv,0.5);
				float n_perlin = FBM_PerlinNoisy(uv,1);
				
				//col = tex2D(_Map,i.uv+offuv);
				//col.rg+=id*0.1;
				col+=n_perlin;


				//float2 cellID =0;
				//float dvoronoi = VoronoiNoisy(uv,cellID);
				//col =dvoronoi;
			   // col =1-pow(col,1);
                return col;
            }
            ENDCG
        }
    }
}
