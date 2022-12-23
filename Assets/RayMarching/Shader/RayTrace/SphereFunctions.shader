Shader "RayMarch/SphereFunction"
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

			float3 CameraDir(float2 uv,float3 ro,float3 lookat ,float zoom)
			{

				float3 f = normalize(lookat-ro);
				float3 r = cross(f,float3(0,1,0));
				float3 up = cross(r,f);
				float3 cpos= ro+zoom*f;
				float3 ipos = cpos +r*uv.x+up*uv.y;
				float3 rd = ipos-ro;

				return rd;
			}


			float4 QMul (float4 q1,float4 q2)
			{
				float w1 = q1.x , x1= q1.y, y1 = q1.z, z1=q1.w;
				float w2 = q2.x , x2= q2.y, y2 = q2.z, z2=q2.w;

				float4 res = float4 (   w1*w2-x1*x2-y1*y2-z1*z2,
										w1*x2+x1*w2+y1*z2-z1*y2,
										w1*y2-x1*z2+y1*w2+z1*x2,
										w1*z2+x1*y2-y1*x2+z1*w2  );

			}

			float3 QRot (float3 axis,float degrees,float3 pos)
			{	
				axis =  normalize(axis);
				float4 qpos = float4 (0,pos);// a pos with  quternions type 
				float rad = 0.5*axis * UNITY_PI / 180.0;
				float4 q =float4(cos(rad),axis*sin(rad));
				float4 q_rec = float4(cos(-rad),axis*sin(-rad));// the reciprocal of quternions q 
				float4 rec = QMul(QMul(q,qpos),q_rec);// q * p * q^-1 ==>> quternions rotation

				return float3 (rec.yzx);
			}

			float3x3 QuternionsMat (float4 q)
			{	
				float w = q.x , x= q.y, y = q.z, z=q.w;

				return float3x3 ( 1-2*y*y-2*z*z , 2*x*y-2*w*z ,   2*x*z+2*w*y,
								  2*x*y+2*w*z ,   1-2*x*x-2*z*z , 2*y*z-2*w*x,
								  2*x*z-2*w*y ,   2*y*z+2*w*x ,   1-2*x*x-2*y*y);
				
			}

			float sphIntersect (float3 ro ,float3 rd, float4 sph)
			{
			/*
				将ro+rd*t带入x*x+y*y+z*z =r*r 求解 根的判别式 D = b*b -4ac 来判断球的交点 求解 根 t= (-b-sqrt(D))/2a
				float iSphere( in vec3 sc, in float sr, in vec3 ro, in vec3 rd )
				{
					float3 oc = ro - sc;
					float a = dot(rd,rd);
					float b = 2.0 * dot(rd, oc);
					float c = dot(oc, oc) - sr*sr;
					float t = b*b - 4.0*a*c;
					if( t > 0.0)
					t = (-b - sqrt(t)) / 2.0; // |rd|=1 >>rd^2 =1 >>a=rd^2 =1
					return t;
				}
				  2.0 and 4.0 can be destroy ==>如下的简化
				拓展：
				对于 椭球 x^2+ m^2 *y^2 +n^2 * z^2 -r^2 =0 同样处理 有  (m= x轴长度/y轴长度)（n = x轴长度/z轴长度）
						a = rd.x^2+m^2*rd.y^2+n^2*rd.z^2
						b = 2(ro.x*rd.x + m^2 *ro.y*rd.y + n^2 *ro.z*rd.z)
						c = ro.x^2+ m^2*ro.y^2+n^2*ro.z^2 -r^2

			*/
				
				float3 oc = ro - sph.xyz;
				float a = 1;
				float b=dot(oc,rd);
				float c = dot(oc,oc)-sph.w*sph.w; 
				float d =b*b -c;

				if (d<0) return -1.0;
				d = sqrt(d);
				return -b-d;
								
			}

			float3 sphNormal (float3 p ,float4 sph)
			{
				return normalize(p-sph.xyz);
			}

			float iPlane (float3 ro,float3 rd)
			{
				return (-1-ro.y)/rd.y; //ro+t*rd = y =>t=(y-ro)/rd;
			}

			float plnIntersect( float3 ro, float3 rd, float4 pln )//pln.xyz = normal pln.w = offset ofsset 的+ - 参考法线方向
			{
				return (pln.w - dot(ro,pln.xyz))/dot(rd,pln.xyz);
			}

			float sphOcclusion1( float3 pos, float3 nor, float4 sph )
			{
			   /*
			   由p点发出N条光线，若由H条方向被遮蔽了，
			   则我们认为该点的遮蔽程度为H/N,我们也将接受到的光线比例称为ambient access.
			   因此ambient access = 1.0 - H/N
			   H/N = blockfactor (r,d);r= 球半径 d = 球心到交点的距离
			   //bl(r,d) = cos(as)(r/d)*(r/d) as= d向量于竖直方向的夹角 ==>cos(as) = dot(n,r)
			   */
				float3  r = sph.xyz - pos;
				float d = length(r);

				//r =r/d;//normalize
				//float v = (sph.w/d);
				//float fao= dot(nor,r)*v*v;
				return max(0.0,dot(nor,r/d))*(sph.w*sph.w)/(d*d);

				
				//return fao;
			}

			float sphOcclusion2( float3 pos, float3 nor, float4 sph )
			{
				float3  di = sph.xyz - pos;
				float l  = length(di);
				float nl = dot(nor,di/l);
				float h  = l/sph.w;
				float h2 = h*h;
				float k2 = 1.0 - h2*nl*nl;

				// above/below horizon: Quilez - http://iquilezles.org/www/articles/sphereao/sphereao.htm
				float res = max(0.0,nl)/h2;
				// intersecting horizon: Lagarde/de Rousiers - http://www.frostbite.com/wp-content/uploads/2014/11/course_notes_moving_frostbite_to_pbr.pdf
				if( k2 > 0.0 ) 
				{
					#if 0
					res = nl*acos(-nl*sqrt( (h2-1.0)/(1.0-nl*nl) )) - sqrt(k2*(h2-1.0));
					res = res/h2 + atan( sqrt(k2/(h2-1.0)));
					res /= 3.141593;
					#else
					// cheap approximation: Quilez
					res = pow( clamp(0.5*(nl*h+1.0)/h2,0.0,1.0), 1.5 );
					#endif
				}

				return res;
			}

			int sphereVisibility( float3 ca, float ra, float3 cb, float rb, float3 camera )
			{
				float aa = dot(ca-camera,ca-camera);
				float bb = dot(cb-camera,cb-camera);
				float ab = dot(ca-camera,cb-camera);
    
				float s = ab*ab + ra*ra*bb + rb*rb*aa - aa*bb; 
				float t = 2.0*ab*ra*rb;

				if( s + t < 0.0 ) return 1;
				else if( s - t < 0.0 ) return 2;
							return 3;
			}

			float2 sphDistances( float3 ro, float3 rd,float4 sph )
			{
				float3 oc = ro - sph.xyz;
				float b = dot( oc, rd );
				float c = dot( oc, oc ) - sph.w*sph.w;
				float h = b*b - c;
				float d = sqrt( max(0.0,sph.w*sph.w-h)) - sph.w;
				return float2( d, -b-sqrt(max(h,0.0)) );
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
				return (t<0.0) ? 1 : smoothstep(0.0, 1.0, 2.5*k*d/t );
			#else
				// cheap but not plausible alternative
				return (b>0.0) ? step(-0.0001,c) : smoothstep( 0.0, 1.0, h*k/b );
			#endif    
			} 

			float sphDensity( float3  ro, float3  rd, float3  sc, float sr,  float dbuffer )  // depth buffer
			{
				// normalize the problem to the canonical sphere
				float ndbuffer = dbuffer / sr;
				float3  rc = (ro - sc)/sr;
	
				// find intersection with sphere
				float b = dot(rd,rc);
				float c = dot(rc,rc) - 1.0;
				float h = b*b - c;

				// not intersecting
				if( h<0.0 ) return 0.0;
	
				h = sqrt( h );
    			
				float t1 = -b - h;
				float t2 = -b + h;

				// not visible (behind camera or behind ndbuffer)
				if( t2<0.0 || t1>ndbuffer ) return 0.0;

				// clip integration segment from camera to ndbuffer
				t1 = max( t1, 0.0 );
				t2 = min( t2, ndbuffer );

				// analytical integration of an inverse squared density
				float i1 = -(c*t1 + b*t1*t1 + t1*t1*t1/3.0);
				float i2 = -(c*t2 + b*t2*t2 + t2*t2*t2/3.0);
				return (i2-i1)*(3.0/4.0);
			}
////////////////////////////////////////////
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
				float2 uv = (2*i.uv-1)*_ScreenParams/_ScreenParams.y;

				

				float3 ro = float3(0,0,4);

				float zoom = 1;
				float3 lookat = float3(0,0.0,0);
				float3 rd = normalize( CameraDir( uv, ro, lookat , zoom));
				//float3 rd = normalize( float3(uv.x,uv.y,-1.0) );

				float3 lig = normalize( float3(0-sin(_Time.y),0.3,1-cos(_Time.y))) ;

				float occ =1.0;
				float tmin = 1e5;
				float4  omin = 0;
				float3 nor =0;
				float3 nor2 = 0;
				float shadow =0;


				float4 pl1 = float4( 0.0, -1.0, 0.0, -1.0 );
				float4 pl2 = float4(  1.0, 0.0, 0.0, -1.0 );
				float4 pl3 = float4( -1.0, 0.0, 0.0, -1.0 );
				float4 pl4 = float4(  0.0, 0.0, 1, -1.0 );
				float4 pl5 = float4 (0.0,1.0,0.0,-1.0);

				float3 sphPos = float3(0+sin(_Time.y*0.5),2.0*max((0.5+0.5*sin(_Time.y*3)),0),0+cos(_Time.y*0.5));
				float4 sph = float4(sphPos,0.2);
				//float4 sph = float4(0,3,0,0.2);
				float4 sph2 = float4(0.2,0.1,0,1);
				
				int vis =sphereVisibility( sph.xyz, sph.w, sph2.xyz, sph2.w, ro );
////////////////////////////////////////////////////////////////////
				#if 1
				float t1= iPlane(ro,rd);
				if (t1>0.0)
				{
					tmin =t1;
					float3 pPos = ro+t1*rd;
					float3 pn = float3(0,1,0);
					occ = 1-sphOcclusion2(pPos,pn,sph);
					//occ *= 1-sphOcclusion2(pPos,pn,sph2);
				}

				float t2 = sphIntersect( ro, rd, sph );
			
				if( t2>0.0 && t2<tmin)
				{
					tmin = t2;
					float3 spos = ro + t2*rd;
					nor = sphNormal(spos, sph );
					
					occ =sphOcclusion1( spos, nor, sph );
					occ = 0.5 + 0.5*nor.y;
				}

				//t2 = sphIntersect( ro, rd, sph2 );
				//if (t2>0 && t2<tmin)
				//{
				//	tmin = t2;
				//	float3 s2pos = ro +t2*rd;
				//	nor =sphNormal(s2pos, sph2 );
				//	occ = sphOcclusion1( s2pos, nor, sph2 );
				//	occ = 0.5 + 0.5*nor.y;
				//}

				if( tmin<100.0 )
				{				
					col =1;
					
					float3 pos = ro + tmin*rd;
					col *=clamp( dot(nor,lig), 0.0, 1.0 );
					
					//if(tmin>t2)
				    //shadow= sphSoftShadow( pos, lig, sph2, 4.0 );
					
					//if( vis==1 ) col.rgb = float3(1.0,1.0,1.0);
					//if( vis==2 ) col.rgb = float3(1.0,1.0,0.0);
					//if( vis==3 ) col.rgb = float3(1.0,0.0,0.0);
					col +=0.3* occ;
					col *=lerp(shadow,1,0.8);
					
					col *= exp( -0.05*tmin );
				}
				
////////////////////////////////////////////////////////////////////	
				#else 
				float th = (-1.0+2.0*smoothstep( 0.8, 0.9, sin( _Time.y*1.0 )));

				float t1 = sphIntersect( ro, rd, sph );
				float t2 = plnIntersect( ro, rd, pl1 );
				float t3 = plnIntersect( ro, rd, pl2 );
				float t4 = plnIntersect( ro, rd, pl3 );
				float t5 = plnIntersect( ro, rd, pl4 );
				float t6 = plnIntersect( ro, rd, pl5 );
				
				
				if( t2>0.0 && t2<tmin ) { tmin=t2; omin=pl1; }
				if( t3>0.0 && t3<tmin ) { tmin=t3; omin=pl2; }
				if( t4>0.0 && t4<tmin ) { tmin=t4; omin=pl3; }
				if( t5>0.0 && t5<tmin ) { tmin=t5; omin=pl4; }
				if( t6>0.0 && t6<tmin ) { tmin=t6; omin=pl5; }

				if( tmin<999.0 )
				{    
					float3 pos = ro + tmin*rd;

					col = 1;
					col *= 0.8 + 0.4*dot(omin.xyz,lig);
        
					float3 w = abs(omin.xyz);
					
					col *= 0.3;
					
					occ *= smoothstep( 0, 1, length( pos.xy-float2( 1.0, -1.0)));
					occ *= smoothstep( 0.0, 1, length( pos.xy-float2(-1.0, -1.0)));
					occ *= smoothstep( 0.0, 1, length( pos.xy-float2( 1.0, 1.0)));
					occ *= smoothstep( 0.0,1, length( pos.xy-float2(-1.0, 1.0)));
					occ *= smoothstep( 0.0, 1, length( pos.yz-float2( 1.0,-1.0)));
					occ *= smoothstep( 0.0, 1, length( pos.yz-float2( -1.0,-1.0)));
					occ *= smoothstep( 0.0, 1, length( pos.xz-float2( 1.0,-1.0)));
					occ *= smoothstep( 0.0, 1, length( pos.xz-float2(-1.0,-1.0)));
					col *= float4(0.5,0.5,0.5,0.5) + float4(0.5,0.5,0.5,0.5)*occ;
					//if( uv.x<th )
					//col *= 1.0 - 0.6*sphOcclusion1( pos, omin.xyz, sph );
        
				}


				float h = sphDensity(ro, rd, sph.xyz, sph.w, tmin );
				if( h>0.0 )
				{
					col = lerp( col, float4(0.25,0.20,0.15,1), h*h );
					col = lerp( col, 1.15*float4(1.0,0.9,0.6,1), h*h*h*h );
				}
////////////////////////////////////////////////////////////////////				
			   #endif

			

				col = sqrt(col);
				//garma
				//col = pow( col, float4(0.4545,0.4545,0.4545,1));
				
                return col;
            }
            ENDCG
        }
    }
}
