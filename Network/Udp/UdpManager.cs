using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpManager : YjjSingleton<UdpManager>
{
    [InfoBox("接受和发送指定端口的UDP消息")]
    public int port = 5452;
    UdpClient client;
    Thread recevieThread;
    byte[] recevieBuffer = new byte[1024];
    IPEndPoint sendRemote;
    [Button]
    public void BeginRecevie()
    {

        if(client == null)
        {
            InitClient();
        }

        recevieThread = new Thread(Recevie);
        recevieThread.IsBackground = true;
        recevieThread.Start();
        Debug.Log($"开始接受端口:{port}的UDP消息");
    }
    private void InitClient()
    {
        sendRemote = new IPEndPoint(IPAddress.Broadcast, port);
        client = new UdpClient(port);
    }
    [Button]
    public void Send(string msg)
    {
        if(client == null)
        {
            InitClient();
        }
        var data = Encoding.UTF8.GetBytes(msg);
        client.Send(data,data.Length, sendRemote);
    }
    private void Recevie()
    {
        IPEndPoint address = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            recevieBuffer = client.Receive(ref address);
            var msg = Encoding.UTF8.GetString(recevieBuffer);
            Debug.Log($"收到{address.Address}的消息:{msg}");
        }
    }
    /// <summary>
    /// 关闭接收和发送端
    /// </summary>
    public void Close()
    {
        if (client != null)
        {
            client.Close();
            client.Dispose();
        }

        if (recevieThread != null)
        {
            recevieThread.Interrupt();
            recevieThread.Abort();
        }
    }
    private void OnDestroy()
    {
        Close();
    }
}
