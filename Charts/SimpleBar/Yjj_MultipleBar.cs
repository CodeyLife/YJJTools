using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Yjj_MultipleBar : MonoBehaviour
{
    public List<int> max = new List<int>();
    public List<float> datas = new List<float>();
    public List<Color> colors = new List<Color>();
    public Sprite fillSprite;
    public List<TextMeshProUGUI> valueTexts = new List<TextMeshProUGUI>();
    public TextMeshProUGUI title ;
    public float animationTime = 2f;
    public float height = 20;

    #region Inspector
    [OnInspectorGUI]
    private void GUIchaneg()
    {
        if (GUI.changed)
        {
            StartCoroutine(YjjUtility.DeLay(() =>
            {
                SetGraph();
            }));
        }
    }
    #endregion
    public void SetData(List<float> data,List<int>max,string title = null)
    {
        if (title != null)
        {
            this.title.text = title;
        }
        this.max = max;
        this.datas = data;
        SetGraph();
    }
    private void SetGraph()
    {
        for(int i = 0; i < datas.Count; i++)
        {
            var bar = transform.GetOrCreatUIChild<Image>($"bar{i}", (image) =>
             {
                 var rect = image.rectTransform;
                 rect.anchorMin = new Vector2(0, 0.5f);
                 rect.anchorMax = new Vector2(1, 0.5f);
                 rect.anchoredPosition = Vector2.zero;
             });
            if (colors.Count < i+1)
            {
                colors.Add(Color.white);
            }
            bar.sprite = fillSprite;
            bar.type = Image.Type.Filled;
            bar.fillMethod = Image.FillMethod.Horizontal;
            bar.fillAmount = datas[i] / max[i];
            bar.color = colors[i];
            bar.rectTransform.sizeDelta = new Vector2(0, height);
            if (valueTexts.Count > i)
            {
                valueTexts[i].text = datas[i].ToString();
            }
        }
        if (Application.isPlaying)
        {
            PlayAnimation();
        }
    }
    private void OnEnable()
    {
        PlayAnimation();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private void PlayAnimation()
    {
        StopAllCoroutines();
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        List<float> targetFills = new List<float>();
        for(int i = 0; i < datas.Count; i++)
        {
            targetFills.Add(datas[i] / max[i]);
        }
        StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
         {
             for (int i = 0; i < datas.Count; i++)
             {
                 var image = transform.Find($"bar{i}").GetComponent<Image>();
                 image.fillMethod = Image.FillMethod.Horizontal;
                 image.fillAmount = Mathf.Lerp(0, targetFills[i], t);
             }
         }));
    }
}
