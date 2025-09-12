Shader "Custom/RaymarchingCube"
{
    Properties
    {
        _MainColor ("Main Color", Color) = (1,1,1,1)
        _GlowIntensity ("Glow Intensity", Range(0.1, 10.0)) = 1.0
        _Speed ("Animation Speed", Range(0.1, 3.0)) = 1.0
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

            float4 _MainColor;
            float _GlowIntensity;
            float _Speed;

            // ȫ�ֱ���
            float d;
            float z;
            float G;
            float M;
         // ԭ������� R ����ע�͵�
// float2 R[2]; 

// �µľ����壬���ڴ洢 2��2 ��ת����
float2x2 R; 

// ���󴴽������������֮ǰ������ת������߼� ��
void RotationMatrix(float angle, out float2x2 mat) {
    float c = cos(angle);
    float s = sin(angle);
    // ���� 2��2 ��ת���� 
    mat = float2x2(c, -s,
                   s,  c);
}



            // ���뺯�� - �����p����������̾���
            float D(float3 p)
            {
                // Ӧ����ת
                p.xy = mul(R, p.xy);
                p.xz = mul(R, p.xz);

                // ������Ƶ�������ڱ���ϸ��
                float3 S = sin(123.0 * p);

                // ���·���ֵ
                G = min(G, max(abs(length(p) - 0.6), 
                    d = pow(dot(p = p * p * p * p, p), 0.125) - 0.5 - pow(1.0 + S.x * S.y * S.z, 8.0) / 100000.0));

                return d;
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
                // ��ʼ������
                M = 0.001;
                float3 p; // ���ߵ�ǰλ��
                float3 O; // ��ɫ/������
                float3 r = _ScreenParams.xyz; // ��Ļ�ֱ���
                float2 C = i.uv * r.xy; // ��������
                float3 I = normalize(float3(C - 0.5 * r.xy, r.y)); // ���߷���
                float3 B = float3(1.0, 2.0, 9.0) * M * _GlowIntensity; // ����������ɫ
                
                // ������ת����
                float2x2 rotMat;
                RotationMatrix(0.3 * _Time.y * _Speed + float4(0, 11, 33, 0).x, rotMat);
                R[0] = rotMat[0];
                R[1] = rotMat[1];
                
                // ���߲�����ʼ��
                z = 0.0;
                G = 9.0;
                d = 1.0;
                
                // ���߲���ѭ��
                for (int steps = 0; steps < 64; steps++)
                {
                    if (z < 9.0 && d > M)
                    {
                        p = z * I;
                        p.z -= 2.0;
                        d = D(p);
                        z += d;
                    }
                    else
                    {
                        break;
                    }
                }
                
                // ������ɫ
                if (z < 9.0)
                {
                    // ���㷨����
                    O = float3(0, 0, 0);
                    for (int i = 0; i < 3; i++)
                    {
                        float3 eps = float3(0, 0, 0);
                        eps[i] = M;
                        O[i] = D(p + eps) - D(p - eps);
                    }
                    
                    // ��׼�����������������������
                    O = normalize(O);
                    z = 1.0 + dot(O, I);
                    
                    // ���㷴������
                    float3 reflectDir = reflect(I, O);
                    
                    // ���㷴���
                    float2 reflectPoint;
                    if (reflectDir.y > 0.0)
                    {
                        float t = (5.0 - p.y) / abs(reflectDir.y);
                        reflectPoint = (p + reflectDir * t).xz;
                    }
                    else
                    {
                        float t = (0.0 - p.y) / abs(reflectDir.y);
                        reflectPoint = (p + reflectDir * t).xz;
                    }
                    
                    // ���㷴����ɫ
                    float3 reflectColor;
                    if (reflectDir.y > 0.0)
                    {
                        float dist = sqrt(dot(reflectPoint, reflectPoint)) + 1.0;
                        reflectColor = 500.0 * smoothstep(5.0, 4.0, dist) * dist * B;
                    }
                    else
                    {
                        reflectColor = exp(-2.0 * length(reflectPoint)) * float3(1.0, 0.0, 6.0);
                    }
                    
                    // ������ɫ����
                    O = z * z * reflectColor + pow(1.0 + O.y, 5.0) * B;
                }
                else
                {
                    // ����ɫ
                    O = float3(0, 0, 0);
                }
                
                // ɫ��ӳ������
                float3 finalColor = sqrt(O + B / G) * _MainColor.rgb;
                return float4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}
    