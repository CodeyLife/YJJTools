using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YJJTool
    {


    public class Yjj_GraphPopulateMeshFroLargeData : Yjj_GraphPopulateMeshBase
    {
        private Color pointColor;
        private float radius;

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
                            Yjj_ChartUtility.DrawLine(vh, new Vector2(set.width, 0), new Vector2(set.width, set.hight), set.verticalLineWidth, set.verticalColor);
                            break;
                    }

                }
            }



            //画水平刻度
            var length = set.hight - set.ruler_distanceFromX - set.ruler_distanceFromTop;
            var per = length / (set.count - 1);
            for (int i = 0; i < set.count; i++)
            {
                float h = i * per + set.ruler_distanceFromX;
                if (h == 0)
                {
                    continue;
                }
                Yjj_ChartUtility.DrawLine(vh, new Vector2(0, h), new Vector2(set.rulerWidth, h), set.rulerLineWidth, set.rulerColor);
            }
            //画标题对应的点
            if (titlePoints == null) return;
            for (int i = 0; i < titlePoints.Count; i++)
            {
                Yjj_ChartUtility.DrawCircle(vh, new Vector2(titlePoints[i], 0), radius, pointColor, 12);
            }
            titlePoints = null;
        }
        private List<Vector2> dataPoints;
        public void SetGraph(BaseSet bs, DataSet ds, float r, Color c, List<Vector2> Points = null)
        {
            dataPoints = Points;
            radius = r; pointColor = c;
            transform.localScale = Vector3.one;
            set = bs; dataSet = ds;
            TextParent.localPosition = Vector3.zero;
            RectTransform rect = GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(set.width, set.hight);
            rect.pivot = Vector2.zero;
            if (set.rulerWidthDependGrah)
            {
                set.rulerWidth = set.width;
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
        /// 删除标尺文字
        /// </summary>
        protected override void DestroyText()
        {

            int count = TextParent.childCount;
            for (int i = 0; i < count; i++)
            {
                DestroyImmediate(TextParent.GetChild(0).gameObject);
            }
        }
        /// <summary>
        /// 生成标尺文字
        /// </summary>
        protected override void GenerateText()
        {
            for (int i = 0; i < set.rulerSet.Count; i++)
            {

                var length = set.hight - set.ruler_distanceFromX - set.ruler_distanceFromTop;
                var per = length / (set.count - 1);
                float xPos = 0;
                Vector2 pivot = Vector2.zero;
                TextAlignmentOptions option = TextAlignmentOptions.MidlineRight;
                switch (set.rulerSet[i].pos)
                {
                    case RulerPos.Left:
                        xPos = -set.ruler_textPos;
                        pivot = new Vector2(1, 0.5f);
                        option = TextAlignmentOptions.MidlineRight;
                        break;
                    case RulerPos.Right:
                        xPos = set.width + set.ruler_textPos;
                        pivot = new Vector2(0, 0.5f);
                        option = TextAlignmentOptions.MidlineLeft;
                        break;
                }
                for (int j = 0; j < set.count; j++)
                {
                    float h = j * per + set.ruler_distanceFromX;
                    var text = TextParent.GetOrCreatUIChild<TextMeshProUGUI>($"ruler{j}");
                    text.text = ((set.rulerSet[i].max - set.rulerSet[i].min) / (set.count - 1) * j + set.rulerSet[i].min).ToString("f0");
                    text.fontSize = set.ruler_textSize;
                    text.color = set.ruler_textColor;
                    text.rectTransform.pivot = pivot;
                    text.alignment = option;
                    if (set.font != null)
                    {
                        text.font = set.font;
                    }
                    text.rectTransform.anchoredPosition = new Vector2(xPos, h);
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
        private List<float> titlePoints;
        /// <summary>
        /// 生成标题
        /// </summary>
        protected override void GenerateTitle()
        {
            titlePoints = new List<float>();
            //  var trect = TitleParent.rectTransform();
            int count = TitleParent.childCount;
            for (int i = 0; i < count; i++)
            {
                DestroyImmediate(TitleParent.GetChild(0).gameObject);
            }
            var length = set.width - dataSet.distanceFormLeft - dataSet.distanceFormRight;
            int nameCount = set.nameSpace;
            var unit = length / (dataSet.names.Count - 1);
            int target = 0;

            for (int i = 0; i < dataSet.names.Count; i++)
            {
                if (i != target)
                {
                    if (i != dataSet.names.Count - 1)
                    {
                        continue;
                    }
                }
                target += nameCount;
                if (dataSet.names.Count - 1 < target + set.nameSpace)
                {
                    target = dataSet.names.Count - 1;
                }
                float p;
                if (dataPoints != null)
                {
                    p = dataPoints[i].x;
                }
                else
                {
                    p = dataSet.distanceFormLeft + i * unit;
                }
                var text = TitleParent.GetOrCreatUIChild<TextMeshProUGUI>(dataSet.names[i]);
                text.text = text.name;
                if (dataSet.font != null)
                {
                    text.font = dataSet.font;
                }
                text.fontSize = dataSet.fontSize;
                text.color = dataSet.fontColor;
                if (!dataSet.isBias)
                {
                    text.alignment = TextAlignmentOptions.Center;
                    text.rectTransform.anchoredPosition = new Vector2(p, dataSet.font_DistanceFomrAsix);
                }
                else
                {
                    text.alignment = TextAlignmentOptions.Right;
                    text.rectTransform.anchorMax = Vector2.one;
                    text.rectTransform.anchorMin = Vector2.one;
                    text.rectTransform.pivot = Vector2.one;
                    text.rectTransform.Rotate(0, 0, dataSet.biasAngle);
                    text.rectTransform.anchoredPosition = new Vector2(p + dataSet.biasHorDis, dataSet.font_DistanceFomrAsix);
                }
                titlePoints.Add(p);
            }
        }

    }
}