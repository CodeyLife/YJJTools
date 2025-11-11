using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using YJJTool;

public class FreeChart : ChartBase
{

    [FoldoutGroup("基础设置")]
    public BaseSet set = new BaseSet();
    [FoldoutGroup("基础设置")]
    public HoverSet hoverSet = new HoverSet();
    [Title("Hover事件",TitleAlignment = TitleAlignments.Centered)]
    [FoldoutGroup("基础设置/Hover事件")]
    public IntEvent HoverEvent = new IntEvent();
    [FoldoutGroup("基础设置/Hover事件")]
    public IntEvent HoverExitEvent = new IntEvent();
    [FoldoutGroup("基础设置/Hover事件")]
    public stringEvent GetHoverNameEvent = new stringEvent();

    [FoldoutGroup("数据设置")]
    [Title("数据标题设置")]
    public DataSet dataSet = new DataSet();
    [FoldoutGroup("数据设置")]
    [Title("数据")]
    public List<MultipleData> datas = new List<MultipleData>();
    [FoldoutGroup("数据设置")]
    public List<DrawFreeChartBase> charts = new List<DrawFreeChartBase>();
    [FoldoutGroup("数据设置")]
    [Title("是否显示数据单位")] public bool showUnit = false;
    [FoldoutGroup("数据设置")]
    [Title("字体")] public TMP_FontAsset font;

    [FoldoutGroup("动画设置")]
    public AnimationSet animationSet = new AnimationSet();

    public override void Awake()
    {
        base.Awake();
    }
    public override void OnEnable()
    {
        base.OnEnable();
    }
    public void Update()
    {
        hoverSet.Updata();
    }
    /// <summary>
    /// 数据类型List<MultipleData>
    /// </summary>
    /// <param name="data"></param>
    /// <param name="names">数据标题名</param>
    public override void SetData(List<MultipleData> data, List<string> names)
    {
        base.SetData(data, names);
        datas = data;
        if (names != null)
        {
            dataSet.names = names;
        }
        dataList = new List<List<Vector2>>();
        SetGraph(true);
    }
    protected List<List<Vector2>> dataList = new List<List<Vector2>>();
    public override void SetGraph()
    {
        base.SetGraph();
        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(set.width, set.hight);
        rect.pivot = Vector2.zero;
        if (set.rulerSet.Count >= 2)
        {
            var left = new List<MultipleData>();
            var right = new List<MultipleData>();
            for (int i = 0; i < charts.Count; i++)
            {
                if (charts[i].isLeftRuler)
                {
                    for (int j = 0;j<charts[i].dataIndex.Count;j++)
                    {
                        left.Add(datas[charts[i].dataIndex[j]]);
                    }
                }
                else
                {
                    for (int j = 0; j < charts[i].dataIndex.Count; j++)
                    {
                        right.Add(datas[charts[i].dataIndex[j]]);
                    }
                }
            }
            var maxMin = Yjj_ChartUtility.ComputeMaxAndMin(left);
            set.rulerSet[0].SetMaxValue(maxMin.maxValue, set);
            set.rulerSet[0].min = maxMin.minValue;
            maxMin = Yjj_ChartUtility.ComputeMaxAndMin(right);
            set.rulerSet[1].SetMaxValue(maxMin.maxValue, set);
            set.rulerSet[1].min = maxMin.minValue;

        }
        else
        {
            //设置标尺
            SetMaxAndMinForTwoData(set, datas);
        }
        //基础图表绘制
        var baseGraph = transform.Find("base");
        if (baseGraph == null)
        {
            baseGraph = new GameObject("base", typeof(Yjj_GraphPopulateMeshBase)).transform;
            baseGraph.parent = transform;
            var br = baseGraph.GetOrAddComponent<RectTransform>();
            br.anchorMin = Vector2.zero;
            br.anchorMax = Vector2.zero;
            br.pivot = Vector2.zero;
            br.anchoredPosition = Vector2.zero;
        }
        var gp = baseGraph.GetComponent<Yjj_GraphPopulateMeshBase>();
        gp.SetGraph(set, dataSet);
        if (datas.Count <= 0)
        {
            return;
        }
        for (int i = 0; i < charts.Count; i++)
        {
            if (charts == null) continue;
            var ci = charts[i].rectTransform();
            ci.anchorMax = Vector2.zero;
            ci.anchorMin = Vector2.zero;
            ci.pivot = Vector2.zero;
            ci.anchoredPosition = Vector2.zero;
            charts[i].SetGraph(this);
        }
      
        hoverSet.SetHover(transform, set, dataSet, datas[0].datas.Count, (index) =>
        {
            List<string> values = new List<string>();
            for (int i = 0; i < datas.Count; i++)
            {
                values.Add(datas[i].datas[index].ToString());
            }
            HoverEvent?.Invoke(index);
            return values;
        }, (index) =>
        {
            HoverExitEvent?.Invoke(index);
        }, (index) =>
        {
            string name = dataSet.names[index];
            GetHoverNameEvent?.Invoke(name);
            return name;
        });

    }
    public override void PlayAnimation()
    {
        base.PlayAnimation();
        for(int i = 0; i < charts.Count; i++)
        {
            var chart = charts[i];
            if(chart!=null && chart.gameObject.activeInHierarchy)
            {
                chart.PlayAnimation();
            }
        }
    }
}
