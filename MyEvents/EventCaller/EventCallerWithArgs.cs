using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventCallerWithArgs : EventCallerBase
{
    [Title("该脚本会把对应的event和str注册到事件管理器")]

    public bool checkContent = true;
    [ShowIf("checkContent")]
    [OnValueChanged("ChangeName")]
    public string content;
    public bool checkIndex = false;
    [ShowIf("checkIndex")]
    [OnValueChanged("ChangeName")]
    public int index = 0;

#if UNITY_EDITOR
    public bool _debug = false;
#endif

    [LabelText("Event事件")]
    public UnityEvent events = new UnityEvent();

    public CenterEvent centerEvent = new CenterEvent();

    private void Awake()
    {
        events.AddListener(DoEvent);
      //  centerEvent.AddListener(CheckAndInvoke);
    }

    //本地逻辑s
    protected void DoEvent()
    {

        shows.ForEach(x =>
        {
            if (x != null)
            {
                x.SetActive(true);
                //if (hideOnExit)
                //{
                //    EventCenter.Instance.tempQueues.Enqueue(x);
                //}
            }
           
        });
        hides.ForEach(x =>
        {
           
            if (x != null)
            {
                x.SetActive(false);
                //if (showOnExit)
                //{
                //    EventCenter.Instance.tempQueues.Enqueue(x);
                //}
            }
           
        });
    }

    protected override void Start()
    {
       
        base.Start();
     
    }
    protected override void Register()
    {
        base.Register();
    }
    protected override void UnRegister()
    {
        base.UnRegister();
    }
    public override bool Check(CenterEventArgs args)
    {
        if(args == null)
        {
            return false;
        }
        if (checkContent && args.content != content)
        {
            return false;
        }else if (checkIndex && args.index != index)
        {
            return false;
        }
        return true;
    }

    public override void ActiveEvent(CenterEventArgs args)
    {
#if UNITY_EDITOR
        if (_debug)
        {
            Debug.Log($"{gameObject}执行了事件", gameObject);
        }
#endif
        events?.Invoke();
        centerEvent?.Invoke(args);
    }
    /// <summary>
    /// 不给参数激活事件
    /// </summary>
    public void InvokeEventFromOther()
    {
        ActiveEvent(null);
    }
    //private void CheckAndInvoke(EventArgs arg0)
    //{
    //    var args = arg0 as CenterEventArgs;
    //    if (args != null)
    //    {
    //        if (checkContent)
    //        {
    //            if(args.content != content)
    //            {
    //                return;
    //            }
    //        }
    //        if (checkIndex)
    //        {
    //            if(args.index != index)
    //            {
    //                return;
    //            }
    //        }
    //        Debug.Log($"执行了事件", gameObject);
    //        events?.Invoke();
    //    }
    //}
    protected override void ChangeName()
    {
        var str =  eventType;
        if (checkContent)
        {
            str += $" {content}";
        }
        if (checkIndex)
        {
            str += $" {index}";
        }
        gameObject.name = str;
    }

}
