using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YJJTool
{



    [RequireComponent(typeof(CanvasRenderer))]
    public class Yjj_GraphPopulateMeshBase : Graphic
    {
        public BaseSet set;
        public DataSet dataSet;
        protected Transform _textParent;
        protected Transform _titleParent;

        public Transform TextParent
        {
            get
            {
                if (_textParent == null)
                {
                    _textParent = transform.parent.GetOrCreatUIChild<RectTransform>("rulerText", (rect) =>
                     {
                         rect.sizeDelta = Vector2.zero;
                         rect.anchorMin = Vector2.zero;
                         rect.anchorMax = Vector2.zero;
                         rect.pivot = Vector2.zero;
                     });
                }
                return _textParent;

            }
            set => _textParent = value;
        }

        public Transform TitleParent
        {
            get
            {
                if (_titleParent == null)
                {
                    _titleParent = transform.parent.GetOrCreatUIChild<RectTransform>("dataNames", (rect) =>
                     {
                         rect.sizeDelta = Vector2.zero;
                         rect.anchorMin = Vector2.zero;
                         rect.anchorMax = Vector2.zero;
                         rect.pivot = Vector2.zero;
                     });
                }
                return _titleParent;
            }
            set => _titleParent = value;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            //水平轴
            if (set.horizatalSprite == null)
            {
                Yjj_ChartUtility.DrawLine(vh, Vector2.zero, new Vector2(set.width, 0), set.lineWidth, set.lineColor);
            }
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
        }
        public virtual void SetGraph(BaseSet bs, DataSet ds)
        {
            raycastTarget = false;
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
                            image.raycastTarget = false;
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
                            image.raycastTarget = false;
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
            //水平轴
            if (set.horizatalSprite != null)
            {
                Image image;
                RectTransform imagerect;
                transform.GetOrCreatUIChild("Horizatal", (t) =>
                 {
                     image = t.GetComponent<Image>();
                     image.raycastTarget = false;
                     image.sprite = set.horizatalSprite;
                     imagerect = image.rectTransform;
                     imagerect.pivot = new Vector2(0, 0.5f);
                     imagerect.anchorMin = Vector2.zero;
                     imagerect.anchorMax = Vector2.zero;
                     imagerect.anchoredPosition = Vector2.zero;
                     imagerect.sizeDelta = new Vector2(set.width, set.lineWidth);

                 }, typeof(Image));

            }
            else
            {
                var go = transform.Find("Horizatal");
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
        protected virtual void DestroyText()
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
        protected virtual void GenerateText()
        {
            if (set.ruler_textSize == 0) return;
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
                    var text = TextParent.GetOrCreatUIChild<TextMeshProUGUI>($"ruler{i}-{j}");
                    text.text = ((set.rulerSet[i].max - set.rulerSet[i].min) / (set.count - 1) * j + set.rulerSet[i].min).ToString("f0");
                    if (set.rulerSet[i].shwoUnitInText)
                    {
                        text.text += set.rulerSet[i].unit;
                    }
                    text.fontSize = set.ruler_textSize;
                    text.color = set.ruler_textColor;
                    text.rectTransform.pivot = pivot;
                    text.alignment = option;
                    if (set.font != null)
                    {
                        text.font = set.font;
                    }
                    text.rectTransform.anchoredPosition = new Vector2(xPos, h);
                    text.raycastTarget = false;
                }
                if (!string.IsNullOrEmpty(set.rulerSet[i].unit) && !set.rulerSet[i].shwoUnitInText)
                {
                    var text = TextParent.GetOrCreatUIChild<TextMeshProUGUI>($"单位{i}");
                    text.text = "(" + set.rulerSet[i].unit + ")";
                    text.fontSize = set.ruler_textSize * (1 - (set.rulerSet[i].unit_sizeOffset));
                    text.color = set.ruler_textColor;
                    text.rectTransform.pivot = pivot;
                    text.alignment = option;
                    if (set.font != null)
                    {
                        text.font = set.font;
                    }
                    text.rectTransform.anchoredPosition = new Vector2(xPos, set.hight) + set.rulerSet[i].unit_pos;
                    text.raycastTarget = false;
                }
            }
        }
        /// <summary>
        /// 生成标题
        /// </summary>
        protected virtual void GenerateTitle()
        {
            //  var trect = TitleParent.rectTransform();
            TitleParent.DelateAllChild();
            var length = set.width - dataSet.distanceFormLeft - dataSet.distanceFormRight;   //总长度
            var unit = dataSet.names.Count > 1 ? length / (dataSet.names.Count - 1) : 0;   //每个标题之间的间隔距离
            int nameCount = dataSet.names.Count;
            float singleLength = 0;

            if (set.autoSpace)
            {
                //自动根据长度  算标题
                float leaveSpace = unit * 0.5f;
                for (int i = 0; i < nameCount; i++)
                {
                    leaveSpace += unit;
                    if (leaveSpace <= 0)
                    {
                        //最后一个标题
                        //if (i == nameCount - 1)
                        //{
                        //    CreatTitle(dataSet.names[i], i, 1);
                        //    break;
                        //}
                        continue;
                    }
                    var current = CreatTitle(dataSet.names[i], i);
                    var perfectWidth = current.preferredWidth * 0.5f;
                    if (leaveSpace - perfectWidth <= set.minSpace && i != 0)
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(current.gameObject);
                        }
                        else
                        {
                            DestroyImmediate(current.gameObject);
                        }
                    }
                    else
                    {
                        leaveSpace = -perfectWidth;
                    }
                }
            }
            else
            {
                int target = 0;
                for (int i = 0; i < nameCount; i++)
                {
                    if (i != target)
                    {
                        //最后一个标题
                        //if (i == nameCount - 1)
                        //{
                        //    CreatTitle(dataSet.names[i], i, 1);
                        //    break;
                        //}
                        continue;
                    }
                    target += set.nameSpace;

                    CreatTitle(dataSet.names[i], i);
                }
            }


            TextMeshProUGUI CreatTitle(string name, int i, float offeset = 0)
            {
                var text = TitleParent.GetOrCreatUIChild<TextMeshProUGUI>($"{i}{dataSet.names[i]}");
                text.text = name;
                float p = offeset == 0 ? dataSet.distanceFormLeft + i * unit : dataSet.distanceFormLeft + (i - 1) * unit + singleLength * 0.5f;
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
                text.rectTransform.sizeDelta = dataSet.rectSize;
                text.raycastTarget = false;
                return text;
            }
        }

    }


}
