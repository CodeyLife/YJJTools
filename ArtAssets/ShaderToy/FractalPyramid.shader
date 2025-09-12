Shader "Custom/FractalPyramid"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        
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

            fixed4 _MainColor;
        

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float3 palette(float d)
            {
                return lerp(float3(0.2, 0.7, 0.9), float3(1.0, 0.0, 1.0), d);
            }

            float2 rotate(float2 p, float a)
            {
                float c = cos(a);
                float s = sin(a);
                return mul(p, float2x2(c, s, -s, c));
            }

            float map(float3 p)
            {
                for (int i = 0; i < 8; i++)
                {
                    float t = _Time.y * 0.2;
                    p.xz = rotate(p.xz, t);
                    p.xy = rotate(p.xy, t * 1.89);
                    p.xz = abs(p.xz);
                    p.xz -= 0.5;
                }
                return dot(sign(p), p) / 5.0;
            }

            float4 rm(float3 ro, float3 rd)
            {
                float t = 0.0;
                float3 col = float3(0.0, 0.0, 0.0);
                float d;
                
                for (float i = 0.0; i < 64.0; i++)
                {
                    float3 p = ro + rd * t;
                    d = map(p) * 0.5;
                    
                    if (d < 0.02)
                        break;
                    
                    if (d > 100.0)
                        break;
                    
                    col += palette(length(p) * 0.1) / (400 * d);
                    t += d;
                }
                
                return float4(col, 1.0 / (d * 100.0));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ����UV���꣬����Ļ����Ϊԭ��
                float2 uv = (i.uv * _ScreenParams.xy - _ScreenParams.xy * 0.5) / _ScreenParams.x;
                
                // ���λ��
                float3 ro = float3(0.0, 0.0, -50.0);
                ro.xz = rotate(ro.xz, _Time.y);
                
                // �����������
                float3 cf = normalize(-ro);
                float3 cs = normalize(cross(cf, float3(0.0, 1.0, 0.0)));
                float3 cu = normalize(cross(cf, cs));
                
                // ������߷���
                float3 uuv = ro + cf * 3.0 + uv.x * cs + uv.y * cu;
                float3 rd = normalize(uuv - ro);
                
                // ���߲���������ɫ
                float4 col = rm(ro, rd);
                
                return col * _MainColor;
            }
            ENDCG
        }
    }
}
