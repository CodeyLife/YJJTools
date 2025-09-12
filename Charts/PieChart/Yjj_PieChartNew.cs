using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using YJJTool;



[RequireComponent(typeof(CanvasRenderer))]
public class Yjj_PieChartNew : Graphic,IPointerMoveHandler,IPointerEnterHandler,IPointerExitHandler
{
    #region 枚举定义
    public enum TitleType
    {
        [LabelText("不显示")]
        不显示,
        [LabelText("显示数据")]
        显示数据,
        [LabelText("显示标题")]
        显示标题,
        [LabelText("显示标题和数据")]
        显示标题和数据
    }
    #endregion

    #region 基础数据配置

    [HorizontalGroup("hor")]
    public List<float> datas = new List<float>();
    [HorizontalGroup("hor")]
    public List<string> names = new List<string>();
#if UNITY_EDITOR
    [ListDrawerSettings(CustomAddFunction = "AddColor")]
#endif
    [LabelText("颜色列表")]
    public List<Color> colors = new List<Color>();
    



    [LabelText("间隔角度")]
    public float distanceAngle = 0.1f;
    
    [LabelText("宽度")]
    public float width = 10;
    
    [LabelText("细分"), MinValue(24)]
    public int smooth = 60;
    
    [LabelText("起始角度")]
    public float startAngle = 90;
    
    [LabelText("圆角半径")]
    public float roundRadiu = 1;
    
    [ReadOnly, LabelText("半径")]
    public float radius = 0;
    
    private bool setAwake = true;
    #endregion

    #region 背景设置
    [Title("背景设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("绘制底板(用最后一个color做底板颜色)")]
    public bool drawBackGround = false;
    
    [ShowIf("drawBackGround"), LabelText("背景细分")]
    public int backGroundSmooth = 36;
    #endregion

    #region 交互设置
    [Title("交互设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("开启悬停")]
    public bool openHover = true;
    
    [ShowIf("openHover"), LabelText("UI相机")]
    public Camera uicamera;
    #endregion

#if UNITY_EDITOR
    private void AddColor()
    {
        UnityEditor.Undo.RecordObject(this, "property");
        colors.Add(Color.white);
    }
#endif

    #region 画线设置
    [Title("画线设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("启用画线")]
    public bool drawLine = true;
    
    [ShowIf("drawLine"), LabelText("线条宽度")]
    public float lineWidth = 1;
    
    [ShowIf("drawLine"), LabelText("线条颜色")]
    public Color lineColor = Color.grey;
    
    [ShowIf("drawLine"), LabelText("线条偏移")]
    public Vector2 lineOffset = new Vector2(20, 20);
    
    [ShowIf("drawLine"), LabelText("线条长度")]
    public float lineLength = 50;
    
    [HideIf("drawLine"), LabelText("文本居中")]
    public bool textInCenter = false;
    #endregion
    #region 文本设置
    [Title("文本设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("选择显示内容"), EnumToggleButtons]
    public TitleType textType = 0;

    [ShowIf("@textType == TitleType.显示标题和数据"), LabelText("标题大小")]
    public float titleSize = 15;
    
    [ShowIf("@textType == TitleType.显示标题和数据"), LabelText("标题颜色")]
    public Color titleColor = Color.white;

    [LabelText("开启数据文本颜色")]
    public bool valueTextColorFollowSprite = false;
    
    [LabelText("数据小数位数")]
    public int floatCount = 0;
    
    [LabelText("文本距离中心的距离"), HideIf("drawLine")]
    public float textDistance = 20;
    
    [HideIf("valueTextColorFollowSprite"), LabelText("文本颜色")]
    public Color text_color = Color.white;
    
    [ShowIf("@textType == TitleType.显示标题"), LabelText("标题颜色")]
    public Color dataColor = Color.white;
    
    [LabelText("文本大小")]
    public float text_size = 20;
    
    [LabelText("字体")]
    public TMP_FontAsset font;
    
    [LabelText("是否显示单位")]
    public bool showUnit = false;
    
    [LabelText("单位"), ShowIf("showUnit")]
    public string unit;
    #endregion
    #region 图例设置
    [Title("图例设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("启用图例")]
    public bool enableLegend = false;
    [ShowIf("enableLegend")]
    [LabelText("图例里是否显示数据")]
    public bool legendWithData = true;
    [ShowIf("enableLegend"),InlineEditor]
    public Yjj_LegendConfig config;
    #endregion
    #region 动画设置
    [Title("动画设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("启用动画")]
    public bool enableAnimation = true;
    
    [LabelText("动画时间")]
    public float animationTime = 2;
    
    [LabelText("动画类型"), EnumToggleButtons]
    public AnimationType animationType = AnimationType.Sequential;
    
    [LabelText("动画曲线")]
    public AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [LabelText("错开延迟")]
    public float staggerDelay = 0.1f;
    
    [LabelText("开启循环")]
    public bool openLoop = true;
    
    [ShowIf("openLoop"), LabelText("循环缩放")]
    public float loopScale = 1.5f;
    
    [ShowIf("openLoop"), LabelText("循环间隔时间")]
    public float loopSpaceTime = 2f;
    
    [ShowIf("openLoop"), LabelText("循环曲线")]
    public AnimationCurve loopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [ShowIf("openLoop"), LabelText("循环事件")]
    public UnityEvent<int> LoopEvent = new UnityEvent<int>();
    
    
    [LabelText("渐入时间")]
    public float fadeInTime = 0.3f;
    
    [LabelText("渐出时间")]
    public float fadeOutTime = 0.2f;
    #endregion

    #region 动画类型枚举
    public enum AnimationType
    {
        [LabelText("顺序播放")]
        Sequential,
        [LabelText("从中心向外")]
        CenterOut,
        [LabelText("从外向内")]
        OutsideIn
    }
    #endregion

    #region 属性
    private RectTransform _rect;
    public RectTransform Rect
    {
        get
        {
            if (_rect == null)
            {
                _rect = GetComponent<RectTransform>();
            }
            return _rect;
        }
        set => _rect = value;
    }
    #endregion

    #region 动画状态管理
    private enum AnimationState
    {
        Idle,
        Playing,
        Paused,
        Stopped
    }
    
    private AnimationState currentAnimationState = AnimationState.Idle;
    private Dictionary<int, float> animationProgress = new Dictionary<int, float>();
    private Dictionary<int, float> loopScales = new Dictionary<int, float>();
    private Dictionary<int, float> lineAlphas = new Dictionary<int, float>();
    private Dictionary<int, float> textAlphas = new Dictionary<int, float>();
    private int currentHoverIndex = -1;
    private bool isHovering = false;
    
    // hover协程引用
    private Coroutine currentHoverFadeInCoroutine;
    private Coroutine currentHoverFadeOutCoroutine;
    
    // 动画阶段标志
    private bool isInInitialFadeInPhase = false; // 是否在初始渐入阶段
    #endregion

    float all;
    protected override void Awake()
    {
        if (setAwake)
        {
            SetGraph();
        }
    }

#if UNITY_EDITOR
    #region Inspector
    [OnInspectorGUI]
    private void GUIChange()
    {
        if (GUI.changed)
        {
            StartCoroutine(YjjUtility.DeLay(() =>
            {
                SetGraph();
            }));
        }
    }

    [OnInspectorInit]
    private void Init()
    {
        if (font == null)
            font = YjjConfigs.Instance.tmpFont;
        
        // 确保raycastTarget正确设置
        raycastTarget = true;
        
        if (!Application.isPlaying)
        {
            SetGraph();
        }
        UnityEditor.EditorApplication.update += ChangeSize;
    }

    private void ChangeSize()
    {
        var w = Rect.sizeDelta.x < Rect.sizeDelta.y ? rectTransform.sizeDelta.x : rectTransform.sizeDelta.y;
        w = w * 0.5f;
        if (radius != w)
        {
            radius = w;
            SetGraph();
        }
    }

    [OnInspectorDispose]
    private void Dispose()
    {
        UnityEditor.EditorApplication.update -= ChangeSize;
    }
    #endregion
#endif
    public void SetData(List<float> data, List<string> titles = null)
    {
        setAwake = false;
        datas = data.Select(x => x).ToList();
        if (titles != null)
        {
            names = titles;
        }
        
        
        SetGraph();
        PlayAnimation();
    }
    private List<List<Vector2>> linePositions = new List<List<Vector2>>();
    private List<List<Vector2>> textLinePostions = new List<List<Vector2>>();

    private List<float> tempList;

    public void SetGraph(bool clear = true)
    {
        if (clear)
        {
            tempList = null;
            List<GameObject> delate = new List<GameObject>();
            int count = transform.childCount;
            for (int i = 0; i < count; i++)
            {
                var go = transform.GetChild(i).gameObject;
                if (go.GetComponent<Image>() == null)
                {
                    delate.Add(go);
                }
            }
            for (int i = 0; i < delate.Count; i++)
            {
                if (Application.isPlaying)
                    Destroy(delate[i]);
                else
                    DestroyImmediate(delate[i]);
            }
        }
        linePositions.Clear();
        //halfCiclePositions.Clear();
        textLinePostions.Clear();
        all = 0;
        var maxRoundRadiu = width * 0.5f;
        if (roundRadiu > maxRoundRadiu)
        {
            roundRadiu = maxRoundRadiu;
        }
        if (this.datas.Count == 1)
        {
            all = 100;
        }
        else
        {
            all = this.datas.Sum();
        }
        var allAngle = 360 - distanceAngle * datas.Where(x => x != 0).Count();
        //var allAngle = 360 - distanceAngle * datas.Count();
        //计算
        float beginAngle = startAngle;
        for (int i = 0; i < datas.Count; i++)
        {
            if (datas[i] == 0)
            {
                linePositions.Add(null);
                textLinePostions.Add(null); // 添加占位符以保持索引一致
                continue;
            }
            if (colors.Count < i + 1)
            {
                colors.Add(Color.white);
            }
            float angle = datas[i] / all * allAngle; //当前数据所占角度
            //angle = angle == 0 ? 1 : angle;
            int smoothValue = (int)(angle / allAngle * smooth); //细分程度
            smoothValue = smoothValue < 3 ? 3 : smoothValue;
            float endAngle = beginAngle + angle;  //结束位置
            List<Vector2> positions = new List<Vector2>();
            for (int j = 0; j <= smoothValue; j++)
            {
                float smoothAnlge = Mathf.Lerp(beginAngle, endAngle, (float)j / smoothValue) * Mathf.Deg2Rad;
                var postion = new Vector2(Mathf.Sin(smoothAnlge) * radius, Mathf.Cos(smoothAnlge) * radius);
                positions.Add(postion);
            }
            linePositions.Add(positions);
            var centerAngle = ((beginAngle + endAngle) * 0.5f) * Mathf.Deg2Rad;
            beginAngle = endAngle + distanceAngle;
            var centerPos = new Vector2(Mathf.Sin(centerAngle) * radius, Mathf.Cos(centerAngle) * radius);
            centerPos += (-centerPos).normalized * width * 0.5f;
            //生成文本
            var text = transform.GetOrCreatUIChild<TextMeshProUGUI>($"Text{i}");
            var rect = text.rectTransform;
            if (font != null)
            {
                text.font = font;
            }
            //文本字体
            if (!text.gameObject.activeInHierarchy)
            {
                text.UpdateFontAsset();
            }

            //画线
            Vector2 threePosition = Vector2.zero;
            if (drawLine)
            {
                float x = centerPos.x > 0 ? lineOffset.x : -lineOffset.x;
                float y = centerPos.y > 0 ? lineOffset.y : -lineOffset.y;
                List<Vector2> list = new List<Vector2>();
                list.Add(centerPos);
                var secondPos = centerPos + new Vector2(x, y);
                list.Add(secondPos);
                threePosition = centerPos.x > 0 ? secondPos + new Vector2(lineLength, 0) : secondPos + new Vector2(-lineLength, 0);
                list.Add(threePosition);
                textLinePostions.Add(list);

                //文本位置
                if (centerPos.x > 0)
                {
                    if (textType == TitleType.显示标题和数据)
                    {
                        rect.pivot = new Vector2(1, 1);
                    }
                    else
                    {
                        rect.pivot = new Vector2(1, 0);
                    }

                    text.alignment = TextAlignmentOptions.BottomRight;
                }
                else
                {
                    if (textType == TitleType.显示标题和数据)
                    {
                        rect.pivot = new Vector2(0, 1);
                    }
                    else
                    {
                        rect.pivot = new Vector2(0, 0);
                    }
                    text.alignment = TextAlignmentOptions.BottomLeft;
                }
                if (textType == TitleType.显示标题和数据)
                {
                    rect.sizeDelta = new Vector2(lineLength, 20);
                    text.alignment = TextAlignmentOptions.Top;
                }
                rect.anchoredPosition = threePosition;
            }
            else
            {
                if (textInCenter && datas.Count == 1)
                {
                    text.alignment = TextAlignmentOptions.Center;
                    var size = Mathf.Min(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y) - width * 2;
                    rect.sizeDelta = new Vector2(size, size);
                    rect.anchoredPosition = Vector2.zero;
                }
                else
                {
                    rect.anchoredPosition = centerPos.normalized * textDistance;
                }
            }

            if (valueTextColorFollowSprite)
            {
                text.color = colors[i];
            }
            else
            {
                text.color = text_color;
            }
            if (textAlphas.ContainsKey(i))
                text.color = text.color.SetAlpha(textAlphas[i]);
            text.fontSize = text_size;
            text.raycastTarget = false;

            // 检查是否在动画过程中，如果是则隐藏数据标签
            bool isAnimating = currentAnimationState == AnimationState.Playing;
            bool shouldShowText = true;

            if (isAnimating)
            {
                // 根据动画类型决定是否显示文本
                switch (animationType)
                {
                    case AnimationType.Sequential:
                        // 顺序动画：当前数据完成时才显示
                        shouldShowText = animationProgress.ContainsKey(i) && animationProgress[i] >= 1f;
                        break;
                    case AnimationType.CenterOut:
                    case AnimationType.OutsideIn:
                        // 中心向外/从外向内：当前数据完成时才显示
                        shouldShowText = animationProgress.ContainsKey(i) && animationProgress[i] >= 1f;
                        break;
                }
            }

            if (textType == TitleType.显示数据)
            {
                text.text = this.datas[i].ToAutoLimitString(floatCount);
            }
            else if (textType == TitleType.显示标题)
            {
                if (names.Count > i)
                {
                    text.text = names[i];
                }
            }
            else if (textType == TitleType.显示标题和数据)
            {
                var title = transform.GetOrCreatUIChild<TextMeshProUGUI>($"title{i}", (t) =>
                {
                    t.alignment = TextAlignmentOptions.Bottom;
                });
                title.rectTransform.sizeDelta = new Vector2(lineLength, 20);
                title.color = titleColor;
                if (textAlphas.ContainsKey(i))
                    title.color = title.color.SetAlpha(textAlphas[i]);
                title.fontSize = titleSize;
                title.font = font;
                if (centerPos.x > 0)
                {
                    title.rectTransform.pivot = new Vector2(1, 0);
                }
                else
                {
                    title.rectTransform.pivot = new Vector2(0, 0);
                }
                if (drawLine)
                {
                    title.rectTransform.anchoredPosition = threePosition;
                }
                if (names.Count > i)
                {
                    title.text = names[i];
                }
                text.text = this.datas[i].ToAutoLimitString(floatCount);

            }

            if (showUnit)
            {
                text.text += unit;
            }
            if (textType == TitleType.不显示)
            {
                text.gameObject.DestroyByRuntimeType();
            }
        }

        if (clear)
        {
            // 生成图例
            if (enableLegend && config!=null)
            {
                var container = transform.GetOrCreatUIChild<RectTransform>("LegendContainer", t =>
                 {
                     t.sizeDelta = rectTransform.sizeDelta;
                     var anchor = new Vector2(0.5f, 1);
                     t.anchorMin = anchor;
                     t.anchorMax = anchor;
                     t.pivot = new Vector2(0.5f, 0);
                     t.anchoredPosition = Vector2.zero;
                 });
                if (legendWithData)
                {
                    config.GenerateLegend(container, colors, names, datas);
                }
                else
                {
                    config.GenerateLegend(container, colors, names);
                }
            }
            tempList = datas.Select(x => x).ToList();
        }
        SetAllDirty();

    }

    #region 动画管理系统
    /// <summary>
    /// 播放饼图动画
    /// </summary>
    [Button]
    public void PlayAnimation()
    {
        if (!enableAnimation || !gameObject.activeInHierarchy || !Application.isPlaying)
        {
            return;
        }

        StopAllAnimations();
        currentAnimationState = AnimationState.Playing;
        
        // 设置初始渐入阶段标志
        isInInitialFadeInPhase = true;
        
        // 初始化动画状态
        InitializeAnimationState();
        
        // 先设置动画状态，然后重新调用SetGraph来隐藏标签
        SetGraph(false);
        
        // 根据动画类型播放
        switch (animationType)
        {
            case AnimationType.Sequential:
                StartCoroutine(PlaySequentialAnimation());
                break;
            case AnimationType.CenterOut:
                StartCoroutine(PlayCenterOutAnimation());
                break;
            case AnimationType.OutsideIn:
                StartCoroutine(PlayOutsideInAnimation());
                break;
        }
    }
    #endregion

    #region 动画状态管理
    /// <summary>
    /// 初始化动画状态
    /// </summary>
    private void InitializeAnimationState()
    {
        animationProgress.Clear();
        loopScales.Clear();
        lineAlphas.Clear();
        textAlphas.Clear();
        
        for (int i = 0; i < datas.Count; i++)
        {
            animationProgress[i] = 0f;
            loopScales[i] = 1f;
            // 所有动画都从透明度0开始，然后通过动画渐入到1
            lineAlphas[i] = 0f;
            textAlphas[i] = 0f;
        }
    }
    #endregion

    #region 主要动画类型

    /// <summary>
    /// 顺序播放动画 - 饼块按顺序依次增长
    /// </summary>
    private IEnumerator PlaySequentialAnimation()
    {
        // 初始化所有数据为0
        for (int i = 0; i < datas.Count; i++)
        {
            if (tempList[i] > 0)
            {
                datas[i] = 0;
            }
        }
        SetGraph(false);
        
        for (int i = 0; i < datas.Count; i++)
        {
            if (tempList[i] == 0) continue;
            
            int index = i;
            float elapsedTime = 0f;
            float singleTime = animationTime / datas.Count;
            
            while (elapsedTime < singleTime)
            {
                if (currentAnimationState == AnimationState.Paused)
                {
                    yield return null;
                    continue;
                }
                
                float t = elapsedTime / singleTime;
                float curveValue = animationCurve.Evaluate(t);
                
                datas[index] = Mathf.Lerp(0, tempList[index], curveValue);
                animationProgress[index] = curveValue;
                lineAlphas[i] = curveValue;
                textAlphas[i] = curveValue;
                SetGraph(false);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 确保最终值
            datas[index] = tempList[index];
            animationProgress[index] = 1f;
            SetGraph(false);
            
            // 错开延迟
            if (staggerDelay > 0)
            {
                yield return new WaitForSeconds(staggerDelay);
            }
        }
        
        CompleteAnimation();
    }

    /// <summary>
    /// 从中心向外动画 - 中心饼块先增长，再向外扩展
    /// </summary>

    private IEnumerator PlayCenterOutAnimation()
    {
        // 从中心向外：先播放中心的数据，再播放外围的
        List<int> centerOutOrder = GetCenterOutOrder();
        
        // 初始化所有数据为0
        for (int i = 0; i < datas.Count; i++)
        {
            if (tempList[i] > 0)
            {
                datas[i] = 0;
            }
        }
        SetGraph(false);
        
        foreach (int index in centerOutOrder)
        {
            if (tempList[index] == 0) continue;
            
            float elapsedTime = 0f;
            float singleTime = animationTime / tempList.Where(x => x > 0).Count();
            
            while (elapsedTime < singleTime)
            {
                if (currentAnimationState == AnimationState.Paused)
                {
                    yield return null;
                    continue;
                }
                
                float t = elapsedTime / singleTime;
                float curveValue = animationCurve.Evaluate(t);
                
                datas[index] = Mathf.Lerp(0, tempList[index], curveValue);
                animationProgress[index] = curveValue;
                lineAlphas[index] = curveValue;
                textAlphas[index] = curveValue;
                
                SetGraph(false);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            datas[index] = tempList[index];
            animationProgress[index] = 1f;
            SetGraph(false);
            
            if (staggerDelay > 0)
            {
                yield return new WaitForSeconds(staggerDelay);
            }
        }
        
        CompleteAnimation();
    }

    private IEnumerator PlayOutsideInAnimation()
    {
        // 从外向内：先播放外围的数据，再播放中心的
        List<int> outsideInOrder = GetOutsideInOrder();
        
        // 初始化所有数据为0
        for (int i = 0; i < datas.Count; i++)
        {
            if (tempList[i] > 0)
            {
                datas[i] = 0;
                animationProgress[i] = 0f;
            }
        }
        SetGraph(false);
        
        foreach (int index in outsideInOrder)
        {
            if (tempList[index] == 0) continue;
            
            float elapsedTime = 0f;
            float singleTime = animationTime / tempList.Where(x => x > 0).Count();
            
            while (elapsedTime < singleTime)
            {
                if (currentAnimationState == AnimationState.Paused)
                {
                    yield return null;
                    continue;
                }
                
                float t = elapsedTime / singleTime;
                float curveValue = animationCurve.Evaluate(t);
                
                datas[index] = Mathf.Lerp(0, tempList[index], curveValue);
                animationProgress[index] = curveValue;
                lineAlphas[index] = curveValue;
                textAlphas[index] = curveValue;
                
                SetGraph(false);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            datas[index] = tempList[index];
            animationProgress[index] = 1f;
            SetGraph(false);
            
            if (staggerDelay > 0)
            {
                yield return new WaitForSeconds(staggerDelay);
            }
        }
        
        CompleteAnimation();
    }

    private List<int> GetCenterOutOrder()
    {
        // 根据角度计算距离中心的远近
        List<int> order = new List<int>();
        List<float> angles = new List<float>();
        
        float currentAngle = startAngle;
        for (int i = 0; i < datas.Count; i++)
        {
            if (datas[i] > 0)
            {
                float centerAngle = currentAngle + (datas[i] / all * (360 - distanceAngle * datas.Where(x => x != 0).Count())) * 0.5f;
                angles.Add(centerAngle);
                order.Add(i);
            }
            currentAngle += (datas[i] / all * (360 - distanceAngle * datas.Where(x => x != 0).Count())) + distanceAngle;
        }
        
        // 按角度排序（从中心开始）
        // 创建索引和角度的配对列表进行排序
        var indexedAngles = new List<(int index, float angle)>();
        for (int i = 0; i < order.Count; i++)
        {
            indexedAngles.Add((order[i], angles[i]));
        }
        
        indexedAngles.Sort((a, b) => Mathf.Abs(a.angle - 180).CompareTo(Mathf.Abs(b.angle - 180)));
        
        // 提取排序后的索引
        order.Clear();
        foreach (var item in indexedAngles)
        {
            order.Add(item.index);
        }
        
        return order;
    }

    private List<int> GetOutsideInOrder()
    {
        List<int> order = GetCenterOutOrder();
        order.Reverse();
        return order;
    }

    private void CompleteAnimation()
    {
        currentAnimationState = AnimationState.Idle;
        
        SetGraph(false);
        
        // 重置初始渐入阶段标志，现在可以支持hover了
        isInInitialFadeInPhase = false;
        
        // 开始循环动画
        if (openLoop)
        {
            StartCoroutine(PlayLoopAnimation());
        }
    }

    #endregion

    #region 循环动画系统
    /// <summary>
    /// 循环动画 - 依次高亮每个饼块，包含渐入渐出效果
    /// </summary>
    private IEnumerator PlayLoopAnimation()
    {
        yield return new WaitForSeconds(loopSpaceTime);
        //清理状态
        ClearAllScaleStates();
        while (openLoop)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i] == 0) continue;
                
                currentHoverIndex = i;
                LoopEvent?.Invoke(i);
                
                // 播放loop效果（包含渐入渐出）
                yield return StartCoroutine(FadeIn(i, false)); // loop模式
                yield return new WaitForSeconds(loopSpaceTime);
                yield return StartCoroutine(FadeOut(i, false)); // loop模式
            }
        }
    }
    #endregion

    #region 透明度动画系统

    /// <summary>
    /// 线条和文本渐入动画
    /// </summary>

    private IEnumerator FadeIn(int index, bool isHover = false)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInTime)
        {
            float t = elapsedTime / fadeInTime;

            lineAlphas[index] = Mathf.Lerp(0f, 1f, t);
            textAlphas[index] = Mathf.Lerp(0f, 1f, t);
            loopScales[index] = Mathf.Lerp(1, loopScale, t);

            // 更新文本标签显示状态
            UpdateTextLabelsForLoop();

            SetVerticesDirty(); // 更新绘制
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        lineAlphas[index] = 1f;
        textAlphas[index] = 1f;
        loopScales[index] = loopScale;

        // 更新文本标签显示状态
        UpdateTextLabelsForLoop();

        SetVerticesDirty(); // 更新绘制
    }

    /// <summary>
    /// 线条和文本渐出动画
    /// </summary>

    private IEnumerator FadeOut(int index, bool isHover = false)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime / fadeOutTime;

            lineAlphas[index] = Mathf.Lerp(1f, 0f, t);
            textAlphas[index] = Mathf.Lerp(1f, 0f, t);
            loopScales[index] = Mathf.Lerp(loopScale, 1, t);

            // 更新文本标签显示状态
            UpdateTextLabelsForLoop();

            SetVerticesDirty(); // 更新绘制
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保最终状态
        lineAlphas[index] = 0f;
        textAlphas[index] = 0f;
        loopScales[index] = 1f;

        // 更新文本标签显示状态
        UpdateTextLabelsForLoop();

        SetVerticesDirty(); // 更新绘制
    }
    #endregion

    #region 文本标签管理系统


    /// <summary>
    /// 更新循环动画中的文本标签显示状态
    /// </summary>
    private void UpdateTextLabelsForLoop()
    {
        // 在循环动画中，只显示当前悬停的饼块对应的文本标签
        for (int i = 0; i < datas.Count; i++)
        {
            if (datas[i] == 0) continue;
            
            var text = transform.Find($"Text{i}")?.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                bool shouldShow = (currentHoverIndex == i);
                float alpha = textAlphas.ContainsKey(i) ? textAlphas[i] : 0f;
                
                if (textType == TitleType.显示数据)
                {
                    text.text = this.datas[i].ToLimitString(floatCount);
                    if (showUnit) text.text += unit;
                }
                else if (textType == TitleType.显示标题和数据)
                {
                    var title = transform.Find($"title{i}")?.GetComponent<TextMeshProUGUI>();
                    if (title != null)
                    {
                        // 设置标题透明度
                        Color titleColor = title.color;
                        titleColor.a = alpha;
                        title.color = titleColor;
                    }
                    
                    if (shouldShow)
                    {
                        text.text = this.datas[i].ToLimitString(floatCount);
                        if (showUnit) text.text += unit;
                    }
                }
                
                // 设置文本透明度
                Color textColor = text.color;
                textColor.a = alpha;
                text.color = textColor;
            }
        }
    }

    /// <summary>
    /// 恢复所有文本标签的正常显示状态
    /// </summary>
    private void RestoreAllTextLabels()
    {
        for (int key = 0; key < datas.Count; key++)
        {
            lineAlphas[key] = 1;
            textAlphas[key] = 1;
            loopScales[key] = 1;
        }
        UpdateTextLabelsForLoop();
          SetVerticesDirty(); // 更新线条绘制
    }
    #endregion


    #region 动画控制方法
    /// <summary>
    /// 停止所有动画
    /// </summary>
    public void StopAllAnimations()
    {
        StopAllCoroutines();
        currentAnimationState = AnimationState.Stopped;
        currentHoverIndex = -1;
        isHovering = false;
        isInInitialFadeInPhase = false;
        
        // 清理所有缩放状态
        ClearAllScaleStates();
    }

    public void PauseAnimation()
    {
        if (currentAnimationState == AnimationState.Playing)
        {
            currentAnimationState = AnimationState.Paused;
        }
    }

    public void ResumeAnimation()
    {
        if (currentAnimationState == AnimationState.Paused)
        {
            currentAnimationState = AnimationState.Playing;
        }
    }

    /// <summary>
    /// 设置动画速度（1.0为正常速度）
    /// </summary>
    public void SetAnimationSpeed(float speed)
    {
        animationTime = 2f / Mathf.Max(0.1f, speed);
    }

    /// <summary>
    /// 获取当前动画进度（0-1）
    /// </summary>
    public float GetAnimationProgress()
    {
        if (animationProgress.Count == 0) return 0f;
        
        float totalProgress = 0f;
        foreach (var progress in animationProgress.Values)
        {
            totalProgress += progress;
        }
        return totalProgress / animationProgress.Count;
    }

    /// <summary>
    /// 获取指定饼块的动画进度
    /// </summary>
    public float GetPieAnimationProgress(int index)
    {
        return animationProgress.ContainsKey(index) ? animationProgress[index] : 0f;
    }

    /// <summary>
    /// 是否正在播放动画
    /// </summary>
    public bool IsAnimating()
    {
        return currentAnimationState == AnimationState.Playing;
    }

    /// <summary>
    /// 重新播放动画
    /// </summary>
    public void RestartAnimation()
    {
        StopAllAnimations();
        PlayAnimation();
    }

    /// <summary>
    /// 设置动画曲线
    /// </summary>
    public void SetAnimationCurve(AnimationCurve curve)
    {
        animationCurve = curve;
    }

    /// <summary>
    /// 强制重建UI以支持射线检测
    /// </summary>
    [Button]
    public void RebuildUI()
    {
        raycastTarget = true;
        SetAllDirty();
        Rebuild(CanvasUpdate.PreRender);
    }

    #endregion

    #region 简化的悬停方法

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!openHover) return;
        CheckHover(eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (!openHover) return;
        CheckHover(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!openHover) return;

        // 停止所有hover协程
        if (currentHoverFadeInCoroutine != null)
        {
            StopCoroutine(currentHoverFadeInCoroutine);
            currentHoverFadeInCoroutine = null;
        }

      

        // 如果openLoop为true，继续播放loop动画
        if (openLoop)
        {
            RemoveCurrentHover();
            // 延迟一点时间再开始loop，让hover渐出完成
            StartCoroutine(ResumeLoopAfterHoverExit());
        }
        else
        {
            // 如果不是在循环动画中，恢复所有文本标签的显示
            RestoreAllTextLabels();
        }
    }

    private void CheckHover(Vector2 pos)
    {
        // 如果在初始渐入阶段，屏蔽hover事件
        if (isInInitialFadeInPhase) return;

        var dir = Quaternion.Euler(0, 0, -startAngle) * Vector2.up;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pos, uicamera, out var local);
        var angle = Vector2.Angle(dir, local);
        if (MeshUtility.ToTheLeft(local, Vector2.zero, dir))
        {
            angle = 360 - angle;
        }

        int index = 0;
        while (angle > 0)
        {
            angle -= datas[index] / all * 360;
            if (angle > 0)
            {
                index++;
            }
        }

        // 如果还是同一个index，不需要处理
        if (index == currentHoverIndex) return;

        // 进入新的index，执行新的hover流程
        SwitchToNewHover(index);
    }

    void RemoveCurrentHover()
    {
        // 如果有当前hover，开启渐出协程
        if (currentHoverIndex >= 0)
        {
            textAlphas[currentHoverIndex] = 0;
            lineAlphas[currentHoverIndex] = 0;
            loopScales[currentHoverIndex] = 0;
        }
        // 重置状态
        currentHoverIndex = -1;
        isHovering = false;
    }

    /// <summary>
    /// 切换到新的hover效果
    /// </summary>
    private void SwitchToNewHover(int newIndex)
    {
        // 1. 关闭hover渐入协程
        if (currentHoverFadeInCoroutine != null)
        {
            StopCoroutine(currentHoverFadeInCoroutine);
            currentHoverFadeInCoroutine = null;
        }

        RemoveCurrentHover();

        // 3. 停止loop动画并重置状态
        if ( currentAnimationState != AnimationState.Stopped)
        {
            // 停止所有协程，包括loop和hover
            StopAllCoroutines();
            // 重置状态但不重置hover相关状态
            currentAnimationState = AnimationState.Stopped;
        }
      

        // 清理loop相关的状态
        ClearAllScaleStates();
        // 更新文本标签显示状态
        UpdateTextLabelsForLoop();
        SetVerticesDirty();

        // 4. 开启新的渐入协程
        currentHoverIndex = newIndex;
        isHovering = true;
        LoopEvent?.Invoke(newIndex);

        currentHoverFadeInCoroutine = StartCoroutine(FadeIn(newIndex, true));
    }


    /// <summary>
    /// 清理所有缩放状态和透明度状态
    /// </summary>
    private void ClearAllScaleStates()
    {
        for (int i = 0; i < datas.Count; i++)
        {
            loopScales[i] = 1;
            lineAlphas[i] = 0;
            textAlphas[i] = 0;
            animationProgress[i] = 0;
        }
    }

    /// <summary>
    /// hover退出后恢复loop动画的协程
    /// </summary>
    private IEnumerator ResumeLoopAfterHoverExit()
    {
        currentAnimationState = AnimationState.Playing;
        if (currentHoverFadeOutCoroutine != null)
        {
            yield return currentHoverFadeOutCoroutine;
        }
        // 清理hover状态（包括缩放和透明度）
        ClearAllScaleStates();
        SetGraph(false);

        // 直接启动loop动画，不重新开始整个动画流程
        if (openLoop && !isHovering)
        {
            // 延迟2秒再开始播放loop动画
            yield return new WaitForSeconds(2f);
            StartCoroutine(PlayLoopAnimation());
        }
        else
        {
            currentAnimationState = AnimationState.Stopped;
        }
    }
    #endregion
    protected override void OnEnable()
    {
        base.OnEnable();
        // 确保raycastTarget正确设置
        raycastTarget = true;
        
        // SetGraph();
        PlayAnimation();
    }


    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        vh.Clear();
        if (drawBackGround)
        {
            //细分程度
            var positions = new List<Vector2>();
            for (int j = 0; j <= backGroundSmooth; j++)
            {
                float smoothAnlge = 360f / backGroundSmooth * j * Mathf.Deg2Rad;
                var postion = new Vector2(Mathf.Sin(smoothAnlge) * radius, Mathf.Cos(smoothAnlge) * radius);
                positions.Add(postion);
            }
            Yjj_ChartUtility.DrawRingSmooth(vh, positions, Vector2.zero, width, colors[colors.Count-1]);
        }
        //画线
        for (int i = 0; i < textLinePostions.Count; i++)
        {
            if (textLinePostions[i] == null) continue; // 跳过数据为0的线条

            Color lineColorWithAlpha = lineColor;
            // 在非循环动画中，如果没有设置透明度，则使用原始颜色
            var alpha = lineAlphas.ContainsKey(i) ? lineAlphas[i] : 1f;
            lineColorWithAlpha.a *= alpha;
            Yjj_ChartUtility.DrawLineSmoothWithLerp(vh, textLinePostions[i], lineWidth, lineColorWithAlpha, alpha);
        }

        //画饼块
        for (int i = 0; i < linePositions.Count; i++)
        {
            if (linePositions[i] == null) continue;
            
            // 计算当前饼块的宽度
            float currentWidth = width;
            
            // 计算循环动画和悬停效果的最大缩放值
            float maxScale = 1f;
            
            // 循环动画效果
            if (openLoop && loopScales.ContainsKey(i))
            {
                maxScale = Mathf.Max(maxScale, loopScales[i]);
            }
            
            // 悬停效果（取与循环动画的最大值）
            if (currentHoverIndex == i && loopScales.ContainsKey(i))
            {
                maxScale = Mathf.Max(maxScale, loopScales[i]);
            }
            
            // 应用最大缩放值
            currentWidth *= maxScale;


            // 获取当前颜色（支持透明度动画）
            Color currentColor = colors[i];

            
            Yjj_ChartUtility.DrawRingRoundSmooth(vh, linePositions[i], Vector2.zero, currentWidth, currentColor, roundRadiu);
        }
    }

}
