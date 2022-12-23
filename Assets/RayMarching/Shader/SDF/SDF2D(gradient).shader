Shader "RayMarch/SDF2D(gradient)"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Map("Map",2D) = "white" {}
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
			sampler2D _Map;
/*
.x = f(p)
.y = ∂f(p)/∂x
.z = ∂f(p)/∂y
.yz = ∇f(p) with ‖∇f(p)‖ = 1
*/

			float3 Sdf2DCircle(float2 p,float r)
			{
				float d = length(p)-r;
				float2 g =p/d;
				return float3 (d,d>0? g: 1-g);
			}

			float3 Sdf2DBox(float2 p,float2 size)
			{
				float2 w = abs(p)-size;
				float2 s = float2(p.x>0?1:-1 ,p.y>0? 1:-1);//sign
				float g = max(w.x,w.y);//  if  g>0 ===> out rec   g<0 ===> inner rec 
				float2 q= max(w,0);
				float l = length(q)-0.012;
				float2  m  = s*(g>0?q/l : (w.x>w.y)? float2(1,0):float2(0,1));//if  inner w.x>w.y==> diagonal line left  else  right 
				return float3 ( (g)>0?l:g, m);
			}

			float sdEquilateralTriangle( in float2 p )
			{
				const float k = sqrt(3.0);
				p.x = abs(p.x) ;
				if( p.x+k*p.y>0.0 ) p = float2(p.x-k*p.y,-k*p.x-p.y)/2.0;
				p.x -= clamp( p.x, -1.0, 1.0 );
				return length(p)*sign(-p.y);
			}

			float sdTriangleIsosceles( in float2 p, in float2 q )//width.height
			{	/* top point  = float2 (0,0) ,heith is negative*/
			    
				p.x = abs(p.x);
				float2 a = p - q*clamp( dot(p,q)/dot(q,q), 0.0, 1.0 );//edge line
				float2 b = p - q*float2( clamp( p.x/q.x, 0.0, 1.0 ), 1.0 );//bottom line
				float k = sign( q.y );
				float d =min(dot(a,a),dot(b,b));
				/*
				res =a corss b  if res >0 a anticlockwise to b  or a is leftside to b 
				if res <0 a clockwise to b or a is rightside to b
				so  if  k<0 res <0  ==>> k*res >0 ===>> out of shape  else  inner 
				*/
				float s = max(k*(p.x*q.y-p.y*q.x),k*(p.y-q.y));
				return sqrt(d)*sign(s);
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
                // sample the texture
				float2 uv  =(2* i.uv-1 )*_ScreenParams/_ScreenParams.y;
				uv*= 1.1;
				float4 col = 0;
				////////////////////////
                //float4  col = tex2D(_Map,uv);
				//col.rgb = pow( col, float3(0.8,0.8,0.8) ); 
				//col.rgb *= 1.6; 
				//col.rgb -= float3(0.03,0.02,0.0);
				//col.rgb *= 0.5 + 0.5*pow( 16.0*uv.x*uv.y*(1.0-uv.x)*(1.0-uv.y), 0.1 );
				//////////////////////
				//float2 pos = uv-float2(0,0);
				//float dc = Sdf2DCircle(pos,0.45).x;
				//float2 gc = Sdf2DCircle(pos,0.45).yz;

				//float db = Sdf2DBox(pos,float2(0.4,0.4)).x;
				//float2 gb = Sdf2DBox(pos,float2(0.4,0.4)).yz;

				//float d = min(dc,db);
				//float2 g = min(gc,gb);

				//col.rgb = float3(0.5+0.5*gc,0.5+0.5*abs(dc));
				//col.rgb *=1-0.5*exp(-16*abs(dc ));//??
				//col.rgb *=0.2+0.2* sin(150.0*dc +_Time.y*10);

				//col.rgb = lerp( col.rgb, float3(1,1,1), 1-smoothstep(0,0.05,abs(dc)) );
				/////////////////////////
				float dt =sdTriangleIsosceles(uv-float2(0.0,0.5),float2(0.3,-1.1));
			    col.rgb =1- sign(dt)*float3(0.1,0.4,0.7);
				col *= 1.0 - exp(-2.0*abs(dt));
				col.rgb *=0.2+0.2* sin(150.0*dt +_Time.y*10);
				col.rgb +=lerp( col.rgb, float3(1,1,1), smoothstep(0.01,0.009,abs(dt)));
                return col;
            }
            ENDCG
        }
    }
}
