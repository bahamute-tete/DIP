Shader "RayMarch/BezierCurve2D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		[Toggle] _Gizimo("_GizimoOn",float) = 0 
		[Toggle] _Rank3("_RANK3_ON",float) = 1 
		[Toggle] _Rank4("_RANK4_ON",float) = 0 
		[Toggle] _Hermite("_HERMITE_ON",float) = 1 
		_ClipValue ("_ClipValue",Range(0,1))= 0.5
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
			#pragma shader_feature _GIZIMO_ON
			#pragma shader_feature _RANK3_ON
			#pragma shader_feature _RANK4_ON
			#pragma shader_feature _HERMITE_ON
			#pragma shader_feature _BACKGROUND
		


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
			float2 _p1;
			float2 _p2;
			float2 _p3;
			float2 _p4;
			float _timelength;
			float _ClipValue;
			int _isAnimation = 0;
			int _clipBezierGizimo= 0;
			int _showOrigin = 0;
			int _showClip = 0 ;
			int _showCatmull_Rom = 0 ;

			float sdline(float2 p ,float2 a ,float2 b)
			{
				float2 pa = p-a;
				float2 ba = b-a;
				float h =clamp(dot(pa,ba)/dot(ba,ba),0,1);//proj =(y.u/u.u)
				float d =length( pa - ba*h);
				return d;
			}

			float sdPoint(float2 p,float2 a)
			{
				return length(p-a);
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				
                
				
				float2 uv = (2*i.uv-1)*_ScreenParams/_ScreenParams.y;
				#ifdef _BACKGROUND
				float4 col = 0.1;
				#else
				float4 col = tex2D(_MainTex,i.uv);
				#endif
				
				uv -=float2(0.3,0);
				uv*=3;
				float alpha = 0.3;

				float2 p1 = float2(-3,-1.5);
				 p1 = _p1;
				float2 pp1 =p1;

				float2 p2 = float2(-1,1);
				 p2 = _p2;
				float2 pp2 = p2;

				float2 p3 = float2(1.0,-1);
				 p3 = _p3;
				float2 pp3 = p3;

				float2 p4 = float2(2,1.33);
				 p4 = _p4;
				float2 pp4 = p4;

				
				float dp1 = sdPoint(uv,pp1);
				float dp2 = sdPoint(uv,pp2);
				float dp3 = sdPoint(uv,pp3);
				float dp4 = sdPoint(uv,pp4);
				

				float d1 = sdline(uv,p1,p2);
				float d2 = sdline(uv,p2,p3);
				float d3 = sdline(uv,p3,p4);

				float r= fwidth(d1)*2;
				float t =min( fmod(_Time.y/_timelength,1.2),1);

				#if _GIZIMO_ON
				col +=smoothstep(0.05,0.04,dp1)*float4(1,0,0,1)*0.6;
				col +=smoothstep(0.05,0.04,dp2)*float4(0,1,0,1)*0.6;
				col +=smoothstep(0.05,0.04,dp3)*float4(0,0,1,1)*0.6;
				col +=smoothstep(0.05,0.04,dp4)*float4(1,0.55,0.85,1)*0.6;

				
				col += smoothstep(r,0,d1)*alpha;
				col += smoothstep(r,0,d2)*alpha;
				col += smoothstep(r,0,d3)*alpha;
				
				
				

				if(_isAnimation==1)
				{
					float2 p1p2 = p1*(1-t)+p2*t;
					float2 p2p3 = p2*(1-t)+p3*t;
					float2 p3p4 = p3*(1-t)+p4*t;

					float dp1p2 = sdPoint(uv,p1p2);
					float dp2p3 = sdPoint(uv,p2p3);
					float dp3p4 = sdPoint(uv,p3p4);

				

					float dl0 = sdline(uv,p1p2,p2p3);
					float dl1 = sdline(uv,p2p3,p3p4);
				
					float2 pb= p1p2*(1-t)+p2p3*t;
					float dpb = sdPoint(uv,pb);

					float2 pb1= p2p3*(1-t)+p3p4*t;
					float dpb1 = sdPoint(uv,pb1);

					float2 pbcubic = pb*(1-t)+pb1*t;
					float dpbcubic = sdPoint(uv,pbcubic);

					float dlcubic = sdline(uv,pb,pb1);
				
				
					col +=smoothstep(0.05,0.04,dp1p2)* lerp(float4(1,0,0,1),float4(0,1,0,1),t)*0.6;
					col +=smoothstep(0.05,0.04,dp2p3)* lerp(float4(0,1,0,1),float4(0,0,1,1),t)*0.6;
					col +=smoothstep(0.05,0.04,dp3p4)* lerp(float4(0,0,1,1),float4(1,0.55,0.85,1),t)*0.6;

					col += smoothstep(r,0,dl0)*alpha*2;
					col += smoothstep(r,0,dl1)*alpha*2;

					col += smoothstep(0.05,0.04,dpb)*float4(1,1,0,1)*0.3;
					col += smoothstep(0.05,0.04,dpb1)*float4(1,1,0,1)*0.3;
					col += smoothstep(0.05,0.04,dpbcubic)*float4(1,0.55,0.85,1);

					col += smoothstep(r,0,dlcubic)*alpha;
				}
				
				#endif

				//Bezier =>Hermite
				float2 T1 = 3*(p2-p1);
				float2 T3 = 3*(p4-p3);
				float4x4 M_hermite = float4x4 ( 1 , 0 , -3 , 2 ,
												0,  0 , 3 , -2,
												0,  1, -2 , 1,
												0,  0, -1 , 1);


				//de_Casteljau=>Clip Berzier
				float s= _ClipValue;
				float2 m_p2p3= p2*(1-s)+s*p3;

				float2 q1= p1;
				float2 q2= p1*(1-s)+s*p2;
				float2 q3= q2*(1-s)+s*m_p2p3;


				float2 r4= p4;
				float2 r3= p3*(1-s)+s*p4;	
				float2 r2= m_p2p3*(1-s)+s*r3;

				float2 q4r1= q3*(1-s)+s*r2;
				float2 q4= q4r1;
				float2 r1= q4r1;

				//Catmull_Rom
				float4x4 M_CatmullRom = float4x4 ( 0 , -0.5 ,1 , -0.5 ,
												   1,  0 , -2.5 , 1.5,
												   0,  0.5, 2 , -1.5,
												   0,  0, -0.5 , 0.5); 
				float2 p0 = float2 (-4,-2);
				float2 p5 = float2 (4,2);
				///////////////////////////
				if(_clipBezierGizimo)
				{
					float dlq1= sdline(uv,q2,q1);
					float dlq2= sdline(uv,q3,q2);
					float dlq3= sdline(uv,q4,q3);

					float dlr1= sdline(uv,r2,r1);
					float dlr2= sdline(uv,r3,r2);
					float dlr3= sdline(uv,r4,r3);

					col +=smoothstep(r*2,0,dlq1)*alpha;
					col +=smoothstep(r*2,0,dlq2)*alpha;
					col +=smoothstep(r*2,0,dlq3)*alpha;

					col +=smoothstep(r*2,0,dlr1)*alpha;
					col +=smoothstep(r*2,0,dlr2)*alpha;
					col +=smoothstep(r*2,0,dlr3)*alpha;
				}
				

				#define SAMPLENUM 64

				float2 sp = p1;
				float2 sp1 = p1;
				float2 sp2 = p1;

				float2 sp3= p1;
				float2 sp4= r1;

				float2 sp5= p2;




				for(int j = 0 ;j<SAMPLENUM ;j++)
				{
					float ani = t* float(j)/SAMPLENUM;

					float4x1 M_t = float4x1 (1,ani,ani*ani,ani*ani*ani); 
					float4x1  M_H = mul(M_hermite,M_t);
					float4x1  M_C = mul(M_CatmullRom,M_t);

					#if _RANK3_ON
					//Bezier 3
					float2 pba = pow(1-ani,2)*p1+2*ani*(1-ani)*p2+pow(ani,2)*p3;//3CV
					float dlba = sdline(uv,sp,pba);
					col += smoothstep(r*2,0,dlba)*float4(1,1,0,1);
					sp = pba;

					#endif

					#if _RANK4_ON
					//Bezier 4
					if (_showOrigin)
					{
						float2 pbacubic = pow(1-ani,3)*p1+3*ani*(1-ani)*(1-ani)*p2+3*pow(ani,2)*(1-ani)*p3+ pow(ani,3)*p4;//4CV
						float dlbacubic = sdline(uv,sp1,pbacubic);
						sp1 = pbacubic;
						col += smoothstep(r*2,0,dlbacubic)*float4(1,0.55,0.85,1);
					}
					
					if(_showClip)
					{
						//BezierClip
					float2  pbBerzierQ = pow(1-ani,3)*q1+3*ani*(1-ani)*(1-ani)*q2+3*pow(ani,2)*(1-ani)*q3+ pow(ani,3)*q4;
					float2  pbBerzierR = pow(1-ani,3)*r1+3*ani*(1-ani)*(1-ani)*r2+3*pow(ani,2)*(1-ani)*r3+ pow(ani,3)*r4;
					
					float dbBerzierQ = sdline(uv,sp3,pbBerzierQ);
					sp3 = pbBerzierQ;
					float dbBerzierR = sdline(uv,sp4,pbBerzierR);
					sp4 = pbBerzierR;

					col += smoothstep(r*2,0,dbBerzierQ)*float4(0.5,0.55,0.85,1);
					col += smoothstep(r*2,0,dbBerzierR)*float4(0.5,0.55,0.85,1);
					}

					#endif

					#if _HERMITE_ON
					//Hermite
					//float2  pbHermite = (1-3*pow(ani,2)+2*pow(ani,3))*p1+pow(ani,2)*(3-2*ani)*p3+ani*pow(ani-1,2)*T1+pow(ani,2)*(ani-1)*T3;
					float1x4  G_Hermite_x = float1x4 (p1.x,p3.x,T1.x,T3.x);
					float1x4  G_Hermite_y = float1x4 (p1.y,p3.y,T1.y,T3.y);

					float  a =mul(G_Hermite_x,M_H);
					float  b =mul(G_Hermite_y,M_H);
					float2  pbHermite = float2 (a,b);

					float dlHermite = sdline(uv,sp2,pbHermite);
					sp2= pbHermite;
					col += smoothstep(r*2,0,dlHermite)*float4(0,0,1,1);

					
					#endif
					

					
					if(_showCatmull_Rom)
					{
						float1x4  G_CR_x = float1x4 (p1.x,p2.x,p3.x,p4.x);
						float1x4  G_CR_y = float1x4 (p1.y,p2.y,p3.y,p4.y);

						float  ca =mul(G_CR_x,M_C);
						float  cb =mul(G_CR_y,M_C);

						float2  pbCatmull_Rom = float2 (ca,cb);
						float  dlCatmull_Rom = sdline(uv,sp5,pbCatmull_Rom);
						sp5= pbCatmull_Rom;
						col += smoothstep(r*2,0,dlCatmull_Rom)*float4(0.2,0.5,1,1);
					}
					
				}


                return col;
            }
            ENDCG
        }
    }
}
