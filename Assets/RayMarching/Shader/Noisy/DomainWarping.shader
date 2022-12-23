Shader "RayMarch/DomainWarping"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_M ("Texture2", 2D) = "white" {}
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
			sampler2D _M;
			float4 _M_TexelSize;


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

			 float2x2 m = float2x2( 0.80,  0.60, -0.60,  0.80 );

			float noise(float2 p)
			{
				return sin(p.x)*sin(p.y);
			}

			float fbm4(float2 p)
			{
				float f = 0.0;
				f += 0.5000 * noise(p);
				p =mul( m,p*2.02);

				f += 0.2500*noise(p); 
				p =mul( m,p*2.03);

				f += 0.1250*noise(p); 
				p = mul(m,p*2.01);

				f += 0.0625*noise(p);

				return f/0.9375;
			}

			float fbm6( float2 p )
			{
				float f = 0.0;
				f += 0.500000*(0.5+0.5*noise( p )); p =mul(m,p*2.02); 
				f += 0.250000*(0.5+0.5*noise( p )); p =mul( m,p*2.03);
				f += 0.125000*(0.5+0.5*noise( p )); p =mul(m,p*2.01); 
				f += 0.062500*(0.5+0.5*noise( p )); p =mul(m,p*2.04); 
				f += 0.031250*(0.5+0.5*noise( p )); p =mul(m,p*2.01); 
				f += 0.015625*(0.5+0.5*noise( p ));
				return f/0.96875;
			}

			float2 fbm4_2( float2 p )
			{
				
				return float2(fbm4(p), fbm4(p+float2(8,8)));
			}

			float2 fbm6_2( float2 p )
			{
				float2 a = float2(16.8,16.8);
				float2 b = float2 (11.5,11.5);
				return float2 (fbm6(p+a),fbm6(p+b));
			} 



			float func( float2 q, float4 ron )
			{
				q += 0.03*sin( float2(0.27,0.23)*_Time.y + length(q)*float2(4.1,4.3));

				float2 o = fbm4_2( 0.9*q );

				o += 0.04*sin( float2(0.12,0.14)*_Time.y + length(o));

				float2 n = fbm6_2( 3.0*o );

				ron = float4( o, n );

				float f = 0.5 + 0.5*fbm4( 1.8*q + 6.0*n );

				return lerp( f, f*f*f*3.5, f*abs(n.x) );
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
                
				float2 p  = (i.uv-0.5)*_ScreenParams/_ScreenParams.y;
                float4 col = 0;

				//float e = 2.0/_ScreenParams.y;

				//float4 on = 0;
				//float f = func(p, on);
				p += 0.03*sin( float2(0.27,0.23)*_Time.y + length(p)*float2(4.1,4.3));
				float ddd1= FBM_PerlinNoisy(p+float2(6.8,6.8),1.1);
				float ddd2 =FBM_PerlinNoisy(ddd1+float2(8,8),0.9);
				float ddd3 =FBM_PerlinNoisy(ddd2,0.7);
				col = tex2D(_M,float2(ddd3,ddd3));
				//col = lerp( float4(0.2,0.1,0.4,1), float4(0.3,0.05,0.05,1), f );
				//col = lerp( col, float4(0.9,0.9,0.9,1), dot(on.zw,on.zw) );
				//col = lerp( col, float4(0.4,0.3,0.3,1), 0.2 + 0.5*on.y*on.y );
				//col = lerp( col, float4(0.0,0.2,0.4,1), 0.5*smoothstep(1.2,1.3,abs(on.z)+abs(on.w)) );
				//col = clamp( col*f*2.0, 0.0, 1.0 );

				//// manual derivatives - better quality, but slower
				//float4 kk;
				//float3 nor = normalize( float3( func(p+float2(e,0.0),kk)-f, 
				//		2.0*e,
				//		func(p+float2(0.0,e),kk)-f ) );
  

				//float3 lig = normalize( float3( 0.9, 0.2, -0.4 ) );
				//float dif = clamp( 0.3+0.7*dot( nor, lig ), 0.0, 1.0 );
				//float3 lin = float3(0.70,0.90,0.95)*(nor.y*0.5+0.5) + float3(0.15,0.10,0.05)*dif;
				//col *= 1.2*float4(lin ,1);
				//col = 1.0 - col;
				//col = 1.1*col*col;
    
				
                return col;
            }
            ENDCG
        }
    }
}
