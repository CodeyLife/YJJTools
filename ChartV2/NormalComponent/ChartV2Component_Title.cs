using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ComponentDesc("数据文本")]
public class ChartV2Component_Title : ChartV2ComponetBaseWithoutGraphic
{
    public float distanceFromAxis = 10;
    public float fontSize = 24;
    public Color fontColor = Color.gray;
    public TMP_FontAsset font;
    public override void InitGraph(ChartV2Base chart)
    {
        base.InitGraph(chart);
 
#if UNITY_EDITOR
        if(font == null)
        {
            font = YjjConfigs.Instance.tmpFont;
        }
#endif
       
    
        SetGraph();
    }

    private void OnDrag(float arg0)
    {
        SetGraph();
    }

    public override void SetGraph()
    {
        base.SetGraph();
        if (_v2Base.ComputeDataPos(false))
        {
            _v2Base.OnDragEvent.RemoveListener(OnDrag);
            _v2Base.OnDragEvent.AddListener(OnDrag);
        }
        else
        {
            _v2Base.OnDragEvent.RemoveListener(OnDrag);
        }
        transform.DelateAllChild();
        GenerateTitle();
    }

    protected void GenerateTitle()
    {
        if (_v2Base == null) return;
        int start = 0;
        int end = 0;
        _v2Base.GetDragDataIndex(ref start, ref end,false);
        for (int i = start; i < end; i++)
        {
            if (i >= _v2Base.names.Count)
            {
                end = _v2Base.names.Count;
                break;
            }
            var x = _v2Base.XList[i] - _v2Base.XOffset;
            var text = transform.GetOrCreatUIChild<TextMeshProUGUI>((i - start).ToString(), (t =>
               {
                   t.rectTransform.sizeDelta = new Vector2(200, 50);
                   t.rectTransform.anchorMin = Vector2.zero;
                   t.rectTransform.anchorMax = Vector2.zero;
                   t.rectTransform.pivot = new Vector2(0.5f, 1);
                   t.alignment = TextAlignmentOptions.Center;
                   t.material = font.material;
                   t.raycastTarget = false;
                   t.OnPreRenderText += PreRender;

               }));
            text.color = fontColor;
            text.fontSize = fontSize;
            if (font != null)
            {
                text.font = font;
            }
            text.text = _v2Base.names[i];
            text.rectTransform.anchoredPosition = new Vector2(x, -distanceFromAxis);
            if (text.transform.childCount > 0)
            {
                var sub = text.transform.GetChild(0).GetComponent<TMP_SubMeshUI>();
                sub.maskable = false;
            }
        }
        while (transform.childCount>end- start)
        {
            DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
        }
    }
    void PreRender(TMP_TextInfo info)
    {
        if (info.textComponent.transform.childCount > 0)
        {
            var sub = info.textComponent.transform.GetChild(0).GetComponent<TMP_SubMeshUI>();
            sub.maskable = false;
        }
      
#if UNITY_EDITOR

#endif
        info.textComponent.OnPreRenderText -= PreRender;
    }
}
