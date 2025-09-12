using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace YjjTool
{
    public class Yjj_ReferenceWindows : OdinMenuEditorWindow
    {
        public static Yjj_ReferenceWindows Instance
        {
            get { var window = GetWindow<Yjj_ReferenceWindows>(); window.Show(); return window; }
        }

        [Header("查询的脚本")]
        public Component script;
        [LabelText("全局查询")]
        public bool globleSearch = false;
        [Header("查询的父节点"), HideIf("globleSearch")]
        public List<Transform> searchRoots = new List<Transform>();
        [LabelText("查找组件的transfrom和Gameobject")]
        public bool transAndGameobj = true;
        [LabelText("查找组件的父节点引用")]
        public bool findParent = false;
        [LabelText("查找事件对该脚本的引用")]
        public bool findEventReference = true;
        [LabelText("查找事件参数"),ShowIf("findEventReference")]
        public bool findEventArgs = true;
        [LabelText("查找字段对该脚本的引用")]
        public bool findObjectReference = true;

        [MenuItem("YJJ/查找场景中的引用")]
        public static void Open()
        {
            GetWindow<Yjj_ReferenceWindows>().Show();
        }
        private Dictionary<string, EventReferenceInfo> dic = new Dictionary<string, EventReferenceInfo>();
        private ObjReferenceInfo obj;
        private Transform scriptTrans;
        private GameObject scriptGO;
        private Transform parentTrans;
        private GameObject parentGO;
        private ObjReferenceInfo parentReference;


        [Button]
        public  void Search()
        {
            //Debug.Log(typeof(CenterEvent).IsSubclassOf(typeof(UnityEventBase)));
            var removes = searchRoots.Where(x => x == null).ToArray();
            foreach (var remove in removes)
            {
                searchRoots.Remove(remove);
            }
            if (searchRoots.Count == 0 && !globleSearch)
            {
                if (EditorUtility.DisplayDialog("空查询", "当前没有添加查询的父节点,是否需要开启全局查询", "开启", "取消"))
                {
                    globleSearch = true;
                }
                else
                {
                    return;
                }
            }
            if (script == null) return;
            if (transAndGameobj)
            {
                scriptTrans = script.transform;
                scriptGO = script.gameObject;
            }
            dic.Clear();
            List<Task> tasks = new List<Task>();
            if (findObjectReference)
            {
                obj = new ObjReferenceInfo
                {
                    objName = script.GetType().Name,
                    goName = script.gameObject.name
                };
            }
            if (findParent)
            {
                parentReference = new ObjReferenceInfo();
                parentTrans = script.transform.parent;
                if (parentTrans != null)
                {
                    parentGO = parentTrans.gameObject;
                    parentReference.objName = parentGO.name;
                }
            }
            var transforms = new List<Transform>();
            if (globleSearch)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    transforms = Resources.FindObjectsOfTypeAll<Transform>()
                     .Where(x => !EditorUtility.IsPersistent(x.root.gameObject) && !(x.gameObject.hideFlags == HideFlags.HideAndDontSave || x.gameObject.hideFlags == HideFlags.NotEditable) && x.gameObject.scene.isLoaded)
                     .ToList();
                }
            }
            else
            {
                foreach (Transform t in searchRoots)
                {
                    if (t == null) continue;
                    transforms.AddRange(t.GetComponentsInChildren<Transform>());
                }
            }

            int length = transforms.Count;
            for (int i = 0; i < length; i++)
            {
                var arr = transforms[i].GetComponents<Component>();
                EditorUtility.DisplayProgressBar("搜索进度", $"{i + 1}/{length}", (i + 1f) / length);
                for (int k = 0; k < arr.Length; k++)
                {
                    var c = arr[k];
                    if(c == null)
                    {
                        continue;
                    }
                    var goName = script.gameObject.name;
                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            Find(c, goName);
                        }
                        catch(Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }));
                }
            }
            Task.WaitAll(tasks.ToArray());
            EditorUtility.ClearProgressBar();
            if (MenuTree == null)
            {
                ForceMenuTreeRebuild();
            }
            MenuTree.MenuItems.Clear();
            MenuTree.Add("查询设置", this);
            int menuIndex = 0;
            foreach (var key in dic.Keys)
            {
                dic.TryGetValue(key, out var data);
                MenuTree.Add(key, data);
                menuIndex++;
            }
            if (findParent && parentReference.list.Count > 0)
            {
                MenuTree.Add("父节点引用", parentReference);
                menuIndex++;
            }
            if (findObjectReference && obj.list.Count > 0)
            {
                MenuTree.Add("组件引用", obj);
                menuIndex++;
            }
            MenuTree.MenuItems[menuIndex].Select();
            GC.Collect();
        }
        private EventReferenceInfo GetInfo(string str,string goName)
        {
            if (dic.TryGetValue(str, out var info))
            {

            }
            else
            {
                info = new EventReferenceInfo
                {
                    eventName = str,
                    goName = goName,
                };
                dic.Add(str, info);
            }
            return info;
        }
        private void Find(Component com, string goName)
        {
            var tp = com.GetType();
            var fileds = tp.GetFields();
            foreach (var field in fileds)
            {
                var value = field.GetValue(com);
                #region 查找事件
                if (findEventReference)
                {
                    var baseType = typeof(UnityEventBase);
                    if (field.FieldType.IsSubclassOf(baseType))
                    {
                        UnityEventBase ue = field.GetValue(com) as UnityEventBase;
                        int length = ue.GetPersistentEventCount();
                        List<string> objs = new List<string>();
                        if (length > 0 && findEventArgs)
                        {
                            //持久化事件
                            var fieldInfo = baseType.GetField("m_PersistentCalls", BindingFlags.NonPublic | BindingFlags.Instance);
                            var m_PersistentCalls = fieldInfo.GetValue(ue);
                            fieldInfo = fieldInfo.FieldType.GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
                            var calls = fieldInfo.GetValue(m_PersistentCalls);
                            var list = calls as IEnumerable<object>;
                            foreach (var item in list)
                            {
                                var itemType = item.GetType();
                                var mode = itemType.GetField("m_Mode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(item).ToString();
                                var argumentsType = itemType.GetField("m_Arguments", BindingFlags.NonPublic | BindingFlags.Instance);
                                var arguments = argumentsType.GetValue(item);
                                //foreach(var f in arguments.GetType().GetFields( BindingFlags.NonPublic | BindingFlags.Instance))
                                //{
                                //    Debug.Log(f);
                                //}
                                FieldInfo resultInfo = null;
                                if (mode == "String")
                                {
                                    resultInfo = argumentsType.FieldType.GetField("m_StringArgument", BindingFlags.NonPublic | BindingFlags.Instance);
                                }
                                else if (mode == "Bool")
                                {
                                    resultInfo = argumentsType.FieldType.GetField("m_BoolArgument", BindingFlags.NonPublic | BindingFlags.Instance);
                                }
                                else if (mode == "Float")
                                {
                                    resultInfo = argumentsType.FieldType.GetField("m_FloatArgument", BindingFlags.NonPublic | BindingFlags.Instance);
                                }
                                else if (mode == "Int")
                                {
                                    resultInfo = argumentsType.FieldType.GetField("m_IntArgument", BindingFlags.NonPublic | BindingFlags.Instance);
                                }
                                else if (mode == "Object")
                                {
                                    resultInfo = argumentsType.FieldType.GetField("m_ObjectArgument", BindingFlags.NonPublic | BindingFlags.Instance);
                                    var result = resultInfo.GetValue(arguments);
                                    string temp = null;
                                    if (result == null)
                                    {
                                        temp = "Null";
                                    }
                                    else
                                    {
                                        temp = resultInfo.FieldType.ToString();
                                    }
                                    objs.Add(temp);
                                    continue;
                                }
                                objs.Add(resultInfo == null ? null : resultInfo.GetValue(arguments).ToString());
                            }
                        }
                        for (int i = 0; i < length; i++)
                        {
                            var target = ue.GetPersistentTarget(i);
                            if (target == script)
                            {
                                var method = ue.GetPersistentMethodName(i);
                                var info = GetInfo(method, goName);
                                //var methedinfo = UnityEventBase.GetValidMethodInfo(target, method, Type.EmptyTypes);
                                info.Add(field, com, objs.Count > i ? objs[i] : null);
                            }
                            else if (transAndGameobj && (target == scriptTrans || target == scriptGO))
                            {
                                 var method = ue.GetPersistentMethodName(i);
                                 var info = GetInfo(method, goName);
                                 info.Add(field, com, objs.Count > i ? objs[i] : null);
                            }
                        }
                    }
                }
                #endregion
                //查找字段
                if (findObjectReference)
                {

                    var st = script.GetType();
                    #region 查找该组件的transform和gameobject
                    if (transAndGameobj)
                    {
                        if(st != typeof(RectTransform) && value == (object)scriptTrans)
                        {
                            obj.Add(field, com);
                        }
                        if (value == (object)scriptGO)
                        {
                            obj.Add(field, com);
                        }
                        if (typeof(System.Collections.IList).IsAssignableFrom(field.FieldType))
                        {
                            var arr = field.FieldType.GenericTypeArguments;
                            if (arr.Length == 1 && (arr[0] == typeof(Transform) || arr[0] == typeof(GameObject)))
                            {
                                var list = value as IList;
                                foreach (var l in list)
                                {
                                    if (l == (object)scriptTrans || (object)scriptGO == l)
                                    {
                                        obj.Add(field, com);
                                    }
                                }
                            }
                        }

                    }
                    #endregion
                    if ((object)value == script)
                    {
                        obj.Add(field, com);
                    }
                    #region 查找list属性是否包含该类型
                    if (typeof(IList).IsAssignableFrom(field.FieldType))
                    {
                        var arr = field.FieldType.GenericTypeArguments;
                        if (arr.Length == 1 && (arr[0] == st))
                        {
                            var list = value as IList;
                            foreach (var l in list)
                            {
                                if (l == (object)script)
                                {
                                    obj.Add(field, com);
                                }
                            }
                        }
                    }
                    #endregion
                }
                #region 查找父物体
                if (findParent && parentTrans != null)
                {
                    if (value == (object)parentTrans || value == (object)parentGO)
                    {
                        parentReference.Add(field, com);
                    }
                    if (typeof(System.Collections.IList).IsAssignableFrom(field.FieldType))
                    {
                        var arr = field.FieldType.GenericTypeArguments;
                        if (arr.Length == 1 && (arr[0] == typeof(Transform) || arr[0] == typeof(GameObject)))
                        {
                            var list = value as IList;
                            foreach (var l in list)
                            {
                                if (l == (object)parentTrans || (object)parentGO == l)
                                {
                                    parentReference.Add(field, com);
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            var propers = tp.GetProperties();
            foreach (PropertyInfo property in propers)
            {
                #region 查找事件

                if (findEventReference)
                {
                    var baseType = typeof(UnityEventBase);
                    if (property.PropertyType.IsSubclassOf(baseType))
                    {
                        UnityEventBase ue = property.GetValue(com) as UnityEventBase;
                        int length = ue.GetPersistentEventCount();
                        List<string> objs = new List<string>();
                        if (length > 0 && findEventArgs)
                        {
                            //持久化事件
                            var fieldInfo = baseType.GetField("m_PersistentCalls", BindingFlags.NonPublic | BindingFlags.Instance);
                            var m_PersistentCalls = fieldInfo.GetValue(ue);
                            fieldInfo = fieldInfo.FieldType.GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
                            var calls = fieldInfo.GetValue(m_PersistentCalls);
                            var list = calls as IEnumerable<object>;
                            foreach(var item in list)
                            {
                                var itemType = item.GetType();
                                var mode = itemType.GetField("m_Mode", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(item).ToString();
                                var argumentsType = itemType.GetField("m_Arguments", BindingFlags.NonPublic | BindingFlags.Instance);
                                var arguments = argumentsType.GetValue(item);
                                //foreach(var f in arguments.GetType().GetFields( BindingFlags.NonPublic | BindingFlags.Instance))
                                //{
                                //    Debug.Log(f);
                                //}
                                FieldInfo resultInfo = null;
                                if (mode == "String")
                                {
                                    resultInfo = argumentsType.FieldType.GetField("m_StringArgument", BindingFlags.NonPublic | BindingFlags.Instance);
                                }
                                else if (mode == "Bool")
                                {
                                    resultInfo = argumentsType.FieldType.GetField("m_BoolArgument", BindingFlags.NonPublic | BindingFlags.Instance);
                                }
                                else if (mode == "Float")
                                {
                                    resultInfo = argumentsType.FieldType.GetField("m_FloatArgument", BindingFlags.NonPublic | BindingFlags.Instance);
                                }
                                else if (mode == "Int")
                                {
                                    resultInfo = argumentsType.FieldType.GetField("m_IntArgument", BindingFlags.NonPublic | BindingFlags.Instance);
                                }
                                else if (mode == "Object")
                                {
                                    resultInfo = argumentsType.FieldType.GetField("m_ObjectArgument", BindingFlags.NonPublic | BindingFlags.Instance);
                                    var result = resultInfo.GetValue(arguments);
                                    string temp = null;
                                    if(result == null)
                                    {
                                        temp = "Null";
                                    }
                                    else
                                    {
                                        temp = resultInfo.FieldType.ToString();
                                    }
                                    objs.Add(temp);
                                    continue;
                                }
                                objs.Add(resultInfo == null?null:resultInfo.GetValue(arguments).ToString());
                            }
                        }

                        for (int i = 0; i < length; i++)
                        {
                            var target = ue.GetPersistentTarget(i);
                            if (target == script)
                            {
                                var method = ue.GetPersistentMethodName(i);
                                EventReferenceInfo info = GetInfo(method, goName);
                                info.Add(property, com, objs.Count > i ? objs[i] : null);
                            }else if (transAndGameobj && (target == scriptTrans || target == scriptGO))
                            {
                                var method = ue.GetPersistentMethodName(i);
                                var info = GetInfo(method, goName);
                                info.Add(property, com, objs.Count > i ? objs[i] : null);
                            }
                        }
 
                    }
                }
                #endregion
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            if(MenuTree == null)
            {
                var tree = new OdinMenuTree();
                tree.Add("查询设置", this);
                return tree;
            }
            else
            {
                return MenuTree;
            }
        }
        #region 数据
        public class EventReferenceInfo
        {
            [HideInInspector]
            public string eventName;
            [HideInInspector]
            public string goName;

            [Title("@eventName", TitleAlignment = TitleAlignments.Centered, HorizontalLine = true, Bold = true, Subtitle = "@goName"), HideLabel]
            [TableList(AlwaysExpanded = true, CellPadding = 5, HideToolbar = true, ShowIndexLabels = false, IsReadOnly = true)]
            public List<ReferenceDetail> list = new List<ReferenceDetail>();

            //[LabelText("引用情况"), HorizontalGroup("use")]
            //public List<string> useInfos = new List<string>();
            //[LabelText("有引用该方法的物体"), HorizontalGroup("use")]
            //public List<Component> gos = new List<Component>();

            public void Add(MemberInfo field, Component com,string args,string details = null)
            {
                var attribute = field.GetCustomAttributes(typeof(LabelTextAttribute), true);
                if (attribute.Length > 0)
                {
                    list.Add(new ReferenceDetail(field.Name, com,args, (attribute[0] as LabelTextAttribute).Text));
                }
                else
                {
                    list.Add(new ReferenceDetail(field.Name, com,args,null));
                }
            }
        }
        public class ObjReferenceInfo
        {
            [HideInInspector]
            public string objName;
            [HideInInspector]
            public string goName;
            [Title("@objName",TitleAlignment = TitleAlignments.Centered,HorizontalLine = true,Bold =true,Subtitle ="@goName")]
            [HideLabel]
            [TableList(AlwaysExpanded = true,CellPadding = 5,HideToolbar =true,ShowIndexLabels =false,IsReadOnly =true)]
            public List<ReferenceDetail> list = new List<ReferenceDetail>();
            public void Add(FieldInfo field, Component com)
            {
                var attribute = field.GetCustomAttributes(typeof(LabelTextAttribute), true);
                if (attribute.Length > 0)
                {
                    //  obj.useInfos.Add((attribute[0] as LabelTextAttribute).Text);
                   list.Add(new ReferenceDetail( field.Name, com, (attribute[0] as LabelTextAttribute).Text));
                }
                else
                {
                    //  obj.useInfos.Add(field.Name);
                    list.Add(new ReferenceDetail(field.Name, com));
                }
            }
        }
        [System.Serializable]
        public class ReferenceDetail
        {
            [VerticalGroup("属性名"),HideLabel, ShowIf("@string.IsNullOrEmpty(descript)")]
            public string fieldName;

            [VerticalGroup("属性名"),HideLabel,HideIf("@string.IsNullOrEmpty(descript)")]
            public string descript;

            public Component component;

            [ShowIf("@args!=null")]
            public string args = null;

            [Button("打开脚本"),TableColumnWidth(200,resizable:false),ButtonGroup("操作")]
            private async void Open()
            {
                var type = component.GetType();
                var str = AssetDatabase.FindAssets($"{type.Name}")[0];
                var path = AssetDatabase.GUIDToAssetPath(str);
                var target = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                var resultPath = PathUtility.GetFullPath(path);
                int i = 0;
                await Task.Run(() =>
                {
                    using (StreamReader sr = new StreamReader(resultPath))
                    {
                        string line;
                        var targetInfo = fieldName;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (Regex.IsMatch(line, $"\\b{targetInfo}\\b"))
                            {
                                break;
                            }
                            i++;
                        }
                    }
                });
                AssetDatabase.OpenAsset(target, i + 1);
            }
            [Button("打开面板"),ButtonGroup("操作")]
            private void Modefine()
            {
                var window = OdinEditorWindow.InspectObject(component);
                //var rect = window.position;
                //window.position = new Rect(rect.position, new Vector2(rect.width, 1600));
                window.position = GUIHelper.GetEditorWindowRect().AlignCenter(450,680);
            }
            public ReferenceDetail(string f,Component c,string dec = "")
            {
                fieldName = f;component = c;descript = dec;
            }
            public ReferenceDetail(string f, Component c,string obj,string dec = null)
            {
                
                fieldName = f; component = c;args = obj; descript = dec;
            }
        }
        #endregion
        [MenuItem("CONTEXT/Component/查找引用", priority = 1000)]
        private static void FindObj(MenuCommand command)
        {
            var window = Instance;
            window.script = (Component)command.context;
            window.Search();
        }

    }
}

