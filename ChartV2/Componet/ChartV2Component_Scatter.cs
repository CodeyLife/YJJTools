using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

[ComponentDesc("散点图")]
public class ChartV2Component_Scatter : ChartV2ComponetBase
{
    [FoldoutGroup("数据设置")]
    public bool useAllData = true;
    [HideIf("useAllData"), FoldoutGroup("数据设置")]
    public List<int> DataIndex = new List<int> { 0 };

    [FoldoutGroup("外观设置")]
    public float pointSize = 5f;
    [FoldoutGroup("外观设置")]
    public Color pointColor = Color.white;
    
    [FoldoutGroup("按值映射")]
    public bool sizeByValue = true;
    [ShowIf("sizeByValue"), FoldoutGroup("按值映射")]
    public float minRadius = 2f;
    [ShowIf("sizeByValue"), FoldoutGroup("按值映射")]
    public float maxRadius = 10f;
    
    [FoldoutGroup("按值映射")]
    public bool colorByValue = true;
    [ShowIf("colorByValue"), FoldoutGroup("按值映射")]
    public Color lowColor = Color.blue;
    [ShowIf("colorByValue"), FoldoutGroup("按值映射")]
    public Color highColor = Color.red;

    [FoldoutGroup("动画设置")]
    public bool enableAnimation = true;  
    [ShowIf("enableAnimation"), FoldoutGroup("动画设置")]
    public bool enableFadeIn = true;
    [ShowIf("enableAnimation"), FoldoutGroup("动画设置")]
    public bool enableScaleIn = true;
    


    private float animationPos = 1;
    private List<List<Vector2>> positions = new List<List<Vector2>>();
    private List<List<float>> values = new List<List<float>>();
    private List<List<Color>> colors = new List<List<Color>>();
    private List<List<float>> sizes = new List<List<float>>();

#if UNITY_EDITOR
    public override void OnCreat()
    {
        base.OnCreat();
        // 设置默认材质
        material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets($"t:material UV_x抗锯齿 清晰")[0]));
    }
#endif

    public override void InitGraph(ChartV2Base chart)
    {
        base.InitGraph(chart);
        
        if (Application.isPlaying)
        {
            _v2Base.InitAnimationEvent.AddListener(PlayAnimation);
            // 始终监听拖拽事件，因为散点图需要响应拖拽
            _v2Base.OnDragEvent.RemoveListener(OnDrag);
            _v2Base.OnDragEvent.AddListener(OnDrag);
        }
        
        ComputePositions();
    }

    public override void SetGraph()
    {
        ComputePositions();
        SetVerticesDirty();
    }

    private void ComputePositions()
    {
        positions.Clear();
        values.Clear();
        colors.Clear();
        sizes.Clear();

        var dataIndices = useAllData ? GetAllDataIndices() : DataIndex;
        
        for (int i = 0; i < dataIndices.Count; i++)
        {
            int dataIndex = dataIndices[i];
            if (dataIndex >= _v2Base.DataList.Count) continue;

            var data = _v2Base.DataList[dataIndex];
            var dataValues = _v2Base.datas[dataIndex].datas;
            
            var posList = new List<Vector2>();
            var valueList = new List<float>();
            var colorList = new List<Color>();
            var sizeList = new List<float>();

            for (int j = 0; j < data.Count; j++)
            {
                posList.Add(data[j]);
                valueList.Add(dataValues[j]);
                
                // 计算颜色
                Color color = pointColor;
                if (colorByValue && dataValues.Count > 0)
                {
                    float normalizedValue = NormalizeValue(dataValues[j], dataValues);
                    color = Color.Lerp(lowColor, highColor, normalizedValue);
                }
                colorList.Add(color);
                
                // 计算大小
                float size = pointSize;
                if (sizeByValue && dataValues.Count > 0)
                {
                    float normalizedValue = NormalizeValue(dataValues[j], dataValues);
                    size = Mathf.Lerp(minRadius, maxRadius, normalizedValue);
                }
                sizeList.Add(size);
            }

            positions.Add(posList);
            values.Add(valueList);
            colors.Add(colorList);
            sizes.Add(sizeList);
        }
    }

    private List<int> GetAllDataIndices()
    {
        var indices = new List<int>();
        for (int i = 0; i < _v2Base.DataList.Count; i++)
        {
            indices.Add(i);
        }
        return indices;
    }

    private float NormalizeValue(float value, List<float> allValues)
    {
        if (allValues.Count == 0) return 0;
        
        float min = allValues[0];
        float max = allValues[0];
        
        for (int i = 1; i < allValues.Count; i++)
        {
            if (allValues[i] < min) min = allValues[i];
            if (allValues[i] > max) max = allValues[i];
        }
        
        if (Mathf.Approximately(max, min)) return 0;
        return (value - min) / (max - min);
    }

    private void PlayAnimation(float arg0)
    {
        animationPos = arg0;
        SetVerticesDirty();
    }

    private void OnDrag(float offset)
    {
        // 拖拽时不需要重新计算位置，只需要重新渲染可见区域
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_v2Base == null || positions.Count == 0) return;

        // 计算可见区域范围（不受动画影响）
        float viewStart = _v2Base.XOffset;
        float viewEnd = _v2Base.XOffset + _v2Base.width;
        var offset = new Vector2(_v2Base.XOffset, 0);

        for (int seriesIndex = 0; seriesIndex < positions.Count; seriesIndex++)
        {
            var posList = positions[seriesIndex];
            var colorList = colors[seriesIndex];
            var sizeList = sizes[seriesIndex];

            if (posList.Count == 0) continue;

            // 使用二分查找找到可见区域内的点
            int startIdx = YJJTool.MathUtility.FindFirstIndexGE(posList, viewStart);
            int endIdx = YJJTool.MathUtility.FindLastIndexLE(posList, viewEnd);
            
            if (endIdx < 0 || startIdx >= posList.Count || startIdx > endIdx) continue;
            for (int i = startIdx; i <= endIdx; i++)
            {
                Vector2 originalPos = posList[i];
                Color color = colorList[i];
                float size = sizeList[i];

                // 应用动画进度 - 按数据索引顺序显示
                if (enableAnimation && animationPos < 1f)
                {
                    // 按数据索引顺序显示
                    float pointProgress = posList.Count > 1 ? (float)i / (posList.Count - 1) : 0f;
                    if (pointProgress > animationPos) continue;
                    
                    // 淡入效果
                    if (enableFadeIn)
                    {
                        // 修复：基于动画进度计算淡入效果
                        float fadeProgress = Mathf.Clamp01(animationPos);
                        color.a *= fadeProgress;
                    }
                    
                    // 缩放效果
                    if (enableScaleIn)
                    {
                        // 修复：基于动画进度计算缩放效果
                        float scaleProgress = Mathf.Clamp01(animationPos);
                        size *= scaleProgress;
                    }
                }

                // 应用拖拽偏移
                Vector2 screenPos = originalPos - offset;
                // 创建圆形点
                CreateCircle(vh, screenPos, size, color);
            }
        }
    }



    private void CreateCircle(VertexHelper vh, Vector2 center, float radius, Color color)
    {
        int segments = 16; // 圆形分段数
        float angleStep = 360f / segments * Mathf.Deg2Rad;

        // 中心点
        int centerIndex = vh.currentVertCount;
        vh.AddVert(center, color, Vector2.zero);

        // 圆周点
        int[] circleIndices = new int[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep;
            Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            circleIndices[i] = vh.currentVertCount;
            vh.AddVert(pos, color, Vector2.zero);
        }

        // 创建三角形
        for (int i = 0; i < segments; i++)
        {
            int current = circleIndices[i];
            int next = circleIndices[i + 1];
            vh.AddTriangle(centerIndex, current, next);
        }
    }

    protected override void OnDestroy()
    {
        if (_v2Base != null)
        {
            _v2Base.InitAnimationEvent.RemoveListener(PlayAnimation);
            _v2Base.OnDragEvent.RemoveListener(OnDrag);
        }
    }
}
