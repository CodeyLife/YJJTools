using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class YJJScritableSingletion<T> : ScriptableObject where T : ScriptableObject
{
#if UNITY_EDITOR
    private static T instance;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                var arr = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}");
                if (arr.Length == 0  && !EditorApplication.isCompiling)
                {
                    var possiblePaths = "Assets/YJJTools/Configs/config.asset";
                    if (System.IO.File.Exists(possiblePaths))
                    {
                        var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(possiblePaths, typeof(T)) as T;
                        if (asset != null)
                        {
                            instance = asset;
                            
                        }
                    }
                    if(instance == null)
                    {
                        instance = CreatNew();
                    }
                }
                else
                {
                    instance = UnityEditor.AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(arr[0]), typeof(T)) as T;
                }
            }
            return instance;
        }
        set => instance = value;
    }


    public static T CreatNew(T config)
    {
        var path = UnityEditor.EditorUtility.SaveFilePanel("选择保存路径", Application.dataPath, $"{typeof(T).Name}", "asset");
        if (string.IsNullOrEmpty(path)) return default(T);
        Sirenix.Utilities.PathUtilities.TryMakeRelative(Path.GetDirectoryName(Application.dataPath), path, out path);
        UnityEditor.AssetDatabase.CreateAsset(config, path);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        return config;
    }
    public static T CreatNew()
    {
        var path = UnityEditor.EditorUtility.SaveFilePanel("选择保存路径", Application.dataPath, $"{typeof(T).Name}", "asset");
        if (string.IsNullOrEmpty(path)) return default(T);
        Sirenix.Utilities.PathUtilities.TryMakeRelative(Path.GetDirectoryName(Application.dataPath), path, out path);
        var config = CreateInstance<T>();
        UnityEditor.AssetDatabase.CreateAsset(config, path);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        return config;
    }
#endif
}
