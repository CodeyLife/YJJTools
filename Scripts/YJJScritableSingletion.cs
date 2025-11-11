using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class YJJScritableSingletion<T> : ScriptableObject where T : ScriptableObject
{
#if UNITY_EDITOR
    private static T instance;
    private static Dictionary<System.Type, bool> hasTriedCreate = new Dictionary<System.Type, bool>();

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                // 如果正在编译，延迟查找
                if (EditorApplication.isCompiling)
                {
                    return null;
                }

                instance = FindExistingAsset();
                
                // 如果找不到且未尝试过创建，则尝试创建（首次访问时）
                if (instance == null && !HasTriedCreate())
                {
                    MarkTriedCreate();
                    var newInstance = CreatNew();
                    // 如果用户取消了对话框，newInstance 为 null，instance 保持为 null
                    // 下次访问时不会再次弹窗（因为已经标记为已尝试创建）
                    if (newInstance != null)
                    {
                        instance = newInstance;
                    }
                }
            }
            return instance;
        }
        set => instance = value;
    }

    /// <summary>
    /// 使用多种策略查找已存在的资源
    /// </summary>
    private static T FindExistingAsset()
    {
        var type = typeof(T);
        
        // 策略1: 使用类型名称查找
        var guids = AssetDatabase.FindAssets($"t:{type.Name}");
        if (guids.Length > 0)
        {
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, type) as T;
                if (asset != null)
                {
                    return asset;
                }
            }
        }

        // 策略2: 使用完整类型名称查找（包含命名空间）
        guids = AssetDatabase.FindAssets($"t:{type.FullName}");
        if (guids.Length > 0)
        {
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, type) as T;
                if (asset != null)
                {
                    return asset;
                }
            }
        }

        // 策略3: 查找所有 ScriptableObject，然后过滤类型
        guids = AssetDatabase.FindAssets("t:ScriptableObject");
        if (guids.Length > 0)
        {
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, type) as T;
                if (asset != null)
                {
                    return asset;
                }
            }
        }

        // 策略4: 尝试默认路径（针对 YjjConfigs 的特殊处理）
        if (type == typeof(YjjConfigs))
        {
            var defaultPath = "Assets/YJJTools/Configs/config.asset";
            if (File.Exists(defaultPath))
            {
                var asset = AssetDatabase.LoadAssetAtPath(defaultPath, type) as T;
                if (asset != null)
                {
                    return asset;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 检查是否已尝试创建过该类型的资源
    /// </summary>
    private static bool HasTriedCreate()
    {
        var type = typeof(T);
        return hasTriedCreate.ContainsKey(type) && hasTriedCreate[type];
    }

    /// <summary>
    /// 标记已尝试创建该类型的资源
    /// </summary>
    private static void MarkTriedCreate()
    {
        var type = typeof(T);
        hasTriedCreate[type] = true;
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

