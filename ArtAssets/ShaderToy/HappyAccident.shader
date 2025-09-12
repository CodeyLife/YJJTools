Shader "Custom/HappyAccident"
{
    Properties
    {
        _Iterations ("Iterations", Int) = 77  // ��������������ϸ��
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
            #pragma target 3.0  // ǿ�Ƹ߾��ȼ���
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            int _Iterations;

            // �������Ƕȴ�����ת���󣨻�ԭShaderToy��mat2�߼���
            float2x2 rotate(float c, float s)
            {
                return float2x2(c, -s, s, c);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float i_loop;
                float d;
                // ��ԭԭ�������㣨ʹ�������������UV��
                float2 C = i.uv * _ScreenParams.xy;  // ת��Ϊ�������꣬ƥ��fragCoord
                float z = frac(dot(C, sin(C))) - 0.5;
                
                float4 o = float4(0, 0, 0, 0);
                float4 p;
                float4 O;
                float2 r = _ScreenParams.xy;
                 // ��ԭ���߷�����㣨�����������꣩
                float3 rayDir = normalize(float3(C - 0.5 * r, r.y));
                for (i_loop = 0; i_loop < _Iterations; i_loop++)
                {
                   
                    p = float4(z * rayDir, 0.1 * _Time.y);
                    
                    p.z += _Time.y;
                    O = p;
                    
                    // ������һ����ת����ʹ��O.z���㣩
                    float angle1 = 2.0 + O.z;
                    float2x2 rot1 = rotate(cos(angle1), sin(angle1));  // ��ȷ����sin��cos
                    p.xy = mul(rot1, p.xy);
                    
                    // �����ڶ�����ת���󣨹ؼ���ʹ��O���ĸ��������㣬��ԭԭ�߼���
                    float4 cosVals = cos(O + float4(0, 11, 33, 0));
                    float2x2 rot2 = rotate(cosVals.x, cosVals.y);  // ȡǰ����������Ϊcos��sin
                    p.xy = mul(rot2, p.xy);
                    
                    // ��ɫ���㱣��һ��
                    O = (1.0 + sin(0.5 * O.z + length(p - O) + float4(0, 4, 3, 6))) 
                        / (0.5 + 2.0 * dot(O.xy, O.xy));
                    
                    p = abs(frac(p) - 0.5);
                    d = abs(min(length(p.xy) - 0.125, min(p.x, p.y) + 0.001)) + 0.001;
                    o += O.w / d * O;
                    z += 0.6 * d;
                }
                
                return tanh(o / 20000.0);
            }
            ENDCG
        }
    }
}
    