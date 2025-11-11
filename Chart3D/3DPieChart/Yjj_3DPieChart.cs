#if DOTWEEN
using DG.Tweening;
#endif
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YJJTool
{
    public class Yjj_3DPieChart : MonoBehaviour
    {
        public List<float> datas = new List<float>();
        public List<Color> colors = new List<Color>();
        public Material mat;
        private Matrix4x4 m;

        [LabelText("饼状图半径")]
        public float radius = 20;
        [LabelText("饼状图厚度")]
        public float pieDepth = 20;
        [LabelText("间隔")]
        public float spacing = 5f;
        [LabelText("细分")]
        public int smooth = 64;
        //线
        [InlineEditor]
        public MarkComponent mark;
        //动画

        public float fadeInTime = 2f;
        public float hoverDuration = 1f, hoverScale = 1.25f;
        private float animationValue = 1;

        #region Inspector

        [OnInspectorInit]
        void InspectorInit()
        {
            if (mark == null)
            {
                mark = transform.GetOrCreatUIChild<MarkComponent>("Mark", (m) =>
                {
                    m.rectTransform.anchorMin = Vector2.zero;
                    m.rectTransform.anchorMax = Vector2.one;
                    m.rectTransform.sizeDelta = Vector2.zero;
                    m.rectTransform.anchoredPosition = Vector2.zero;
                });
            }
            InitGraph();
            SetGraph();
        }
        [OnInspectorGUI]

        private void GuiChange()
        {
            if (GUI.changed)
            {
                StartCoroutine(YjjUtility.DeLay(() =>
                {
                    animationValue = 1;
                    SetGraph();
                }));
            }
        }
        #endregion

        void InitGraph()
        {
            mark.Init((m) =>
            {
                m.scaleVlaue = hoverScale;
            });
        }

        public void SetGraph(List<float> datas)
        {
            this.datas = datas;
            SetGraph();
            if (Application.isPlaying)
            {
                PlayAnimation();
            }
        }
        private void SetGraph()
        {
            mark.Clear();
            //生成数据
            var max = datas.Count == 1 ? 100 : datas.Sum();

            var noZeroCount = datas.Where(x => x != 0).Count();

            float halfSpacting = spacing * 0.5f;
            float startAngle = halfSpacting;
            float allAngle = 360 - noZeroCount * spacing;
            for (int i = 0; i < datas.Count; i++)
            {
           
                float angle = datas[i] / max * allAngle;

                int smoothCount = Mathf.CeilToInt(angle / allAngle * smooth);
                float perAngle = angle / smoothCount * animationValue;
                List<Vector2[]> list = new List<Vector2[]>();
                for (int j = 0; j < smoothCount; j++)
                {
                    float begin = startAngle + perAngle * j;
                    float end = begin + perAngle;
                    begin *= Mathf.Deg2Rad;
                    end *= Mathf.Deg2Rad;
                    var beginPos = new Vector3(Mathf.Sin(begin) * radius, Mathf.Cos(begin) * radius);
                    var endPos = new Vector3(Mathf.Sin(end) * radius, Mathf.Cos(end) * radius);

                    Vector2[] arr = new Vector2[2];
                    arr[0] = beginPos;
                    arr[1] = endPos;
                    //Yjj_ChartUtility.DrawTriangleMesh(vh, Vector3.zero, beginPos, endPos, pieDepth, drawLeft, drawRight, color: colors[i]);
                    list.Add(arr);
                }
                //绘制线 
                var half = startAngle + angle * 0.5f;
                if (angle == 0)
                    half -= spacing * 0.5f;
                half *= Mathf.Deg2Rad;
                var halfPos = new Vector3(Mathf.Sin(half) * radius, Mathf.Cos(half) * radius, pieDepth * 0.5f);
                mark.Add(halfPos, datas[i]);
                startAngle += angle;
                if (angle != 0)
                    startAngle += spacing;

                //生成子节点
                var pieChild = transform.GetOrCreatUIChild<Pie3DComponent>($"pie{i}", (t) =>
                {
                    //  t.gameObject.hideFlags = HideFlags.HideInHierarchy;
                    t.rectTransform.FullByParent();
                });
                pieChild.FillData(list, i, this);
                pieChild.material = mat;
            }
            mark.SetGraph();
            mark.LerpValue = animationValue;
        }
        protected void OnEnable()
        {
            PlayAnimation();
        }
        public void PlayAnimation()
        {
            if (fadeInTime <= 0) return;
            if (Application.isPlaying)
            {
#if DOTWEEN
                DOTween.To(() => 0f,
                    x => animationValue = x,
                    1, fadeInTime)
                    .SetEase(Ease.OutQuad)
                    .OnUpdate(() => SetGraph());
#else
             StopAllCoroutines();
                StartCoroutine(YjjUtility.FadeIn(animationValue, (t) =>
                {
                    animationValue = t;
                    SetGraph();
                }));
#endif

            }
        }


        private Vector3 GetPosFormMatrix(Matrix4x4 m, Vector3 v)
        {
            Vector3 result = m.MultiplyPoint(v);
            return result;
        }
        private Matrix4x4 GetMatrix()
        {
            return Matrix4x4.Rotate(transform.localRotation);
        }
#if DOTWEEN
        DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> markTween;
#endif

        internal void OnPieEnter(int dataIndex)
        {
            mark.maskList.Add(dataIndex);
#if DOTWEEN
            markTween = DOTween.To(() => 0,
               x => mark.LerpValue = x,
               1f,
               hoverDuration)
               .SetEase(Ease.OutCubic);
#endif
        }

        internal void OnPieExit(int dataIndex)
        {
            mark.maskList.Remove(dataIndex);
#if DOTWEEN
            markTween.Kill();
            var text = mark.transform.Find(dataIndex.ToString()).GetComponent<TextMeshProUGUI>();
            text.color = text.color.SetAlpha(0);
#endif
            mark.LerpValue = 0;
        }
    }
}