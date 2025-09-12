using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EventCallerBase : MonoBehaviour
{

    [ValueDropdown("GetValue", ExpandAllMenuItems = true)]
    [OnValueChanged("ChangeName")]
    public string eventType = "";


    [Title("执行事件时显示")]
    public List<GameObject> shows = new List<GameObject>();
    [ShowIf("@shows.Count>0"), LabelText("退出事件时关闭")]
    public bool hideOnExit = true;
    [Title("执行事件时关闭")]
    public List<GameObject> hides = new List<GameObject>();
    [ShowIf("@hides.Count>0"), LabelText("退出事件时显示")]
    public bool showOnExit = true;

    [LabelText("退出该事件执行"),PropertyOrder(1000)]
    public UnityEvent LeaveEvent = new UnityEvent();
#if UNITY_EDITOR
    protected IEnumerable<string> GetValue()
    {
        return EventCenterType.Instance.types.Select(x => x);
    }
#endif

    [LabelText("Enable控制消息是否注册")]
    public bool enableAndDisable = false;


    /// <summary>
    /// 激活事件
    /// </summary>
    public virtual void ActiveEvent(CenterEventArgs args)
    {

    }

    protected virtual void Start()
    {
        if (!enableAndDisable)
        {
            Register();
        }
    }

    protected void OnEnable()
    {
        if (enableAndDisable)
        {
            Register();
        }
    }

    protected void OnDisable()
    {
        if (enableAndDisable)
        {
            UnRegister();
        }
    }

    protected virtual void OnDestroy()
    {
        if (!enableAndDisable)
        {
            UnRegister();
        }
    }

    public virtual bool Check(CenterEventArgs args)
    {
        return true;
    }


    /// <summary>nen
    /// 注册事件
    /// </summary>
    protected virtual void Register()
    {
        EventCenter.Instance.RegisterCaller(eventType,this);
    }

    /// <summary>
    /// 取消注册消息
    /// </summary>
    protected virtual void UnRegister() 
    {
        if(EventCenter.Instance.callerDic.TryGetValue(eventType,out var list))
        {
            list.Remove(this);
        }
    }

#if UNITY_EDITOR
    [Button("显示枚举"),PropertyOrder(1000)]
    protected void ShowSO()
    {
        Sirenix.OdinInspector.Editor.OdinEditorWindow.InspectObject(EventCenterType.Instance);
    }

#endif
    protected virtual void ChangeName()
    {
        gameObject.name = eventType;
    }
}
