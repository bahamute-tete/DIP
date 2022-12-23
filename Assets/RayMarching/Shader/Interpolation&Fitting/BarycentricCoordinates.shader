Shader "RayMarch/BarycentricCoordinates"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_tx ("floatX",Range(-2,2))= 0
		_ty ("floatY",Range(-2,2))= 0
		
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
			float _tx;
			float _ty;

			float2 _p1;
			float2 _p2;
			float2 _p3;
			float3 _p1c;
			float3 _p2c;
			float3 _p3c;

			float2 _MousePos;
			float _Speed;
			float _deltaTime;
			

			float2x2 Rot(float a)
			{
				float s = sin(a);
				float c = cos(a);
				return float2x2(c,-s,s,c);
			}

			float2 N22(float2 p)
			{
				float3 a = frac(p.xyy * float3(.1031,.11369,.13787));
				a += dot (a,a.yxz+34.45);
				return -1+2*frac(float2 (a.x *a.y ,a.y *a.z));

			}

			float sdLine(float2 p ,float2 a ,float2 b)
			{
				float2 pa = p-a;
				float2 ba = b-a;
				float h =clamp(dot(pa,ba)/dot(ba,ba),0,1);
				float d =length( pa - ba*h);
				return d;
			}

			float sdCircle(float2 p ,float2 a ,float r)
			{
				return length(p-a)-r;
			}

			float sdPoint (float2 p ,float2 a )
			{
				return length(a-p);
			}

			bool insideTriangle( float2 p,float2 a ,float2 b,float2 c)
			{
				
				float3 vp = float3(p,0),va=float3(a,0),vb= float3(b,0),vc=float3(c,0);
				float3 pa = vp-va, pb = vp-vb,pc = vp-vc;		
				float3 ba = vb-va, cb = vc-vb,ac = va-vc;

				float si1 = sign(cross(ba,pa).z);
				float si2 = sign(cross(cb,pb).z);
				float si3 = sign(cross(ac,pc).z);
				
				if(si1 >0 && si2>0 && si3 >0) return 1;
				else
				return 0;
			}

			float3 Barycentric( float2 p,float2 a ,float2 b,float2 c)
			{
				float3 vp = float3(p,0),va=float3(a,0),vb= float3(b,0),vc=float3(c,0);
				float3 pa = vp-va, pb = vp-vb,pc = vp-vc;		
				float3 ba = vb-va, cb = vc-vb,ac = va-vc;

				float beta = 0;
				float gama = 0;
				float alpha = 0;
				float3 n = cross(ba,-ac);
				 alpha  = dot(n,cross(ba,pa))/ (length(n)*length(n));
				 beta  =  dot(n,cross(cb,pb))/ (length(n)*length(n));
				 gama  =  dot(n,cross(ac,pc))/ (length(n)*length(n));

				return float3(alpha ,beta ,gama);
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
                
                float4 col = 0;
                float2 uv = (2*i.uv-1)*_ScreenParams/_ScreenParams.y;
				uv *= 2;
				

			    ////////////////////////////////
				//float t =min(fmod(_Time.y/10,1),1);
				float t =_Time.z;
				float4 change =float4( 0.5*cos(t+uv.xyx+float3(0,2,4))+0.6,1);
				float speed = _Speed;
				//uv = mul(Rot(t),uv);

				//float2 a = float2(-2,-1);
				//float4 coa= float4(1,1,0,1);
				float2 a = _p1;
				float4 coa= float4(_p1c,0);

				//float2 b = float2(1.4,-1.3);
				//float4 cob= float4(1,0,0,1);
				float2 b = _p2;
				float4 cob=float4( _p2c,0);
				
				//float2 c = float2(0,1.5);
				//float4 coc= float4(1,0,1,1);
				float2 c = _p3;
				float4 coc=float4( _p3c,0);

				float2 p  = 2* _MousePos;

				//float2 v = N22(uv);
				////////////////////////////////////////
				float4 BarycentricP = float4(Barycentric(p,a,b,c),1);
				float4 lc = BarycentricP.y*coa + BarycentricP.z*cob+ BarycentricP.x*coc;
				lc*=change;

				float psda =  sdPoint(uv,a);
				float psdb =  sdPoint(uv,b);
				float psdc =  sdPoint(uv,c);

				col+= smoothstep(0.06,0.05,psda)*lc;
				col+= smoothstep(0.06,0.05,psdb)*lc;
				col+= smoothstep(0.06,0.05,psdc)*lc;

				float ba = sdLine(uv,b,a);
				float cb = sdLine(uv,c,b);
				float ac = sdLine(uv,a,c);

				float px = fwidth(ba)*4;
				col+= smoothstep(px,0,ba);
				col+= smoothstep(px,0,cb);
				col+= smoothstep(px,0,ac);
			 ////////////////////////////////
				
				

				if(insideTriangle(p,a,b,c))
				{

					float dp = sdCircle(uv,p,0.1)-0.2;
					float4 BarycentricP = float4(Barycentric(p,a,b,c),1);
					float4 lc = BarycentricP.y*coa + BarycentricP.z*cob+ BarycentricP.x*coc;
					lc*=change;
					col += smoothstep(0.08,0.07,abs(dp))*lc;

					float lp0 = sdLine(uv,p,a);
					float lp1 = sdLine(uv,p,b);
					float lp2 = sdLine(uv,p,c);
					float px = fwidth(lp0)*2;

					col+= smoothstep(px,0,lp0)*lc;
					col+= smoothstep(px,0,lp1)*lc;
					col+= smoothstep(px,0,lp2)*lc;
				}


				if(insideTriangle(uv,a,b,c))
				{	
					
					float4 BarycentricP = float4(Barycentric(p,a,b,c),1);
					float4 lc = BarycentricP.y*coa + BarycentricP.z*cob+ BarycentricP.x*coc;
					lc*=change;
					col +=lc;
				}

				
			


                return col;
            }
            ENDCG
        }
    }
}
