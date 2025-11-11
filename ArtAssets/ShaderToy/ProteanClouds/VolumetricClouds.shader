// 1. ShaderLab 头部：定义 Shader 名称和类型
Shader "Custom/VolumetricClouds_Fixed"
{
    // 2. ShaderLab Properties：资源面板可配置的参数（纯 ShaderLab 语法）
    Properties
    {
        _NoiseTex1 ("Noise Texture 1 (256x256)", 2D) = "white" {}
        _DitherTex ("Dither Texture (1024x1024)", 2D) = "white" {}
        _NoiseTex3D ("3D Noise Texture (32x32x32)", 3D) = "white" {}
        _CloudSpeed ("Cloud Speed", Float) = 1.0
        _CloudBrightness ("Cloud Brightness", Float) = 1.0
        _ViewDistance ("Max View Distance", Float) = 60.0
    }

    // 3. ShaderLab SubShader：渲染管线配置（纯 ShaderLab 语法）
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "IgnoreProjector"="True" }
        LOD 100

        // 4. ShaderLab Pass：单个渲染通道（纯 ShaderLab 语法）
        Pass
        {
            // 5. CGPROGRAM 块：嵌入 CG/HLSL 代码（语法隔离）
            CGPROGRAM
            // 5.1 CG 编译指令（指定顶点/片段函数，纯 CG 语法）
            #pragma vertex vert
            #pragma fragment frag
            // 5.2 引入 Unity 内置 CG 库（纯 CG 语法）
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            // 5.3 绑定 ShaderLab Properties 到 CG 变量（纯 CG 语法）
            // 2D 纹理需声明 _TexName_ST（用于平铺/偏移）
            sampler2D _NoiseTex1;
            float4 _NoiseTex1_ST;
            sampler2D _DitherTex;
            float4 _DitherTex_ST;
            sampler3D _NoiseTex3D;
            float4 _NoiseTex3D_ST;
            // 数值参数直接声明
            float _CloudSpeed;
            float _CloudBrightness;
            float _ViewDistance;

            // 5.4 CG 宏定义（仅在 CG 块内生效，纯 CG 语法）
            #define LOOK 1          // 0=日落风格，1=明亮风格
            #define NOISE_METHOD 1  // 0=3D噪声，1=2D硬件插值，2=2D软件插值
            #define USE_LOD 1       // 0=无LOD，1=有LOD
            #define kDiv 1          // 质量系数（越大越清晰，性能越低）

            // 太阳方向（关联 Unity 平行光，纯 CG 语法）
            #if LOOK == 0
                #define sundir normalize(_WorldSpaceLightPos0.xyz)
            #else
                #define sundir normalize(float3(-0.7071, 0.0, -0.7071))
            #endif

            // 5.5 CG 工具函数：相机矩阵计算（纯 CG 语法）
            float3x3 setCamera(float3 ro, float3 ta, float cr)
            {
                float3 cw = normalize(ta - ro);
                float3 cp = float3(sin(cr), cos(cr), 0.0);
                float3 cu = normalize(cross(cw, cp));
                float3 cv = normalize(cross(cu, cw));
                return float3x3(cu, cv, cw); // Unity CG 中矩阵用 float3x3，非 mat3
            }

            // 5.6 CG 工具函数：噪声采样（纯 CG 语法）
            float noise(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f); // 平滑插值

                #if NOISE_METHOD == 0
                    // 3D 噪声采样（CG 用 tex3Dlod，非 textureLod）
                    x = p + f;
                    return tex3Dlod(_NoiseTex3D, float4((x + 0.5) / 32.0, 0.0)).x * 2.0 - 1.0;
                #elif NOISE_METHOD == 1
                    // 2D 硬件插值（CG 用 tex2Dlod，非 textureLod）
                    float2 uv = (p.xy + float2(37.0, 239.0) * p.z) + f.xy;
                    uv = TRANSFORM_TEX(uv, _NoiseTex1); // CG 内置宏：应用纹理平铺/偏移
                    float2 rg = tex2Dlod(_NoiseTex1, float4(uv, 0.0, 0.0)).yx;
                    return lerp(rg.x, rg.y, f.z) * 2.0 - 1.0; // CG 用 lerp，非 mix（mix 也兼容，但 lerp 更标准）
                #else
                    // 2D 软件插值（CG 用 tex2D，纯 CG 语法）
                    int3 q = int3(p);
                    int2 uv = q.xy + int2(37, 239) * q.z;
                    uv = uv & 255; // 纹理大小 256x256，取模避免越界
                    float2 rg = lerp(
                        lerp(tex2D(_NoiseTex1, uv / 256.0), tex2D(_NoiseTex1, (uv + int2(1,0)) / 256.0), f.x),
                        lerp(tex2D(_NoiseTex1, (uv + int2(0,1)) / 256.0), tex2D(_NoiseTex1, (uv + int2(1,1)) / 256.0), f.x),
                        f.y
                    ).yx;
                    return lerp(rg.x, rg.y, f.z) * 2.0 - 1.0;
                #endif
            }

            // 5.7 CG 函数：云密度计算（纯 CG 语法）
            #if LOOK == 0
                float map(float3 p, int oct)
                {
                    // 云流动：用 _Time.y（Unity 时间）替代 ShaderToy 的 iTime
                    float3 q = p - float3(0.0, 0.1, 1.0) * _Time.y * _CloudSpeed;
                    float g = 0.5 + 0.5 * noise(q * 0.3);

                    float f;
                    f  = 0.50000 * noise(q); q = q * 2.02;
                    #if USE_LOD == 1
                        if (oct >= 2)
                    #endif
                        f += 0.25000 * noise(q); q = q * 2.23;
                    #if USE_LOD == 1
                        if (oct >= 3)
                    #endif
                        f += 0.12500 * noise(q); q = q * 2.41;
                    #if USE_LOD == 1
                        if (oct >= 4)
                    #endif
                        f += 0.06250 * noise(q); q = q * 2.62;
                    #if USE_LOD == 1
                        if (oct >= 5)
                    #endif
                        f += 0.03125 * noise(q);

                    f = lerp(f * 0.1 - 0.5, f, g * g);
                    return 1.5 * f - 0.5 - p.y;
                }
            #else
                // 明亮风格：4个 LOD 密度函数（纯 CG 语法）
                float map5(float3 p)
                {
                    float3 q = p - float3(0.0, 0.1, 1.0) * _Time.y * _CloudSpeed;
                    float f;
                    f  = 0.50000 * noise(q); q = q * 2.02;
                    f += 0.25000 * noise(q); q = q * 2.03;
                    f += 0.12500 * noise(q); q = q * 2.01;
                    f += 0.06250 * noise(q); q = q * 2.02;
                    f += 0.03125 * noise(q);
                    return saturate(1.5 - p.y - 2.0 + 1.75 * f); // CG 用 saturate，非 clamp（clamp 也兼容，saturate 更简洁）
                }

                float map4(float3 p)
                {
                    float3 q = p - float3(0.0, 0.1, 1.0) * _Time.y * _CloudSpeed;
                    float f;
                    f  = 0.50000 * noise(q); q = q * 2.02;
                    f += 0.25000 * noise(q); q = q * 2.03;
                    f += 0.12500 * noise(q); q = q * 2.01;
                    f += 0.06250 * noise(q);
                    return saturate(1.5 - p.y - 2.0 + 1.75 * f);
                }

                float map3(float3 p)
                {
                    float3 q = p - float3(0.0, 0.1, 1.0) * _Time.y * _CloudSpeed;
                    float f;
                    f  = 0.50000 * noise(q); q = q * 2.02;
                    f += 0.25000 * noise(q); q = q * 2.03;
                    f += 0.12500 * noise(q);
                    return saturate(1.5 - p.y - 2.0 + 1.75 * f);
                }

                float map2(float3 p)
                {
                    float3 q = p - float3(0.0, 0.1, 1.0) * _Time.y * _CloudSpeed;
                    float f;
                    f  = 0.50000 * noise(q);
                    q = q * 2.02; f += 0.25000 * noise(q);
                    return saturate(1.5 - p.y - 2.0 + 1.75 * f);
                }
            #endif

            // 5.8 CG 函数：Raymarch 体积云（核心，纯 CG 语法）
            #if LOOK == 0
                float4 raymarch(float3 ro, float3 rd, float3 bgcol, int2 px)
                {
                    // 云的上下边界
                    const float yb = -3.0;
                    const float yt = 0.6;
                    float tb = (yb - ro.y) / rd.y;
                    float tt = (yt - ro.y) / rd.y;

                    // 计算 Raymarch 有效范围
                    float tmin, tmax;
                    if (ro.y > yt)
                    {
                        if (tt < 0.0) return float4(0.0, 0.0, 0.0, 0.0); // 不在云范围内，直接返回
                        tmin = tt;
                        tmax = tb;
                    }
                    else
                    {
                        tmin = 0.0;
                        tmax = _ViewDistance;
                        if (tt > 0.0) tmax = min(tmax, tt);
                        if (tb > 0.0) tmax = min(tmax, tb);
                    }

                    // 抖动采样（抗锯齿）
                    float t = tmin + 0.1 * tex2D(_DitherTex, (px & 1023) / 1024.0).x;

                    float4 sum = float4(0.0, 0.0, 0.0, 0.0);
                    // 强制循环展开：避免 Unity 编译错误（CG 用 [unroll(n)]）
                    [unroll(200)]
                    for (int i = 0; i < 190 * kDiv; i++)
                    {
                        float dt = max(0.05, 0.02 * t / float(kDiv));

                        // LOD 等级计算
                        #if USE_LOD == 0
                            const int oct = 5;
                        #else
                            int oct = 5 - int(log2(1.0 + t * 0.5));
                        #endif

                        // 采样云密度
                        float3 pos = ro + t * rd;
                        float den = map(pos, oct);
                        if (den > 0.01)
                        {
                            // 光照计算：方向导数求阴影
                            float dif = saturate((den - map(pos + 0.3 * sundir, oct)) / 0.25);
                            float3 lin = float3(0.65, 0.65, 0.75) * 1.1 + 0.8 * float3(1.0, 0.6, 0.3) * dif;
                            float4 col = float4(lerp(float3(1.0, 0.93, 0.84), float3(0.25, 0.3, 0.4), den), den);

                            // 应用亮度和雾效
                            col.xyz *= lin * _CloudBrightness;
                            col.xyz = lerp(col.xyz, bgcol, 1.0 - exp2(-0.1 * t));
                            col.w = min(col.w * 8.0 * dt, 1.0);
                            col.rgb *= col.a;
                            sum += col * (1.0 - sum.a); // 前向混合
                        }

                        // 推进射线
                        t += dt;
                        if (t > tmax || sum.a > 0.99) break;
                    }

                    return saturate(sum);
                }
            #else
                // 明亮风格：用 CG 宏简化 Raymarch 循环（纯 CG 语法）
                #define MARCH(STEPS, MAPLOD) \
                    for (int i = 0; i < STEPS; i++) { \
                        float3 pos = ro + t * rd; \
                        if (pos.y < -3.0 || pos.y > 2.0 || sum.a > 0.99) break; \
                        float den = MAPLOD(pos); \
                        if (den > 0.01) { \
                            float dif = saturate((den - MAPLOD(pos + 0.3 * sundir)) / 0.6); \
                            float3 lin = float3(1.0, 0.6, 0.3) * dif + float3(0.91, 0.98, 1.05); \
                            float4 col = float4(lerp(float3(1.0, 0.95, 0.8), float3(0.25, 0.3, 0.35), den), den); \
                            col.xyz *= lin * _CloudBrightness; \
                            col.xyz = lerp(col.xyz, bgcol, 1.0 - exp(-0.003 * t * t)); \
                            col.w *= 0.4; \
                            col.rgb *= col.a; \
                            sum += col * (1.0 - sum.a); \
                        } \
                        t += max(0.06, 0.05 * t); \
                    }

                float4 raymarch(float3 ro, float3 rd, float3 bgcol, int2 px)
                {
                    float4 sum = float4(0.0, 0.0, 0.0, 0.0);
                    float t = 0.05 * tex2D(_DitherTex, (px & 255) / 1024.0).x;
                    
                    // 分 LOD 采样
                    MARCH(40, map5);
                    MARCH(40, map4);
                    MARCH(30, map3);
                    MARCH(30, map2);
                    
                    return saturate(sum);
                }
            #endif

            // 5.9 CG 函数：渲染主逻辑（纯 CG 语法）
            float4 render(float3 ro, float3 rd, int2 px)
            {
                float sun = saturate(dot(sundir, rd));
                float3 bgcol;

                // 计算背景天空色
                #if LOOK == 0
                    bgcol = float3(0.76, 0.75, 0.95);
                    bgcol -= 0.6 * float3(0.90, 0.75, 0.95) * rd.y;
                    bgcol += 0.2 * float3(1.00, 0.60, 0.10) * pow(sun, 8.0);
                #else
                    bgcol = float3(0.6, 0.71, 0.75) - rd.y * 0.2 * float3(1.0, 0.5, 1.0) + 0.15 * 0.5;
                    bgcol += 0.2 * float3(1.0, 0.6, 0.1) * pow(sun, 8.0);
                #endif

                // 渲染云并混合背景
                float4 cloudCol = raymarch(ro, rd, bgcol, px);
                float3 finalCol = lerp(bgcol, cloudCol.xyz, cloudCol.w);

                // 太阳眩光
                #if LOOK == 0
                    finalCol += 0.2 * float3(1.0, 0.4, 0.2) * pow(sun, 3.0);
                    finalCol = smoothstep(0.15, 1.1, finalCol); // 色调映射
                #else
                    finalCol += float3(0.2, 0.08, 0.04) * pow(sun, 3.0);
                #endif

                return float4(finalCol, 1.0);
            }

            // 5.10 CG 顶点输入结构（纯 CG 语法）
            struct appdata
            {
                float4 vertex : POSITION; // 模型空间顶点位置
                float2 uv : TEXCOORD0;    // 模型 UV
            };

            // 5.11 CG 顶点输出结构（纯 CG 语法）
            struct v2f
            {
                float2 uv : TEXCOORD0;       // 传递 UV 到片段着色器
                float4 vertex : SV_POSITION; // 裁剪空间顶点位置
                float3 worldPos : TEXCOORD1; // 世界空间顶点位置
                float3 worldCamPos : TEXCOORD2; // 世界空间相机位置
            };

            // 5.12 CG 顶点着色器（纯 CG 语法）
            v2f vert (appdata v)
            {
                v2f o;
                // 模型空间 → 裁剪空间（Unity 内置宏）
                o.vertex = UnityObjectToClipPos(v.vertex);
                // 模型空间 → 世界空间（Unity 内置宏）
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                // 获取 Unity 相机世界位置（内置变量）
                o.worldCamPos = _WorldSpaceCameraPos;
                o.uv = v.uv;
                return o;
            }

            // 5.13 CG 片段着色器（纯 CG 语法，最终输出颜色）
            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 计算屏幕坐标（类似 ShaderToy 的 fragCoord）
                float2 fragCoord = i.uv * _ScreenParams.xy; // _ScreenParams.xy = 屏幕分辨率
                float2 p = (2.0 * fragCoord - _ScreenParams.xy) / _ScreenParams.y; // 归一化坐标

                // 2. 相机参数（世界空间）
                float3 ro = i.worldCamPos; // 相机位置
                float3 ta = float3(0.0, -1.0, 0.0); // 相机目标点（可自定义）
                float cr = 0.07 * cos(0.25 * _Time.y); // 相机滚动角（随时间动画）

                // 3. 计算相机矩阵和射线方向
                float3x3 camMat = setCamera(ro, ta, cr);
                float3 rd = mul(camMat, normalize(float3(p.xy, 1.5))); // 射线方向（CG 矩阵乘法：mul(矩阵, 向量)）

                // 4. 调用渲染函数，输出最终颜色
                float4 finalCol = render(ro, rd, int2(fragCoord - 0.5));
                return fixed4(finalCol.rgb, 1.0); // 转换为 Unity 固定精度颜色
            }

            // 6. 结束 CG 代码块（纯 ShaderLab 语法）
            ENDCG
        }
    }

    // 7. ShaderLab FallBack：降级方案（纯 ShaderLab 语法）
    FallBack "Diffuse"
}