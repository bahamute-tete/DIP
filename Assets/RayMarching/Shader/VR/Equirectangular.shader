Shader "VR/Equirectangular"
{
    Properties
    {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("PanoromaMap", 2D) = "white" {}
		_LogoTex ("Logo", 2D) = "white" {}
		_Rotation ("Rotation", Range(0, 360)) = 0
		
		_BanlanceR ("BalanceR",Range(0,1)) = 1
		_BanlanceG ("BalanceG",Range(0,1)) = 1
		_BanlanceB ("BalanceB",Range(0,1)) = 1

		_SaturateRange ("Saturate",Range (0,2))=1
		//_ContrastMidPoint  ("MidPoint",range(0,1))=0.5
		_ContrastIntensity("ContrastIntensity",range(0,1))=1

		_MixerColorR ("ChannelMixerR",Range (-2,2))=0
		_MixerColorG ("ChannelMixerG",Range (-2,2))=0
		_MixerColorB ("ChannelMixerB",Range (-2,2))=0

		_BlendIntensity("ChannelMixBlend",Range(0,1))=1

		//_ThresholdValue("_Threshold",Range(0,1))=1
        //_KneeValue ("KneeValue",Range(0,5))=1
		
    }
    SubShader
    {
        Tags {"LightMode" = "Always"}
        LOD 100
		Cull Front

		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma glsl
			#pragma target 3.0

            #include "UnityCG.cginc"
			#define FLT_EPSILON     1.192092896e-07 // Smallest positive number, such that 1.0 + FLT_EPSILON != 1.0
			#define EPSILON         1.0e-4

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float3 objSPVert :TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Rotation;
			float4 _Color;
			float _BanlanceR;
			float _BanlanceG;
			float _BanlanceB;
			float _SaturateRange;
			float _ContrastMidPoint;
			float _ContrastIntensity;
			float _MixerColorR;
			float _MixerColorG;
			float _MixerColorB;
			float _BlendIntensity;
			float _ThresholdValue;
			float _KneeValue;
			float4 _Threshold; // x: threshold value (linear), y: threshold - knee, z: knee * 2, w: 0.25 / knee
			sampler2D _LogoTex;
			float4 _LogoTex_ST;


			 float2 EquirectangularUV(float3 objvert)
			{
				float3 objvertex = normalize(objvert);
				float u = (atan2(objvertex.z, objvertex.x)/UNITY_PI)*0.5;//[0,2PI]
				float v = asin(objvertex.y)/UNITY_PI+0.5;//[-PI/2 ,PI/2 ]
				float2 uv = float2 (u,v)*_MainTex_ST.xy+_MainTex_ST.zw;
				return uv;

			}

			inline float2 ToRadialCoords(float3 coords)
			{
				float3 normalizedCoords = normalize(coords);
				float latitude = acos(normalizedCoords.y);
				float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				float2 sphereCoords = float2(longitude, latitude) * float2(0.5/UNITY_PI, 1.0/UNITY_PI);
				return float2(0.5,1.0) - sphereCoords;
			}

			 float3 RotateAroundYInDegrees (float3 vertex, float degrees)
			{
				float alpha = degrees * UNITY_PI / 180.0;
				float sina, cosa;
				sincos(alpha, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);
				return float3(mul(m, vertex.xz), vertex.y).xzy;
			}


			float3 ColorSpacePositivePow(float3 base, float3 power)
			{
				return pow(max(abs(base), float3(FLT_EPSILON, FLT_EPSILON, FLT_EPSILON)), power);
			}

			half3 ColorSpaceSRGBToLinear(half3 c)
			{
				#if USE_VERY_FAST_SRGB
				return c * c;
				#elif USE_FAST_SRGB
				return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
				#else
				half3 linearRGBLo = c / 12.92;
				half3 linearRGBHi = ColorSpacePositivePow((c + 0.055) / 1.055, half3(2.4, 2.4, 2.4));
				half3 linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
				return linearRGB;
				#endif
			}

			half4 ColorSpaceSRGBToLinear(half4 c)
			{
				return half4(ColorSpaceSRGBToLinear(c.rgb), c.a);
			}

			half3 ColorSpaceLinearToSRGB(half3 c)
			{
				#if USE_VERY_FAST_SRGB
				return sqrt(c);
				#elif USE_FAST_SRGB
				return max(1.055 * PositivePow(c, 0.416666667) - 0.055, 0.0);
				#else
				half3 sRGBLo = c * 12.92;
				half3 sRGBHi = (ColorSpacePositivePow(c, half3(1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
				half3 sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
				return sRGB;
				#endif
			}

			half4 ColorSpaceLinearToSRGB(half4 c)
			{
				return half4(ColorSpaceLinearToSRGB(c.rgb), c.a);
			}

			half ColorLuminance(half3 linearRgb)
			{
				return dot(linearRgb, float3(0.2126729, 0.7151522, 0.0721750));
			}

			half ColorLuminance(half4 linearRgba)
			{
				return Luminance(linearRgba.rgb);
			}

			float rcp(float value)
			{
				return 1.0 / value;
			}

			float Max3(float a, float b, float c)
			{
			return max(max(a, b), c);
			}

			float2 Max3(float2 a, float2 b, float2 c)
			{
			return max(max(a, b), c);
			}

			float3 Max3(float3 a, float3 b, float3 c)
			{
			return max(max(a, b), c);
			}

			float4 Max3(float4 a, float4 b, float4 c)
			{
			return max(max(a, b), c);
			}

			float3 ColorFastTonemap(float3 c)
			{
				return c * rcp(Max3(c.r, c.g, c.b) + 1.0);
			}

			float4 ColorFastTonemap(float4 c)
			{
				return float4(ColorFastTonemap(c.rgb), c.a);
			}

			float3 ColorNeutralCurve(float3 x, float a, float b, float c, float d, float e, float f)
			{
			return ((x * (a * x + c * b) + d * e) / (x * (a * x + b) + d * f)) - e / f;
			}

			float3 ColorNeutralTonemap(float3 x)
			{
			// Tonemap
			float a = 0.2;
			float b = 0.29;
			float c = 0.24;
			float d = 0.272;
			float e = 0.02;
			float f = 0.3;
			float whiteLevel = 5.3;
			float whiteClip = 1.0;

			float3 whiteScale = (1.0).xxx / ColorNeutralCurve(whiteLevel, a, b, c, d, e, f);
			x = ColorNeutralCurve(x * whiteScale, a, b, c, d, e, f);
			x *= whiteScale;

			// Post-curve white point adjustment
			x /= whiteClip.xxx;

			return x;
			}


			static const float3x3 Color_LIN_2_LMS_MAT = {
			3.90405e-1, 5.49941e-1, 8.92632e-3,
			7.08416e-2, 9.63172e-1, 1.35775e-3,
			2.31082e-2, 1.28021e-1, 9.36245e-1
			};

			static const float3x3 Color_LMS_2_LIN_MAT = {
			2.85847e+0, -1.62879e+0, -2.48910e-2,
			-2.10182e-1,  1.15820e+0,  3.24281e-4,
			-4.18120e-2, -1.18169e-1,  1.06867e+0
			};

			float3 ColorWhiteBalance(float3 c, float3 balance)
			{
			float3 lms = mul(Color_LIN_2_LMS_MAT, c);
			lms *= balance;
			return mul(Color_LMS_2_LIN_MAT, lms);
			}


			float3 ColorSaturation(float3 c, float sat)
			{
				float luma = ColorLuminance(c);
				return luma.xxx + sat.xxx * (c - luma.xxx);
			}


			float3 ColorContrast(float3 c, float midpoint, float contrast)
			{
				return (c - midpoint) * contrast + midpoint;
			}

			float3 ColorChannelMixer(float3 c, float3 red, float3 green, float3 blue)
			{
				return float3(
					dot(c, red),
					dot(c, green),
					dot(c, blue)
					);
			}

			half4 ColorQuadraticThreshold(half4 color, half threshold, half3 curve)
			{
				// Pixel brightness
				half br = Max3(color.r, color.g, color.b);

				// Under-threshold part: quadratic curve
				half rq = clamp(br - curve.x, 0.0, curve.y);
				rq = curve.z * rq * rq;

				// Combine and apply the brightness response curve.
				color *= max(rq, br - threshold) / max(br, EPSILON);

				return color;
			}

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;

				float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
                o.vertex = UnityObjectToClipPos(rotated);
				o.objSPVert = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float2 finalUV = EquirectangularUV(i.objSPVert);


				float4 col = tex2D (_MainTex, finalUV)*_Color;
				float4 logoCol = tex2D (_LogoTex, finalUV*_LogoTex_ST.xy+_LogoTex_ST.zw)*_Color;
							

				float3  banlacne =float3 (_BanlanceR,_BanlanceG,_BanlanceB);
				col.xyz=ColorWhiteBalance(col.xyz,banlacne);
				

				col.xyz= ColorContrast(col.xyz,_ContrastMidPoint,_ContrastIntensity);
				col.xyz= ColorSaturation(col.xyz,_SaturateRange);
				float3 mixColor= ColorChannelMixer(col.xyz,_MixerColorR,_MixerColorG,_MixerColorB);
				col.xyz = lerp (col.xyz,mixColor,_BlendIntensity);
				
				
				col = lerp(col,logoCol,logoCol.a);
				//col.xyz =ColorNeutralTonemap(col.xyz);
				
                return  col;
            }
            ENDCG
        }
    }
}
