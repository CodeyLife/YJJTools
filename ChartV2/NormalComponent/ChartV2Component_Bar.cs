using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// 悬停效果模式
/// </summary>
public enum HoverEffectMode
{
    [InspectorName("变亮")]
    Brighten,
    [InspectorName("变暗")]
    Darken,
    [InspectorName("增加饱和度")]
    Saturate,
    [InspectorName("减少饱和度")]
    Desaturate
}

[ComponentDesc("柱状图")]
public class ChartV2Component_Bar : ChartV2ComponetBaseWithoutGraphic
{
    public bool useAllData = true;
    [HideIf("useAllData")]
    public List<int> DataIndex = new List<int> { 0 };
    [PropertyTooltip("每个datas[i]为一组柱状图数据，否则一整个数据做为多个柱状图")]
    public bool Multidimensional = true;

    [HorizontalGroup("sprite")]
    [PreviewField]
    [Title("柱状图sprite")] public Sprite sprite;
    [LabelText("使用fill模式"), PropertyTooltip("fill填充模式需要相应的sprite")]
    public bool spriteIsFill = false;
    [HorizontalGroup("sprite")]
    [PreviewField]
    [Title("柱状图sprite")]
    public Sprite barBg;
    [Title("柱状图宽度")] public float barWidth = 20;
    [Title("柱状图间距"), ShowIf("Multidimensional")] public float distance = 20;
    //  public LineSet lineSet = new LineSet();
    [LabelText("是否显示数据文本")]
    public bool openDataText = false;
    [ShowIf("openDataText")]
    public Vector2 textOffset = Vector2.zero;
    [ShowIf("openDataText")]
    public float textSize = 32;
    [ShowIf("openDataText"), LabelText("保留几位小数")]
    public int textEnd = 0;
    [ShowIf("openDataText")]
    public TMP_FontAsset textFont;
    [ShowIf("openDataText")]
    public Color textColor = Color.white;

    [Title("交互设置")]
    [LabelText("启用悬停效果")]
    public bool enableHoverEffect = true;
    [ShowIf("enableHoverEffect")]
    [LabelText("悬停缩放倍数")]
    public float hoverScale = 1.1f;
    [ShowIf("enableHoverEffect")]
    [LabelText("悬停效果模式")]
    public HoverEffectMode hoverEffectMode = HoverEffectMode.Darken;
    [ShowIf("enableHoverEffect")]
    [LabelText("悬停效果强度")]
    [Range(0.1f, 1.0f)]
    public float hoverEffectIntensity = 0.3f;
    [ShowIf("enableHoverEffect")]
    [LabelText("悬停动画时间")]
    public float hoverAnimationTime = 0.2f;

    [LabelText("启用点击事件")]
    public bool enableClickEvent = true;
    [ShowIf("enableClickEvent")]
    [LabelText("点击缩放倍数")]
    public float clickScale = 0.95f;
    [ShowIf("enableClickEvent")]
    [LabelText("点击动画时间")]
    public float clickAnimationTime = 0.1f;



    //本地数据
    protected List<List<Vector2>> dataList = new List<List<Vector2>>();
    private int currentIndex = -1;
    
    //交互相关
    private Dictionary<string, RectTransform> barTransforms = new Dictionary<string, RectTransform>();
    private Dictionary<string, Image> barImages = new Dictionary<string, Image>();
    private Dictionary<string, Color> originalColors = new Dictionary<string, Color>();
    private Dictionary<string, Vector3> originalScales = new Dictionary<string, Vector3>();
    private string currentHoveredBar = "";

    [System.Serializable]
    public class BarClickEvent : UnityEvent<BarClickData> { }
    
    [System.Serializable]
    public class BarHoverEvent : UnityEvent<BarHoverData> { }

    [System.Serializable]
    public class BarClickData
    {
        public int dataIndex;
        public int barIndex;
        public float value;
        public Vector2 worldPosition;
        public string barKey;
    }

    [System.Serializable]
    public class BarHoverData
    {
        public int dataIndex;
        public int barIndex;
        public float value;
        public Vector2 worldPosition;
        public string barKey;
        public bool isEntering;
    }

    [Title("交互事件")]
    public BarClickEvent OnBarClicked = new BarClickEvent();
    public BarHoverEvent OnBarHovered = new BarHoverEvent();
#if UNITY_EDITOR
    public override void OnCreat()
    {
        base.OnCreat();
        AutoLoadDefaultSprite();
    }

    /// <summary>
    /// 自动加载默认sprite
    /// </summary>
    private void AutoLoadDefaultSprite()
    {
        if (sprite == null)
        {
            try
            {
                var barGuids = UnityEditor.AssetDatabase.FindAssets("t:sprite bar");
                if (barGuids.Length > 0)
                {
                    var barGuid = barGuids[0];
                    sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
                        UnityEditor.AssetDatabase.GUIDToAssetPath(barGuid));
                }
                else
                {
                    Debug.LogWarning("未找到名为'bar'的sprite，请手动设置柱状图sprite");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载默认sprite时出错: {e.Message}");
            }
        }
    }


#endif
    public override void InitGraph(ChartV2Base chart)
    {
        base.InitGraph(chart);
        
        if (!ValidateInitialization(chart))
            return;

        SetupMask();
        SetupDragEvents();
        InitializeDataList();
        SetupAnimation();
        SetupHoverEvents();
    }

    /// <summary>
    /// 验证初始化条件
    /// </summary>
    private bool ValidateInitialization(ChartV2Base chart)
    {
        if (chart == null)
        {
            Debug.LogError("ChartV2Base 参数为空");
            return false;
        }

        if (chart.datas == null || chart.datas.Count == 0)
        {
            Debug.LogWarning("图表数据为空，无法初始化柱状图");
            return false;
        }

        ValidateConfiguration();
        return true;
    }

    /// <summary>
    /// 验证配置参数
    /// </summary>
    private void ValidateConfiguration()
    {
        var issues = new List<string>();

        if (sprite == null) 
            issues.Add("缺少柱状图sprite");
        
        if (barWidth <= 0) 
        {
            issues.Add("柱状图宽度必须大于0，已重置为默认值");
            barWidth = 20f;
        }
        
        if (distance < 0) 
        {
            issues.Add("柱状图间距不能为负数，已重置为0");
            distance = 0f;
        }
        
        if (textSize <= 0 && openDataText) 
        {
            issues.Add("文本大小必须大于0，已重置为默认值");
            textSize = 32f;
        }

        if (issues.Count > 0)
        {
            Debug.LogWarning($"柱状图配置问题：\n{string.Join("\n", issues)}");
        }
    }

    /// <summary>
    /// 设置遮罩
    /// </summary>
    private void SetupMask()
    {
        var mask = transform.GetOrCreatUIChild<Image>("Mask", (img) =>
        {
            var m = img.gameObject.AddComponent<Mask>();
            m.showMaskGraphic = false;
            _v2Base.InitLocalRect(m.rectTransform);
        });
        mask.rectTransform.sizeDelta = new Vector2(_v2Base.width, _v2Base.height);
    }

    /// <summary>
    /// 设置拖拽事件
    /// </summary>
    private void SetupDragEvents()
    {
        if (_v2Base.ComputeDataPos(false))
        {
            _v2Base.OnDragEvent.AddListener(OnDrag);
        }
        else
        {
            _v2Base.OnDragEvent.RemoveListener(OnDrag);
        }
        }

    /// <summary>
    /// 初始化数据列表
    /// </summary>
    private void InitializeDataList()
    {
        dataList.Clear();
        var heightLength = _v2Base.height - _v2Base.set.distanceFromTop - _v2Base.set.distanceFromButtom;

        if (Multidimensional)
        {
            InitializeMultidimensionalData(heightLength);
        }
        else
        {
            InitializeSingleDimensionalData(heightLength);
        }
    }

    /// <summary>
    /// 初始化多维数据
    /// </summary>
    private void InitializeMultidimensionalData(float heightLength)
    {
            for (int i = 0; i < _v2Base.datas.Count; i++)
            {
                var data = _v2Base.datas[i];
            var list = CreateDataPositionList(data, heightLength, i);
                dataList.Add(list);
            }
    }

    /// <summary>
    /// 初始化单维数据
    /// </summary>
    private void InitializeSingleDimensionalData(float heightLength)
    {
        if (useAllData)
        {
            InitializeAllData(heightLength);
        }
        else
        {
            InitializeSelectedData(heightLength);
        }
    }

    /// <summary>
    /// 初始化所有数据
    /// </summary>
    private void InitializeAllData(float heightLength)
            {
                for (int i = 0; i < _v2Base.datas.Count; i++)
                {
                    var data = _v2Base.datas[i];
            var list = CreateDataPositionList(data, heightLength, -1); // -1 表示使用索引作为x位置
                    dataList.Add(list);
                }
            }

    /// <summary>
    /// 初始化选中的数据
    /// </summary>
    private void InitializeSelectedData(float heightLength)
    {
                for (int i = 0; i < DataIndex.Count; i++)
                {
                    var targetIndex = DataIndex[i];
            if (targetIndex >= _v2Base.datas.Count)
            {
                Debug.LogWarning($"数据索引 {targetIndex} 超出范围，跳过");
                break;
            }
            
                    var data = _v2Base.datas[targetIndex];
            var list = CreateDataPositionList(data, heightLength, -1);
            dataList.Add(list);
        }
    }

    /// <summary>
    /// 创建数据位置列表
    /// </summary>
    private List<Vector2> CreateDataPositionList(MultipleData data, float heightLength, int groupIndex)
    {
                    var list = new List<Vector2>();

                    for (int j = 0; j < data.datas.Count; j++)
                    {
            var x = CalculateXPosition(j, groupIndex);
            var y = CalculateYPosition(data.datas[j], heightLength);
            list.Add(new Vector2(x, y));
        }
        
        return list;
    }

    /// <summary>
    /// 计算X位置
    /// </summary>
    private float CalculateXPosition(int dataIndex, int groupIndex)
    {
        if (Multidimensional && groupIndex >= 0)
        {
            // 多维数据模式：以组为中心分布
            var middle = (_v2Base.datas[groupIndex].datas.Count - 1) * 0.5f;
            return (dataIndex - middle) * distance + _v2Base.DataPositionInX(groupIndex);
        }
        else
        {
            // 单维数据模式：使用数据索引
            return _v2Base.DataPositionInX(dataIndex);
        }
    }

    /// <summary>
    /// 计算Y位置
    /// </summary>
    private float CalculateYPosition(float value, float heightLength)
    {
        return YjjUtility.SmoothLerp(_v2Base.min, _v2Base.max, value) * heightLength + _v2Base.set.distanceFromButtom;
    }

    /// <summary>
    /// 设置动画
    /// </summary>
    private void SetupAnimation()
    {
        SetGraph();
        
        if (Application.isPlaying)
        {
            if (_v2Base.set.openAnimation)
            {
                _v2Base.InitAnimationEvent.AddListener(Draw);
            }
            else
            {
                Draw(1);
            }
        }
        else
        {
            Draw(1);
        }
        }

        /// <summary>
    /// 设置悬停事件
    /// </summary>
    private void SetupHoverEvents()
    {
        _v2Base.OnHoverEvent.AddListener(Hover);
    }

    private void Hover(Vector2 arg0)
    {
        //var index = _v2Base.HoverDataIndex;
        //if(index != currentIndex)
        //{

        //    currentIndex = index;

        //}
    }

    private void OnDrag(float arg0)
    {
        SetGraph();
    }
    /// <summary>
    /// 绘制柱状图
    /// </summary>
    /// <param name="t">渐入动画的插值</param>
    private void Draw(float t)
    {

        // 清理之前的交互数据
        ClearInteractionData();

        var mask = transform.Find("Mask");
        int start = 0, end = 0;
        _v2Base.GetDragDataIndex(ref start, ref end, true);

        if (Multidimensional)
        {
            DrawMultidimensionalBars(mask, start, end, t);
        }
        else
        {
            DrawSingleDimensionalBars(mask, start, end, t);
        }
    }


    /// <summary>
    /// 绘制多维数据柱状图
    /// </summary>
    private void DrawMultidimensionalBars(Transform mask, int start, int end, float t)
        {
            for (int i = start; i < end; i++)
            {
                if (i >= dataList.Count)
                {
                    Debug.LogError("names.count大于datas.count，请验证数据(每个data[i]为一组柱状图数据)");
                    break;
                }

            var bar = CreateBarContainer(mask, $"bar{i}");
                var barCount = dataList[i].Count;

                for (int j = 0; j < barCount; j++)
                {
                    var pos = dataList[i][j];
                CreateBarBackground(bar, j, pos.x, true);
                CreateBarImage(bar,i, j, pos, t, GetBarColor(j));
                }

            CleanupExcessBars(bar, barCount);
        }

        CleanupExcessContainers(mask, end - start);
    }

    /// <summary>
    /// 绘制单维数据柱状图
    /// </summary>
    private void DrawSingleDimensionalBars(Transform mask, int start, int end, float t)
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            var list = dataList[i];
            var bar = CreateBarContainer(mask, $"bar{i}");
            var colorIndex = useAllData ? i : DataIndex[i];
            var color = GetBarColor(colorIndex);

            for (int j = start; j < end; j++)
            {
                if (j >= list.Count)
                    continue;

                var pos = dataList[i][j];
                CreateBarBackground(bar, j, pos.x, true);
                var groupIndex = useAllData ? i : DataIndex[i];
                CreateBarImage(bar, groupIndex, j, pos, t, color);
            }

            var maxEnd = Mathf.Min(list.Count, end);
            var maxCount = maxEnd - start;
            CleanupExcessBars(bar, maxCount);
        }

        CleanupExcessContainers(mask, dataList.Count);
    }

    /// <summary>
    /// 创建柱状图容器
    /// </summary>
    private RectTransform CreateBarContainer(Transform parent, string name)
    {
        var rect = parent.GetOrCreatUIChild<RectTransform>(name, (rect) =>
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(_v2Base.width, _v2Base.height);
          
        });
        rect.SetAsFirstSibling();
        return rect;
    }

    /// <summary>
    /// 创建柱状图背景
    /// </summary>
    private void CreateBarBackground(Transform parent, int index, float x, bool shouldCreate)
    {
        if (!shouldCreate || barBg == null)
            return;

        var bg = parent.GetOrCreatUIChild<Image>($"bg{index}", (bg) =>
        {
            var bgRect = bg.rectTransform;
                            bgRect.anchorMin = Vector2.zero;
                            bgRect.anchorMax = Vector2.zero;
                            bgRect.pivot = new Vector2(0.5f, 0);
                            
                            bgRect.sizeDelta = new Vector2(barWidth, _v2Base.height);
            bg.sprite = barBg;
            bg.raycastTarget = false;
            
         
        });
        // 确保背景在柱状图后面
        bg.rectTransform.SetAsFirstSibling();
        bg.rectTransform.anchoredPosition = new Vector2(x, 0);
    }

    /// <summary>
    /// 创建柱状图图像
    /// </summary>
    private void CreateBarImage(Transform parent, int groupIndex,int dataIndex, Vector2 pos, float t, Color color)
    {
        var bar = parent.GetOrCreatUIChild<RectTransform>($"data{dataIndex}", (rect) =>
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0);
                    }, typeof(Image));

        bar.anchoredPosition = new Vector2(pos.x - _v2Base.XOffset, 0);
        bar.SetAsLastSibling();

        var image = bar.GetComponent<Image>();
        SetupBarImage(image, color);

                    if (!spriteIsFill)
                    {
            bar.sizeDelta = new Vector2(barWidth, pos.y * t);
                        image.type = Image.Type.Simple;
                    }
                    else
                    {
                        image.type = Image.Type.Filled;
                        image.fillMethod = Image.FillMethod.Vertical;
            bar.sizeDelta = new Vector2(barWidth, _v2Base.height);
            image.fillAmount = pos.y * t / _v2Base.height;
        }

        SetupDataText(bar, groupIndex, dataIndex, pos);
        SetupBarInteraction(bar, dataIndex, pos, GetDataIndexForBar(parent, dataIndex), dataIndex);
    }

    /// <summary>
    /// 设置柱状图图像属性
    /// </summary>
    private void SetupBarImage(Image image, Color color)
    {
        image.sprite = sprite;
        image.raycastTarget = false;
        image.color = color;
    }

    /// <summary>
    /// 设置数据文本
    /// </summary>
    private void SetupDataText(RectTransform bar, int groupIndex, int dataIndex, Vector2 pos)
    {
                    if (openDataText)
                    {
            CreateDataText(bar, groupIndex, dataIndex);
        }
        else
        {
            RemoveDataText(bar);
        }
    }

    /// <summary>
    /// 创建数据文本
    /// </summary>
    private void CreateDataText(RectTransform bar, int groupIndex, int dataIndex)
    {
        var text = bar.GetOrCreatUIChild<TextMeshProUGUI>("dataText", (t) =>
                        {
                            t.maskable = false;
                        });

                        var textRect = text.rectTransform;
                        textRect.anchorMin = new Vector2(0.5f, 1);
                        textRect.anchorMax = new Vector2(0.5f, 1);
                        textRect.pivot = new Vector2(0.5f, 0);

#if UNITY_2023_1_OR_NEWER
                        text.textWrappingMode = TextWrappingModes.NoWrap;
#else
                        text.enableWordWrapping = false;
#endif

                        textRect.anchoredPosition = textOffset;
        text.font = textFont;
        text.alignment = TextAlignmentOptions.Center;
                        text.fontSize = textSize;
                        text.color = textColor;

        var textContent = GetDataTextContent(groupIndex, dataIndex);
                        text.text = textContent;
                        textRect.sizeDelta = text.GetPreferredValues() + new Vector2(10, 0);
    }

    /// <summary>
    /// 获取数据文本内容
    /// </summary>
    private string GetDataTextContent(int groupIndex, int dataIndex)
    {
        if (Multidimensional)
        {
            // 多维数据模式下的文本内容
            return _v2Base.datas[groupIndex].datas[dataIndex].ToLimitString(textEnd);
                    }
                    else
                    {

            return _v2Base.datas[groupIndex].datas[dataIndex].ToLimitString(textEnd);
        }
    }

    /// <summary>
    /// 移除数据文本
    /// </summary>
    private void RemoveDataText(RectTransform bar)
    {
        if (bar.childCount > 0)
        {
            DestroyImmediate(bar.GetChild(0).gameObject);
        }
    }

    /// <summary>
    /// 获取柱状图颜色
    /// </summary>
    private Color GetBarColor(int index)
    {
        return index >= _v2Base.set.colors.Count ? Color.white : _v2Base.set.colors[index];
    }

            /// <summary>
    /// 清理多余的柱状图
    /// </summary>
    private void CleanupExcessBars(Transform bar, int expectedCount)
    {
        // 计算期望的子对象数量
        int expectedChildCount = expectedCount;
        if (barBg != null)
        {
            expectedChildCount = expectedCount * 2; // 背景 + 柱状图
        }
        
        if (Application.isPlaying)
        {
            //要删除多少个
            var delateCount = bar.childCount - expectedChildCount;

            for (int z = 0; z < delateCount; z++)
            {
                Destroy(bar.GetChild(z));
            }
        }
        else
        {   
            var deleteCount = bar.childCount - expectedChildCount;
            while (deleteCount > 0)
            {
                DestroyImmediate(bar.GetChild(0).gameObject);
                deleteCount--;
            }
        }
    }

    /// <summary>
    /// 清理多余的容器
    /// </summary>
    private void CleanupExcessContainers(Transform mask, int expectedCount)
    {
        while (mask.childCount > expectedCount)
            {
                DestroyImmediate(mask.GetChild(mask.childCount - 1).gameObject);
            }
        }

    /// <summary>
    /// 设置柱状图交互
    /// </summary>
    private void SetupBarInteraction(RectTransform bar, int index, Vector2 pos, int dataIndex, int barIndex)
    {
        if (!enableHoverEffect && !enableClickEvent)
            return;

        var barKey = GetBarKey(bar);
        var image = bar.GetComponent<Image>();
        
        // 存储引用
        barTransforms[barKey] = bar;
        barImages[barKey] = image;
        originalColors[barKey] = image.color;
        originalScales[barKey] = bar.localScale;

        // 启用射线检测
        image.raycastTarget = true;

        // 获取数据值
        float dataValue = GetDataValue(dataIndex, barIndex);

        // 添加交互组件
        var barInteraction = bar.GetOrAddComponent<BarInteraction>();
        barInteraction.Initialize(this, barKey, dataIndex, barIndex, pos, dataValue);
    }

    /// <summary>
    /// 获取数据值
    /// </summary>
    private float GetDataValue(int dataIndex, int barIndex)
    {
        if (_v2Base?.datas == null || dataIndex >= _v2Base.datas.Count)
            return 0f;

        var data = _v2Base.datas[dataIndex];
        if (data?.datas == null || barIndex >= data.datas.Count)
            return 0f;

        return data.datas[barIndex];
    }

    /// <summary>
    /// 获取柱状图对应的数据索引
    /// </summary>
    private int GetDataIndexForBar(Transform parent, int barIndex)
    {
        // 从父容器名称中提取数据索引
        var parentName = parent.name;
        if (parentName.StartsWith("bar"))
        {
            var indexStr = parentName.Substring(3); // 移除"bar"前缀
            if (int.TryParse(indexStr, out int dataIndex))
            {
                return dataIndex;
            }
        }
        return 0; // 默认返回0
    }

    /// <summary>
    /// 获取柱状图的唯一键
    /// </summary>
    private string GetBarKey(RectTransform bar)
    {
        var parent = bar.parent;
        var parentName = parent.name;
        var barName = bar.name;
        return $"{parentName}_{barName}";
    }

    /// <summary>
    /// 处理柱状图悬停进入
    /// </summary>
    public void OnBarHoverEnter(string barKey, int dataIndex, int barIndex, Vector2 worldPos, float value)
    {
        if (!enableHoverEffect) return;

        currentHoveredBar = barKey;
        
        if (barTransforms.ContainsKey(barKey))
        {
            var bar = barTransforms[barKey];
            var image = barImages[barKey];
            
            // 悬停动画
            StartCoroutine(AnimateBarScale(bar, originalScales[barKey] * hoverScale, hoverAnimationTime));
            
            // 基于原始颜色计算悬停效果颜色
            var originalColor = originalColors[barKey];
            var effectColor = ApplyHoverEffect(originalColor, hoverEffectMode, hoverEffectIntensity);
            StartCoroutine(AnimateBarColor(image, effectColor, hoverAnimationTime));
        }

        // 触发悬停事件
        var hoverData = new BarHoverData
        {
            dataIndex = dataIndex,
            barIndex = barIndex,
            value = value,
            worldPosition = worldPos,
            barKey = barKey,
            isEntering = true
        };
        OnBarHovered?.Invoke(hoverData);
    }

    /// <summary>
    /// 处理柱状图悬停退出
    /// </summary>
    public void OnBarHoverExit(string barKey, int dataIndex, int barIndex, Vector2 worldPos, float value)
    {
        if (!enableHoverEffect) return;

        currentHoveredBar = "";
        
        if (barTransforms.ContainsKey(barKey))
        {
            var bar = barTransforms[barKey];
            var image = barImages[barKey];
            
            // 恢复动画
            StartCoroutine(AnimateBarScale(bar, originalScales[barKey], hoverAnimationTime));
            StartCoroutine(AnimateBarColor(image, originalColors[barKey], hoverAnimationTime));
        }

        // 触发悬停事件
        var hoverData = new BarHoverData
        {
            dataIndex = dataIndex,
            barIndex = barIndex,
            value = value,
            worldPosition = worldPos,
            barKey = barKey,
            isEntering = false
        };
        OnBarHovered?.Invoke(hoverData);
    }

    /// <summary>
    /// 处理柱状图点击
    /// </summary>
    public void OnBarClick(string barKey, int dataIndex, int barIndex, Vector2 worldPos, float value)
    {
        if (!enableClickEvent) return;

        if (barTransforms.ContainsKey(barKey))
        {
            var bar = barTransforms[barKey];
            
            // 点击动画
            StartCoroutine(ClickAnimation(bar));
        }

        // 触发点击事件
        var clickData = new BarClickData
        {
            dataIndex = dataIndex,
            barIndex = barIndex,
            value = value,
            worldPosition = worldPos,
            barKey = barKey
        };
        OnBarClicked?.Invoke(clickData);
    }

    /// <summary>
    /// 柱状图缩放动画
    /// </summary>
    private IEnumerator AnimateBarScale(RectTransform bar, Vector3 targetScale, float duration)
    {
        var startScale = bar.localScale;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;
            bar.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        bar.localScale = targetScale;
    }

    /// <summary>
    /// 柱状图颜色动画
    /// </summary>
    private IEnumerator AnimateBarColor(Image image, Color targetColor, float duration)
    {
        var startColor = image.color;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / duration;
            image.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        image.color = targetColor;
    }

    /// <summary>
    /// 点击动画
    /// </summary>
    private IEnumerator ClickAnimation(RectTransform bar)
    {
        var originalScale = bar.localScale;
        var clickScaleVector = originalScale * clickScale;
        
        // 缩小
        yield return StartCoroutine(AnimateBarScale(bar, clickScaleVector, clickAnimationTime * 0.5f));
        
        // 恢复
        yield return StartCoroutine(AnimateBarScale(bar, originalScale, clickAnimationTime * 0.5f));
    }

    

    /// <summary>
    /// 清理交互数据
    /// </summary>
    private void ClearInteractionData()
    {
        barTransforms.Clear();
        barImages.Clear();
        originalColors.Clear();
        originalScales.Clear();
        currentHoveredBar = "";
    }

    /// <summary>
    /// 应用悬停效果
    /// </summary>
    private Color ApplyHoverEffect(Color originalColor, HoverEffectMode mode, float intensity)
    {
        Color.RGBToHSV(originalColor, out float h, out float s, out float v);
        
        switch (mode)
        {
            case HoverEffectMode.Brighten:
                // 变亮：增加亮度值
                v = Mathf.Clamp01(v + intensity);
                break;
                
            case HoverEffectMode.Darken:
                // 变暗：减少亮度值
                v = Mathf.Clamp01(v - intensity);
                break;
                
            case HoverEffectMode.Saturate:
                // 增加饱和度
                s = Mathf.Clamp01(s + intensity);
                break;
                
            case HoverEffectMode.Desaturate:
                // 减少饱和度
                s = Mathf.Clamp01(s - intensity);
                break;
        }
        
        return Color.HSVToRGB(h, s, v);
    }
}
