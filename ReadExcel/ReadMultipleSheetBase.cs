using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadMultipleSheetBase : ReadExcelBase
{
    [LabelText("读取到第几个sheet结束")]
    public int endSheet;
    protected override async void Awake()
    {
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, excelPath);
        var table = await Yjj_ExcelUtility.ReadExcelToTableList(path, sheet,endSheet, row,saveFirst:saveFirstRow);
        ReadTable(table);
    }
    protected virtual void ReadTable(List<System.Data.DataTable> tables) { }
}
