Shader "RayMarch/Pathtracing"
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

			struct Ray { 
				float3 origin;	// 光线原点
				float3 dir; 		// 光线方向
			};

			struct OMaterial {
				
				float3 emissionColor;	// 自发光
				float3 albedo;		// 颜色
				
			};

			struct Sphere {
				float radius;	// 半径
				float3 pos;		// 位置
				OMaterial mat;	// 材质
			};

			struct Plane {
				float3 pos;		// 位置
				float3 normal;	// 法线
				OMaterial mat;	// 材质
			};



            sampler2D _MainTex;
            float4 _MainTex_ST;


			
			float rand(float seed) { return frac(sin(seed++)*43758.5453123); }

			float3 cosWeightedSampleHemisphere(float3 n,float seed) 
			{
				float u1 = rand(seed), u2 = rand(seed);
				float r = sqrt(u1);
				float theta = 2. * UNITY_PI * u2;
    
				float x = r * cos(theta);
				float y = r * sin(theta);
				float z = sqrt(max(0., 1. - u1));
    
				float3 a = n, b;
    
				if (abs(a.x) <= abs(a.y) && abs(a.x) <= abs(a.z))
				a.x = 1.0;
				else if (abs(a.y) <= abs(a.x) && abs(a.y) <= abs(a.z))
				a.y = 1.0;
				else
				a.z = 1.0;
        
				a = normalize(cross(n, a));
				b = normalize(cross(n, a));
    
				return normalize(a * x + b * y + n * z);
			}


			#define NUM_SPHERES 3
			#define NUM_PLANES 6
			Sphere spheres[NUM_SPHERES];
			Plane planes[NUM_PLANES];

			void initScene() 
			{

				OMaterial sphMat1,sphMat2,planeMat1,planeMat2,planeMat3,planeMat4,planeMat5,planeMat6;

				sphMat1.emissionColor = 0;
				sphMat1.albedo = 1;

				sphMat2.emissionColor =5;
				sphMat2.albedo = 1;

				planeMat1.emissionColor = 0;
				planeMat1.albedo = 0.75;

				planeMat2.emissionColor = 0;
				planeMat2.albedo = float3(.75, .25, .25);

				planeMat3.emissionColor = 0;
				planeMat3.albedo = 0.75;

				planeMat4.emissionColor = 0;
				planeMat4.albedo = float3(.25, .25, .75);

				planeMat5.emissionColor = 0;
				planeMat5.albedo = 0;

				planeMat6.emissionColor = 0;
				planeMat6.albedo = 0.75;


				
				Sphere sph1,sph2,sph3;
				Plane  plane1,plane2,plane3,plane4,plane5,plane6;

				sph1.radius = 2;
				sph1.pos = float3(27,16.5,47);
				sph1.mat = sphMat1;

				sph2.radius = 2;
				sph2.pos = float3(73, 16.5, 78);
				sph2.mat = sphMat1;

				sph3.radius = 30;
				sph3.pos = float3(50, 689.3, 50);
				sph3.mat = sphMat2;

				plane1.pos = float3(0, 0, 0);
				plane1.normal = float3(0, 1, 0);
				plane1.mat = planeMat1;

				plane2.pos = float3(0, 0, 0);
				plane2.normal = float3(0, 1, 0);
				plane2.mat = planeMat2;

				plane3.pos = float3(0, 0, 0);
				plane3.normal = float3(0, 1, 0);
				plane3.mat = planeMat3;

				plane4.pos = float3(0, 0, 0);
				plane4.normal = float3(0, 1, 0);
				plane4.mat = planeMat4;

				plane5.pos = float3(0, 0, 0);
				plane5.normal = float3(0, 1, 0);
				plane5.mat = planeMat5;

				plane6.pos = float3(0, 0, 0);
				plane6.normal = float3(0, 1, 0);
				plane6.mat = planeMat6;

				spheres[0]=sph1;
				spheres[1]=sph2;
				spheres[2]=sph3;

				

				planes[0] =plane1;
				planes[1] =plane2;
				planes[2] =plane3;
				planes[3] =plane4;
				planes[4] =plane5;
				planes[5] =plane6;
			}

			float intersectSphere(Ray r, Sphere s) 
			{
				float3 op = s.pos - r.origin;
				float b = dot(op, r.dir);
    
				float delta = b * b - dot(op, op) + s.radius * s.radius;
				if (delta < 0.)           // 光线与球体未相交
				return 0.; 		        
				else                      // 光线与球体相交
				delta = sqrt(delta);
    
				float t;                  // 找到t最小的交点
				if ((t = b - delta) > 0.001)
				return t;
				else if ((t = b + delta) >0.001)
				return t;
				else
				return 0.;
			}


			float intersectPlane(Ray r, Plane p) 
			{
				float t = dot(p.pos - r.origin, p.normal) / dot(r.dir, p.normal);
				return lerp(0., t, float(t > 0.001));
			}

			int intersect(Ray ray, out float t, out float3 normal, out OMaterial mat) 
			{
				int id = -1;
				t = 1e5;
				for (int i = 0; i < NUM_SPHERES; i++) {
				float d = intersectSphere(ray,  spheres[i]);
				if (d != 0. && d<t) { 
				id = i; 
				t = d; 
				normal = normalize(ray.origin + ray.dir * t - spheres[i].pos);
				mat = spheres[i].mat;
				}
				}
    
				for (int i = 0; i < NUM_PLANES; i++) {
				float d = intersectPlane(ray, planes[i]);
				if (d != 0. && d < t) {
				id = i;
				t = d;
				normal = planes[i].normal;
				mat = planes[i].mat;
				}
				}
				return id;
			}

			//Ray generateRay(float2 uv)
			//{
			//	float2 p = uv * 2. - 1.;
    
			//	float3 camPos = float3(50., 40.8, 172.);
			//	float3 cz = normalize(float3(50., 40., 81.6) - camPos);
			//	float3 cx = float3(1., 0., 0.);
			//	float3 cy = normalize(cross(cx, cz)); 
			//	cx = cross(cz, cy);
    
				
			//	return Ray(camPos, normalize(.5135 * (  p.x * cx + p.y * cy) + cz));
			//}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

				float2  uv = (2*i.uv-1)*_ScreenParams/_ScreenParams.y;
                float4 col = 0;

                return col;
            }
            ENDCG
        }
    }
}
