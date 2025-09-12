#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;


namespace YJJTool
{
    public class NetworkDebuger : OdinEditorWindow
    {
        private static NetworkDebuger instance;
        [TableList(AlwaysExpanded = true, HideToolbar = true), HideLabel]
        public List<NetworkError> errors = new List<NetworkError>();

        public static NetworkDebuger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GetWindow<NetworkDebuger>();
                }
                return instance;
            }
            set => instance = value;
        }

        [Button("清理",buttonSize:ButtonSizes.Medium,Icon = SdfIconType.Bug),ShowIf("@errors.Count > 0"),GUIColor(0,1,0)]
        public void ClearError()
        {
            errors.Clear();
        }

        private static void PlayChange(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                Instance.errors.Clear();
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public static void UpdateError(NetworkError error)
        {
            Instance.errors.Add(error);
            Instance.Show();
        }


        public class NetworkError
        {
            public NetworkError(object obj, string fuc, string data, System.Exception e)
            {
                this.data = data;
                function = fuc;
                target = obj;
                this.content = e.ToString();
                exception = e;
            }
            [VerticalGroup("报错脚本"), HideLabel, TableColumnWidth(120, Resizable = false)]
            public object target;
            [VerticalGroup("报错函数"), HideLabel, TableColumnWidth(120, Resizable = false)]
            public string function;
            [VerticalGroup("报错数据"), HideLabel, MultiLineProperty,ReadOnly]
            public string data;
            [VerticalGroup("报错内容"), HideLabel, ReadOnly,VerticalGroup("content")]
            public string content;

            private System.Exception exception;

            [VerticalGroup("报错数据"),Button("Inspector")]
            private void SHowData()
            {
                GUIUtility.systemCopyBuffer = data;
                Debug.LogError(data);
            }

            [VerticalGroup("报错内容"), Button("Inspector")]
            private void SHowContent()
            {
                GUIUtility.systemCopyBuffer = content;
                Debug.LogError(content);
            }

            [ButtonGroup("操作"), Button("打开脚本"), TableColumnWidth(80, Resizable = false)]
            public  void OpenScript()
            {
                var type = target.GetType();
                var str = AssetDatabase.FindAssets($"{type.Name}")[0];
                var path = AssetDatabase.GUIDToAssetPath(str);
                var result = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
               // var resultPath = PathUtility.GetFullPath(path);
                //int i = 0;
                var msg = exception.StackTrace;
                var line = Regex.Match(msg, @"(?<=(.cs:)).*(?= )");
                Debug.Log(msg);

                AssetDatabase.OpenAsset(result, int.Parse(line.Captures[0].ToString()));
            }
        }
    }

}
#endif