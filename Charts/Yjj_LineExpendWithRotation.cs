using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YJJTool
{
    public class Yjj_LineExpendWithRotation : Yjj_Line
    {
        public virtual void SetGraph(List<Vector2> arr, LineSet set, bool loseLeft, bool loseRight, List<float> rotations, int index = 0, List<float> datas = null)
        {
            CheckColor(set, index);
            if (set.isCurve)
            {

                int count = (arr.Count - 1) * set.smooth;
                pos = Yjj_ChartUtility.GetCurvePosFroJob(arr, count);

            }
            else
            {
                pos = arr;
            }
            colorIndex = index;
            lineSet = set;
            material = set.material;
            //删除
            if (!Application.isPlaying)
            {
                int child = transform.childCount;
                for (int j = 0; j < child; j++)
                {
                    DestroyImmediate(transform.GetChild(0).gameObject);
                }
            }
            //曲线

            if (datas != null && pos.Count > datas.Count + 2)
            {

                int length = datas.Count + 1;
                if (!loseLeft) length--;
                if (!loseRight) length--;
                var perLenth = (pos.Count) / length;
                if (set.sprite != null)
                {
                    for (int i = 0; i < datas.Count; i++)
                    {
                        var image = transform.GetOrCreatUIChild("image" + i, typeof(Image)).GetComponent<Image>();
                        image.sprite = set.sprite;
                        image.rectTransform.anchorMin = Vector2.zero;
                        image.rectTransform.anchorMax = Vector2.zero;
                        var posIndex = loseLeft ? i + 1 : i;
                        image.rectTransform.anchoredPosition = pos[posIndex * perLenth];
                        image.transform.localScale = Vector3.one * set.scale;
                        image.color = set.spriteColor;
                    }
                }
                if (set.font != null && datas != null)
                {
                    for (int i = 0; i < datas.Count; i++)
                    {
                        var text = transform.GetOrCreatUIChild("text" + i, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
                        text.font = set.font;
                        text.rectTransform.pivot = Vector2.zero;
                        text.rectTransform.anchorMax = Vector2.zero;
                        text.rectTransform.anchorMin = Vector2.zero;
                        text.fontSize = set.fontSize;
                        var posIndex = loseLeft ? i + 1 : i;
                        text.rectTransform.anchoredPosition = pos[posIndex * perLenth] + set.fontOffeset;
                        text.color = set.fontColor;
                        text.text = datas[i].ToString();
                        text.alignment = TextAlignmentOptions.Left;
                    }
                }
            }
            else
            {
                int i = loseLeft ? 1 : 0;
                int count = loseRight ? arr.Count - 1 : arr.Count;

                if (set.sprite != null)
                {
                    for (; i < count; i++)
                    {
                        var image = transform.GetOrCreatUIChild("image" + i, typeof(Image)).GetComponent<Image>();
                        image.sprite = set.sprite;
                        image.rectTransform.anchorMin = Vector2.zero;
                        image.rectTransform.anchorMax = Vector2.zero;
                        image.rectTransform.anchoredPosition = arr[i];
                        image.transform.localScale = Vector3.one * set.scale;
                        image.color = set.spriteColor;
                        image.transform.eulerAngles = new Vector3(0, 0, rotations[i]);
                    }
                }

                if (set.font != null && datas != null)
                {
                    i = loseLeft ? 1 : 0;
                    for (; i < count; i++)
                    {
                        var text = transform.GetOrCreatUIChild("text" + i, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
                        text.font = set.font;
                        text.rectTransform.pivot = Vector2.zero;
                        text.rectTransform.anchorMax = Vector2.zero;
                        text.rectTransform.anchorMin = Vector2.zero;
                        text.fontSize = set.fontSize;
                        text.rectTransform.anchoredPosition = arr[i] + set.fontOffeset;
                        text.color = set.fontColor;
                        text.text = datas[i].ToString();
                        text.alignment = TextAlignmentOptions.Left;
                    }
                }
            }


            SetVerticesDirty();
        }
    }
}