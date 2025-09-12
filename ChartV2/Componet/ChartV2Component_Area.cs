using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ComponentDesc("面积图填充")]
[ComponentOrder(10)]
public class ChartV2Component_Area : ChartV2ComponetBase
{
    public bool useAllData = true;
    [HideIf("useAllData")]
    public List<int> DataIndex = new List<int> { 0 };

    [Title("样式", TitleAlignment = TitleAlignments.Centered)]
    public Color fillColor = new Color(1, 1, 1, 0.25f);
    [Tooltip("是否按序列堆叠绘制")]
    public bool stacked = false;

    [Title("渐变", TitleAlignment = TitleAlignments.Centered)]
    public bool useVerticalGradient = false;
    [ShowIf("useVerticalGradient")] public Color topColor = new Color(1, 1, 1, 0.45f);
    [ShowIf("useVerticalGradient")] public Color bottomColor = new Color(1, 1, 1, 0.05f);

    private readonly List<List<Vector2>> cacheVisible = new List<List<Vector2>>();
    private Vector2[] quad = new Vector2[4];

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
        cacheVisible.Clear();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        if (_v2Base == null) return;

        int seriesCount = useAllData ? _v2Base.datas.Count : DataIndex.Count;
        EnsureVisibleCache(seriesCount);

        float xOffset = _v2Base.XOffset;
        float viewStart = xOffset;
        float length = _v2Base.width;
        float viewEnd = xOffset + length;

        _v2Base.ComputeMaxAndMin();
        float defaultBaseY = _v2Base.ValueToLocalY(Mathf.Max(0, _v2Base.min));

        List<float> prevTopY = null; // 上一序列的顶部y（用于堆叠）

        for (int s = 0; s < seriesCount; s++)
        {
            int realIndex = useAllData ? s : DataIndex[s];
            var src = _v2Base.DataList[realIndex];
            if (src == null || src.Count == 0) { prevTopY = prevTopY; continue; }

            var visible = cacheVisible[s];
            visible.Clear();

            int startIdx = FindFirstIndexGE(src, viewStart);
            int endIdx = FindLastIndexLE(src, viewEnd);
            if (endIdx < 0 || startIdx >= src.Count || startIdx > endIdx) { continue; }
            int iterStart = Mathf.Max(startIdx - 1, 0);
            int iterEnd = Mathf.Min(endIdx + 1, src.Count - 1);

            var offset = new Vector2(xOffset, 0);
            bool addedHead = false;
            
            for (int i = iterStart; i <= iterEnd; i++)
            {
                var original = src[i];
                var data = original - offset;
                
                if (i == iterStart && data.x < 0)
                {
                    // 在左侧边界内插一个点到 x=0
                    if (i + 1 <= iterEnd)
                    {
                        var next = src[i + 1] - offset;
                        var t0 = YjjUtility.SmoothLerp(data.x, next.x, 0);
                        var p0 = Vector2.Lerp(data, next, t0);
                        visible.Add(p0);
                        addedHead = true;
                    }
                    continue;
                }
                
                if (data.x > length)
                {
                    // 在右侧边界内插一个点到 x=length
                    if (i - 1 >= iterStart)
                    {
                        var prev = src[i - 1] - offset;
                        var t1 = YjjUtility.SmoothLerp(prev.x, data.x, length);
                        var p1 = Vector2.Lerp(prev, data, t1);
                        visible.Add(p1);
                    }
                    break;
                }
                
                if (!addedHead && i > 0 && (src[i - 1].x - _v2Base.XOffset) < 0)
                {
                    // 确保第一个可见点前的插值被加入
                    var lastPos = src[i - 1] - offset;
                    var t = YjjUtility.SmoothLerp(lastPos.x, data.x, 0);
                    visible.Add(Vector2.Lerp(lastPos, data, t));
                    addedHead = true;
                }
                
                visible.Add(data);
            }
            if (visible.Count < 2) { continue; }

            // 若堆叠：构造当前序列的顶部y数组，与上一序列对齐
            List<float> curTopY = null;
            if (stacked)
            {
                int n = visible.Count;
                curTopY = new List<float>(n);
                for (int i = 0; i < n; i++)
                {
                    float baseY = (prevTopY != null && i < prevTopY.Count) ? prevTopY[i] : defaultBaseY;
                    curTopY.Add(Mathf.Max(baseY, visible[i].y));
                }
            }

            // 绘制面积：逐段，允许左右两端基线不同（支持堆叠阶梯）
            for (int i = 1; i < visible.Count; i++)
            {
                var p0 = visible[i - 1];
                var p1 = visible[i];
                float base0 = stacked ? ((prevTopY != null && (i - 1) < prevTopY.Count) ? prevTopY[i - 1] : defaultBaseY) : defaultBaseY;
                float base1 = stacked ? ((prevTopY != null && i < prevTopY.Count) ? prevTopY[i] : defaultBaseY) : defaultBaseY;
                DrawAreaSegment(vh, p0, p1, base0, base1, fillColor);
            }

            // 更新上一序列顶部
            if (stacked)
            {
                prevTopY = curTopY;
            }
        }
    }

    private void EnsureVisibleCache(int count)
    {
        while (cacheVisible.Count < count) cacheVisible.Add(new List<Vector2>(64));
        while (cacheVisible.Count > count) cacheVisible.RemoveAt(cacheVisible.Count - 1);
    }

    private static int FindFirstIndexGE(List<Vector2> arr, float targetX)
    {
        int low = 0, high = arr.Count - 1, ans = arr.Count;
        while (low <= high)
        {
            int mid = (low + high) >> 1;
            if (arr[mid].x >= targetX) { ans = mid; high = mid - 1; }
            else low = mid + 1;
        }
        return ans;
    }
    private static int FindLastIndexLE(List<Vector2> arr, float targetX)
    {
        int low = 0, high = arr.Count - 1, ans = -1;
        while (low <= high)
        {
            int mid = (low + high) >> 1;
            if (arr[mid].x <= targetX) { ans = mid; low = mid + 1; }
            else high = mid - 1;
        }
        return ans;
    }

    private void DrawArea(VertexHelper vh, List<Vector2> line, float baseY, Color color)
    {
        // 将折线段按相邻两点与基线组成梯形，逐段填充
        for (int i = 1; i < line.Count; i++)
        {
            var p0 = line[i - 1];
            var p1 = line[i];
            quad[0] = new Vector2(p0.x, baseY);
            quad[1] = new Vector2(p0.x, p0.y);
            quad[2] = new Vector2(p1.x, p1.y);
            quad[3] = new Vector2(p1.x, baseY);
            AddQuad(vh, quad, color);
        }
    }

    private static void AddQuad(VertexHelper vh, Vector2[] q, Color color)
    {
        int start = vh.currentVertCount;
        for (int i = 0; i < 4; i++)
        {
            vh.AddVert(q[i], color, Vector2.zero);
        }
        vh.AddTriangle(start + 0, start + 1, start + 2);
        vh.AddTriangle(start + 2, start + 3, start + 0);
    }

    private void DrawAreaSegment(VertexHelper vh, in Vector2 p0Top, in Vector2 p1Top, float base0, float base1, Color solidColor)
    {
        int start = vh.currentVertCount;
        // 顶部颜色与底部颜色
        Color cTop0 = useVerticalGradient ? topColor : solidColor;
        Color cTop1 = useVerticalGradient ? topColor : solidColor;
        Color cBot0 = useVerticalGradient ? bottomColor : solidColor;
        Color cBot1 = useVerticalGradient ? bottomColor : solidColor;

        vh.AddVert(new Vector3(p0Top.x, base0), cBot0, Vector2.zero); // v0
        vh.AddVert(new Vector3(p0Top.x, p0Top.y), cTop0, Vector2.zero); // v1
        vh.AddVert(new Vector3(p1Top.x, p1Top.y), cTop1, Vector2.zero); // v2
        vh.AddVert(new Vector3(p1Top.x, base1), cBot1, Vector2.zero); // v3
        vh.AddTriangle(start + 0, start + 1, start + 2);
        vh.AddTriangle(start + 2, start + 3, start + 0);
    }
}


