Shader "RayMarch/Heart"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_blur("_blur",Range(0,0.2))=0
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
			float _blur;

			float sSubtraction(float a ,float b,float k)
			{			
				float h = clamp(0.5-0.5*(b+a)/k,0,1); 
				return lerp(b ,-a, h)+ k*h*(1.0-h);
			}

			float sIntersection(float a ,float b,float k)
			{
				float h = clamp( 0.5 - 0.5*(b-a)/k, 0.0, 1.0 );
				return lerp( b, a, h ) + k*h*(1.0-h);

				 //float h = clamp((b-a)/k+0.5,0,1); 
				 //return lerp(a ,b, h)+ k*h*(1-h)*0.5;
			}

			float sUnion( float a, float b, float k )
			{
				float h = clamp( 0.5+0.5*(b-a)/k, 0.0, 1.0 );
				return lerp( b, a, h ) - k*h*(1.0-h);
			}



            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv-0.5;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float4 col = 0;
				float2 uv =i.uv*_ScreenParams/_ScreenParams.y;
				float blur =_blur;

				uv.x *=0.8;
				uv.y+=0.1;
				uv.y -=sIntersection( sqrt(abs(uv.x))*0.6,blur,0.1);
				
				float r =length(uv);
				float c = smoothstep (0.3,0.299-blur,r);
				col =c;
                return col;
            }
            ENDCG
        }
    }
}
