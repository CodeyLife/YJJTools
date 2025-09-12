using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class ChartComponent : Graphic
{
    private RectTransform _rect;

    public RectTransform Rect { get
        {
            if(_rect == null)
            {
                _rect = transform.GetOrAddComponent<RectTransform>();
            }
            return _rect;
        }
        set => _rect = value; }
    public virtual void InitGraph()
    {
        Rect.anchorMin = Vector2.zero;
        Rect.anchorMax = Vector2.one;
        Rect.pivot = Vector2.zero;
        Rect.sizeDelta = Vector2.zero;
        Rect.anchoredPosition = Vector2.zero;
    }
    public virtual void SetGraph()
    {
        
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        vh.Clear();
    }
    [OnInspectorGUI]
    protected void OnInspectorGui()
    {
        if (GUI.changed)
        {
            this.Delay(() =>
            {
                var chart = transform.parent.GetComponent<ChartBase>();
                if (chart != null)
                {
                    chart.SetGraph();
                }
            });
        }
    }
}
