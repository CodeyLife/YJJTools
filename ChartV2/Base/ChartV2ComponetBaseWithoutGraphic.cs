using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChartV2ComponetBaseWithoutGraphic : MonoBehaviour,IChartV2Component
{
    protected ChartV2Base _v2Base;
    /// <summary>
    /// 初始化脚本
    /// </summary>
    /// <param name="chart"></param>
    public virtual void InitGraph(ChartV2Base chart)
    {
     
        _v2Base = chart;
  
    }
    public virtual void SetGraph()
    {

    }


#if UNITY_EDITOR
    public virtual void OnCreat()
    {

    }
#endif
}
