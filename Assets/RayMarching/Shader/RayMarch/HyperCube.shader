Shader "RayMarch/HyperCube"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_iChannel0("Tex0",2D) = "white" {}
		_iChannel1("Tex1",2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
           #pragma exclude_renderers gles
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
			sampler2D _iChannel1;
			sampler2D _iChannel0;
///////////////////////////////////////////////////////////////////////
			float smin( float a, float b, float k )
			{
				float h = clamp( 0.5 + 0.5*(b-a)/k, 0.0, 1.0 );
				return lerp( b, a, h ) - k*h*(1.0-h);
			}

			float smax( float d1, float d2, float k ) 
			{
				float h = clamp( 0.5 - 0.5*(d2+d1)/k, 0.0, 1.0 );
				return lerp( d2, -d1, h ) + k*h*(1.0-h); 
			}

			float4x4 MRot (float a,int index)
			{
				float s = sin(a);
				float c = cos(a);
				float4x4 M =0;
				switch(index)
				{
					case (0):
					 M = float4x4 ( c ,-s , 0, 0,
									s , c,  0, 0,
									0,  0,  1, 0,
									0,  0,  0, 1 );
									break;

					case 1:
					 M = float4x4 ( c ,0 ,-s, 0,
									0 ,1,  0, 0,
									s, 0,  c, 0,
									0, 0,  0, 1 );
									break;
					case 2:
					 M = float4x4 ( 1 , 0 ,0,  0,
									0 , c, 0, -s,
									0,  0,  1, 0,
									0,  s,  0, c );
									break;
					case 3:
					 M = float4x4 ( c ,0 ,0, -s,
									0 , 1, 0, 0,
									0,  0, 1, 0,
									s,  0, 0, c );
									break;
				}
				
			 return M;
			
			}

			float sdSphere (float3 p,float3 sph,float r)
			{	
				
				float d = length(p-sph)-r;
				return d;
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




			float map(float3 pos)
			{	
				float dsp[16];
				float3 nhcp[16];
				float dsc[32]; 

				
				float d  = 1000;
				float dl =0;
				float dp =0;

				float rp = 0.05;
				float r = 0.01;
				

				float4 hcp[16] = { float4(1,1,1,1), float4(1,1,-1,1), float4(-1,1,-1,1), float4(-1,1,1,1),
								  float4(1,-1,1,1),float4(1,-1,-1,1),float4(-1,-1,-1,1),float4(-1,-1,1,1),
								  float4(1,1,1,-1), float4(1,1,-1,-1), float4(-1,1,-1,-1), float4(-1,1,1,-1),
								  float4(1,-1,1,-1),float4(1,-1,-1,-1),float4(-1,-1,-1,-1),float4(-1,-1,1,-1),};

				float distance = 2;

				

				for(int j  = 0 ; j< 16;j++ )
				{
					//hcp[j] = mul(MRot(_Time.y,3),hcp[j]);
					hcp[j] = mul(MRot(_Time.y,2),hcp[j]);
					//hcp[j] = mul(MRot(_Time.y,1),hcp[j]);
					hcp[j] = mul(MRot(_Time.y,0),hcp[j]);

					float w  = 1 /(distance - hcp[j].w);
					//float w = 2.5*hcp[j].xyz/(3.0+hcp[j].w);
					//2.5*p.xyz/(3.0+p.w); 
					float3x4 Mpro = float3x4 ( w, 0, 0, 0,
											   0, w, 0, 0,
											   0, 0, w, 0 );
											  

					nhcp[j] =mul(Mpro,hcp[j]);

					dsp[j] = sdSphere(pos, nhcp[j], rp);

				}


				for(int k  = 0 ; k <4 ;k++)
				{
					dsc[k] = sdCapusle(pos,nhcp[k],nhcp[fmod(k+1,4)],r);
					dsc[k+4] = sdCapusle(pos,nhcp[k+4],nhcp[fmod(k+1,4)+4],r);
					dsc[k+8] = sdCapusle(pos,nhcp[k],nhcp[k+4],r);

					dsc[k+12] = sdCapusle(pos,nhcp[k+8],nhcp[fmod(k+1,4)+8],r);
					dsc[k+16] = sdCapusle(pos,nhcp[k+12],nhcp[fmod(k+1,4)+12],r);
					dsc[k+20] = sdCapusle(pos,nhcp[k+8],nhcp[k+12],r);

					dsc[k+24] = sdCapusle(pos,nhcp[k],nhcp[k+8],r);
					dsc[k+28] = sdCapusle(pos,nhcp[k+4],nhcp[k+12],r);
				}

				for(int m  = 0 ; m < 31;m++ )
				{
					
					 dl= min(dsc[m],dsc[m+1]);
					 d= smin(d,dl,0.05);
				}

				for(int n  = 0 ; n < 15;n++ )
				{
					
					 dp= min(dsp[n],dsp[n+1]);
					 d = smin(d,dp,0.05);
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

			float Shadow( float3 ro, float3 rd,float k )
			{

				float res = 1;
				float ph = 1e20;
				float tmin= 0.01;
				float tmax = 30.0;
				for(float t=tmin; t<tmax;)
				{
					float h = map(ro + rd*t);
					if( h<0.001 )
					return 0;
					//res = min( res,saturate( k*h/t) );

					float y = h*h/(2.0*ph);
					float d = sqrt(h*h-y*y);
					res = min( res, k*d/max(0.0,t-y) );
					ph = h;
					t += h;
				}
				return res;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {

                float4 col = 0;
				float2 uv  =  (2*i.uv-1)*_ScreenParams/_ScreenParams.y;

				float3 ro = float3(2,0,3);
				float3 ta = float3(0,0,0);

				float3 rd = 0;
				rd = CameraRd(ro,ta,uv);
				float3 lig = normalize (float3 (1,1,1)); 


				float3 pos =0;
				float3 nor = 0;
				float sha = 0.0;


				
				const float tmax = 30.0;
				float t = 0.0;
				for( int i=0; i<128; i++ )
				{
					float3 pos = ro + rd*t;
					float h = map(pos);
					if(abs(h)<0.01 ) break;
					t += h;
					if( t>tmax ) break;
				}
        
				
				if( t<tmax )
				{	
				    col =1;
					float3 pos = ro + rd*t;
					float3 nor = calcNormal( pos, 0.001 );

					float amb = clamp(0.5 + 0.5*nor.y, 0.0, 1.0);
					float diff  = clamp (dot (nor,lig),0,1);
					float h = normalize(lig+rd);
					float rim = pow(clamp(1.0+dot(nor,rd),0.0,1.0),2.0);
					float HV =saturate( dot(h,nor));
					float spe = pow(HV,2);
					sha = Shadow(pos+nor*0.01,lig,10);

					col = tex2D( _iChannel1, nor );

					float3 reff = reflect(rd,nor);
					float fre = 0.3 + 0.7*pow( clamp( 1.0 + dot( rd, nor ), 0.0, 1.0 ), 10.0 );
					float4 sss = tex2D( _iChannel0, reff );
					col += 2.0*pow(sss,4.0)*fre;


					col = col*(diff+spe)*rim;
					col *= amb;
					col *= 2.0;
					
				}

				col = sqrt(col);


                return col;
            }
            ENDCG
        }
    }
}
