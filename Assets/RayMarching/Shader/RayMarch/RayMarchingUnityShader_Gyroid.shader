Shader "RayMarch/RayMarchingUnityShader_Gyriod"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_SmoothValue ("_SmoothValue",Range(0,1))=0.5

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

			#define _MAX_STEPS 100
			#define _MAX_DISTANCE 50
			#define _SURFACE_DISTANCE 1e-3


            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float3 ray:TEXCOORD1; 
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float4x4 _CameraConnerContent;
			float4x4 _MatrixCameraViewToWorld;
			float3 _CameraWPos;
			sampler2D  _CameraDepthTexture;
			sampler2D _ColorRamp;
			float _SmoothValue;

///////////////////////////////////////////////////////////////////
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

			float2 smin2(float2 a ,float2 b ,float k)
			{
				float h = clamp(0.5+0.5*(b.x-a.x)/k,0.0,1.0);
				float minValue = (a.x<b.x)? a.y:b.y;
				return float2(mix(b.x,a.x,h) - k*h*(1.0-h),minValue);
			}

			float2 min2(float2 a,float2 b)
			{
				return (a.x<b.x)? a:b;
			}
//////////////////////////////////////////////////////////////////////
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

			float sdTorus(float3 p, float2 t)
			{
				float2 q = float2(length(p.xz) - t.x, p.y);
				return length(q) - t.y;
			}

			float sdGyroid(float3 p,float scale,float thickness,float bias)//cos(x)sin(y) + cos(y)sin(z) + cos(z)sin(x) = 0.
			{
				float3 gyroidPos =p;
				gyroidPos *= scale;
				float gyroidd = abs(dot(sin(gyroidPos),cos(gyroidPos.zxy))-bias)/(scale)-thickness;
				return gyroidd;
			}

			float3 Background(float3 rd)
			{
				float3 col = 0;
				float y = rd.y*0.5+0.5;//float3 up(0,1,0) up=1 down =0 tirck
				
				
				col+=(1-y)*float3(0.9,0.5,0.1)*2;
				float a =atan2(rd.x,rd.z);
				float flame =sin(a*10+_Time.y) * sin(a*7 -_Time.y)* sin(a*5 );
				flame *=smoothstep(0.8,0.4,y);
				col+=flame;
				col = max(col,0);
				col+= smoothstep(0.5,0,y);
				return col;
			}

			float3 Translate(float3 p)
			{
				p.xy +=mul(Rot(p.z),p.xy)*0.5;
				p.z += _Time.x* 1;
				//p.y = 0.1;
				return p;
			}
/////////////////////////////////////////////////////////////////////////////////
			float2 GetDistance(float3 p) // return float2(distance,mapID)
			{

				p =Translate(p);
				//float pd = p.y;//Ground
				////////////////////////////////////
				float3 boxData =0.2;
				float3 boxPos =p;
				//boxPos.xz =mul(Rot(_Time.y),boxPos.xz);
				//boxPos.xy =mul(Rot(_Time.y),boxPos.xy);
				boxPos -=float3(0,0.5,0);
				boxPos.xz =mul(Rot(_Time.y),boxPos.xz);
				boxPos.xy =mul(Rot(_Time.y),boxPos.xy);
				float bd = sdBox(boxPos,boxData);//Cube
				float2 cube= float2(bd,sin(_Time.y*0.1+0.1)*0.5+5);
				////////////////////////////////////
				float3 boxDataB =0.25;
				float3 boxPosB =p;
				boxPosB -=float3(0,0.5,0);
				float bdB = sdBox(boxPosB,boxDataB);//Cube

				float3 spherePosC = p;
				spherePosC -= float3 (0.0,0.5,0);
				float sphereRadiusC =0.25;
				float sdC = sdSphere(spherePosC,sphereRadiusC);//Sphere

				float scale =0;
				float thickness =0.01;
				float bias=1.5;
				float gyroidd1 = sdGyroid(p,20+scale,0.02,1.5);
				float gyroidd2 = sdGyroid(p,45.6+scale,0.01,0.3);
				float gyroidd3 = sdGyroid(p,58.33+scale,0.01,0.3);
				float gyroidd4 = sdGyroid(p,80.33+scale,0.01,0.3);
				float gyroidd5 = sdGyroid(p,250+scale,0.01,0.3);
				float gyroidd6 = sdGyroid(p,500+scale,0.01,0.3);
				//float g = min(gyroidd1,gyroidd2);//union
				//float g = max(gyroidd1,-gyroidd2);//substract
				gyroidd1-=gyroidd2*0.3;//bump
				gyroidd1-=gyroidd3*0.2;
				gyroidd1+=gyroidd4*0.2;
				gyroidd1+=gyroidd5*0.35;
				gyroidd1+=gyroidd6*0.25;
				
				//float mm= max(sdC,gyroidd1*0.5);//make stepsize smaller
				float mm=gyroidd1*0.5;
				float2 gyroid= float2(mm,0.16);
			    ////////////////////////////////////
				////////////////////////////////////
				float3 spherePosB = p;
				spherePosB.xz =mul(Rot(_Time.y),spherePosB.xz);
				 spherePosB -= float3 (0.3,0.7,0);
				float sphereRadius =0.25;
				float sdB = sdSphere(spherePosB,sphereRadius);//Sphere
				float2 sphereB = float2(sdB,sin(_Time.y*0.1+0.1)*0.5+5);
				////////////////////////////////////////////
				float3 torusPos = p ;
				torusPos -= float3(-0.3,0.5,0);
				torusPos.zy =mul(Rot(_Time.y),torusPos.zy);
				float sdT =sdTorus(torusPos,float2(0.3,0.05));
				float2 torus = float2(sdT ,sin(_Time.y*0.1+0.1)*0.5+5);
				////////////////////////////////////////////
				float3 torusPosB = p ;
				torusPosB -= float3(0.0,0.8,0);
				torusPosB.xy =mul(Rot(_Time.y),torusPosB.xy);
				torusPosB.yz =mul(Rot(_Time.y*1),torusPosB.yz);
				float sdTB =sdTorus(torusPosB,float2(0.25,0.03));
				float2 torusB = float2(sdTB ,sin(_Time.y*0.1+0.1)*0.5+5);
				/////////////////////////////////////////////////////////////
				//float mixD = mix (sdB,bd,sin(_Time.y)*0.5+0.5);
				//float2 d2 =smin2 (smin2( smin2(cube,sphereB,_SmoothValue),torus,_SmoothValue),torusB,_SmoothValue);
				float2 d2 = gyroid;
				return d2;
			}
			

			float2 RayMarch (float3 ro ,float3 rd)
			{

				float distance = 0; 
				float mapID =0;
				for (int i = 0; i < _MAX_STEPS; ++i) 
				{
					float3 p = ro + rd * distance; 
					float2 d_Id = GetDistance(p);			
					distance += d_Id.x;
					
					if ( abs( d_Id.x) < _SURFACE_DISTANCE || distance >_MAX_DISTANCE ) 
					{	
						mapID = d_Id.y;
						break;
					}
				}
				return float2(distance,mapID);
			}


			float3 GetNormal (float3 p)
			{
				float2 e= float2 (1e-2,0);
				float3 n = GetDistance(p).x- float3 (GetDistance(p-e.xyy).x,GetDistance(p-e.yxy).x,GetDistance(p-e.yyx).x);
				return normalize(n);
			}

			float4 GetColor(float2 primary)
			{
				return tex2D (_ColorRamp,float2(primary.y,0));
			}

			float3 GetLight(float3 p)
			{

				float3 l =_WorldSpaceLightPos0-p;
				float3 n = GetNormal (p);

				float diff = 0.5*saturate (dot(n,l))+0.5;
				

				float d = RayMarch(p+ n*_SURFACE_DISTANCE*5,l).x;

				//Shadow
				if (d < length(l -p)) 
				diff =diff* 0.8;
				return diff;
			}




///////////////////////////////////////////////////////////////////////////
            v2f vert (appdata v)
            {
                v2f o;

				float index = v.vertex.z;
				v.vertex.z = 0.1;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y<0)
				o.uv.y = 1-o.uv.y;
				#endif
			
				o.ray =normalize( _CameraConnerContent[(int)index].xyz);

				o.ray /= abs(o.ray.z);// eyespace coordinates, z < 0
				o.ray =  mul(_MatrixCameraViewToWorld,o.ray);
                return o;
            }
/////////////////////////////////////////////////////////////////////////

            fixed4 frag (v2f i) : SV_Target
            {
				float t = _Time.x;
			     float4 col = tex2D(_MainTex,i.uv);
				 float2 uv = i.uv-0.5;
				 uv +=sin(uv*30+t*10)*0.;//distortion
				//float3 baseColor = float3 (1,1,1);
				float3 rd = normalize(i.ray.xyz);//float3(uv.x,uv.y,1);
				float3 ro = _CameraWPos;
				
				

				float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv).r);
				depth = length(i.ray.xyz)*depth;

				float2  d_ID = RayMarch(ro, rd);
				float2  mapUV = float2(d_ID.y,0);

				if (d_ID.x <_MAX_DISTANCE && d_ID.x <depth)
				{
					
					float3 p = ro +rd*d_ID.x;	
					float3 n = GetNormal(p);
					p =Translate(p);
					float height = p.y;
					float3 diff = GetLight(p);
					float newDiff = n.y*0.5+0.5;
					float4 mapCol = tex2D(_ColorRamp,mapUV);

					float gyroidd2 = sdGyroid(p,45.6,0.01,0.3);

					
					//float4 rmCol =float4(0.5*n+0.5,1);
					float4 rmCol =float4(newDiff,newDiff,newDiff,1)*newDiff*0.75;
					//rmCol *=gyroidd2*5;
					float crackWidth =-0.001+smoothstep(0,-6,n.y)*0.2;
					col = rmCol*smoothstep(-0.04,0.02,gyroidd2);
					float crack = smoothstep(crackWidth,-0.014,gyroidd2);
					
					float gyroidd3 = sdGyroid(p+t*0.6,10,0.01,0.3);
					float gyroidd4 = sdGyroid(p-t*0.3,20,0.01,0.3);
					crack*=gyroidd3*gyroidd4*150+0.2*smoothstep(0.2,0,n.y);
					col +=crack* float4(0.9,0.5,0.1,1)*2;

					//float gyroidd5 = sdGyroid(p-float3(0,t,0),40,0.01,0.5);
					//col +=gyroidd5*float4(0.8,0.5,0.1,1)*10;

					//col = float4 (col*(1-rmCol.w)+rmCol.xyz*rmCol.w ,1.0);//AlphaBlending
				}

				col.rgb =lerp(col,Background(rd),smoothstep(0,1.1,d_ID.x));
				col -=dot(i.uv-0.5,i.uv-0.5)*0.5;


				//col.rgb =Background(rd);
				//col = 0;
				//float gyroid2D = sdGyroid(float3(i.uv.x,i.uv.y,_Time.y*0.1),30,0.01,1);
				//col =abs(gyroid2D)*7;
				return col;

				

            }
            ENDCG
        }
    }
}
