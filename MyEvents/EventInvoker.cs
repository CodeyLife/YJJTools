using UnityEngine;

public class EventInvoker : MonoBehaviour
{
    public static EventInvoker Instance;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    public void Invoke(string msgType)
    {
        EventCenter.Instance.InvokeMsg(msgType);
        //Debug.Log(msgType);
    }

    public void Invoke(EventEnums enums)
    {
        EventCenter.Instance.InvokeMsg(enums.ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="msgType"></param>
    /// <param name="args"></param>
    /// <param name="check">如果不检查,不会执行之前事件的退出，检查会检查当前消息是否同一个组</param>
    public static void Invoke(string msgType,CenterEventArgs args,bool check)
    {
        EventCenter.Instance.InvokeMsg(msgType, args,check);
    }

    public void AfterLoadSceneInvoke(string msgType)
    {
        EventCenter.Instance.AfterLoad2Invoke(msgType,null,true);
    }
}
