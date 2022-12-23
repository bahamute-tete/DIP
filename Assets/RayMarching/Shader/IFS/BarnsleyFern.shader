Shader "RayMarch/Barnsley Fern"
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
			#define MOD3 float3(.1031,.11369,.13787)

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


			float hash(  float n )
			{
				return frac(sin(n)*43758.5453123);
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

				float t = 0.1*_Time.y - 1.0;
				#if 1
				float2x2 am = float2x2( cos(t*1.71+0.18), -cos(t*1.31+3.18), cos(t*1.31+3.18), cos(t*1.44+4.21) );
				float2x2 bm = float2x2( cos(t*2.57+1.66), -cos(t*1.08+0.74), cos(t*1.08+0.74), cos(t*1.23+1.29) );
				float2x2 cm = float2x2( cos(t*1.15+6.33), -cos(t*2.94+2.92), cos(t*2.94+2.92), cos(t*2.58+2.36) );
				float2x2 dm = float2x2( cos(t*1.42+4.89), -cos(t*2.73+6.34), cos(t*2.73+6.34), cos(t*1.21+3.84) );
				float2 at = float2( cos(t*2.13+0.94), cos(t*1.19+0.29) )*0.8;
				float2 bt = float2( cos(t*1.09+5.25), cos(t*1.27+1.77) )*0.8;
				float2 ct = float2( cos(t*2.76+4.39), cos(t*2.35+2.04) )*0.8;
				float2 dt = float2( cos(t*1.42+4.71), cos(t*2.81+3.51) )*0.8;
				#else 
				float2x2 am = float2x2( cos(t*1.71+0.18), cos(t*1.31+3.18), cos(t*2.13+3.29), cos(t*1.44+4.21) );
				float2x2 bm = float2x2( cos(t*2.57+1.66), cos(t*1.08+0.74), cos(t*1.25+2.78), cos(t*2.23+1.29) );
				float2x2 cm = float2x2( cos(t*1.15+6.33), cos(t*2.94+2.92), cos(t*1.78+0.82), cos(t*2.58+2.36) );
				float2x2 dm = float2x2( cos(t*1.42+4.89), cos(t*2.73+6.34), cos(t*1.67+5.91), cos(t*1.21+3.84) );
				float2 at = float2( cos(t*2.13+0.94), cos(t*1.19+0.29) )*0.8;
				float2 bt = float2( cos(t*1.09+5.25), cos(t*1.27+1.77) )*0.8;
				float2 ct = float2( cos(t*2.76+4.39), cos(t*2.35+2.04) )*0.8;
				float2 dt = float2( cos(t*1.42+4.71), cos(t*2.81+3.51) )*0.8;
				#endif

                float4 col = 0;
				float3 cola=0;
				float3 colb=0;
				float2 uv = 2*i.uv-1;
				uv =uv*_ScreenParams/_ScreenParams.y*3-float2(0,0);
				float pi = 3.14159;

				

				float2 z = 0;
				float p = hash(dot(uv,float2(156.0,569.0))+_Time.x);

				float  it = p ;
				float cad= 0;
				for(int  j =  0  ; j <512 ; j ++)
				{
					p= frac( p*8.1);


					#if 0
					if (p<0.01)
					{
						z = float2(0.0,0.16*z.y);
					}else if(p<0.84)
					{
						z = mul(float2x2(0.85,-0.04,0.04,0.85),z)+float2(0,1.6);
					}else if(p<0.93)
					{
						z = mul(float2x2(0.2,-0.26,0.23,0.22),z)+float2(0,1.6);
					}else
					{
						z= mul(float2x2(-0.15,0.28,0.26,0.24),z)+float2(0,0.44);
					}
					#else

					
					cad *= 0.25;
					if (p<0.25)
					{
						z = mul(am,z)+at;
						cad+=0;

					}else if(p<0.5)
					{
						z = mul(bm,z)+bt;
						cad += 0.25;
					}else if(p<0.75)
					{
						z = mul(cm,z)+ct;
						cad += 0.75;
					}else
					{
						z= mul(dm,z)+dt;
						cad += 1;
					}

					float zr = length(z);
					float ar = atan2( z.y, z.x )+ zr*0.5;
					z = 2.0*float2( cos(ar), sin(ar))/zr;


					if( j>10 )
					{
					float3  coh = 0.5 + 0.5*sin(2.*cad + float3(0.0,1.2,2.0));
					float cok = dot(uv-z,uv-z);
					cola = lerp( cola, coh, exp( -512.0*cok ) );
					colb = lerp( colb, coh, exp(  -48.0*cok ) );
					}
					#endif

					//col = max( col, exp( -100.0*dot(uv-z,uv-z) ) );
					 col = float4(cola,1) + 0.5*float4(colb,1);
				}

				//col *= float4(0.1,0.756,0.23,1);

                return col;
            }
            ENDCG
        }
    }
}
