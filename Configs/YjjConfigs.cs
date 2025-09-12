using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using YJJTool;

public class YjjConfigs : YJJScritableSingletion<YjjConfigs>
{
    [LabelText("自动转为sprite的文件夹路径"),BoxGroup("TextureImport",ShowLabel = false)]
    public List<string> autoSpriteList = new List<string>();
    [LabelText("把debug写入日志"),OnValueChanged("OnDebugChange")]
    public bool openDebug = false;

    [LabelText("同步脚本到Tool的地址"),OnValueChanged("PathChange")]
    public string toolPath;
    #region 字体预设

    [LabelText("默认颜色")]
    [Title("创建文本设置",bold:true)]
    [BoxGroup("Text",GroupID = "Text",ShowLabel =false),GUIColor("@textColor")]
    public Color textColor = Color.white;

    [BoxGroup("Text")]
    public bool warping = false;
    [LabelText("默认字体大小"),BoxGroup("Text")]
    public float textSize = 24;

    [TabGroup("TextMeshPro", GroupID = "Text/Tab")]
    public TMP_FontAsset tmpFont;
    [TabGroup("TextMeshPro", GroupID = "Text/Tab")]
    public TextAlignmentOptions alignment = TextAlignmentOptions.Center;

    [TabGroup("Text", GroupID = "Text/Tab")]
    public Font font;
    [TabGroup("Text",GroupID = "Text/Tab")]
    public TextAnchor textAligin = TextAnchor.MiddleLeft;
    #endregion
    //[TabGroup("图表默认设置","base"),HideInInspector]
    //public BaseSet baseSet;
    //[TabGroup("图表默认设置", "data"), HideInInspector]
    //public DataSet dataSet;
    //[TabGroup("图表默认设置", "line"), HideInInspector]
    //public LineSet lineSet;
#if UNITY_EDITOR
    private void PathChange()
    {
        toolPath = Regex.Match(toolPath, @"^((?!Assets).)*").Value;
    }
    [Button("选择并添加自动转为sprite的文件夹路径"), BoxGroup("TextureImport")]
    private void AddPath()
    {
        var path = UnityEditor.EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
        if (!string.IsNullOrEmpty(path))
        {
            int index = path.IndexOf("Assets");
            path = path.Substring(index, path.Length - index);
            autoSpriteList.Add(path);
        }
    }
    //开关debug
    private void OnDebugChange()
    {
        var log = FindObjectOfType<Yjj_Logger>();
        //Debug.Log(log);
        if (openDebug)
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            if(log == null)
            {
                var go = new GameObject("Logger");
                go.AddComponent<Yjj_Logger>();
            }
        }
        else
        {
            if (log != null)
            {
                DestroyImmediate(log.gameObject);
            }
        }
    }

#if Use_CameraController
#else
    [Button("启用相机控制器组件(Cinemachine版本需要3.0以上)",ButtonHeight = 50),GUIColor(0,1,0)]
    private void ChangeUseCamera()
    {
        if (!IsCinemachineInstalled())
        {
            InstallCinemachine();
        }
        var newSymbol = "Use_CameraController";
        UnityEditor.BuildTargetGroup buildTargetGroup = UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup;
        Debug.Log("当前平台：" + buildTargetGroup);
        if (buildTargetGroup == UnityEditor.BuildTargetGroup.Unknown)
        {
            return;
        }
        var symbols = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Trim();
        Debug.Log("当前平台的ScriptingDefineSymbols：" + symbols);
        var defines = symbols.Split(';');
        if (defines.Contains(newSymbol) == false)
        {
            if (symbols.EndsWith(";", System.StringComparison.InvariantCulture) == false)
            {
                symbols += ";";
            }
            symbols += newSymbol;

            UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
            //AssetDatabase.SaveAssets();
            UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            Debug.Log($"向当前平台的ScriptingDefineSymbols中添加了：{newSymbol}");
            UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceUpdate);
        }
        else
        {
            Debug.Log($"当前平台的ScriptingDefineSymbols中已包含{newSymbol}, 不可再添加！");
        }
    }

    private static bool IsCinemachineInstalled()
    {
        // 检查项目中是否已经存在 Cinemachine 的相关内容
        try
        {
            var type =System.Type.GetType("Unity.Cinemachine.CinemachineBrain, Unity.Cinemachine");
            return type != null;
        }
        catch
        {
            return false;
        }
    }
    private static void InstallCinemachine()
    {
        // 导入 Cinemachine 插件
        Debug.Log("Cinemachine 插件未安装，正在导入...");

        // 使用 Unity 的包管理 API 来安装 Cinemachine 插件
        UnityEditor.PackageManager.Client.Add("com.unity.cinemachine");

        // 刷新编辑器以确保包管理器完成安装
        UnityEditor.AssetDatabase.Refresh();
        EditorApplication.Step();
    }
#endif
#endif
}
