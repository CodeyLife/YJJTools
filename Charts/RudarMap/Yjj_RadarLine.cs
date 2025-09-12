using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

[RequireComponent(typeof(CanvasRenderer))]
public class Yjj_RadarLine : Graphic
{
    public List<float> datas;
    public int max = 1000;
    public float radius = 1;
    Vector3[] posArr;
    public float width = 1;
    public Color line_color = Color.yellow;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        DrawLine(vh);
    }
    public void DrawLine(VertexHelper vh)
    {
        if (posArr == null || posArr.Length != datas.Count)
        {
            posArr = new Vector3[datas.Count];
        }
        float angle = 360f / datas.Count;
        Vector2 lastPos = Vector3.zero;
        List<Vector2> vs = new List<Vector2>();
        for (int i = 0; i < datas.Count; i++)
        {
            Vector2 v1 = Vector3.zero;
            Vector2 v2 = Vector3.zero;
            float cos;
            float sin;
            float value;
            float currentAngle;
            if (lastPos == Vector2.zero)
            {
                value = datas[i] / max;
                currentAngle = angle * i;
                currentAngle *= Mathf.Deg2Rad;
                cos = Mathf.Cos(currentAngle);
                sin = Mathf.Sin(currentAngle);
                v1 = new Vector2(sin * radius * value, cos * radius * value);
                vs.Add(v1);
            }
            else
            {
                v1 = lastPos;
            }

            int nextIndex = i + 1;
            nextIndex = nextIndex == datas.Count ? 0 : nextIndex;

            value = datas[nextIndex] / max;
            currentAngle = angle * nextIndex;
            currentAngle *= Mathf.Deg2Rad;
            cos = Mathf.Cos(currentAngle);
            sin = Mathf.Sin(currentAngle);
            v2 = new Vector2(sin * radius * value, cos * radius * value);
            lastPos = v2;
            vs.Add(v2);
            //  Yjj_ChartUtility.DrawLine(vh, v1, v2, width, line_color);
          //  Yjj_ChartUtility.DrawLineSmooth(vh, v1, v2, width,line_color);
        }
        //vs.Add(vs[0]);

   
        Yjj_ChartUtility.DrawLineSmooth(vh, vs, width, line_color,true);
    }
}
