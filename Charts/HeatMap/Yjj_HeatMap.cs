using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Yjj_HeatMap : MonoBehaviour
{
    #region 参数
    public enum ComputeType
    {
        像素直接相加,
        像素加权相加
    }
    public List<Vector3> pointsList = new List<Vector3>();
    public List<float> dataList = new List<float>();

    public GameObject plane;
    [Range(0.02f, 0.99f)]
    public float damping = 0.2f;
    [LabelText("衰减随机系数")]
    [Range(0, 99)]
    public int dampingLevel = 50;
    [MinValue(1), LabelText("最小辐射范围")]
    public int minLength = 10;
    [Range(0,1),LabelText("最大值映射")]
    public float maxPercent = 0.5f;
    [LabelText("取颜色最大值进行映射")]
    public bool curveRamap = true;
    [EnumToggleButtons,HideLabel]
    public ComputeType ct = ComputeType.像素直接相加;
    [LabelText("颜色曲线")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 2);

    [FoldoutGroup("读取数据")]
    [LabelText("数据表格位置")]
    public string excelPath;
    [FoldoutGroup("从Excel读取数据")]
    [LabelText("awake时是否读取excel")]
    public bool readDataAtAwake = false;
    [FoldoutGroup("从Excel读取数据")]
    [HideIf("@string.IsNullOrEmpty(this.excelPath)")]
    [LabelText("数据所在excel位置")]
    public int dataIndex = 1;
    [FoldoutGroup("可不更改的默认设置")]
    [Title("图片最长的一边的像素")]
    public int maxPix = 128;
    //[LabelText("图片宽度")]
    //public int pixWidth = 128;
    //[FoldoutGroup("可不更改的默认设置")]
    //[LabelText("图片高度")]
    //public int pixHeight = 128;
    #endregion
    private void Awake()
    {
        Compute();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 start = transform.position - transform.right * transform.localScale.x * 5 - transform.forward * transform.localScale.z * 5;
        Vector3 end = transform.position + transform.right * transform.localScale.x * 5 + transform.forward * transform.localScale.z * 5;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(start, 2);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(end, 2);
    }
    private List<CancellationTokenSource> cancelStack = new List<CancellationTokenSource>();
    private float _max;
    public async void Compute()
    {
        try
        {
            //计时
            var watch = new Stopwatch();
            watch.Start();
       
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minZ = float.MaxValue;
            var maxZ = float.MinValue;
            for (int i = 0; i < pointsList.Count; i++)
            {
                var p = pointsList[i];
                minX = minX > p.x ? p.x : minX;
                maxX = maxX < p.x ? p.x : maxX;
                minZ = minZ > p.z ? p.z : minZ;
                maxZ = maxZ < p.z ? p.z : maxZ;
            }
            var maxDamping = 1 / damping; // 最红的地方衰减距离 
            maxDamping = Mathf.Ceil(maxDamping);

            var xLength = maxX - minX;
            var zLength = maxZ - minZ;
            xLength = Mathf.Max(1, xLength);
            zLength = Mathf.Max(1, zLength);
            //maxDamping = xLength > zLength ? xLength / maxPix * maxDamping : zLength / maxPix * maxDamping;
        
            transform.localScale = new Vector3((xLength + maxDamping * 2) * 0.1f, 1, (zLength + maxDamping * 2) * 0.1f);

            //计算每像素对应多少实际距离
            var pixLength = Mathf.Max(transform.localScale.x, transform.localScale.z) * 10 / maxPix;
            //每一个像素的衰减值 
            var pixDamping = pixLength * damping;

            transform.position = new Vector3((minX + maxX) * 0.5f, transform.position.y, (minZ + maxZ) * 0.5f);


            //计算左下角 和 右上角 的世界坐标
            Vector3 start = transform.position - transform.right * transform.localScale.x * 5 - transform.forward * transform.localScale.z * 5;
            Vector3 end = transform.position + transform.right * transform.localScale.x * 5 + transform.forward * transform.localScale.z * 5;
            //计算总的宽度和高度
            float width = Mathf.Abs(end.x - start.x);
            float height = Mathf.Abs(end.z - start.z);

            //初始化数组

            int pixWidth = 0;
            int pixHeight = 0;

            if (width < height)
            {
                pixWidth = Mathf.CeilToInt(width / height * maxPix);
                pixHeight = maxPix;
            }
            else
            {
                pixWidth = maxPix;
                pixHeight = Mathf.CeilToInt(height / width * maxPix);
            }

            float[,] colorArr = new float[pixWidth, pixHeight];

            //算数据最大值 x 最大值百分比
            _max = 100;
            if (dataList != null && dataList.Count >= pointsList.Count)
            {
                _max = dataList.Max();
            }
            _max *= maxPercent;
            var list = pointsList;
            //取消之前的任务
            foreach (var c in cancelStack)
            {
                c.Cancel();
            }
            //cancelStack.Clear();
            var source = new CancellationTokenSource();
            CancellationToken cancel = source.Token;
            //添加进取消队列
            cancelStack.Add(source);

            var task = Task.Run(() =>
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (cancel.IsCancellationRequested)
                    {
                        cancel.ThrowIfCancellationRequested();
                        break;
                    }
                    var p = list[i];
                    int index = i;
                    //float u = Mathf.Abs(p.x - start.x) / width;
                    //float v = Mathf.Abs(p.z - start.z) / hight;
                    float u = (p.x - start.x) / width;
                    float v = (p.z - start.z) / height;
                    if (u < 0 || u > 1) continue;
                    if (v < 0 || v > 1) continue;

                    int index_x = Mathf.FloorToInt(pixWidth * u);
                    int index_y = Mathf.FloorToInt(pixHeight * v);

                    float weight = dataList != null && dataList.Count > index ? dataList[index] : 100;
                    SetColor(index_x, index_y, weight, colorArr, pixWidth, pixHeight, pixDamping);
                }
                if (curveRamap)
                {
                    float m = 0.1f;
                    for (int i = 0; i < pixWidth; i++)
                    {
                        if (cancel.IsCancellationRequested)
                        {

                            cancel.ThrowIfCancellationRequested();
                            break;
                        }
                        for (int j = 0; j < pixHeight; j++)
                        {
                            float value = colorArr[i, j];
                            if (value > m) m = value;
                        }
                    }
                    m *= maxPercent;
                    for (int i = 0; i < pixWidth; i++)
                    {
                        if (cancel.IsCancellationRequested)
                        {
                            cancel.ThrowIfCancellationRequested();
                            break;
                        }
                        for (int j = 0; j < pixHeight; j++)
                        {
                            float value = colorArr[i, j];
                            value = value / m;
                            value = curve.Evaluate(value);
                            colorArr[i, j] = Mathf.Lerp(0, 1, value);
                        }
                    }
                }

            }, source.Token);
            try
            {
                await task;
                Texture2D tex = new Texture2D(pixWidth, pixHeight);
                Color[] colors = new Color[pixWidth * pixHeight];
                for (int i = 0; i < pixHeight; i++)
                {
                    for (int j = 0; j < pixWidth; j++)
                    {
                        //Debug.Log($"{j}-{i}");
                        colors[i * pixWidth + j] = new Color(colorArr[j, i], 0, 0, 0);
                    }
                }
                tex.SetPixels(colors);
                tex.Apply();
                watch.Stop();
                Debug.Log($"{watch.ElapsedMilliseconds}毫秒");
                var mat = plane.GetComponent<MeshRenderer>().sharedMaterial;
                mat.SetTexture("_MainTex", tex);
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                Debug.Log("任务取消");
            }
            finally
            {
                //Debug.Log(cancelStack.Count);
                cancelStack.Remove(source);
                source.Dispose();
            }

            gameObject.SetActive(true);
        }
        catch { }
    }

    /// <summary>
    /// 直接给Unity世界坐标
    /// </summary>
    /// <param name="data"></param>
    public void SetData(List<Vector3> data,List<float> values = null)
    {
        pointsList = data;
        dataList = values;
        //Compute();
    }
    private void SetColor(int x, int y, float weight, float[,] arr,int pixWidth, int pixHeight, float pixDamping)
    {
        //var random = new System.Random();
        //var rv = random.Next(100-dampingLevel, 100+dampingLevel);
        //颜色
        float r = Mathf.Lerp(0, 1, weight / _max);
        if (!curveRamap)
        {
            r = curve.Evaluate(r);
        }
        float length = r / pixDamping;
        int maxLenght = Mathf.CeilToInt(length);
        maxLenght = maxLenght < minLength ? minLength : maxLenght;
        int startx = 0;
        int endx = 0;
        int startY = 0;
        int endY = 0;
        startx = Mathf.Clamp(x - maxLenght, 0, pixWidth);
        endx = Mathf.Clamp(x + maxLenght, 0, pixWidth);
        startY = Mathf.Clamp(y - maxLenght, 0, pixHeight);
        endY = Mathf.Clamp(y + maxLenght, 0, pixHeight);
        for (int u = startx; u < endx; u++)
        {
            for (int v = startY; v < endY; v++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(u, v));
                var value = Mathf.Lerp(r, 0, distance / length);
                var oldvalue = arr[u, v];
                if(ct == ComputeType.像素直接相加)
                {
                    value = (oldvalue + value);
                }
                else
                {
                    value = (oldvalue + (1 - oldvalue) * value);
                }
                arr[u, v] = value;
            }
        }
    }
    #region Inspector面板
    [OnInspectorGUI]
    private void OnValueChange()
    {
        if (GUI.changed)
        {
            Compute();
        }
    }
    [OnInspectorInit]
    private void InSpectorInit()
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(YjjUtility.DeLay(() =>
        {
            Compute();
        }));
    }
    [Title("---------------------------------------------", TitleAlignment = TitleAlignments.Centered)]
    [Button("生成热力图")]
    private void GenerateHeatMap()
    {
        Compute();
    }

#if UNITY_EDITOR
    [LabelText("随机生成数据范围")]
    [FoldoutGroup("随机生成数据")]
    public Vector2 randomDataRange = new Vector2(100, 200);

    private void GenerateRandomData()
    {
        dataList.Clear();
        //for (int i = 0; i < pointsList.Count; i++)
        //{
        //    dataList.Add(Random.Range(randomDataRange.x, randomDataRange.y));
        //}
        Compute();
    }
    [FoldoutGroup("随机生成数据")]
    [LabelText("随机生成标记点数量")]
    [InfoBox("该操作会清空原数据点，生成新数据点和数据")]
    public int randomCount = 100;
    [FoldoutGroup("随机生成数据")]
    [Button("随机生成数据")]
    private void GeneratePoint()
    {

        var maxDamping = 1 / damping; // 最红的地方衰减距离 
        maxDamping = Mathf.Ceil(maxDamping) * 2;
        var x = (transform.localScale.x * 10 - maxDamping) * 0.5f;
        var z = (transform.localScale.z * 10 - maxDamping) * 0.5f;

        var s = transform.position - new Vector3(x, 0, z);
        var e = transform.position + new Vector3(x, 0, z);
        pointsList.Clear();
        for (int i = 0; i < randomCount; i++)
        {
            Vector3 pos = new Vector3(UnityEngine.Random.Range(s.x, e.x), UnityEngine.Random.Range(s.y, e.y), UnityEngine.Random.Range(s.z, e.z));
            pointsList.Add(pos);
        }
        pointsList.Add(s,e);
        GenerateRandomData();
    }

#endif
    #endregion
}