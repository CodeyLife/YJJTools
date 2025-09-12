
//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

// 简化版均值模糊 - 3x3 内核
    void BoxBlur_float(
       UnityTexture2D Source,
       float2 uv,
       float blueSize,
       out float4 color    
       )
    {
        // 3×3均值模糊的权重(每个位置权重相等)
        float weight = 1.0/9;
        
        // 3×3采样偏移
        float2 offsets[9] = {
            float2(-1, -1), float2(0, -1), float2(1, -1),
            float2(-1,  0), float2(0,  0), float2(1,  0),
            float2(-1,  1), float2(0,  1), float2(1,  1)
        };
        
        color = float4(0, 0, 0, 0);
        
        // 遍历3×3内核进行采样
        for (int i = 0; i < 9; i++)
        {
            float2 sampleUV = uv + offsets[i]*blueSize;
            color += tex2D(Source, sampleUV) * weight;
        }
    }


	float4 horizBars(float2 p)
	{
		return 1 - saturate(round(abs(frac(p.y * 100) * 2)));
	}
	float4 verticalBars(float2 p)
	{
		return 1 - saturate(round(abs(frac(p.x * 100) * 2)));
	}

    void Scan_float(float dist,float2 uv,float _ScanDistance,float _ScanWidth,float linearDepth,float4 _MidColor,float4 _LeadColor,float4 _TrailColor,float4 _HBarColor,float _LeadSharp,out float4 scannerCol)
	{
			
        if (dist < _ScanDistance && dist > _ScanDistance - _ScanWidth && linearDepth < 1)
				{
			          	half4 scannerCol1 = half4(0, 0, 0, 0);
			          	half4 scannerCol2 = half4(0, 0, 0, 0);
					float diff = 1 - (_ScanDistance - dist) / (_ScanWidth);
					half4 edge = lerp(_MidColor, _LeadColor, pow(diff, _LeadSharp));
					scannerCol1 = lerp(_TrailColor, edge, diff) + horizBars(uv) * _HBarColor;
					scannerCol2 = lerp(_TrailColor, edge, diff) + verticalBars(uv) * _HBarColor;
					scannerCol = scannerCol1*0.5+scannerCol2*0.5;
					//scannerCol = lerp(_TrailColor, edge, diff) + horizTex(i.uv) * _HBarColor;
					scannerCol *= diff;
				}else
				{
					scannerCol = float4(0,0,0,0);
				}
	}

#endif //MYHLSLINCLUDE_INCLUDED