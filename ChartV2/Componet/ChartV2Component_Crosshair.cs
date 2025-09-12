using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

[ComponentDesc("十字光标")]
[ComponentOrder(100)]
public class ChartV2Component_Crosshair : ChartV2ComponetBase
{
    [Title("样式", TitleAlignment = TitleAlignments.Centered)]
    public Color lineColor = new Color(1, 1, 1, 0.6f);
    [Range(0.5f, 3f)] public float lineWidth = 1f;
    public bool showHorizontal = true;
    public bool showVertical = true;

    [Title("吸附与标签", TitleAlignment = TitleAlignments.Centered)]
    public bool snapToNearest = true;
    [ShowIf("snapToNearest")] public bool showLabel = true;
    [ShowIf("showLabel")] public Vector2 labelOffset = new Vector2(12, 12);
    [ShowIf("showLabel")] public Color labelColor = Color.white;
    [ShowIf("showLabel")] public int labelFontSize = 22;

    [Title("多序列汇总", TitleAlignment = TitleAlignments.Centered)]
    public bool showSeriesSummary = true;
    [ShowIf("showSeriesSummary")] public bool showSeriesColors = true;

    [Title("调试", TitleAlignment = TitleAlignments.Centered)]
    public bool enableDebug = false;
    [ShowIf("enableDebug"), ReadOnly] public Vector2 debugHoverPos;
    [ShowIf("enableDebug"), ReadOnly] public Vector2 debugSnappedPos;
    [ShowIf("enableDebug"), ReadOnly] public bool debugHasSnap;
    [ShowIf("enableDebug"), ReadOnly] public int debugHoverIndex;
    [ShowIf("enableDebug")] public bool showDebugInScene = false;

    private bool _hovering = false;
    private Vector2 _hoverPos;
    private int _lastHoverIndex = -1;
    private Vector2 _snappedPos;
    private bool _hasSnap = false;
    private TextMeshProUGUI _label;

    public override void InitGraph(ChartV2Base chart)
    {
        base.InitGraph(chart);
        raycastTarget = false;
        _hovering = false;
        if (Application.isPlaying)
        {
            _v2Base.OnPointerEnterEvent.AddListener(() => { _hovering = true; SetVerticesDirty(); UpdateLabelVisibility(true); });
            _v2Base.OnPointerExitEvent.AddListener(() => { _hovering = false; SetVerticesDirty(); UpdateLabelVisibility(false); });
            _v2Base.OnHoverEvent.AddListener(OnHover);
            _v2Base.OnDragEvent.AddListener(_ => { SetVerticesDirty(); UpdateLabelPosition(); });
        }
        EnsureLabel();
        SetGraph();
    }

    public override void SetGraph()
    {
        base.SetGraph();
    }

    private void EnsureLabel()
    {
        if (!showLabel) return;
        _label = transform.GetOrCreatUIChild<TextMeshProUGUI>("CrosshairLabel", t =>
        {
            var anchor = new Vector2(0, 0);
            t.rectTransform.anchorMin = anchor;
            t.rectTransform.anchorMax = anchor;
            t.rectTransform.pivot = new Vector2(0, 0);
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.raycastTarget = false;
        });
        _label.fontSize = labelFontSize;
        _label.color = labelColor;
        _label.gameObject.SetActive(false);
        _label.richText = true;
        if (_v2Base.set.font != null)
        {
            _label.font = _v2Base.set.font;
        }
    }

    private void UpdateLabelVisibility(bool visible)
    {
        if (_label != null) _label.gameObject.SetActive(visible && showLabel);
    }

    private void UpdateLabelPosition()
    {
        if (_label == null || !showLabel) return;
        var refPos = _hasSnap ? _snappedPos : _hoverPos;
        var pos = refPos + labelOffset;
        pos.x = Mathf.Clamp(pos.x, 0, _v2Base.width);
        pos.y = Mathf.Clamp(pos.y, 0, _v2Base.height);
        _label.rectTransform.anchoredPosition = pos;
    }

    private void OnHover(Vector2 local)
    {
        // 使用基础类计算后的 HoverPos，避免坐标基准差异
        _hoverPos = _v2Base.HoverPos;
        _hoverPos.x = Mathf.Clamp(_hoverPos.x, 0, _v2Base.width);
        _hoverPos.y = Mathf.Clamp(_hoverPos.y, 0, _v2Base.height);

        _hasSnap = false;
        if (snapToNearest)
        {
            _lastHoverIndex = _v2Base.HoverDataIndex;
            _snappedPos = GetNearestDataPointLocalPos(_lastHoverIndex, out string labelText);
            _hasSnap = _lastHoverIndex >= 0;
            if (showSeriesSummary) labelText = BuildSeriesSummary(_lastHoverIndex, labelText);
            if (_label != null && showLabel)
            {
                _label.text = labelText;
                _label.gameObject.SetActive(true);
                UpdateLabelPosition();
            }
        }

        // 更新调试信息
        if (enableDebug)
        {
            debugHoverPos = _hoverPos;
            debugSnappedPos = _snappedPos;
            debugHasSnap = _hasSnap;
            debugHoverIndex = _lastHoverIndex;
        }

        SetVerticesDirty();
    }

    private string BuildSeriesSummary(int dataIndex, string fallback)
    {
        if (dataIndex < 0 || _v2Base.datas == null || _v2Base.datas.Count == 0) return fallback;
        System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
        string name = (dataIndex < _v2Base.names.Count) ? _v2Base.names[dataIndex] : "";
        if (!string.IsNullOrEmpty(name)) sb.Append(name).Append('\n');
        var unit = _v2Base.Unit;
        var colors = _v2Base.set != null ? _v2Base.set.colors : null;
        for (int s = 0; s < _v2Base.datas.Count; s++)
        {
            var series = _v2Base.datas[s];
            if (series == null || series.datas == null || dataIndex >= series.datas.Count) continue;
            float v = series.datas[dataIndex];
            if (showSeriesColors && colors != null && s < colors.Count)
            {
                Color c = colors[s];
                string hex = ColorUtility.ToHtmlStringRGB(c);
                sb.Append("<color=#").Append(hex).Append(">");
                sb.Append($"S{s}: ");
                sb.Append(v);
                if (!string.IsNullOrEmpty(unit)) sb.Append(unit);
                sb.Append("</color>\n");
            }
            else
            {
                sb.Append($"S{s}: ").Append(v);
                if (!string.IsNullOrEmpty(unit)) sb.Append(unit);
                sb.Append('\n');
            }
        }
        if (sb.Length == 0) return fallback;
        return sb.ToString();
    }

    private Vector2 GetNearestDataPointLocalPos(int dataIndex, out string labelText)
    {
        labelText = string.Empty;
        if (dataIndex < 0) return _hoverPos;
        float bestDist = float.MaxValue;
        Vector2 best = _hoverPos;
        float xOffset = _v2Base.XOffset;
        string name = (dataIndex < _v2Base.names.Count) ? _v2Base.names[dataIndex] : "";
        float val = 0f;
        bool found = false;
        for (int s = 0; s < _v2Base.DataList.Count; s++)
        {
            var series = _v2Base.DataList[s];
            if (series == null || dataIndex >= series.Count) continue;
            var p = series[dataIndex];
            var localPos = new Vector2(p.x - xOffset, p.y);
            float d = Mathf.Abs(localPos.x - _hoverPos.x);
            if (d < bestDist)
            {
                bestDist = d;
                best = localPos;
                if (s < _v2Base.datas.Count && dataIndex < _v2Base.datas[s].datas.Count)
                {
                    val = _v2Base.datas[s].datas[dataIndex];
                    found = true;
                }
            }
        }
        if (found)
        {
            labelText = string.IsNullOrEmpty(_v2Base.Unit) ? $"{name}: {val}" : $"{name}: {val}{_v2Base.Unit}";
        }
        return best;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        if (_v2Base == null || !_hovering) return;

        var pos = (_hasSnap && _lastHoverIndex >= 0) ? _snappedPos : _hoverPos;
        pos.x = Mathf.Clamp(pos.x, 0, _v2Base.width);
        pos.y = Mathf.Clamp(pos.y, 0, _v2Base.height);
        if (showVertical)
        {
            Yjj_ChartUtility.DrawLine(vh, new Vector2(pos.x, 0), new Vector2(pos.x, _v2Base.height), lineWidth, lineColor);
        }
        if (showHorizontal)
        {
            Yjj_ChartUtility.DrawLine(vh, new Vector2(0, pos.y), new Vector2(_v2Base.width, pos.y), lineWidth, lineColor);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enableDebug || !showDebugInScene || !Application.isPlaying) return;
        
        // 绘制调试信息
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(_hoverPos.x, _hoverPos.y, 0), 5f);
        
        if (_hasSnap)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(new Vector3(_snappedPos.x, _snappedPos.y, 0), 8f);
        }
        
        // 绘制调试文本
        UnityEditor.Handles.Label(new Vector3(_hoverPos.x + 10, _hoverPos.y + 10, 0), 
            $"Hover: {_hoverPos}\nSnap: {_snappedPos}\nIndex: {_lastHoverIndex}");
    }
#endif
}
