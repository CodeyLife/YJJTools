using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class EventCenterType : YJJScritableSingletion<EventCenterType>
{
    public List<string> types = new List<string>();


#if UNITY_EDITOR
    [Button]
    private void Inspector()
    {
        Sirenix.OdinInspector.Editor.OdinEditorWindow.InspectObject(this);
    }
    [Button]
    private void GenerateEnums()
    {
        var path = AssetDatabase.FindAssets("EventEnums")[0];
        path = AssetDatabase.GUIDToAssetPath(path);
        var sb = new StringBuilder();
        sb.Append("public enum EventEnums\n{\n");
        foreach (var str in types)
        {
            sb.AppendLine($"     {str},");
        }
        sb.AppendLine("}");
        var reuslt = sb.ToString();
        Debug.Log(reuslt);
        File.WriteAllText(path, reuslt);
        AssetDatabase.Refresh();
    }
#endif


}
