Shader "Custom/Glass"
{
	Properties
	{   _Color ("Color",Color)=(1,1,1,1)
		_rimColor ("RimColor",Color)=(1,1,1,1)
		_rimPower ("RimPower",Range(0,36))=0.1
		_rimStrenth("RimStrenth",Range(0,10))=1.0
		_cubeMap("CubeMap",Cube) ="_Skybox"{}
		_NormalMap1("Normal",2D)="bump"{}
		_NormalMap2("Normal",2D)="bump"{}
		_b ("BlendNor",Range(0,1))=0.5
		_refractionAmount("_refractionAmount",Range(0,1))=0.5
		_Distorition("Distorition",Range(0,100))=10
		_refractionColor("ReflectionColor",Color)=(1,1,1,1)

		_LineColor ("LineColor",Color)= (1,0,0,0)
		_LineWidth ("LineWidth",Range(0,0.01))=0.001

	
	}
	SubShader
	{
		//Tags { "RenderType"="Opaque" }
		Tags { "Queue"="Transparent" "IgnorProjector"="True" "RenderType"="Opaque" "DisableBatching"="True" }
		LOD 300

		GrabPass {"_RefractionTex"}

		
		Pass
		{

			ZWrite on
			Cull Off

			Blend  SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent:TANGENT; 
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;

				float4 vertex : SV_POSITION;
				float3 worldNormal :TEXCOORD1;
				float4 worldPos : TEXCOORD2;
				float3 worldViewDir :TEXCOORD3;
				float3 worldRefracion:TEXCOORD4;
				float4 scrPos :TEXCOORD6;
				float3 TtoWorld0 : TEXCOORD7;
				float3 TtoWorld1 : TEXCOORD8;
				float3 TtoWorld2 : TEXCOORD9;
				SHADOW_COORDS(5)
			};

			fixed4 _Color;
			float4  _rimColor;
			float _rimPower;
			float _rimStrenth;
			samplerCUBE  _cubeMap;
			float  _refractionAmount;
			float4 _refractionColor;
			float  _refractionRatio;

			float _FresnelScale;
			float  _Distorition;

            sampler2D _NormalMap1;
			float4 _NormalMap1_ST;

			sampler2D _NormalMap2;
			float4 _NormalMap2_ST;

			float _b;

			sampler2D _RefractionTex;
			float4 _RefractionTex_TexelSize;




		

			
			v2f vert (appdata v)
			{
				v2f o;


				UNITY_INITIALIZE_OUTPUT(v2f, o);
				

				o.worldNormal =UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				o.worldViewDir = UnityWorldSpaceViewDir(o.worldPos);
				//o.worldRefracion = refract (-o.worldViewDir,o.worldNormal,_refractionRatio);

				float3 worldTangent =UnityObjectToWorldDir(v.tangent.xyz);
				float3 worldBinormal =cross (o.worldNormal,worldTangent)*v.tangent.w; 


				o.TtoWorld0 =float3(worldTangent.x,worldBinormal.x,o.worldNormal.x);
				o.TtoWorld1 =float3(worldTangent.y,worldBinormal.y,o.worldNormal.y);
				o.TtoWorld2 =float3(worldTangent.z,worldBinormal.z,o.worldNormal.z);
                

				o.uv = TRANSFORM_TEX(v.uv, _NormalMap1);


				o.vertex = UnityObjectToClipPos(v.vertex);
				o.scrPos = ComputeGrabScreenPos(o.vertex);

				TRANSFER_SHADOW (o);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{			

				  float3 worldNormal =normalize(i.worldNormal);

				 float3 normalMap1=UnpackNormal(tex2D(_NormalMap1,i.uv+float2(_Time.x*0.1,_Time.x*0.2)));
				  float3 normalMap2=UnpackNormal(tex2D(_NormalMap2,i.uv+float2(_Time.x*0.2,_Time.x*0.1)));

				 float3 normalMap=normalize(lerp(normalMap1,normalMap2,_b));
				 //float3 normalMap=normalize(normalMap1);

				  float2 offset =normalMap.xy*_Distorition*_RefractionTex_TexelSize.xy;
				  i.scrPos.xy +=offset;
				  float3 refractionColor =tex2D(_RefractionTex,i.scrPos.xy/i.scrPos.w).rgb;

				 normalMap= normalize(half3(dot(i.TtoWorld0,normalMap),dot(i.TtoWorld1,normalMap),dot(i.TtoWorld2,normalMap)));


				 //float3 diffuse =_LightColor0.rgb*_Color.rgb*(dot(worldNormal,lightPos));
				 fixed3 reflectionDir =reflect(normalize(-i.worldViewDir),normalMap);
				 float3 reflectionColor =texCUBE( _cubeMap,reflectionDir ).rgb*_Color.rgb;

				 fixed fresnel=_FresnelScale+(1-_FresnelScale)*pow(1-dot(i.worldViewDir,worldNormal),5);

				 float3 rim =pow((1.0-saturate(dot(normalMap,normalize(i.worldViewDir)))),_rimPower)*_rimColor.rgb*_rimStrenth;
				  //float3 rim =(1.0-saturate(dot(normalMap,normalize(i.worldViewDir))))*_rimColor.rgb*_rimStrenth;

				//UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos.xyz);
				//UNITY_APPLY_FOG(i.fogCoord, col);

				//float3 finalColor = lerp(reflectionColor,refractionColor,saturate(fresnel))+rim;
				float3 finalColor = lerp(reflectionColor,refractionColor,_refractionAmount)+rim;
				

				return fixed4 (finalColor,1.0);

			}
			ENDCG
		}
	}
}
