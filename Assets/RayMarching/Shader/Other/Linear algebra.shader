Shader "RayMarch/Linear algebra"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_LineWidth("_LineWidth",Range(0.01,0.1))=0.1
		_Map2("_Map2",2D)= "white" {}
		t ("t",Range(0,1))=0
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			sampler2D _Map2;
            float4 _MainTex_ST;
			float _LineWidth;
			float t;

			float sdline(float2 p,float2 a ,float2 b)
			{
				float2 pb = p-a;
				float2 ab = b-a;
				float  t= clamp(dot(pb,ab)/dot(ab,ab),0,1);
				return length(pb - t*ab);
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
				float2 uv = i.uv*_ScreenParams/_ScreenParams.y;
				float2 ouv = i.uv*_ScreenParams/_ScreenParams.y;
				float4 col = 0;
				uv *=8;
				ouv*=8;
				//Unity shader matrix is colume matrix
				float2x2 M1  = float2x2 (1,1,0,1);
				float2x2 IM1  = float2x2 (1,-1,0,1);
				float2x2 M2 = float2x2 (0,-1,1,0);
				float2x2 IM2 = float2x2 (0,1,-1,0);
				// 如果需要转换到 以 的变换矩阵的值为基的空间里 需要乘以这个变换矩阵的逆矩阵 
				//Inv(A)*A*[x_new,y_new]=Inv(A)*[x_origin,y_origin]
				//例如M1矩阵的i~=[1,0] j~=[1,1] 如果要构成以这2个向量为基的向量空间  则需要把用M1的逆矩阵IM1乘以原来的uv
				uv =mul(IM1,uv);

				float dorigin= length(ouv);
				float dxn=0;				
				float dyn =0;

				float pc= smoothstep(0.02,0.019,dorigin);
				float cxn = 0;
				float cyn = 0;

				for (int j =-_ScreenParams.x ;j< _ScreenParams.x ;j++)
				{
					dxn = length(ouv-float2(j,0));
					cxn = smoothstep(0.02,0.019,dxn);
					dyn = length(ouv-float2(0,j));
					cyn =smoothstep(0.02,0.019,dyn);
					col +=cxn;
					col +=cyn;

				}
				//float dx = sdline(ouv,float2(0,0),float2(1,0));
				//float dy = sdline(ouv,float2(0,0),float2(0,1));
				//float px = smoothstep(0.01,0.009,dx);
				//float py = smoothstep(0.01,0.009,dy);

				float di = sdline(uv,float2(0,0),float2(1,0));
				float dj = sdline(uv,float2(0,0),float2(0,1));
				float pi = smoothstep(0.01,0.009,di);
				float pj = smoothstep(0.01,0.009,dj);

				col += pi *float4(0.3,0,0,1);
				col += pj *float4(0,0.3,0,1);
				col += pc*float4(1,1,1,1);
				//col += px*float4(1,0,0,1);
				//col += py*float4(0,1,0,1);



				float2 testv = float2(1,1);
				// 对于一个变换矩阵乘以一个向量 可以看成以该矩阵构成的2个基张成的向量空间
				//例如 对于一个向量 v=[1,1]一个变换矩阵M1 则可以看成 i~=[1,0] j~=[1,1] v在这个空间的线性变换
				float2 ov =mul(M1,testv);


				float dd= sdline(uv,0,ov);
				float cdd = smoothstep(0.01,0.009,dd);
				col +=cdd;

				float dv = sdline(uv,0,testv);
				float cv = smoothstep(0.01,0.009,dv);

				col+=cv*float4(1,1,0,1);
				col += tex2D (_Map2,ouv)*0.2;




                return col;
            }
            ENDCG
        }
    }
}
