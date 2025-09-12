Shader "Unlit/Button3DShader_SSS_BRDF"
{
    Properties {
        _FieldOfView ("Field Of View", Range(1, 120)) = 45
        _Ambient ("Ambient Color", Color) = (0.2, 0.2, 0.2, 1)
        _Diffuse ("Albedo (Diffuse Color + Alpha)", Color) = (0.7, 0.2, 0.2, 1) // 漫反射色=Albedo，Alpha控制整体透明度
        _SSSSamples ("SSS Sample Count (采样次数)", Range(3, 16)) = 5
        _SSSColor ("SSS Tint (散射色调)", Color) = (1.0, 0.8, 0.6, 1.0)
        _SSSRadius ("SSS Radius (散射半径)", Range(0.01, 0.5)) = 0.1
        _SSSStrength ("SSS Strength (散射强度)", Range(0.0, 3.0)) = 1.0
        _SSSAttenuation ("SSS Attenuation (散射衰减)", Range(0.1, 5.0)) = 2.0
        _SSSMode ("SSS Mode (散射模式)", Range(0, 3)) = 2
        _Roughness ("Roughness (粗糙度)", Range(0.01, 0.99)) = 0.3 // BRDF粗糙度（越小越光滑）
        _Metallic ("Metallic (金属度)", Range(0.0, 1.0)) = 0.0 // BRDF金属度（1=金属，0=非金属）
        
        _BoxSize ("Cube Size (立方体尺寸)", vector) = (1,1,1,1)
        _BoxRound ("Corner Radius (圆角半径)", float) = 1.0
        _ViewPos ("Eye Position (相机位置)", vector) = (0,0,0,0)
        _LightPos ("Main Light Position (主光源位置)", vector) = (0,0,0,0)
        _AnchorX ("X Anchor (X轴锚点)", Range(-1,1.0)) = 0.0
        _AnchorY ("Y Anchor (Y轴锚点)", Range(-1,1)) = 0.0
        _AnchorZ ("Z Anchor (Z轴锚点)", Range(-1,1)) = 0.0
        _Interaion ("Raymarch Steps (光线步进次数)", int) = 20
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        ZWrite Off // 透明物体关闭深度写入
        Blend SrcAlpha OneMinusSrcAlpha // 标准透明混合
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma target 3.0 // 支持高级数学函数
            
            // 全局常量
            #define MIN_DIST 0.0
            #define MAX_DIST 100.0
            #define EPSILON 0.0001
            #define PI 3.141592653589793
            
            // 外部参数（Properties映射）
            float _FieldOfView;
            float4 _Ambient;
            float4 _Diffuse;
            float _SSSRadius;
            int _SSSSamples;
            float _SSSStrength;
            float4 _SSSColor;
            float _SSSAttenuation;
            float _SSSMode;
            float _Roughness;
            float _Metallic;
            vector _BoxSize;
            float _BoxRound;
            vector _ViewPos;
            int _Interaion;
            vector _LightPos;
            float _AnchorX;
            float _AnchorY;
            float _AnchorZ;

            // -------------------------- 1. SDF 工具函数 --------------------------
            // 圆角立方体SDF（原逻辑保留）
            float sdRoundBox(float3 p, float3 b, float r) {
                float3 q = abs(p) - b + r;
                return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0) - r;
            }

            // 场景SDF（仅圆角立方体，原逻辑保留）
            float sceneSDF(float3 samplePoint) {
                return sdRoundBox(samplePoint, _BoxSize.xyz, _BoxRound);
            }

            // -------------------------- 2. 光线步进工具 --------------------------
            // 光线步进求表面最近距离（原逻辑保留）
            float shortestDistanceToSurface(float3 eye, float3 marchingDirection, float start, float end) {
                float depth = start;
                for (int i = 0; i < _Interaion; i++) {
                    float dist = sceneSDF(eye + depth * marchingDirection);
                    if (dist < EPSILON) return depth; // 命中表面
                    depth += dist;
                    if (depth >= end) return end; // 超出最大距离
                }
                return end;
            }

            // 计算光线方向（原逻辑保留）
            float3 rayDirection(float fieldOfView, float2 size, float2 fragCoord) {
                float2 xy = fragCoord - size * 0.5;
                float z = size.y / tan(radians(fieldOfView) * 0.5);
                return normalize(float3(xy, -z));
            }

            // 估算表面法向量（原逻辑保留，保证光照精度）
            float3 estimateNormal(float3 p) {
                return normalize(float3(
                    sceneSDF(float3(p.x + EPSILON, p.y, p.z)) - sceneSDF(float3(p.x - EPSILON, p.y, p.z)),
                    sceneSDF(float3(p.x, p.y + EPSILON, p.z)) - sceneSDF(float3(p.x, p.y - EPSILON, p.z)),
                    sceneSDF(float3(p.x, p.y, p.z + EPSILON)) - sceneSDF(float3(p.x, p.y, p.z - EPSILON))
                ));
            }

            float hash31(float3 p) {
                  // 1. 浮点数转无符号整数（HLSL用asuint，替代GLSL的floatBitsToUint）
                  uint3 pu = asuint(p);
                  
                  // 2. 位运算逻辑（保留原计算，仅将uvec3改为uint3）
                  pu = 1664525U * ((pu >> 2U) ^ (pu.yzx >> 1U) ^ pu.zxy);
                  uint h32 = 1103515245U * (((pu.x) ^ (pu.y >> 3U)) ^ (pu.z >> 6U));
                  
                  // 3. 截取31位（避免符号位影响，确保随机值为正）
                  uint n = h32 ^ (h32 >> 16U); // 右移16位，异或自身，混合高位与低位
                  n = n & 0x7fffffffU; // 截取低31位（0x7fffffff是31位全1，确保值为正）
                  
                  // 4. 归一化到[0,1)区间
                  return float(n) / float(0x7fffffffU);
                  }

            // -------------------------- 3. SSS 计算（改进的次表面散射） --------------------------
            // 沿光源方向采样，计算散射强度（改进的Poisson风格，更真实的散射效果）
            float sssCalculation(float3 p, float3 normal, float3 lightDir) {
                float sss = 0.0;
                float3 sampleDir = normalize(lightDir); // 沿光源方向向内采样（模拟光线穿透）
                float3 startP = p - normal * 0.01; // 从表面向内偏移，避免采样表面
                
                for (int i = 0; i < _SSSSamples; i++) {
                    // 改进的随机采样距离（使用更好的噪声函数）
                    float rnd = hash31(p + float(i) + _Time.x * 0.1);
                    float d = float(i) * _SSSRadius * (1.0 + rnd * 0.5);
                    float3 sampleP = startP + sampleDir * d;
                    
                    float distToSurface = sceneSDF(sampleP);
                    
                    // 距离权重（距离越远权重越低）
                    float weight = 1.0 - (float(i) / float(_SSSSamples));
                    weight = pow(weight, _SSSAttenuation); // 可调节的衰减曲线
                    
                    // 散射强度计算（改进的公式）
                    float scatter = clamp(distToSurface / (d + 0.001), 0.0, 1.0);
                    sss += scatter * weight;
                }
                
                // 归一化+强度控制，确保散射值在0~1
                sss = smoothstep(0.0, 1.0, sss / float(_SSSSamples) * _SSSStrength * 2.0);
                return sss;
            }

            float subsurface(float3 ro, float3 rd, float ra) 
            {
                        const int sN = _SSSSamples;  // 采样次数（平衡效果与性能）
                        float sss = 0.;     // SSS累积值
                        float weightSum = 0.0;
                        
                        // 从表面向光线方向随机步进，累积加权值
                        for (int i = 0; i < sN; i++){
                            // 改进的随机递增采样距离
                            float rnd = hash31(ro + float(i) + _Time.x * 0.05) * 0.3;
                            float d = float(i) * ra * (1.0 + rnd); 
                            
                            // 距离权重（距离越远权重越低）
                            float weight = 1.0 - (float(i) / float(sN));
                            weight = pow(weight, _SSSAttenuation); // 可调节的衰减曲线
                            
                            // 累积加权采样值
                            float sampleValue = clamp(sceneSDF(ro + rd * d) / (d + 0.001), 0.0, 1.0);
                            sss += sampleValue * weight;
                            weightSum += weight;
                        }
                        
                        sss = weightSum > 0.0 ? sss / weightSum : 0.0;  // 加权平均
                        
                        // 用平滑步长函数调整分布，使其更接近钟形曲线
                        return smoothstep(0.0, 1.0, sss * _SSSStrength); 
             }
             float3 hash33(float3 p)
             {
                 // 缩放输入并取小数部分，打破原始分布
                 p = frac(p * float3(0.5031, 0.6030, 0.4973));
                 
                 // 通过点积运算增强随机性，引入常量偏移避免模式重复
                 p += dot(p, p.yxz + 142.5453);
                 
                 // 混合分量并取小数部分，生成最终随机向量
                 return frac((p.xxy + p.yxx) * p.zyx);
             }

             float subsurface1(in float3 p, in float3 rd, float ra){
               float occ = 0.;     // 散射累积值
               float weightSum = 0.0;
               float i0 = hash31(p + rd) * ra;  // 初始随机偏移（避免采样同步）
               
               // 使用可配置的采样次数
               int sampleCount = min(_SSSSamples, 16);
               for( int i = 0; i < sampleCount; i++){
                   // 采样距离（初始偏移+递增距离）
                   float h = i0 + float(i) * ra;
                   
                   // 距离权重
                   float weight = 1.0 - (float(i) / float(sampleCount));
                   weight = pow(weight, _SSSAttenuation); // 可调节的衰减曲线
                   
                   // 改进的分散散射（随机方向，更真实）
                   float v = h + float(i) + _Time.x * 0.1;
                   float3 dir = normalize(hash33(p + float3(v, v, v))) - float3(0.5, 0.5, 0.5);
                   
                   // 确保方向与光线方向一致（控制散射方向）
                   dir *= sign(dot(dir, rd));
                   dir = normalize(dir);
                   
                   // 累积散射值（距离与场景距离的差值）
                   float sampleValue = (h - sceneSDF(p - h * dir));
                   occ += sampleValue * weight;
                   weightSum += weight;
               }
               
               // 归一化并平滑处理，返回SSS强度
               occ = weightSum > 0.0 ? occ / weightSum : 0.0;
               return smoothstep(0.0, 1.0, 1.0 - occ / 3.0 * _SSSStrength);     
           }

            // 高级SSS计算（基于物理的次表面散射）
            float advancedSSS(float3 p, float3 normal, float3 lightDir) {
                float3 sampleDir = normalize(lightDir);
                float3 startP = p - normal * 0.02; // 从表面向内偏移
                
                float sss = 0.0;
                float weightSum = 0.0;
                
                for (int i = 0; i < _SSSSamples; i++) {
                    // 分层采样（模拟光线在材质中的传播）
                    float layer = float(i) / float(_SSSSamples - 1);
                    float rnd = hash31(p + float(i) + _Time.x * 0.05) * 0.4;
                    
                    // 采样距离（非线性分布，更符合物理）
                    float d = _SSSRadius * (layer * layer + rnd * 0.3);
                    float3 sampleP = startP + sampleDir * d;
                    
                    float distToSurface = sceneSDF(sampleP);
                    
                    // 物理衰减（指数衰减）
                    float attenuation = exp(-d * _SSSAttenuation);
                    
                    // 散射强度（基于距离和衰减）
                    float scatter = clamp(distToSurface / (d + 0.001), 0.0, 1.0);
                    float weight = attenuation * (1.0 - layer);
                    
                    sss += scatter * weight;
                    weightSum += weight;
                }
                
                // 归一化并应用强度
                sss = weightSum > 0.0 ? sss / weightSum : 0.0;
                return smoothstep(0.0, 1.0, sss * _SSSStrength);
            }

            // SSS混合函数（根据模式选择不同的SSS算法）
            float getSSS(float3 p, float3 normal, float3 lightDir) {
                float mode = _SSSMode;
                
                if (mode < 1.0) {
                    return sssCalculation(p, normal, lightDir);
                } else if (mode < 2.0) {
                    return subsurface(p - normal * 0.005, lightDir, _SSSRadius);
                } else if (mode < 3.0) {
                    return subsurface1(p, lightDir, _SSSRadius);
                } else {
                    return advancedSSS(p, normal, lightDir);
                }
            }

            // -------------------------- 4. Cook-Torrance BRDF（基于物理的光照） --------------------------
            // 1. 微表面分布（GGX模型：模拟粗糙表面的法线分布）
            float D_GGX(float NdotH, float roughness) {
                float a = roughness * roughness;
                float a2 = a * a;
                float denom = NdotH * NdotH * (a2 - 1.0) + 1.0;
                denom = PI * denom * denom;
                return a2 / denom;
            }

            // 2. 几何遮挡（Schlick-GGX模型：模拟微表面对光线的遮挡）
            float G_SchlickGGX(float NdotV, float roughness) {
                float r = (roughness + 1.0);
                float k = (r * r) / 8.0;
                float denom = NdotV * (1.0 - k) + k;
                return NdotV / denom;
            }

            // 3. 双向几何遮挡（同时考虑视线和光线方向）
            float G_Smith(float NdotV, float NdotL, float roughness) {
                float gView = G_SchlickGGX(NdotV, roughness);
                float gLight = G_SchlickGGX(NdotL, roughness);
                return gView * gLight;
            }

            // 4. 菲涅尔效应（Schlick模型：模拟不同角度的反射率变化）
            float3 F_Schlick(float HdotV, float3 F0) {
                return F0 + (1.0 - F0) * pow(clamp(1.0 - HdotV, 0.0, 1.0), 5.0);
            }

            // 5. 完整Cook-Torrance BRDF（漫反射 + 镜面反射）
            float3 cookTorranceBRDF(float3 N, float3 V, float3 L, float3 albedo) {
                float3 H = normalize(V + L); // 半向量（视线与光线的中间方向）
                
                // 核心点积（clamp避免负数值）
                float NdotV = clamp(dot(N, V), 0.001, 1.0);
                float NdotL = clamp(dot(N, L), 0.001, 1.0);
                float NdotH = clamp(dot(N, H), 0.001, 1.0);
                float HdotV = clamp(dot(H, V), 0.001, 1.0);
                
                // F0（基础反射率）：金属用Albedo，非金属用固定0.04
                float3 F0 = lerp(float3(0.04, 0.04, 0.04), albedo, _Metallic);
                
                // BRDF三组件
                float D = D_GGX(NdotH, _Roughness);
                float G = G_Smith(NdotV, NdotL, _Roughness);
                float3 F = F_Schlick(HdotV, F0);
                
                // 镜面反射项（能量守恒）
                float3 specular = (D * G * F) / (4.0 * NdotV * NdotL + EPSILON);
                
                // 漫反射项（金属无漫反射，非金属用Lambert）
                float3 diffuse = albedo / PI * (1.0 - F) * (1.0 - _Metallic);
                
                // 总BRDF = (漫反射 + 镜面) * 光线入射角度（受光面才有效）
                return (diffuse + specular) * NdotL;
            }

            // -------------------------- 5. 总光照计算（BRDF + SSS） --------------------------
            float3 brdfSSSIllumination(float3 albedo, float3 eye, float3 p) {
                float3 N = estimateNormal(p); // 表面法向量
                float3 V = normalize(eye - p); // 视线方向（从点到相机）
                float3 totalColor = 0.0;

                // 1. 环境光（金属用F0，非金属用Albedo，符合物理）
                float3 F0 = lerp(float3(0.04, 0.04, 0.04), albedo, _Metallic);
                totalColor += _Ambient.rgb * lerp(albedo, F0, _Metallic);

                // 2. 动态光源1：旋转辅助光源（原逻辑保留，增强层次感）
                float3 light1Pos = float3(4.0 * sin(_Time.y), 2.0, 4.0 * cos(_Time.y));
                light1Pos += _ViewPos.xyz + float3(0, _ViewPos.y, 0);
                float3 light1Intensity = float3(0.4, 0.4, 0.4);
                float3 L1 = normalize(light1Pos - p);
                // 叠加BRDF
                totalColor += cookTorranceBRDF(N, V, L1, albedo) * light1Intensity;
                // 叠加SSS（仅受光面有效）
                float NdotL1 = clamp(dot(N, L1), 0.0, 1.0);
                if (NdotL1 > EPSILON) {
                    float sss = getSSS(p, N, L1); // 使用可选择的SSS函数
                    totalColor += _SSSColor.rgb * sss * light1Intensity * (1.0 - _Metallic);
                }
                

                // 3. 动态光源2：用户指定主光源（_LightPos）
                float3 light2Pos = _LightPos.xyz;
                float3 light2Intensity = float3(1.0, 1.0, 1.0) * 0.6;
                float3 L2 = normalize(light2Pos - p);
                // 叠加BRDF
                totalColor += cookTorranceBRDF(N, V, L2, albedo) * light2Intensity;
                // 叠加SSS（仅受光面有效）
                //float NdotL2 = clamp(dot(N, L2), 0.0, 1.0);
                //if (NdotL2 > EPSILON) {
                //    totalColor += _SSSColor.rgb * sssCalculation(p, N, L2) * light2Intensity * (1.0 - _Metallic);
                //}

                return totalColor;
            }

            // -------------------------- 6. 视图矩阵（原逻辑保留） --------------------------
            float4x4 viewMatrix(float3 eye, float3 center, float3 up) {
                float3 f = normalize(center - eye); // 前向向量（相机→目标）
                float3 u = normalize(cross(f, float3(1, 0, 0))); // 上向向量
                float3 s = cross(u, f); // 右向向量
                return float4x4(
                    float4(s, 0),
                    float4(u, 0.0),
                    float4(-f, 0.0),
                    float4(0, 0, 0, 1)
                );
            }

            // -------------------------- 7. 顶点/碎片着色器（原结构保留） --------------------------
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // 顶点变换到裁剪空间
                o.uv = v.uv; // 传递UV用于碎片着色
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 1. 屏幕坐标与相机参数
                float2 resolution = _ScreenParams.xy;
                float2 fragCoord = i.uv * resolution;
                float3 eye = _ViewPos.xyz; // 相机位置
                float3 center = float3(_BoxSize.x*_AnchorX, _BoxSize.y*_AnchorY, _BoxSize.z*_AnchorZ); // 立方体锚点
                float3 up = float3(0, 1, 0); // 上向向量

                // 2. 计算视图矩阵与光线方向
                float4x4 viewToWorld = viewMatrix(eye, center, up); // 视图→世界矩阵
                float3 viewDir = rayDirection(_FieldOfView, resolution, fragCoord); // 视图空间光线方向
                float3 worldDir = mul(viewToWorld, float4(viewDir, 0.0)).xyz; // 转换到世界空间

                // 3. 光线步进求命中点
                float dist = shortestDistanceToSurface(eye, worldDir, MIN_DIST, MAX_DIST);
                if (dist > MAX_DIST - EPSILON) {
                    return fixed4(0, 0, 0, 0); // 未命中物体，返回透明
                }
                float3 p = eye + dist * worldDir; // 命中点坐标

                // 4. 计算BRDF+SSS光照颜色
                float3 albedo = _Diffuse.rgb; // Albedo=漫反射色（非金属）/金属色（金属）
                float3 finalColor = brdfSSSIllumination(albedo, eye, p);

                // 5. 透明度控制（用_Diffuse的Alpha通道）
                float alpha = _Diffuse.a;

                // 6. 输出最终颜色
                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
}