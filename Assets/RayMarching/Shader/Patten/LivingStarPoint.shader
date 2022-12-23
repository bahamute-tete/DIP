Shader "RayMarch/LivingStarPoint"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MousePos("_MousePos",Vector) = (0,0,0,0)
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
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_nicest 
			

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
			float4 _MousePos;
////////////////////////////////////////////////////
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

			float2x2 M_rotation (float t)
			{
				float s = sin(t);
				float c = cos(t);
				return float2x2 (s,-c,c,s);
			}

////////////////////////////////////////////////////
			float DistLine(float2 uv ,float2 a ,float2 b)
			{
				float2 pa = uv -a ;
				float2 ba =  b-a;

				float t  = clamp (dot(pa,ba)/(dot(ba,ba)+1e-10),0,1 ) ;
				float d = length (pa-ba*t);
				return d;
			}

			float N21 (float2 p)
			{
				p= frac( p * float2(123.33,854.32));
				p+= dot(p,p+23.56);
				return frac(p.x*p.y);
			}

			float2 N22(float2 p)
			{
				float n = N21(p);
				return float2 (n,N21(p+n));
			}

			float2 GetPos (float2 id,float2 offs)
			{
				//float2 p = N22(id);
				//float x= sin(_Time.y*p.x);
				//float y = cos(_Time.y* p.y);
				//return float2(x,y)*0.4;
				float2 p = N22(id+offs)*_Time.y;
				return sin(p)*0.4+offs;
			}

			float DrawLine (float2 p ,float2 a ,float2 b)
			{
				float d = DistLine(p,a,b);
				float m =smoothstep(.03,.01,d);
				float d2= length(a-b);
				m *= smoothstep(1.2,0.8,d2)*0.5+smoothstep(0.1,0.09,abs(d2-0.75));
				return m;
			}

			float LineLayer(float2 uv)
			{

				float m =0;
				float2 guv= frac(uv)-0.5;
				float2 gid = floor(uv);

				//float2 p = GetPos(gid,float2(0,0));
				//float d = length(guv-p);
				//m = S(0.1,0.05,d);
				float2 p[9];

				p[0] = GetPos(gid,float2(-1,1));
				p[1] = GetPos(gid,float2(0,1));
				p[2] = GetPos(gid,float2(1,1));
				p[3] = GetPos(gid,float2(-1,0));
				p[4] = GetPos(gid,float2(0,0));
				p[5] = GetPos(gid,float2(1,0));
				p[6] = GetPos(gid,float2(-1,-1));
				p[7] = GetPos(gid,float2(0,-1));
				p[8] = GetPos(gid,float2(1,-1));


				float t  = _Time.y*10;
				for (int l = 0 ;l < 9 ; l++)
				{
					m += DrawLine(guv,p[4],p[l]);
					float d =  length(guv - p[l])*20;
					float spark = 1/length(dot(d,d));
					m += spark*(sin(t+frac(p[l].x)*10)*0.5+0.5);
				}
				m += DrawLine(guv,p[1],p[3]);
				m += DrawLine(guv,p[1],p[5]);
				m += DrawLine(guv,p[5],p[7]);
				m += DrawLine(guv,p[7],p[3]);

				return m;
			}

/////////////////////////////////////////////////////
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv - 0.5;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float4 col = 0;

				float2 uv = i.uv * _ScreenParams/_ScreenParams.y;
				float2 mousePos = _MousePos.xy-0.5;
                

				uv*= 9.9;
				float m = 0;
				float grandian = uv.y*.1;
				float t = _Time.y*0.1;

				uv = mul(M_rotation(t),uv);
				mousePos.xy = mul(M_rotation(t),mousePos.xy);

				for (float l =0 ; l<1.0 ;l+=1.0/4.0)
				{
					float depth = frac(l+t);
					float layerSize = mix (1.5,0.2,depth);
					float fade = smoothstep(0,0.5,depth)*smoothstep(1.0,0.5,depth);
					m +=LineLayer(uv*layerSize+l*20-mousePos.xy)*fade;
				}

				float3 base = sin(t*5*float3(0.234,0.564,0.483))*0.4+0.6;

				//if (guv.x>0.48 || guv.y >0.48) col =float4(1,0,0,1);
				col.rgb = m*base;
				col.rgb -=grandian*base;
                return col;
            }
            ENDCG
        }
    }
}
