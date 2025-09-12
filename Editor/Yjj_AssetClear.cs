using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static System.Environment;

namespace YjjTool
{
    public class Yjj_AssetClear : OdinEditorWindow
    {
        public enum FolderType
        {
            不清理文件夹内容,
            只清理文件夹内容,
        }
        [Title("忽略文件类型")]
        [EnumToggleButtons, HideLabel,InfoBox("<color=red>请注意:清理脚本文件和shader可能导致项目报错和材质丢失</color>",VisibleIf = "@!ignorType.HasFlag(IgnorType.cs) || !ignorType.HasFlag(IgnorType.shader)")]
        public IgnorType ignorType = IgnorType.cs |IgnorType.shader;
        [EnumToggleButtons, HideLabel, Space(15)]
        public FolderType folderType = FolderType.不清理文件夹内容;
        [LabelText("文件夹剔除"), InlineButton("Add")]
        public List<string> ignorePath = new List<string>() { @"Assets\AllPlugins\YjjTools" };

        [BoxGroup("list")]
        [TableList(ShowIndexLabels = true), LabelText("可删除文件"),HideIf("@results.Count == 0")]
        public List<ClearInfo> results = new List<ClearInfo>();
        #region 快捷按钮
        [BoxGroup("list", ShowLabel = false),HideIf("@results.Count == 0")]
        [ButtonGroup("list/select")]
        private void All()
        {
            
            results.ForEach(x => x.clear = true);
        }
        [BoxGroup("list")]
        [ButtonGroup("list/select"), HideIf("@results.Count == 0")]
        private void None()
        {
            results.ForEach(x => x.clear = false);
        }
        #endregion

        [TableList(ShowIndexLabels = true), LabelText("谨慎删除的文件"),HideIf("@unSelectResults.Count == 0")]
        public List<ClearInfo> unSelectResults = new List<ClearInfo>();

        [TableList, ShowIf("@rules.Count>0"),LabelText("根据后缀选择文件")]
        public List<TypeSelect> rules = new List<TypeSelect>();

        private void Add()
        {
            var path = EditorUtility.OpenFolderPanel("选择要忽略的文件夹", Application.dataPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                path = PathUtility.GetRelativeAsset(path, true);
                ignorePath.Add(path.Replace("/", "\\"));
            }
        }
        [MenuItem("YJJ/AssetClear")]
        private static void Open()
        {
            var window = GetWindow<Yjj_AssetClear>();
        }

        [Button("打开Package文件夹")]
        private void OpenPackageAssets()
        {
            var path = GetDirctory();
            System.Diagnostics.Process.Start("explorer.exe", path);
        }


        [Button("搜集未使用资源", ButtonSizes.Large), ButtonGroup("but")]
        public void SearchAssets()
        {

            //搜集resources prefab
            EditorUtility.DisplayProgressBar("clear", "收集Resources下的prefab依赖...", 0.1f);
            var folders = new DirectoryInfo(Application.dataPath).GetDirectories("Resources", SearchOption.AllDirectories);
            var temps = folders.Select(x => PathUtility.GetRelativeAsset(x.FullName, true)).ToArray();
            var guids = AssetDatabase.FindAssets("t:Prefab", temps);
            var paths = guids.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToArray();
            EditorUtility.DisplayProgressBar("clear", "收集场景依赖...", 0.25f);
            var scenes = EditorBuildSettings.scenes.Where(x => x.enabled = true).Select(x => x.path).Concat(paths).ToArray();
            List<string> dependAssets = AssetDatabase.GetDependencies(scenes, true).Where(x => !x.StartsWith("Packages/")).Select(x => x.Replace("/", "\\")).ToList();

            EditorUtility.DisplayProgressBar("clear", "收集所有资源...", 0.5f);
            var dir = new DirectoryInfo(Application.dataPath);

            #region 忽略类型
            var ignors = new List<string>();
            if (ignorType.HasFlag(IgnorType.jpg))
            {
                ignors.Add(".jpg");
            }
            if (ignorType.HasFlag(IgnorType.png))
            {
                ignors.Add(".png");
            }
            if (ignorType.HasFlag(IgnorType.scene))
            {
                ignors.Add(".unity");
            }
            if (ignorType.HasFlag(IgnorType.shader))
            {
                ignors.Add(".shader", ".shadersubgraph", ".shadergraph");
            }
            if (ignorType.HasFlag(IgnorType.cs))
            {
                ignors.Add(".cs", ".js");
            }
            if (ignorType.HasFlag(IgnorType.font))
            {
                ignors.Add(".ttf",".ttc", ".TTF",".ttf", ".otf");
            }
            ignors.Add(".dll", ".meta", ".mdb", ".hlsl", ".cginc", ".asmdef");

            #endregion
            //默认不选中类型
            var unSelect = new List<string>();
            unSelect.Add(".xml", ".pfx", ".compute", ".rsp",".asset");

            List<string> temp = null;
            #region 收集要清理的信息
            if (folderType == FolderType.不清理文件夹内容)
            {
                temp = SearchAssets(dir, dependAssets, ignors);
                var ordered = temp.OrderBy(x => Path.GetExtension(x));
                temp = ordered.ToList();
            }
            else
            {
                temp = new List<string>();
                for (int i = 0; i < ignorePath.Count; i++)
                {
                    var p = PathUtility.GetFullPath(ignorePath[i]);
                    var tempDir = new DirectoryInfo(p);
                    temp.AddRange(SearchAssets(tempDir, dependAssets, ignors));
                }
                var ordered = temp.OrderBy(x => Path.GetExtension(x));
                temp = ordered.ToList();
            }
            EditorUtility.DisplayProgressBar("clear", "搜集所有文件类型...", 0.95f);

            //要删除的 asset类型
            var delateTypes = new string[] { "TMP_FontAsset", "LightingDataAsset" }; 

            var unSelectTemp = temp.Where(x => unSelect.Contains(Path.GetExtension(x)) && !AssetCheck(x,delateTypes)).ToList();
            foreach(var un in unSelectTemp)
            {
                temp.Remove(un);
            }
            #endregion

            var groups = temp.GroupBy(x => Path.GetExtension(x));
            rules = groups.Select(x => new TypeSelect(this, x.Key)).ToList();

            //生成清理信息
            results = new List<ClearInfo>();
            for (int i = 0; i < temp.Count; i++)
            {
                if (string.IsNullOrEmpty(temp[i])) continue;
                results.Add(new ClearInfo(temp[i]));
            }


            unSelectResults = new List<ClearInfo>();
            for (int i = 0; i < unSelectTemp.Count; i++)
            {
                if (string.IsNullOrEmpty(unSelectTemp[i])) continue;
                unSelectResults.Add(new ClearInfo(unSelectTemp[i],false));
            }


            EditorUtility.ClearProgressBar();
        }

        //检查是不是可以删除的.asset  返回是否可删除
        private bool AssetCheck(string path,string[] deletaTypes)
        {
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(object));
            if (obj == null) return false;
            if (deletaTypes.Contains(obj.GetType().Name))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [Button("删除选中的资源", ButtonSizes.Large), ButtonGroup("but"), GUIColor(1, 0.3f, 0.2f, 1)]
        public void Delete()
        {
            var str = GetDirctory();
            var name = "unuseAssets" + System.DateTime.Now.ToString("_yyyy-MM-dd_HH-mm-ss");
            var savePath = EditorUtility.SaveFilePanel("请选择保存路径", str, name, "unitypackage");
            if (string.IsNullOrEmpty(savePath)) return;
            var paths = results.Where(x => x.clear).Select(x => x.path).Concat(unSelectResults.Where(x=>x.clear).Select(x=>x.path)).ToArray();
            if (paths.Length == 0) return;

            EditorUtility.DisplayProgressBar("clear", $"导出package,共{paths.Length}个文件,请等待...", 0.5f);
            AssetDatabase.ExportPackage(paths, savePath, ExportPackageOptions.Default);

            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                EditorUtility.DisplayProgressBar("clear", $"删除{path}", 0.5f + (i + 1f) / paths.Length);
                path = PathUtility.GetFullPath(path);
                //AssetDatabase.DeleteAsset(path);
                File.Delete(path);
            }
            var size = new FileInfo(savePath).Length / 1024f / 1024 + "M";
            results.Clear();
            Debug.Log($"<color=yellow>资源清理完毕,共清理{paths.Length}个,导出资源包{size},路径;{savePath}</color>");
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
        private List<string> SearchAssets(DirectoryInfo dir, List<string> depends, List<string> ignors)
        {
            var results = new List<string>();
            var files = dir.GetFiles("*", SearchOption.TopDirectoryOnly).Where(x => !ignors.Contains(x.Extension))
                .Select(x => PathUtility.GetRelativeAsset(x.FullName, true)).Where(x => !depends.Contains(x)).ToArray();
            results.AddRange(files);
            var dirctorys = dir.GetDirectories("*", SearchOption.TopDirectoryOnly)
                .Where(x => x.Name != "Plugins" && x.Name != "Resources" && x.Name != "StreamingAssets" && x.Name != "Editor");
            if(folderType == FolderType.不清理文件夹内容)
            {
                dirctorys = dirctorys.Where(x => !ignorePath.Contains(PathUtility.GetRelativeAsset(x.FullName, true)));
            }
            foreach (var d in dirctorys)
            {
                results.AddRange(SearchAssets(d, depends, ignors));
            }
            return results;
        }
        private string GetDirctory()
        {
            var dir = new DirectoryInfo(Application.dataPath);
            dir = dir.Parent;
            var str = Path.Combine(dir.FullName, "ClearPackages");
            if (!Directory.Exists(str))
            {
                Directory.CreateDirectory(str);
            }
            return str;
        }
    }


    [System.Flags]
    public enum IgnorType
    {
        none = 0,
        jpg = 1 << 1,
        png = 1 << 2,
        scene = 1 << 3,
        shader = 1 << 4,
        cs = 1 << 5,
        font = 1 << 6,
        all = jpg | png | scene | shader | cs | font

    }
    public class ClearInfo
    {
        [TableColumnWidth(40, Resizable = false)]
        public bool clear = true;
        [TableColumnWidth(50, Resizable = false), PreviewField(Alignment = ObjectFieldAlignment.Center, Height = 25)]
        [ReadOnly]
        public Texture icon;
        [ReadOnly]
        public string path;
        public ClearInfo(string p,bool select = true)
        {
            path = p;
            icon = AssetDatabase.GetCachedIcon(p);
            clear = select;
        }
        [Button("选中"), TableColumnWidth(80, resizable: false)]
        private void 操作()
        {
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(object));
        }
    }
    public class TypeSelect
    {
        [TableColumnWidth(40, Resizable = false)]
        private bool select = true;
        private Yjj_AssetClear clear;
        public string expend;

        [ShowInInspector]
        public bool Select
        {
            get => select; set
            {
                var all = clear.results.Where(x => x.path.EndsWith(expend)).ToArray();
                all.Foreach(x => x.clear = value);
                select = value;
            }
        }

        public TypeSelect(Yjj_AssetClear c, string str)
        {
            clear = c; expend = str;
        }
    }
}

