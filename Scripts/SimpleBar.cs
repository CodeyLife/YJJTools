using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

[RequireComponent(typeof(CanvasRenderer))]
public class SimpleBar : Graphic
{
    private List<Vector2> datas;
    private float width;
    private Color barColor;
    private Color hoverColor;
    private int hoverIndex = -1;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        vh.Clear();
        if (datas == null) return;
        for (int i = 0; i < datas.Count; i++)
        {
            if(i == hoverIndex)
            {
                Yjj_ChartUtility.DrawLine(vh, new Vector2(datas[i].x, 0), datas[i], width, hoverColor);
            }
            else
            {
                Yjj_ChartUtility.DrawLine(vh, new Vector2(datas[i].x, 0), datas[i], width, barColor);
            }
        }

    }
    public void SetGraph(List<Vector2> data, float w, Color c)
    {
        datas = data;
        width = w; barColor = c;
        SetVerticesDirty();
    }
    public void SetGraph(List<Vector2> data, float w, Color c,int index,Color hColor)
    {
        datas = data;
        width = w; barColor = c;
        hoverIndex = index;hoverColor = hColor;
        SetVerticesDirty();
    }
    public void SetGraph(List<Vector2> data)
    {
        datas = data;
        SetVerticesDirty();
    }
}
