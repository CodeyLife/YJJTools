using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

[ComponentDesc("阈值线与区间带")]
[ComponentOrder(5)]
public class ChartV2Component_Bands : ChartV2ComponetBase
{
    [System.Serializable]
    public struct Band
    {
        public float min;
        public float max;
        public Color color;
        [Range(0f, 1f)] public float alpha;
    }

    [Title("线与区间", TitleAlignment = TitleAlignments.Centered)]
    public List<float> thresholds = new List<float>();
    public List<Band> bands = new List<Band>();
    [Title("样式", TitleAlignment = TitleAlignments.Centered)]
    public float lineWidth = 1f;
    public Color lineColor = Color.red;

    private readonly List<Vector2> tmp = new List<Vector2>(4);

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
        _v2Base.ComputeMaxAndMin();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        if (_v2Base == null) return;

        float left = 0f;
        float right = _v2Base.width;
        float bottom = _v2Base.set.distanceFromButtom;
        float top = _v2Base.height - _v2Base.set.distanceFromTop;

        // 区间带
        for (int i = 0; i < bands.Count; i++)
        {
            var b = bands[i];
            float yMin = Mathf.Clamp(_v2Base.ValueToLocalY(b.min), bottom, top);
            float yMax = Mathf.Clamp(_v2Base.ValueToLocalY(b.max), bottom, top);
            if (yMax < yMin) { var t = yMin; yMin = yMax; yMax = t; }
            var col = new Color(b.color.r, b.color.g, b.color.b, b.alpha);
            AddRect(vh, new Vector2(left, yMin), new Vector2(right, yMax), col);
        }

        // 阈值线
        for (int i = 0; i < thresholds.Count; i++)
        {
            float y = Mathf.Clamp(_v2Base.ValueToLocalY(thresholds[i]), bottom, top);
            Yjj_ChartUtility.DrawLine(vh, new Vector2(left, y), new Vector2(right, y), lineWidth, lineColor);
        }
    }

    private static void AddRect(VertexHelper vh, Vector2 min, Vector2 max, Color color)
    {
        int start = vh.currentVertCount;
        vh.AddVert(new Vector3(min.x, min.y), color, Vector2.zero);
        vh.AddVert(new Vector3(min.x, max.y), color, Vector2.zero);
        vh.AddVert(new Vector3(max.x, max.y), color, Vector2.zero);
        vh.AddVert(new Vector3(max.x, min.y), color, Vector2.zero);
        vh.AddTriangle(start + 0, start + 1, start + 2);
        vh.AddTriangle(start + 2, start + 3, start + 0);
    }
}


