using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace YJJTool
{
    public class Yjj_3dBarGraph : ChartBase
    {
        #region Fields
        [FoldoutGroup("基础设置")]
        [HideLabel]
        public BaseSet set = new BaseSet();
        [FoldoutGroup("基础设置")]
        public HoverSet hoverSet = new HoverSet();
        [FoldoutGroup("数据设置")]
        [Title("数据标题设置")]
        public DataSet dataSet = new DataSet();
        [FoldoutGroup("数据设置")]
        public Yjj_3dBarDrawer.Bar3DSet barSet = new Yjj_3dBarDrawer.Bar3DSet();
        [FoldoutGroup("数据设置")]
        [Title("柱状图宽度")] public float barWidth = 20;
        [FoldoutGroup("数据设置")]
        [Title("柱状图间距")] public float distance = 20;
        //  public LineSet lineSet = new LineSet();
        [FoldoutGroup("数据设置")]
        [Title("数据")]
        public List<MultipleData> datas = new List<MultipleData>();
        [FoldoutGroup("数据设置")]
        [Title("柱状图颜色")] public List<Color> colorList = new List<Color>();
        [FoldoutGroup("动画设置")]
        public AnimationSet animationSet = new AnimationSet();
        [FoldoutGroup("动画设置")]
        [Title("是否开启循环动画")] public bool openLoop = true;

        [FoldoutGroup("数据设置")]
        public bool openDataText = false;
        [FoldoutGroup("数据设置"), ShowIf("openDataText")]
        public bool showUnit = false;
        [FoldoutGroup("数据设置"), ShowIf("openDataText")]
        public float textSize = 32;
        [FoldoutGroup("数据设置"), ShowIf("openDataText")]
        public Vector2 textOffset = Vector2.zero;
        [FoldoutGroup("数据设置"), ShowIf("openDataText"), LabelText("保留几位小数")]
        public int textEnd = 0;
        [FoldoutGroup("数据设置"), ShowIf("openDataText")]
        public TMP_FontAsset textFont;
        [FoldoutGroup("数据设置"), ShowIf("openDataText")]
        public Color textColor = Color.white;

        #endregion
        public override void Awake()
        {
            base.Awake();
        }
        public override void OnEnable()
        {
            base.OnEnable();
        }
        public void Update()
        {
            hoverSet.Updata();
        }
        /// <summary>
        /// 数据类型List<MultipleData>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="names">数据标题名</param>
        public override void SetData(List<MultipleData> data, List<string> names)
        {
            base.SetData(data, names);
            datas = data;
            if (names != null)
            {
                dataSet.names = names;
            }
            all = 0;
            SetGraph(true);
        }
        public override void SetGraph()
        {
            base.SetGraph();
            //删除多余的bar
            int barIndex = datas.Count;
            var barDeleta = transform.Find("bar" + barIndex);
            while (barDeleta != null)
            {
                DestroyImmediate(barDeleta.gameObject);
                barIndex++;
                barDeleta = transform.Find($"bar{barIndex}'");
            }
            for (int i = colorList.Count; i < datas.Count; i++)
            {
                colorList.Add(Color.white);
            }
            RectTransform rect = GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(set.width, set.hight);
            rect.pivot = Vector2.zero;
            //设置标尺
            SetMaxAndMinForTwoData(set, datas);
            //基础图表绘制
            var gp = transform.GetOrCreatUIChild<Yjj_GraphPopulateMeshBase>("base", (b) =>
             {
                 var br = b.rectTransform;
                 br.anchorMin = Vector2.zero;
                 br.anchorMax = Vector2.zero;
                 br.pivot = Vector2.zero;
                 br.anchoredPosition = Vector2.zero;
             });
            gp.SetGraph(set, dataSet);
            if (datas.Count <= 0)
            {
                return;
            }

            //获取数据点
            List<List<Vector2>> dataList = new List<List<Vector2>>();
            for (int i = 0; i < datas.Count; i++)
            {
                dataList.Add(Yjj_ChartUtility.GetPosFromData(datas[i].datas, set, dataSet, 0, true, false));
            }

            //柱状图
            all = GetCount(dataList);
            var temps = new List<List<Vector2>>();
            var bardraw = transform.GetOrCreatUIChild<RectTransform>("3Dbar", (bar3d) =>
             {
                 bar3d.anchorMin = Vector2.zero;
                 bar3d.anchorMax = Vector2.zero;
                 bar3d.pivot = Vector2.zero;
                 bar3d.sizeDelta = Vector2.zero;
             });
            for (int i = 0; i < dataList.Count; i++)
            {
                var list = new List<Vector2>();
                for (int j = 0; j < dataList[i].Count; j++)
                {
                    float x = dataList[i][j].x + ((i - all)) * distance;
                    var pos = new Vector2(x, dataList[i][j].y);
                    list.Add(pos);
                    var bar = bardraw.GetOrCreatUIChild<Yjj_3dBarDrawer>($"bar{i}{j}", (c) =>
                     {
                         var cr = c.rectTransform;
                         cr.pivot = Vector2.zero;
                     });
                    bar.rectTransform.anchoredPosition = new Vector2(x, 0);
                    bar.transform.localEulerAngles = new Vector3(0, barSet.rotation, 0);
                    bar.material = barSet.mat;
                    bar.SetGraph(barSet, pos, i);
                    //生成数据文本
                    //#region 生产数据文本
                    //if (openDataText)
                    //{
                    //    var text = b.GetOrCreatUIChild("dataText", typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
                    //    var textRect = text.rectTransform;
                    //    textRect.anchorMin = new Vector2(0.5f, 1);
                    //    textRect.anchorMax = new Vector2(0.5f, 1);
                    //    textRect.pivot = new Vector2(0.5f, 0);
                    //    textRect.sizeDelta = new Vector2(60, 25);
                    //    text.enableWordWrapping = false;
                    //    textRect.anchoredPosition = textOffset;
                    //    text.font = textFont; text.alignment = TextAlignmentOptions.Center;
                    //    text.fontSize = textSize;
                    //    text.color = textColor;
                    //    var textContent = datas[i].datas[j].ToLimitString(textEnd);
                    //    if (showUnit)
                    //    {
                    //        textContent += set.rulerSet[0].unit;
                    //    }
                    //    text.text = textContent;
                    //}
                    //else
                    //{
                    //    if (b.childCount > 0)
                    //    {
                    //        DestroyImmediate(b.GetChild(0).gameObject);
                    //    }
                    //}
                    //#endregion
                }
                temps.Add(list);
            }
            //  bardraw.SetGraph(barSet, temps);
            if (openDataText)
            {
                CheckTextOverlap();
            }

            hoverSet.SetHover(transform, set, dataSet, datas[0].datas.Count, (index) =>
            {
                List<string> values = new List<string>();
                for (int i = 0; i < datas.Count; i++)
                {
                    values.Add(datas[i].datas[index].ToString());
                    var go = transform.Find("bar" + i);
                    go = go.GetChild(index);
                    go.localScale = Vector3.one * hoverSet.hoverScale;
                }
                return values;
            }, (index) =>
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    var go = transform.Find("bar" + i);
                    go = go.GetChild(index);
                    go.localScale = Vector3.one;
                }
            }, (index) =>
            {
                string name = dataSet.names[index];
                return name;
            });

        }
        protected float all = 0;
        protected float GetCount(List<List<Vector2>> list)
        {
            float v = 0;
            for (int i = 0; i < list.Count; i++)
            {
                v++;
            }
            v = (v - 1) * 0.5f;
            v = Mathf.Clamp(v, 0, float.MaxValue);
            return v;
        }


        /// <summary>
        /// 检查标注重叠
        /// </summary>
        private void CheckTextOverlap()
        {
            //去重叠
            //try
            //{
            //    for (int i = 0; i < dataList[0].Count; i++)
            //    {
            //        var rects = new List<RectTransform>();
            //        for (int j = 0; j < dataList.Count; j++)
            //        {
            //            rects.Add(transform.Find($"bar{j}").Find($"data{i}").Find("dataText").rectTransform());
            //        }
            //        //rects.ForEach(x => Debug.Log(x, x.gameObject));
            //        CheckOverlap(rects);
            //    }
            //}
            //catch { }
        }
        /// <summary>
        /// 检查重叠并做出调整
        /// </summary>
        /// <param name="rects"></param>
        private void CheckOverlap(List<RectTransform> rects)
        {
            float middel = (rects.Count - 1) * 0.5f;
            bool isOver = false;
            for (int i = 0; i < rects.Count - 1; i++)
            {
                if (rects[i].CheckOverlap(rects[i + 1]))
                {
                    isOver = true;
                    //Debug.Log("重叠", rects[i].gameObject);
                    //Debug.Log("第二个", rects[i + 1].gameObject);
                    break;
                }
            }
            if (isOver)
            {
                for (int i = 0; i < rects.Count; i++)
                {
                    rects[i].anchoredPosition += (i - middel) * Vector2.right * 5;
                }
                CheckOverlap(rects);
            }
        }
    }
}