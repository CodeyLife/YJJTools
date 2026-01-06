using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class ReadSingleSheetStander : ReadExcelBase
{
    public enum DataType
    {
        floatList,
        MultipleList,
    }
    [LabelText("图表")]
    public GameObject chart;
    [InfoBox("如果图表继承自chartbase该值可为空，否则填写图表的类型名")]
    public string typeName;
    [LabelText("图表的数据类型")]
    public DataType dataType = DataType.floatList;
    [LabelText("图表标题所在的列")]
    public int nameIndex = 0;
    [Title("需要添加到数据的excel的列",TitleAlignment = TitleAlignments.Centered)]
    public List<int> columns = new List<int>();
    protected override void ReadTable(DataTable table)
    {
        base.ReadTable(table);
        Type type = null;
        if (string.IsNullOrEmpty(typeName))
        {
            type = typeof(ChartBase);
        }
        else
        {
            type = Type.GetType(typeName);
        }
        List<string> names = new List<string>();
        switch (dataType)
        {
            case DataType.floatList:
                List<float> singles = new List<float>();
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    names.Add(table.Rows[i][nameIndex].ToString());
                    singles.Add(Parse(table.Rows[i][columns[0]].ToString()));

                }
                var ch = chart.GetComponent(type);
                var me = ch.GetType().GetMethod("SetData",new Type[] { typeof(List<float>), typeof(List<string>) });
                me.Invoke(ch, new object[] { singles, names });
                break;
            case DataType.MultipleList:
                List<MultipleData> datas = new List<MultipleData>();
                for(int i = 0; i < columns.Count; i++)
                {
                    datas.Add(new MultipleData());
                }
                for(int i = 0; i < table.Rows.Count; i++)
                {
                    names.Add(table.Rows[i][nameIndex].ToString());
                    for (int j = 0; j < columns.Count; j++)
                    {
                        datas[j].datas.Add(Parse(table.Rows[i][columns[j]].ToString()));
                    }
                }
                var c = chart.GetComponent(type);
                var tt = c.GetType();
                var method =   tt.GetMethod("SetData",new Type[] { typeof(List<MultipleData>), typeof(List<string>) });
                method.Invoke(c, new object[] { datas,names });
                break;
        }
    }
}