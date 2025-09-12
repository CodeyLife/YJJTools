// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/PieShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15
		_Riduas("raduas",Range(0,0.5)) = 0.4
		_SmoothMaxDistance("SmoothMaxDistance",Range(0,0.2)) = 0.02
		_SmoothMinDistance("SmoothMaxDistance",Range(0,0.2)) =0.02 
		_StartAngle("startAngle",Range(0,360)) = 0 
		_EndAngle("endAngle",Range(0,360)) = 120
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
				//float4 color    : COLOR;
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
			float _StartAngle;
			float _EndAngle;
			float _Riduas;
			float _SmoothMaxDistance;
		    float _SmoothMinDistance;

			v2f vert(appdata_t v)
			{
				v2f OUT;
				//UNITY_SETUP_INSTANCE_ID(v);
				//UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = v.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				// OUT.color = v.color * _Color;
				 return OUT;
			 }

			
			 fixed4 frag(v2f IN) : SV_Target
			 {
			     half2 uv2 = IN.texcoord;
				 half4 c = (tex2D(_MainTex,uv2) + _TextureSampleAdd);//* IN.color;
				 c *= _Color;
				 float3 uv  = float3(uv2.x-0.5,uv2.y-0.5,0);
				 float3 up =  float3(0,0.5,0);
				 float angle = acos(dot(uv,up)/(length(uv)*length(up)));
				 angle  = degrees(angle);
				 angle *= sign(cross(uv,up).z);
				 angle += step(angle,0)*360;
				 float inAngle = step(_StartAngle,angle) * step(angle,_EndAngle);
				 //float fade = lerp(1,0,clamp((angle - _StartAngle)/_Width,0,1))+lerp(1,0,clamp((_EndAngle-angle)/_Width,0,1));
				 //fade = clamp(fade,0,1);
				 //画圆
				 float startAngle = radians(_StartAngle -  step(180,_StartAngle)*360);
				 float startSin = sin(startAngle);
				 float startCos = cos(startAngle);
				  //sincos(startAngle,out  startSin,out  startCos);
				 float2 startUV =  float2(startSin * _Riduas + 0.5,startCos*_Riduas + 0.5);
				 float dis = distance(startUV,uv2);
				// float isInCicle = step(dis,_CircleRiadus);
				 float isInCicle = smoothstep(_SmoothMaxDistance,_SmoothMinDistance,dis);
				 isInCicle = clamp(isInCicle,0,1);
				 
				 float endAngle = radians(_EndAngle - step(180,_EndAngle)*360);
				 float endSin = sin(endAngle);
				 float endCos = cos(endAngle);
				 float2 endUV = float2(endSin*_Riduas + 0.5,endCos *_Riduas + 0.5);
				// float inEndCicle = step(dis,_CircleRiadus);
				 dis = distance(endUV,uv2);
				 float inEndCicle = smoothstep(_SmoothMaxDistance,_SmoothMinDistance,dis);
				 inEndCicle = clamp(inEndCicle,0,1);
				 float a = max(inAngle,isInCicle);
				  a = max(a,inEndCicle);
				 c.a *= a;
				// c = float4(c.x,c.y,c.r,fade);
				 return c;
				 }
			 ENDCG
			 }
		}
}
