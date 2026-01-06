using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using System.Text.RegularExpressions;
using System.IO;

public class ReadExcelBase : MonoBehaviour
{
    [FilePath(AbsolutePath = false,Extensions = ".xlsx",ParentFolder = "@UnityEngine.Application.streamingAssetsPath"),Required(errorMessage:"地址不能为空!")]
    [InlineButton("OpenExcel",label:"open")]
    public string excelPath;
    [LabelText("从第几行开始读")]
    public int row = 0;
    [LabelText("读第几个sheet")]
    public int sheet = 0;
    [LabelText("保留首行数据")]
    public bool saveFirstRow = true;
    public const string patten = @"[()（）/&，、—]";
    protected virtual  async  void Awake()
    {
        var path = System.IO.Path.Combine(Application.streamingAssetsPath, excelPath);
        var table = await  Yjj_ExcelUtility.ReadExcelToTable(path, sheet, row,saveFirst:saveFirstRow);
        ReadTable(table);
    }
    private void OpenExcel()
    {
        var path = Path.Combine(Application.streamingAssetsPath, excelPath);
        System.Diagnostics.Process.Start(path);
    }
    protected virtual void ReadTable(System.Data.DataTable table) { }
    /// <summary>
    /// 将字符串转为float添加进list，如果失败添加0
    /// </summary>
    /// <param name="list"></param>
    /// <param name="value"></param>
    public static void ParseAndAdd(List<float> list, string value)
    {
        list.Add(value.ParseAnyway());
    }
    /// <summary>
    /// 将字符串转为float 如果失败返回0
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static float Parse(string value)
    {
        float result = 0;
        if (float.TryParse(value, out float r))
        {
            result = r;
        }
        return result;
    }
    public static string GetStreamingAssetWithPath(string path)
    {
        return System.IO.Path.Combine(Application.streamingAssetsPath, path);
    }
    /// <summary>
    /// 从dataTable生成对应类的数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="table"></param>
    /// <returns></returns>
    public static List<T> GetDatas<T>(DataTable table) where T:new()
    {
        List<T> datas = new List<T>();
        var type = typeof(T);
        var fields = type.GetFields();
        Dictionary<string, int> valueDic = new Dictionary<string, int>();
        for (int i = 0; i < fields.Length; i++)
        {
            var name = fields[i].Name;
            for (int j = 0; j < table.Columns.Count; j++)
            {
                string tableName = table.Columns[j].ColumnName;
                tableName = Regex.Replace(tableName, patten, string.Empty);
                if (tableName == name)
                {
                    valueDic.Add(name, j);
                    break;
                }
            }
        }
        for (int i = 0; i < table.Rows.Count; i++)
        {
            T data = new T();
            foreach(var f in fields)
            {
                if (f.GetCustomAttribute(typeof(NotReadFieldAttribute)) != null) continue;
#if UNITY_EDITOR
                if (!valueDic.Keys.Contains(f.Name))
                {
                    Debug.Log($"该表格不包含:<color=red>{f.Name}</color>");
                    string str = "";
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        str += table.Columns[j].ColumnName +"|";
                    }
                    Debug.Log(str);
                }
#endif
                var index = valueDic[f.Name];
             //   f.SetValue(data, table.Rows[i][index]);
                SetData(data, f, table.Rows[i][index].ToString());
            }
            datas.Add(data);
        }
        return datas;
    }
    private static void SetData(object data, FieldInfo f, string value)
    {
        if (f.FieldType == typeof(string))
        {
            f.SetValue(data, value);
        }
        else if (f.FieldType == typeof(int))
        {
            var intValue = 0;
            int.TryParse(value, out intValue);
            f.SetValue(data, intValue);
        }
        else if (f.FieldType == typeof(float))
        {
            var floatValue = value.ParseAnyway();
            f.SetValue(data, floatValue);
        }
        else if (f.FieldType == typeof(double))
        {
            double doubleValue = 0;
            double.TryParse(value, out doubleValue);
            f.SetValue(data, doubleValue);
        }
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class ToListAttribute : Attribute { }
    /// <summary>
    /// 把带有tolist 标记的字段转为list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="names"></param>
    /// <returns></returns>
    public static List<T> ToList<T>(object obj,List<string> names = null)
    {
        var type = obj.GetType();
        var fields = type.GetFields();
        List<T> values = new List<T>();
        foreach(var filed in fields)
        {
            if (filed.GetCustomAttribute<ToListAttribute>() != null)
            {
                names?.Add(filed.Name);
                values.Add((T)filed.GetValue(obj));
            }
        }
        return values;
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class NotReadFieldAttribute : System.Attribute
    {

    }
}
