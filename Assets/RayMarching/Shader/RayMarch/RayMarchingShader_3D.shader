Shader "RayMarch/RayMarchingShader_3D"
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
			#include "UnityLightingCommon.cginc"
			#include "AutoLight.cginc"

			#define _MAX_STEPS 100
			#define _MAX_DISTANCE 100
			#define _SURFACE_DISTANCE 1e-2

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float3 wPos :TEXCOORD1;
				float3 ro:TEXCOORD2;
				float3 hitPos:TEXCOORD3;
				float3 lorigin:TEXCOORD4;
                float4 vertex : SV_POSITION;
            };


            sampler2D _MainTex;
            float4 _MainTex_ST;


////////////////////////////////////////////////////////////////////////////////
			float2x2 Rot (float a)
			{
				float s = sin(a);
				float c = cos(a);
				return float2x2(c,-s,s,c);
			}

			float mix(float a,float b,float c)
			{
				return a*(1-c)+b*c;
			}

			float smin(float a ,float b ,float k)
			{
				float h = clamp(0.5+0.5*(b-a)/k,0.0,1.0);
				return mix(b,a,h) - k*h*(1.0-h);
			}

			float sdBox (float3 p ,float3 v)
			{
				float3 q = abs(p) - v;
				
				float box = length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
				return box;
			}

			float sdSphere (float3 p ,float r)
			{
			    float sphere = length (p)-r;
				return sphere;
			}


			float GetDistance(float3 p )
			{				
				float pd = p.y;

				float sphereRadius =0.2;
				float3 spherePosA = p;
				spherePosA -= float3 (0,0.2,0);
				spherePosA *=float3(1.1,2,1);

				float3 spherePosB = p;
				spherePosB -= float3 (0,0.2,0);
				spherePosB *=float3(1,1,1);

				float3 boxData =0.2;
				float3 boxPos =p;
				//boxPos.xz =mul(Rot(_Time.y),boxPos.xz) ;
				boxPos -=float3(0,0.2,0);

				 

				float bd = sdBox(boxPos,boxData);
				float sdA = sdSphere(spherePosA,sphereRadius);
				float sdB = sdSphere(spherePosB,sphereRadius);

				float mixD = mix (sdB,bd,sin(_Time.y)*0.5+0.5);
				//float mixd = max(-sdA/2,sdB/1);//subtrantion
				//float mixD = max(mixd,bd);//intersection
				float  d= min(mixD,pd);
				
				
				return d;
			}

			
			float opElongate( in float primitive, in float3 p, in float3 h )
			{
				float3 q = p - clamp( p, -h, h );
				return GetDistance( q );
			}

			float RayMarch (float3 ro ,float3 rd)
			{
				float dOrigin =  0 ;
				float pDistance =0 ;
				for (int  i = 0 ;i <_MAX_STEPS ; i++)
				{
					float3 p = ro +rd*dOrigin;
					pDistance  = GetDistance (p);
					dOrigin += pDistance;
					if (dOrigin >_MAX_DISTANCE || pDistance<_SURFACE_DISTANCE)
					break;
				}
				return dOrigin;
			}


			float3 GetNormal (float3 p)
			{

			    /*
			    曲面 f(x,y) = 0 的隐函数 设（a,b）为曲线上的一点  对f(x,y)求导 得到切向量（dx,dy）所以有切线方程  d(f(x,y))(x-a)+d(f(x,y))(y-b) =0 
				在对切线方程求导 得到梯度 G  G=法线 所以有 N=floa2( （x-a），（x-b）)
				*/

				/*
				*/
				float2 e= float2 (1e-2,0);
				
				float3 n = GetDistance(p)- float3 (GetDistance(p-e.xyy),GetDistance(p-e.yxy),GetDistance(p-e.yyx));
				
				float dx = (GetDistance(p+e.xyy)-GetDistance(p)).x /e.x;
				float dy = (GetDistance(p+e.yxy)-GetDistance(p)).x /e.x;
				float dz = (GetDistance(p+e.yyx)-GetDistance(p)).x /e.x;

				float2  iqe = float2(1.0,-1.0)*0.5773*0.0005;
                float3 iqn=  iqe.xyy*GetDistance( p + iqe.xyy ).x + 
							 iqe.yyx*GetDistance( p + iqe.yyx ).x + 
							 iqe.yxy*GetDistance( p + iqe.yxy ).x + 
							 iqe.xxx*GetDistance( p + iqe.xxx ).x ;


				float3 newN = float3(dx,dy,dz);

				return normalize(iqn);
			}


			float3 GetLight(float3 p)
			{

				float3 l =_WorldSpaceLightPos0-p;
				float3 n = GetNormal (p);

				float diff = 0.5*saturate (dot(n,l))+0.5;

				float d = RayMarch(p+ n*_SURFACE_DISTANCE*2,l);
				if (d < length(l -p)) 
				diff =diff* 0.8;

				return diff;
			}
///////////////////////////////////////////////////////////////////////////////
            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.wPos = mul (unity_ObjectToWorld , v.vertex );
				o.hitPos = v.vertex;
				o.ro =mul (unity_WorldToObject,float4(_WorldSpaceCameraPos,1)); 
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 texCol = tex2D(_MainTex,i.uv);
				fixed4 col = 0.2;
				float2 uv  = i.uv-0.5;
				float3 ro = i.ro;
				float3 rd =normalize(i.hitPos -ro );
				float3 lo = i.lorigin;
			
				float d = RayMarch(ro,rd);

				float m = dot(uv,uv)*1;

				

				if (d<_MAX_DISTANCE)
				{
					float3  p  = ro +rd *d;
					float3 n = GetNormal(p);
					float diff = GetLight(p);
					col.rgb =diff;
				}else
				discard;
				
				
				//col = lerp (col,texCol,smoothstep(0.1,0.2,m));
				
                return col;
            }
            ENDCG
        }
    }
}
