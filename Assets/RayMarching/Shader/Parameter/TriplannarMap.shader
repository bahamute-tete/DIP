Shader "RayMarch/TriplannarMap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MainTex2 ("Texture2", 2D) = "white" {}
		_d("distance",Range(-15,15))=3
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
			sampler2D _MainTex2;
            float4 _MainTex2_ST;
			float _d;
//////////////////////////////////////////////////////
			float2x2 rotmat (float a)
			{
				float k =1;
				float t = _Time.x*10;
				float s= sin(k*a+t);
				float c= cos(k*a+t);
				return float2x2 (c,-s,s,c);
			}

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


			float N210(float2 uv)
			{
				//float c = frac(sin(uv.x*100+uv.y*6875)*5647.0);
				float n= frac(sin(dot(uv,float2(200,6885)))*5674.0);
				return n;
			}

			float N21 (float2 p)
			{
				 p = frac(p*float2(234.56,567.89));
				 p+=dot(p,p+34.52);
				 return frac(p.x*p.y);
			}

			float2 N22(float2 p)
			{
				float3 m = frac(p.xyx*float3(234.56,567.89,365.23));
				m+=dot(m,m+34.52);
				return frac( float2 (m.x * m.y, m.y * m.z));
			}

			float2 N220(float2 p)
			{
				float2 m =float2( dot(p,float2(123.56,627.6)),dot(p,float2(345.56,567.12)));
				 m= -1+2*sin(m *3854.36);
				return frac(m);
			}

			float ValueN(float2 uv)
			{
				float2 gv = frac(uv);
				
				gv= gv*gv*(3-2*gv);
				float2 id = floor(uv);

				float bl = N210(id);
				float br = N210(id+float2(1,0));
				float a = lerp(bl,br,gv.x);

				float tl = N210(id+float2(0,1));
				float tr = N210(id+float2(1,1));
				float b = lerp(tl,tr,gv.x);
				
				float c = lerp(a,b,gv.y);
				return c;
			}

			float PerlinN(float2 uv)
			{
				float2 gv = frac(uv);
			    
				gv= gv*gv*(3-2*gv);
				float2 id = floor(uv);
				
				
				float2 bl = N220(id);
				float2 br = N220(id+float2(1,0));
				float2 tl = N220(id+float2(0,1));
				float2 tr = N220(id+float2(1,1));

				float j = dot(bl,gv);
				float k = dot(br,gv-float2(1,0));
				float l = dot(tl,gv-float2(0,1));
				float g = dot(tr,gv-float2(1,1));

				float a = lerp(j,k,gv.x);
				float b = lerp(l,g,gv.x);
				float c = lerp(a,b,gv.y);

				return c;
			}

			float Voroino (float2 p, out float2 cellid)
			{
				
				float2 gv = frac(p)-0.5;
				float2 id = floor(p);
				float minD =100;
				 cellid =0;

				for(int y =-1;y<=1;y++)
				{
					for(int x=-1;x<=1;x++)
					{
						float2 offsetuv = float2(x,y);
						float2 n = N21(id+offsetuv);
						float2 p =0.5* sin(n*_Time.y)+offsetuv;
						float d = length(gv-p);
						if (d<minD)
						{
							minD = d;
							cellid =id+offsetuv;
						}
					}
				}
				return minD;
			}

			float FBM_Value (float2 p,float h)
			{
				p*=4;
				float G= exp2(-h);
				float f= 1;
				float t= 0;
				float a=1;
				int num = 5;
				for (int j = 0 ; j<num ; j++)
				{
					t +=abs(ValueN(p*f))*a;
					f*=2;
					a *=G;
				}

				return t/2;
			}

			float FBM_Perlin (float2 p,float h)
			{
				p*=4;
				float G= exp2(-h);
				float f= 1;
				float t= 0;
				float a=1;
				int num = 5;
				for (int j = 0 ; j<num ; j++)
				{
					t +=abs(PerlinN(p*f))*a;
					f*=2;
					a *=G;
				}

				return t;
			}


			float3  repeat (float3 p,float3 c)
			{
				float3 q = exp2(c)*(frac(exp2(-c)*(p+c))-0.5);
				return q;
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

			float sdSphere (float3 p,float4 sph)
			{	
				
				float d = length(p-sph.xyz)-sph.w;
				return d;
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

			float sdPlane (float3 pos,float h)
			{
				float d = pos.y -h ;
				return d;
			}

			float sdBox (float3 p,float3 s)
			{	
				
				#if 0
				//float3 rp = fmod(abs(p)+1,2)- 1 ;
				float3 rp = fmod(abs(p) + float3(3,0.5,2), float3(6,1,2))- float3(3,0.5,2);
				#else
				float3 rp  =repeat(p,float3(2,0.5,2));
				#endif
				//rp.xz= mul(rotmat(rp.y),rp.xz);
				//rp.xz= mul(rotmat(_Time.x),rp.xz);
				float3  q = abs (rp)-s ;	
				float i =min(max(q.x,max(q.y,q.z)),0.0);
				float box= length (max(q,0))+i;
				return box;
			}

			float map(float3 pos)
			{	
				float d=0;
				float y = 0+sin(_Time.y)*0.5*0;
				float4 sph = float4 (4*sin(_Time.y),0,2*cos(_Time.y*0.5),2.5);
				float4 sph2 = float4 (1.5*cos(_Time.y),2,1*sin(_Time.y),1);

				float3 size =0.58;
				float dbox =sdBox(pos*float3(1,1,1),size)-0.05;
				//dbox +=FBM_Perlin(0.1*pos.xz,1);
				//dbox +=0.05*sin(10*pos.x)+0.05*cos(10*pos.y)+0.05*cos(10*pos.z);
				float dsph1 = sdSphere(pos,sph);
				//float3 q= fmod(abs(pos)+3,6)-3;
				float dsph2 = sdSphere(pos,sph2);
				//dsph2 += 0.01*sin(20*pos.x)+0.01*sin(20*pos.y)+0.01*sin(20*pos.z);
				//float d =  smin( sdSphere(pos,sph),sdPlane(pos,0),1);

				//float dplane= abs(sdPlane(pos,0))-0.4;
				float dplane= sdPlane(pos,0);
				dplane +=FBM_Value(0.1*pos.xz,1);
				float d2 =smax( dsph1,dplane,1);
				float d3= smin(d2,dsph2,0.2);
				d= smin(d2,dbox,0.1);
				//d = dbox;
				return d/2;
			}

			float3 calcNormal (float3 pos,  float eps )
			{
				
				float2 e = float2(1.0,-1.0)*0.5773*eps;
				
				return normalize( e.xyy*map( pos + e.xyy ) + 
								  e.yyx*map( pos + e.yyx ) + 
								  e.yxy*map( pos + e.yxy ) + 
								  e.xxx*map( pos + e.xxx ) );
			}

			float Shaded (float3 pos ,float3 lig,float3 nor)
			{
				float3 l = normalize(lig-pos);
				float diff =clamp( dot(nor,l),0,1);
				return diff;
			}


			float4 boxmap(  sampler2D s,  float3 p,  float3 n,  float k,float scale )
			{
			// project+fetch
			float4 x = tex2D( s, p.yz*scale );
			float4 y = tex2D( s, p.zx*scale );
			float4 z = tex2D( s, p.xy*scale );
    
			// and blend
			float3 m = pow( abs(n), float3(k,k,k) );
			return (x*m.x + y*m.y + z*m.z) / (m.x + m.y + m.z);
			}

/////////////////////////////////////////////////////
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv -0.5;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float2 uv = i.uv*_ScreenParams/_ScreenParams.y;
                float4 col = 0;
				float3 ro = float3(3,2,10*sin(_Time.x*2));
				float3 ta = float3(0+sin(_Time.y*0.5*0),2,0+cos(_Time.y*0.5*0));
				float3 rd = 0;
				rd = CameraRd(ro,ta,uv);
				float3 lig = normalize (float3 (1,1,1)); 
			    float4 sph2 = float4 (2*cos(_Time.y),(2+sin(_Time.y*2)),2*sin(_Time.y),0.25);
				float4 sph3 = float4 (2*sin(_Time.y*0.8),(2+cos(_Time.y*2)),2*cos(_Time.y*0.8),0.25);

				float3 pos =0;
				float3 nor = 0;
			


				
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
					float occ = clamp(0.5 + 0.5*nor.y, 0.0, 1.0);
					float diff  = clamp (dot (nor,lig),0,1)*0.5+0.5;
					col *= boxmap( _MainTex2,2* pos, nor, 8.0 ,0.2);
					float h = sphDensity(ro, rd, sph2.xyz, sph2.w, t );
					float h1 = sphDensity(ro, rd, sph3.xyz, sph3.w, t );
					if( h>0.0 )
					{
						h = min(h,t);
						col = lerp( col, float4(0.25,0.80,0.85,1), h*h*h );
						col += lerp( col, 1.1*float4(0.5,0.9,0.9,1), h*h*h*h*h);
					}

					if( h1>0.0 )
					{	
						h = min(h,t);
						col = lerp( col, float4(0.95,0.40,0.15,1), h1*h1*h1 );
						col += lerp( col, 1.1*float4(1,0.9,0.9,1), h1*h1*h1*h1*h1);
					}
					col = col*col*diff;
					col *= occ;
					col *= 2.0;
					col *= 1.0-smoothstep(1.0,12.0,length(pos.xz));
				}

               col = sqrt( col );
                return col;
            }
            ENDCG
        }
    }
}
