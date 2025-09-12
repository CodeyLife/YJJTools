using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(CanvasRenderer))]
public class ChartV2ComponetBase : MaskableGraphic,IChartV2Component
{
    protected ChartV2Base _v2Base;

    /// <summary>
    /// 初始化脚本
    /// </summary>
    /// <param name="chart"></param>
    public virtual void InitGraph(ChartV2Base chart)
    {
        raycastTarget = false;
        _v2Base = chart;
        //SetAllDirty();
        SetVerticesDirty();
    }

#if UNITY_EDITOR
    public virtual void OnCreat() { }
#endif

    public virtual void SetGraph()
    {
        //SetAllDirty();
        SetVerticesDirty();
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_v2Base == null) return;
    }

}
