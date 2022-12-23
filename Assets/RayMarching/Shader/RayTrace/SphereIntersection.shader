Shader "RayMarch/SpherIntersection"
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

			float remap01  (float a,float b,float c)
			{
				return (c-a)/(b-a);
			}

			float iPlane( in float3 ro, in float3 rd )
			{
				return (-1.0 - ro.y)/rd.y;
			}

			float sphIntersect( float3 ro, float3 rd, float3 sc ,float sr )
			{

				/*
					球公式和直线公式 求解 有一元二次方程
					D =b*b -4ac if (D>0)have 2Points  t = (-b-sqrt(D))/2a
					下面是这个公式的的优化
				*/
				 float3 oc = ro -sc;
				 float  b = dot (rd,oc);
				 float c = dot(oc,oc)-sr*sr;
				 float  D = b*b -c;
				 float t= 0;
				 if (D<0) return -1.0; 				 
				 return   -b-sqrt(D) ;
			}

			float3 sphNormal(  float3 pos, float3 sc )
			{
				return normalize(pos-sc);
			}

			float sphOcclusion( in float3 pos, in float3 nor, in float4 sph )
			{
				float3  r = sph.xyz - pos;
				float l = length(r);
				return dot(nor,r)*(sph.w*sph.w)/(l*l*l);
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

                fixed4 col =0;
				float2 uv = (2*i.uv-1)*_ScreenParams/_ScreenParams.y;
				float3 ro = float3 (0,0,3);
				float3 rd =normalize( float3 (uv.x,uv.y,-1));
				float r =2;
				float occ = 1.0;
				float t1 =0;
				float t2 =0;

				float3 sc = float3 (0,0.5,1);
				float4 sph = float4(0,0.5,1,0.5);
				float3 lightPos =normalize( float3(0.6,0.0,0.5));
				//float3 op = sc - ro;
				//float  t  = dot(op,rd);//dot(p,q) =|p||q|cosa ==>|q|=1 ==> dot(p,identity) = |p|cosa =t
				//float3 p_ray = ro +rd*t;
				//float h =length (op - p_ray);
				//col = smoothstep(2,1,h);
				//if ( h <r)
				//{
				//	 t1 = t-sqrt(r*r-h*h);
				//	 t2 = t+sqrt(r*r-h*h);

				//	 float c = remap01(sc.z,sc.z-r,t1);
				//	 col = c;
				//}
				//float t_plane = iPlane( ro, rd );
				//if (t_plane>0)
				//{
				//	float3 pos = ro +t_plane*rd;
				//	float3 nor = float3(0,1,0);
				//	occ =1- sphOcclusion(pos,nor,sph);
				//}
				float t = sphIntersect(ro,rd,sc,r);
				float3  psphere = ro +t*rd;	
				
				float3 n = sphNormal( psphere, sc );

				

				float3 l = normalize(lightPos - psphere);

				float dif  =  saturate( dot(n,l));
               // float occ = 0.5 + 0.5*nor.y;
				col =dif;


                return col;
            }
            ENDCG
        }
    }
}
