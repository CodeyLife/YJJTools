using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Yjj_ExportAssetWithoutCS : MonoBehaviour
{
    [MenuItem("GameObject/导出资源(不含脚本)",priority = 100000)]
    public static void Exprot()
    {
        var targetPath = EditorUtility.SaveFilePanel("选择保存路径","","asset", "unitypackage");
        if (string.IsNullOrEmpty(targetPath)) return;
        var guids = Selection.assetGUIDs;
        var paths = guids.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToArray();
        var depends = AssetDatabase.GetDependencies(paths, true).Where(x => !x.EndsWith(".cs")).ToArray();
        AssetDatabase.ExportPackage(depends, targetPath, ExportPackageOptions.Interactive);
    }
}
