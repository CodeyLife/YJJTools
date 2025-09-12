using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YJJTool;

public class FreeDrawLine : DrawFreeChartBase
{
    protected List<List<Vector2>> dataList = new List<List<Vector2>>();
    protected bool left;
    protected bool right;
    [LabelText("补全前后数据")]
    protected bool autoFrontAndBack = true;
    public LineSet lineset = new LineSet();
    public override void SetGraph(FreeChart root)
    {
        base.SetGraph(root);
        //获取数据点
        dataList.Clear();
        int index = isLeftRuler ? 0 : 1;
        for (int i = 0; i < dataIndex.Count; i++)
        {
            dataList.Add(Yjj_ChartUtility.GetPosFromData(root.datas[dataIndex[i]].datas, root.set, root.dataSet, index, true, false));
        }
        left = root.dataSet.distanceFormLeft > 0;
        right = root.dataSet.distanceFormRight > 0;
        for (int i = 0; i < dataList.Count; i++)
        {
            var line = transform.GetOrCreatUIChild("line" + i, true, typeof(Yjj_Line)).GetComponent<Yjj_Line>();
            var arr = dataList[i];
            if (autoFrontAndBack)
            {
                var temp = new List<Vector2>();
                var left = arr[0] * 2 - arr[1];
                left = new Vector2(left.x, Mathf.Clamp(left.y, 0, chart.set.hight));
                temp.Add (left);
                for (int m = 0; m < arr.Count; m++)
                {
                    temp.Add(arr[m]);
                }
                var right = arr[arr.Count - 1] * 2 - arr[arr.Count - 2];
                temp.Add(new Vector2(right.x,Mathf.Clamp(right.y,0,chart.set.hight)));
                arr = temp;
            }
     
            line.SetGraph(arr, lineset, false, false, i, root.datas[dataIndex[i]].datas);
        }
    }
    public override void PlayAnimation()
    {
        base.PlayAnimation();
        var temp = new List<List<Vector2>>();
        for (int i = 0; i < dataList.Count; i++)
        {
            temp.Add(new List<Vector2>());
        }
        StartCoroutine(YjjUtility.FadeIn(Chart.animationSet.fadeInTime, (t) =>
        {
            FadeList(t, ref temp, dataList);
            for (int i = 0; i < dataList.Count; i++)
            {
                var line = transform.Find("line" + i).GetComponent<Yjj_Line>();
                line.SetGraph(temp[i], lineset, false, false, i, Chart.datas[dataIndex[i]].datas);
            }
        }));
    }
}
