Shader "RayMarch/Face"
{
    Properties
    {
        
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



			float sd2DCircle (float2 uv,float2 center ,float r,float smooth )
			{
				float d = length(uv-center);
				d = smoothstep(r,r-smooth,d);
				return d;
			}

			float band (float star,float end,float t,float smooth)
			{
				float step1 = smoothstep(star-smooth,star+smooth,t);
				float step2 = smoothstep (end+smooth,end-smooth, t);
				return step1*step2;
			}

			float Rec(float2 uv ,float left, float right,float bottom,float up,float smooth)
			{
				float band1 = band(left,right,uv.x,smooth);
				float band2 = band (bottom,up,uv.y,smooth);
				return band1*band2;
			}

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

			float remap01(float a,float b,float t)//normalize t
			{
				// t= clamp(t,0,1);
				return (t-a)/(b-a);
			}

			float remap01Normalize(float a,float b,float t)//normalize t
			{
				
				return saturate( (t-a)/(b-a));
			}

			float remap(float a ,float b ,float c, float d ,float t)//if t=a then equal c if t=b then equal d  if t= (b-a)*0.5 then equal (d-c)*0.5
			{
				return remap01(a,b,t)*(d-c)+c;
			}

			float remapNormalize(float a ,float b ,float c, float d ,float t)
			{
				return  saturate((t-a)/(b-a)*(d-c)+c);
			}

			float2 withInRect (float2 uv ,float4 rec)
			{
				return (uv-rec.xy)/(rec.zw-rec.xy);
			}
/////////////////////////////////////////////////////////////////////////////////////

			float4 smileFace (float2 uv,float2 center,float size,float4 faceColor)
			{
				uv -=center;
				uv /=size;

				float smooth =0.01;
				float face = sd2DCircle(uv,float2(0,0),0.4,smooth);

				//float d = length(uv);
				//float edgeCol_a = smoothstep(0.5,0.49,d);

				//float edgeShade = remap01(0.35,0.4,d);
				//edgeShade =1- edgeShade *1;
				//faceColor.rgb *= edgeShade;
				face -=sd2DCircle(uv,float2(-0.12,0.12),0.07,smooth);
				face -=sd2DCircle(uv,float2(0.12,0.12),0.07,smooth);

				float mouth = sd2DCircle(uv,float2(0,0),0.25,smooth);
				mouth -=sd2DCircle(uv,float2(0,0.08),0.26,smooth);

				face -=mouth;
				float4 finalface = faceColor*face;
				return  finalface;
			}

			float4 Head (float2 uv)
			{
				float4 col = float4(1,0.55,0.1,1);

				float d = length(uv);
				col.a = smoothstep(0.35,0.34,d);
				float edgeShadow = remap01Normalize(0.1,0.4,d);
				edgeShadow *= edgeShadow;
				col.rgb *=1-edgeShadow; 
				float circleEdge = smoothstep(0.33,0.35,d);
				col.rgb  = mix(col.rgb,float3 (0.9,0.75,0),circleEdge);
				float highlight = smoothstep(0.31,0.29,d);

				highlight *= remapNormalize(0.3,0,0.7,0.1,uv.y);
				highlight *= smoothstep(0.15,0.16,length(uv-float2(0.16,0.06)));
				col.rgb  = mix(col.rgb,float3 (1,1,1),highlight);

				float ad = length(uv -float2 (.19,-.12));
				float cheek= smoothstep(0.15,0.03,ad)*0.4;
				cheek *= smoothstep(0.14,0.13,ad);
				col.rgb = mix(col.rgb,float3(1,.1,.1),cheek);
				return col;
			}

			float4 Eye(float2 uv)
			{
				uv -=0.5;

				float d = length(uv);
				float4 irisCol = float4 (.3,.5,1.,1);
				float4 col = mix (float4(1,1,1,1),irisCol,smoothstep(0.1,0.7,d)*0.5);
				col.rgb *= 1-smoothstep(0.45,0.5,d)*0.5*saturate( -uv.y*2);//irisOutline

				col.rgb = mix(col.rgb,float4(0,0,0,1),smoothstep(0.33,0.3,d));
				irisCol.rgb *=1+smoothstep(0.27,0.01,d);
				col.rgb = mix(col.rgb,irisCol,smoothstep(0.3,0.29,d));
				col.rgb = mix(col.rgb,float4(0,0,0,0),smoothstep(0.16,0.14,d));
				//highlight
				float highlight = smoothstep(.1,0.09,length(uv -float2(0.1,0.2)));
				highlight +=smoothstep(.07,0.05,length(uv -float2(-0.1,-0.10)));
				col.rgb = mix(col.rgb,float4(1,1,1,1),highlight);
				col.a = smoothstep(0.5,0.48,d);
				return col;

			}

			float4 Mouth (float2 uv)
			{
				uv -=.5;
				float4 col = float4 (0.5,0.18,0.05,1);
				
				uv.x *=1;
				uv.y += 1*pow(uv.x,2);
				float d  = length(uv);
				col.a =smoothstep(.5,.48,d);

				float td = length(uv+float2(0.0,0.6));
				float3 toothCol = float3(1,1,1)*smoothstep(0.6,0.35,d);
				col.rgb = mix(col.rgb,toothCol,smoothstep(.45,.42,td));
				td = length(uv - float2(0.0,0.5));
				col.rgb = mix(col.rgb,float3(1.0,0.5,0.5),smoothstep(0.4,0.3,td));
				return col;
			}

			float4 EyeBow(float2 uv)
			{	
				float y = uv.y;
				//uv.y +=uv.x*0.4 -0.1;
				//uv.x -= 0.1;
				uv -=.5;
				float4 col = 0;
				float4 eyebowCol = float4 (.3,.16,.01,1);
				float d1 = length(uv);
				float eyebow1 = smoothstep (0.29,0.28,d1);
				float d2 = length(uv - float2(0.01,0.2)*0.7);
				float eyebow2 = smoothstep(0.25,0.24,d2);
				float eyeBowMask = saturate(eyebow2- eyebow1);
				float colMask = remap01Normalize(0.4,0.5,y)*0.75;
				colMask *= smoothstep(0.5,0.65,eyeBowMask);
				eyebowCol = mix(float4(1,.2,.2,1),eyebowCol,colMask);

				uv.y +=.05;
				col = mix (col,eyebowCol,colMask);

				float d3 = length(uv);
				float eyebowshadow1 = smoothstep (0.29,0.28,d3);
				float d4 = length(uv - float2(0.01,0.2)*0.7);
				float eyebowshadow2 = smoothstep(0.25,0.24,d4);
				float eyeBowMask2 = saturate(eyebowshadow2- eyebowshadow1);
				float4 eyebowShadowCol = mix(float4(0,0,0,0.5),eyebowCol,colMask);

				col = mix (col,eyebowShadowCol,eyeBowMask2);
				return col;
			}

			float4 smileFace02 (float2 uv)
			{
				float4 col =0;
				uv.x = abs (uv.x);

				float4 head = Head(uv);
				float4 eye =Eye(withInRect(uv,float4(.02,-.05,.27,.20)));
				float4 mouth =Mouth(withInRect(uv,float4(-.15,-.08,0.15,-0.25)));
				float4 eyebow = EyeBow(withInRect(uv,float4(-.05,0.05,0.4,0.30)));

				col = mix(col,head,head.a);
				col = mix (col,eye,eye.a);
				col = mix (col,mouth,mouth.a);
				col = mix (col,eyebow,eyebow.a);
				return col;
			}
///////////////////////////////////////////////////////////////////////
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
//////////////////////////////////////////////////////////////////////
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv-0.5 ;
				o.uv =o.uv *_ScreenParams/_ScreenParams.y;
		
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col =0;
				float2 center = float2 (0,0);
				float size = 0.5;
				float4 faceColor = float4(0.9,0.6,0,0);
				//float4 face = smileFace (i.uv,center,size,faceColor);
				

				float x= i.uv.x;
				float y= i.uv.y;
				//x+=0.2;//translate
				//x += y*0.5;//skew
				//float m =- (x-0.5)*(x+0.5);
				//m= m*m*3;
				float m = 0.1*sin(x*8 +_Time.y*20);
				y=y-m;

				float blur = remap(-0.4,0.4,0.01,0.25,x);
				blur = pow(blur*2,3);

				float recMask = Rec(float2(x,y),-0.4,0.4,-0.05,0.05,blur);


				col = smileFace02(i.uv);

                return col;
            }
            ENDCG
        }
    }
}
