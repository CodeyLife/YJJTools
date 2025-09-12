using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace YJJTool
{

    public static class Yjj_Tool
    {

        #region 快捷键
        [MenuItem("YJJ/快捷键/inspector锁定开关 %g")]
        static void LockInspector()
        {
            ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
            if (!ActiveEditorTracker.sharedTracker.isLocked)
            {
                ActiveEditorTracker.sharedTracker.ForceRebuild();
            }
        }
        [MenuItem("YJJ/使用说明文档",priority = 0)]

        static void OpenDocument()
        {
            Application.OpenURL("https://gist.github.com/574b4bdf7926a87d79389d9da55d9fb7");
        }

        [MenuItem("YJJ/快捷键/快捷粘贴文本或颜色到该物体或第一个子物体 %&v")]
        static void Parse()
        {
            var go = Selection.activeGameObject;
            var text = go.GetComponent<TextMeshProUGUI>();
            bool isColor = ColorUtility.TryParseHtmlString(GUIUtility.systemCopyBuffer, out var color);
            if (!isColor)
            {
                if (Regex.IsMatch(GUIUtility.systemCopyBuffer, @"^[A-Z0-9]{6}$"))
                {
                    isColor = true;
                    ColorUtility.TryParseHtmlString("#" + GUIUtility.systemCopyBuffer, out color);
                }
            }
            if (text != null)
            {
                Undo.RecordObject(text, "ChangeText");
                if (isColor)
                {
                    text.color = color;
                }
                else
                {
                    text.text = GUIUtility.systemCopyBuffer;
                }
                EditorUtility.SetDirty(text);
                return;
            }
            text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                Undo.RecordObject(text, "ChangeText");
                if (isColor)
                {
                    text.color = color;
                }
                else
                {
                    text.text = GUIUtility.systemCopyBuffer;
                }
                EditorUtility.SetDirty(text);
                return;
            }
            var t = go.GetComponent<UnityEngine.UI.Text>();
            if (t != null)
            {
                Undo.RecordObject(t, "ChangeText");
                if (isColor)
                {
                    t.color = color;
                }
                else
                {
                    t.text = GUIUtility.systemCopyBuffer;
                }
                EditorUtility.SetDirty(t);
                return;
            }
            t = go.GetComponentInChildren<Text>();
            if (t != null)
            {
                Undo.RecordObject(t, "ChangeText");
                if (isColor)
                {
                    t.color = color;
                }
                else
                {
                    t.text = GUIUtility.systemCopyBuffer;
                }
                EditorUtility.SetDirty(t);
                return;
            }
        }

        [MenuItem("Assets/复制完整文件夹路径 #&c", false, 20)]
        private static void GetSelectionFolderPath()
        {
            if (Selection.activeObject != null)
            {
                if (Selection.activeObject.GetType() == typeof(GameObject))
                {

                    var str = ConbainParent(((GameObject)Selection.activeObject).transform, new StringBuilder(Selection.activeObject.name));
                    Debug.Log(str);
                    GUIUtility.systemCopyBuffer = str;
                    return;
                }
                var target = Selection.assetGUIDs;
                if (target.Length == 0) return;
                var path = AssetDatabase.GUIDToAssetPath(target[0]);
                path = PathUtility.GetFullPath(path);
                if (File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }
                Debug.Log(path);
                GUIUtility.systemCopyBuffer = path;
            }

            string ConbainParent(Transform t, StringBuilder sb)
            {
                if (t.parent != null)
                {
                    sb = sb.Insert(0, t.parent.name + "/");
                    ConbainParent(t.parent, sb);
                }
                return sb.ToString();
            }
        }

        [MenuItem("Assets/代码转为utf-8保存", true, 20)]
        private static bool ValidateSaveAsUTF8()
        {

            foreach (var obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                // 无效路径检查
                if (string.IsNullOrEmpty(path)) return false;
                if (AssetDatabase.IsValidFolder(path)) return true;
                // 检查扩展名
                if (Path.GetExtension(path).ToLower() == ".cs") return true;
            }

            return false;
        }

        #region 代码转为utf-8
        [MenuItem("Assets/代码转为utf-8保存", false, 20)]
        private static void SaveAsUTF8()
        {
            foreach (var obj in Selection.objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(obj);

                // 跳过无效路径
                if (string.IsNullOrEmpty(assetPath)) continue;

                // 如果是文件夹，递归处理文件夹下所有.cs文件
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    ProcessFolder(assetPath);
                }
                // 如果是.cs文件，直接处理
                else if (Path.GetExtension(assetPath).ToLower() == ".cs")
                {
                    ProcessFile(assetPath);
                }
            }

            // 刷新资源数据库
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 判断文件是否是 UTF-8 编码
        /// </summary>
        private static bool IsUTF8Encoded(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);

            // 检查是否带 BOM
            if (fileBytes.Length >= 3 && fileBytes[0] == 0xEF && fileBytes[1] == 0xBB && fileBytes[2] == 0xBF)
            {
                return true; // 带 BOM 的 UTF-8
            }

            // 检查是否符合 UTF-8 编码规则
            try
            {
                string content = Encoding.UTF8.GetString(fileBytes);
                byte[] reencodedBytes = Encoding.UTF8.GetBytes(content);
                return fileBytes.Length == reencodedBytes.Length; // 如果重新编码后字节数不变，则可能是 UTF-8
            }
            catch
            {
                return false; // 解码失败，不是 UTF-8
            }
        }

        /// <summary>
        /// 处理单个.cs文件
        /// </summary>
        private static void ProcessFile(string assetPath)
        {
            string fullPath = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                assetPath
            );
            // 检查文件是否已经是 UTF-8 编码
            if (IsUTF8Encoded(fullPath))
            {
              //  Debug.Log($"文件已经是 UTF-8 编码: {assetPath}");
                return;
            }
            try
            {
                // 读取文件内容（假设原始编码是GBK）
                string content = File.ReadAllText(fullPath, Encoding.GetEncoding("GBK"));

                // 使用UTF-8编码保存（不带BOM）
                File.WriteAllText(
                    fullPath,
                    content,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
                );

                Debug.Log($"成功转换编码: {assetPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"转换失败 {assetPath}: {e.Message}");
            }
        }

        /// <summary>
        /// 递归处理文件夹下所有.cs文件
        /// </summary>
        private static void ProcessFolder(string folderPath)
        {
            // 获取文件夹下所有.cs文件
            string[] csFiles = Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories);

            foreach (string csFile in csFiles)
            {
                // 将完整路径转换为Unity资源路径
                string assetPath = csFile.Replace("\\", "/").Replace(Application.dataPath, "Assets");

                // 处理.cs文件
                ProcessFile(assetPath);
            }
        }

        #endregion

        [MenuItem("YJJ/重置所有ButtonGroup")]
        static void InitButtonGroup()
        {
            var buttons = GameObject.FindObjectsByType<ButtonGroupContent>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            buttons.Foreach(x => x.EditorInit());
        }
        #endregion
        #region 更新项目
        [MenuItem("Assets/Tool同步/更新文件到Tool", false, 20)]
        private static void Update2Tool()
        {
            if (EditorUtility.DisplayDialog($"提交更新", $"更新到\n{Yjj_ConfigWindows.Config.toolPath}", "更新", "取消"))
            {
                var target = Selection.assetGUIDs;
                if (target.Length == 0) return;
                foreach (var p in target)
                {
                    var path = AssetDatabase.GUIDToAssetPath(p);
                    //选中的是文件夹
                    if (Directory.Exists(path))
                    {
                        List<string> paths = new List<string>();
                        PathUtility.GetAllDirectoryFiles(path, paths);
                        foreach (var info in paths)
                        {
                            Write2Tool(info);
                        }
                    }
                    else if (File.Exists(path))
                    {
                        Write2Tool(path);
                    }
                }
            }
        }
        [MenuItem("Assets/Tool同步/从Tool更新到项目", false, 20)]
        public static void ReadFromToll()
        {
            if (EditorUtility.DisplayDialog($"从Tool Copy", $"从\n{Yjj_ConfigWindows.Config.toolPath}\n下载到项目", "更新", "取消"))
            {
                var target = Selection.assetGUIDs;
                if (target.Length == 0) return;
                foreach (var p in target)
                {
                    var path = AssetDatabase.GUIDToAssetPath(p);
                    //选中的是文件夹
                    if (Directory.Exists(path))
                    {
                        List<string> paths = new List<string>();
                        var targetDir = Path.Combine(Yjj_ConfigWindows.Config.toolPath, path);
                        PathUtility.GetAllDirectoryFiles(targetDir, paths);
                        for (int i = 0; i < paths.Count; i++)
                        {
                            EditorUtility.DisplayProgressBar("更新中", $"{i}/{paths.Count},{paths[i]}", i / paths.Count);
                            ReadFromTool(paths[i]);
                        }
                        EditorUtility.ClearProgressBar();
                    }
                    else if (File.Exists(path))
                    {
                        var targetDir = Path.Combine(Yjj_ConfigWindows.Config.toolPath, path);
                        ReadFromTool(targetDir);
                    }
                }
                AssetDatabase.Refresh();
            }
        }
        [MenuItem("Assets/Tool同步/和Tool对比该文件差异", false, 20)]
        public static void CheckDiffrece()
        {
            var target = Selection.assetGUIDs;
            if (target.Length == 0) return;
            foreach (var p in target)
            {
                var path = AssetDatabase.GUIDToAssetPath(p);
                //选中的是文件夹
                if (Directory.Exists(path))
                {
                    var files = PathUtility.GetAllDirectoryFiles(path);
                    foreach (var f in files)
                    {
                        var targetPath = PathUtility.GetRelativeAsset(f, true);
                        targetPath = Path.Combine(Yjj_ConfigWindows.Config.toolPath, targetPath);
                        CheckFunction(f, targetPath);
                    }
                }
                else if (File.Exists(path))
                {
                    var targetDir = Path.Combine(Yjj_ConfigWindows.Config.toolPath, path);
                    CheckFunction(path, targetDir);
                }
            }
        }
        private static void CheckFunction(string path, string targetDir)
        {
            if (path.EndsWith(".meta")) return;
            if (!File.Exists(targetDir))
            {
                Debug.Log($"<color=yellow>新增{Path.GetFileName(path)}</color>");
                return;
            }
            Debug.Log($"{Path.GetFileName(path)}");
            var current = File.ReadAllLines(path).ToList();
            var source = File.ReadAllLines(targetDir).ToList();

            var news = current.FindAll(x => !source.Contains(x));
            foreach (var n in news)
            {
                Debug.Log($"<color=red>新增{n}</color>");
            }
            var olds = source.FindAll(x => !current.Contains(x));
            foreach (var o in olds)
            {
                Debug.Log($"<color=yellow>删除:{o}</color>");
            }

            Debug.Log("比较完毕");
        }
        private static void Write2Tool(string source)
        {
            if (source.EndsWith(".meta") && !source.EndsWith("cs.meta")) return;
            var result = PathUtility.GetRelativeAsset(source);
            result = Path.Combine(Yjj_ConfigWindows.Config.toolPath, result);
            Task.Run(() =>
            {
                var dir = Path.GetDirectoryName(result);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.Copy(source, result, true);
                var fileName = Path.GetFileName(result);
                Debug.Log($"提交:{fileName}");
            });
        }
        private static void ReadFromTool(string source)
        {
            if (source.EndsWith(".meta")) return;
            int index = source.IndexOf("Assets") + 7;
            var result = source.Substring(index, source.Length - index);
            Debug.Log($"更新:{Path.GetFileName(result)}");
            result = Path.Combine(Application.dataPath, result);
            var dir = Path.GetDirectoryName(result);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.Copy(source, result, true);
        }

        #endregion
        #region Hierarchy右键扩展
        #region Creat
        [MenuItem("GameObject/我的/TextmeshproUGUI", priority = 1)]
        public static void CreatTmpText()
        {
            var go = new GameObject("Text", typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(go, "CreatTextPro");
            GameObjectUtility.SetParentAndAlign(go, Selection.activeGameObject);
            var pro = go.GetComponent<TextMeshProUGUI>();
            pro.font = Yjj_ConfigWindows.Config.tmpFont;
            pro.enableWordWrapping = Yjj_ConfigWindows.Config.warping;
            pro.alignment = Yjj_ConfigWindows.Config.alignment;
            pro.fontSize = Yjj_ConfigWindows.Config.textSize;
            pro.color = Yjj_ConfigWindows.Config.textColor;
            pro.text = "Text";
            pro.raycastTarget = false;
            pro.rectTransform.sizeDelta = new Vector2(100, pro.preferredHeight);
            Selection.activeGameObject = go;
        }
        [MenuItem("GameObject/我的/Text", priority = 2)]
        public static void CreatText()
        {
            var go = new GameObject("TextPro", typeof(Text));
            Undo.RegisterCreatedObjectUndo(go, "CreatText");
            GameObjectUtility.SetParentAndAlign(go, Selection.activeGameObject);
            var pro = go.GetComponent<Text>();
            pro.font = Yjj_ConfigWindows.Config.font;
            pro.alignment = Yjj_ConfigWindows.Config.textAligin;
            pro.fontSize = (int)Yjj_ConfigWindows.Config.textSize;
            pro.color = Yjj_ConfigWindows.Config.textColor;
            pro.text = "Text";
            pro.raycastTarget = false;
            pro.rectTransform.sizeDelta = new Vector2(100, pro.preferredHeight);
            Selection.activeGameObject = go;
        }
        [MenuItem("GameObject/我的/UIItems", priority = 4)]
        public static void CreatUIItems()
        {
            var configPath = AssetDatabase.GetAssetPath(Yjj_ConfigWindows.Config);
            DirectoryInfo d = new DirectoryInfo(configPath);
            var arr = d.Parent.Parent.GetDirectories("UIItems");
            if (arr.Length > 0)
            {
                var files = arr[0].GetFiles("*.prefab");
                foreach (var file in files)
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(PathUtility.GetRelativeAsset(file.FullName));
                    var component = go.GetComponent(typeof(UIItemManager));
                    if (component != null && component.GetType() == typeof(UIItemManager))
                    {
                        CreatChart(go);
                        return;
                    }
                }
            }
        }
        [MenuItem("GameObject/我的/3dPoint", priority = 5)]
        public static void Creat3dPoint()
        {
            var configPath = AssetDatabase.GetAssetPath(Yjj_ConfigWindows.Config);
            DirectoryInfo d = new DirectoryInfo(configPath);
            var arr = d.Parent.Parent.GetDirectories("3DTo2DPointUI");
            if (arr.Length > 0)
            {
                var files = arr[0].GetFiles("*.prefab");
                foreach (var file in files)
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(PathUtility.GetRelativeAsset(file.FullName));
                    var component = go.GetComponent(typeof(PointUI));
                    if (component != null && component.GetType() == typeof(PointUI))
                    {
                        CreatChart(go);
                        return;
                    }
                }
            }
        }
        [MenuItem("GameObject/我的/UIItemOptimization", priority = 3)]
        private static void CreatUIOptimization()
        {
            var configPath = AssetDatabase.GetAssetPath(Yjj_ConfigWindows.Config);
            DirectoryInfo d = new DirectoryInfo(configPath);
            var arr = d.Parent.Parent.GetDirectories("UIItems");
            if (arr.Length > 0)
            {
                var files = arr[0].GetFiles("*.prefab");
                foreach (var file in files)
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(PathUtility.GetRelativeAsset(file.FullName));
                    var component = go.GetComponent(typeof(UIItemOptimization));
                    if (component != null && component.GetType() == typeof(UIItemOptimization) && go.GetComponentInChildren<ScrollRect>() == null)
                    {
                        CreatChart(go, false);
                        return;
                    }
                }
            }
        }
        [MenuItem("GameObject/我的/UiItemScroll", priority = 4)]
        private static void CreatUIOptimizationScroll()
        {
            var configPath = AssetDatabase.GetAssetPath(Yjj_ConfigWindows.Config);
            DirectoryInfo d = new DirectoryInfo(configPath);
            var arr = d.Parent.Parent.GetDirectories("UIItems");
            if (arr.Length > 0)
            {
                var files = arr[0].GetFiles("*.prefab");
                foreach (var file in files)
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(PathUtility.GetRelativeAsset(file.FullName));
                    Debug.Log(go.name);
                    var component = go.GetComponent(typeof(UIItemOptimization));
                    if (component != null && component.GetType() == typeof(UIItemOptimization) && go.GetComponentInChildren<ScrollRect>() != null)
                    {
                        CreatChart(go, false);
                        return;
                    }
                }
            }
        }
        #endregion

        [MenuItem("GameObject/快捷操作/关闭选中物体及所有子物体的raycast", priority = 0)]
        public static void CloseAllRayCast()
        {
            var arr = Selection.gameObjects;
            foreach (var child in arr)
            {
                LoopCloseRaycast(child.transform);
            }

        }
        private static void LoopCloseRaycast(Transform t)
        {
            var image = t.GetComponent<Image>();
            if (image != null)
            {
                Undo.RecordObject(image, "raycast");
                image.raycastTarget = false;
            }
            var text = t.GetComponent<Text>();
            if (text != null)
            {
                Undo.RecordObject(text, "raycast");
                text.raycastTarget = false;
            }
            var pro = t.GetComponent<TextMeshProUGUI>();
            if (pro != null)
            {
                Undo.RecordObject(pro, "raycast");
                pro.raycastTarget = false;
            }
            EditorUtility.SetDirty(t.gameObject);
            foreach (Transform child in t)
            {
                LoopCloseRaycast(child);
            }
        }
        #region 改变Hierachy的物体名称
        [MenuItem("GameObject/快捷操作/SetNameWith_Sprite", priority = 11)]
        public static void ChangeAllSpriteName()
        {
            if (Selection.activeGameObject != null)
            {
                LoopChild(Selection.activeGameObject.transform);
            }
        }
        private static void LoopChild(Transform t)
        {
            var image = t.GetComponent<UnityEngine.UI.Image>();
            if (image != null && image.sprite != null)
            {
                Undo.RecordObject(t.gameObject, "setName");
                t.gameObject.name = image.sprite.name;
                EditorUtility.SetDirty(t.gameObject);
            }
            foreach (Transform child in t)
            {
                LoopChild(child);
            }
        }

        [MenuItem("GameObject/快捷操作/SetNameWith_Text", priority = 12)]
        public static void ChangeAllTextName()
        {
            if (Selection.activeTransform != null)
            {
                foreach (Transform child in Selection.transforms)
                {
                    SetNameWithText(child);
                }
            }
        }
        private static void SetNameWithText(Transform trans)
        {
            var text = trans.GetComponent<Text>();
            if (text != null)
            {
                Undo.RecordObject(trans.gameObject, "ChangeNmae");
                trans.name = text.text;
                return;
            }
            else
            {
                var pro = trans.GetComponent<TextMeshProUGUI>();
                if (pro != null)
                {
                    Undo.RecordObject(trans.gameObject, "ChangeNmae");
                    trans.name = pro.text;
                    return;
                }
            }
            foreach (Transform child in trans)
            {
                SetNameWithText(child);
            }
        }
        #endregion

        [MenuItem("GameObject/快捷操作/关闭所选物体下所有子物体PointUI的定位", false, priority = 13)]
        static void CleanPointUIPoint()
        {
            var go = Selection.activeTransform;
            for (int i = 0; i < go.childCount; i++)
            {
                PointUICleanLoop(go.GetChild(i));
            }
        }
        static void PointUICleanLoop(Transform t)
        {
            var pointUI = t.GetComponent<PointUI>();
            if (pointUI != null)
            {
                pointUI.ClosePointObject();
            }
            for (int i = 0; i < t.childCount; i++)
            {
                PointUICleanLoop(t.GetChild(i));
            }
        }

        [MenuItem("GameObject/快捷操作/所选物体在Hierarchy禁止选中 %h", priority = 14)]
        private static void Visibility()
        {
            var gos = Selection.gameObjects;
            SceneVisibilityManager.instance.DisablePicking(gos, true);
            EditorApplication.Step();
        }

        #region 图表
        [MenuItem("GameObject/我的/图表/V2图表", priority = -1)]
        public static void CreatChart_V2()
        {
            var go = GetGraphPrefab(typeof(ChartV2Base));
            CreatChart(go);
        }


        [MenuItem("GameObject/我的/图表/雷达图", priority = 0)]
        public static void CreatChart_Ladar()
        {
            var go = GetGraphPrefab(typeof(Yjj_RadarMap));
            CreatChart(go);
        }


        [MenuItem("GameObject/我的/图表/水滴图", priority = 0)]
        public static void CreatChart_WaterChart()
        {
            var go = GetGraphPrefab(typeof(WaterChart));
            CreatChart(go);
        }


        [MenuItem("GameObject/我的/图表/饼状图withMesh", priority = 0)]
        public static void CreatChart_MeshPie()
        {
            var go = GetGraphPrefab(typeof(Yjj_PieChartNew));
            CreatChart(go);
        }

        [MenuItem("GameObject/我的/图表/3D饼状图", priority = 0)]
        public static void Creat3dChartPie()
        {
            var go = GetGraphPrefab(typeof(Yjj_3DPieChart));
            CreatChart(go);
        }


        //[MenuItem("GameObject/我的/图表/3D饼状图", priority = 0)]
        //public static void CreatChart_3DPie()
        //{
        //    var go = GetGraphPrefab(typeof(Yjj_3DPieChart));
        //    CreatChart(go);
        //}
        [MenuItem("GameObject/我的/Toggle", priority = 1)]
        public static void CreatToggle()
        {
            var go = AssetDatabase.FindAssets("YjjToggle");
            var path = AssetDatabase.GUIDToAssetPath(go[0]);
            var d = new DirectoryInfo(path);
            var prefabs = d.Parent.GetFiles("*.prefab");
            var prefabPath = PathUtility.GetRelativeAsset(prefabs[0].FullName);
            var result = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            CreatChart(result, false);
            //CreatChart(go);
        }
        [MenuItem("GameObject/我的/图表/Grid方块图", priority = 0)]
        public static void CreatGrid()
        {
            var go = GetGraphPrefab(typeof(Yjj_GridGraph));
            CreatChart(go);
        }
        #endregion

        private static void CreatChart(GameObject go, bool unPackPrefab = true)
        {
            var g = GameObject.Instantiate(go);
#if !UNITY_2021_1_OR_NEWER
        Undo.RegisterCreatedObjectUndo(g, "Creat");
#endif
            GameObjectUtility.SetParentAndAlign(g, Selection.activeGameObject);
            //g.transform.SetParent(Selection.activeGameObject.transform);
            if (PrefabUtility.IsPartOfPrefabAsset(g) && unPackPrefab)
            {
                PrefabUtility.UnpackPrefabInstance(g, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            Selection.activeGameObject = g;
            var camera = SceneView.lastActiveSceneView.camera;
            var rect = g.transform.rectTransform();

            var screenPos = new Vector2(camera.transform.position.x, camera.transform.position.y);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect.parent.rectTransform(), screenPos, camera, out var local);
            rect.position = screenPos;
            g.name = go.name;
        }
        private static GameObject GetGraphPrefab(Type t)
        {
            var configPath = AssetDatabase.GetAssetPath(Yjj_ConfigWindows.Config);
            DirectoryInfo d = new DirectoryInfo(configPath);
            var arr = d.Parent.Parent.GetDirectories("Prefabs");
            if (arr.Length > 0)
            {
                var files = arr[0].GetFiles("*.prefab");
                foreach (var file in files)
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(PathUtility.GetRelativeAsset(file.FullName));
                    var component = go.GetComponent(t);
                    if (component != null && component.GetType() == t)
                    {
                        return go;
                    }
                }
            }
            return null;
        }

        #endregion
        #region 原生Script右键扩展
        [MenuItem("CONTEXT/RectTransform/基于父节点大小变化", priority = 1000)]
        private static void RectTransfromSetZero(MenuCommand command)
        {
            var rect = (RectTransform)command.context;
            Undo.RecordObject(rect, "SetRect");
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
        }
        [MenuItem("CONTEXT/TextMeshProUGUI/Rect大小匹配文本", priority = 1000)]
        private static void BestSizeWithTextPro(MenuCommand command)
        {
            var pro = (TextMeshProUGUI)command.context;
            Undo.RecordObject(pro, "SetText");
            var rect = pro.rectTransform;
            rect.sizeDelta = pro.GetPreferredValues();
            EditorApplication.Step();
            rect.sizeDelta = pro.GetPreferredValues();

        }
        [MenuItem("CONTEXT/Text/Rect大小匹配文本", priority = 1000)]
        private static void BestSizeWithText(MenuCommand command)
        {
            var pro = (Text)command.context;
            Undo.RecordObject(pro, "SetText");
            var rect = pro.rectTransform;
            rect.sizeDelta = new Vector2(pro.preferredWidth, pro.preferredHeight);
            EditorApplication.Step();
            rect.sizeDelta = new Vector2(pro.preferredWidth, pro.preferredHeight);

        }
        [MenuItem("CONTEXT/Component/获取Transform路径", priority = 1000)]
        private static void GetPath(MenuCommand command)
        {
            var trans = ((Component)command.context).transform;
            List<string> nameList = new List<string>();
            nameList.Add(trans.name);
            while (trans.parent != null)
            {
                nameList.Add(trans.parent.name);
                trans = trans.parent;
            }
            nameList.Reverse();
            var sb = new StringBuilder();
            for (int i = 0; i < nameList.Count; i++)
            {
                sb.Append(nameList[i]);
                if (i != nameList.Count - 1)
                {
                    sb.Append("/");
                }
            }
            var result = sb.ToString();
            Debug.Log(result);
            GUIUtility.systemCopyBuffer = result;
        }
        [MenuItem("CONTEXT/Image/HalfSize", priority = 1000)]
        private static void HlafImage(MenuCommand command)
        {
            var image = (Image)command.context;
            image.SetNativeSize();
            var scale = image.rectTransform.sizeDelta;
            image.rectTransform.sizeDelta = scale * 0.5f;
        }
        #endregion
    }
}