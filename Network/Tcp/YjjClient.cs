using Sirenix.OdinInspector;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("_YjjTool/网络/客户端")]
[RequireComponent(typeof(Loom))]
public class YjjClient : YjjSingleton<YjjClient>
{
    public bool raw = false;
    private TcpClient client;
    public stringEvent RecevieEvent = new stringEvent();
    /// <summary>
    /// 申请连接时找不到服务器
    /// </summary>
    public UnityEvent NotFoundEvent = new UnityEvent();
    /// <summary>
    /// 与服务器丢失连接
    /// </summary>
    public UnityEvent LostEvent = new UnityEvent();
    public UnityEvent ConectEvent = new UnityEvent();
    [ShowIf("raw")]
    public ByteEvent RawEvent = new ByteEvent();
    byte[] data;
    private DataCache cache;
    private NetworkStream stream;
    /// <summary>
    /// 返回是否连接
    /// </summary>
    public bool IsConected
    {
        get
        {
            return client != null && client.Connected;
        }
    }
    [Button("连接服务器")]
    public void Conect(string ip, int port, Action<bool> callback)
    {
        if (IPAddress.TryParse(ip, out var add))
        {
            Conect(add, port, callback);
        }
        else
        {
            callback?.Invoke(false);
        }
    }
    /// <summary>
    /// 连接服务器的回调
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="callback"></param>
    public async void Conect(IPAddress ip, int port, Action<bool> callback)
    {
        try
        {
            if (client != null)
            {
                Close();
                await Task.Delay(500);
            }
            client = new TcpClient();
            await client.ConnectAsync(ip, port);
            if (!client.Connected)
            {
                NotFoundEvent?.Invoke();
                callback?.Invoke(false);
            }
            else
            {
                data = new byte[client.ReceiveBufferSize];
                Debug.Log($"{data.Length}数据缓存长度");
                cache = new DataCache(data.Length);
                stream = client.GetStream();
                Debug.Log("开始接收服务器消息");
                stream.BeginRead(data, 0, data.Length, ReceiveMsg, stream);
                ConectEvent?.Invoke();
                callback?.Invoke(true);
            }
        }
        catch (System.Exception e)
        {
            callback?.Invoke(false);
            Debug.LogException(e);
        }
    }

#if UNITY_EDITOR
    [Button]
    private void TestClient()
    {
        Conect("127.0.0.1", 8000, (b)=>
        { Debug.Log(b); });
        RecevieEvent.AddListener((str) => Debug.Log($"{str}:{Time.realtimeSinceStartup}"));
    }
#endif
    /// <summary>
    /// 接收消息回调
    /// </summary>
    /// <param name="ar"></param>
    private void ReceiveMsg(IAsyncResult ar)
    {
        int length;
        var stream = client.GetStream();
        try
        {
            lock (stream)
            {
                length = stream.EndRead(ar);
            }
            if (length == 0)
            {
                // TODO
                Debug.Log("与服务器断开");
                LostEvent?.Invoke();
                return;
            }
            else
            {
                if (!raw)
                {
                    cache.Append(data, length);
                    while (cache.GetValue(out var str))
                    {
                        //Debug.Log($"收到服务器消息:{str}");
                        Loom.Instance.Enqueue(() =>
                        {
                     
                            RecevieEvent?.Invoke(str);
                        });
                    }
                }
                else
                {
                    Loom.Instance.Enqueue(() =>
                    {
                        // data数组中从索引0开始，长度为length的区域就是收到的数据
                        byte[] receivedData = new byte[length];
                        Array.Copy(data, 0, receivedData, 0, length);
                        var str = Encoding.UTF8.GetString(receivedData);
                        RecevieEvent?.Invoke(str);
                
                    });
                
                }
            }
            lock (stream)
            {
                stream.BeginRead(data, 0, data.Length, ReceiveMsg, stream);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    [Button("发送消息")]
    public void SendMsg(string msg)
    {
        try
        {
            var data = raw?Encoding.UTF8.GetBytes(msg): NetworkUtility.Pack(msg);
            lock (stream)
            {
                stream.Write(data, 0, data.Length);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    /// <summary>
    /// 转为json发送
    /// </summary>
    /// <param name="obj"></param>
    public void Send(object obj)
    {
        var msg = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        SendMsg(msg);
    }
    [Button]
    /// <summary>
    /// 关闭当前已有连接
    /// </summary>
    public void Close()
    {
        if (client != null)
        {
            client.Close();
            client.Dispose();
        }
    }
    private void OnDestroy()
    {
        Close();
    }
}
