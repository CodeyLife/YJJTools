using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class Yjj_ValueAnimationSingle : MonoBehaviour
{
    public float animationTime = 2f;
    [Header("数据保留位数")]
    public string valueCount = "f0";
    public float value = 0;

    private void OnEnable()
    {
        Play();
    }
    public void Play()
    {
        StopAllCoroutines();
        if (this.gameObject.activeInHierarchy)
        {
            StartCoroutine(Animation());
        }
    }
    IEnumerator Animation()
    {
        var text = GetComponent<TextMeshProUGUI>();
        if (text != null)
        {
            if (value == 0)
            {
                value = float.Parse(text.text);
            }

            yield return StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
             {
                 text.text = Mathf.Lerp(0, value, t).ToString(valueCount);
             }));

        }
        else
        {
            UnityEngine.UI.Text nt = GetComponent<UnityEngine.UI.Text>();
            if (nt != null)
            {
                if (value == 0)
                {
                    value = float.Parse(nt.text);
                }

              yield return  StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
                {
                    nt.text = Mathf.Lerp(0, value, t).ToString(valueCount);
                }));

            }
        }
    }
    public void SetData(float data)
    {
        value = data;
        ComputeCount();
        Play();
    }
    public void SetData()
    {
        var text = GetComponent<TextMeshProUGUI>();
        value = float.Parse(text.text);
        ComputeCount();
        Play();
    }
    private void ComputeCount()
    {
        var str = value.ToString();
        var index = str.IndexOf('.');
        if (index > 0)
        {
            valueCount = $"f{str.Length - index-1}";
        }
        else
        {
            valueCount = "f0";
        }
    }

}
