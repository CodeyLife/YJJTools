using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventArgsInvoker : MonoBehaviour
{
    [ValueDropdown("GetValue", ExcludeExistingValuesInList =false)]
    [OnValueChanged("ChangeName")]
    public string eventType;
    [OnValueChanged("ChangeName")]
    public int index;
    [OnValueChanged("ChangeName")]
    public string content;

    [Title("检查是否属于同级",subtitle:"不属于执行推出,属于储存消息.不检查直接储存消息")]
    public bool check = true;

    [LabelText("加载场景后执行消息")]
    public bool afterSceneLoadInvoke = false;


#if UNITY_EDITOR
    protected IEnumerable GetValue()
    {
        return EventCenterType.Instance.types.Select(x => new ValueDropdownItem(x,x));
    }
    [Button("创建一个消息接受器")]
    protected void CreatRecevier()
    {
        var go = new GameObject();
        go.transform.SetParent(transform.parent, false);
        var rec = go.AddComponent<EventCallerWithArgs>();
        rec.eventType = eventType;
        if (!string.IsNullOrEmpty(content))
        {
            rec.checkContent = true;
            rec.content = content;
        }
        if (index != 0)
        {
            rec.checkIndex = true;
            rec.index = index;
        }
        rec.GetType().GetMethod("ChangeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(rec,null);
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "CreatEventRecevie");

    }
#endif
    private CenterEventArgs _args;

    public CenterEventArgs Args { get
        {
            if(_args == null)
            {
                _args = new CenterEventArgs() { content = content, index = index };
            }
            return _args;
        }
        set => _args = value; }

    public void Invoke()
    {
        if (afterSceneLoadInvoke)
        {
            EventCenter.Instance.AfterLoad2Invoke(eventType, Args, check);
        }
        else
        {
            EventCenter.Instance.InvokeMsg(eventType, Args,check);
        }
    }
    protected void ChangeName()
    {
        gameObject.name = $"{eventType} {content} {index}";
    }
}
