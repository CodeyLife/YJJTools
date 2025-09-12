using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChartV2Component 
{
    /// <summary>
    /// 重构图表参数时调用
    /// </summary>
    /// <param name="chart"></param>
    public abstract void InitGraph(ChartV2Base chart);
    /// <summary>
    /// 频繁调用 如动画 drag
    /// </summary>
    public abstract void SetGraph();

#if UNITY_EDITOR
    public abstract void OnCreat();
#endif

}
public class ComponentOrderAttribute : Attribute
{
    public int order;
    public ComponentOrderAttribute(int value)
    {
        order = value;
    }
}
