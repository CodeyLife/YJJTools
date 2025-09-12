using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class EncryptUtility
{
    /// <summary>
    /// sha256加密 编码格式utf-8
    /// </summary>
    /// <param name="str"></param>
    /// <param name="convert66"></param>
    /// <returns></returns>
    public static async Task<string> Sha256(string str,bool convert66 = true)
    {
        string result = null;
        await Task.Run(() =>
        {
            var data = Encoding.UTF8.GetBytes(str);
            SHA256Managed managed = new SHA256Managed();
            var rdata = managed.ComputeHash(data);
            if (convert66)
            {
                result = BitConverter.ToString(rdata).Replace("-", "").ToLower();
            }
            else
            {
                result = Convert.ToBase64String(rdata);
            }
            managed.Dispose();
        });
        return result;
    }
}
