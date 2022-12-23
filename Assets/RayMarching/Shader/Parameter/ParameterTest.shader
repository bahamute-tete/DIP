Shader "RayMarch/ParameterTest"
{
    Properties
    {
        _BG ("BG", 2D) = "white" {}
		_Sph ("Sph", 2D) = "white" {}
		_Box ("Box", 2D) = "white" {}
		_t_aroundX("camerax",Range(0,1.5))=0
		_t_aroundY("cameray",Range(0,3.14))=0
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
			#pragma fragmentoption ARB_precision_hint_nicest
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

            sampler2D _BG;
			sampler2D _Sph;
			sampler2D _Box;
			float _t_aroundX;
			float _t_aroundY;
           


		   float2x2 Rot (float a)
			{

				float s= sin(a);
				float c= cos(a);
				return float2x2 (c,-s,s,c);
			}

			float2 sdSphere (float3 p,float3 sph,float r,float matid)
			{	
				
				float d = length(p-sph)-r;
				return float2(d,matid);
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

			float2 sdBox (float3 p,float3 s,float matid)
			{
				float3  q = abs (p)-s ;	
				float i =min(max(q.x,max(q.y,q.z)),0.0);
				float d= length (max(q,0))+i;
				return float2(d,matid);
			}

			float sdPlane (float3 pos,float h)
			{
				//pos*=2;
				float d = pos.y -h ;
				return d;
			}

			float2 map(float3 pos)
			{	
				float2 d1= 1000 ;
				
				//float2 d1 = -sdSphere(pos,float3(0,0,0),10,1.0);
				
				float2 d2 = sdSphere(pos,float3(-1.5,0,0),1, 2.0);
				float2 d3 = sdBox(pos-float3(1.5,0,0),float3(0.7,0.7,.7),3.0);
				if( d2.x<d1.x) d1 = d2;
				if( d3.x<d1.x) d1 = d3;
				return d1;

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
				
				return normalize( e.xyy*map( pos + e.xyy ).x + 
								  e.yyx*map( pos + e.yyx ).x + 
								  e.yxy*map( pos + e.yxy ).x + 
								  e.xxx*map( pos + e.xxx ).x );
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
					float d = map( pos + h*nor ).x;
					occ += (h-d)*sca;
					sca *= 0.9;
				}
				return clamp( 1.0 - 1.5*occ, 0.0, 1.0 );    
			}

			float2 CubeMap (float3 pos)
			{

				float2 uv = 0;

				
				if (abs(pos.y)<abs(pos.x) && abs(pos.z)<abs(pos.x))//-x
				{
					uv = 0.5*(1+float2(pos.z,pos.y)/abs(pos.x));
				}

	
				if (abs(pos.x)<abs(pos.y)&& abs(pos.z)<abs(pos.y))//-y
				{
					uv = 0.5*(1+float2(pos.x,pos.z)/abs(pos.y));
				}

	

				if (abs(pos.y)<abs(pos.z) && abs(pos.x)<abs(pos.z))//-z
				{
					uv = 0.5*(1+float2(pos.x,pos.y)/abs(pos.z));
				}

				return uv;
	
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
                
				float2 uv  =  (2*i.uv-1)*_ScreenParams/_ScreenParams.y;

				float4  col= 0;
				//col = tex2D(_BG, i.uv);
				float pi = 3.1415;
				float3 ro =float3(0,0,3);
				ro.yz = mul(Rot(_t_aroundX),ro.yz);
				ro.xz = mul(Rot(_t_aroundY),ro.xz);
				float3 ta = float3(0,0,0);

				float3 rd = 0;
				rd = CameraRd(ro,ta,uv);
				float3 lig = normalize (float3 (5,5,5)); 
				//lig.xz = mul(Rot(_Time.y),lig.xz);

				float3 pos =0;
				
				float3 nor = 0;
				float3 shadow = 0.0;
				
				float sid = 0 ;

				const float tmax = 30.0;
				float t = 0.0;
				for( int i=0; i<128; i++ )
				{
					float3 pos = ro + rd*t;
					float2 h = map(pos);
					if(abs(h.x)<0.01 ) break;
					t += h.x;
					sid = h.y;
					if( t>tmax ) break;
				}
				
				
				
				if( t<tmax )
				{	
				    col =1;
					float3 pos = ro + rd*t;
					float3 nor = calcNormal( pos, 0.01 );
					
					
					float sha =Shadow(pos+nor*0.01,lig,8)+0.2;
			
					float ao = calcAO(pos,nor);

					float3 reflec = reflect(rd,nor);

		            float rim = pow(clamp(1+dot(rd,nor),0,1),4.0);
					
					float amb = clamp(0.6 + 0.4*nor.y, 0.0, 1.0);
					float diff  = clamp (dot (nor,lig),0,1)*0.5+0.5;

					if (sid >0.5 && sid<2.5)
					{
						float2 uvBG = float2((atan2(nor.z,nor.x)/pi)*0.5+0.5,(acos(nor.y)/pi)*0.5);
						
					   // float2 uvBG2 = getSphereMappedTexture(pos);
						pos +=float3(1.5,0,0);
						pos.xz = mul(Rot(_Time.y*0.5),pos.xz);
						float2 uvBG2 = CubeMap(pos);

						col = tex2D(_Sph,uvBG2);
						float3 h = normalize(lig-rd);
						float spe =3/pow(length(float3(2,2,2)-pos),2)* pow(clamp(dot(h,nor),0,1),64);
						//float specu = pow(dot(reflec,lig),8);
						//float specu = pow(dot(h,nor),8);
						col =col*diff+spe*float4(0.9,0.95,1,1);
						col += 0.6*rim*pow(ao,2.0);
						float fre = 0.3 + 0.7*pow( clamp( 1.0 + dot( rd, nor ), 0.0, 1.0 ),4.0 );
						float4 sss = tex2D( _Box, reflec );
						//col += 1.0*pow(sss,2.0)*fre;
					}
					
					if(sid >2.5)
					{	
						float r = 1;
						float dir = dot(nor,normalize( float3(0,0,1)));
						//if(dir>-0.5)
						//col = tex2D(_Box,float2(pos.x,pos.z)*r);
						//else 
						//col = tex2D(_Box,float2(pos.x,pos.y)*r);
						pos -=float3(1.5,0,0);
						//pos.xz = mul(Rot(_Time.y*0.5),pos.xz);
						float2 uvBox = CubeMap(pos);
						col = tex2D(_Box,uvBox);
						col *=diff;

					}

					col =ao*col*col*sha;
					col *= amb;
					col *= 2;
	
				}

               col = sqrt( col );

                return col;
            }
            ENDCG
        }
    }
}
