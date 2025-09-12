using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawFreeChartBase : MonoBehaviour
{
    [LabelText("数据所在的索引列表")]
    public List<int> dataIndex = new List<int>();
    [LabelText("使用左侧标尺")]
    public bool isLeftRuler = true;
    protected FreeChart chart;

    protected FreeChart Chart { get
        {
            if(chart == null)
            {
                chart = transform.parent.GetComponent<FreeChart>();
            }
            return chart;
        }
        set => chart = value; }

    public virtual void SetGraph(FreeChart root)
    {
        StopAllCoroutines();
        Chart = root;
    }

    protected  bool CheckData(FreeChart root)
    {
        for (int i = 0; i < dataIndex.Count; i++)
        {
            if (dataIndex[i] >= root.datas.Count)
            {
                gameObject.SetActive(false);
                return false;
            }
        }
        gameObject.SetActive(true);
        return true;
    }
    public virtual void PlayAnimation()
    {

    }
    protected void FadeList(float t, ref List<List<Vector2>> temp, List<List<Vector2>> target)
    {
        for (int i = 0; i < temp.Count; i++)
        {
            for (int j = 0; j < target[i].Count; j++)
            {
                if (temp[i].Count <= j)
                {
                    temp[i].Add(new Vector2(target[i][j].x, Mathf.Lerp(0, target[i][j].y, t)));
                }
                else
                {
                    temp[i][j] = new Vector2(target[i][j].x, Mathf.Lerp(0, target[i][j].y, t));
                }
            }
        }
    }
    protected void FadeList(float t, List<Vector2> resultList, ref List<Vector2> list)
    {
        for (int i = 0; i < resultList.Count; i++)
        {
            if (list.Count <= i)
            {
                list.Add(new Vector2(resultList[i].x, Mathf.Lerp(0, resultList[i].y, t)));
            }
            else
            {
                list[i] = new Vector2(resultList[i].x, Mathf.Lerp(0, resultList[i].y, t));
            }
        }
    }
    protected void FadeArr(float t, Vector2[] source, ref Vector2[] target)
    {
        for (int i = 0; i < source.Length; i++)
        {
            target[i] = new Vector2(source[i].x, Mathf.Lerp(0, source[i].y, t));
        }
    }
    #region Inspector
    [OnInspectorGUI]
    private void OnGuiChange()
    {
        if (GUI.changed)
        {
            StartCoroutine(YjjUtility.DeLay(() =>
            {
                Chart.SetGraph();
            }));
        }
    }
    #endregion
}
