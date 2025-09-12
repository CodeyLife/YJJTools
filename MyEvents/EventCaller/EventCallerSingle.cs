using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventCallerSingle : EventCallerBase
{
    [Title("该脚本会把对应的event和str注册到事件管理器")]
    [LabelText("Event事件")]
    public UnityEvent events = new UnityEvent();

#if UNITY_EDITOR
    public bool _debug = false;
#endif

    private void Awake()
    {
        events.AddListener(DoEvent);
    }

    public override void ActiveEvent(CenterEventArgs args)
    {
       // base.ActiveEvent();
       if(args == null)
        {
            events?.Invoke();
        }
    }
    //本地逻辑s
    protected void DoEvent()
    {
#if UNITY_EDITOR
        if (_debug)
        {
            Debug.Log($"{gameObject.name}执行了事件", gameObject);
        }
#endif
        shows.ForEach(x =>
        {
            if (x == null) return;
            x.SetActive(true);

        });
        hides.ForEach(x =>
        {
            if (x == null) return;
            x.SetActive(false);

        });
    }
    public override bool Check(CenterEventArgs args)
    {
        return args == null;
    }
    protected override void Register()
    {
        base.Register();

    }
    protected override void UnRegister()
    {
        base.UnRegister();
    }
}
