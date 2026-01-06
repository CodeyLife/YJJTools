# if UNITY_EDITOR
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class ReadExcelDemo : ReadExcelBase
{
    public ChartV2Base chart;
    protected override void ReadTable(DataTable table)
    {
        //base.ReadTable(table);
        var datas = new List<MultipleData>();
        var titles = new List<string>();
        //遍历每一横排  //从需要的数据的index开始
        for (int i = 0; i < table.Rows.Count; i++)
        {
            var data = new MultipleData();
            //标题是第0横排
        
            //遍历每一竖排
            for (int j = 1; j < table.Columns.Count; j++)
            {
                titles.Add(table.Rows[0][j].ToString());
                data.datas.Add(Parse(table.Rows[i][j].ToString()));

            }
        
            datas.Add(data);
        }

        chart.SetGraph(datas, titles);
    }
}
#endif