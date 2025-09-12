using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

[ComponentDesc("网格线")]
[ComponentOrder(1)] // 在背景层绘制
public class ChartV2Component_Grid : ChartV2ComponetBase
{
    [Title("网格设置")]
    [LabelText("显示垂直网格线")]
    public bool showVerticalGrid = true;
    [LabelText("显示水平网格线")]
    public bool showHorizontalGrid = true;
    
    [Title("样式设置")]
    [LabelText("网格线颜色")]
    public Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    [LabelText("网格线宽度")]
    [Range(0.5f, 5f)]
    public float gridLineWidth = 1f;
    
    [Title("网格分割")]
    [LabelText("垂直网格分割数")]
    [Range(1, 20)]
    public int verticalDivisions = 5;
    [LabelText("水平网格分割数")]
    [Range(1, 20)]
    public int horizontalDivisions = 5;
    
    [Title("高级设置")]
    [LabelText("显示主网格线")]
    public bool showMajorGrid = true;
    [LabelText("显示次网格线")]
    public bool showMinorGrid = false;
    [LabelText("次网格线颜色")]
    [ShowIf("showMinorGrid")]
    public Color minorGridColor = new Color(0.7f, 0.7f, 0.7f, 0.15f);
    [LabelText("次网格线宽度")]
    [ShowIf("showMinorGrid")]
    [Range(0.5f, 3f)]
    public float minorGridLineWidth = 0.5f;

    public override void InitGraph(ChartV2Base chart)
    {
        base.InitGraph(chart);
        if (Application.isPlaying)
        {
            _v2Base.OnDragEvent.AddListener(_ => SetVerticesDirty());
            _v2Base.InitAnimationEvent.AddListener(_ => SetVerticesDirty());
        }
        SetGraph();
    }

    public override void SetGraph()
    {
        base.SetGraph();
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        if (_v2Base == null) return;

        _v2Base.ComputeMaxAndMin();
        
        float width = _v2Base.width;
        float height = _v2Base.height;
        float leftMargin = _v2Base.set.distanceFromLeft;
        float rightMargin = _v2Base.set.distanceFromRight;
        float topMargin = _v2Base.set.distanceFromTop;
        float bottomMargin = _v2Base.set.distanceFromButtom;
        
        float chartWidth = width - leftMargin - rightMargin;
        float chartHeight = height - topMargin - bottomMargin;
        
        var offset = new Vector2(_v2Base.XOffset, 0);

        // 绘制垂直网格线
        if (showVerticalGrid)
        {
            DrawVerticalGridLines(vh, leftMargin, chartWidth, chartHeight, topMargin, bottomMargin, offset);
        }

        // 绘制水平网格线
        if (showHorizontalGrid)
        {
            DrawHorizontalGridLines(vh, leftMargin, chartWidth, chartHeight, topMargin, bottomMargin, offset);
        }
    }

    private void DrawVerticalGridLines(VertexHelper vh, float leftMargin, float chartWidth, float chartHeight, float topMargin, float bottomMargin, Vector2 offset)
    {
        float startX = leftMargin - offset.x;
        float endX = leftMargin + chartWidth - offset.x;
        
        // 计算网格线位置
        List<float> gridPositions = CalculateGridPositions(startX, endX, verticalDivisions);
        
        foreach (float x in gridPositions)
        {
            // 只绘制在可见区域内的网格线
            if (x >= 0 && x <= _v2Base.width)
            {
                Vector2 start = new Vector2(x, bottomMargin);
                Vector2 end = new Vector2(x, bottomMargin + chartHeight);
                Yjj_ChartUtility.DrawLine(vh, start, end, gridLineWidth, gridColor);
            }
        }

        // 绘制次网格线
        if (showMinorGrid)
        {
            DrawMinorVerticalGridLines(vh, startX, endX, chartHeight, topMargin, bottomMargin, offset);
        }
    }

    private void DrawHorizontalGridLines(VertexHelper vh, float leftMargin, float chartWidth, float chartHeight, float topMargin, float bottomMargin, Vector2 offset)
    {
        float startY = bottomMargin;
        float endY = bottomMargin + chartHeight;
        
        // 计算网格线位置
        List<float> gridPositions = CalculateGridPositions(startY, endY, horizontalDivisions);
        
        foreach (float y in gridPositions)
        {
            Vector2 start = new Vector2(leftMargin - offset.x, y);
            Vector2 end = new Vector2(leftMargin + chartWidth - offset.x, y);
            Yjj_ChartUtility.DrawLine(vh, start, end, gridLineWidth, gridColor);
        }

        // 绘制次网格线
        if (showMinorGrid)
        {
            DrawMinorHorizontalGridLines(vh, leftMargin, chartWidth, startY, endY, offset);
        }
    }

    private void DrawMinorVerticalGridLines(VertexHelper vh, float startX, float endX, float chartHeight, float topMargin, float bottomMargin, Vector2 offset)
    {
        float step = (endX - startX) / (verticalDivisions * 2);
        for (int i = 1; i < verticalDivisions * 2; i += 2)
        {
            float x = startX + i * step;
            if (x >= 0 && x <= _v2Base.width)
            {
                Vector2 start = new Vector2(x, bottomMargin);
                Vector2 end = new Vector2(x, bottomMargin + chartHeight);
                Yjj_ChartUtility.DrawLine(vh, start, end, minorGridLineWidth, minorGridColor);
            }
        }
    }

    private void DrawMinorHorizontalGridLines(VertexHelper vh, float leftMargin, float chartWidth, float startY, float endY, Vector2 offset)
    {
        float step = (endY - startY) / (horizontalDivisions * 2);
        for (int i = 1; i < horizontalDivisions * 2; i += 2)
        {
            float y = startY + i * step;
            Vector2 start = new Vector2(leftMargin - offset.x, y);
            Vector2 end = new Vector2(leftMargin + chartWidth - offset.x, y);
            Yjj_ChartUtility.DrawLine(vh, start, end, minorGridLineWidth, minorGridColor);
        }
    }

    private List<float> CalculateGridPositions(float start, float end, int divisions)
    {
        List<float> positions = new List<float>();
        float step = (end - start) / divisions;
        
        for (int i = 0; i <= divisions; i++)
        {
            positions.Add(start + i * step);
        }
        
        return positions;
    }
}
