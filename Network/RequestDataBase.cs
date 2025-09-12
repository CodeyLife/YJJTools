using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


public class RequestDataBase : MonoBehaviour
{
    public static string baseUrl = "https://api.cqdxtkj.com";
    // public static string baseUrl = "http://10.130.210.90:8015";
    public static bool record = false;
    private static RequestDataBase instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    public static string GetUrl(string api)
    {
        return baseUrl + api;
    }

    public static string recordPath = "Record";

    public static RequestDataBase Instance { get
        {
            if(instance == null)
            {
                instance = GameObject.FindObjectOfType<RequestDataBase>();
                if(instance == null)
                {
                    instance = new GameObject("RequestDataBase").AddComponent<RequestDataBase>();
                    GameObject.DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        }
        set => instance = value; }

    public enum ContentType
    {
        json,
    }
    public static IEnumerator GetTexture(string url, Action<Texture2D> action)
    {
        using UnityWebRequest r = new UnityWebRequest(url);
        var texHandle = new DownloadHandlerTexture();
        r.downloadHandler = texHandle;
        yield return r.SendWebRequest();
        try
        {
            action?.Invoke(texHandle.texture);
        }
        catch (System.Exception e)
        {
            Debug.LogError(url);
            Debug.LogException(e);
        }
    }
    public static void InstanceGet(string url, Action<string> action, Action failedAction = null)
    {
        Instance.StartCoroutine(Get(url, action, failedAction));
    }
    #region 数据缓存及网络链接失败时读取缓存
    private static void RecordWrite(string url, string value)
    {
        if (!record) return;

        Task.Run(() =>
        {
            url = MD5Encript(url) + ".txt";

            var path = Path.Combine(Application.streamingAssetsPath, recordPath, url);
            File.WriteAllText(path, value);
        });
    }
    private static async void RecordRead<T>(string url, Action<T> action)
    {
        string str = null;
        await Task.Run(() =>
        {
            string path = MD5Encript(url) + ".txt";
            path = Path.Combine(Application.streamingAssetsPath, recordPath, url);
            if (File.Exists(path))
            {
                str = File.ReadAllText(path);
            }
        });
        if (str != null)
        {
            var t = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
            action?.Invoke(t);
        }
    }
    private static async void RecordRead(string url, Action<string> action)
    {
        string str = null;
        await Task.Run(() =>
        {
            string path = MD5Encript(url) + ".txt";
            path = Path.Combine(Application.streamingAssetsPath, recordPath, path);
            if (File.Exists(path))
            {
                str = File.ReadAllText(path);
            }
            else
            {
                Debug.Log($"没有找到{url}的缓存");
            }
        });
        if (str != null)
        {
            action?.Invoke(str);
        }
    }
    #endregion
    public static IEnumerator Get(string url, Action<string> action,Action failedAction = null)
    {
        using UnityWebRequest r = UnityWebRequest.Get(url);
        yield return r.SendWebRequest();

        if (r.result != UnityWebRequest.Result.Success)
        {
            if (record)
            {
                RecordRead(url, action);
            }
            Debug.LogError($"{action.Target} {action.Method}\n{url}\n{r.error}");
            failedAction?.Invoke();
        }
        else
        {
            var str = r.downloadHandler.text;
            RecordWrite(url, str);
            try
            {
                action?.Invoke(str);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{url}\n{str}");
                Debug.LogException(e);
            }
        }
    }
    public static IEnumerator Get<T>(string url, Action<T> action,int repeat = 10)
    {
        using UnityWebRequest r = UnityWebRequest.Get(url);
        yield return r.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
        if (r.result != UnityWebRequest.Result.Success)
#else
        if (r.isHttpError || r.isNetworkError)
#endif
        {
            if (record)
            {
                RecordRead(url, action);
            }
            Debug.LogError($"{action.Target} {action.Method}\n{url}\n{r.error}");
            repeat--;
            if (repeat > 0)
            {
                Instance.StartCoroutine(Get<T>(url, action, repeat));
            }
        }
        else
        {
            var str = r.downloadHandler.text;
            RecordWrite(url, str);
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(str);
                action?.Invoke(obj);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{url}\n{str}");
                Debug.LogException(e);
            }
        }
    }
    public static IEnumerator Get<T>(string url, Action<T> action, bool debug)
    {
        using UnityWebRequest r = UnityWebRequest.Get(url);
        yield return r.SendWebRequest();
        if (r.result != UnityWebRequest.Result.Success)
        {
            if (record)
            {
                RecordRead(url, action);
            }
            Debug.LogError($"{action.Target} {action.Method}\n{url}\n{r.error}");
        }
        else
        {
            var str = r.downloadHandler.text;
            if (debug)
            {
                Debug.Log(url);
                Debug.Log(str);
            }
            RecordWrite(url, str);
            try
            {
                var obj = JsonConvert.DeserializeObject<T>(str);
                action?.Invoke(obj);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{url}\n{str}");
                Debug.LogException(e);
            }
        }
    }
    public static IEnumerator Post(string url, string postMsg = null, WWWForm form = null, List<string> headerKey = null, List<string> headerValue = null, ContentType type = ContentType.json, Action<string> action = null,int repeat = 1,Action failedAction = null)
    {
        UnityWebRequest r;
        if (form != null)
        {
            r = UnityWebRequest.Post(url, form);
        }
        else
        {
            postMsg = postMsg == null ? "{}" : postMsg;
            r = new UnityWebRequest(url, "POST");
            r.uploadHandler?.Dispose();
            var postDataBytes = Encoding.UTF8.GetBytes(postMsg);
            r.uploadHandler = new UploadHandlerRaw(postDataBytes);
           
        }
        switch (type)
        {
            case ContentType.json:
                r.SetRequestHeader("Content-Type", "application/json");
                break;
        }
        if (headerKey != null)
        {
            for (int i = 0; i < headerKey.Count; i++)
            {
                r.SetRequestHeader(headerKey[i], headerValue[i]);
            }
        }
        r.downloadHandler?.Dispose();
        r.downloadHandler = new DownloadHandlerBuffer();

        r.disposeDownloadHandlerOnDispose = true;
        r.disposeUploadHandlerOnDispose = true;
        yield return r.SendWebRequest();

        if (r.result != UnityWebRequest.Result.Success)
        {
            if (record)
            {
                RecordRead(url, action);
            }
            Debug.Log($"{url}\n{r.error}");
            repeat--;
            if (repeat > 0)
            {
                Instance.StartCoroutine(Post(url, postMsg, form, headerKey, headerValue, type, action, repeat));
            }
            else
            {
                failedAction?.Invoke();
            }
        }
        else
        {
            var str = r.downloadHandler.text;
            RecordWrite(url, str);
            try
            {
                action?.Invoke(str);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{url}\n{str}");
                Debug.LogError($"<color=green>{action.Target} {action.Method}</color>");
                Debug.LogException(e);
            }
        }
        r.Dispose();
    }
    public static IEnumerator Post<T>(string url, string postMsg = null, WWWForm form = null,bool debug = false,
                                      List<string> headerKey = null, List<string> headerValue = null, ContentType type = ContentType.json, Action<T> action = null,int repeat = 2)
    {
        UnityWebRequest r;
        byte[] postDataBytes = null;
        if (form != null)
        {
            r = UnityWebRequest.Post(url, form);
        }
        else
        {
            postMsg = postMsg == null ? "{}" : postMsg;
            r = new UnityWebRequest(url, "POST");
            r.uploadHandler?.Dispose();
            postDataBytes = Encoding.UTF8.GetBytes(postMsg);
            r.uploadHandler = new UploadHandlerRaw(postDataBytes);

        }
        switch (type)
        {
            case ContentType.json:
                r.SetRequestHeader("Content-Type", "application/json");
                break;
        }
        if (headerKey != null)
        {
            for (int i = 0; i < headerKey.Count; i++)
            {
                r.SetRequestHeader(headerKey[i], headerValue[i]);
            }
        }
        r.downloadHandler?.Dispose();
        r.downloadHandler = new DownloadHandlerBuffer();
        r.disposeUploadHandlerOnDispose = true;
        r.disposeDownloadHandlerOnDispose = true;
        r.disposeCertificateHandlerOnDispose = true;
        yield return r.SendWebRequest();
#if UNITY_2020_1_OR_NEWER
        if (r.result != UnityWebRequest.Result.Success)
#else
        if (r.isHttpError || r.isNetworkError)
#endif
        {
            if (record)
            {
                RecordRead(url, action);
            }
            Debug.Log($"{url}\n{r.error}");
            repeat--;
            if (repeat > 0)
            {
                Instance.StartCoroutine(Post<T>(url, postMsg, form, debug, headerKey, headerValue, type, action, repeat));
            }
        
        }
        else
        {
            var str = r.downloadHandler.text;
            if (debug)
            {
                Debug.Log($"<color=green>{action.Target} {action.Method}</color>");
            }
            RecordWrite(url, str);
            try
            {
                var data = JsonConvert.DeserializeObject<T>(str);
                action?.Invoke(data);
            }
            catch (System.Exception e)
            {

#if UNITY_EDITOR
                YJJTool.NetworkDebuger.UpdateError(new YJJTool.NetworkDebuger.NetworkError(action.Target, action.Method.Name, str, e));
#else
                Debug.LogError($"{url}\n{str}");
                Debug.Log($"<color=green>{action.Target} {action.Method}</color>");
                Debug.LogException(e);
#endif
            }
        }
        r.Dispose();
    }

    public static IEnumerator Post(UnityWebRequest r, Action<string> action = null, Action failedAction = null)
    {
        yield return r.SendWebRequest();

        if (r.result != UnityWebRequest.Result.Success)
        {
            if (record)
            {
                RecordRead(r.url, action);
            }
            Debug.Log($"{r.url}\n{r.error}");
        }
        else
        {
            var str = r.downloadHandler.text;
            RecordWrite(r.url, str);
            try
            {
                action?.Invoke(str);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{r.url}\n{str}");
                Debug.LogError($"<color=green>{action.Target} {action.Method}</color>");
                Debug.LogException(e);
            }
        }
        r.Dispose();
    }

    public static UnityWebRequest GetPostRequset(string url,string postMsg = null, WWWForm form = null, List<string> headerKey = null, List<string> headerValue = null, ContentType type = ContentType.json)
    {
        UnityWebRequest r;
        if (form != null)
        {
            r = UnityWebRequest.Post(url, form);
        }
        else
        {
            postMsg = postMsg == null ? "{}" : postMsg;
            r = new UnityWebRequest(url, "POST");
            r.uploadHandler?.Dispose();
            var postDataBytes = Encoding.UTF8.GetBytes(postMsg);
            r.uploadHandler = new UploadHandlerRaw(postDataBytes);

        }
        switch (type)
        {
            case ContentType.json:
                r.SetRequestHeader("Content-Type", "application/json");
                break;
        }
        if (headerKey != null)
        {
            for (int i = 0; i < headerKey.Count; i++)
            {
                r.SetRequestHeader(headerKey[i], headerValue[i]);
            }
        }
        return r;
    }
    public static float GetValueFromPercent(string percent, bool returnDecimal = true)
    {
        percent = percent.Replace("%", "");
        if (float.TryParse(percent, out var value))
        {
            if (returnDecimal)
            {
                return value / 100;
            }
            else
            {
                return value;
            }
        }
        else
        {
            Debug.LogError($"{percent},转化为小数失败");
            return 0;
        }
    }
    ///
    /// 获取字符串中的数字
    ///
    /// 字符串
    /// 数字
    public static float GetNumber(string str)
    {
        float result = 0;
        if (str != null && str != string.Empty)
        {
            // 正则表达式剔除非数字字符（不包含小数点.）
            str = Regex.Replace(str, @"[^\d.\d]", "");
            // 如果是数字，则转换为decimal类型
            if (Regex.IsMatch(str, @"^[+-]?\d+[.]?\d*$"))
            {
                result = float.Parse(str);
            }
        }
        return result;
    }

    /// <summary>
    /// 把时间戳转为时间
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <param name="ten">是10位时间戳吗</param>
    /// <returns></returns>
    public static DateTime TimeStamp2DateTime(long timeStamp)
    {
        if(timeStamp.ToString().Length == 10)
        {
            var date = DateTimeOffset.FromUnixTimeSeconds(timeStamp).DateTime;
            return date.ToLocalTime();
        }
        else
        {
            var date = DateTimeOffset.FromUnixTimeMilliseconds(timeStamp).DateTime;
            return date.ToLocalTime();
        }
        //var date = new DateTime(1970, 1, 1);
        //if (second)
        //{
        //    date.AddSeconds(timeStamp);
        //}
        //else
        //{
        //    date.AddMilliseconds(timeStamp);
        //}
        //return date;
    }
    private static string MD5Encript(string input)
    {
        using (var m = MD5.Create())
        {
            var buffer = Encoding.UTF8.GetBytes(input);
            var newBuffer = m.ComputeHash(buffer);
            var sb = new StringBuilder();
            foreach (var b in newBuffer)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    public static void InstancePost(string url, string postMsg = null, WWWForm form = null, List<string> headerKey = null, List<string> headerValue = null, ContentType type = ContentType.json, Action<string> action = null, int repeat = 1, Action failedAction = null)
    {
        Instance.StartCoroutine(Post(url, postMsg, form, headerKey, headerValue, type, action, repeat, failedAction));
    }
    public static void ResumeDownload(string url, string localPath, Action<bool> OnComplete)
    {
        var load = new ResumeDownloader();
        load.StartDownload(url, localPath, OnComplete);
    }
}
public class ResumeDownloader
{
    // 下载保存路径
    private string _savePath;
    // 已下载的字节数
    private long _downloadedBytes;
    // 文件总大小
    private long _totalBytes;
    // 当前下载请求
    private UnityWebRequest _webRequest;
    // 下载是否正在进行
    private bool _isDownloading;


    // 下载完成回调
    public Action<bool> OnDownloadComplete;
    /// <summary>
    /// 开始或继续下载
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="localPath">本地保存路径</param>
    public void StartDownload(string url, string localPath, Action<bool> OnDownloadComplete)
    {
        if (_isDownloading)
        {
            Debug.LogWarning("正在下载中，请勿重复开始");
            return;
        }
        this.OnDownloadComplete = OnDownloadComplete;
        _savePath = localPath;
        _downloadedBytes = 0;

        // 检查是否已有部分下载的文件
        if (File.Exists(_savePath))
        {
            FileInfo fileInfo = new FileInfo(_savePath);
            _downloadedBytes = fileInfo.Length;
            Debug.Log($"发现已下载文件，大小: {_downloadedBytes} 字节，将从断点继续下载");
        }

        RequestDataBase.Instance.StartCoroutine(DownloadFile(url));
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public void PauseDownload()
    {
        if (_webRequest != null && !_webRequest.isDone)
        {
            _webRequest.Abort();
            _isDownloading = false;
            Debug.Log($"下载已暂停，已下载: {_downloadedBytes} 字节");
        }
    }

    private System.Collections.IEnumerator DownloadFile(string url)
    {
        _isDownloading = true;

        // 创建请求
        _webRequest = UnityWebRequest.Get(url);

        // 设置Range头，从已下载的位置继续下载
        if (_downloadedBytes > 0)
        {
            _webRequest.SetRequestHeader("Range", $"bytes={_downloadedBytes}-");
        }

        // 发送请求
        yield return _webRequest.SendWebRequest();

        // 处理错误
        if (_webRequest.result == UnityWebRequest.Result.ConnectionError ||
            _webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"下载错误: {_webRequest.error}");
            _isDownloading = false;
            yield break;
        }

        // 获取文件总大小
        if (_totalBytes == 0)
        {
            if (long.TryParse(_webRequest.GetResponseHeader("Content-Length"), out long contentLength))
            {
                // 如果是断点续传，Content-Length是剩余大小，需要加上已下载的大小
                _totalBytes = _downloadedBytes > 0 ? _downloadedBytes + contentLength : contentLength;
            }
        }

        try
        {
            // 获取下载的数据
            byte[] data = _webRequest.downloadHandler.data;

            // 将数据追加到文件
            using (FileStream fs = new FileStream(_savePath, FileMode.Append))
            {
                fs.Write(data, 0, data.Length);
                _downloadedBytes += data.Length;
            }

            Debug.Log($"下载完成，文件大小: {_downloadedBytes} 字节");

            // 检查是否下载完成
            if (_downloadedBytes >= _totalBytes)
            {
                Debug.Log("文件下载完成！");
                OnDownloadComplete?.Invoke(true);
            }
            else
            {
                Debug.LogWarning("文件未完全下载，可能需要继续");
                OnDownloadComplete?.Invoke(false);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"保存文件错误: {e.Message}");
            OnDownloadComplete?.Invoke(false);
        }

        _isDownloading = false;
    }

}

 public class FtpResumeDownloader
{
    // 下载状态
    public enum DownloadState { Idle, Downloading, Paused, Completed, Error }
    public DownloadState CurrentState { get; private set; } = DownloadState.Idle;

    // 已下载字节数
    public long DownloadedBytes { get; private set; }
    // 文件总大小
    public long TotalBytes { get; private set; }
    // 下载进度(0-1)
    public float Progress => TotalBytes > 0 ? (float)DownloadedBytes / TotalBytes : 0;

    // FTP请求对象
    private FtpWebRequest _ftpRequest;
    // 网络响应
    private FtpWebResponse _ftpResponse;
    // 流对象
    private Stream _responseStream;


    // 进度更新回调
    public Action<float> OnProgressUpdated;
    // 下载完成回调
    public Action<bool> OnDownloadCompleted;

    // 取消标识
    private bool _isCancelled;

    public FtpResumeDownloader(string ftpUrl, string localPath, string username, string password, Action<float> OnProcessUpdate, Action<bool> OnDownLoadCompleted)
    {
        this.OnDownloadCompleted = OnDownLoadCompleted;
        this.OnProgressUpdated = OnProcessUpdate;
        _ = StartDownload(ftpUrl, localPath, username, password);
    }

    /// <summary>
    /// 开始或继续FTP下载
    /// </summary>
    /// <param name="ftpUrl">FTP文件地址 (格式: ftp://server/path/file.ext)</param>
    /// <param name="localPath">本地保存路径</param>
    /// <param name="username">FTP用户名</param>
    /// <param name="password">FTP密码</param>
    public async Task StartDownload(string ftpUrl, string localPath, string username, string password)
    {
        if (CurrentState == DownloadState.Downloading)
        {
            Debug.LogWarning("正在下载中，请勿重复操作");
            return;
        }

        CurrentState = DownloadState.Downloading;
        _isCancelled = false;

        try
        {
            // 检查本地文件
            DownloadedBytes = 0;
            
            if (File.Exists(localPath))
            {
                var fileInfo = new FileInfo(localPath);
                DownloadedBytes = fileInfo.Length;
                Debug.Log($"发现已下载文件，大小: {DownloadedBytes} 字节，将从断点继续");
            }

            // 获取文件总大小
            TotalBytes = await GetFileSize(ftpUrl, username, password);
            if (TotalBytes <= 0)
            {
                Debug.LogError("无法获取文件大小");
                CurrentState = DownloadState.Error;
                return;
            }

            // 如果已下载大小等于总大小，说明已经下载完成
            if (DownloadedBytes >= TotalBytes)
            {
                Debug.Log("文件已完全下载");
                CurrentState = DownloadState.Completed;
                return;
            }

            // 创建FTP请求
            _ftpRequest = (FtpWebRequest)WebRequest.Create(ftpUrl);
            _ftpRequest.Credentials = new NetworkCredential(username, password);
            _ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            // 设置断点续传位置
            if (DownloadedBytes > 0)
            {
                _ftpRequest.ContentOffset = DownloadedBytes;
            }

            // 获取响应
            _ftpResponse = (FtpWebResponse)await _ftpRequest.GetResponseAsync();

            // 获取响应流
            _responseStream = _ftpResponse.GetResponseStream();

            // 打开文件流(追加模式)

            var  _fileStream = new FileStream(localPath, FileMode.Append, FileAccess.Write, FileShare.None);

            // 缓冲区
            byte[] buffer = new byte[1024*1024];
            int bytesRead;

            // 读取数据并写入文件
            while ((bytesRead = await _responseStream.ReadAsync(buffer, 0, buffer.Length)) > 0 && !_isCancelled)
            {
                await _fileStream.WriteAsync(buffer, 0, bytesRead);
                DownloadedBytes += bytesRead;

                // 可以在这里添加进度回调
                OnProgressUpdated?.Invoke(Progress);
            }

            // 清理资源
            await _fileStream.FlushAsync();
            _fileStream.Dispose();
            _responseStream.Close();
            _ftpResponse.Close();

            // 检查下载状态
            if (_isCancelled)
            {
                CurrentState = DownloadState.Paused;
                Debug.Log($"下载已暂停，已下载: {DownloadedBytes}/{TotalBytes} 字节");
            }
            else if (DownloadedBytes >= TotalBytes)
            {
                CurrentState = DownloadState.Completed;
                Debug.Log("文件下载完成");
                OnDownloadCompleted?.Invoke(true);
            }
            else
            {
                CurrentState = DownloadState.Error;
                Debug.LogError("下载意外终止");
                OnDownloadCompleted?.Invoke(false);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"下载错误: {ex.Message}");
            CurrentState = DownloadState.Error;
            OnDownloadCompleted?.Invoke(false);
        }
    }

    /// <summary>
    /// 暂停下载
    /// </summary>
    public void PauseDownload()
    {
        if (CurrentState == DownloadState.Downloading)
        {
            _isCancelled = true;
        }
    }

    /// <summary>
    /// 获取FTP文件大小
    /// </summary>
    private async Task<long> GetFileSize(string ftpUrl, string username, string password)
    {
        try
        {
            var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.GetFileSize;

            using (var response = (FtpWebResponse)await request.GetResponseAsync())
            {
                return response.ContentLength;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"获取文件大小错误:{ftpUrl}  {ex.Message}");
            return 0;
        }
    }

    //public void Dispose()
    //{
    //    if (CurrentState == DownloadState.Downloading)
    //    {
    //        PauseDownload();
    //    }
    //    _responseStream?.Dispose();
    //    _ftpResponse?.Close();
    //    _ftpRequest?.Abort();
    //}
}

