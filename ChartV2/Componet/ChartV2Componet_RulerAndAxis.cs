using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

[ComponentDesc("轴线和刻度线")]
[ComponentOrder(-1)]
public class ChartV2Componet_RulerAndAxis : ChartV2ComponetBase
{
    [Title("轴线参数", TitleAlignment = TitleAlignments.Centered)]
    public float axisWidth = 1;
    [Range(1, 10)]
    public int rulerCount = 3;
    public Color axisColor = Color.gray;
    public string unit;
    [ShowIf("@!string.IsNullOrEmpty(unit)"),LabelText("单位显示在刻度中")]
    public bool showInData = false;
    [ShowIf("@!string.IsNullOrEmpty(unit) && !showInData"), LabelText("单位偏移")]
    public Vector2 offset = new Vector2(0, 20);
    [Title("刻度线参数", TitleAlignment = TitleAlignments.Centered)]
    public  Color lineColor = Color.white;
    [Range(0,2)]
    public float lineWith = 0.2f;
    [Title("字体参数", TitleAlignment = TitleAlignments.Centered)]
    public float distance = 10;
    public float fontSize = 24;
    public TMP_FontAsset font;
    public Color fontColor = Color.gray;

#if UNITY_EDITOR
    public override void OnCreat()
    {
        base.OnCreat();
        font = YjjConfigs.Instance.tmpFont;
    }
#endif

    public override void InitGraph(ChartV2Base chart)
    {
        
        base.InitGraph(chart);
        SetGraph();

    }
    public override void SetGraph()
    {
        base.SetGraph();
        raycastTarget = false;

        var width = _v2Base.width;
        var height = _v2Base.height;
        var start = _v2Base.set.distanceFromButtom;
        var end = _v2Base.height - _v2Base.set.distanceFromTop;
        _v2Base.ComputeMaxAndMin();
        //起点最小值
        var text = GetOrCreatText("0");
        text.rectTransform.anchoredPosition = new Vector2(-distance, start);
        text.text = _v2Base.min.ToString();


        var space = (end - start) / rulerCount;
        var rulerSpace = (_v2Base.max - _v2Base.min) / rulerCount;

        for (int i = 0; i < rulerCount; i++)
        {
            var h = start + (i + 1) * space;
            text = GetOrCreatText((i + 1).ToString());
            text.rectTransform.anchoredPosition = new Vector2(-distance, h);
            var value = ((i + 1) * rulerSpace + _v2Base.min);
            text.text = value.ToAutoLimitString(2);
            if (showInData)
            {
                text.text += unit;
            }
        }
        int delateIndex = rulerCount + 1;
        if (!string.IsNullOrEmpty(unit) && !showInData)
        {

            text = GetOrCreatText(delateIndex.ToString());
            text.text = $"({unit})";
            delateIndex++;
            text.rectTransform.anchoredPosition = new Vector2(-distance, height) + offset;

        }
        if (Application.isPlaying)
        {
            for (int i = delateIndex; i < transform.childCount; i++)
            {
                Destroy(transform.Find(i.ToString()).gameObject);
            }
        }
        else
        {
            var destroys = new List<GameObject>();
            for (int i = delateIndex; i < transform.childCount; i++)
            {
                destroys.Add(transform.Find(i.ToString()).gameObject);
            }
            destroys.ForEach(x => DestroyImmediate(x));
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        try
        {
            var width = _v2Base.width;
            var start = _v2Base.set.distanceFromButtom;
            var end = _v2Base.height - _v2Base.set.distanceFromTop;
            if (axisWidth > 0)
            {
                var list = new List<Vector2>();
                list.Add(new Vector2(0, _v2Base.height));
                list.Add(Vector2.zero);
                list.Add(new Vector2(width, 0));
                Yjj_ChartUtility.DrawLineSmooth(vh, list, axisWidth, axisColor);
            }

            if (lineWith > 0)
            {
               
                var space = (end - start) / rulerCount;
                for (int i = 0; i < rulerCount; i++)
                {
                    var h = start + (i + 1) * space;
                    Yjj_ChartUtility.DrawLine(vh, new Vector2(0, h), new Vector2(width, h), lineWith, lineColor);
                }
            }
        }
        catch { }
    }

    private TextMeshProUGUI GetOrCreatText(string name)
    {
        var text = transform.GetOrCreatUIChild<TextMeshProUGUI>(name, (t) =>
        {
            var anchor = new Vector2(0, 0);
            t.rectTransform.anchorMin = anchor;
            t.rectTransform.anchorMax = anchor;
            t.rectTransform.pivot = new Vector2(1, 0.5f);
            t.alignment = TextAlignmentOptions.MidlineRight;
            t.raycastTarget = false;

        });
        text.font = font;
        text.color = fontColor;
        text.fontSize = fontSize;
        text.maskable = true;
     
        return text;
    }
}
