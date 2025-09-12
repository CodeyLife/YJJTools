using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class PathUtility
{
    /// <summary>
    /// 传入完整路径，返回相对于Assets的路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="containsAsset"></param>
    /// <returns></returns>
    public static string GetRelativeAsset(string path,bool containsAsset = true)
    {
        if (containsAsset)
        {
            return Regex.Match(path, @"Assets.*").Value;
        }
        else
        {
            path = path.Replace("\\", "/");
            return Regex.Match(path, @"(?<=Assets/).*").Value;
        }
    }
    /// <summary>
    /// 传入相对与Assets的路径，获取完整路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetFullPath(string path)
    {
        var str = Application.dataPath;
      //  path = path.Replace("Assets", "");
        path = Regex.Replace(path, @"^Assets", "");
        str += path;
        return str;
    }
    /// <summary>
    /// 获取文件夹下的所有文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filePath"></param>
    public static void GetAllDirectoryFiles(string path, List<string> filePath)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        //检索表示当前目录的文件和子目录
        FileSystemInfo[] fsinfos = dir.GetFileSystemInfos();
        //遍历检索的文件和子目录
        foreach (FileSystemInfo fsinfo in fsinfos)
        {
            //判断是否为空文件夹　　
            if (fsinfo is DirectoryInfo)
            {
                //递归调用
                GetAllDirectoryFiles(fsinfo.FullName, filePath);
            }
            else
            {
                //将得到的文件全路径放入到集合中
                filePath.Add(fsinfo.FullName);
            }
        }
    }
    public static List<string> GetAllDirectoryFiles(string path)
    {
        List<string> filePath = new List<string>();
        DirectoryInfo dir = new DirectoryInfo(path);
        //检索表示当前目录的文件和子目录
        FileSystemInfo[] fsinfos = dir.GetFileSystemInfos();
        //遍历检索的文件和子目录
        foreach (FileSystemInfo fsinfo in fsinfos)
        {
            //判断是否为空文件夹　　
            if (fsinfo is DirectoryInfo)
            {
                //递归调用
                GetAllDirectoryFiles(fsinfo.FullName, filePath);
            }
            else
            {
                //将得到的文件全路径放入到集合中
                filePath.Add(fsinfo.FullName);
            }
        }
        return filePath;
    }
}
