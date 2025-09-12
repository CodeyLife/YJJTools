#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;


public class Yjj_ConfigWindows : OdinEditorWindow
{

    [MenuItem("YJJ/配置",priority = -100)]
    public static void OpenWindow()
    {
        GetWindow<Yjj_ConfigWindows>();
    }

    private static YjjConfigs _config;

    public static YjjConfigs Config
    {
        get
        {
            //if (_config == null)
            //{
            //    _config = YjjConfigs.Instance;
            //}
            return _config;
        }
        set => _config = value;
    }
    [InitializeOnLoadMethod]
    private static void OnInitializeOnLoad()
    {
        _config = YjjConfigs.Instance;
        Events.registeringPackages += Events_registeringPackages;

    }

    private static void Events_registeringPackages(PackageRegistrationEventArgs obj)
    {
        foreach (var change in obj.removed)
        {
            if(change.displayName == "Cinemachine")
            {
                var newSymbol = "Use_CameraController";
                UnityEditor.BuildTargetGroup buildTargetGroup = UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup;
               
                if (buildTargetGroup == UnityEditor.BuildTargetGroup.Unknown)
                {
                    return;
                }
                var symbols = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Trim();
                var defines = symbols.Split(';');
                if (defines.Contains(newSymbol) == true)
                {
                    var sb = new StringBuilder();
                    for (int i = 0;i < defines.Length; i++)
                    {
                        var symbol = defines[i];
                        if (symbol == newSymbol) continue;
                        sb.Append(symbol);
                        if(i <defines.Length - 1)
                        {
                            sb.Append(';');
                        }
                    }
                    UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, sb.ToString());
                    //AssetDatabase.SaveAssets();
                    UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                    Debug.Log("Cinemachine被移除，已禁用相机组件");
                }
            }
        }
    }

    protected override IEnumerable<object> GetTargets()
    {
        yield return Config;
    }
    protected override void DrawEditor(int index)
    {
        var currentDrawingEditor = this.CurrentDrawingTargets[index];

        SirenixEditorGUI.Title(
            title:"配置表",
            subtitle: currentDrawingEditor.GetType().GetNiceFullName(),
            textAlignment: TextAlignment.Left,
            horizontalLine: true
        );

        base.DrawEditor(index);

        if (index != this.CurrentDrawingTargets.Count - 1)
        {
            SirenixEditorGUI.DrawThickHorizontalSeparator(15, 15);
        }
    }
}
#endif
