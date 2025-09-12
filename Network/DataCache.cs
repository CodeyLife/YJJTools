using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DataCache
{
    public byte[] data;
    private int position;
    private int currentLength;
    private byte[] lenthData = new byte[4];

    public DataCache(int length)
    {
        //标准是64K
        data = new byte[length * 16];
    }

    public byte[] Append(byte[] source, int length)
    {
        //检测是否越界
        var targetLength = currentLength + length;
        //总长度不够
        if (data.Length < targetLength)
        {
            var newLenth = Mathf.ClosestPowerOfTwo(targetLength);
            if (newLenth < targetLength)
            {
                newLenth *= 2;
            }
            var tempData = new byte[newLenth];
            Array.Copy(data,position, tempData,0, currentLength);
            data = tempData;
            position = 0;
        }
        //剩余缓存区不够 将数据copy到缓存开始位置
        if(data.Length - position - currentLength < length)
        {
            Array.Copy(data, position, data, 0, currentLength);
            position = 0;
        }


        Array.Copy(source, 0, data, position + currentLength, length);
        currentLength += length;
        return data;
    }

    public bool GetValue(out string msg)
    {
        msg = null;
        bool read = false;
        if (currentLength >= 4)
        {
            Array.Copy(data, position, lenthData, 0, 4);
            int length = BitConverter.ToInt32(lenthData, 0);
            if (currentLength >= length)
            {
                read = true;
                var msgData = new byte[length - 4];
                Array.Copy(data, position + 4, msgData, 0, length - 4);
                msg = Encoding.UTF8.GetString(msgData);
                position += length;
                currentLength -= length;
              //  Update();
            }
        }
        return read;
    }

}
