Shader "Unlit/YjjHeatMapShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorRemap("映射颜色",2D) = "white"{}
        	_RandomOffset("weight随机", Float) = 0.3
        _RandomSpeed("运动速度",Range(0,1)) =0.1
        		[Toggle]_refelect("是否镜像UV",Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
            float4 _MainTex_ST;
            sampler2D _ColorRemap;
            float4 _ColorRemap_ST;
            uniform float _refelect;
            float _RandomSpeed;
            	uniform float _RandomOffset;


            inline float2 unity_voronoi_noise_randomVector(float2 UV, float offset)
			{
				float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
				UV = frac(sin(mul(UV, m)) * 46839.32);
				return float2(sin(UV.y * +offset) * 0.5 + 0.5, cos(UV.x * offset) * 0.5 + 0.5);
			}

            void Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity, out float Out, out float Cells)
			{
				float2 g = floor(UV * CellDensity);
				float2 f = frac(UV * CellDensity);
				float t = 8.0;
				float3 res = float3(8.0, 0.0, 0.0);

				for (int y = -1; y <= 1; y++)
				{
					for (int x = -1; x <= 1; x++)
					{
						float2 lattice = float2(x, y);
						float2 offset = unity_voronoi_noise_randomVector(lattice + g, AngleOffset);
						float d = distance(lattice + offset, f);

						if (d < res.x)
						{

							res = float3(d, offset.x, offset.y);
							Out = res.x;
							Cells = res.y;

						}
					}

				}

			}
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                 float2 uv = i.uv;
			    uv = _refelect * float2((1-uv.x),(1-uv.y)) + (1 - _refelect)*uv;
                // sample the texture
                float r = tex2D(_MainTex, uv).r;
                float _Voronoi_D068132B_Out;
				float _Voronoi_D068132B_Cells;
                Unity_Voronoi_float(uv.xy, _Time.y* _RandomSpeed, 5, _Voronoi_D068132B_Out, _Voronoi_D068132B_Cells);
                float add = _Voronoi_D068132B_Out*_RandomOffset;
                add = lerp(0,1,r)*add;
                r+=add;
                r= clamp(r,0,0.999);
                float4 col = tex2D(_ColorRemap,fixed2(r,0.5));
                col.a = r;
                return col;
            }
            ENDCG
        }
    }
}
