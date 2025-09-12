using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("_YjjTool/Roll")]
public class Roll : MonoBehaviour
{
    public enum LineType
    {
        单行,
        多行,
    }
    public bool connect = true;
    RectTransform rect;
    RectTransform maskRect;
    TextMeshProUGUI _pro;
    Text _text;
    [ReadOnly, LabelText("该脚本是否用于Text")]
    public bool isText = true;
    [Header("文本类型"), OnValueChanged("ChangeType")]
    public LineType lineType;
    [Header("滚动速度")]
    public float rollSpeed = 30;
    [Header("滚动至末尾停留时间")]
    public float endStayTime = 0;
    [Header("第一次开始滚动延迟时间")]
    public float waitFirstTime = 2;

    public RectTransform Rect
    {
        get
        {
            if (rect == null)
            {
                rect = GetComponent<RectTransform>();
            }
            return rect;
        }
        set => rect = value;
    }

    public RectTransform MaskRect { get { if (maskRect == null) { maskRect = transform.parent.rectTransform(); } return maskRect; } set => maskRect = value; }

    public TextMeshProUGUI Pro { get { if (_pro == null) { _pro = GetComponent<TextMeshProUGUI>(); } return _pro; } set => _pro = value; }

    public Text Text { get { if (_text == null) { _text = GetComponent<Text>(); } return _text; } set => _text = value; }

    #region Inspector方法
#if UNITY_EDITOR
    private void ChangeType()
    {
        var rect = transform.rectTransform();
        var parentRect = transform.parent.rectTransform();
        switch (lineType)
        {
            case LineType.单行:
                if (Text != null)
                {
                    Text.horizontalOverflow = HorizontalWrapMode.Overflow;
                    Text.verticalOverflow = VerticalWrapMode.Truncate;
                }
                if (Pro != null)
                {
                    Pro.enableWordWrapping = false;
                    Pro.overflowMode = TextOverflowModes.Overflow;
                }
                rect.sizeDelta = parentRect.sizeDelta;
                rect.anchoredPosition = Vector2.zero;
                break;
            case LineType.多行:
                if (Text != null)
                {
                    Text.horizontalOverflow = HorizontalWrapMode.Wrap;
                    Text.verticalOverflow = VerticalWrapMode.Overflow;
                }
                if (Pro != null)
                {
                    Pro.enableWordWrapping = true;
                }
                break;
        }
        SetAnchor();
    }
    private void SetAnchor()
    {
        if (lineType == LineType.单行)
        {
            if (Text != null)
            {
                if (Text.alignment == TextAnchor.MiddleRight || Text.alignment == TextAnchor.LowerRight || Text.alignment == TextAnchor.UpperRight)
                {
                    Rect.anchorMin = new Vector2(1, 0.5f);
                    Rect.anchorMax = new Vector2(1, 0.5f);
                    Rect.pivot = new Vector2(1, 0.5f);
                }
                else if (Text.alignment == TextAnchor.UpperCenter || Text.alignment == TextAnchor.MiddleCenter || Text.alignment == TextAnchor.LowerCenter)
                {
                    Vector2 v = Vector2.one * 0.5f;
                    Rect.anchorMin = v;
                    Rect.anchorMax = v;
                    Rect.pivot = v;
                }
                else
                {
                    Rect.anchorMin = new Vector2(0, 0.5f);
                    Rect.anchorMax = new Vector2(0, 0.5f);
                    Rect.pivot = new Vector2(0, 0.5f);
                }
            }
        }
        else if (lineType == LineType.多行)
        {
            Vector2 v = new Vector2(0.5f, 1);
            Rect.anchorMin = v;
            Rect.anchorMax = v;
            Rect.pivot = v;
        }
    }
    [OnInspectorInit]
    private void OnInit()
    {
        if (Text != null)
        {
            isText = true;
            Text.resizeTextForBestFit = false;
            var mask = transform.parent.GetComponent<Mask>();
            if (mask == null)
            {
                int index = transform.GetSiblingIndex();
                var go = new GameObject(Text.gameObject.name + "_Mask", new System.Type[] { typeof(Image), typeof(Mask) });
                go.GetComponent<Mask>().showMaskGraphic = false;
                go.GetComponent<Image>().raycastTarget = false;
                MaskRect = go.GetOrAddComponent<RectTransform>();
                MaskRect.SetParent(transform.parent,false);
                MaskRect.SetSiblingIndex(index);
                MaskRect.anchorMin = Rect.anchorMin;
                MaskRect.anchorMax = Rect.anchorMax;
                MaskRect.pivot = Rect.pivot;
                MaskRect.anchoredPosition = Rect.anchoredPosition;
                MaskRect.sizeDelta = Rect.sizeDelta;
                transform.SetParent(MaskRect);
                UnityEditor.Selection.activeGameObject = transform.gameObject;
            }
            ChangeType();
        }
        else
        {
            isText = false;
            Pro.enableAutoSizing = false;
            var mask = transform.parent.GetComponent<Mask>();
            if (mask == null)
            {
                int index = transform.GetSiblingIndex();
                var go = new GameObject(Pro.gameObject.name + "_Mask", new System.Type[] { typeof(Image), typeof(Mask) });
                go.GetComponent<Mask>().showMaskGraphic = false;
                go.GetComponent<Image>().raycastTarget = false;
                MaskRect = go.GetOrAddComponent<RectTransform>();
                MaskRect.SetParent(transform.parent,false);
                MaskRect.SetSiblingIndex(index);
                MaskRect.anchorMin = Rect.anchorMin;
                MaskRect.anchorMax = Rect.anchorMax;
                MaskRect.pivot = Rect.pivot;
                MaskRect.anchoredPosition = Rect.anchoredPosition;
                MaskRect.sizeDelta = Rect.sizeDelta;
                transform.SetParent(MaskRect);
                UnityEditor.Selection.activeGameObject = transform.gameObject;
            }
            ChangeType();
        }
    }
#endif
#endregion
    private void OnEnable()
    {
        InitAnimation();
    }
    /// <summary>
    /// 初始化播放动画
    /// </summary>
    public void InitAnimation()
    {
        StopAllCoroutines();
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        if (isText)
        {
            if (lineType == LineType.单行)
            {
                PlayAnimationText();
            }
            else if (lineType == LineType.多行)
            {
                PlayAnimationTextFroMultipleLine();
            }
        }
        else
        {
            PlayAnimationPro();
        }
    }
    /// <summary>
    /// 设置文本内容
    /// </summary>
    /// <param name="str"></param>
    public void SetData(string str)
    {
        if (Text != null)
        {
            Text.text = str;
        }
        else
        {
            if (Pro != null)
            {
                Pro.text = str;
            }
        }
        InitAnimation();
    }

    private void PlayAnimationTextFroMultipleLine()
    {
        // int line = CalculateMultipleLineOfText(Text, MaskRect.sizeDelta.x);
        float moveLength = Text.preferredHeight;
        if (moveLength < MaskRect.sizeDelta.y)
        {
            Rect.sizeDelta = new Vector2(Rect.sizeDelta.x, MaskRect.sizeDelta.y);
            Rect.anchoredPosition = Vector2.zero;
            return;
        }
        Rect.sizeDelta = new Vector2(Rect.sizeDelta.x, moveLength);
        float time = moveLength / rollSpeed * 3;
        Vector2 start = new Vector2(0, -MaskRect.sizeDelta.y);
        Vector2 end = new Vector2(0, moveLength);
        Vector2 first = Vector2.zero;
        StartCoroutine(TextVerticalLoop(time, start, end, first, true));
    }

    private void PlayAnimationPro()
    {
        if(lineType == LineType.单行)
        {
            if (!connect)
            {
                Pro.margin = Vector4.zero;
                float length = Pro.preferredWidth;
                float size = MaskRect.sizeDelta.x;
                //Debug.Log($"最优{length},当前：{size}");
                float moveLenth = length - size;
                if (moveLenth <= 0)
                {
                    return;
                }
                Pro.alignment = TextAlignmentOptions.MidlineLeft;
                float time = length / rollSpeed;
                StartCoroutine(ProTextLoop(time, -length));
            }
            else
            {
                float length = Pro.preferredWidth;
                float size = MaskRect.sizeDelta.x;
                float moveLenth = length - size;
                if (moveLenth <= 0)
                {
                    return;
                }
                string orign = Pro.text;
                Pro.text += "   " + Pro.text;

                length = Pro.preferredWidth - length;
        
                Pro.alignment = TextAlignmentOptions.MidlineLeft;
                float time = length / rollSpeed;
                Pro.text += orign;
                StartCoroutine(ProTextLoopConnect(time, -length));
            }
        }
        else if (lineType == LineType.多行)
        {
            Pro.margin = Vector4.zero;
            float bestH = Pro.preferredHeight;
            float moveLength = bestH - Rect.sizeDelta.y;
            if (moveLength<=0)
            {
                return;
            }
            float time = bestH / rollSpeed;
            StartCoroutine(ProTextLoopVertical(time,-bestH));
        }
    }

    private void PlayAnimationText()
    {
        float length = Text.preferredWidth;
        Rect.sizeDelta = new Vector2(length, Rect.sizeDelta.y);
        float maskSize = MaskRect.sizeDelta.x;
        float moveLenth = length;
        if (moveLenth <= maskSize)
        {
            Rect.anchoredPosition = Vector2.zero;
            return;
        }
        float time = length / rollSpeed;
        Vector2 target = new Vector2(-length, 0);
        Vector2 startPos = new Vector2(maskSize, 0);
        Vector2 firstPos = Vector2.zero;
        if (Text.alignment == TextAnchor.MiddleRight || Text.alignment == TextAnchor.LowerRight || Text.alignment == TextAnchor.UpperRight)
        {
            startPos = new Vector2(moveLenth, 0);
            target = new Vector2(-maskSize, 0);
            firstPos = new Vector2((length - maskSize), 0);
        }
        else if (Text.alignment == TextAnchor.UpperCenter || Text.alignment == TextAnchor.MiddleCenter || Text.alignment == TextAnchor.LowerCenter)
        {
            var startx = (length + maskSize) * 0.5f;
            startPos = new Vector2(startx, 0);
            target = new Vector2(-startx, 0);
            firstPos = new Vector2((length - maskSize) * 0.5f, 0);
        }

        StartCoroutine(TextLoop(time, target, startPos, firstPos, true));
    }
    IEnumerator TextLoop(float time, Vector2 target, Vector2 startPos, Vector2 first, bool isFirst)
    {
        if (isFirst)
        {
            Rect.anchoredPosition = first;
            yield return new WaitForSeconds(waitFirstTime);
        }
        Rect.anchoredPosition = startPos;
        if (isFirst)
        {
            yield return StartCoroutine(YjjUtility.FadeIn(time, (t) =>
            {
                Rect.anchoredPosition = Vector2.Lerp(first, target, t);
            }));
        }
        else
        {
            yield return StartCoroutine(YjjUtility.FadeIn(time, (t) =>
            {
                Rect.anchoredPosition = Vector2.Lerp(startPos, target, t);
            }));
        }

        yield return new WaitForSeconds(endStayTime);
        StartCoroutine(TextLoop(time, target, startPos, first, false));
    }
    IEnumerator TextVerticalLoop(float time, Vector2 start, Vector2 end, Vector2 first, bool isFirst)
    {
        if (isFirst)
        {
            Rect.anchoredPosition = first;
            yield return new WaitForSeconds(waitFirstTime);
        }
        if (isFirst)
        {
            yield return StartCoroutine(YjjUtility.FadeIn(time, (t) =>
            {
                Rect.anchoredPosition = Vector2.Lerp(first, end, t);
            }));
        }
        else
        {
            yield return StartCoroutine(YjjUtility.FadeIn(time, (t) =>
            {
                Rect.anchoredPosition = Vector2.Lerp(start, end, t);
            }));
        }
        yield return new WaitForSeconds(endStayTime);
        StartCoroutine(TextVerticalLoop(time, start, end, first, false));
    }
    IEnumerator ProTextLoop(float time, float target)
    {
        float x = Rect.sizeDelta.x;
        Pro.margin = new Vector4(x, 0, 0, 0);
        yield return StartCoroutine(YjjUtility.FadeIn(time, (t) =>
        {
            Pro.margin = new Vector4(Mathf.Lerp(x, target, t), 0, 0, 0);
        }));
        yield return new WaitForSeconds(endStayTime);
        StartCoroutine(ProTextLoop(time, target));
    }
    IEnumerator ProTextLoopConnect(float time, float target)
    {
       
        Pro.margin = new Vector4(0, 0, 0, 0);
        yield return StartCoroutine(YjjUtility.FadeIn(time, (t) =>
        {
            Pro.margin = new Vector4(Mathf.Lerp(0, target, t), 0, 0, 0);
        }));
        yield return new WaitForSeconds(endStayTime);
        StartCoroutine(ProTextLoopConnect(time, target));
    }
    IEnumerator ProTextLoopVertical(float time,float target)
    {
        Pro.margin = new Vector4(0, target, 0, 0);
        float y = Rect.sizeDelta.y;
        yield return StartCoroutine(YjjUtility.FadeIn(time, (t) =>
        {
            Pro.margin = new Vector4(0, Mathf.Lerp(y, target, t), 0, 0);
        }));
        yield return new WaitForSeconds(endStayTime);
        StartCoroutine(ProTextLoopVertical(time, target));
    }

#if UNITY_EDITOR
    void OnRectTransformDimensionsChange()
    {
        if (MaskRect != null &&!Application.isPlaying)
        {
            // 同步Mask尺寸和位置
            MaskRect.sizeDelta = Rect.sizeDelta;
            MaskRect.anchorMin = Rect.anchorMin;
            MaskRect.anchorMax = Rect.anchorMax;
            MaskRect.pivot = Rect.pivot;
            MaskRect.anchoredPosition = Rect.anchoredPosition;
        }
    }
#endif
}
