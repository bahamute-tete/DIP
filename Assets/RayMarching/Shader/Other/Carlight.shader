Shader "RayMarch/CarLight"
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

			float N1(float a )
			{
				return frac(sin(a*4536.25)*3754.12);
			}

			float3 N13 (float a)
			{
				return frac(sin(a*float3(123.56,456.23,789.56))*float3(458.23,753.14,943.23));
			}

			float2 hash22(float2 p)
			{
				float3 a= frac(p.xyy*float3(356.23,786.12,543.36));
				a+=dot(a,a.yzx+34.56);
				return  frac( float2(a.x*a.y,a.y*a.z));
			}

			float3 cameraSet( float3 ro ,  float3 la, float zoom,float2 uv)
			{
				float3 f = normalize( la - ro ), ri = cross(float3(0,1,0),f), up = cross(f,ri);
				float3 ct = ro+f*zoom;
				float3 ip = ct+ri*uv.x+up*uv.y;
				return normalize(ip-ro);	
			}

			float sdPoint (float3 p,float3 ro,float3 rd)
			{	
				float3 rop = p -ro;
				float3 proj =dot(rop,rd)*rd;
				return length(rop -proj);/*float  d =  length (cross(rop ,rd))/length(rd);*/
			}

			float bokeh(float3 ro,float3 la,float zoom,float3 p,float2 uv,float size,float blur,float symmetry)
			{
				float3 rd = cameraSet(ro,la,zoom,uv);
				rd.x = sign(symmetry)>0? abs(rd.x):rd.x;//>0 symmetry  =0&<0  origin
				float d = sdPoint(p,ro,rd);
				size*= length(p);
				float c= smoothstep(size,size*(1-blur),d);
				c *=lerp(0.6,1.0,smoothstep(size*0.8,size,d));//outer birghter
				return c;
			}

			float4 streetLight (float3 ro,float3 la,float zoom,float2 uv,float size,float blur )
			{
				float  sum =0;
				float3 p =float3(2.0,2.0,50);
				//float s = step(0,cameraSet(ro,la,zoom,uv).x);//left=1 right=0
				float s = sign(cameraSet(ro,la,zoom,uv).x)>0? 1:0;
				
				
				for(float j = 0 ; j<1.0 ;j+=0.1)
				{
					float ti =frac(_Time.x+j+s*j*0.5);//make left and right have diffrent distance
					//float fade = ti*ti*(3-2*ti);
					float fade = ti*ti*ti;
				    sum += bokeh(ro,la,zoom,p-float3(0,0,50*ti),uv,size,blur,1)*fade;//move & fade 
				}
				return  sum* float4 (1.0,0.7,0.1,1);
			}

			float4 evnLight (float3 ro,float3 la,float zoom,float2 uv,float size)
			{
				float  sum =0;
				
				//float s = step(0,cameraSet(ro,la,zoom,uv).x);//left=1 right=0
				float s = sign(cameraSet(ro,la,zoom,uv).x)>0? 1:0;
				float4 c =0;
				
				for(float j = 0 ; j<1.0 ;j+=0.1)
				{	
					float3 n= N13(j);
					float x =lerp(2,6,n.x);
					float y =lerp(0,4,n.y);
					size *= lerp(1,1.05,n.z);
					
				
					float3 p =float3(x,y,25);
					
					float ti =frac(_Time.x+j+s*j*0.5);//make left and right have diffrent distance
					float blur = lerp(0.3,0.15,ti);
					//float fade = ti*ti*(3-2*ti);
					float fade = sin((ti*ti*ti)*100*n.y)*0.5+0.5;
					float4 evncol =float4(n.zyx,1);
				    c += bokeh(ro,la,zoom,p-float3(0,0,25*ti),uv,size,blur,1)*fade*evncol;//move & fade
					
				}
				return  c*0.6;
			}

			float4 carHeadLight (float3 ro,float3 la,float zoom,float2 uv,float size )
			{
				float  sum =0;
				float3 p =float3(-1,0.15,50);
				float3 pRef =float3(-1,-0.45,50);
				float offset1= 0.2;
				float offset2= offset1*1.2;
				
				for(float j = 0 ; j<1.0 ;j+=0.05)
				{	
					float n= N1(j);
					if (n>0.3) continue;
					float ti =frac(_Time.x*3+j);
					//float fade = ti*ti*(3-2*ti);
					float fade = ti*ti*ti*ti*ti;
					float focus = smoothstep(0.9,1,ti);
					size = lerp (size,size*0.7,focus);
				    sum += bokeh(ro,la,zoom,p-float3(offset1,0,50*ti),uv,size,0.1,0)*fade;//move & fade 
					sum += bokeh(ro,la,zoom,p-float3(-offset1,0,50*ti),uv,size,0.1,0)*fade;

					sum += bokeh(ro,la,zoom,p-float3(offset2,0,50*ti),uv,size,0.1,0)*fade;//move & fade 
					sum += bokeh(ro,la,zoom,p-float3(-offset2,0,50*ti),uv,size,0.1,0)*fade;

					float reflection = 0 ;
					reflection += bokeh(ro,la,zoom,pRef-float3(offset2,0,50*ti),uv,size*3,1,0)*fade;//move & fade 
					reflection += bokeh(ro,la,zoom,pRef-float3(-offset2,0,50*ti),uv,size*3,1,0)*fade;

					sum+=reflection*focus;
				}
				return  sum* float4 (1.0,0.9,0.8,1);
			}

			float4 carTrailLight (float3 ro,float3 la,float zoom,float2 uv,float size )
			{
				float  sum =0;

				float offset1= 0.2;
				float offset2= offset1*1.2;
				
				for(float j = 0 ; j<1.0 ;j+=0.1)
				{	

					float3 p =float3(1,0.15,50);
					float3 pRef =float3(1,-0.45,50);
					float n= N1(j);
					if (n>0.7) continue;


					float ti =frac(_Time.x*0.75+j);

					float s = step(0.20,n);
					float laneoffset = smoothstep(0.99,0.9,ti);
					float lane = s*laneoffset;

					//float fade = ti*ti*(3-2*ti);
					float fade = ti*ti*ti*ti*ti;
					float focus = smoothstep(0.9,1,ti);
					size = lerp (size,size*0.7,focus);

					//float  blink = sign(sin(_Time.x*200))>0?1:0;
					//blink =blink*s*smoothstep(0.96,0.8,ti);
					float  blink = step(0,sin(_Time.x*100));

				    sum += bokeh(ro,la,zoom,p-float3(offset1+lane,0,50*ti),uv,size,0.1,0)*fade;//move & fade 
					sum += bokeh(ro,la,zoom,p-float3(-offset1+lane,0,50*ti),uv,size,0.1,0)*fade*(1+ blink);

					sum += bokeh(ro,la,zoom,p-float3(offset2+lane,0,50*ti),uv,size,0.1,0)*fade;//move & fade 
					sum += bokeh(ro,la,zoom,p-float3(-offset2+lane,0,50*ti),uv,size,0.1,0)*fade*(1+ blink);

					float reflection = 0 ;
					reflection += bokeh(ro,la,zoom,pRef-float3(offset2+lane,0,50*ti),uv,size*3,1,0)*fade;//move & fade 
					reflection += bokeh(ro,la,zoom,pRef-float3(-offset2+lane,0,50*ti),uv,size*3,1,0)*fade*(1+ blink);

					sum+=reflection*focus;
				}
				return  sum* float4 (1.0,0.2,0.1,1);
			}

			float2 rainDistorition (float2 uv)
			{
				float t  = _Time.y*3;
				float2 p =0;
				uv*= 5;
				

				float2 asp=float2(2,1);
				float2 st = uv*asp;
				st.y +=t*0.3;
			
				float2 id = floor(st);
				st.y += frac(sin(id.x*235.68)*1896.24);
				
				 id = floor(st);
				 t += frac(sin(id.x*35.68+id.y*45.98)*186.24)*6.28;
				
				float2 gv= frac(st)-0.5;
				
				float y = -sin(t+sin(t+sin(t)*0.5))*0.45;
				float2 posoff = float2 (0,y);
				float2 o1 =(gv-posoff)/asp;
				float d = length(o1);
				float p1 = smoothstep(0.09,0,d);

				float2 gv2 = frac(uv*2*float2(1,3))-0.5;
				float2 o2 = gv2/float2(1,3);
				float d2 = length(o2);
				float p2 = smoothstep(0.22*(1-(gv.y+0.5)),0,d2)*smoothstep(-0.1,0.1,gv.y-posoff.y);

				//float uvx = gv.x>0.49? 1:0;
				//float uvy = gv.y>0.49? 1:0;
				//p+=float2(uvx,uvy);
				////p+=gv;
				//p+=p1*o1+p2;
				

				
				return 20*(o1*p1+o2*p2);
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

                float4 col =0;
				float2 uv = (i.uv -0.5)*_ScreenParams/_ScreenParams.y;
				uv -= rainDistorition(uv*1)*0.2;
				uv += rainDistorition(uv*2)*0.1;
				uv.x += sin(uv.y*60)*0.001;
				uv.y += sin(uv.x*60)*0.002;
				//////////////////////////////////
				float3 ro = float3(0.5,0.2,0);
				float3 la = float3 (0.5,0,5);
				float zoom = 1.0;
				float blur =0.1;
				float size = 0.05;
				float4  street =0;
				float4  carHead =0;
				float4  carTrail =0;
				float4  evn =0;
				float4  skyCol =float4(0.1,0.2,0.8,1) ;
				float3 rd=0;
				//////////////////////////////////
				street= streetLight(ro,la,zoom,uv,size,blur);
				carHead =carHeadLight(ro,la,zoom,uv,size);
				carTrail=carTrailLight(ro,la,zoom,uv,size);
				evn=evnLight(ro,la,zoom,uv,size);

				col =carHead+street+carTrail+evn;

				rd =cameraSet( ro ,la,zoom,uv);
				col =lerp(col,skyCol,rd.y-0.1);
				//col =float4(rainDistorition(uv),0,0);

                return col;
            }
            ENDCG
        }
    }
}
