Shader "UI/UI_PBR_Simple"
{
    Properties
    {
        [PerRendererData] _MainTex ("UI Texture (Albedo)", 2D) = "white" {}
        _Color ("Tint (Albedo + Alpha)", Color) = (1,1,1,1)
        
        // �򻯰��PBR����
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
        
        // ���ղ���������UI��
        _LightDirection ("Light Direction", Vector) = (0.3, 0.8, 0.5, 0)
        _LightIntensity ("Light Intensity", Range(0.0, 3.0)) = 1.0
        _AmbientColor ("Ambient Color", Color) = (0.2, 0.2, 0.2, 1)
        
        // UIģ�����
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite On
        ZTest Less
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI_PBR"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Smoothness;
            float _Metallic;
            float3 _LightDirection;
            float _LightIntensity;
            float4 _AmbientColor;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float3 lightDir : TEXCOORD3;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                
                // ��������ռ�����
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                o.lightDir = normalize(_LightDirection);
                
                return o;
            }

            // �򻯵�BRDF����
            float3 SimpleBRDF(float3 normal, float3 viewDir, float3 lightDir, float3 albedo, float smoothness, float metallic)
            {
                float NdotL = saturate(dot(normal, lightDir));
                float3 halfVec = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(normal, halfVec));
                
                // ����������
                float3 F0 = lerp(0.04, albedo, metallic);
                
                // �򻯵ľ��淴��
                float roughness = 1.0 - smoothness;
                float specular = pow(NdotH, 1.0 / max(roughness * roughness, 0.0001)) * smoothness;
                
                // ������ЧӦ
                float3 F = F0 + (1.0 - F0) * pow(1.0 - saturate(dot(viewDir, halfVec)), 5.0);
                
                // �����غ��BRDF
                float3 specularTerm = specular * F;
                float3 diffuseTerm = albedo * (1.0 - metallic) * (1.0 - F);
                
                return (diffuseTerm + specularTerm) * NdotL;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // ������������ɫ
                half4 texColor = tex2D(_MainTex, i.uv);
                float3 albedo = texColor.rgb * i.color.rgb;
                float alpha = texColor.a * i.color.a;
                
                // ��һ������
                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);
                float3 lightDir = normalize(i.lightDir);
                
                // ����ֱ�ӹ���
                float3 directLight = SimpleBRDF(normal, viewDir, lightDir, albedo, _Smoothness, _Metallic) * _LightIntensity;
                
                // ������й���
                float3 finalColor = _AmbientColor.rgb * albedo + directLight;
                
                #ifdef UNITY_UI_ALPHACLIP
                clip(alpha - 0.001);
                #endif
                
                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
    CustomEditor "UnityEditor.UIElements.UIShaderGUI"
}