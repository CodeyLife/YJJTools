using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;
using TMPro;

public enum PageAnimationStyle
{
    Fade,           // 淡入淡出
    Slide,          // 滑动
    Scale,          // 缩放
    Rotate,         // 旋转
    SlideAndFade    // 滑动+淡入淡出
}

[ComponentDesc("热力图")]
[ComponentOrder(15)]
public class ChartV2Component_Heatmap : ChartV2ComponetBase
{
    [Title("热力图设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("网格行数")]
    [Range(2, 24)]
    public int gridRows = 3;

    [LabelText("最小单元格宽度")]
    [Range(10f, 100f)]
    public float minCellWidth = 30f;

    [LabelText("网格间距")]
    [Range(0f, 10f)]
    public float gridSpacing = 2f;
    
    // 拖拽支持由系统自动判断，不需要手动设置

    [Title("颜色设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("最小值颜色")]
    public Color minColor = Color.blue;

    [LabelText("最大值颜色")]
    public Color maxColor = Color.red;

    [LabelText("空值颜色")]
    public Color emptyColor = new Color(0.8f, 0.8f, 0.8f, 0.3f);


    [Title("交互设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("显示数值标签")]
    public bool showValueLabels = false;

    [ShowIf("showValueLabels")]
    [LabelText("标签字体大小")]
    [Range(8f, 24f)]
    public float labelFontSize = 12f;

    [ShowIf("showValueLabels")]
    [LabelText("标签颜色")]
    public Color labelColor = Color.white;

    [LabelText("启用悬停效果")]
    public bool enableHover = true;

    [ShowIf("enableHover")]
    [LabelText("悬停时透明度")]
    [Range(0.1f, 1f)]
    public float hoverAlpha = 0.7f;

    [Title("标签设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("显示标签")]
    public bool showLabels = true;
    
    [LabelText("标签大小")]
    [Range(0.1f, 1f)]
    public float labelScale = 0.6f;
    
    [LabelText("显示数值")]
    public bool showValue = true;
    

    [Title("翻页设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("每页显示列数")]
    [Range(1, 20)]
    public int columnsPerPage = 5;
    
    [LabelText("翻页速度")]
    [Range(0.1f, 2f)]
    public float scrollSpeed = 1f;
    
    [Title("翻页动画设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("启用翻页动画")]
    public bool enablePageAnimation = true;
    
    [LabelText("翻页动画时长")]
    [Range(0.1f, 2f)]
    public float pageAnimationDuration = 0.5f;
    
    [LabelText("翻页动画曲线")]
    public AnimationCurve pageAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [LabelText("翻页动画样式")]
    public PageAnimationStyle pageAnimationStyle = PageAnimationStyle.Fade;

    // 内部数据
    private float[,] heatmapData;
    private Color[,] heatmapColors;
    private Vector2[,] gridPositions;
    private float animationProgress = 1f; // 初始化为1，表示动画已完成
    private bool isAnimating = false;
    private int hoveredRow = -1;
    private int hoveredColumn = -1;
    
    // 计算得出的列数
    private int calculatedColumns = 10;
    
    // 翻页相关
    private bool enablePagination = false; // 根据数据量自动判断
    private int currentPage = 0;
    private int totalPages = 1;
    private int startColumn = 0;
    private int endColumn = 0;
    
    // 翻页动画相关
    private bool isPageAnimating = false;
    private float pageAnimationProgress = 0f;
    private int targetPage = 0;
    private int previousPage = 0;
    
    // 标签相关 - 不再存储列表，通过命名查找

    public override void InitGraph(ChartV2Base chart)
    {
        base.InitGraph(chart);
        
        // 注册动画事件，参考漏斗图组件的实现
        _v2Base.InitAnimationEvent.AddListener(OnAnimationUpdate);
        
        // 注册滚轮翻页事件
        if (enablePagination)
        {
            _v2Base.OnWheelScrollEvent.AddListener(OnWheelScroll);
        }
        
        // 注册悬停事件
        if (enableHover)
        {
            _v2Base.OnPointerEnterEvent.AddListener(OnV2BasePointerEnter);
            _v2Base.OnPointerExitEvent.AddListener(OnV2BasePointerExit);
        }
        
        // 立即尝试初始化数据
        if (_v2Base != null)
        {
            // 设置overrideDrag为图表宽度，禁用拖拽
            _v2Base.OverrideDrag(_v2Base.width);
            
            InitializeHeatmapData();
            CheckPaginationNeed();
            CalculatePagination();
            CalculateGridPositions();
            CalculateHeatmapColors();
            
            // 在InitGraph中也尝试创建标签
            if (showLabels)
            {
                CreateOrUpdateAllLabels();
            }
            
            SetVerticesDirty();
        }
    }

    public override void SetGraph()
    {
        base.SetGraph();
        
        // 在SetGraph时进行数据计算，确保_v2Base已完全初始化
        if (_v2Base != null)
        {
            InitializeHeatmapData();
            CheckPaginationNeed();
            CalculatePagination();
            CalculateGridPositions();
            CalculateHeatmapColors();
            
            // 创建或更新标签
            if (showLabels)
            {
                CreateOrUpdateAllLabels();
            }
        }
        
        SetVerticesDirty();
    }
    
    protected override void OnDestroy()
    {
        if (_v2Base != null)
        {
            _v2Base.InitAnimationEvent.RemoveListener(OnAnimationUpdate);
            if (enablePagination)
            {
                _v2Base.OnWheelScrollEvent.RemoveListener(OnWheelScroll);
            }
            if (enableHover)
            {
                _v2Base.OnPointerEnterEvent.RemoveListener(OnV2BasePointerEnter);
                _v2Base.OnPointerExitEvent.RemoveListener(OnV2BasePointerExit);
            }
        }
        base.OnDestroy();
    }

    private void InitializeHeatmapData()
    {
        // 计算列数
        CalculateColumns();
        
        // 确保数组已正确初始化
        if (heatmapData == null || heatmapData.GetLength(0) != gridRows || heatmapData.GetLength(1) != calculatedColumns)
        {
            heatmapData = new float[gridRows, calculatedColumns];
        }
        
        if (heatmapColors == null || heatmapColors.GetLength(0) != gridRows || heatmapColors.GetLength(1) != calculatedColumns)
        {
            heatmapColors = new Color[gridRows, calculatedColumns];
        }
        
        if (gridPositions == null || gridPositions.GetLength(0) != gridRows || gridPositions.GetLength(1) != calculatedColumns)
        {
            gridPositions = new Vector2[gridRows, calculatedColumns];
        }
        
        // 从ChartV2数据源生成热力图数据
        GenerateHeatmapFromChartData();
    }

    private void CalculateColumns()
    {
        if (_v2Base == null || _v2Base.datas == null || _v2Base.datas.Count == 0)
        {
            calculatedColumns = 10; // 默认值
            return;
        }
        
        // 获取数据总数
        int totalDataPoints = _v2Base.datas[0].datas.Count;
        
        // 计算实际需要的列数（基于数据量，不考虑显示限制）
        calculatedColumns = Mathf.CeilToInt((float)totalDataPoints / gridRows);
        
        // 确保至少有一列
        calculatedColumns = Mathf.Max(1, calculatedColumns);
        
    }
    
    private void CheckPaginationNeed()
    {
        if (_v2Base == null) return;
        
        // 计算可用宽度
        float availableWidth = _v2Base.width - _v2Base.set.distanceFromLeft - _v2Base.set.distanceFromRight;
        
        // 计算在可用宽度内最多能显示多少列
        int maxColumnsInView = Mathf.FloorToInt((availableWidth + gridSpacing) / (minCellWidth + gridSpacing));
        maxColumnsInView = Mathf.Max(1, maxColumnsInView);
        
        
        // 如果总列数大于最多能显示的列数，则需要翻页
        bool needPagination = calculatedColumns > maxColumnsInView;
        
        
        // 如果翻页状态发生变化，需要重新注册/移除事件
        if (needPagination != enablePagination)
        {
            enablePagination = needPagination;
            
            if (enablePagination)
            {
                // 注册滚轮翻页事件
                _v2Base.OnWheelScrollEvent.AddListener(OnWheelScroll);
        }
        else
        {
                // 移除滚轮翻页事件
                _v2Base.OnWheelScrollEvent.RemoveListener(OnWheelScroll);
            }
        }
    }
    
    private void CalculatePagination()
    {
        if (!enablePagination)
        {
            // 如果禁用翻页，显示所有列
            startColumn = 0;
            endColumn = calculatedColumns - 1;
            totalPages = 1;
            currentPage = 0;
            return;
        }
        
        // 计算可用宽度内最多能显示多少列
        float availableWidth = _v2Base.width - _v2Base.set.distanceFromLeft - _v2Base.set.distanceFromRight;
        int maxColumnsInView = Mathf.FloorToInt((availableWidth + gridSpacing) / (minCellWidth + gridSpacing));
        maxColumnsInView = Mathf.Max(1, maxColumnsInView);
        
        // 使用实际能显示的列数作为每页列数
        int actualColumnsPerPage = Mathf.Min(columnsPerPage, maxColumnsInView);
        
        // 计算总页数
        totalPages = Mathf.CeilToInt((float)calculatedColumns / actualColumnsPerPage);
        totalPages = Mathf.Max(1, totalPages);
        
        // 确保当前页在有效范围内
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);
        
        // 计算当前页的列范围
        startColumn = currentPage * actualColumnsPerPage;
        endColumn = Mathf.Min(startColumn + actualColumnsPerPage - 1, calculatedColumns - 1);
        
    }
    
    private void OnWheelScroll(float scrollDelta)
    {
        if (!enablePagination) 
        {
            return;
        }
        
        if (isPageAnimating)
        {
            return;
        }
        
        
        int newPage = currentPage;
        
        // 根据滚轮方向翻页
        if (scrollDelta > 0)
        {
            // 向上滚动，上一页
            if (currentPage > 0)
            {
                newPage = currentPage - 1;
            }
            else
            {
                return;
            }
        }
        else if (scrollDelta < 0)
        {
            // 向下滚动，下一页
            if (currentPage < totalPages - 1)
            {
                newPage = currentPage + 1;
            }
            else
            {
                return;
            }
        }
        
        // 开始翻页动画
        StartPageAnimation(newPage);
    }
    
    private void RefreshPagination()
    {
        
        CalculatePagination();
        CalculateGridPositions();
        CalculateHeatmapColors();
        
        if (showLabels)
        {
            CreateOrUpdateAllLabels();
        }
        if (_v2Base.set.openAnimation)
        {
            _v2Base.StopAllCoroutines();
            _v2Base.Play();
        }
        else
        {
            SetVerticesDirty();
        }

    }
    
    private void StartPageAnimation(int newPage)
    {
        if (!enablePageAnimation)
        {
            // 如果禁用动画，直接翻页
            currentPage = newPage;
            RefreshPagination();
            return;
        }
        
        if (isPageAnimating)
        {
            return;
        }
        
        previousPage = currentPage;
        targetPage = newPage;
        isPageAnimating = true;
        pageAnimationProgress = 0f;
        
        
        StartCoroutine(PageAnimationCoroutine());
    }
    
    private IEnumerator PageAnimationCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < pageAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            pageAnimationProgress = elapsedTime / pageAnimationDuration;
            
            // 应用动画曲线
            float curveValue = pageAnimationCurve.Evaluate(pageAnimationProgress);
            
            // 根据动画样式应用不同的效果
            ApplyPageAnimation(curveValue);
            
            // 更新标签动画
            UpdateLabelsAnimation(curveValue);
            
            SetVerticesDirty();
            yield return null;
        }
        
        // 动画完成
        pageAnimationProgress = 1f;
        currentPage = targetPage;
        isPageAnimating = false;
        
        // 重新计算翻页数据
        CalculatePagination();
        CalculateGridPositions();
        CalculateHeatmapColors();
        
        if (showLabels)
        {
            CreateOrUpdateAllLabels();
        }
        
        SetVerticesDirty();
        
    }
    
    private void ApplyPageAnimation(float progress)
    {
        // 根据动画样式应用不同的效果
        switch (pageAnimationStyle)
        {
            case PageAnimationStyle.Fade:
                ApplyFadeAnimation(progress);
                break;
            case PageAnimationStyle.Slide:
                ApplySlideAnimation(progress);
                break;
            case PageAnimationStyle.Scale:
                ApplyScaleAnimation(progress);
                break;
            case PageAnimationStyle.Rotate:
                ApplyRotateAnimation(progress);
                break;
            case PageAnimationStyle.SlideAndFade:
                ApplySlideAnimation(progress);
                ApplyFadeAnimation(progress);
                            break;
                        }
    }
    
    private void ApplyFadeAnimation(float progress)
    {
        // 淡入淡出效果通过修改颜色透明度实现
        // 在绘制时应用
    }
    
    private void ApplySlideAnimation(float progress)
    {
        // 滑动效果通过修改位置实现
        // 在CalculateGridPositions中应用
    }
    
    private void ApplyScaleAnimation(float progress)
    {
        // 缩放效果通过修改单元格大小实现
        // 在绘制时应用
    }
    
    private void ApplyRotateAnimation(float progress)
    {
        // 旋转效果通过修改单元格旋转实现
        // 在绘制时应用
    }
    
    private void ApplyCellAnimation(ref Color cellColor, ref Vector2 cellPosition, int row, int col)
    {
        if (!isPageAnimating) return;
        
        float progress = pageAnimationCurve.Evaluate(pageAnimationProgress);
        
        switch (pageAnimationStyle)
        {
            case PageAnimationStyle.Fade:
                ApplyCellFadeAnimation(ref cellColor, progress, row, col);
                break;
            case PageAnimationStyle.Slide:
                ApplyCellSlideAnimation(ref cellPosition, progress, row, col);
                break;
            case PageAnimationStyle.Scale:
                ApplyCellScaleAnimation(ref cellColor, progress, row, col);
                break;
            case PageAnimationStyle.Rotate:
                ApplyCellRotateAnimation(ref cellPosition, progress, row, col);
                break;
            case PageAnimationStyle.SlideAndFade:
                ApplyCellFadeAnimation(ref cellColor, progress, row, col);
                ApplyCellSlideAnimation(ref cellPosition, progress, row, col);
                break;
        }
    }
    
    private void ApplyCellFadeAnimation(ref Color cellColor, float progress, int row, int col)
    {
        // 淡入淡出效果
        bool isCurrentPage = (col >= startColumn && col <= endColumn);
        bool isTargetPage = (col >= GetTargetPageStartColumn() && col <= GetTargetPageEndColumn());
        
        if (isCurrentPage && !isTargetPage)
        {
            // 当前页的单元格淡出
            cellColor.a *= (1f - progress);
        }
        else if (!isCurrentPage && isTargetPage)
        {
            // 目标页的单元格淡入
            cellColor.a *= progress;
        }
        else if (isCurrentPage && isTargetPage)
        {
            // 两页都包含的单元格保持不透明
            cellColor.a *= 1f;
                }
                else
                {
            // 其他单元格完全透明
            cellColor.a *= 0f;
        }
    }
    
    private void ApplyCellSlideAnimation(ref Vector2 cellPosition, float progress, int row, int col)
    {
        // 滑动效果 - 根据翻页方向显示不同效果
        bool isCurrentPage = (col >= startColumn && col <= endColumn);
        bool isTargetPage = (col >= GetTargetPageStartColumn() && col <= GetTargetPageEndColumn());
        
        // 判断翻页方向
        bool isForwardPage = targetPage > previousPage;
        
        if (isCurrentPage && !isTargetPage)
        {
            // 当前页的单元格滑出
            float slideOffset = (1f - progress) * _v2Base.width;
            if (isForwardPage)
            {
                // 向前翻页：当前页向左滑出
                cellPosition.x -= slideOffset;
            }
            else
            {
                // 向后翻页：当前页向右滑出
                cellPosition.x += slideOffset;
            }
        }
        else if (!isCurrentPage && isTargetPage)
        {
            // 目标页的单元格滑入
            float slideOffset = (1f - progress) * _v2Base.width;
            if (isForwardPage)
            {
                // 向前翻页：目标页从右滑入
                cellPosition.x += slideOffset;
            }
            else
            {
                // 向后翻页：目标页从左滑入
                cellPosition.x -= slideOffset;
            }
        }
    }
    
    private void ApplyCellScaleAnimation(ref Color cellColor, float progress, int row, int col)
    {
        // 缩放效果通过修改透明度和颜色强度模拟
        bool isCurrentPage = (col >= startColumn && col <= endColumn);
        bool isTargetPage = (col >= GetTargetPageStartColumn() && col <= GetTargetPageEndColumn());
        
        if (isCurrentPage && !isTargetPage)
        {
            // 当前页的单元格缩小并淡出
            float scale = 1f - progress * 0.8f; // 更明显的缩小效果
            cellColor.a *= scale;
            // 增加颜色强度变化
            cellColor.r = Mathf.Lerp(cellColor.r, 0.3f, progress);
            cellColor.g = Mathf.Lerp(cellColor.g, 0.3f, progress);
            cellColor.b = Mathf.Lerp(cellColor.b, 0.3f, progress);
        }
        else if (!isCurrentPage && isTargetPage)
        {
            // 目标页的单元格放大并淡入
            float scale = 0.2f + progress * 0.8f; // 从更小开始放大
            cellColor.a *= scale;
            // 增加颜色强度变化
            cellColor.r = Mathf.Lerp(0.3f, cellColor.r, progress);
            cellColor.g = Mathf.Lerp(0.3f, cellColor.g, progress);
            cellColor.b = Mathf.Lerp(0.3f, cellColor.b, progress);
        }
    }
    
    private void ApplyCellRotateAnimation(ref Vector2 cellPosition, float progress, int row, int col)
    {
        // 旋转效果通过位置偏移模拟
        bool isCurrentPage = (col >= startColumn && col <= endColumn);
        bool isTargetPage = (col >= GetTargetPageStartColumn() && col <= GetTargetPageEndColumn());
        
        if (isCurrentPage && !isTargetPage)
        {
            // 当前页的单元格旋转消失
            float rotation = progress * Mathf.PI;
            float offset = Mathf.Sin(rotation) * 50f;
            cellPosition.y += offset;
            cellPosition.x += Mathf.Cos(rotation) * 20f;
        }
        else if (!isCurrentPage && isTargetPage)
        {
            // 目标页的单元格旋转出现
            float rotation = (1f - progress) * Mathf.PI;
            float offset = Mathf.Sin(rotation) * 50f;
            cellPosition.y += offset;
            cellPosition.x += Mathf.Cos(rotation) * 20f;
        }
    }
    
    private int GetTargetPageStartColumn()
    {
        if (targetPage < 0 || targetPage >= totalPages) return 0;
        
        float availableWidth = _v2Base.width - _v2Base.set.distanceFromLeft - _v2Base.set.distanceFromRight;
        int maxColumnsInView = Mathf.FloorToInt((availableWidth + gridSpacing) / (minCellWidth + gridSpacing));
        maxColumnsInView = Mathf.Max(1, maxColumnsInView);
        int actualColumnsPerPage = Mathf.Min(columnsPerPage, maxColumnsInView);
        
        return targetPage * actualColumnsPerPage;
    }
    
    private int GetTargetPageEndColumn()
    {
        if (targetPage < 0 || targetPage >= totalPages) return 0;
        
        float availableWidth = _v2Base.width - _v2Base.set.distanceFromLeft - _v2Base.set.distanceFromRight;
        int maxColumnsInView = Mathf.FloorToInt((availableWidth + gridSpacing) / (minCellWidth + gridSpacing));
        maxColumnsInView = Mathf.Max(1, maxColumnsInView);
        int actualColumnsPerPage = Mathf.Min(columnsPerPage, maxColumnsInView);
        
        return Mathf.Min(targetPage * actualColumnsPerPage + actualColumnsPerPage - 1, calculatedColumns - 1);
    }
    
    private void UpdateLabelsAnimation(float progress)
    {
        if (!showLabels) return;
        
        // 通过命名查找所有热力图标签
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < calculatedColumns; col++)
            {
                var labelName = $"HeatmapLabel_{row}_{col}";
                var labelTransform = transform.Find(labelName);
                
                if (labelTransform != null)
                {
                    var label = labelTransform.GetComponent<TextMeshProUGUI>();
                    if (label != null)
                    {
                        // 检查是否有数据
                        if (HasDataAt(row, col))
                        {
                            // 应用翻页动画效果到标签
                            ApplyLabelAnimation(label, progress, row, col);
                        }
                        else
                        {
                            // 没有数据，隐藏标签
                            label.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }
    
    private void ApplyLabelAnimation(TextMeshProUGUI label, float progress, int row, int col)
    {
        if (!isPageAnimating) return;
        
        // 获取标签的原始位置
        Vector2 originalPosition = GetLabelPosition(row, col);
        Color originalColor = labelColor;
        
        // 根据动画样式应用不同的效果
        switch (pageAnimationStyle)
        {
            case PageAnimationStyle.Fade:
                ApplyLabelFadeAnimation(label, progress, row, col, originalColor);
                break;
            case PageAnimationStyle.Slide:
                ApplyLabelSlideAnimation(label, progress, row, col, originalPosition);
                break;
            case PageAnimationStyle.Scale:
                ApplyLabelScaleAnimation(label, progress, row, col, originalColor);
                break;
            case PageAnimationStyle.Rotate:
                ApplyLabelRotateAnimation(label, progress, row, col, originalPosition);
                break;
            case PageAnimationStyle.SlideAndFade:
                ApplyLabelFadeAnimation(label, progress, row, col, originalColor);
                ApplyLabelSlideAnimation(label, progress, row, col, originalPosition);
                break;
        }
    }
    
    private void ApplyLabelFadeAnimation(TextMeshProUGUI label, float progress, int row, int col, Color originalColor)
    {
        // 淡入淡出效果
        bool isCurrentPage = (col >= startColumn && col <= endColumn);
        bool isTargetPage = (col >= GetTargetPageStartColumn() && col <= GetTargetPageEndColumn());
        
        if (isCurrentPage && !isTargetPage)
        {
            // 当前页的标签淡出
            originalColor.a *= (1f - progress);
            label.color = originalColor;
        }
        else if (!isCurrentPage && isTargetPage)
        {
            // 目标页的标签淡入
            originalColor.a *= progress;
            label.color = originalColor;
        }
        else if (isCurrentPage && isTargetPage)
        {
            // 两页都包含的标签保持不透明
            label.color = originalColor;
        }
        else
        {
            // 其他标签完全透明
            originalColor.a = 0f;
            label.color = originalColor;
        }
    }
    
    private void ApplyLabelSlideAnimation(TextMeshProUGUI label, float progress, int row, int col, Vector2 originalPosition)
    {
        // 滑动效果 - 根据翻页方向显示不同效果
        bool isCurrentPage = (col >= startColumn && col <= endColumn);
        bool isTargetPage = (col >= GetTargetPageStartColumn() && col <= GetTargetPageEndColumn());
        
        // 判断翻页方向
        bool isForwardPage = targetPage > previousPage;
        
        Vector2 animatedPosition = originalPosition;
        
        if (isCurrentPage && !isTargetPage)
        {
            // 当前页的标签滑出
            float slideOffset = (1f - progress) * _v2Base.width;
            if (isForwardPage)
            {
                // 向前翻页：当前页向左滑出
                animatedPosition.x -= slideOffset;
            }
            else
            {
                // 向后翻页：当前页向右滑出
                animatedPosition.x += slideOffset;
            }
        }
        else if (!isCurrentPage && isTargetPage)
        {
            // 目标页的标签滑入
            float slideOffset = (1f - progress) * _v2Base.width;
            if (isForwardPage)
            {
                // 向前翻页：目标页从右滑入
                animatedPosition.x += slideOffset;
            }
            else
            {
                // 向后翻页：目标页从左滑入
                animatedPosition.x -= slideOffset;
            }
        }
        
        label.rectTransform.anchoredPosition = animatedPosition;
    }
    
    private void ApplyLabelScaleAnimation(TextMeshProUGUI label, float progress, int row, int col, Color originalColor)
    {
        // 缩放效果通过修改透明度和字体大小模拟
        bool isCurrentPage = (col >= startColumn && col <= endColumn);
        bool isTargetPage = (col >= GetTargetPageStartColumn() && col <= GetTargetPageEndColumn());
        
        if (isCurrentPage && !isTargetPage)
        {
            // 当前页的标签缩小并淡出
            float scale = 1f - progress * 0.8f;
            originalColor.a *= scale;
            label.color = originalColor;
            label.fontSize = CalculateLabelSize(row, col) * labelScale * scale;
        }
        else if (!isCurrentPage && isTargetPage)
        {
            // 目标页的标签放大并淡入
            float scale = 0.2f + progress * 0.8f;
            originalColor.a *= scale;
            label.color = originalColor;
            label.fontSize = CalculateLabelSize(row, col) * labelScale * scale;
        }
        else
        {
            label.color = originalColor;
            label.fontSize = CalculateLabelSize(row, col) * labelScale;
        }
    }
    
    private void ApplyLabelRotateAnimation(TextMeshProUGUI label, float progress, int row, int col, Vector2 originalPosition)
    {
        // 旋转效果通过位置偏移模拟
        bool isCurrentPage = (col >= startColumn && col <= endColumn);
        bool isTargetPage = (col >= GetTargetPageStartColumn() && col <= GetTargetPageEndColumn());
        
        Vector2 animatedPosition = originalPosition;
        
        if (isCurrentPage && !isTargetPage)
        {
            // 当前页的标签旋转消失
            float rotation = progress * Mathf.PI;
            float offset = Mathf.Sin(rotation) * 50f;
            animatedPosition.y += offset;
            animatedPosition.x += Mathf.Cos(rotation) * 20f;
        }
        else if (!isCurrentPage && isTargetPage)
        {
            // 目标页的标签旋转出现
            float rotation = (1f - progress) * Mathf.PI;
            float offset = Mathf.Sin(rotation) * 50f;
            animatedPosition.y += offset;
            animatedPosition.x += Mathf.Cos(rotation) * 20f;
        }
        
        label.rectTransform.anchoredPosition = animatedPosition;
    }

    private void GenerateHeatmapFromChartData()
    {
        // 如果ChartV2有数据，使用ChartV2的数据
        if (_v2Base != null && _v2Base.datas != null && _v2Base.datas.Count > 0)
        {
            // 将ChartV2的线性数据转换为二维热力图数据
            ConvertLinearDataToHeatmap();
        }
        else
        {
            // 如果没有数据，生成示例数据
            GenerateSampleData();
        }

         CalculateMinMaxFromData();
    }

    private void ConvertLinearDataToHeatmap()
    {
        // 只使用第0个数据序列
        if (_v2Base.datas == null || _v2Base.datas.Count == 0 || _v2Base.datas[0].datas == null)
        {
            GenerateSampleData();
            return;
        }
        
        var firstDataSeries = _v2Base.datas[0].datas;
        int totalDataPoints = firstDataSeries.Count;
        
        if (totalDataPoints == 0)
        {
            GenerateSampleData();
            return;
        }
        
        // 将第0个数据序列映射到二维网格
        int dataIndex = 0;
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < calculatedColumns; col++)
            {
                if (dataIndex < totalDataPoints)
                {
                    // 只使用第0个数据序列的值
                    heatmapData[row, col] = firstDataSeries[dataIndex];
                    dataIndex++;
                }
                else
                {
                    heatmapData[row, col] = float.NaN; // 使用NaN标记填充位置
                }
            }
        }
    }

    private void GenerateSampleData()
    {
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < calculatedColumns; col++)
            {
                // 生成一些示例数据，可以替换为实际数据
                float value = Random.Range(0f, 100f);
                heatmapData[row, col] = value;
            }
        }
    }

    private void CalculateMinMaxFromData()
    {
        // 使用v2base的数据范围计算
        if (_v2Base != null)
        {
            _v2Base.ComputeMaxAndMin();
        }
    }

    private void CalculateGridPositions()
    {
        if (_v2Base == null) return;

        // 确保gridPositions数组已初始化（使用总列数）
        if (gridPositions == null || gridPositions.GetLength(0) != gridRows || gridPositions.GetLength(1) != calculatedColumns)
        {
            gridPositions = new Vector2[gridRows, calculatedColumns];
        }

        float chartWidth = _v2Base.width - _v2Base.set.distanceFromLeft - _v2Base.set.distanceFromRight;
        float chartHeight = _v2Base.height - _v2Base.set.distanceFromTop - _v2Base.set.distanceFromButtom;

        // 计算当前页实际显示的列数
        int currentPageColumns = endColumn - startColumn + 1;
        
        // 根据当前页的列数重新计算单元格宽度，使其填满整个图表宽度
        float cellWidth = (chartWidth - (currentPageColumns - 1) * gridSpacing) / currentPageColumns;
        float cellHeight = (chartHeight - (gridRows - 1) * gridSpacing) / gridRows;

        float startX = _v2Base.set.distanceFromLeft;
        float startY = _v2Base.set.distanceFromButtom;

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < calculatedColumns; col++)
            {
                if (row >= gridPositions.GetLength(0) || col >= gridPositions.GetLength(1))
                {
                    continue;
                }

                // 计算在当前页中的相对列索引
                int relativeCol = col - startColumn;
                
                // 只计算当前页范围内的位置
                if (col >= startColumn && col <= endColumn)
                {
                    float x = startX + relativeCol * (cellWidth + gridSpacing) + cellWidth * 0.5f;
                float y = startY + row * (cellHeight + gridSpacing) + cellHeight * 0.5f;
                gridPositions[row, col] = new Vector2(x, y);
            }
                else
                {
                    // 不在当前页的列，位置设为0（不会被绘制）
                    gridPositions[row, col] = Vector2.zero;
            }
        }
        }
        
    }

    private void CalculateHeatmapColors()
    {
        if (heatmapData == null || heatmapColors == null)
        {
            return;
        }

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < calculatedColumns; col++)
            {
                if (row >= heatmapData.GetLength(0) || col >= heatmapData.GetLength(1) ||
                    row >= heatmapColors.GetLength(0) || col >= heatmapColors.GetLength(1))
                {
                    continue;
                }

                float value = heatmapData[row, col];
                heatmapColors[row, col] = GetColorForValue(value);
            }
        }
    }

    private Color GetColorForValue(float value)
    {
        if (_v2Base == null)
        {
            return minColor;
        }

        // 如果数据范围无效，返回最小值颜色
        if (_v2Base.max <= _v2Base.min)
        {
            return minColor;
        }

        // 如果是NaN，按0值处理
        if (float.IsNaN(value))
        {
            value = 0f;
        }

        float normalizedValue = Mathf.Clamp01((value - _v2Base.min) / (_v2Base.max - _v2Base.min));
        Color result = Color.Lerp(minColor, maxColor, normalizedValue);
        
        // 确保颜色有足够的透明度以显示
        if (result.a < 0.1f)
        {
            result.a = 0.5f; // 设置最小透明度
        }
        
        return result;
    }

    private void OnAnimationUpdate(float progress)
    {
        // 使用_v2Base.set的动画参数
        if (!_v2Base.set.openAnimation) return;
        
        animationProgress = _v2Base.set.curve.Evaluate(progress);
        isAnimating = progress < 1f;
        
        
        // 更新标签可见性
        if (showLabels)
        {
            UpdateLabelsVisibility();
        }
        
        SetVerticesDirty();
    }
    

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        if (_v2Base == null) 
        {
            return;
        }

        // 确保数据已初始化
        if (heatmapData == null || heatmapColors == null || gridPositions == null)
        {
            return; // 数据未初始化，跳过绘制
        }

        int drawnCells = 0;
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = startColumn; col <= endColumn; col++)
            {
                // 简化的边界检查
                if (row >= heatmapData.GetLength(0) || col >= heatmapData.GetLength(1)) continue;
                if (row >= heatmapColors.GetLength(0) || col >= heatmapColors.GetLength(1)) continue;
                if (row >= gridPositions.GetLength(0) || col >= gridPositions.GetLength(1)) continue;

                float value = heatmapData[row, col];
                // 注释掉跳过0值的逻辑，允许显示0值
                // if (value == 0f) continue; // 跳过空值

                Color cellColor = heatmapColors[row, col];

                // 应用动画效果 - 使用_v2Base.set的动画设置
                if (_v2Base.set.openAnimation && animationProgress < 1f)
                {
                    // 计算当前单元格的进度（按行优先顺序）
                    float cellProgress = (float)(row * calculatedColumns + col) / (gridRows * calculatedColumns);
                    
                    // 顺序动画：只有当单元格进度小于动画进度时才显示
                    if (cellProgress > animationProgress) continue;
                    
                    // 计算单元格的动画缩放
                    float cellAnimationProgress = Mathf.Clamp01((animationProgress * (gridRows * calculatedColumns)) - (row * calculatedColumns + col));
                    cellColor.a *= cellAnimationProgress;
                }

                // 应用悬停效果
                if (enableHover && hoveredRow == row && hoveredColumn == col)
                {
                    cellColor.a *= hoverAlpha;
                }

                // 获取单元格位置
                Vector2 cellPosition = gridPositions[row, col];

                // 应用翻页动画效果
                if (isPageAnimating)
                {
                    ApplyCellAnimation(ref cellColor, ref cellPosition, row, col);
                }

                DrawHeatmapCell(vh, cellPosition, value, cellColor);
                drawnCells++;
            }
        }
        
    }

    private void DrawHeatmapCell(VertexHelper vh, Vector2 center, float value, Color color)
    {
        float chartWidth = _v2Base.width - _v2Base.set.distanceFromLeft - _v2Base.set.distanceFromRight;
        float chartHeight = _v2Base.height - _v2Base.set.distanceFromTop - _v2Base.set.distanceFromButtom;

        // 计算当前页实际显示的列数
        int currentPageColumns = endColumn - startColumn + 1;
        
        // 根据当前页的列数重新计算单元格宽度，使其填满整个图表宽度
        float cellWidth = (chartWidth - (currentPageColumns - 1) * gridSpacing) / currentPageColumns;
        float cellHeight = (chartHeight - (gridRows - 1) * gridSpacing) / gridRows;

        Vector2 halfSize = new Vector2(cellWidth * 0.5f, cellHeight * 0.5f);

        // 计算圆角半径（单元格较小边的20%，确保圆角可见）
        float cornerRadius = Mathf.Min(cellWidth, cellHeight) * 0.2f;

        // 绘制圆角矩形单元格
        //Vector2 topLeft = center - halfSize;
        //Vector2 topRight = center + new Vector2(halfSize.x, -halfSize.y);
        //Vector2 bottomRight = center + halfSize;
        //Vector2 bottomLeft = center + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topLeft = center + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = center + new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomRight = center + new Vector2(halfSize.x, -halfSize.y);
        Vector2 bottomLeft = center + new Vector2(-halfSize.x, -halfSize.y);


        // 使用新的圆角矩形绘制方法
        Yjj_ChartUtility.DrawRoundQuad(vh, topLeft, topRight, bottomRight, bottomLeft, cornerRadius, color);

        // 绘制数值标签
        if (showValueLabels)
        {
            DrawValueLabel(vh, center, value, color);
        }
    }

    private void DrawValueLabel(VertexHelper vh, Vector2 center, float value, Color backgroundColor)
    {
        // 如果是NaN，不绘制标签
        if (float.IsNaN(value)) return;
        
        // 计算标签文本
        string labelText = value.ToString("F1");
        
        // 计算标签位置和大小
        float chartWidth = _v2Base.width - _v2Base.set.distanceFromLeft - _v2Base.set.distanceFromRight;
        float chartHeight = _v2Base.height - _v2Base.set.distanceFromTop - _v2Base.set.distanceFromButtom;
        int currentPageColumns = endColumn - startColumn + 1;
        float cellWidth = (chartWidth - (currentPageColumns - 1) * gridSpacing) / currentPageColumns;
        float cellHeight = (chartHeight - (gridRows - 1) * gridSpacing) / gridRows;
        
        // 计算字体大小（基于单元格大小）
        float fontSize = Mathf.Min(cellWidth, cellHeight) * 0.3f;
        
        // 计算文本颜色（与背景色形成对比）
        Color textColor = (backgroundColor.r + backgroundColor.g + backgroundColor.b) / 3f > 0.5f ? Color.black : Color.white;
        
        // 绘制文本背景（可选）
        float padding = fontSize * 0.2f;
        Vector2 textSize = new Vector2(fontSize * labelText.Length * 0.6f, fontSize);
        Vector2 bgSize = textSize + new Vector2(padding * 2, padding * 2);
        
        // 绘制背景矩形
        Vector2 bgTopLeft = center + new Vector2(-bgSize.x * 0.5f, bgSize.y * 0.5f);
        Vector2 bgTopRight = center + new Vector2(bgSize.x * 0.5f, bgSize.y * 0.5f);
        Vector2 bgBottomRight = center + new Vector2(bgSize.x * 0.5f, -bgSize.y * 0.5f);
        Vector2 bgBottomLeft = center + new Vector2(-bgSize.x * 0.5f, -bgSize.y * 0.5f);
        
        Color bgColor = new Color(0, 0, 0, 0.7f);
        Yjj_ChartUtility.DrawRoundQuad(vh, bgTopLeft, bgTopRight, bgBottomRight, bgBottomLeft, 2f, bgColor);
        
        // 注意：实际的文本绘制需要使用TextMeshPro或其他文本组件
        // 这里只是绘制了背景，真正的文本需要在CreateOrUpdateAllLabels中处理
    }

    // 公共方法，用于设置热力图数据
    public void SetHeatmapData(float[,] data)
    {
        if (data == null)
        {
            return;
        }
        
        if (data.GetLength(0) != gridRows || data.GetLength(1) != calculatedColumns)
        {
            return;
        }
        
        // 确保数组已初始化
        if (heatmapData == null || heatmapData.GetLength(0) != gridRows || heatmapData.GetLength(1) != calculatedColumns)
        {
            heatmapData = new float[gridRows, calculatedColumns];
        }
        
        // 复制数据
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = 0; col < calculatedColumns; col++)
            {
                heatmapData[row, col] = data[row, col];
            }
        }
        
            CalculateMinMaxFromData();
        CalculateHeatmapColors();
        SetVerticesDirty();
    }

    // 动态调整网格大小
    public void SetGridSize(int rows)
    {
        if (rows < 2)
        {
            return;
        }

        gridRows = rows;

        // 重新初始化数据
        InitializeHeatmapData();
        CalculateGridPositions();
        SetVerticesDirty();
    }

    // 获取当前数据
    public float[,] GetHeatmapData()
    {
        return heatmapData;
    }

    // 获取指定位置的数据值
    public float GetDataValue(int row, int column)
    {
        if (row >= 0 && row < gridRows && column >= 0 && column < calculatedColumns)
        {
            return heatmapData[row, column];
        }
        return 0f;
    }

    // 设置指定位置的数据值
    public void SetDataValue(int row, int column, float value)
    {
        if (row >= 0 && row < gridRows && column >= 0 && column < calculatedColumns)
        {
            heatmapData[row, column] = value;
                CalculateMinMaxFromData();
            CalculateHeatmapColors();
            SetVerticesDirty();
        }
    }

    // 强制刷新热力图
    public void ForceRefresh()
    {
        if (_v2Base != null)
        {
            InitializeHeatmapData();
            CalculateGridPositions();
            CalculateHeatmapColors();
            
            // 更新标签
            if (showLabels)
            {
                CreateOrUpdateAllLabels();
            }
            
            SetVerticesDirty();
        }
    }
    
    private void CreateOrUpdateAllLabels()
    {
        // 删除所有现有的热力图标签
        DeleteAllHeatmapLabels();
        
        if (!showLabels || heatmapData == null) 
        {
            return;
        }
        
        // 创建新标签，只为有数据的单元格创建
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = startColumn; col <= endColumn; col++)
            {
                // 检查是否有数据
                if (HasDataAt(row, col))
                {
                var text = GetLabelText(row, col);
                    
                    // 只有当文本不为空时才创建和显示标签
                    if (!string.IsNullOrEmpty(text))
                    {
                        var label = GetOrCreateLabel(row, col);
                var position = GetLabelPosition(row, col);
                
                label.text = text;
                label.rectTransform.anchoredPosition = position;
                label.color = labelColor;
                label.font = _v2Base.set.font;
                label.fontSize = CalculateLabelSize(row, col) * labelScale;
                label.gameObject.SetActive(true);
                    }
                }
            }
        }
    }
    
    private void DeleteAllHeatmapLabels()
    {
        // 查找所有以"HeatmapLabel_"开头的子物体并删除
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("HeatmapLabel_"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
    
    private bool HasDataAt(int row, int col)
    {
        // 检查边界
        if (row < 0 || row >= gridRows || col < 0 || col >= calculatedColumns)
        {
            return false;
        }
        
        // 检查热力图数据
        if (heatmapData == null || row >= heatmapData.GetLength(0) || col >= heatmapData.GetLength(1))
        {
            return false;
        }
        
        // 检查是否有有效数据（非NaN，包括0值）
        float value = heatmapData[row, col];
        return !float.IsNaN(value);
    }
    
    private TextMeshProUGUI GetOrCreateLabel(int row, int col)
    {
        var label = transform.GetOrCreatUIChild<TextMeshProUGUI>($"HeatmapLabel_{row}_{col}", CreatNewAction: (t) =>
        {
            t.fontSize = 12f;
            t.color = labelColor;
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;

            
            var rectTransform = t.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.zero;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(100, 20);
        });

        // 使用V2BaseSet中的字体设置
        if (_v2Base != null && _v2Base.set != null && _v2Base.set.font != null)
        {
            label.font = _v2Base.set.font;
        }

        return label;
    }
    
    private string GetLabelText(int row, int col)
    {
        var parts = new List<string>();
        
        if (showValue)
        {
            float value = heatmapData[row, col];
            
            // 如果是NaN（填充数据），不显示数值
            if (!float.IsNaN(value))
            {
            parts.Add(value.ToString("F1"));
            }
        }
        
        // 添加对应的names内容作为标签
        if (_v2Base != null && _v2Base.names != null && _v2Base.names.Count > 0)
        {
            int dataIndex = row * calculatedColumns + col;
            if (dataIndex < _v2Base.names.Count)
            {
                parts.Add(_v2Base.names[dataIndex]);
            }
        }
        
        return string.Join("\n", parts);
    }
    
    private Vector2 GetLabelPosition(int row, int col)
    {
        if (gridPositions == null) return Vector2.zero;
        
        Vector2 cellCenter = gridPositions[row, col];
        
        // 应用与绘制时相同的偏移
        var offset = new Vector2(_v2Base.XOffset, 0);
        Vector2 adjustedPosition = cellCenter - offset;
        
        // 将图表坐标系转换为UI坐标系（中心为原点）
        // 图表坐标系：左下角为原点，Y轴向上
        // UI坐标系：中心为原点，Y轴向上
        float chartCenterX = _v2Base.width * 0.5f;
        float chartCenterY = _v2Base.height * 0.5f;
        
        // 直接使用调整后的位置作为UI坐标
        Vector2 uiPosition = adjustedPosition;
        
        
        return uiPosition;
    }
    
    private float CalculateLabelSize(int row, int col)
    {
        float chartWidth = _v2Base.width - _v2Base.set.distanceFromLeft - _v2Base.set.distanceFromRight;
        float chartHeight = _v2Base.height - _v2Base.set.distanceFromTop - _v2Base.set.distanceFromButtom;
        
        // 计算当前页实际显示的列数
        int currentPageColumns = endColumn - startColumn + 1;
        
        // 根据当前页的列数重新计算单元格宽度
        float cellWidth = (chartWidth - (currentPageColumns - 1) * gridSpacing) / currentPageColumns;
        float cellHeight = (chartHeight - (gridRows - 1) * gridSpacing) / gridRows;
        
        return Mathf.Min(cellWidth, cellHeight) * 0.3f;
    }
    
    private void UpdateLabelsVisibility()
    {
        if (!showLabels) return;
        
        // 通过命名查找所有热力图标签
        for (int row = 0; row < gridRows; row++)
        {
            for (int col = startColumn; col <= endColumn; col++)
            {
                var labelName = $"HeatmapLabel_{row}_{col}";
                var labelTransform = transform.Find(labelName);
                
                if (labelTransform != null)
                {
                    var label = labelTransform.GetComponent<TextMeshProUGUI>();
                    if (label != null)
                    {
                        // 首先检查是否有数据
                        bool hasData = HasDataAt(row, col);
                        
                        if (hasData)
                    {
                        // 根据动画进度决定标签可见性
                        if (_v2Base.set.openAnimation && animationProgress < 1f)
                        {
                                float cellProgress = (float)(row * calculatedColumns + col) / (gridRows * calculatedColumns);
                            label.gameObject.SetActive(cellProgress <= animationProgress);
                        }
                        else
                        {
                            label.gameObject.SetActive(true);
                            }
                        }
                        else
                        {
                            // 没有数据，隐藏标签
                            label.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    // v2base悬停事件处理
    private void OnV2BasePointerEnter()
    {
        if (!enableHover) return;
        // 开始定时更新悬停状态
        StartCoroutine(UpdateHoverCoroutine());
    }
    
    private void OnV2BasePointerExit()
    {
        if (!enableHover) return;
        // 停止悬停更新协程
        StopCoroutine(UpdateHoverCoroutine());
        ClearHoverState();
    }
    
    private IEnumerator UpdateHoverCoroutine()
    {
        while (enableHover)
        {
            UpdateHoverState();
            yield return null; // 每帧更新一次
        }
    }
    
    private void UpdateHoverState()
    {
        if (_v2Base == null || gridPositions == null) return;
        
        Vector2 mousePosition = _v2Base.HoverPos;
        Vector2 localPosition = mousePosition - new Vector2(_v2Base.XOffset, 0);
        
        int newHoveredRow = -1;
        int newHoveredColumn = -1;

        for (int row = 0; row < gridRows; row++)
        {
            for (int col = startColumn; col <= endColumn; col++)
            {
                Vector2 cellCenter = gridPositions[row, col];
                float chartWidth = _v2Base.width - _v2Base.set.distanceFromLeft - _v2Base.set.distanceFromRight;
                float chartHeight = _v2Base.height - _v2Base.set.distanceFromTop - _v2Base.set.distanceFromButtom;
                
                // 计算当前页实际显示的列数
                int currentPageColumns = endColumn - startColumn + 1;
                float cellWidth = (chartWidth - (currentPageColumns - 1) * gridSpacing) / currentPageColumns;
                float cellHeight = (chartHeight - (gridRows - 1) * gridSpacing) / gridRows;

                float distance = Vector2.Distance(localPosition, cellCenter);
                float threshold = Mathf.Min(cellWidth, cellHeight) * 0.5f;
                
                if (distance < threshold)
                {
                    newHoveredRow = row;
                    newHoveredColumn = col;
                    break;
                }
            }
            if (newHoveredRow >= 0) break;
        }
        
        // 只有当悬停状态发生变化时才更新
        if (newHoveredRow != hoveredRow || newHoveredColumn != hoveredColumn)
        {
            hoveredRow = newHoveredRow;
            hoveredColumn = newHoveredColumn;
            SetVerticesDirty();
        }
    }
    
    private void ClearHoverState()
    {
        if (hoveredRow >= 0 || hoveredColumn >= 0)
        {
            hoveredRow = -1;
            hoveredColumn = -1;
            SetVerticesDirty();
        }
    }

#if UNITY_EDITOR
    public override void OnCreat()
    {
        base.OnCreat();
        // 设置默认材质
        material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets($"t:material UV_x抗锯齿 清晰")[0]));
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying && _v2Base != null)
        {
            ForceRefresh();
        }
    }
#endif

}

