Shader "Qternions/QternionsRotation"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_A("_Angle",Range(0,360)) = 0
		_Map ("Map", 2D) = "white" {}
		_Map2 ("Map2", 2D) = "white" {}
		_aixs ("Aixs",Vector) = (1,0,0,0)
		_CamerA ("_CamerA",Range(0,360)) = 0
		_ScreenWidth("_ScreenWidth",Range(-1,1)) =0
		_sharpness("_sharpness",Range(0.1,10))=1
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
			sampler2D _Map;
			sampler2D _Map2;
            float4 _MainTex_ST;
			float4 _Map_ST;
			float4 _Map2_ST;
			float _A;
			float4 _aixs;
			float _CamerA;
			float _ScreenWidth;
			float _sharpness;
//////////////////////////////////////////////////////////
			float2x2 Rot (float degrees)
			{
				float rad = degrees * UNITY_PI / 180.0;
				float s = sin(rad);
				float c = cos(rad);
				return float2x2(c,-s,s,c);
				
			}
			float4x4 RotAixs (float degrees)
			{
				float rad = degrees * UNITY_PI / 180.0;
				float s = sin(rad);
				float c = cos(rad);

				
				float4x4 rotY =float4x4(c,  0,  s, 0,
										0,  1,  0, 0,
										-s, 0,  c, 0,
										0,  0,  0, 1 );

				float4x4 rotX =float4x4(1,  0,  0, 0,
										0,  c,  -s,0,
										0,  s,  c, 0,
										0,  0,  0, 1 );

				float4x4 rotZ =float4x4(c,  -s,  0, 0,
										s,  c,   0, 0,
										0,  0,   1, 0,
										0,  0,   0, 1 );


				return rotX;
				
				
			}
			float4 QrotAix (float3 a,float degrees)
			{	
				//a= normalize(a);
				float rad = 0.5*degrees * UNITY_PI / 180.0;
				float4 q =float4( cos(rad),a*sin(rad));
				return q;
			}
			float4 QMulti (float4 q1,float4 q2)
			{
				float w1 = q1.x , x1= q1.y ,  y1 = q1.z , z1=q1.w;
				float w2 = q2.x , x2= q2.y,  y2 = q2.z,  z2=q2.w;

				float4 res = float4 (   w1*w2-x1*x2-y1*y2-z1*z2,
										w1*x2+x1*w2+y1*z2-z1*y2,
										w1*y2-x1*z2+y1*w2+z1*x2,
										w1*z2+x1*y2-y1*x2+z1*w2  );
				return res;						

			}
			float4 QRot(float3 aixs,float degrees,float3 pos)
			{
				float4 nPos =QMulti(QMulti( QrotAix(aixs,degrees),float4 (0,pos)),QrotAix(aixs,-degrees));
				return nPos;
			}

			float2 tri (float2 p)
			{
				float2 h= frac(0.5*p)-0.5;
				return 1-2*abs(h);
			}

			float checkersTextureGradBox( float2 p, float2 ddx, float2 ddy )
			{
				// filter kernel
				float2 w = max(abs(ddx), abs(ddy)) + 0.01;   //避免除0  因为方波 在边界的地方  ddx 或者ddy w =1
				// analytical integral (box filter)
				//float2 i = 2.0*(abs(frac((p-0.5*w)/2.0)-0.5)-abs(frac((p+0.5*w)/2.0)-0.5))/w;
				float2 i = (tri(p+0.5*w)-tri(p-0.5*w))/w;//triangle wave 相减 》》平滑部分是线性过度的 
				
				// xor pattern
				return 0.5 - 0.5*i.x*i.y;     //when -1 return 1    when 1 return 0             
			}

			float filteredGrid( float2 p, float2 dpdx, float2 dpdy )
			{
				const float N = 20.0;
				float2 w = max(abs(dpdx), abs(dpdy));
				float2 a = p + 0.5*w;                        
				float2 b = p - 0.5*w;           
				float2 i = (floor(a)+min(frac(a)*N,1.0)-
				floor(b)-min(frac(b)*N,1.0))/(N*w);
				return (1.0-i.x)*(1.0-i.y);
			}

			float filteredSquares( float2 p,float2 dpdx, float2 dpdy )
			{
				const float N = 2.0;
				float2 w = max(abs(dpdx), abs(dpdy));
				float2 a = p + 0.5*w;                        
				float2 b = p - 0.5*w;           
				float2 i = (floor(a)+min(frac(a)*N,1.0)-
				floor(b)-min(frac(b)*N,1.0))/(N*w);
				return 1.0-i.x*i.y;
			}

			float filteredCrosses( float2 p, float2 dpdx, float2 dpdy )
			{
				const float N = 3.0;
				float2 w = max(abs(dpdx), abs(dpdy));
				float2 a = p + 0.5*w;                        
				float2 b = p - 0.5*w;           
				float2 i = (floor(a)+min(frac(a)*N,1.0)-
				floor(b)-min(frac(b)*N,1.0))/(N*w);
				return 1-i.x-i.y+2.0*i.x*i.y;
			}

//////////////////////////////////////////////////////////
			float2 SphUV(float3 pos,float4 sph)
			{	
			
				//pos = mul(unity_WorldToObject,pos);.
				
				float3 nor = normalize( pos-sph.xyz);
				
				float u = (atan2(nor.z, nor.x)/UNITY_PI)*0.5+0.5;//[-PI,PI]
				float v = asin(nor.y)/UNITY_PI+0.5;//[-PI/2 ,PI/2 ]
				float2 uv = float2 (u,v)*_Map_ST.xy+_Map_ST.zw;
				return uv;

				

			}

			float2 SphIntersection (float3 ro ,float3 rd, float4 sph)
			{
				float3 oc =  ro-sph.xyz;
				float b= dot(oc,rd);
				float c= dot(oc,oc)-sph.w*sph.w;
				float D = b*b-c;
				if (D < 0) return -1.0;
				D = sqrt(D);
				return  float2( -b-D,-b+D) ;
			}

			float3 SphNormal (float3 pos,float4 sph)
			{
				return  normalize( float3 (pos-sph.xyz));
			}

			float SphOcc(float3 pos,float3 nor,float4 sph)
			{
				float3 r =  sph.xyz-pos;
				float l = length(r);
				r = r/l;
				float v= sph.w/l;
				float occ = dot(nor,r)*v*v;
				return max(0.0,occ);
			}

			float sphSoftShadow( float3 ro, float3 rd, float4 sph,  float k )
			{
				float3 oc = ro - sph.xyz;
				float b = dot( oc, rd );
				float c = dot( oc, oc ) - sph.w*sph.w;
				float h = b*b - c;
    
			#if 1
				// physically plausible shadow
				float d =  sqrt( max(0,sph.w*sph.w-h))-sph.w;

				float t = -b - sqrt( max(h,0.0) );
				return (t<0.0) ? 1 : smoothstep(0.0, 1.0, k*d/t );
			#else
				// cheap but not plausible alternative
				return (b>0.0) ? step(-0.0001,c) : smoothstep( 0.0, 1.0, h*k/b );
			#endif    
			} 
			float PlnIntersection (float3 ro,float3 rd,float4 plane)
			{
				return (plane.w- dot(ro,normalize(plane.xyz)))/(dot(rd,normalize(plane.xyz)));
			}


			float3 CamerRd  (float3 ro ,float3 target ,float zoom,float2 uv)
			{
				float3 f= normalize(target - ro);
				float3 r= cross(f,float3(0.0,1.0,0.0));
				float3 u = cross (r,f);
				float3 cc = ro+f*zoom;
				float3 ipos = cc+uv.x*r+uv.y*u;
				float3 rd = normalize (ipos-ro);
				return rd;
			}

			float2 texCoords( float3 pos, int mid,float4 sph,float4 nPos,float3 aixs,float degree )
			{
				float2 matuv;
    
				if( mid==0 )
				{
					matuv = 1-pos.xz/(_ScreenParams/_ScreenParams.y);
				}
				else if( mid==1 )
				{
					
					sph.xyz -=nPos.yzw;
					pos =  QRot(aixs,degree,pos).yzw;
					//float4 ppos = float4(sphPos,1);
					//sphPos = mul(RotAixs(degree),ppos).xyz;
					//sphPos.xz = mul (Rot(degree),sphPos.xz);
					float3 q = normalize( pos - sph.xyz );
					matuv = 12*float2( (atan2(q.z,q.x)/UNITY_PI)*0.5+0.5, asin(q.y)/UNITY_PI+0.5)*sph.w;
					//matuv = SphUV(sphPos,sph);
					sph.xyz +=nPos.yzw;
				}
				return 1.0*matuv;
			}
			float4 boxmap( sampler2D s, float3 p, float3 n,  float k ) 
			{ 
				// project+fetch 
				float4  x = tex2D( s, p.yz );
				float4  y = tex2D( s, p.zx );
				float4  z = tex2D( s, p.xy );
				// blend factors 
				float3 w = pow( abs(n), float3(k,k,k) ); 
				// blend and return
				return (x*w.x + y*w.y + z*w.z) / (w.x + w.y + w.z);
			}

			float3 CalNormal( float3 pos, int mid,float4 sph )
			{
				float3 nor;
    
				if( mid==0 )
				{
					nor =normalize (float3(0,1,0));
				}
				else if( mid==1 )
				{
					nor  = normalize( pos - sph.xyz );
					
				}
				return nor;
			}

/////////////////////////////////////////////////////////////
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv-0.5 ;

                return o;
            }
//////////////////////////////////////////////////////////////
            fixed4 frag (v2f i) : SV_Target
            {
				float tmin =1e5;
				float occ = 1.0;
                float4 col = 0.9 ;
				float3 nor =0;
				float3 pos =0;
				float2 uv  =  (i.uv)*_ScreenParams/_ScreenParams.y;

				float3 ro = float3(0,1,3);
				float3 target = float3 (0.0,0.0,0.0);

				float zoom = 1;

				float degree = 360*frac(_Time.y*0.1);
				float3 Caixs = float3 (0,1,0);
				ro = QRot(Caixs,_CamerA,ro).yzw;
				
				//ro.xz= mul(Rot(_A+_Time.y*100),ro.xz);

				float3 rd = CamerRd (ro,target,zoom,uv);

				float3 ddx_rd = CamerRd (ro,target,zoom,uv+float2(1,0));
				float3 ddy_rd = CamerRd (ro,target,zoom,uv+float2(0,1));



				float3 sc = float3 (0,0,0);
				int  mid = 0;
				float2 matuv = 0;
				//float4x4 tras = float4x4 (   1, 1, -1, 0,
				//							 -1,1, 1, 0,
				//							 1, 1,  1, 0,
				//							 0, 0,  0, 1 );

				float4x4 tras = float4x4 (   1,  0,  0,  -1,
											 0,  1,  0,  0,
											 0,  0,  1,  0,
											 0,  0,  0,  1 );

				//_aixs.xyzw = mul (tras,_aixs.xyzw);
				float3 aixs =normalize( _aixs.xyz);
				
				float4 nPos= QRot(aixs,degree,sc);
				
				//sc.xz= mul(Rot(degree),sc.xz);
				float4 sph = float4(nPos.yzw,1);
				//float4 sph = float4(sc,1);
				float3 sphPos =0;
				float3 sphPos2 =0;

				float2 sphuv =0;

				float4 pln = float4(0.0,1.0,0.0,-1.0);
				float2 plnuv =0;

				float3 lig = normalize (float3 (0,1,1));



				//sph.xy= mul(Rot(_A+_Time.y*100),sph.xy);

				float t1 = PlnIntersection(ro,rd,pln);
				if(t1>0)
				{	
					tmin = t1;
					float3 pos = ro + t1*rd;
					nor = normalize( pln.xyz);
					mid =0;
					occ = 1-SphOcc(pos,nor,sph);
				}

				float t2 = SphIntersection(ro,rd,sph).x;
				if (t2>0 && t2<tmin)
				{
					tmin = t2;
					sphPos = ro +t2*rd;
					mid=1;
					nor  = normalize( sphPos - sph.xyz );
					occ = 0.5+0.5*nor.y; 
			
				}


				if (tmin<1000)
				{
					float3 pos = ro+tmin*rd;
					col =1;
					

					float2 matuv = texCoords( pos,mid,sph,nPos,aixs,degree );

					#if 0
					float3 ddx_Pos = ro - ddx_rd*dot(ro-pos,nor)/(ddx_rd,nor);
					float3 ddy_Pos = ro - ddy_rd*dot(ro-pos,nor)/(ddy_rd,nor);
					float2 ddxuv = texCoords( ddx_Pos,mid,sph,nPos,aixs,degree )-matuv;
					float2 ddyuv = texCoords( ddy_Pos,mid,sph,nPos,aixs,degree )-matuv;
					float  boxfilter = checkersTextureGradBox(matuv,ddxuv,ddyuv);
					float gridfilter = filteredGrid(matuv,ddxuv,ddyuv);
					#else 
					float2 ddxuv = ddx(matuv);
					float2 ddyuv = ddy(matuv);
					float  boxfilter = checkersTextureGradBox(matuv,ddxuv,ddyuv);
					float gridfilter = filteredGrid(matuv,ddxuv,ddyuv);
					float squarfilter= filteredSquares(matuv,ddxuv,ddyuv);
					float crossfilter = filteredCrosses(matuv,ddxuv,ddyuv);
					#endif
					
					//matuv *=boxfilter;
					float2 q = floor(matuv);
					  


					//occ *= smoothstep( 0, 0.1, length( pos.xz-float2( 0,0)));
					//occ *= smoothstep( 0.0,0.1, length( pos.xz-float2(-1.0,0)));
					//occ *= smoothstep( 0.0, 0.1, length( pos.xz-float2(0,1)));
					//occ *= smoothstep( 0.0, 0.1, length( pos.xz-float2(0,-1)));
					//occ *= smoothstep( 0.0,0.1, length( pos.xz-float2(1,0)));
					//occ *= smoothstep( 0.0, 0.1, length( pos.xz-float2(2,0)));
					
					col *=clamp( dot(nor,lig), 0.01, 1 );
					col *= sphSoftShadow( pos, lig, sph, 4.0 )+0.2;
					#if 1
					 if( uv.x-_ScreenWidth<0 ) 
					 {	
						float2 s = sign(frac(matuv*0.5)-0.5);
					    col *= 0.5 - .5*s.x*s.y;
					 }else
					 {
						col *= boxfilter;
					 }
					col *=tex2D (_Map,matuv); 
					#else 
					col *= boxmap(_Map2,pos,nor,_sharpness);
				    #endif

					col +=occ*0.05;
					col = lerp( col, float4(0.8,0.9,0.9,1), 1.0-exp( -0.00002*tmin*tmin ));//fog
					//col *= 1-exp(-0.05*tmin);
					
				}
				
				col += smoothstep(0.004,0.003,abs(uv.x-_ScreenWidth))*float4(1,1,1,1);
				col= sqrt(col);
				//col = pow( col, float4(0.4545,0.4545,0.4545,1));
				
                return col;
            }
            ENDCG
        }
    }
}
