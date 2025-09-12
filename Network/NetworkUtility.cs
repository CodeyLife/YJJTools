using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public static class NetworkUtility
{
    private static byte[] lengthData = new byte[4];
    /// <summary>
    /// 获取ip地址
    /// </summary>
    /// <returns></returns>
    public static string GetIP()
    {
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface adater in adapters)
        {
            if (adater.Supports(NetworkInterfaceComponent.IPv4))
            {
                UnicastIPAddressInformationCollection UniCast = adater.GetIPProperties().UnicastAddresses;
                if (UniCast.Count > 0)
                {
                    foreach (UnicastIPAddressInformation uni in UniCast)
                    {
                        if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return uni.Address.ToString();
                        }
                    }
                }
            }
        }
        return null;
    }
    /// <summary>
    /// 返回所有ip地址
    /// </summary>
    /// <returns></returns>
    public static List<string> GetAllIP()
    {
        List<string> ips = new List<string>();
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        foreach (NetworkInterface adater in adapters)
        {
            if (adater.Supports(NetworkInterfaceComponent.IPv4))
            {
                UnicastIPAddressInformationCollection UniCast = adater.GetIPProperties().UnicastAddresses;
                if (UniCast.Count > 0)
                {
                    foreach (UnicastIPAddressInformation uni in UniCast)
                    {
                        if (uni.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            var str = uni.Address.ToString();
                            if( str != "127.0.0.1")
                            {
                                ips.Add(str);
                            }
                            
                        }
                    }
                }
            }
        }
        return ips;
    }
    public static byte[] Pack(string msg)
    {
        var tempData = Encoding.UTF8.GetBytes(msg);
        var data = new byte[tempData.Length + 4];
        var lenthdata = BitConverter.GetBytes(data.Length);
        Array.Copy(lenthdata,0, data,0, lenthdata.Length);
        Array.Copy(tempData, 0, data, 4, tempData.Length);
        return data;
    }
    public static byte[] Pack(byte[] tempData)
    {
        var data = new byte[tempData.Length + 4];
        var lenthdata = BitConverter.GetBytes(data.Length);
        Array.Copy(lenthdata, 0, data, 0, 4);
        Array.Copy(tempData, 0, data, 4, tempData.Length);
        return data;
    }
}
