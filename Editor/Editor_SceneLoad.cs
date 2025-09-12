using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class Editor_SceneLoad 
{
    static  Editor_SceneLoad()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }
    [InitializeOnLoadMethod]
    static void OnProjectLoadedInEditor()
    {
        // 当项目在编辑器中加载时执行
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        CheckAsync(true);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
    }
    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        CheckAsync(false);
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
    }
    private static async Task CheckAsync(bool openTask)
    {
        if (openTask)
        {
            await Task.Delay(2000);
        }
        var charts = GameObject.FindObjectsByType<ChartV2Base>(FindObjectsSortMode.None);

        foreach (var chart in charts)
        {
            // 使用反射调用函数
            chart.GetType().InvokeMember("SetGraph", BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance, null, chart, new object[] { false });

        }
    }
}
