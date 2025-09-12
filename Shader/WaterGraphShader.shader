Shader "UI/WaterGraph"
 {
     Properties
     {
         [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
         _Color ("Tint", Color) = (1,1,1,1)
         _height("高度",Range(0,1)) = 0.5
         _length("频率",float) = 1
         _am("振幅",Range(0,0.3)) = 0.1
         _speed ("速度",float) =1
         _lineColor("描边颜色",color) = (1,1,1,1)
         _lineWidth("边缘宽度",Range(0,0.1)) = 0.05
         _fadeOutWidth("抗锯齿过度距离",Range(0,0.1)) = 0.05
 
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
         ZWrite Off
         ZTest [unity_GUIZTestMode]
         Blend SrcAlpha OneMinusSrcAlpha
         ColorMask [_ColorMask]
 
         Pass
         {
             Name "Default"
         CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma target 2.0
 
             #include "UnityCG.cginc"
             #include "UnityUI.cginc"
 
             #pragma multi_compile __ UNITY_UI_CLIP_RECT
             #pragma multi_compile __ UNITY_UI_ALPHACLIP
 
             struct appdata_t
             {
                 float4 vertex   : POSITION;
                 float4 color    : COLOR;
                 float2 texcoord : TEXCOORD0;
                 UNITY_VERTEX_INPUT_INSTANCE_ID
             };
 
             struct v2f
             {
                 float4 vertex   : SV_POSITION;
                 fixed4 color    : COLOR;
                 float2 texcoord  : TEXCOORD0;
                 float4 worldPosition : TEXCOORD1;
                 UNITY_VERTEX_OUTPUT_STEREO
             };
 
             sampler2D _MainTex;
             fixed4 _Color;
             fixed4 _TextureSampleAdd;
             float4 _ClipRect;
             float4 _MainTex_ST;
             float  _height;
             float  _length;
             float  _speed;
             float _am;
             float4  _lineColor;
             float  _lineWidth;
             float _fadeOutWidth;
             v2f vert(appdata_t v)
             {
                 v2f OUT;
                 UNITY_SETUP_INSTANCE_ID(v);
                 UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                 OUT.worldPosition = v.vertex;
                 OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
 
                 OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
 
                 OUT.color = v.color * _Color;
                 return OUT;
             }
 
             fixed4 frag(v2f IN) : SV_Target
             {
                 half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                 float2 uv = IN.texcoord;
                 float h = sin(uv.x*_length+_Time*_speed)*_am + _height;
                 int max = step(uv.y,h);
                 float distance =clamp( (uv.y-h)/_fadeOutWidth,0,1);
                 distance = lerp(1,0,distance);
                 color.a  = color.a * max + (1-max)*distance*color.a;
                // int isLine = step(h-uv.y,_lineWidth);
                float t = (h-uv.y)/_lineWidth;
                t = clamp(t,0,1);
                 color = lerp(color*_lineColor,color,t);
               //  color = color*_lineColor * isLine + color*(1-isLine);
 
                 #ifdef UNITY_UI_CLIP_RECT
                 color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                 #endif
 
                 #ifdef UNITY_UI_ALPHACLIP
                 clip (color.a - 0.001);
                 #endif
               //  color = float4(uv.x,0,0,1);
                 return color;
             }
         ENDCG
         }
     }
 }