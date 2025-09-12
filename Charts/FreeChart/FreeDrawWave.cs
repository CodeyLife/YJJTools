using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YJJTool
{
    public class FreeDrawWave : DrawFreeChartBase
    {
        private RawImage image;
        [FoldoutGroup("数据设置")]
        public LineSet lineSet = new LineSet();
        [FoldoutGroup("数据设置")]
        [LabelText("细分")]
        [Range(1, 8)]
        public int smooth = 3;
        [FoldoutGroup("数据设置")]
        [LabelText("图表颜色")]
        public Color data_lineColor = Color.yellow;
        [FoldoutGroup("数据设置")]
        [LabelText("computeShader")]
        public ComputeShader cs;
        [FoldoutGroup("数据设置")]
        [LabelText("背景底图")]
        public Texture2D background;
        [FoldoutGroup("数据设置")]
        public Vector2 texturePix = new Vector2(512, 512);
        [FoldoutGroup("数据设置")]
        [LabelText("图像最高不透明度")]
        [Range(0.1f, 1f)]
        public float maxA = 0.6f;
        [FoldoutGroup("数据设置")]
        [LabelText("图像最低不透明度")]
        [Range(-1f, 1)]
        public float minA = 0.1f;

        [FoldoutGroup("动画设置")]
        [Title("循环动画相关设置", TitleAlignment = TitleAlignments.Centered)]
        public Image loopLine;
        [FoldoutGroup("动画设置")]
        [LabelText("线条高度是否跟随数据高度")]
        public bool dataHigh = true;
        [FoldoutGroup("动画设置")]
        [LabelText("圆点")]
        public Image loopSpere;
        [FoldoutGroup("动画设置")]
        [LabelText("循环动画中显示数据的文本")]
        public TextMeshProUGUI loopText;
        [FoldoutGroup("动画设置")]
        [LabelText("是否显示数据单位")]
        public bool showUnit = true;

        protected RenderTexture rt;
        protected ComputeBuffer buffer;

        protected List<Vector2> list;
        protected Vector2[] arr;
        public override void SetGraph(FreeChart root)
        {
            base.SetGraph(root);
            if (!CheckData(root))
            {
                return;
            }
            if (!Application.isPlaying)
            {
                GC.Collect();
            }
            var distance = Chart.set.width / (Chart.datas[0].datas.Count + 1);
            Chart.dataSet.distanceFormLeft = distance;
            Chart.dataSet.distanceFormRight = distance;
            //获取数据点
            int index = isLeftRuler ? 0 : 1;
            list = Yjj_ChartUtility.GetPosFromData(Chart.datas[dataIndex[0]].datas, Chart.set, Chart.dataSet, index, true, true);
            var c = smooth * (list.Count + 1);
            arr = Yjj_ChartUtility.GetCurveArr(list);
            list = Yjj_ChartUtility.GetCurvePosFroJob(list, c);


            //computeshader
            image = transform.GetOrCreatUIChild("rawImage", true, typeof(RawImage)).GetComponent<RawImage>();
            image.rectTransform.sizeDelta = new Vector2(Chart.set.width, Chart.set.hight);

            int width = (int)texturePix.x;
            int height = (int)texturePix.y;
            if (rt == null || rt.texelSize != texturePix)
            {
                rt = new RenderTexture(width, height, 24);
                rt.enableRandomWrite = true;
                rt.Create();
                image.texture = rt;

                //设置属性
                cs.SetTexture(0, "Result", rt);
                cs.SetInt("width", width);
                cs.SetInt("height", height);

            }
            //计算数据
            if (buffer != null)
            {
                buffer.Release();
            }
            buffer = new ComputeBuffer(arr.Length, arr.Length * 8);

            buffer.SetData(arr);
            //   cs.SetFloat("lineWidth", data_lineWidth);
            cs.SetVector("line1Color", data_lineColor);
            cs.SetInt("dataCount", arr.Length);
            cs.SetBuffer(0, "datas1", buffer);
            cs.SetTexture(0, "background", background);
            cs.SetFloat("left", Chart.dataSet.distanceFormLeft / Chart.set.width);
            cs.SetFloat("right", Chart.dataSet.distanceFormRight / Chart.set.width);
            cs.SetFloat("maxA", maxA);
            cs.SetFloat("minA", minA);
            cs.SetFloat("maxY", Chart.set.hight);
            cs.Dispatch(0, width / 8, height / 8, 1);
            //画线
            var line = transform.GetOrCreatUIChild("line", true, (typeof(Yjj_Line))).GetComponent<Yjj_Line>();
            line.SetGraph(list, lineSet);
            //  Test();
        }
        public override void PlayAnimation()
        {
            base.PlayAnimation();
            InitLoop();
            StopAllCoroutines();
            StartCoroutine(Animation());
        }
        protected void InitLoop()
        {
            if (loopLine != null)
            {
                loopLine.color = loopLine.color.SetAlpha(0);
            }
            if (loopSpere != null)
            {
                loopSpere.color = loopSpere.color.SetAlpha(0);
            }
            if (loopText != null)
            {
                loopText.color = loopText.color.SetAlpha(0);
            }
        }
        protected IEnumerator Animation()
        {
            var tempList = new List<Vector2>();
            var tempArr = new Vector2[arr.Length];
            var line = transform.Find("line").GetComponent<Yjj_Line>();
            int width = (int)texturePix.x;
            int height = (int)texturePix.y;
            yield return StartCoroutine(YjjUtility.FadeIn(Chart.animationSet.fadeInTime, (t) =>
            {
                FadeList(t, list, ref tempList);
                line.SetGraph(tempList, lineSet);
                FadeArr(t, arr, ref tempArr);
                buffer.SetData(tempArr);
            //   cs.SetFloat("lineWidth", data_lineWidth);
            cs.SetTexture(0, "Result", rt);
                cs.SetInt("width", width);
                cs.SetInt("height", height);
                cs.SetVector("line1Color", data_lineColor);
                cs.SetInt("dataCount", arr.Length);
                cs.SetBuffer(0, "datas1", buffer);
                cs.SetTexture(0, "background", background);
                cs.SetFloat("left", Chart.dataSet.distanceFormLeft / Chart.set.width);
                cs.SetFloat("right", Chart.dataSet.distanceFormRight / Chart.set.width);
                cs.SetFloat("maxA", maxA);
                cs.SetFloat("minA", minA);
                cs.SetFloat("maxY", Chart.set.hight);
                cs.Dispatch(0, width / 8, height / 8, 1);
            }, null, Chart.animationSet.fadeInCurve));
            if (loopLine != null)
            {
                loopLine.rectTransform.anchorMin = Vector2.zero;
                loopLine.rectTransform.anchorMax = Vector2.zero;
                loopLine.rectTransform.pivot = new Vector2(0.5f, 0);
                loopLine.rectTransform.sizeDelta = new Vector2(loopLine.rectTransform.sizeDelta.x, Chart.set.hight);
            }
            if (loopSpere != null)
            {
                loopSpere.rectTransform.anchorMin = Vector2.zero;
                loopSpere.rectTransform.anchorMax = Vector2.zero;
            }
            if (loopText != null && loopText.transform.parent != loopSpere.transform)
            {
                loopText.transform.SetParent(loopSpere.transform);
            }
            StartCoroutine(Loop(0));
        }
        protected IEnumerator Loop(int index)
        {
            if (loopLine != null || loopSpere != null)
            {
                //设置位置
                if (loopLine != null)
                {
                    loopLine.rectTransform.anchoredPosition = new Vector2(arr[index + 2].x, 0);
                    if (dataHigh)
                    {
                        loopLine.rectTransform.sizeDelta = new Vector2(loopLine.rectTransform.sizeDelta.x, arr[index + 2].y);
                    }
                }
                if (loopSpere != null)
                {
                    loopSpere.rectTransform.anchoredPosition = arr[index + 2];
                }
                if (loopText != null)
                {
                    loopText.text = Chart.datas[dataIndex[0]].datas[index].ToString();
                    if (showUnit)
                    {
                        loopText.text += Chart.set.rulerSet[0].unit;
                    }
                }
                //fadeIn
                yield return StartCoroutine(YjjUtility.FadeIn(Chart.animationSet.loopFadeTime, (t) =>
                {
                    if (loopSpere != null)
                    {
                        loopSpere.color = loopSpere.color.SetAlpha(t);
                    }
                    if (loopLine != null)
                    {
                        loopLine.color = loopLine.color.SetAlpha(t);
                    }
                    if (loopText != null)
                    {
                        loopText.color = loopText.color.SetAlpha(t);
                    }
                }));
                yield return new WaitForSeconds(Chart.animationSet.loopTime);
                yield return StartCoroutine(YjjUtility.FadeOut(Chart.animationSet.loopFadeTime, (t) =>
                {
                    if (loopSpere != null)
                    {
                        loopSpere.color = loopSpere.color.SetAlpha(t);
                    }
                    if (loopLine != null)
                    {
                        loopLine.color = loopLine.color.SetAlpha(t);
                    }
                    if (loopText != null)
                    {
                        loopText.color = loopText.color.SetAlpha(t);
                    }
                }));
                index++;
                index = index >= Chart.datas[dataIndex[0]].datas.Count ? 0 : index;
                StartCoroutine(Loop(index));
            }
        }
    }
}