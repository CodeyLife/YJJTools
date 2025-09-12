using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Yjj_EventWindow : OdinEditorWindow
{
    [LabelText("查找所有事件")]
    public bool searchAll = true;

    [Title("事件参数")]
    [ValueDropdown("GetValue", ExpandAllMenuItems = true), HideIf("searchAll")]
    public string eventType;
    [HideIf("searchAll")]
    public string content;
    [HideIf("searchAll")]
    public int index;

#if UNITY_EDITOR
    protected IEnumerable<string> GetValue()
    {
        return EventCenterType.Instance.types.Select(x => x);
    }
#endif

    [MenuItem("YJJ/事件管理窗口")]
    private static void Open()
    {
        var window = GetWindow<Yjj_EventWindow>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(650);
    }


    [ShowIf("@datas.Count>0"), LabelText("查询结果")]
    [TableList(AlwaysExpanded = true)]
    public List<Infos> datas = new List<Infos>();

    [ButtonGroup("查找")]
    [Button("查找发起者")]
    private void SearchInvoker()
    {
        datas.Clear();
        var trans = GetAllTrans();

        foreach (var item in trans)
        {
            var arr = item.GetComponents<EventArgsInvoker>();
            if (!searchAll)
            {
                var temp = arr.Where(x => x.eventType == eventType);
                if (!string.IsNullOrEmpty(content))
                {
                    temp = temp.Where(x => x.content == content);
                }
                if (index != 0)
                {
                    temp = temp.Where(x => x.index == index);
                }
                arr = temp.ToArray();
            }
            foreach (var data in arr)
            {
                datas.Add(new Infos { content = data.content, eventType = data.eventType, index = data.index, target = data.transform });
            }
        }
        datas = datas.OrderBy(x => x.eventType).ThenBy(x => x.content).ThenBy(x => x.index).ToList();
    }
    [ButtonGroup("查找")]
    [Button("查找监听者")]
    private void SearchListener()
    {
        var trans = GetAllTrans();
        datas.Clear();
        foreach (var tran in trans)
        {
            var arr = tran.GetComponents<EventCallerSingle>();
            if (!searchAll)
            {
                if (!string.IsNullOrEmpty(content))
                {
                    arr = null;
                }
                if (index != 0)
                {
                    arr = null;
                }
                if (arr != null)
                {
                    arr = arr.Where(x => x.eventType == eventType).ToArray();
                }
            }
            if (arr != null)
            {
                foreach (var data in arr)
                {
                    datas.Add(new Infos { eventType = data.eventType, target = data.transform });
                }
            }

            var arr2 = tran.GetComponents<EventCallerWithArgs>();

            if (!searchAll)
            {
                var temp = arr2.Where(x => x.eventType == eventType);
                if (!string.IsNullOrEmpty(content))
                {
                    temp = temp.Where(x => x.content == content);
                }
                if (index != 0)
                {
                    temp = temp.Where(x => x.index == index);
                }
                arr2 = temp.ToArray();
            }

            foreach (var data in arr2)
            {
                datas.Add(new Infos { eventType = data.eventType, target = data.transform, content = data.content, index = data.index });
            }
        }
        datas = datas.OrderBy(x => x.eventType).ThenBy(x => x.content).ThenBy(x => x.index).ToList();
    }

    private List<Transform> GetAllTrans()
    {
        var results = new List<Transform>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var transforms = Resources.FindObjectsOfTypeAll<Transform>()
             .Where(x => !EditorUtility.IsPersistent(x.root.gameObject) && !(x.gameObject.hideFlags == HideFlags.HideAndDontSave || x.gameObject.hideFlags == HideFlags.NotEditable) && x.gameObject.scene.isLoaded)
             .ToList();
            results.AddRange(transforms);
        }
        return results;
    }

    [System.Serializable]
    public class Infos
    {
        public string eventType;
        public string content;
        public int index;
        public Transform target;
    }
}
