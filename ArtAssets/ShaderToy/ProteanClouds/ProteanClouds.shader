Shader "Custom/ProteanClouds"
{
    Properties
    {
        _CloudSpeed ("Cloud Speed", Range(0, 10)) = 1.0
        _CloudDensity ("Cloud Density", Range(-1, 2)) = 0.5
        _CloudBrightness ("Cloud Brightness", Range(0, 2)) = 1.0
       _Speed("_Speed",float) = 1
       _iteration("iteration",int) = 130
       //prm1("prm1",float) = 0
       
    }
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            // ����
            float _CloudSpeed;
            float _CloudDensity;
            float _CloudBrightness;
            float _Speed;
            float4 _MousePos;
            int _iteration;
            float2 bsMo;
            
            // ����
            static const float3x3 m3 = float3x3(
                0.33338, 0.56034, -0.71817,
                -0.87887, 0.32651, -0.15323,
                0.15162, 0.69596, 0.61339) * 1.93;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 ray : TEXCOORD1;
            };
            
             float2x2 rot(float a)
            {
                float c = cos(a);
                float s = sin(a);
                // HLSL�о���������洢
                return float2x2(c, -s, s, c);
            }

            
            float mag2(float2 p) { return dot(p, p); }
            
            float linstep(float mn, float mx, float x) {
                return clamp((x - mn) / (mx - mn), 0.0, 1.0);
            }
            
            float2 disp(float t) { 
                return float2(sin(t * 0.22), cos(t * 0.175)) * 2.0; 
            }
            
            // ������������
            float2 map(float3 p)
            {
                float3 p2 = p;
                p2.xy -= disp(p.z).xy;

                   // ������ת����Ӧ�õ�2D����
                float2x2 rotation = rot(sin(p.z + _Time.y * _Speed) * (0.1 + _CloudDensity * 0.05) + _Time.y * _Speed * 0.09);
                p.xy = mul(rotation, p.xy);
                
                //p.xy *= rot(sin(p.z + _Time.y) * (0.1 + _CloudDensity * 0.05) + _Time.y * 0.09);
                float cl = mag2(p2.xy);
                float d = 0.0;
                p *= 0.61;
                float z = 1.0;
                float trk = 1.0;
                float dspAmp = 0.1 + _CloudDensity * 0.2;
                
                [unroll]
                for (int i = 0; i < 5; i++)
                {
                    p += sin(p.zxy * 0.75 * trk + _Time.y * trk * 0.8) * dspAmp;
                    d -= abs(dot(cos(p), sin(p.yzx))) * z;
                    z *= 0.57;
                    trk *= 1.4;
                    p = mul(m3, p);
                }
                d = abs(d + _CloudDensity * 3.0) + _CloudDensity * 0.3 - 2.5  + bsMo.y;
                return float2(d + cl * 0.2 + 0.25, cl);
            }
            
            // ��Ⱦ����
            float4 render(float3 ro, float3 rd)
            {
                float4 rez = float4(0, 0, 0, 0);
                const float ldst = 8.0;
                float3 lpos = float3(disp(_Time.y + ldst) * 0.5, _Time.y + ldst);
                float t = 1.5;
                float fogT = 0.0;
            
                [loop]
                for (int i = 0; i < _iteration; i++) // ���ٵ��������������
                {
                    if (rez.a > 0.99) break;

                    float3 pos = ro + t * rd;
                    float2 mpv = map(pos);
                    float den = clamp(mpv.x - 0.3, 0.0, 1.0) * 1.12;
                    float dn = clamp((mpv.x + 2.0), 0.0, 3.0);
                    
                    float4 col = float4(0, 0, 0, 0);
                    if (mpv.x > 0.6)
                    {
                        //float3 baseCol = lerp(_ColorA, _ColorB, mpv.y * 0.1 + sin(pos.z * 0.4) * 0.5 + 1.8);
                     
                        //col = float4(saturate(baseCol), 0.08);
                         col = float4(sin(float3(5.0,0.4,0.2) + mpv.y*0.1 + sin(pos.z * 0.4)*0.5 + 1.8)*0.5 + 0.5,0.08);
                        col *= den * den * den;
                        col.rgb *= linstep(4.0, -2.5, mpv.x) * 2.3;
                        
                        float dif = clamp((den - map(pos + 0.8).x) / 9.0, 0.001, 1.0);
                        dif += clamp((den - map(pos + 0.35).x) / 2.5, 0.001, 1.0);
                        col.rgb *= den * (float3(0.005, 0.045, 0.075) + 1.5 * float3(0.033, 0.07, 0.03) * dif);
                    }
                    
                    float fogC = exp(t * 0.2 - 2.2);
                    col.rgba += float4(0.06, 0.11, 0.11, 0.1) * clamp(fogC - fogT, 0.0, 1.0);
                    fogT = fogC;
                    rez = rez + col * (1.0 - rez.a);
                    t += clamp(0.5 - dn * dn * 0.05, 0.09, 0.3);
                }
                return saturate(rez) * _CloudBrightness;
            }

            float getsat(float3 c)
            {
                float mi = min(min(c.x, c.y), c.z);
                float ma = max(max(c.x, c.y), c.z);
                return (ma - mi)/(ma+ 1e-7);
            }

            float3 iLerp(in float3 a, in float3 b, in float x)
            {
                float3 ic = lerp(a, b, x) + float3(1e-6,0.,0.);
                float sd = abs(getsat(ic) - lerp(getsat(a), getsat(b), x));
                float3 dir = normalize(float3(2.*ic.x - ic.y - ic.z, 2.*ic.y - ic.x - ic.z, 2.*ic.z - ic.y - ic.x));
                float lgt = dot(float3(1.0,1.0,1.0), ic);
                float ff = dot(dir, normalize(ic));
                ic += 1.5*dir*sd*ff*lgt;
                return clamp(ic,0.,1.);
            }

      
            
            // ������ɫ��
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
              
                // ������������
                float3 viewVector = mul(unity_CameraInvProjection, float4(v.uv * 2 - 1, 0, -1));
                o.ray = mul(unity_CameraToWorld, float4(viewVector,0));
                return o;
            }
            
            // ƬԪ��ɫ��
            float4 frag(v2f i) : SV_Target
            {
                bsMo = (_MousePos.xy - _ScreenParams.xy*0.5)/_ScreenParams.y;
                float2 q = i.uv;
                float2 p = (i.uv * 2.0 - 1.0) * float2(_ScreenParams.x / _ScreenParams.y, 1.0);
             
                float time = _Time.y * _CloudSpeed;
                float3 ro = float3(0, 0, time);
                ro.xy += disp(ro.z) * 0.85;
                ro.x -= bsMo.x*2;
                
                float tgtDst = 3.5;
                float3 target = normalize(ro - float3(disp(time + tgtDst) * 0.85, time + tgtDst));
                float3 rightdir = normalize(cross(target, float3(0, 1, 0)));
                float3 updir = normalize(cross(rightdir, target));
                float3 rd = normalize(p.x * rightdir + p.y * updir - target);
                rd.xy = mul(rd.xy, rot(-disp(time + 3.5).x * 0.2));

                //�ܶȶ�̬�仯
                //_CloudDensity = smoothstep(-0.4, 0.4,sin(_Time.y*0.3));

                float4 scn = render(ro, rd);
                float3 col = scn.rgb;
                
                col = iLerp(col.bgr, col.rgb, clamp(1.-_CloudDensity,0.05,1.));
                // ���ӶԱȶ�
                col = pow(col, float3(0.55, 0.65, 0.6)) * float3(1.0, 0.97, 0.9);
                col *= pow( 16.0*q.x*q.y*(1.0-q.x)*(1.0-q.y), 0.12)*0.7+0.3;
                
                // �����ӰЧ��
                float vignette = q.x * q.y * (1.0 - q.x) * (1.0 - q.y) * 16.0;
                vignette = pow(vignette, 0.12) * 0.7 + 0.3;
                col *= vignette;
                
                return float4(col, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}