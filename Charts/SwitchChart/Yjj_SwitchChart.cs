using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YJJTool
{

    public class Yjj_SwitchChart : ChartBase
    {
        [FoldoutGroup("基础设置")]
        public BaseSet set;
        [FoldoutGroup("基础设置")] public List<string> titles = new List<string>();
        [FoldoutGroup("数据设置")]
        public DataSet dataSet;
        [FoldoutGroup("数据设置")]
        public List<bool> datas = new List<bool>();
        [FoldoutGroup("数据设置")]
        public List<string> times = new List<string>();
        [FoldoutGroup("数据设置")]
        public LineSet lineSet = new LineSet();
        [FoldoutGroup("数据设置")]
        public Color onColor = Color.green;
        public Color offColor = Color.red;

        public void SetData(List<bool> data, List<string> times)
        {
            StopAllCoroutines();
            datas = data;
            this.times = times;
            SetGraph();
        }
        public override void SetGraph()
        {
            base.SetGraph();
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
            gp.SetGraph(set, dataSet, titles, false);
            if (datas.Count == 0) return;


            //生成数据表格
            var draw = transform.GetOrCreatUIChild<Yjj_SwitchDrawer>("drawer", (c) =>
             {
                 c.rectTransform.anchorMin = Vector2.zero;
                 c.rectTransform.anchorMax = Vector2.zero;
                 c.rectTransform.pivot = Vector2.zero;
                 c.rectTransform.anchoredPosition = Vector2.zero;
             });
            draw.rectTransform.sizeDelta = this.rectTransform().sizeDelta;
            draw.SetGraph(this);
        }
    }
}