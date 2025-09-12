using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class Yjj_Logger : MonoBehaviour
{


    private void LowMemory()
    {
        Debug.LogError($"低内存报警!");
    }
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    private FileStream fi;
    private void Awake()
    {
        Application.lowMemory += LowMemory;
        Application.logMessageReceived += Log;
        var path = Path.Combine(Application.streamingAssetsPath, "log.txt");
        fi = File.Open(path, FileMode.OpenOrCreate);
        fi.Seek(0, SeekOrigin.Begin);
        fi.SetLength(0);
        DontDestroyOnLoad(this.gameObject);

        Debug.Log($"内存:{SystemInfo.systemMemorySize}M");
        Debug.Log($"显存:{SystemInfo.graphicsMemorySize}M");
        Debug.Log($"显卡:{SystemInfo.graphicsDeviceName}");   
        Debug.Log($"CPU:{SystemInfo.processorType}");
    }

    private async void Log(string condition, string stackTrace, LogType type)
    {
        string str = $"{type}:{condition}\n{stackTrace}\n";
        var data = Encoding.UTF8.GetBytes(str);
        await fi.WriteAsync(data, 0, data.Length);
        fi.Position = fi.Length;
        fi.Flush();
    }
    private void OnDestroy()
    {
        fi.Dispose();
         Application.lowMemory -= LowMemory;
        Application.logMessageReceived -= Log;
    }
#endif
}
