using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Yjj_ValueAnimationForText : MonoBehaviour
{
    [Header("是否在awake时初始化")]
    public bool initAtAwake = true;
    [Header("是否单独播放")]
    public bool alone = true;
    [Header("数据所在的文本列表")]
    public List<UnityEngine.UI.Text> valueList = new List<UnityEngine.UI.Text>();
    [Header("轮播开始时间")]
    public float waitStartTime = 3f;
    [Header("数值增长完所需要的时间")]
    public float animationTime = 2f;
    [Header("每次动画的间隔时间")]
    public float delayTime = 1f;
    //[Header("数据保留位数")]
    //public string valueCount = "f0";
    private List<float> dataList = new List<float>();
    [Header("是否改变颜色")]
    public bool changeColor = false;
    public Color targetColor = Color.yellow;
    private Color originColor;
    public bool replay = false;

    private string[] valueCountArr;
    private void Awake()
    {
        if (initAtAwake)
        {
            Init();
        }
    }
    public void Init()
    {
        valueCountArr = new string[valueList.Count];
        for (int i = 0; i < valueList.Count; i++)
        {
            float value = float.Parse(valueList[i].text);
            dataList.Add(value);
            var va = valueList[i].gameObject.AddComponent<Yjj_ValueAnimationSingle>();
            va.animationTime = animationTime;
            va.value = value;
            string valueStr = value.ToString();
            va.valueCount = GetFloatCount(valueStr);
            valueCountArr[i] = va.valueCount;
        }
        if (valueList.Count > 0)
        {
            originColor = valueList[0].color;
        }
        initAtAwake = true;
    }
    private void OnEnable()
    {
        if (initAtAwake)
        {
            RePlay();
        }
    }
    private string GetFloatCount(string value)
    {
        string result = "f";
        int index = value.IndexOf(".");
        if (index > 0)
        {
            result += (value.Length - index - 1).ToString();
        }
        else
        {
            result += "0";
        }
        return result;
    }
    public void RePlay()
    {
        StopAllCoroutines();
        if (changeColor)
        {
            foreach (var text in valueList)
            {
                text.color = originColor;
            }
        }
        if (valueList.Count > 0)
        {
            StartCoroutine(Wait());
        }
    }
    IEnumerator Wait()
    {
        yield return new WaitForSeconds(waitStartTime);
        if (alone)
        {
            StartCoroutine(Animation(0));
        }
        else
        {
            StartCoroutine(Animation());
        }
    }
    /// <summary>
    /// 单独播放数值增长动画
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    IEnumerator Animation(int index)
    {
        if (replay && index == valueList.Count)
        {
            RePlay();
        }
        index = index >= valueList.Count ? 0 : index;
        UnityEngine.UI.Text text = valueList[index];
        float value = dataList[index];
        Color oldColor = text.color;

        yield return StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
        {
            text.text = Mathf.Lerp(0, value, t).ToString(valueCountArr[index]);
            if (changeColor)
            {
                text.color = Color.Lerp(oldColor, targetColor, t);
            }
        }));
        yield return new WaitForSeconds(delayTime);
        if (changeColor)
        {
            text.color = oldColor;
        }
        index++;
        StartCoroutine(Animation(index));
    }
    /// <summary>
    /// 所有数值一起增长
    /// </summary>
    /// <returns></returns>
    IEnumerator Animation()
    {

        yield return StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
        {
            for (int i = 0; i < valueList.Count; i++)
            {
                valueList[i].text = Mathf.Lerp(0, dataList[i], t).ToString(valueCountArr[i]);
            }
        }));
        yield return new WaitForSeconds(delayTime);
        StartCoroutine(Animation());
    }
}
