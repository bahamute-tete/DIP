Shader "RayMarch/RayMarchShadow"
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
			sampler2D _iChannel0;
			sampler2D _iChannel1;

			float N21(float2 p)
			{
				 return -1+2*frac(sin(dot(p,float2(100,6875)))*5637.56);
				             
				//p = frac(p*float2(234.56,567.45));
				//p += dot(p,p+34.56);
				//return  frac(p.x*p.y);
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



			float smin(float a ,float b ,float k)
			{
				float h = clamp(0.5+0.5*(b-a)/k,0.0,1.0);
				return lerp(b,a,h) - k*h*(1.0-h);
			}


			float2x2 rotmat (float a)
			{
				float k =1;
				float t = _Time.x*10;
				float s= sin(k*a+t);
				float c= cos(k*a+t);
				return float2x2 (c,-s,s,c);
			}

			float2x2 Rot (float a)
			{

				float s= sin(a);
				float c= cos(a);
				return float2x2 (c,-s,s,c);
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

			float sdBox (float3 p,float3 s)
			{
				float3  q = abs (p)-s ;	
				float i =min(max(q.x,max(q.y,q.z)),0.0);
				float box= length (max(q,0))+i;
				return box;
			}

			float sdPlane (float3 pos,float h)
			{
				//pos*=2;
				float d = pos.y -h ;
				return d;
			}

			float map(float3 pos)
			{	
				float d = 1000;


				float dbox = sdBox(pos,float3(2,1,0.25))/2-0.02;
				float dcapusle= sdCapusle(pos-float3(0,-0.5,0),float3(0,0,1),float3(0,0, -1),1.3);
				
				float noisy = FBM_ValueNoisy(pos.xz+float2(cos(_Time.y*0.5),sin(_Time.y*0.7)),1.2);
				float dn =  2*sin(10*pos.x+_Time.y)*sin(10*pos.y+_Time.y)*sin(10*pos.z+_Time.y);
				float3 sppp = pos*float3(0.8,1.5+0.5*(cos(10*_Time.y)*0.5+0.5),1.3);
				float dsph = sdSphere(sppp,float3(0,1.5,0),1)/2+noisy*0.03;
				float dplane = sdPlane(pos,-1);

				d=min(d, min(dplane,smin(dsph,max(-dcapusle,dbox),0.45)));

				return d;

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
				float tmin= 0.001;
				float tmax = 30.0;
				for(float t=tmin; t<tmax;)
				{
					float h = map(ro + rd*t);
					if( h<0.001 )
					return 0;
					res = min( res,saturate( k*h/t) );

					//float y = h*h/(2.0*ph);
					//float d = sqrt(h*h-y*y);
					//res = min( res, k*d/max(0.0,t-y) );
					ph = h;
					t += h;
				}
				return res;
			}



			float calcAO( float3 pos, float3 nor )
			{
				float occ = 0.0;
				float sca = 1.0;
				for( int i=0; i<5; i++ )
				{
					float h = 0.001 + 0.15*float(i)/4.0;
					float d = map( pos + h*nor );
					occ += (h-d)*sca;
					sca *= 0.9;
				}
				return clamp( 1.0 - 1.5*occ, 0.0, 1.0 );    
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =v.uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {               
				float4 col = 0;
				float2 uv  =  (2*i.uv-1)*_ScreenParams/_ScreenParams.y;

				float3 ro =float3(0,1,3);
				ro.xz = mul(rotmat(0.2),ro.xz);
				float3 ta = float3(0,0,0);

				float3 rd = 0;
				rd = CameraRd(ro,ta,uv);
				float3 lig = normalize (float3 (1,1,1)); 


				float3 pos =0;
				float3 nor = 0;
				float3 shadow = 0.0;

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
					float3 nor = calcNormal( pos, 0.01 );
					float amb = clamp(0.5 + 0.5*nor.y, 0.0, 1.0);
					float diff  = clamp (dot (nor,lig),0,1)*0.5+0.5;
					float sha =  Shadow(pos+nor*0.01,lig,8)+0.5;
					float ao = calcAO(pos,nor);
					float3 reff = reflect(rd,nor);
		            float rim = pow(clamp(1.0+dot(nor,rd),0.0,1.0),2.0);
					col = tex2D( _iChannel1, nor );

					float fre = 0.3 + 0.7*pow( clamp( 1.0 + dot( rd, nor ), 0.0, 1.0 ), 10.0 );
					float4 sss = tex2D( _iChannel0, reff );
					col += 2.0*pow(sss,4.0)*fre;

					col =ao*col*col*diff*sha*rim;
					col *= amb;
					col *= 2;
					col *= 1.0-smoothstep(1.0,25.0,length(pos.xz));
				}

               col = sqrt( col );



                return col;
            }
            ENDCG
        }
    }
}
