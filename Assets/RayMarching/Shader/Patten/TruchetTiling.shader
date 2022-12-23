Shader "RayMarch/Truchet"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Map("_Map",2D) = "black"{}
		_LineWidth("_LineWidth",Range(0.1,0.5))=0.1
		_Seed ("_Seed",Vector)=(234.56,568.35,0,0)
		_Number("_Number",float) =5
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
			sampler2D _Map;
            float4 _MainTex_ST;
			float _LineWidth,_Number;
			float2 _Seed;

			float N21 (float2 p,float2 seed)
			{
				 p = frac( p* seed);
				p+= dot(p,p+34.56);
				return frac(p.x*p.y);
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
                // sample the texture
                fixed4 col = 0;
				float _width =_LineWidth;
				float2 seed = _Seed;
				float2 uv  = i.uv*_ScreenParams/_ScreenParams.y;
				

				float r  = length(uv);
				float a = atan2(uv.x,uv.y)/UNITY_PI*2;




				

				uv *= _Number;
				float2 guv = frac(uv)-0.5;
				float2 id =floor(uv);

				float n = N21(id,seed);
				if (n>0.5) guv.x*=-1;

			    float lined=abs( abs(guv.x+guv.y)-0.5);//Line
				float linec = smoothstep(0.03,-0.03,abs(lined)-_width);
				/////way A
				//float d =length(guv-float2(0.5,0.5))-0.5;//circle1
				//float c = smoothstep(0.03,-0.03,abs(d)-_width);
				//d =length(guv-float2(-0.5,-0.5))-0.5;//circle2
				//c += smoothstep(0.03,-0.03,abs(d)-_width);
				/////wayB
				float2 cuv =guv-0.5*sign(guv.x+guv.y+1e-3);
				float2 d =length(cuv);
				float c = smoothstep(0.02,-0.02,abs(d-0.5)-_width);

				float angle = atan2(cuv.x,cuv.y);

				float checker = fmod((id.x+id.y)+2*_Number,2)*2-1;
				float flow =sin(_Time.y+checker*angle*10);



				float x = frac(_Time.y*0.3+checker* angle/UNITY_PI*2);
				float y = (d-(0.5-_width))/(2*_width);
				y =abs( y-0.5)*2;
				float2 tuv =float2 (x,y);
				//col =flow*c;
				col += tex2D(_Map,tuv)*c;
				col *=tuv.y*1.2;
				col =linec;


				col = tex2D (_Map,float2(1/r+_Time.y,a+_Time.x*2))*r;
				//col.rg =tuv*c;
				//col+=checker;
				//col.rg =checker;
				//col.rg =guv;
				//if (guv.x>0.48 || guv.y >0.48) col =float4(1,0,0,1);
                return col;
            }
            ENDCG
        }
    }
}
