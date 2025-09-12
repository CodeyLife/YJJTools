using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ComponentOrder(2000)]
[ComponentDesc("数据标注")]
public class ChartV2Componet_DataText : ChartV2ComponetBaseWithoutGraphic
{
    public Sprite icon;
    public bool useIconSize = true;
    [HideIf("useIconSize")]
    public Vector2 iconSize = new Vector2(20, 20);

    public bool showDataText = true;
    [ShowIf("showDataText")]
    public TMP_FontAsset titleFont;
    [ShowIf("showDataText")]
    public float fontSize = 12;
    [ShowIf("showDataText")]
    public Color color = Color.white;
    [ShowIf("showDataText")]
    public Vector2 offset = new Vector2(0, 20);


    private Transform iconRoot;
    private Transform titleRoot;

#if UNITY_EDITOR
    public override void OnCreat()
    {
        base.OnCreat();
        titleFont = YjjConfigs.Instance.tmpFont;
    }
#endif
    public override void InitGraph(ChartV2Base chart)
    {
        base.InitGraph(chart);
        if (Application.isPlaying)
        {
            _v2Base.InitAnimationEvent.AddListener(PlayAnimation);
        }
        iconRoot = transform.GetOrCreatUIChild<RectTransform>("Icons", (t) =>
        {
            _v2Base.InitLocalRect(t.rectTransform());
        });
        titleRoot = transform.GetOrCreatUIChild<RectTransform>("Titles", (t) =>
        {
            _v2Base.InitLocalRect(t.rectTransform());
        });
        _v2Base.ComputeDataPos(false);
        if (_v2Base.CanDrag)
        {
            _v2Base.OnDragEvent.AddListener(OnDrag);
        }

        SetGraph();
    }

    private void OnDrag(float arg0)
    {
        UpdateTitle(arg0 + _v2Base.width);
    }

    public override void SetGraph()
    {
        base.SetGraph();
        UpdateTitle(_v2Base.width);
    }
    private void PlayAnimation(float arg0)
    {
        UpdateTitle(arg0 * _v2Base.width);
    }

    private void UpdateTitle(float endWidth)
    {
        int start = 0;
        int end = 0;
        _v2Base.GetDragDataIndex(ref start, ref end, false);
        if (icon == null)
        {
            iconRoot.DelateAllChild();
        }
        if (!showDataText)
        {
            titleRoot.DelateAllChild();
        }
        int childIndex = 0;
        for (int i = start; i < end; i++)
        {
            for (int j = 0; j < _v2Base.DataList.Count; j++)
            {
                var datalist = _v2Base.DataList[j];
                if (datalist.Count <= i) break;
                var pos = datalist[i];
                var x = pos.x - _v2Base.XOffset;
                if (x - 0.1f <= endWidth)
                {
                    string name = $"{j}-{i}";
                    var targetPos = new Vector2(x, pos.y);
                    if (icon != null)
                    {
                        var temp = iconRoot.GetOrCreatUIChild<Image>(childIndex, name, (go) =>
                        {
                            go.rectTransform.anchorMin = Vector2.zero;
                            go.rectTransform.anchorMax = Vector2.zero;
                        }, typeof(RectTransform));
                        var image = temp.GetComponent<Image>();
                        image.sprite = icon;
                        if (useIconSize)
                        {
                            temp.GetComponent<Image>().SetNativeSize();
                        }
                        else
                        {
                            image.rectTransform.sizeDelta = iconSize;
                        }
                        image.rectTransform.anchoredPosition = targetPos;

                    }
                    if (showDataText)
                    {
                        var textGO = titleRoot.GetOrCreatUIChild<TextMeshProUGUI>(childIndex, name, (go) =>
                        {
                            go.rectTransform.anchorMin = Vector2.zero;
                            go.rectTransform.anchorMax = Vector2.zero;
                            go.alignment = TextAlignmentOptions.Bottom;
                        }, typeof(RectTransform));
                        var text = textGO.GetComponent<TextMeshProUGUI>();
                        text.font = titleFont;
                        text.color = color;
                        text.fontSize = fontSize;
                        text.text = _v2Base.datas[j].datas[i].ToAutoLimitString(2);
                        text.rectTransform.anchoredPosition = targetPos + offset;
                    }
                    childIndex++;
                }
                else
                {
                    break;
                }
            }
        }
        if (Application.isPlaying)
        {
            for (int i = childIndex; i < iconRoot.childCount; i++)
            {
                Destroy(iconRoot.GetChild(i).gameObject);
            }
            for (int i = childIndex; i < titleRoot.childCount; i++)
            {
                Destroy(titleRoot.GetChild(i).gameObject);
            }
        }
        else
        {
            while (iconRoot.childCount > childIndex)
            {
                DestroyImmediate(iconRoot.GetChild(iconRoot.childCount - 1).gameObject);
            }
            while (titleRoot.childCount > childIndex)
            {
                DestroyImmediate(titleRoot.GetChild(titleRoot.childCount - 1).gameObject);
            }
        }
    }

}
