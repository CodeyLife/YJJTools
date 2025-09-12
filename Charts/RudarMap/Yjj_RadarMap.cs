using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace YJJTool
{

    [RequireComponent(typeof(CanvasRenderer))]
    public class Yjj_RadarMap : Graphic
    {
        [FoldoutGroup("基础设置"), LabelText("数据")]
        public List<float> datas;
        #region 标题设置
        [FoldoutGroup("基础设置/标题"), LabelText("标题")]
        public List<string> names = new List<string>();
        [FoldoutGroup("基础设置/标题"), LabelText("标题字体")]
        public TMP_FontAsset titleFont;
        [FoldoutGroup("基础设置/标题"), LabelText("标题颜色")]
        public Color titelColor = Color.white;
        [FoldoutGroup("基础设置/标题"), LabelText("标题大小")]
        public float titleSize = 24;
        [FoldoutGroup("基础设置/标题"), LabelText("标题距离")]
        public float titleDistance = 5;
        #endregion
        [FoldoutGroup("基础设置")]
        public Color centerColor = Color.blue;
        [HorizontalGroup("基础设置/radiuSet")]
        public float radius = 100;
        [FoldoutGroup("基础设置")]
        private int max = 1000;
        [FoldoutGroup("基础设置"), LabelText("底图线层数")]
        public int radarCount = 4;
        [FoldoutGroup("基础设置"), LabelText("底图线颜色")]
        public Color radarLineColor = Color.gray;
        [FoldoutGroup("基础设置"), LabelText("底图线宽")]
        public float radarWidth = 1;
        //画线
        // public bool drawLine = true;
        [FoldoutGroup("基础设置")]
        public float line_width = 10;
        [FoldoutGroup("基础设置")]
        public Color line_color = Color.yellow;
        [FoldoutGroup("基础设置")]
        private Yjj_RadarLine line;
        //字体
        [FoldoutGroup("基础设置")]
        [LabelText("圆点sprite")]
        public Sprite sprite;
        [FoldoutGroup("基础设置")]
        [LabelText("圆点颜色"), ShowIf("@sprite!=null")]
        public Color sprite_color = Color.white;
        [FoldoutGroup("基础设置")]
        [LabelText("圆点大小"), ShowIf("@sprite!=null")]
        public Vector2 sprite_size = new Vector2(20, 20);
        [FoldoutGroup("基础设置")]
        [LabelText("文本大小")]
        public float text_size = 20;
        [FoldoutGroup("基础设置")]
        [LabelText("文本与中心距离")]
        public float text_distance = 5;
        [FoldoutGroup("基础设置")]
        [LabelText("文本颜色")]
        public Color text_Color = Color.white;
        //动画参数
        [FoldoutGroup("动画设置")]
        public float animation_easyIn_time = 2f;
        [FoldoutGroup("动画设置")]
        public AnimationCurve easyIn_Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [FoldoutGroup("动画设置")]
        public float easyIn_stayTime = 0.2f;
        #region inspector
#if UNITY_EDITOR
        [Button("设置随机数据")]
        private void TestSetData()
        {
            int length = Random.Range(4, 8);
            List<float> tests = new List<float>();
            var names = new List<string>();
            for (int i = 0; i < length; i++)
            {
                tests.Add(Random.Range(1, 100));
                names.Add($"属性{i + 1}");
            }
            SetData(tests,names);
        }
        [OnInspectorGUI]
        private void GuiChange()
        {
            if (transform.parent == null) return;
            if (GUI.changed)
            {
                StartCoroutine(YjjUtility.DeLay(() =>
                {
                    GeneratText();
                    SetLineValue();
                    Line.SetVerticesDirty();
                    SetVerticesDirty();
                }));
            }
        }
        [OnInspectorInit]
        private void Init()
        {
            if (transform.parent == null) return;
            GeneratText();
            SetLineValue();
            Line.SetVerticesDirty();
            SetVerticesDirty();
        }

#endif
        #endregion
        public Yjj_RadarLine Line
        {
            get
            {
                if (line == null)
                {
                    var rect = new GameObject("radarLine").AddComponent<RectTransform>();
                    rect.SetParent(transform);
                    rect.anchoredPosition = Vector2.zero;
                    rect.localScale = Vector3.one;
                    line = rect.gameObject.AddComponent<Yjj_RadarLine>();
                }
                return line;
            }
            set => line = value;
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="autoSetMaxData">是否自动计算最大值</param>
        /// <param name="maxData">如果不自动计算，该参数设置数据最大值</param>
        public void SetData(List<float> list, List<string> names = null)
        {
            dataValue = null;
            StopAllCoroutines();
            var newlist = new List<float>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                newlist.Add(list[i]);
            }
            if (names != null)
            {
                this.names = names;
            }
            datas = newlist;
            GeneratText();
            SetLineValue();
            Reflesh();
            if (gameObject.activeInHierarchy && Application.isPlaying)
            {
                PlayAnimation();
            }
        }
        protected override void Awake()
        {
            base.Awake();
            Line = GetComponentInChildren<Yjj_RadarLine>();

            SetLineValue();
            GeneratText();
        }
        protected override void OnEnable()
        {
            StopAllCoroutines();
            if (Application.isPlaying)
            {
                PlayAnimation();
            }
        }
        public void SetLineValue()
        {
            max = Yjj_ChartUtility.GetMaxData(datas.Max());
            Line.width = line_width;
            Line.datas = datas;
            Line.radius = radius;
            Line.max = max;
            Line.line_color = line_color;
            line.raycastTarget = false;
            Line.SetVerticesDirty();
        }
        #region 文本
        RectTransform[] spriteRTs;
        TextMeshProUGUI[] texts;
        public void GeneratText()
        {
            // Transform t = transform.Find("文本");
            Transform t = transform.GetOrCreatUIChild("文本", typeof(RectTransform));
            RectTransform rt = null;
            if (rt == null)
            {
                rt = t.rectTransform();
            }
            int count = rt.childCount;
            for (int i = 0; i < count; i++)
            {
                DestroyImmediate(rt.GetChild(0).gameObject);
            }
            spriteRTs = new RectTransform[datas.Count];
            texts = new TextMeshProUGUI[datas.Count];
            for (int j = 0; j < datas.Count; j++)
            {
                if (sprite != null)
                {
                    var temp = new GameObject(datas[j].ToString()).AddComponent<Image>();
                    temp.sprite = sprite;
                    temp.color = sprite_color;
                    temp.raycastTarget = false;
                    spriteRTs[j] = temp.rectTransform;
                    temp.rectTransform().sizeDelta = sprite_size;
                    temp.transform.SetParent(t);
                    temp.transform.localScale = Vector3.one;
                }
                var text = t.GetOrCreatUIChild<TextMeshProUGUI>($"data{j}");
                text.fontSize = text_size;
                text.color = text_Color;
                text.raycastTarget = false;
                text.alignment = TextAlignmentOptions.Center;
                texts[j] = text;
            }
            for (int i = 0; i < datas.Count; i++)
            {
                ComputeTextPosition(1, i);
            }

            //标题
            var title = transform.GetOrCreatUIChild<RectTransform>("标题");
            while (title.childCount > 0)
            {
                DestroyImmediate(title.GetChild(0).gameObject);
            }
            if (datas.Count == names.Count)
            {
                float angle = 360f / names.Count;
                for (int i = 0; i < names.Count; i++)
                {
                    var v = Vector3.up;
                    v = Quaternion.AngleAxis(angle * -i, Vector3.forward) * v * (radius + titleDistance);
                    var text = title.GetOrCreatUIChild<TextMeshProUGUI>($"标题{i}", (x) => x.raycastTarget = false);
                    text.font = titleFont;
                    text.color = titelColor;
                    text.fontSize = titleSize;
                    text.text = names[i];
                    text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
                    var rect = text.rectTransform;
                    if (v.x == 0)
                    {
                        text.alignment = TextAlignmentOptions.Center;
                        rect.pivot = new Vector2(0.5f, 0.5f);
                    }
                    else if (v.x < 0)
                    {
                        text.alignment = TextAlignmentOptions.MidlineRight;
                        rect.pivot = new Vector2(1, 0.5f);
                    }
                    else
                    {
                        text.alignment = TextAlignmentOptions.MidlineLeft;
                        rect.pivot = new Vector2(0, 0.5f);
                    }
                    text.rectTransform.anchoredPosition = v;
                }
            }
        }
        private void ComputeTextPosition(float t, int i)
        {
            float angle = 360 / datas.Count * Mathf.Deg2Rad;
            float thisAngle = angle * i;
            float value = datas[i];
            value = value / max * radius;
            Vector2 pos = new Vector2(Mathf.Sin(thisAngle) * value, Mathf.Cos(thisAngle) * value);
            if (spriteRTs[i] != null)
            {
                spriteRTs[i].anchoredPosition = pos;
                spriteRTs[i].sizeDelta = sprite_size * t;
            }
            var text = texts[i];
            text.rectTransform.anchoredPosition = pos.normalized * text_distance + pos;
            text.text = (datas[i] * t).ToString("f0");
            text.color = new Color(text_Color.r, text_Color.g, text_Color.b, t);

        }
        private void InitText()
        {
            foreach (var d in spriteRTs)
            {
                if (d == null) continue;
                d.anchoredPosition = Vector2.zero;
                d.sizeDelta = Vector2.zero;
            }
            foreach (var t in texts)
            {
                t.color = Color.clear;
            }
        }
        #endregion
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            DrawRadarLine(vh);
            ComputeData(vh, datas);
        }
        private void DrawRadarLine(VertexHelper vh)
        {

            int count = datas.Count;
            float angle = 360f / count;
            List<Vector3> dic = new List<Vector3>();
            //计算五个向量
            for (int i = 0; i <= count; i++)
            {
                var v = Vector3.up;
                v = Quaternion.AngleAxis(angle * -i, Vector3.forward) * v;
                dic.Add(v);
            }
            for (int i = 1; i <= radarCount; i++)
            {
                float value = i / (float)radarCount * radius;
                List<Vector2> list = new List<Vector2>();
                dic.ForEach(x =>
                {
                    list.Add(x * value);
                });
                Yjj_ChartUtility.DrawLineSmooth(vh, list, radarWidth, radarLineColor, true);
            }

        }
        private void ComputeData(VertexHelper vh, List<float> datas)
        {
            UIVertex v = new UIVertex();
            v.color = centerColor;
            v.uv0 = Vector2.zero;
            v.position = Vector3.zero;
            //  List<UIVertex> list = new List<UIVertex>();
            float angle = 360f / datas.Count;
            for (int i = 0; i < datas.Count; i++)
            {
                //  list.Add(v);
                vh.AddVert(v);
                UIVertex vertex = new UIVertex();
                vertex.color = centerColor;
                float value = datas[i] / max * radius;
                float currentAngle = angle * i;
                currentAngle *= Mathf.Deg2Rad;
                float cos = Mathf.Cos(currentAngle);
                float sin = Mathf.Sin(currentAngle);
                vertex.position = new Vector3(sin * value, cos * value, 0);
                vertex.uv0 = new Vector2(1, 1);
                vertex.color = centerColor;
                //  list.Add(vertex);
                vh.AddVert(vertex);
                if (i < datas.Count - 1)
                {
                    vertex = new UIVertex();
                    vertex.color = centerColor;
                    value = datas[i + 1] / max;
                    currentAngle = angle * (i + 1);
                    currentAngle *= Mathf.Deg2Rad;
                    cos = Mathf.Cos(currentAngle);
                    sin = Mathf.Sin(currentAngle);
                    vertex.position = new Vector3(sin * radius * value, cos * radius * value, 0);
                    vertex.uv0 = new Vector2(1, 1);
                    vertex.color = centerColor;
                    // list.Add(vertex);
                    vh.AddVert(vertex);
                }
                else
                {
                    vertex = new UIVertex();
                    vertex.color = centerColor;
                    value = datas[0] / max;
                    currentAngle = 0;
                    currentAngle *= Mathf.Deg2Rad;
                    cos = Mathf.Cos(currentAngle);
                    sin = Mathf.Sin(currentAngle);
                    vertex.position = new Vector3(sin * radius * value, cos * radius * value, 0);
                    vertex.uv0 = new Vector2(1, 1);
                    vertex.color = centerColor;
                    // list.Add(vertex);
                    vh.AddVert(vertex);
                }
                int index = vh.currentVertCount - 1;
                vh.AddTriangle(index - 2, index - 1, index);
            }

            //  vh.AddUIVertexTriangleStream(list);
        }

        #region 动画
        public void PlayAnimation()
        {
            StopAllCoroutines();
            if (dataValue == null || dataValue.Length != datas.Count)
            {
                dataValue = new float[datas.Count];
                for (int i = 0; i < datas.Count; i++)
                {
                    dataValue[i] = datas[i];
                }
            }
            for (int i = 0; i < datas.Count; i++)
            {
                datas[i] = 0;
            }
            InitText();
            StartCoroutine(WatiToPlay());
        }
        //oldValue
        float[] dataValue;
        IEnumerator WatiToPlay()
        {
            int index = 0;
            StartCoroutine(EsayIn(index));
            index++;
            while (index < datas.Count)
            {
                yield return new WaitForSeconds(easyIn_stayTime);
                StartCoroutine(EsayIn(index));
                index++;
            }

        }
        IEnumerator EsayIn(int index)
        {
            float current = 0;
            float t;
            while (current < animation_easyIn_time)
            {
                current += Time.deltaTime;
                t = current / animation_easyIn_time;
                t = Mathf.Clamp(t, 0, 1);
                t = easyIn_Curve.Evaluate(t);
                datas[index] = Mathf.Lerp(0, dataValue[index], t);
                ComputeTextPosition(t, index);
                Reflesh();
                yield return null;
            }
        }
        #endregion

        private void Reflesh()
        {
            line.SetVerticesDirty();
            SetVerticesDirty();
            SetMaterialDirty();
        }

        //刷新显示
        protected override void OnDisable()
        {
            base.OnDisable();
            StopAllCoroutines();
        }

    }
}