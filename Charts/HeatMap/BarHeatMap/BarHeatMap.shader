// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Unlit/BarHeatMap"
{
        Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _clip("裁剪值",float) = 0.2
        _maxAlpha("最高点透明度",Range(0.1,1)) = 0.7
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}
     //   Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase 
            #include "UnityCG.cginc"
	        #include "Lighting.cginc"
         //   #include "AutoLight.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
	            float3 normal:NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal:TEXCOORD1;
	            // SHADOW_COORDS(3)
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float _clip;
            float _maxAlpha;
            float _Diffuse;

            // SRP 中，可以使用一个 UnityPerMaterial 再裹一层
            // 总之这些潜规则要熟悉 Unity 才能知道
            //  - https://forum.unity.com/threads/materialpropertyblock-with-shader-graph.697868/
            // CBUFFER_START(UnityPerMaterial)
                UNITY_INSTANCING_BUFFER_START(MyInstancing)
                    UNITY_DEFINE_INSTANCED_PROP(float, weight)
                UNITY_INSTANCING_BUFFER_END(MyInstancing)
            // CBUFFER_END

            // unity 的 instancing 参考下面的连接内容
            // jave.lin : refer to : 
            //  - https://docs.unity3d.com/Manual/GPUInstancing.html
            // UNITY_INSTANCING_BUFFER_START(MyInstancing)
            //     UNITY_DEFINE_INSTANCED_PROP(float4x4, _MMat)
            //     UNITY_DEFINE_INSTANCED_PROP(half4, _Color)
            // UNITY_INSTANCING_BUFFER_END(MyInstancing)

            v2f vert (appdata v, uint instanceID : SV_InstanceID)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v)
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.worldNormal =mul(v.normal,unity_WorldToObject);
                //TRANSFER_SHADOW(o); // 填充阴影坐标，根据平台自动调整
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                //阴影
                //fixed shadow = SHADOW_ATTENUATION(i);
                //光照
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
	            fixed3 worldNormal = normalize(i.worldNormal);
	            fixed3 worldLight = normalize(_WorldSpaceLightPos0);

                //映射颜色
                float r = UNITY_ACCESS_INSTANCED_PROP(MyInstancing, weight);
                 fixed4 col = tex2D(_MainTex, float2(r,0.5));
                  fixed3 diffuse = _LightColor0.rgb*col.rgb*saturate(dot(worldNormal,worldLight));
                  fixed3 color = diffuse + ambient;
	             return fixed4(color,1.0) ;

                //col.a = lerp(0,_maxAlpha,i.uv.y)*r;
                //clip(col.a - _clip);
                //return col;
            }
            ENDCG
        }
    }
}