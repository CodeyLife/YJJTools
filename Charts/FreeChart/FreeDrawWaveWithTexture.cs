using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace YJJTool
{
    public class FreeDrawWaveWithTexture : DrawFreeChartBase
    {
        #region 参数
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
        [FoldoutGroup("数据设置")]
        [LabelText("背景底图")]
        public Texture2D background;
        [ShowIf("@background != null")]
        public Material baseMaterial;
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


        protected Vector2[] arr;

        protected List<Vector2> list;
        protected List<Vector2> texList;
        protected Texture2D tex;
        #endregion
        public override void SetGraph(FreeChart root)
        {
            base.SetGraph(root);
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

            if (tex == null || tex.width != texturePix.x || tex.height != texturePix.y)
            {
                tex = new Texture2D(width, height);
            }
            //计算数据
            RemathList(list, ref texList, root.set.width, root.set.hight);
            DrawImage(tex, texList, lineSet.isCurve, smooth);
            image.texture = tex;
            //画线
            var line = transform.GetOrCreatUIChild("line", true, (typeof(Yjj_Line))).GetComponent<Yjj_Line>();
            line.SetGraph(list, lineSet);
            if (background != null && baseMaterial != null)
            {
                var m = new Material(baseMaterial);
                m.SetTexture("_BaseTex", background);
                image.material = m;
            }
            //  Test();
        }
        private void RemathList(List<Vector2> list, ref List<Vector2> outList, float width, float height)
        {
            float w = tex.width / width;
            float h = tex.height / height;
            outList = new List<Vector2>();
            for (int i = 0; i < list.Count; i++)
            {
                //list[i] = new Vector2(list[i].x * w, list[i].y * h);
                outList.Add(new Vector2(list[i].x * w, list[i].y * h));
            }
        }
        private void DrawImage(Texture2D tex, List<Vector2> posints, bool isCurve, int smooth = 8)
        {
            Color[] colors = new Color[tex.width * tex.height];
            int width = tex.width;
            int height = tex.height;
            List<Vector2> curves = Yjj_ChartUtility.GetCurvePosFroJob(posints, posints.Count * smooth);
            //i为Y轴，j为x轴
            for (int i = 0; i < height; i++)
            {
                int index = 0;
                for (int j = 0; j < width; j++)
                {
                    if (j > curves[index].x && index < curves.Count - 1)
                    {
                        index++;
                    }
                    float y = index > 0 ? Mathf.Lerp(curves[index - 1].y, curves[index].y, (curves[index].x - curves[index - 1].x) / (j - curves[index - 1].x)) : 0;
                    float a = i > y ? 0 : 1;
                    a = a == 1 ? Mathf.Lerp(minA, maxA, i / y) : 0;
                    colors[i * width + j] = data_lineColor.SetAlpha(a);
                }

            }
            tex.SetPixels(colors);
            tex.Apply();
        }

    }
}