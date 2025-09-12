using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace YJJTool
{
    [System.Serializable]
    public class HoverSet
    {
        [LabelText("开启hover功能")]
        public bool active = true;
        [Header("弹窗基于鼠标偏移"), ShowIf("active")]
        public Vector2 offset = Vector2.zero;

        [ShowIf("active"), LabelText("hover时图表缩放系数"), Title("hover效果设置，某些图表类型部分参数没有用", TitleAlignment = TitleAlignments.Centered)]
        public float hoverScale = 1.2f;
        [ShowIf("active"), LabelText("hover改变颜色")]
        public Color hoverColor = Color.yellow;
        [ShowIf("active"), Header("hover时对应位置显示的垂直线")]
        public RectTransform hoverRect;

        [Title("如果UI相机不是MainCam需要手动设置")]
        public Camera uicamera;
        [LabelText("弹窗根节点"), ShowIf("active")]
        public Transform root;
        [ShowIf("active"), Header("用于接收并显示数值的文本")]
        public List<TextMeshProUGUI> valueTextList = new List<TextMeshProUGUI>();
        [ShowIf("active"), Header("用于显示标题的文本")]
        public TextMeshProUGUI nameText;

#if UNITY_EDITOR
        [Title("自动生成设置", TitleAlignment = TitleAlignments.Centered)]
        public TMP_FontAsset font;
        [Title("数据维度取数据个数（如果false取第0个数据量）")]
        public bool wideData = true;
#endif

        Func<int, List<string>> GetDataFunc;  //获取value 方法
        Action<int> ExitAction;
        Func<int, string> GetNameFunc;

        #region Inspector
#if UNITY_EDITOR
        [Button("清理hover弹窗所有节点raycast", ButtonHeight = 20), GUIColor(0, 1, 0)]
        private void ClearAllRaycast()
        {
            GetAndSet(root);
            foreach (Transform t in root)
            {
                GetAndSet(t);
            }
            if (hoverRect != null)
            {
                hoverRect.GetComponent<Image>().raycastTarget = false;
            }
        }
        private void GetAndSet(Transform t)
        {
            var image = t.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
                return;
            }
            var textmeshpro = t.GetComponent<TextMeshProUGUI>();
            if (textmeshpro != null)
            {
                textmeshpro.raycastTarget = false;
            }
        }
        [Button("生成默认弹窗"), ShowIf("@root == null"), GUIColor(0, 1, 0)]
        private void GenerateDefault()
        {
            valueTextList.Clear();
            Transform root = UnityEditor.Selection.activeGameObject.transform;
            var go = new GameObject("hoverWindow", new Type[] { typeof(Image), typeof(VerticalLayoutGroup) });
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            go.transform.SetParent(root, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.18f, 0.23f, 0.30f, 0.8f);
            image.raycastTarget = false;
            this.root = go.transform;
            var chart = root.GetComponent<ChartBase>();
            Type t = chart.GetType();
            var f = t.GetField("datas");
            var values = f.GetValue(chart);
            if (values.GetType() == typeof(List<MultipleData>))
            {
                var datas = (List<MultipleData>)values;
                if (wideData)
                {
                    GenerateText(go.transform, datas.Count);
                }
                else
                {
                    GenerateText(go.transform, datas[0].datas.Count);
                }
            }
            else
            {
                GenerateText(go.transform, 1);
            }
            go.transform.localPosition = Vector2.zero;

            //自动生成文本颜色
            var colorsF = t.GetField("colorList");
            //if(colorsF == null)
            //{
            //    colorsF = t.GetField("colors");
            //}
            if (colorsF != null)
            {
                var list = (List<Color>)colorsF.GetValue(chart);
                for (int i = 0; i < valueTextList.Count; i++)
                {
                    valueTextList[i].color = list[i];
                }
            }
            ClearAllRaycast();
        }
        private void GenerateText(Transform root, int count)
        {
            var rect = root.rectTransform();
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("值文本" + i.ToString(), typeof(TextMeshProUGUI));
                go.transform.SetParent(root, false);
                var text = go.GetComponent<TextMeshProUGUI>();
                text.rectTransform.sizeDelta = new Vector2(rect.sizeDelta.x, 20);
                text.fontSize = 12;
                text.text = "default文本" + i.ToString();
                text.raycastTarget = false;
                text.alignment = TextAlignmentOptions.Center;
                if (font != null)
                {
                    text.font = font;
                }
                valueTextList.Add(text);
            }
        }
#endif
        #endregion
        public void SetHover(Transform transform, BaseSet baseSet, DataSet dataSet, int dataCount, Func<int, List<string>> func, Action<int> exit = null, Func<int, string> getName = null)
        {
            if (!active || root == null)
            {
                //清楚现有效果
                var d = transform.Find("hoverRoot");
                if (d != null)
                {
                    UnityEngine.Object.DestroyImmediate(d.gameObject);
                }
                return;
            }
            GetDataFunc = func;
            ExitAction = exit;
            GetNameFunc = getName;
            float width = baseSet.width;
            float height = baseSet.hight;
            width = width - dataSet.distanceFormLeft - dataSet.distanceFormRight;
            float length = width / (dataCount - 1);
            //  Debug.Log(string.Format("总宽:{0},减去左右:{1},单个宽度;{2}~左右测距离{3},{4}", baseSet.width, height, length, dataSet.distanceFormLeft, dataSet.distanceFormRight));
            //hover节点
            var hover = transform.GetOrCreatUIChild("hoverRoot", typeof(RectTransform)).rectTransform();
            hover.anchorMin = Vector2.zero;
            hover.anchorMax = Vector2.zero;
            hover.pivot = Vector2.zero;
            hover.anchoredPosition = Vector2.zero;
            hover.transform.DelateAllChild();

            for (int i = 0; i < dataCount; i++)
            {
                var go = hover.GetOrCreatUIChild(i.ToString(), new System.Type[] { typeof(ChartHoverFunction), typeof(Image) }).GetComponent<RectTransform>();
                go.anchorMin = Vector2.zero;
                go.anchorMax = Vector2.zero;
                go.pivot = new Vector2(0.5f, 0);
                go.sizeDelta = new Vector2(length, height);
                go.anchoredPosition = new Vector2(i * length + dataSet.distanceFormLeft, 0);
                go.GetComponent<Image>().color = new Color(1, 1, 1, 0);
                var function = go.GetComponent<ChartHoverFunction>();
                function.index = i;
                function.ExitEvent.RemoveAllListeners();
                function.ExitEvent.AddListener(HoverExit);
                function.EnterEvent.RemoveAllListeners();
                function.EnterEvent.AddListener(HoverEnter);
            }
            if (root != null)
            {
                var hoverRect = root.rectTransform();
                hoverRect.anchorMin = Vector2.zero;
                hoverRect.anchorMax = Vector2.zero;
                hoverRect.pivot = Vector2.zero;
                hoverRect.anchoredPosition = offset;
                if (Application.isPlaying)
                {
                    root.gameObject.SetActive(false);
                }
            }
            if (hoverRect != null)
            {
                hoverRect.sizeDelta = new Vector2(hoverRect.sizeDelta.x, baseSet.hight);
                hoverRect.gameObject.SetActive(false);
            }
        }
        private bool isStay = false;
        public void Updata()
        {
            if (!isStay)
            {
                return;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRect, Input.mousePosition, uicamera, out var local);
            //  Vector2 delta = (ParentRect.pivot - Vector2.one * 0.5f);
            Rect.anchoredPosition = local + offset;
        }
        RectTransform parentRect;
        RectTransform _rect;
        public RectTransform ParentRect
        {
            get
            {
                if (parentRect == null)
                {
                    parentRect = root.parent.rectTransform();
                }
                return parentRect;
            }
            set => parentRect = value;
        }

        public RectTransform Rect
        {
            get
            {
                if (_rect == null)
                {
                    _rect = root.rectTransform();
                }
                return _rect;
            }
            set => _rect = value;
        }
        private int lastIndex = -1;
        protected void HoverExit()
        {
            // Debug.Log("鼠标退出");
            isStay = false;
            root.gameObject.SetActive(false);
            ExitAction?.Invoke(lastIndex);
            lastIndex = -1;
            if (hoverRect != null)
            {
                hoverRect.gameObject.SetActive(false);
            }
        }
        protected void HoverEnter(int index, Vector2 pos)
        {
            isStay = true;
            if (lastIndex > -1)
            {
                ExitAction?.Invoke(lastIndex);
            }
            lastIndex = index;
            var datas = GetDataFunc(index);
            if (GetNameFunc != null && nameText != null)
            {
                nameText.text = GetNameFunc.Invoke(index);
            }
            root.gameObject.SetActive(true);
            for (int i = 0; i < valueTextList.Count; i++)
            {
                valueTextList[i].text = datas[i];
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRect, pos, uicamera, out var local);
            //Debug.Log($"{index}:{pos}:{ParentRect}:{local}");
            //   Vector2 delta = (ParentRect.pivot - Vector2.one*0.5f);
            //   Rect.anchoredPosition = local+offset + new Vector2(delta.x * ParentRect.sizeDelta.x,delta.y* ParentRect.sizeDelta.y);
            Rect.anchoredPosition = local + offset;
            //改变标注位置
            if (hoverRect != null)
            {
                hoverRect.gameObject.SetActive(true);
                hoverRect.SetParent(ParentRect.Find("hoverRoot/" + index));
                hoverRect.anchoredPosition = Vector2.zero;
            }
        }

    }
}