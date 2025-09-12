using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;
using TMPro;
namespace YJJTool
{
    public class UIMultiBar : MonoBehaviour
    {
        [FoldoutGroup("基础设置")]
        [Title("基础设置")]
        public UIMultiBarSetting set = new UIMultiBarSetting();
        [FoldoutGroup("基础设置")]
        [Title("柱状图sprite")] public Sprite sprite;
        [FoldoutGroup("颜色设置")]
        public List<Color> colorList = new List<Color>();
        [FoldoutGroup("标题设置")]
        public List<string> titleList = new List<string>();
        [FoldoutGroup("数据设置")]
        [Title("数据设置")]
        public List<UIMultiBarData> data = new List<UIMultiBarData>();
        [FoldoutGroup("动画设置")]
        public AnimationSet animationSet = new AnimationSet();
        [FoldoutGroup("动画设置")]
        public float delayOffset = 0.005f;
        private Transform hor;
        protected List<GameObject> hors = new List<GameObject>();
        protected List<GameObject> titleTexts = new List<GameObject>();
        protected List<GameObject> valueTexts = new List<GameObject>();
        protected List<List<GameObject>> bars = new List<List<GameObject>>();
        protected int num;
        protected float maxData;
        protected float maxHorData;

        private void OnEnable()
        {
            SetGraph();
            PlayAnimation();
        }

        public void SetGraph()
        {
            num = data.Count;
            Clear();

            if (data.Count == 0)
            {
                return;
            }

            List<UIMultiBarData> tmps = data.OrderByDescending(d => d.dataList.Sum()).ToList();
            UIMultiBarData tmp = tmps.FirstOrDefault();

            for (int i = 0; i < num; i++)
            {
                var title = transform.GetOrCreatUIChild("title" + i, typeof(RectTransform)).GetComponent<RectTransform>();
                var value = transform.GetOrCreatUIChild("value" + i, typeof(RectTransform)).GetComponent<RectTransform>();

                hor = transform.GetOrCreatUIChild("HorLayout" + i, typeof(RectTransform)).GetComponent<RectTransform>();
                hor.rectTransform().pivot = new Vector2(0, 0.5f);
                hor.rectTransform().anchorMax = new Vector2(0, 0.5f);
                hor.rectTransform().anchorMin = new Vector2(0, 0.5f);
                hor.rectTransform().anchoredPosition = new Vector2(0, -set.spaceHight * i);

                title.rectTransform().anchoredPosition = new Vector2(0 - set.titlefontOffsetX, -set.spaceHight * i - set.titlefontOffsetY);
                value.rectTransform().anchoredPosition = new Vector2(0 + set.valueFontOffsetX, -set.spaceHight * i - set.valueFontOffsetY);


                TextMeshProUGUI t = title.GetOrAddComponent<TextMeshProUGUI>();
                TextMeshProUGUI v = value.GetOrAddComponent<TextMeshProUGUI>();
                title.GetComponent<TextMeshProUGUI>().fontSize = set.titleFontSize;
                value.GetComponent<TextMeshProUGUI>().fontSize = set.valueFontSize;
                title.GetComponent<TextMeshProUGUI>().color = set.titleFontColor;
                value.GetComponent<TextMeshProUGUI>().fontSize = set.valueFontSize;
                value.GetComponent<TextMeshProUGUI>().color = set.valueFontColor;
                title.GetComponent<TextMeshProUGUI>().font = set.font;
                value.GetComponent<TextMeshProUGUI>().font = set.font;

                if (set.font == null)
                {
                    title.GetComponent<TextMeshProUGUI>().UpdateFontAsset();
                    value.GetComponent<TextMeshProUGUI>().UpdateFontAsset();
                }

                HorizontalLayoutGroup layout = hor.GetOrAddComponent<HorizontalLayoutGroup>();
                ContentSizeFitter fit = hor.GetOrAddComponent<ContentSizeFitter>();
                layout.childControlHeight = false;
                layout.childControlWidth = false;
                layout.spacing = set.space;
                fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                hor.rectTransform().sizeDelta = new Vector2(hor.rectTransform().sizeDelta.x, set.hight);
                hors.Add(hor.gameObject);


                if (i <= (titleList.Count - 1))
                {
                    t.text = titleList[i];
                }
                v.text = data[i].dataList.Sum() + "";

                titleTexts.Add(title.gameObject);
                valueTexts.Add(value.gameObject);
            }

            float calcMax = tmp.dataList.Sum();
            maxHorData = calcMax;
            for (int i = 0; i < data.Count; i++)
            {
                List<float> dataList = data[i].dataList;
                List<GameObject> barList = new List<GameObject>();

                for (int j = 0; j < dataList.Count; j++)
                {
                    var bar = hors[i].transform.GetOrCreatUIChild("bar" + j, typeof(RectTransform)).GetComponent<RectTransform>();

                    bar.anchorMin = Vector2.zero;
                    bar.anchorMax = Vector2.zero;
                    bar.pivot = Vector2.zero;

                    float percent = (dataList[j] / dataList.Sum()) * (dataList.Sum() / calcMax);
                    bar.sizeDelta = new Vector2(percent * set.tolWidth, set.hight);
                    bar.anchoredPosition = Vector2.zero;

                    Image img = bar.GetOrAddComponent<Image>();
                    if (sprite != null)
                    {
                        img.sprite = sprite;
                    }
                    if (j <= (colorList.Count - 1))
                    {
                        img.color = colorList[j];
                    }
                    barList.Add(bar.gameObject);
                }
                bars.Add(barList);
            }
        }


        public void PlayAnimation()
        {
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }
        protected virtual IEnumerator FadeIn()
        {
            hor.gameObject.SetActive(false);

            float delay = 0.01f;

            foreach (var hor in hors)
            {
                hor.gameObject.SetActive(false);
            }
            foreach (var t in titleTexts)
            {
                t.gameObject.SetActive(false);
            }
            foreach (var v in valueTexts)
            {
                v.gameObject.SetActive(false);
            }
            foreach (var hor in hors)
            {
                hor.gameObject.SetActive(false);
            }

            foreach (var bar in bars)
            {
                foreach (var item in bar)
                {
                    item.gameObject.SetActive(false);
                    item.transform.rectTransform().sizeDelta = new Vector2(0, set.hight);
                }
            }
            for (int i = 0; i < data.Count; i++)
            {
                yield return new WaitForSeconds(delay);

                hors[i].gameObject.SetActive(true);
                for (int j = 0; j < data[i].dataList.Count; j++)
                {
                    bars[i][j].gameObject.SetActive(true);
                }
                delay += delayOffset;
            }

            yield return StartCoroutine(YjjUtility.FadeIn(animationSet.fadeInTime, (t) =>
            {
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data[i].dataList.Count; j++)
                    {
                        float anim = (data[i].dataList[j] / data[i].dataList.Sum()) * (data[i].dataList.Sum() / maxHorData);
                        bars[i][j].transform.rectTransform().sizeDelta = Vector2.Lerp(bars[i][j].transform.rectTransform().sizeDelta, new Vector2(anim * set.tolWidth, set.hight), t);
                        bars[i][j].gameObject.SetActive(true);
                        titleTexts[i].gameObject.SetActive(true);
                        valueTexts[i].gameObject.SetActive(true);
                    }
                }
            }));
        }
        public virtual void SetData(List<UIMultiBarData> bData, List<string> titles)
        {
            StopAllCoroutines();
            data = bData;
            titleList = titles;
            SetGraph();
            PlayAnimation();
        }

        protected void Clear()
        {
            foreach (var bar in bars)
            {
                foreach (var item in bar)
                {
                    DestroyImmediate(item.gameObject);
                }
            }
            foreach (var hor in hors)
            {
                DestroyImmediate(hor.gameObject);
            }
            foreach (var t in titleTexts)
            {
                DestroyImmediate(t.gameObject);
            }
            foreach (var v in valueTexts)
            {
                DestroyImmediate(v.gameObject);
            }
            hors.Clear();
            bars.Clear();
            titleTexts.Clear();
            valueTexts.Clear();
        }
        protected IEnumerator SetNextFramePlay()
        {
            yield return new WaitForSeconds(0.01f);
            SetGraph();
        }
        [OnInspectorGUI]
        private void Init()
        {
#if UNITY_EDITOR
            if (transform.parent == null)
            {
                return;
            }
            if (GUI.changed)
            {
                StopAllCoroutines();
                StartCoroutine(SetNextFramePlay());
            }
#endif

            if (Application.isPlaying)
            {
                return;
            }
        }

    }
}