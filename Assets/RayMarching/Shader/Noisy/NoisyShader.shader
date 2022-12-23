Shader "RayMarch/NosiyShader"
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

			float mix(float a,float b,float c)
			{
				return a*(1-c)+b*c;
			}

			float2 mix(float2 a,float2 b,float c)
			{
				return a*(1-c)+b*c;
			}

			float3 mix(float3 a,float3 b,float c)
			{
				return a*(1-c)+b*c;
			}

			float4 mix(float4 a,float4 b,float c)
			{
				return a*(1-c)+b*c;
			}

			float Noisy2D(float2 uv)
			{
				//float c = frac(sin(uv.x*100+uv.y*6875)*5647.0);
				float n= frac(sin(dot(uv,float2(100,6875)))*5674.0);
				return n;
			}

			float2 NosiyVector2(float2 uv)
			{
				float3 a = frac(uv.xyx * float3 (123.34,234.34,345.65));
				a += dot (a,a+34.45);
				return frac(float2 (a.x *a.y ,a.y *a.z));
			}

			float SmoothNoisy2D (float2 uv)
			{
				float2 lv = frac(uv);
				lv =lv*lv*(3-2*lv);
				float2 id = floor(uv);

				float bottomleftPoint = Noisy2D(id);
				float bottomRightPoint = Noisy2D(id+float2(1,0));
				float b =mix(bottomleftPoint,bottomRightPoint,lv.x);


				float topleftPoint = Noisy2D(id+float2(0,1));
				float topRightPoint = Noisy2D(id+float2(1,1));
				float t =mix(topleftPoint,topRightPoint,lv.x);

				float c = mix(b,t,lv.y);
				return c;
			}

			float ValueNoisy(float2 uv)
			{
				float c = SmoothNoisy2D(uv*4);
				c += SmoothNoisy2D(uv*8)*0.5;
				c += SmoothNoisy2D(uv*16)*0.25;
				c += SmoothNoisy2D(uv*32)*0.125;
				c += SmoothNoisy2D(uv*64)*0.0625;

				return c/2;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				//o.uv = o.uv*_ScreenParams/_ScreenParams.y;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float4 col = 0;
				float2 uv = 2*i.uv -1;
				uv = uv*_ScreenParams/_ScreenParams.y;
//////////////////////////////////////////////////////////////
				//float c = Noisy2D(uv);
				uv+=_Time.x*5;
				float c = ValueNoisy(uv);
				float m = NosiyVector2(uv).x;
				col+=c;
////////////////////////////////////////
				//float m =0;
				//float mindis = 100;
				//float cellIndex =0 ;
				//if (false)
				//{
				//	for (int  i  =  0  ; i < 50 ;i ++)
				//{
				//	float2 n= NosiyVector2(float2 (i , i));
				//	float2 p = sin (n *_Time.y*0.2);

				//	float d = length (uv - p);
				//    m +=  smoothstep(0.02,0.015,d);

				//	if (d < mindis)
				//	{
				//		mindis =d;
				//		cellIndex = i;
				//	}
				//}
				//}else
				//{
				//	uv *= 5;
				//	float2 gridUV = frac(uv)-.5;
				//	float2 gridID = floor(uv);
				//	float2 cellID = 0;

				//	for (float y=-1;y<=1;y++)
				//	{
				//		for (float x=-1;x<=1 ;x++)
				//		{
				//			float2 offsetUV = float2(x,y);
				//			float2 n = NosiyVector2(offsetUV+gridID);
				//			float2 p = 0.5*sin( n * _Time.y *.5)+offsetUV;
				//			 p -=gridUV;
				//			float od = length(p);
				//			float md= abs(p.x)+abs(p.y);//Manhattan distance
				//			float d = mix(od,md,sin(_Time.y*0.5)+0.3);
							

				//			if (d< mindis)
				//			{
				//				mindis = d ;
				//				cellID = offsetUV +gridID;
				//			}
				//		}
				//	}
				//	col = mindis;
				//	col.rg = cellID*0.1;
				//}
				


				//col = cellIndex/50;
				//col.rg = gridUV;
				
                return col;
            }
            ENDCG
        }
    }
}
