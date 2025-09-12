using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Yjj_SimpleBar : MonoBehaviour
{
    public enum FillType
    {
        Fill,
        Width,
    }
    [LabelText("背景")]
    public Sprite backGroud;
    [ LabelText("填充")]
    public Sprite fillSprite;
    [OnValueChanged("FillChange")]
    public FillType imageFillType = FillType.Fill;
    [EnumPaging]
    public Image.FillMethod fillType = Image.FillMethod.Horizontal;
    [HorizontalGroup("color")]
    public Color bgColor = Color.white;
    [HorizontalGroup("color")]
    public Color fillColor = Color.white;
    public float data = 20;
    public float maxData = 100;
    protected RectTransform _rect;
    protected bool playAnimationAtAwake = true;
    public float animationTime = 2;
    public string title = "";
    public TextMeshProUGUI valueText;
    [ShowIf("@valueText!=null"), LabelText("数据保留小数位数")]
    public int valueCount = 1;
    [LabelText("标题")]
    public TextMeshProUGUI titleText;
#if UNITY_EDITOR
    void FillChange()
    {
        ChangeRect(Fill);
    }
#endif
    private void ChangeRect(Image fillImgae)
    {
        if (imageFillType == FillType.Fill)
        {
            fillImgae.type = Image.Type.Filled;
            fillImgae.fillMethod = fillType;
            fillImgae.rectTransform.anchorMin = Vector2.zero;
            fillImgae.rectTransform.anchorMax = Vector2.one;
            fillImgae.rectTransform.sizeDelta = Vector2.zero;
            fillImgae.rectTransform.pivot = Vector2.zero;
            fillImgae.rectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
           fillImgae.type = Image.Type.Sliced;
           fillImgae.fillMethod = fillType;
           fillImgae.rectTransform.anchorMin = Vector2.zero;
           fillImgae.rectTransform.anchorMax = Vector2.up;
           fillImgae.rectTransform.pivot = Vector2.zero;
            // fillImgae.rectTransform.sizeDelta = Vector2.zero;
            fillImgae.rectTransform.anchoredPosition = Vector2.zero;
        }
    }
    protected RectTransform Rect
    {
        get
        {
            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>();
            }
            return _rect;
        }
        set => _rect = value;
    }

    protected Image Bg
    {
        get
        {
            if (_bg == null)
            {
                _bg = transform.GetOrCreatUIChild<Image>("Background", (s) =>
                {
                    s.rectTransform.anchorMin = Vector2.zero;
                    s.rectTransform.anchorMax = Vector2.one;
                    s.rectTransform.sizeDelta = Vector2.zero;
                    s.rectTransform.anchoredPosition = Vector2.zero;
                });
            }
            return _bg;
        }
        set => _bg = value;
    }

    protected Image Fill
    {
        get
        {
            if (_fill == null)
            {
                _fill = transform.GetOrCreatUIChild<Image>("Fill", (s) =>
                {
                    ChangeRect(s);
                });
            }
            //  _fill.type = Image.Type.Filled;
            return _fill;
        }
        set => _fill = value;
    }

    protected Image _bg;
    protected Image _fill;

    #region Inspector
#if UNITY_EDITOR
    [OnInspectorGUI]
    protected void OnGuiChange()
    {
        if (GUI.changed)
        {
            StartCoroutine(YjjUtility.DeLay(() =>
            {
                SetGraph();
            }));
        }
    }
#endif
    #endregion
    protected void Awake()
    {
        if (playAnimationAtAwake)
        {
            PlayAnimation();
        }
    }
    public void SetData(float data, float maxData, string title = null)
    {
        this.data = data; this.maxData = maxData; this.title = title;
        SetGraph();
        PlayAnimation();
    }
    private void OnEnable()
    {
        PlayAnimation();
    }
    /// <summary>
    /// 播放动画
    /// </summary>
    protected void PlayAnimation()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        StopAllCoroutines();
        float target = data / maxData;
        StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
        {
          
            if (imageFillType == FillType.Fill)
            {
                Fill.fillAmount = Mathf.Lerp(0, target, t);
            }
            else
            {
                Fill.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(0, target, t) * Rect.sizeDelta.x,0);
            }
            if (valueText != null)
            {
                valueText.text = (data * t).ToLimitString(valueCount);
            }
        }));
    }

    public void SetGraph()
    {
        ChangeRect(Fill);
        if (titleText != null && !string.IsNullOrEmpty(title))
        {
            titleText.text = title;
        }
        if (valueText != null)
        {
            valueText.text = data.ToLimitString(valueCount);
        }
        Bg.sprite = backGroud;
        Bg.color = bgColor;
        Fill.sprite = fillSprite;
        if(imageFillType == FillType.Fill)
        {
            Fill.fillAmount = data / maxData;
        }
        else
        {
            Fill.rectTransform.sizeDelta = new Vector2(data / maxData * Rect.sizeDelta.x,0);
        }
        Fill.color = fillColor;
    }
}
