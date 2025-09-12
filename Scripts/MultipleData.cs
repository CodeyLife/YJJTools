using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MultipleData
{
    public List<float> datas = new List<float>();
    public MultipleData() { }
    public MultipleData(List<float> list)
    {
        datas = list;
    }
    /// <summary>
    /// 传入list<float>数组 返回用于图表的数据类型
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public static List<MultipleData> GetDatas(params List<float>[] values)
    {
        List<MultipleData> datas = new List<MultipleData>();
        for (int i = 0; i < values.Length; i++)
        {
            datas.Add(new MultipleData(values[i]));
        }
        return datas;
    }
    public static List<MultipleData> GetDatas(List<List<float>> values)
    {
        List<MultipleData> datas = new List<MultipleData>();
        for (int i = 0; i < values.Count; i++)
        {
            datas.Add(new MultipleData(values[i]));
        }
        return datas;
    }
}
