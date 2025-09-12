using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

[AddComponentMenu("_YjjTool/网络/服务器")]
[RequireComponent(typeof(Loom))]
public class TcpServer : YjjSingleton<TcpServer>
{
    public bool raw = false;
    private List<TcpListener> servers = new List<TcpListener>();
    [InfoBox("如果不勾选需要在脚本里手动初始化服务器", visibleIfMemberName: "@!InitAtStart")]
    [LabelText("程序运行时初始化服务器")]
    public bool InitAtStart = false;
    public bool multipleNetInterface = false;
    [ShowIf("InitAtStart")]
    public int port = 8888;
    public List<ClientObject> clientList = new List<ClientObject>();
    List<Thread> listenThraeds = new List<Thread>();
    public stringEvent RecevieEvent = new stringEvent();
    public stringEvent ConnectEvent = new stringEvent();
    public stringEvent DisconnectEvent = new stringEvent();
    protected override void Awake()
    {
        base.Awake();
        if (InitAtStart)
        {
            InitServer(port);
        }
    }

    [Button("初始化服务器")]
    public void InitServer(int port)
    {
        if (!multipleNetInterface)
        {
            IPEndPoint point = new IPEndPoint(IPAddress.Any, port);
            if (servers.Count > 0)
            {
                CloseServer();
            }
            var server = new TcpListener(point);
            server.Start();
            var listenThraed = new Thread(() => { RecevieConnect(server); });
            listenThraed.IsBackground = true;
            listenThraed.Start();
            listenThraeds.Add(listenThraed);
            servers.Add(server);
            Debug.Log($"服务器初始化完毕,服务器地址:{NetworkUtility.GetIP()},服务器端口:{port}");
        }
        else
        {
            var ips = NetworkUtility.GetAllIP();
            if (servers.Count > 0)
            {
                CloseServer();
            }
            foreach (var ip in ips)
            {
                IPEndPoint point = new IPEndPoint(IPAddress.Parse(ip), port);
                var server = new TcpListener(point);
                server.Start();
                var listenThraed = new Thread(() => { RecevieConnect(server); });
                listenThraed.IsBackground = true;
                listenThraed.Start();
                listenThraeds.Add(listenThraed);
                servers.Add(server);
                Debug.Log($"服务器初始化完毕,服务器地址:{ip},服务器端口:{port}");
            }
        }
    }
    /// <summary>
    /// 关闭服务器
    /// </summary>
    private void CloseServer()
    {
        foreach (var listen in servers)
        {
            listen.Stop();
        }
        servers.Clear();
        for (int i = 0; i < clientList.Count; i++)
        {

            clientList[i].Close();
        }
        clientList.Clear();

        foreach (var t in listenThraeds)
        {
            t?.Interrupt();
            t?.Abort();
        }
        listenThraeds.Clear();


    }
    private void OnDestroy()
    {
        CloseServer();
    }
    /// <summary>
    /// 有客户端连接进来
    /// </summary>
    /// <param name="obj"></param>
    private void RecevieConnect(TcpListener server)
    {
        try
        {
            while (true)
            {
                var client = server.AcceptTcpClient();
                var ep = client.Client.RemoteEndPoint;
                Debug.Log($"{ep}连接到了服务器");
                clientList.Add(new ClientObject(client, RecevieEvent,raw));
                ConnectEvent?.Invoke(client.Client.RemoteEndPoint.ToString());
            }

        }
        catch (SystemException e)
        {
            Debug.LogException(e);
        }
    }
    [Button("发送消息")]
    public void Send(string str)
    {
        var data = raw?Encoding.UTF8.GetBytes(str): NetworkUtility.Pack(str);
        for (int i = 0; i < clientList.Count; i++)
        {
            clientList[i].Send(data);
        }
    }
    public void Send(object obj)
    {
        var str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        Send(str);
    }
    #region editor Function
#if UNITY_EDITOR
    [Button]
    private void MultipleTest(int length = 100, int msgLength = 1024)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < msgLength; i++)
        {
            sb.Append($"{UnityEngine.Random.Range(0, 10)}");
        }
        string str = sb.ToString();

        for (int i = 0; i < length; i++)
        {
            Send($"测试消息:{i.ToString()}:{str}");
        }
    }
#endif
    #endregion
}
public class ClientObject
{
    private TcpClient client;
    private NetworkStream stream;
    private byte[] data;
    DataCache cache;
    private stringEvent RecevieEvent;
    bool raw = false;

    public ClientObject(TcpClient client, stringEvent e, bool raw)
    {
        this.client = client;
        this.raw = raw;
        stream = client.GetStream();
        data = new byte[client.ReceiveBufferSize];
        cache = new DataCache(data.Length);
        RecevieEvent = e;
        stream.BeginRead(data, 0, System.Convert.ToInt32(this.client.ReceiveBufferSize), ReceiveMessage, null);
    }
    public void Send(byte[] data)
    {
        try
        {
            lock (stream)
            {
                stream.Write(data, 0, data.Length);
            }
        }
        catch
        {
            Debug.Log($"{client.Client.RemoteEndPoint}连接出现问题，强行关闭");
            TcpServer.Instance.DisconnectEvent?.Invoke(client.Client.LocalEndPoint.ToString());
            TcpServer.Instance.clientList.Remove(this);
            Close();
        }
    }
    private void ReceiveMessage(IAsyncResult ar)
    {
        int length;
        try
        {
            lock (stream)
            {
                length = stream.EndRead(ar);
            }

            if (length == 0)
            {
                TcpServer.Instance.DisconnectEvent?.Invoke(client.Client.LocalEndPoint.ToString());
                TcpServer.Instance.clientList.Remove(this);
                Debug.Log("客户端断开连接");
                return;
            }
            else
            {
               
               
                if (raw)
                {
                    // data数组中从索引0开始，长度为length的区域就是收到的数据
                    byte[] receivedData = new byte[length];
                    Array.Copy(data, 0, receivedData, 0, length);
                    var str = Encoding.UTF8.GetString(receivedData);
                    //Debug.Log($"{str}:{str.Length}");
                    RecevieEvent?.Invoke(str);
                }
                else
                {
                    cache.Append(data, length);
                    while (cache.GetValue(out var str))
                    {
                        Loom.Instance.Enqueue(() =>
                        {
                            //Debug.Log($"{str}:{str.Length}");
                            RecevieEvent?.Invoke(str);
                        });
                    }
                }

            }

            lock (stream)
            {
                stream.BeginRead(data, 0, data.Length, ReceiveMessage, null);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.Log("服务器接收消息出现错误，强制关闭该客户端连接");
            TcpServer.Instance.DisconnectEvent?.Invoke(client.Client.LocalEndPoint.ToString());
            TcpServer.Instance.clientList.Remove(this);
            Close();

        }
    }
    public void Close()
    {
        if (client != null)
        {
            if (client.Connected)
            {
                client.Client.Shutdown(SocketShutdown.Both);
            }
            Thread.Sleep(10);
            client.Close();
            client.Dispose();
            stream.Dispose();
        }
    }
}
