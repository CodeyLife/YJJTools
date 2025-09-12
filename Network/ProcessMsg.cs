using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[AddComponentMenu("_YjjTool/网络/ProcessMsg")]
public class ProcessMsg : YjjSingleton<ProcessMsg>
{
    public bool requestServer = false;
    public bool requestClient = false;
    private Dictionary<TcpType, stringEvent> dic = new Dictionary<TcpType, stringEvent>();
    private void Start()
    {
        if (requestServer)
        {
            TcpServer.Instance.RecevieEvent.AddListener(Recevie);
        }
        if (requestClient)
        {
            YjjClient.Instance.RecevieEvent.AddListener(Recevie);
        }
    }


    /// <summary>
    /// 注册TCP对应消息类型的事件
    /// </summary>
    /// <param name="t"></param>
    /// <param name="action"></param>
    public void AddAction(TcpType t,UnityAction<string> action)
    {
        if (dic.TryGetValue(t, out var e))
        {
            e.AddListener(action);
        }
        else
        {
            e = new stringEvent();
            e.AddListener(action);
            dic.Add(t, e);
        }
    }
    private void Recevie(string arg0)
    {
        var data = JsonConvert.DeserializeObject<TCPData>(arg0);
        if (Enum.TryParse<TcpType>(data.type, out var t))
        {
            if (dic.TryGetValue(t, out var e))
            {
                e?.Invoke(data.content);
            }
        }
        else
        {
            Debug.Log($"处理消息出错;{arg0}");
        }
    }
    /// <summary>
    /// json转义
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static string PackJson(string json)
    {
        return json.Replace("\"", "\\\"");
    }
    /// <summary>
    /// 解析转义json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static T UnPackJson<T>(string json)
    {
        json = json.Replace("\\\"", "\"");
        return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
    }

    /// <summary>
    /// 解析转义json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json"></param>
    /// <returns></returns>
    public static string UnPackJson(string json)
    {
        json = json.Replace("\\\"", "\"");
        return json;
    }

    /// <summary>
    /// 把"加转移
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static string EncodeJson(JObject json)
    {
        var str = JsonConvert.SerializeObject(json, Formatting.None, new JsonSerializerSettings
        {
            StringEscapeHandling = StringEscapeHandling.EscapeHtml
        });
        return str;
    }
}/// <summary>
/// 用于tcp消息传输
/// </summary>
public class TCPData
{
    public string type;
    public string content;
    public TCPData()
    {

    }
    public TCPData(string type,string content)
    {
        this.type = type;
        this.content = content;
    }
    public TCPData(TcpType t,string content)
    {
        type = t.ToString();
        this.content = content;
    }
    public TCPData(TcpType t,object obj)
    {
        type = t.ToString();
        var str = JsonConvert.SerializeObject(obj);
        content = ProcessMsg.PackJson(str);
    }
}

