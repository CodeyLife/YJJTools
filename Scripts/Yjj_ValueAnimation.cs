using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[AddComponentMenu("_YjjTool/ValueAnimation")]
public class Yjj_ValueAnimation : MonoBehaviour
{
    [Header("awake初始化")]
    public bool initAtAwake = true;
    [Header("循环动画是否单独播放")]
    public bool alone = true;
    [Header("数据所在的文本列表")]
    public List<TextMeshProUGUI> valueList = new List<TextMeshProUGUI>();
    [Header("动画开始等待时间")]
    public float waitStartTime = 3f;
    [Header("数值增长时长")]
    public float animationTime = 2f;
    [Header("每次动画的间隔时间")]
    public float delayTime = 1f;
    //[Header("数据保留位数")]
    //public string valueCount = "f0";
    [ReadOnly,ShowInInspector]
    private List<float> dataList = new List<float>();
    [Header("是否改变颜色")]
    public bool changeColor = false;
    [ShowIf("changeColor")]
    public Color targetColor = Color.yellow;
    private Color originColor;
    [LabelText("循环播放")]
    public bool replay = false;


    private bool isInit = false;
    private string[] valueCountArr;
    private void Awake()
    {
        if(initAtAwake && !isInit)
        {
            Init();
        }
    }
    public void Init()
    {
        dataList.Clear();
        valueCountArr = new string[valueList.Count];
        for (int i = 0; i < valueList.Count; i++)
        {
            var textValue = valueList[i].text;
            float value = float.Parse(textValue);
            dataList.Add(value);
            valueCountArr[i] = GetFloatCount(textValue);
        }
        if (valueList.Count > 0 && !isInit)
        {
            originColor = valueList[0].color;
        }
        isInit = true;
    }
    public void ReLoadData(Action action)
    {
        StopAllCoroutines();
        action.Invoke();
        Init();
        RePlay();
    }
    private void OnEnable()
    {
        if (isInit)
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
            result += (value.Length - index-1).ToString();
        }
        else
        {
            result += "0";
        }
        return result;
    }
    public void RePlay()
    {
        if (!this.gameObject.activeInHierarchy)
        {
            return;
        }
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
        yield return StartCoroutine(AllAdd2Max());
        yield return new WaitForSeconds(delayTime);
        if (replay)
        {
            if (alone)
            {
                StartCoroutine(Animation(0));
            }
            else
            {
                StartCoroutine(Animation());
            }
        }
    }
    /// <summary>
    /// 单独播放数值增长动画
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    IEnumerator Animation(int index)
    {
        if (index == valueList.Count)
        {
            if (replay)
            {
                index = 0;
            }
            else
            {
                yield break;
            }
           
        }
  
     //   index = index >= valueList.Count ? 0 : index;
        TextMeshProUGUI text = valueList[index];
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
            this.FadeIn((animationTime * 0.2f), (t) =>
               {
                   text.color = Color.Lerp(targetColor,oldColor,t);
               });
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
        yield return StartCoroutine(AllAdd2Max());
        if (replay)
        {
            yield return new WaitForSeconds(delayTime);
            StartCoroutine(Animation());
        }
    }
    //所有数值从0增长到max
    IEnumerator AllAdd2Max()
    {
        yield return StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
        {
            for (int i = 0; i < valueList.Count; i++)
            {
                valueList[i].text = Mathf.Lerp(0, dataList[i], t).ToString(valueCountArr[i]);
            }
        }));
    }
}
