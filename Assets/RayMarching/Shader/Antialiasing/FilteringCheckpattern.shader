Shader "RayMarch/FilteringCheckpattern"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_t ("t",Range(-1,1))=0
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
			float _t;



			float checkersTextureGradBox(  float2 p,  float2 ddx,  float2 ddy )
			{
				// filter kernel
				float2 w = max(abs(ddx), abs(ddy)) + 0.01; 
				// analytical integral (box filter)
				float2 i = 2.0*(abs(frac((p-0.5*w)/2.0)-0.5)-abs(frac((p+0.5*w)/2.0)-0.5))/w;
				// xor pattern
				return 0.5 - 0.5*i.x*i.y;                  
			}

			// --- unfiltered checkerboard ---

			float checkersTexture(  float2 p )
			{
				//float2 q = floor(p);
				//return fmod( q.x+q.y, 2.0 );            // xor pattern
				float2 s = sign(frac(p*0.5)-0.5);//squar wave
				return 0.5 - .5*s.x*s.y;//
			}
///////////////////////////////////////////////////////////////////////////////
			float softShadowSphere(  float3 ro,  float3 rd,  float4 sph )
			{
				float3 oc = sph.xyz - ro;
				float b = dot( oc, rd );
	
				float res = 1.0;
				if( b>0.0 )
				{
					float h = dot(oc,oc) - b*b - sph.w*sph.w;
					res = smoothstep( 0.0, 1.0, 2.0*h/b );
				}
				return res;
			}

			float occSphere(  float4 sph,  float3 pos,  float3 nor )
			{
				float3 di = sph.xyz - pos;
				float l = length(di);
				return 1.0 - dot(nor,di/l)*sph.w*sph.w/(l*l); 
				}

				float iSphere( in float3 ro, in float3 rd, in float4 sph )
				{
				float t = -1.0;
				float3  ce = ro - sph.xyz;
				float b = dot( rd, ce );
				float c = dot( ce, ce ) - sph.w*sph.w;
				float h = b*b - c;
				if( h>0.0 )
				{
					t = -b - sqrt(h);
				}
	
				return t;
			}
///////////////////////////////////////////////////////////////////////////////////


					float intersect( float3 ro, float3 rd, out float3 pos, out float3 nor, out float occ, out int matid )
					{
						// raytrace
						float tmin = 10000.0;
						nor = 0;
						occ = 1.0;
						pos = 0;
						matid = -1;
					 float4 sc0 = float4(  3.0, 0.5, 0.0, 0.5 );
					 float4 sc1 = float4( -4.0, 2.0,-5.0, 2.0 );
					 float4 sc2 = float4( -4.0, 2.0, 5.0, 2.0 );
					 float4 sc3 = float4(-30.0, 8.0, 0.0, 8.0 );
						// raytrace-plane
						float h = (-1-ro.y)/rd.y;
						if( h>0.0 ) 
						{ 
							tmin = h; 
							nor = float3(0.0,1.0,0.0); 
							pos = ro + h*rd;
							matid = 0;
							occ = occSphere( sc0, pos, nor ) * 
								  occSphere( sc1, pos, nor ) *
								  occSphere( sc2, pos, nor ) *
								  occSphere( sc3, pos, nor );
						}


						// raytrace-sphere
						h = iSphere( ro, rd, sc0 );
						if( h>0.0 && h<tmin ) 
						{ 
							tmin = h; 
							pos = ro + h*rd;
							nor = normalize(pos-sc0.xyz); 
							matid = 1;
							occ = 0.5 + 0.5*nor.y;
						}

						h = iSphere( ro, rd, sc1 );
						if( h>0.0 && h<tmin ) 
						{ 
							tmin = h; 
							pos = ro + tmin*rd;
							nor = normalize(pos-sc1.xyz); 
							matid = 2;
							occ = 0.5 + 0.5*nor.y;
						}

						h = iSphere( ro, rd, sc2 );
						if( h>0.0 && h<tmin ) 
						{ 
							tmin = h; 
							pos = ro + tmin*rd;
							nor = normalize(pos-sc2.xyz); 
							matid = 3;
							occ = 0.5 + 0.5*nor.y;
						}

						h = iSphere( ro, rd, sc3 );
						if( h>0.0 && h<tmin ) 
						{ 
							tmin = h; 
							pos = ro + tmin*rd;
							nor = normalize(pos-sc3.xyz); 
							matid = 4;
							occ = 0.5 + 0.5*nor.y;
						}

						return tmin;	
					}

					float2 texCoords( float3 pos, int mid )
					{
						float2 matuv;
						float4 sc0 = float4(  3.0, 0.5, 0.0, 0.5 );
						float4 sc1 = float4( -4.0, 2.0,-5.0, 2.0 );
						float4 sc2 = float4( -4.0, 2.0, 5.0, 2.0 );
						float4 sc3 = float4(-30.0, 8.0, 0.0, 8.0 );
						if( mid==0 )
						{
							matuv = pos.xz;
						}
						else if( mid==1 )
						{
							float3 q = normalize( pos - sc0.xyz );
							matuv = float2( atan2(q.x,q.z), acos(q.y ) )*sc0.w;
						}
						else if( mid==2 )
						{
							float3 q = normalize( pos - sc1.xyz );
							matuv = float2( atan2(q.x,q.z), acos(q.y ) )*sc1.w;
						}
						else if( mid==3 )
						{
							float3 q = normalize( pos - sc2.xyz );
							matuv = float2( atan2(q.x,q.z), acos(q.y ) )*sc2.w;
						}
						else if( mid==4 )
						{
							float3 q = normalize( pos - sc3.xyz );
							matuv = float2( atan2(q.x,q.z), acos(q.y ) )*sc3.w;
						}

						return 6.0*matuv;
					}


					void calcCamera( out float3 ro, out float3 ta )
					{
						float an = 0.1*sin(0.1*_Time.y);
						ro = float3( 5.0*cos(an), 0.5, 5.0*sin(an) );
						ta = float3( 0.0, 1.0, 0.0 );
					}

					float3 doLighting( float3 pos, float3 nor,  float occ,  float3 rd )
					{

						float4 sc0 = float4(  3.0, 0.5, 0.0, 0.5 );
						float4 sc1 = float4( -4.0, 2.0,-5.0, 2.0 );
						float4 sc2 = float4( -4.0, 2.0, 5.0, 2.0 );
						float4 sc3 = float4(-30.0, 8.0, 0.0, 8.0 );
						float sh = min( min( min( softShadowSphere( pos, float3(0.57703,0.57703,0.57703), sc0 ),
												  softShadowSphere( pos, float3(0.57703,0.57703,0.57703), sc1 )),
												  softShadowSphere( pos, float3(0.57703,0.57703,0.57703), sc2 )),
												  softShadowSphere( pos, float3(0.57703,0.57703,0.57703), sc3 ));
						float dif = clamp(dot(nor,float3(0.57703,0.57703,0.57703)),0.0,1.0);
						float bac = clamp(0.5+0.5*dot(nor,float3(-0.707,0.0,-0.707)),0.0,1.0);
						float3 lin  = dif*float3(1.50,1.40,1.30)*sh;
							 lin += occ*float3(0.15,0.20,0.30);
							 lin += bac*float3(0.10,0.10,0.10)*(0.2+0.8*occ);

						return lin;
					}
///////////////////////////////////////////////////////////////////////////////////
					void calcRayForPixel(  float2 pix, out float3 resRo, out float3 resRd )
					{
						
	
						 // camera movement	
						float3 ro, ta;
						//ro = float3(0,0,4);
						//ta = float3(0,0,0);
						calcCamera( ro, ta );
						// camera matrix
						float3 ww = normalize( ta - ro );
						float3 uu = normalize( cross(ww,float3(0.0,1.0,0.0) ) );
						float3 vv = normalize( cross(uu,ww));
						// create view ray
						float3 rd = normalize( pix.x*uu + pix.y*vv + 1.0*ww );
	
						resRo = ro;
						resRd = rd;
					}

/////////////////////////////////////////////////////////////////////////////////
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv-0.5;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {

                float3 col = 0;
				float2 p  =  (i.uv)*_ScreenParams/_ScreenParams.y;
				



				float3 ro, rd, ddx_ro, ddx_rd, ddy_ro, ddy_rd;
				calcRayForPixel( p + float2(0.0,0.0), ro, rd );
				calcRayForPixel( p + float2(1.0,0.0), ddx_ro, ddx_rd );
				calcRayForPixel( p + float2(0.0,1.0), ddy_ro, ddy_rd );
		
				// trace
				float3 pos, nor;
				float occ;
				int mid;
				float t = intersect( ro, rd, pos, nor, occ, mid );

				col = 0.8;
				if( t<1000)
				{

				
				#if 0
					// -----------------------------------------------------------------------
					// compute ray differentials by intersecting the tangent plane to the  
					// surface.		
					// -----------------------------------------------------------------------

					// computer ray differentials
					float3 ddx_pos = ddx_ro - ddx_rd*dot(ddx_ro-pos,nor)/dot(ddx_rd,nor);
					float3 ddy_pos = ddy_ro - ddy_rd*dot(ddy_ro-pos,nor)/dot(ddy_rd,nor);

					// calc texture sampling footprint		
					float2     uvw = texCoords(pos, mid );
					float2 ddx_uv = texCoords( ddx_pos, mid )-uvw ;
					float2 ddy_uv = texCoords( ddy_pos, mid ) -uvw;
				#else
					// -----------------------------------------------------------------------
					// Because we are in the GPU, we do have access to differentials directly
					// This wouldn't be the case in a regular raytrace.
					// It wouldn't work as well in shaders doing interleaved calculations in
					// pixels (such as some of the 3D/stereo shaders here in Shadertoy)
					// -----------------------------------------------------------------------
					float2 uvw = texCoords( pos, mid );

					// calc texture sampling footprint		
					float2 ddx_uv = ddx( uvw ); 
					float2 ddy_uv = ddy( uvw ); 
				#endif

        
					// shading		
					float3 mate = 0;

					if( p.x-_t<0) mate = float3(1,1,1)*checkersTexture( uvw );
					else          mate =float3(1,1,1)*checkersTextureGradBox( uvw, ddx_uv, ddy_uv );
            
					// lighting	
					float3 lin = doLighting( pos, nor, occ, rd );

					// combine lighting with material		
					col = mate * lin;
		
					// fog		
					col = lerp( col, float3(0.9,0.9,0.9), 1.0-exp( -0.00001*t*t ) );
				}
	
				// gamma correction	
				col = pow( col, float3(0.4545,0.4545,0.4545) );

				col *= smoothstep( 1.0, 2.0, abs(p.x-_t)/(2.0/_ScreenParams.y) );

				return float4(col,1);
				}
            ENDCG
        }
    }
}
