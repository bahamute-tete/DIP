Shader "RayMarch/KoheCurve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_MapTex ("MapTex",2D) = "white"{}
		_KochPattenShape("_KochPattenShape",range(0,1))=0.1

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
			sampler2D _MapTex;
			float4 _MapTex_ST;
			float4 _MousePos;
			float _KochPattenShape;
//////////////////////////////////////////////////////////////////
				//float  d = dot(uv,n);// uv.x*n.x+uv.y*n.y =====>>>>uv.x Move to n.x direction the same to uv.y,finally we can get a new vector forme n direction
				//uv -= n*min(0,d)*2;//make UV flip with the Line====>>>>>multi 2 means move 2times distance,so that goto the next side of the line
				//col.rg +=sin(uv*10);
				//col+= smoothstep(0.01,0,abs(d));
//////////////////////////////////////////////////////////////////
			float2 N(float angle)
			{
				return float2 (sin(angle),cos(angle));
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
				uv*=1.25;

				uv.x= abs(uv.x);


				float rad =0.5/0.6;
				uv.y += tan(rad*UNITY_PI)*0.5;//Move to Center

				float angle = rad * UNITY_PI;
				float2 n =N(angle);
				float d=dot(uv-float2(0.5,0),n);
				uv -= n*max(0,d)*2;
				col += smoothstep(0.01,0,abs(d));



				n =N(_KochPattenShape*0.66*UNITY_PI);
				uv.x += 0.5;
				float scale =1;
				for(int num =0 ;num<10 ;num ++)
				{
					uv*=3;
					uv.x-= 1.5;
					scale*=3;
					uv.x = abs(uv.x);
					uv.x -=0.5;
					uv -= n*min(0,dot(uv,n))*2;
				}

				d = length(uv - float2(clamp(uv.x,-1,1),0));
				d = smoothstep(3/_ScreenParams.y,0,d/scale);



				col =d;
				//col.rg+=uv/scale;
				uv/=scale;
				col += tex2D(_MapTex,uv*8-_Time.y*0.1);
                return col;
            }
            ENDCG
        }
    }
}
