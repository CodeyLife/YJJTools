#if UNITY_STANDALONE_WIN
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;

public class Yjj_ExcelUtility
{
    /// <summary>  
    /// 将excel导入到datatable  
    /// </summary>  
    /// <param name="filePath">excel路径</param>  
    /// <param name="isColumnName">第一行是否是列名</param>  
    /// <returns>返回datatable</returns>  
    public static DataTable ExcelToDataTable(string filePath, int sheetIndex, int startRow = 0, int lastRow = 0,bool saveFirst = true)
    {
#if UNITY_STANDALONE_WIN
        DataTable dataTable = null;
        FileStream fs = null;
        DataColumn column = null;
        DataRow dataRow = null;
        IWorkbook workbook = null;
        ISheet sheet = null;
        IRow row = null;
        ICell cell = null;
        //try
        //{
        //    //using (fs = File.OpenRead(filePath))
        //    using (fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        //    {
        //        // 2007版本  
        //        if (filePath.IndexOf(".xlsx") > 0)
        //            workbook = new XSSFWorkbook(fs);
        //        // 2003版本  
        //        else if (filePath.IndexOf(".xls") > 0)
        //            workbook = new HSSFWorkbook(fs);

        //        if (workbook != null)
        //        {
        //            sheet = workbook.GetSheetAt(sheetIndex);//读取第一个sheet，当然也可以循环读取每个sheet  
        //            dataTable = new DataTable();
        //            dataTable.TableName = sheet.SheetName;
        //            if (sheet != null)
        //            {
        //                int rowCount = lastRow == 0 ? sheet.LastRowNum : lastRow;//总行数  
        //                if (rowCount > 0)
        //                {
        //                    IRow firstRow = sheet.GetRow(startRow);//第一行  
        //                     // var firstCellNum = firstRow.FirstCellNum;
        //                    var firstCellNum = 0;  //从哪一列开始读
        //                    int cellCount = firstRow.LastCellNum - firstCellNum;//列数
        //                    for (int i = firstCellNum; i < cellCount; ++i)
        //                    {
        //                        cell = firstRow.GetCell(i);
        //                        string str = null;
        //                        if(cell == null || dataTable.Columns.Contains(cell.StringCellValue))
        //                        {
        //                            str = $"columns{i}";
        //                        }
        //                        else
        //                        {
        //                            str = cell.StringCellValue;
        //                        }
        //                        column = new DataColumn(str);
        //                        dataTable.Columns.Add(column);
        //                    }
        //                    startRow = saveFirst ? startRow : startRow + 1;
        //                    //填充行  
        //                    for (int i = startRow; i <= rowCount; ++i)
        //                    {

        //                        row = sheet.GetRow(i);
        //                        if (row == null) continue;

        //                        dataRow = dataTable.NewRow();
        //                        for (int j = firstCellNum; j < cellCount; ++j)
        //                        {
        //                            cell = row.GetCell(j);
        //                            GetCellRowValue(cell, dataRow, j);
        //                        }
        //                        dataTable.Rows.Add(dataRow);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    return dataTable;
        //}
        //catch (Exception e)
        //{
        //    Debug.Log(e.ToString());
        //    if (fs != null)
        //    {
        //        fs.Close();
        //    }
        //    return null;
        //}
        //using (fs = File.OpenRead(filePath))
        using (fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            // 2007版本  
            if (filePath.IndexOf(".xlsx") > 0)
                workbook = new XSSFWorkbook(fs);
            // 2003版本  
            else if (filePath.IndexOf(".xls") > 0)
                workbook = new HSSFWorkbook(fs);

            if (workbook != null)
            {
                sheet = workbook.GetSheetAt(sheetIndex);//读取第一个sheet，当然也可以循环读取每个sheet  
                dataTable = new DataTable();
                dataTable.TableName = sheet.SheetName;
                if (sheet != null)
                {
                    int rowCount = lastRow == 0 ? sheet.LastRowNum : lastRow;//总行数  
                    if (rowCount > 0)
                    {
                        IRow firstRow = sheet.GetRow(startRow);//第一行  
                                                               // var firstCellNum = firstRow.FirstCellNum;
                        var firstCellNum = 0;  //从哪一列开始读
                        int cellCount = firstRow.LastCellNum - firstCellNum;//列数
                        for (int i = firstCellNum; i < cellCount; ++i)
                        {
                            cell = firstRow.GetCell(i);
                            string str = null;
                            if (cell == null || dataTable.Columns.Contains(cell.ToString()))
                            {
                                str = $"columns{i}";
                            }
                            else
                            {
                                str = cell.ToString();
                            }
                            column = new DataColumn(str);
                            dataTable.Columns.Add(column);
                        }
                        startRow = saveFirst ? startRow : startRow + 1;
                        //填充行  
                        for (int i = startRow; i <= rowCount; ++i)
                        {

                            row = sheet.GetRow(i);
                            if (row == null) continue;

                            dataRow = dataTable.NewRow();
                            for (int j = firstCellNum; j < cellCount; ++j)
                            {
                                cell = row.GetCell(j);
                                GetCellRowValue(cell, dataRow, j);
                            }
                            dataTable.Rows.Add(dataRow);
                        }
                    }
                }
            }
        }
        return dataTable;
#else

     return null;
#endif
    }
    /// <summary>
    /// 读取excel的所有sheet
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="startRow"></param>
    /// <param name="lastRow"></param>
    /// <returns></returns>
    public static List<DataTable> ExcelToDataTables(string filePath,int startRow = 0,int lastRow = 0)
    {
#if UNITY_STANDALONE_WIN
        DataTable dataTable = null;
        FileStream fs = null;
        DataColumn column = null;
        DataRow dataRow = null;
        IWorkbook workbook = null;
        ISheet sheet = null;
        IRow row = null;
        ICell cell = null;
        try
        {
            List<DataTable> tables = new List<DataTable>();
            //using (fs = File.OpenRead(filePath))
            using (fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // 2007版本  
                if (filePath.IndexOf(".xlsx") > 0)
                    workbook = new XSSFWorkbook(fs);
                // 2003版本  
                else if (filePath.IndexOf(".xls") > 0)
                    workbook = new HSSFWorkbook(fs);

                if (workbook != null)
                {
                    int count = workbook.NumberOfSheets;
                    for(int s = 0; s < count; s++)
                    {
                        sheet = workbook.GetSheetAt(s);//读取第一个sheet，当然也可以循环读取每个sheet  
                        if (sheet != null)
                        {
                            dataTable = new DataTable();
                            dataTable.TableName = sheet.SheetName;
                            int rowCount = lastRow == 0 ? sheet.LastRowNum : lastRow;//总行数  
                            if (rowCount > 0)
                            {
                                IRow firstRow = sheet.GetRow(startRow);//第一行  
                                int cellCount = firstRow.LastCellNum;//列数  
                                for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                                {
                                    cell = firstRow.GetCell(i);
                                    if (cell != null)
                                    {
                                        string str = i.ToString();
                                        column = new DataColumn(str);
                                        dataTable.Columns.Add(column);
                                    }
                                }
                                //填充行  
                                for (int i = startRow; i <= rowCount; ++i)
                                {

                                    row = sheet.GetRow(i);
                                    if (row == null) continue;

                                    dataRow = dataTable.NewRow();
                                    for (int j = row.FirstCellNum; j < cellCount; ++j)
                                    {
                                        cell = row.GetCell(j);
                                        GetCellRowValue(cell, dataRow, j);
                                    }
                                    dataTable.Rows.Add(dataRow);
                                }
                            }
                            tables.Add(dataTable);
                        }
                    }
                }
            }
            return tables;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            if (fs != null)
            {
                fs.Close();
            }
            return null;
        }
#else
      return null;
#endif
    }

    /// <summary>
    /// 异步读取Excel
    /// </summary>
    /// <param name="path"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="startRow"></param>
    /// <param name="endRow"></param>
    /// <returns></returns>
    public async static Task<List<DataTable>> ReadExcelToTableList(string path,int start,int end,int startRow = 0,int endRow = 0,bool saveFirst = true)
    {
#if UNITY_STANDALONE_WIN
        var tableList =  await Task.Run(() =>
        {
            List<DataTable> list = new List<DataTable>();
            end++;
            for (int i = start; i < end; i++)
            {
                list.Add(ExcelToDataTable(path,i, startRow, endRow, saveFirst));
            }
            return list;
        });
        return tableList;
#else
        return null;
#endif
    }
    public async static Task<List<DataTable>> ReadExcelToTableList(string path, int startRow = 0, int endRow = 0)
    {
#if UNITY_STANDALONE_WIN
        var list = await Task.Run(() =>
        {
            var datas = ExcelToDataTables(path, startRow, endRow);
            return datas;
        });
        return list;
#else
        return null;
#endif
    }
    public async static Task<DataTable> ReadExcelToTable(string path, int index, int startRow = 0, int endRow = 0,bool saveFirst = true)
    {
#if UNITY_STANDALONE_WIN
        //DataTable tableList = await Task.Run(() =>
        //{
        //    DataTable dt = ExcelToDataTable(path, index, startRow, endRow);
        //    Debug.Log(dt.Rows.Count);
        //    return dt;
        //});
        //Debug.Log(tableList);
        var t = await Task<DataTable>.Run(() =>
       {
           DataTable dt = ExcelToDataTable(path, index, startRow, endRow,saveFirst);
           return dt;
       });
      //  var tableList = t.Result;
        return t;
#else
        return null;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dt"></param>
    /// <param name="excePath"></param>
    /// <param name="sheetIndex"></param>
    /// <param name="writeIndex">如果小于0 则在表格末尾添加不覆盖以前的数据</param>
    /// <returns></returns>
    public static bool WriteExcel(DataTable dt, string excePath, int sheetIndex, int writeIndex = 0)
    {
#if UNITY_STANDALONE_WIN
        bool result = false;
        IWorkbook workbook = null;
        FileStream fs = null;
        IRow row = null;
        ISheet sheet = null;
        ICell cell = null;
        try
        {
            using (fs = File.OpenRead(excePath))
            {
                // 2007版本  
                if (excePath.IndexOf(".xlsx") > 0)
                    workbook = new XSSFWorkbook(fs);
                // 2003版本  
                else if (excePath.IndexOf(".xls") > 0)
                    workbook = new HSSFWorkbook(fs);
                if (dt != null && workbook != null)
                {
                    sheet = workbook.GetSheetAt(sheetIndex);
                    int rowCount = dt.Rows.Count;//行数  
                    int columnCount = dt.Columns.Count;//列数  
                    int startIndex = writeIndex > 0 ? writeIndex : sheet.LastRowNum + 1;
                    //设置列头  
                    row = sheet.CreateRow(startIndex);//excel第一行设为列头  
                    for (int c = 0; c < columnCount; c++)
                    {
                        cell = row.CreateCell(c);
                    }
                    //设置每行每列的单元格,  
                    for (int i = 0; i < rowCount; i++)
                    {
                        row = sheet.CreateRow(startIndex + i);
                        for (int j = 0; j < columnCount; j++)
                        {
                            cell = row.CreateCell(j);//excel第二行开始写入数据  
                            cell.SetCellValue(dt.Rows[i][j].ToString());
                        }
                    }
                    using (fs = File.OpenWrite(excePath))
                    {
                        workbook.Write(fs);//向打开的这个xls文件中写入数据  
                        result = true;
                    }
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            if (fs != null)
            {
                fs.Close();
            }
            return false;
        }
#else
        return false;
#endif
    }
    public static bool WriteExcel(List<object[]> list, string excePath, int sheetIndex, int writeIndex = 0)
    {
#if UNITY_STANDALONE_WIN
        bool result = false;
        IWorkbook workbook = null;
        FileStream fs = null;
        IRow row = null;
        ISheet sheet = null;
        ICell cell = null;
        try
        {
                // 2007版本  
                if (excePath.IndexOf(".xlsx") > 0)
                    workbook = new XSSFWorkbook();
                // 2003版本  
                else if (excePath.IndexOf(".xls") > 0)
                    workbook = new HSSFWorkbook();
                if (workbook != null && list.Count>0)
                {
                    sheet = workbook.CreateSheet("数据");
                    int rowCount = list.Count;//行数  
                    int columnCount = list[0].Length;//列数  
                    int startIndex = writeIndex > 0 ? writeIndex : sheet.LastRowNum + 1;
                    //设置列头  
                    row = sheet.CreateRow(0);//excel第一行设为列头  
                    for (int c = 0; c < columnCount; c++)
                    {
                        cell = row.CreateCell(c);
                    row.CreateCell(0).SetCellValue("数据名称");
                    row.CreateCell(1).SetCellValue("数据1");
                    row.CreateCell(2).SetCellValue("数据2");
                    row.CreateCell(3).SetCellValue("颜色");
                    }
                    //设置每行每列的单元格,  
                    for (int i = 0; i < rowCount; i++)
                    {
                        row = sheet.CreateRow(i+1);
                        for (int j = 0; j < columnCount; j++)
                        {
                            if(list[i].Length<= j|| list[i][j] == null)
                            {
                                continue;
                            }
                            cell = row.CreateCell(j);//excel第二行开始写入数据  
                            cell.SetCellValue(list[i][j].ToString());
                        }
                    }
                    using (fs = File.Create(excePath))
                    {
                        workbook.Write(fs);//向打开的这个xls文件中写入数据  
                        result = true;
                    }
                }
            return result;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            if (fs != null)
            {
                fs.Close();
            }
            return false;
        }
#else
        return false;
#endif
    }
#if UNITY_STANDALONE_WIN
    private static string GetCellColumnValue(ICell cell)
    {
        if (cell == null)
        {
            return "";
        }
        //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)  
        switch (cell.CellType)
        {
            case CellType.Blank:
                return "";
            case CellType.Numeric:
                short format = cell.CellStyle.DataFormat;
                //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理  
                if (format == 14 || format == 31 || format == 57 || format == 58)
                    return cell.DateCellValue.ToString();
                else
                    return cell.NumericCellValue.ToString();
            case CellType.String:
                return cell.StringCellValue;
            default:
                return "";
        }
    }
    private static void GetCellRowValue(ICell cell, DataRow dw, int index)
    {
        if (cell == null)
        {
            dw[index] = "";
            return;
        }
        //CellType(Unknown = -1,Numeric = 0,String = 1,Formula = 2,Blank = 3,Boolean = 4,Error = 5,)  
        switch (cell.CellType)
        {
            case CellType.Blank:
                dw[index] = "";
                break;
            case CellType.Numeric:
                short format = cell.CellStyle.DataFormat;
                //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理  
                if (format == 14 || format == 31 || format == 57 || format == 58)
                    dw[index] = cell.DateCellValue;
                else
                    dw[index] = cell.NumericCellValue;
                break;
            case CellType.String:
                dw[index] = cell.StringCellValue;
                break;
        }
    }
#endif
}
