using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

[RequireComponent(typeof(CanvasRenderer))]
public class MarkComponent : Graphic
{
    public float lineWidth = 2, lineLength = 20;
    public Color lineColor = Color.gray;
    public TMP_FontAsset font;
    public Color textColor = Color.white;
    public float textSize = 24;
    public float textSpace = 20;

    public float scaleVlaue = 1;


    public List<int> maskList=  new List<int>();
    private List<Vector2> markPosition = new List<Vector2>();
    private List<float> markValues = new List<float>();
    private float lerpValue = 1;
    [ShowInInspector]
    private List<Vector2[]> lineArrs = new List<Vector2[]>();
    Vector2[] tempArr = new Vector2[3];  
    //动画插值
    public float LerpValue
    {
        get => lerpValue; set
        {
            lerpValue = value;
         
             SetVerticesDirty();
        }
    }

    public void Init(Action<MarkComponent> action)
    {
        action?.Invoke(this);
    }

    public void Clear()
    {
        markPosition.Clear(); markValues.Clear();
    }
    public void Add(Vector3 point, float value)
    {
        markPosition.Add(point); markValues.Add(value);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (lerpValue == 0) return;

        if (Application.isPlaying)
        {
            for (int i = 0; i < maskList.Count; i++)
            {
                var index = maskList[i];
                var arr = lineArrs[index];
                Array.Copy(arr, tempArr, 3);
                var text = transform.Find(index.ToString()).GetComponent<TextMeshProUGUI>();
                text.color = textColor.SetAlpha(lerpValue);
                var scale = Mathf.Lerp(1, scaleVlaue, lerpValue);
                tempArr[1] *= scale;
                tempArr[2] = new Vector2(tempArr[2].x, tempArr[1].y);
                if (lerpValue == 1)
                {
                    text.rectTransform.anchoredPosition = (tempArr[1] + tempArr[2]) * 0.5f;
                    Yjj_ChartUtility.DrawLineSmooth(vh, tempArr, lineWidth, lineColor);
                }
                else
                {
                    var result = Yjj_ChartUtility.DrawLineSmoothWithLerp(vh, tempArr, lineWidth, lineColor, LerpValue);
                    text.rectTransform.anchoredPosition = (tempArr[1] + result) * 0.5f;
                }
                text.ForceMeshUpdate();
            }
        }
        else
        {
            //全部绘制 屏蔽
            for (int i = 0; i < lineArrs.Count; i++)
            {
                var arr = lineArrs[i];
                if (lerpValue == 1)
                {
                    Yjj_ChartUtility.DrawLineSmooth(vh, arr, lineWidth, lineColor);
                }
                else
                {
                    Yjj_ChartUtility.DrawLineSmoothWithLerp(vh, arr, lineWidth, lineColor, LerpValue);
                }
            }
        }

    }
    public void SetGraph()
    {
        lineArrs.Clear();
        for (int i = 0; i < markPosition.Count; i++)
        {
            var pos = markPosition[i];
            var left = MeshUtility.ToTheLeft(pos, Vector3.zero, Vector3.up);
            Vector2[] arr = new Vector2[3];
            arr[0] = pos;
            arr[1] = pos + pos.normalized * lineLength;

            //生成文本
            var text = transform.GetOrCreatUIChild<TextMeshProUGUI>(i.ToString(), (t) =>
            {
                t.enableWordWrapping = false;
                //t.textWrappingMode = TextWrappingModes.NoWrap;
                t.raycastTarget = false;
                t.rectTransform.pivot = new Vector2(0.5f, 0);
            });
            text.alignment = TextAlignmentOptions.Bottom;

            //if (left)
            //{
            //    //text.rectTransform.anchorMin  = new Vector2(1, 0);
            //    //text.rectTransform.anchorMax = text.rectTransform.anchorMin;
            //    text.rectTransform.pivot = new Vector2(1, 0);
            //    text.alignment = TextAlignmentOptions.BottomRight;
            //}
            //else
            //{
            //    //text.rectTransform.anchorMin = new Vector2(0, 0);
            //    //text.rectTransform.anchorMax = text.rectTransform.anchorMin;
            //    text.rectTransform.pivot = new Vector2(0, 0);
            //    text.alignment = TextAlignmentOptions.BottomLeft;
            //}
           
            text.color = textColor.SetAlpha(Application.isPlaying?0:1);
            text.fontSize = textSize;
            text.text = markValues[i].ToAutoLimitString(1);

            var prefer = text.GetPreferredValues();
            var x = left ? -prefer.x - textSpace * 2 : prefer.x + textSpace * 2;
            arr[2] = new Vector3(x + arr[1].x, arr[1].y);

            text.rectTransform.anchoredPosition = (arr[1] + arr[2]) * 0.5f;
            lineArrs.Add(arr);

        }
        //删除多余的文本
        transform.DelateChildByCount(markPosition.Count);
        SetVerticesDirty();
        SetAllDirty();
    }

}
