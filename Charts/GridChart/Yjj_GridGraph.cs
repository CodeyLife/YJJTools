using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

public class Yjj_GridGraph : ChartBase
{
    [FoldoutGroup("基础设置")]
    public BaseSet set;
    [FoldoutGroup("基础设置")]
    public HoverSet hoverSet = new HoverSet();
    [FoldoutGroup("基础设置")] public List<string> titles = new List<string>();
    [FoldoutGroup("数据设置")]
    public DataSet dataSet;
    [FoldoutGroup("数据设置")]
    public List<MultipleData> datas;
    [FoldoutGroup("数据设置")]
    public float radiu = 5f;
    [FoldoutGroup("数据设置")]
    public float space = 5;
    [FoldoutGroup("数据设置")]
    public Material gridMat;
    [FoldoutGroup("数据设置")] public List<Color> colors = new List<Color>() { Color.green, Color.yellow, Color.red };
    [FoldoutGroup("数据设置")] public List<float> values = new List<float>() { 0, 50, 100 };

    public override void SetData(List<MultipleData> data, List<string> Names)
    {
        base.SetData(data, Names);
        datas = data;
        dataSet.names = Names;
        SetGraph();
    }
    public override  void SetGraph()
    {
        //基础图表绘制
        var baseGraph = transform.Find("base");
        if (baseGraph == null)
        {
            baseGraph = new GameObject("base", typeof(Yjj_GraphPopulateMeshForGrid)).transform;
            baseGraph.parent = transform;
            var br = baseGraph.GetOrAddComponent<RectTransform>();
            br.anchorMin = Vector2.zero;
            br.anchorMax = Vector2.zero;
            br.pivot = Vector2.zero;
            br.anchoredPosition = Vector2.zero;
        }
        var gp = baseGraph.GetComponent<Yjj_GraphPopulateMeshForGrid>();
        gp.SetGraph(set, dataSet,titles);
        if (datas.Count == 0) return;


        //生成grid
        var grid = transform.GetOrCreatUIChild<RectTransform>("grid",(rect)=>
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
        });
        grid.sizeDelta = transform.rectTransform().sizeDelta;
        var draw = grid.transform.GetOrAddComponent<GridDrawer>();
        draw.grid = this;
        draw.material = gridMat;
        draw.SetAllDirty();
    }

}
