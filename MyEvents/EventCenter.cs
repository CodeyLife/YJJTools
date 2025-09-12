using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


#region 消息类型
public class CenterEventArgs : EventArgs
{
    public int index;
    public string content;
    public CenterEventArgs() { }
    public CenterEventArgs(string content, int index    )
    {
        this.index = index;
        this.content = content;
    }
    public CenterEventArgs(string content)
    {
        this.content = content;
    }
    public override string ToString()
    {
        return $"{content}-{index}";
    }
}
[System.Serializable]
public class CenterEvent : UnityEvent<EventArgs> { }
#endregion

public class EventCenter : YjjSingleton<EventCenter>
{
    //public static EventCenter Instance;
#if UNITY_EDITOR
    public bool _debug = false;
#endif
    //private void Awake()
    //{
    //    if (Instance == null)
    //    {
    //        Instance = this;
    //    }
    //    else
    //    {
    //        DestroyImmediate(this.gameObject);
    //    }
    //}
    ///// <summary>
    ///// 消息字典
    ///// </summary>
    //public Dictionary<string, List<UnityEvent>> eventDic = new Dictionary<string, List<UnityEvent>>();
    /// <summary>
    /// enum类型字典
    /// </summary>
    public Dictionary<EventEnums, List<UnityEvent>> enumDic = new Dictionary<EventEnums, List<UnityEvent>>();


    public Dictionary<string, List<EventCallerBase>> callerDic = new Dictionary<string, List<EventCallerBase>>();

    private Stack<MsgRecevier> oldMsgs = new Stack<MsgRecevier>();
    private Queue<Action> msgQueue = new Queue<Action>();

    public void InvokeMsg(string str)
    {
        InvokeMsg(str, null, true);
    }

    /// <summary>
    /// 只激活带参数的注册事件
    /// </summary>
    /// <param name="str"></param>
    /// <param name="args"></param>
    public void InvokeMsg(string str, CenterEventArgs args, bool check)
    {
        msgQueue.Enqueue(() => DoMsg(str, args, check));
        Wait2InvokeMsg();
    }
    private void DoMsg(string str, CenterEventArgs args, bool check)
    {
#if UNITY_EDITOR
        var _debugMsg = $"收到事件;<color=red>{str} </color>";
        if (args != null)
        {
            _debugMsg += $">参数 <color=red>{args.content} {args.index} </color>";
        }
        Debug.Log(_debugMsg);
#else
  var _debugMsg = $"收到事件;{str} ";
        if (args != null)
        {
            _debugMsg += $">参数 {args.content} {args.index}";
        }
        Debug.Log(_debugMsg);
#endif
        if (!check)
        {
#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log($"消息:{str}-{args.content}:{args.index}不检测");
            }
#endif
        }
        else if (oldMsgs.Count > 0)
        {
            //上一个消息
            var lastMsg = oldMsgs.Peek();
            if (args == null || lastMsg.type != str)
            {
                ClearOlds();
            }
            else
            {
                var currentArr = args.content.Split('/');
                bool goon = CheckMsg(str, currentArr);
                while (goon && oldMsgs.Count > 0)
                {
                    goon = CheckMsg(str, currentArr);
                }
            }

        }
        else
        {
            Debug.Log($"之前的消息是空的");
        }

        if (callerDic.TryGetValue(str, out var callers))
        {
            foreach (var call in callers)
            {
                if (call.Check(args))
                {
                    //Debug.Log($"{call}开始执行事件",call.gameObject);
                    call.ActiveEvent(args);
                }
            }
        }
        oldMsgs.Push(new MsgRecevier(str, args, check));
    }


    //清理之前储存的消息
    public void ClearOlds()
    {
#if UNITY_EDITOR
        if (_debug)
        {
            Debug.Log("清理所有消息");
        }
#endif
        while (oldMsgs.Count > 0)
        {
            var msg = oldMsgs.Pop();
            InvokeEventExit(msg.type, msg.args);
        }

    }

    private void InvokeEventExit(string type,CenterEventArgs args)
    {
        if (callerDic.TryGetValue(type, out var cancels))
        {
            foreach (var cancel in cancels)
            {
                if (cancel.Check(args))
                {
                    //YjjUtility.Log($"{cancel.gameObject.name}执行取消事件",Color.green);
                    cancel.LeaveEvent?.Invoke();
                    if (cancel.hideOnExit)
                    {
                        cancel.shows.ForEach(x =>
                        {
                            if (x != null)
                            {
                                x.gameObject.SetActive(false);
                            }
                        });
                    }
                    if (cancel.showOnExit)
                    {
                        cancel.hides.ForEach(x =>
                        {
                            if (x != null)
                            {
                                x.SetActive(true);
                            }
                        });
                    }
                }
            }
        }
    }

    //返回是否继续往上取消息  参数是按'/'拆分的数组
    private bool CheckMsg(string type,string[] currentArr)
    {
        var lastMsg = oldMsgs.Peek();
        if (!lastMsg.check)  //上一个是不检测的消息， 直接撤销
        {
            oldMsgs.Pop();
            InvokeEventExit(lastMsg.type, lastMsg.args);
            return true;
        }
        if (lastMsg.args == null && currentArr != null)  //上一个没有详细参数，但是当前消息有  不往上取消息
        {
#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log($"上一个没有参数，当前有，所以不撤销");
            }
#endif
            return false;
        }
        if (currentArr == null || currentArr.Length <= 1) //当前消息没有参数  退出上一个消息
        {
#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log($"当前消息参数过少，执行上一个消息撤销");
            }
#endif
            oldMsgs.Pop();
            InvokeEventExit(lastMsg.type, lastMsg.args);
            return true;
        }

#if UNITY_EDITOR
        if (_debug)
        {
            Debug.LogError($"上一个{lastMsg.args}    当前{type}-{currentArr[0]}");
        }
#endif
        
        var lastContentArr = lastMsg.args.content.Split('/');
        if(currentArr[^2] ==lastContentArr[^1])
        {
            return false;
          
        }
        else
        {
#if UNITY_EDITOR
            if (_debug)
            {
                Debug.Log($"{lastMsg.type}-{lastMsg.args.content}:{lastMsg.args.index}退出");
            }
#endif
            oldMsgs.Pop();
            InvokeEventExit(lastMsg.type, lastMsg.args);
            return true;
        }
    }

    /// <summary>
    /// 注册对象进事件
    /// </summary>
    /// <param name="key"></param>
    /// <param name="caller"></param>
    public void RegisterCaller(string key, EventCallerBase caller)
    {
        if (callerDic.TryGetValue(key, out var datas))
        {
            datas.Add(caller);
        }
        else
        {
            callerDic.Add(key, new List<EventCallerBase>() { caller });
        }
    }

    public void AfterLoad2Invoke(string eventType, CenterEventArgs args, bool check)
    {
        msgQueue.Enqueue(() => DoMsg(eventType, args, check));
        Wait2InvokeMsg();
    }

    //等待下一帧 检测执行所有消息
    private void Wait2InvokeMsg()
    {
        StopAllCoroutines();
        this.DelayWhile(() =>
        {
            return !SceneLoader.Instance.isLoading;
          
        }, () =>
        {
            while (msgQueue.Count > 0)
            {
                msgQueue.Dequeue().Invoke();
            }
        });
    }

    [Button]
    private void DequeueMsg()
    {
        if (oldMsgs.Count > 0)
        {
            var msg = oldMsgs.Pop();
            Debug.Log($"取到了消息{msg.type}-{msg.args}");
        }
    }


    private class MsgRecevier
    {
        public string type;
        public CenterEventArgs args;
        public bool check;
        public MsgRecevier(string arg1,CenterEventArgs arg2,bool check = true)
        {
            type = arg1;args = arg2;this.check = check;
        }
    }
}
