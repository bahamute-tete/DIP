
Shader "RayMarch/PolarCoord"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_GroundDirection("GroundDirection",Vector)=(0,1,0,1)
		_LightPos ("LightPosition",Vector) = (1,4,1,0)
		_CameraPos ("_CameraPos",Vector) = (0,7,-8,1)
		_LookAtPos ("_LookAtPos",Vector) = (0,0,0,0)
		_ShadowIntensity("ShadowIntensity",range(0,1))=0.75
		_boxTwist ("_boxTwist",Range(0,360))=0
		_boxRotation ("_boxRotation",Range(0,360))=0
		_boxThickness("_boxThickness",float)= 0.1
		_smooth("_smooth",Range(0,1))=0
		_FlowerNumber ("_FlowerNumber",int ) =3
		//[KeywordEnum(Image2D,Image3D)] _RayMarchMode ("_RayMarchMode",float) = 0
		[Toggle] _RayMarch ("_RayMarch",float)=0
		_a("_a",Range(0,20))=0
		_b("_b",Range(0,10))=0
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
			//#pragma multi_compile _RAYMARCHEMODE_IMAGE2D _RAYMARCHEMODE_IMAGE3D 
			#pragma multi_compile _RAYMARCH_ON


            #include "UnityCG.cginc"
			#include "AutoLight.cginc"

			#define MAX_STEPS 100
			#define MAX_DISTACNE 100.0
			#define SURFANCE_DIS 0.01
			#define PI  3.1415926			
			

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
			float4 _LightPos;
			float4 _CameraPos;
			float4 _LookAtPos;
			float _ShadowIntensity;
			float4 _GroundDirection;
			float _boxRotation;
			float _boxTwist;
			float _boxThickness;
			float _smooth,_a,_b;
			float _FlowerNumber;
////////////////////////////////////////BaseFun
float2x2 Rotation(float degrees)
{
    float a = degrees * UNITY_PI / 180.0;
	float s = sin(a);
	float c = cos(a);
	float2x2 rotationMat =  float2x2 (c,-s,s,c);
	return rotationMat;
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

float opSmoothSubtraction( float d1, float d2, float k ) 
{
    float h = clamp( 0.5 - 0.5*(d2+d1)/k, 0.0, 1.0 );
    return mix( d2, -d1, h ) + k*h*(1.0-h); 
}

float opSmoothIntersection( float d1, float d2, float k ) 
{
	float h = clamp( 0.5 - 0.5*(d2-d1)/k, 0.0, 1.0 );
	return mix( d2, d1, h ) + k*h*(1.0-h); 
}

float displacement(float3 p )
{
	return 2*sin(5*p.x)*sin(5*p.y)*sin(5*p.z);
}

float opDisplace( float primitive, float3 p )
{
    float d1 = primitive;
    float d2 = displacement(p);
    return d1+d2;
}

/////////////////////////////////////////////////////////////////////////SDF
float sdSphere (float3 p,float r)
{
	float sphere = length(p)-r;
	return sphere;
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

float sdTorus (float3 p ,float2 r)
{
	float vec= length(p.xz)-r.x;
	float torus = length(float2(vec,p.y))-r.y;
	return torus;
}

float sdBox (float3 p,float3 s)
{
	float3  q = abs (p)-s ;	
	float i =min(max(q.x,max(q.y,q.z)),0.0);
	float box= length (max(q,0))+i;
	return box;
}

float sdCylinder (float3 p ,float3 a,float3 b,float r)
{
	float3 ap = p-a;
	float3 ab= b-a;

	float t = dot(ab,ap)/dot(ab,ab);
	//t = clamp(t,0,1);
	float3 c = a +t*ab;

	float x = length(p-c)-r;
	float y = abs(t-0.5)-0.5*length(ab);
	float i  = min(max(x,y),0);
	float cylinder = length(max(float2(x,y),0))+i;
	return cylinder;
}


float sdCappedCylinder( float3 p, float h, float r )
{
  float2 d = abs(float2(length(p.xz),p.y)) - float2(h,r);
  return min(max(d.x,d.y),0.0) + length(max(d,0.0));
}



///////////////////////////////////////////////////////////////////////RayMarch
			float GetDis( float3 p)
			{

				float plane = dot(p,normalize(_GroundDirection.xyz));//Ground = p dot N
				//////////////////

				float4 sphereData = float4 (0,0,3,2);
				float3 spherePos = p-sphereData.xyz;
				sphereData.w +=0.5*sin(sphereData.w+_Time.y*10.0);
				//spherePos.yz = mul(Rotation(_boxRotation),spherePos.yz);
				float sphere =  sdSphere(spherePos,sphereData.w)*20;
			
				sphere =opDisplace(sphere,p)/20;
				//////////////////
				float3 capsuleDataA =float3 (0,1,1);
				float3 capsuleDataB = float3 (1,1,1);
				float3 capsuleCenter = float3(0,1,2);
				float radius = 0.2;
				float capsule = sdCapusle(p-capsuleCenter,capsuleDataA,capsuleDataB,radius);
				//////////////////

				float2 torusRadius = float2(1.5,1);
				float3 torusCenter= float3 (0,0.5,3);
				float3 torusPos= p-torusCenter;
				torusPos.yz = mul(Rotation(_Time.y*2),torusPos.yz);
				torusPos.xy = mul(Rotation(_Time.y*2),torusPos.xy);
				float torus = sdTorus(torusPos,torusRadius);
				///////////////////
				float3 boxSize = float3 (1,1,1);
				float3 boxCenter = float3 (0,2,4);
				float3 boxPos = p - boxCenter;
				float scale = mix (2,3,smoothstep(-1,1,boxPos.y));
				boxPos = abs (boxPos);//mirror
				boxPos -=1.5;
				float3 n = normalize (_GroundDirection.xyz);
				//boxPos -= 1.5*n*min(0,dot(p,n));
				boxPos.xz *=scale;
				boxPos.xz = mul(Rotation(smoothstep(-1,1,boxPos.y)*_boxTwist),boxPos.xz);//twsit
				boxPos.xz = mul(Rotation(_boxRotation+_Time.y*20),boxPos.xz);
				//boxPos.z += sin(boxPos.y*5+_Time.y*5)*0.1;//flagWave
				
				float box = sdBox(boxPos,boxSize)/scale-0.1;
				//box = opDisplace(box,p);
				//box -=sin(p.y*7+_Time.y*2)*0.05;//displayMapping
				//box= abs(box)-_boxThickness;//shell
				///////////////////
				float3 cylinderDataA =float3 (0,0,1);
				float3 cylinderDataB = float3 (0.7,0.5,1.5);
				float3 cylinderCenter = float3(0,1,2);
				float3 cylinderPos = p-cylinderCenter;
				float cylinderRadius = 0.2;
				float cylinder = sdCylinder(cylinderPos,cylinderDataA,cylinderDataB,cylinderRadius);
				//////////////////
				float3 cappedCylinderCenter = float3(0,1,2);
				float3 cappedCylinderPos = p-cappedCylinderCenter;
				float h =0.5;
				float r =0.1;
				float cappedCylinder = sdCappedCylinder(cappedCylinderPos,h,r);

				float dcy= min(cylinder,box);
				float dcap = min (dcy,capsule);
				float d= smin (box,plane,_smooth);
				
				//float d= mix(torus,sphere,sin(_Time.y)*0.5+0.5);
				 d= min (d,plane);

				return d;
			}

			float RayMarch (float3 ro,float3 rd)
			{
				float depthDistance = 0;

				for (int  i = 0  ; i <MAX_STEPS ;i++)
				{
					float3 p  = ro+rd*depthDistance;
					float ds = GetDis (p);
					depthDistance+=ds;

					if (depthDistance >MAX_DISTACNE || abs(ds)<SURFANCE_DIS )
					break;
				}
				return depthDistance;
			}

			float3 GetNormal(float3 p)
			{
				float d = GetDis(p);
				float2 e = float2 (0.01,0);

				float3 n = d- float3(
										GetDis(p-e.xyy),
										GetDis(p-e.yxy),
										GetDis(p-e.yyx));
				return normalize(n);
			}

			float GetLight (float3 p)
			{
			 float3 lightPos = _LightPos.xyz;

			 //lightPos.xz += float2(sin(_Time.y),cos(_Time.y));
			 float3 l = normalize(lightPos - p);
			

			 float3 n = GetNormal (p);
			
			 float d = RayMarch(p+n*SURFANCE_DIS*2,l);


			 float dif  =  saturate( dot(n,l));
		
			 if (d < length(lightPos -p )) 
			 dif =dif*(1-_ShadowIntensity);
			 return  dif;
			}

			float3 getCamera(float3 cameraPos,float3 targetPos,float zoom,float2 screenUV)
			{	
				float3 forward =normalize(targetPos-cameraPos);
				float3 right = cross(forward,float3(0,1,0));
				float3 up = cross(right,forward);
				float3 centerScreen = cameraPos+zoom*forward;
				float3 intersectionPoint = centerScreen + screenUV.x*right+screenUV.y*up;
				float3 pos = intersectionPoint -cameraPos;
				return pos;
			}

/////////////////////////////////////////////////////////////////VF

            v2f vert (appdata v)
            {
                v2f o = (v2f) 0;
                o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv =v.uv -0.5;
				o.uv = o.uv * _ScreenParams.xy/_ScreenParams.y;

				
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {


                fixed4 col =float4 (0,0,0,1);

//////////////////////////////////////////////////////// RayMarchShape
				float3 ro = _CameraPos;
				float3 _CameraZoom = _CameraPos.w;
				//float3 rd = normalize (float3 (i.uv.x,i.uv.y-0.5,1));
				float3 rd = getCamera(_CameraPos,_LookAtPos,_CameraZoom,i.uv);
				
				float d = RayMarch(ro,rd);

				float3 p  = ro+rd*d;
				float AmbientLight = 0.07;
				float diff = GetLight(p)+AmbientLight;

				if (d>MAX_DISTACNE)
				{
					col = 0;
				}
				else
				{
					col.rgb = diff;
				}

//////////////////////////////////////////////////////////polarCoord
				float ouvx = i.uv.x *4;
				float ox =min (frac(ouvx),frac(1.0-ouvx));
				float mox = smoothstep(0,0.2,ox*0.3+0.2-i.uv.y);


				float fuvx =sin(1-i.uv.x*i.uv.x*10);
				float absx =frac(abs(i.uv.x)*1);
				float nm = lerp(fuvx,absx,1);
				nm= smoothstep(0.05,0.1,nm*0.5-i.uv.y);

				

				float2 polarCoord = float2 (atan2(i.uv.x,i.uv.y),length(i.uv));
				//float2 polarCoord = float2 (atan(i.uv.y/i.uv.x),length(i.uv));
				polarCoord = float2 (polarCoord.x/PI*0.5+0.5,polarCoord.y);

				float x = polarCoord.x*_FlowerNumber;
				float m = min (frac(x),frac(1.0-x));
				float c = smoothstep(0,0.1,.5*m-2*polarCoord.y+0.4);

				//float fuvxp =sin(1-polarCoord.x*polarCoord.x*15);
				//float absxp =frac(abs(polarCoord.x)*30);
				//float nmp = lerp(fuvxp,absxp,1);
				//nmp= smoothstep(0.08,0.1,nmp*0.5-polarCoord.y);

				float fuvxp =sin(1-polarCoord.x*polarCoord.x*15);
				float absxp =frac(abs(polarCoord.x)*10);
				//float nmp =(polarCoord.x)*_a+_b;
				//float nmp =sqrt(polarCoord.x);
				float nmp = _a*(1-sin(polarCoord.x));
				nmp=smoothstep(0.07,0.1,nmp*0.1-polarCoord.y);

				col = c;
/////////////////////////////////////////////////////////
				
				//col.rgb =ox;
				


                return col;
            }
            ENDCG
        }
    }
}
