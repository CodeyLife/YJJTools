// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/SmoothWithUV_X"
{
	Properties
	{
		//_TestTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_buttomColor("buttomColor", Color) = (1,1,1,1)
		_topColor("topColor", Color) = (1,1,1,1)
		_alpha("alpha",Float)=0.5

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			Pass
			{
				Name "Default"
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0

				#include "UnityCG.cginc"
				#include "UnityUI.cginc"

				#define fwidth(x) (abs(ddx(x)) + abs(ddy(x))) // fwidth 是 dx11 的函数, 可以这样定义才能使用到其它平台

			//#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			//#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			//sampler2D _TestTex;
			fixed4 _buttomColor;
			fixed4 _topColor;
			float4 _ClipRect;

			float _alpha;

			v2f vert(appdata_t v)
			{
				v2f OUT;
				//UNITY_SETUP_INSTANCE_ID(v);
				//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = v.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = v.texcoord;

				 OUT.color = v.color;
				 return OUT;
			 }


			fixed4 frag(v2f IN) : SV_Target
			{
					 float2 uv = IN.texcoord;
					 //half4 color = (tex2D(_TestTex, (0.5,uv.y)));//* IN.color;
					 half4 color = _buttomColor * (1- uv.y) + _topColor * (uv.y);
					 // color*=_Color;
					 color *= IN.color;
					
					 //return (uv.x, uv.y, 0, uv.y);
					
					 return half4(color.r,color.g,color.b,_alpha);
					
					  // color.a = min(color.a,IN.color.a);
					    //return (1, 1, 1, uv.x);
					}
				ENDCG
				}
		}
}
