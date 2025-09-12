using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace YJJTool
{

    public class Yjj_GraphPopulateMeshBaseHorizatalCenter : Yjj_GraphPopulateMeshBaseHorizatal
    {
        public override void SetGraph(BaseSet bs, DataSet ds)
        {
            if (bs.count % 2 == 0)
            {
                bs.count++;
            }
            base.SetGraph(bs, ds);
        }
        protected override void GenerateText()
        {
            //  base.GenerateText();
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
                int center = Mathf.FloorToInt(set.count / 2f);
                int maxDistance = center;
                float unit = (set.rulerSet[i].max - set.rulerSet[i].min) / maxDistance;
                for (int j = 0; j < set.count; j++)
                {
                    float h = j * per + set.ruler_distanceFromX;
                    var go = new GameObject("ruler");
                    go.transform.parent = TextParent;
                    var text = go.AddComponent<TextMeshProUGUI>();
                    var distance = Mathf.Abs(center - j);
                    text.text = (distance * unit).ToString("f0");
                    text.transform.localScale = Vector3.one;
                    text.fontSize = set.ruler_textSize;
                    text.color = set.ruler_textColor;
                    text.rectTransform.pivot = pivot;
                    text.alignment = option;
                    if (set.font != null)
                    {
                        text.font = set.font;
                    }
                    text.rectTransform.anchoredPosition = new Vector2(h + text.rectTransform.sizeDelta.x * 0.5f, xPos);
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
    }
}