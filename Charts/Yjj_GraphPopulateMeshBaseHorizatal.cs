using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

public class Yjj_GraphPopulateMeshBaseHorizatal : Yjj_GraphPopulateMeshBase
{
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        //水平轴
        Yjj_ChartUtility.DrawLine(vh, Vector2.zero, new Vector2(set.width, 0), set.lineWidth, set.lineColor);
        if (set.verticalSprite == null)
        {
            //垂直轴
            for (int i = 0; i < set.rulerSet.Count; i++)
            {
                switch (set.rulerSet[i].pos)
                {
                    case RulerPos.Left:
                        Yjj_ChartUtility.DrawLine(vh, Vector2.zero, new Vector2(0, set.hight), set.verticalLineWidth, set.verticalColor);
                        break;
                    case RulerPos.Right:
                        Yjj_ChartUtility.DrawLine(vh, Vector2.zero, new Vector2(0, set.hight), set.verticalLineWidth, set.verticalColor);
                        break;
                }

            }
        }



        //画垂直刻度
        var length = set.width - set.ruler_distanceFromX - set.ruler_distanceFromTop;
        var per = length / (set.count - 1);
        for (int i = 0; i < set.count; i++)
        {
            float h = i * per + set.ruler_distanceFromX;
            if (h == 0)
            {
                continue;
            }
            Yjj_ChartUtility.DrawLine(vh, new Vector2(h, 0), new Vector2(h, set.rulerWidth), set.rulerLineWidth, set.rulerColor);
        }
    }
    public override void SetGraph(BaseSet bs, DataSet ds)
    {
        transform.localScale = Vector3.one;
        set = bs; dataSet = ds;
        TextParent.localPosition = Vector3.zero;
        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(set.width, set.hight);
        rect.pivot = Vector2.zero;
        if (set.rulerWidthDependGrah)
        {
            set.rulerWidth = set.hight;
        }
        //垂直轴
        if (set.verticalSprite != null)
        {
            for (int i = 0; i < set.rulerSet.Count; i++)
            {
                Image image;
                RectTransform imagerect;
                switch (set.rulerSet[i].pos)
                {
                    case RulerPos.Left:
                        image = transform.GetOrCreatUIChild("LeftVertical", typeof(Image)).GetComponent<Image>();
                        image.sprite = set.verticalSprite;
                        imagerect = image.rectTransform;
                        imagerect.pivot = new Vector2(0.5f, 0);
                        imagerect.anchorMin = Vector2.zero;
                        imagerect.anchorMax = Vector2.zero;
                        imagerect.anchoredPosition = Vector2.zero;
                        imagerect.sizeDelta = new Vector2(set.verticalLineWidth, set.hight);
                        break;
                    case RulerPos.Right:
                        image = transform.GetOrCreatUIChild("RightVertical", typeof(Image)).GetComponent<Image>();
                        image.sprite = set.verticalSprite;
                        imagerect = image.rectTransform;
                        imagerect.pivot = new Vector2(0.5f, 0);
                        imagerect.anchorMin = Vector2.zero;
                        imagerect.anchorMax = Vector2.zero;
                        imagerect.anchoredPosition = new Vector2(set.width, 0);
                        imagerect.sizeDelta = new Vector2(set.verticalLineWidth, set.hight);
                        break;
                }

            }
        }
        else
        {
            var go = transform.Find("LeftVertical");
            if (go != null)
            {
                DestroyImmediate(go.gameObject);
            }
            go = transform.Find("RightVertical");
            if (go != null)
            {
                DestroyImmediate(go.gameObject);
            }
        }

        DestroyText();
        GenerateText();
        SetVerticesDirty();
        GenerateTitle();
    }
    /// <summary>
    /// 生成标尺文字
    /// </summary>
    protected override void GenerateText()
    {
        for (int i = 0; i < set.rulerSet.Count; i++)
        {
            if (i == 1)
            {
                break;
            }
            var length = set.width - set.ruler_distanceFromX - set.ruler_distanceFromTop;
            var per = length / (set.count - 1);
            float xPos = 0;
            Vector2 pivot = Vector2.zero;
            TextAlignmentOptions option = TextAlignmentOptions.MidlineRight;
            switch (set.rulerSet[i].pos)
            {
                case RulerPos.Left:
                    xPos = -set.ruler_textPos;
                    pivot = new Vector2(1, 0.5f);
                    option = TextAlignmentOptions.Midline;
                    break;
                case RulerPos.Right:
                    xPos = set.hight + set.ruler_textPos;
                    pivot = new Vector2(1, 0.5f);
                    option = TextAlignmentOptions.Midline;
                    break;
            }
            for (int j = 0; j < set.count; j++)
            {
                float h = j * per + set.ruler_distanceFromX;
                var go = new GameObject("ruler");
                go.transform.parent = TextParent;
                var text = go.AddComponent<TextMeshProUGUI>();
                text.text = ((set.rulerSet[i].max - set.rulerSet[i].min) / (set.count - 1) * j + set.rulerSet[i].min).ToString("f0");
                text.transform.localScale = Vector3.one;
                text.fontSize = set.ruler_textSize;
                text.color = set.ruler_textColor;
                text.rectTransform.pivot = pivot;
                text.alignment = option;
                if (set.font != null)
                {
                    text.font = set.font;
                }
                text.rectTransform.anchoredPosition = new Vector2(h + text.rectTransform.sizeDelta.x*0.5f, xPos);
            }
            if (!string.IsNullOrEmpty(set.rulerSet[i].unit))
            {
                var go = new GameObject("单位");
                go.transform.parent = TextParent;
                var text = go.AddComponent<TextMeshProUGUI>();
                text.text = "(" + set.rulerSet[i].unit + ")";
                text.transform.localScale = Vector3.one;
                text.fontSize = set.ruler_textSize * (1 - (set.rulerSet[i].unit_sizeOffset));
                text.color = set.ruler_textColor;
                text.rectTransform.pivot = pivot;
                text.alignment = option;
                if (set.font != null)
                {
                    text.font = set.font;
                }
                text.rectTransform.anchoredPosition = new Vector2(xPos, set.hight) + set.rulerSet[i].unit_pos;
            }
        }
    }
    /// <summary>
    /// 生成标题
    /// </summary>
    protected override void GenerateTitle()
    {
        //  var trect = TitleParent.rectTransform();
        int count = TitleParent.childCount;
        for (int i = 0; i < count; i++)
        {
            DestroyImmediate(TitleParent.GetChild(0).gameObject);
        }
        var length = set.hight - dataSet.distanceFormLeft - dataSet.distanceFormRight;
        var unit = length / (dataSet.names.Count - 1);
        for (int i = 0; i < dataSet.names.Count; i++)
        {
            float p = dataSet.distanceFormLeft + i * unit;
            var go = new GameObject(dataSet.names[i], typeof(TextMeshProUGUI));
            go.transform.SetParent(TitleParent);
            go.transform.localScale = Vector3.one;
            var text = go.GetComponent<TextMeshProUGUI>();
            text.text = go.name;
            if (dataSet.font != null)
            {
                text.font = dataSet.font;
            }
            text.fontSize = dataSet.fontSize;
            text.color = dataSet.fontColor;
            if (!dataSet.isBias)
            {
                //text.alignment = TextAlignmentOptions.Center;
                text.alignment = dataSet.alignment;
                text.rectTransform.anchoredPosition = new Vector2(dataSet.font_DistanceFomrAsix,p);
            }
            else
            {
              //  text.alignment = TextAlignmentOptions.Right;
                text.alignment = dataSet.alignment;
                text.rectTransform.anchorMax = Vector2.one;
                text.rectTransform.anchorMin = Vector2.one;
                text.rectTransform.pivot = Vector2.one;
                text.rectTransform.Rotate(0, 0, dataSet.biasAngle);
                text.rectTransform.anchoredPosition = new Vector2(dataSet.font_DistanceFomrAsix, p + dataSet.biasHorDis);
            }
            text.rectTransform.sizeDelta = dataSet.rectSize;
        }
    }
}
