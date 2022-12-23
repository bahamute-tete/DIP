Shader "RayMarch/Hexgon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_a("_a",Range(0,1))=0
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
			float _a;

			float  sdHexagon2D (float2 uv)
			{
				uv = abs(uv);
				/*
				(uv.x uv.y) dot (1,sqrt(3)) = uv.x*1+uv.y*sqrt(3) 几何意义为 基向量i= （1，0）旋转了atan(sqrt(3)/1)的弧度 
				在2维情况下 2个矢量内积 和 利用矩阵进行线性变化是数值是等价的  
				单位向量 u（u.x,u.y）的投影矩阵为[u.x u.y ]  对偶性可得
				*/
				float d = dot (uv, normalize (float2(1,1.73)));//旋转30° [uv.x]
				d = max(d,uv.x);// 四个对称三角星 - UV.X 的矩形 = hexagon 
				return d;
			}

			float4 HexCoord(float2 uv)
			{
/*
frac(x) = mod(x,1)
if we want have diffrence with float2(x,y) ,we should use mod(x,ratio) 

mod() not equal fmod() ==> fmod() >0  
so if we want have the same as  mod()  we should translate some value that like
_ScreenParams
mod(x) =fmod(x+_ScreenParams)  
*/				
				//float2 gva = frac(uv)-0.5;
				//float2 gvb = frac(uv-0.5)-0.5;

				float2 r =float2 (1,1.73);
				float2 h = 0.5*r;
				float2 gva = fmod(uv+_ScreenParams,r)-h;
				float2 gvb = fmod(uv+_ScreenParams-h,r)-h;

				float2 gv = dot(gva,gva)<dot(gvb,gvb)? gva:gvb;
				float2 id = uv-gv;

				float x = atan2(gv.x,gv.y);
				float y =0.5- sdHexagon2D(gv);//distance to edge

				return float4 (x,y,id.x,id.y);
			}

			float2 mod(float2 x,float2 y)
			{
				return x - y*int2(x/y);
			}

			float mod(float x,float y)
			{
				return x - y*int(x/y);
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
				float2 uv = (i.uv-0.5)*_ScreenParams/_ScreenParams.y;
				float hexagon =0;
				

			   // hexagon += sin(sdHexagon2D(uv)*10);
				//hexagon += step(sdHexagon2D(uv),0.2);

				uv *= 10;

				float4 hc = HexCoord(uv);
				
				float c  = smoothstep(0.05,0.08,hc.y*sin(hc.z*hc.w+_Time.y));

				//col.rg = lerp(gva,gvb,_a);
				col =c;
				
				

                return col;
            }
            ENDCG
        }
    }
}
