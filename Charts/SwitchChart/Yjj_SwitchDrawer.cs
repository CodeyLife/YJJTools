using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YJJTool
{

    [RequireComponent(typeof(CanvasRenderer))]
    public class Yjj_SwitchDrawer : Graphic
    {
        private Yjj_SwitchChart chart;


        public void SetGraph(Yjj_SwitchChart c)
        {
            chart = c;
            //标题父物体
            var titleRoot = transform.parent.GetOrCreatUIChild<RectTransform>("title", (rect) =>
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.sizeDelta = rectTransform.sizeDelta;
                rect.anchoredPosition = Vector2.zero;
            });
            var count = chart.datas.Count;
            for (int i = 0; i < count; i++)
            {
                GenerateTitle(i, titleRoot);
            }
            while (titleRoot.childCount > count)
            {
                DestroyImmediate(titleRoot.GetChild(count).gameObject);
            }
            SetVerticesDirty();

        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            vh.Clear();
            if (chart == null) return;
            var titleRoot = transform.parent.Find("title");
            Vector2 size = rectTransform.sizeDelta;
            float hMin = chart.set.ruler_distanceFromX;  // off高度
            float hMax = size.y - chart.set.ruler_distanceFromTop; //on 高度
            float xMin = chart.dataSet.distanceFormLeft;
            float xMax = size.x - chart.dataSet.distanceFormRight;
            var dates = chart.times.Select(x =>
            {
                return DateTime.Parse(x).ToTimeStamp();
            }).ToList();
            var min = dates[0];
            for (int i = 0; i < dates.Count; i++)
            {
                dates[i] -= min;
            }
            var dateMax = dates[dates.Count - 1];

            //dates.ForEach(x => Debug.Log(x));
            bool lastState = chart.datas[0];
            SetTitle(0, xMin, titleRoot);
            Vector2 last = lastState ? new Vector2(xMin, hMax) : new Vector2(xMin, hMin);
            var postions = new List<Vector2>() { last };
            float rectX = 0;
            for (int i = 1; i < chart.datas.Count; i++)
            {
                //当前状态
                bool currentState = chart.datas[i];
                float h = currentState ? hMax : hMin;
                float x = (float)dates[i] / (float)dateMax;
                x = (xMax - xMin) * x + xMin;
                SetTitle(i, x, titleRoot);
                //添加水平点
                var current = new Vector2(x, h);
                //如果当前状态不等于上一个状态
                if (lastState != currentState)
                {
                    postions.Add(new Vector2(x, last.y));

                }
                //画方块
                if (lastState != currentState || i == chart.datas.Count - 1)
                {
                    Yjj_ChartUtility.DrawQuad(vh, new Vector3(rectX, hMin), new Vector3(rectX, hMax), new Vector3(x, hMax), new Vector3(x, hMin), lastState ? chart.onColor : chart.offColor);
                    rectX = x;
                }
                postions.Add(current);

                //    Yjj_ChartUtility.DrawLineSmooth(vh, last, current, chart.lineSet.width, chart.lineSet.colors[0]);
                last = current;
                lastState = currentState;
            }

            Yjj_ChartUtility.DrawLines(vh, postions, chart.lineSet.width, chart.lineSet.colors[0]);

        }
        private void SetTitle(int index, float x, Transform parent)
        {
            var rect = parent.Find(index.ToString()).GetComponent<TextMeshProUGUI>();
            rect.rectTransform.anchoredPosition = new Vector2(x, chart.dataSet.font_DistanceFomrAsix);
        }
        private void GenerateTitle(int index, Transform parent)
        {
            //var pro = new GameObject(index.ToString(), typeof(TextMeshProUGUI));
            var pro = parent.GetOrCreatUIChild<RectTransform>(index.ToString(), (rect) =>
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
            }, typeof(TextMeshProUGUI));
            pro.transform.SetParent(parent);
            var text = pro.GetComponent<TextMeshProUGUI>();
            text.fontSize = chart.dataSet.fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.font = chart.dataSet.font;
            text.color = chart.dataSet.fontColor;
            text.text = DateTime.Parse(chart.times[index]).ToString("HH:mm");
        }
    }
}