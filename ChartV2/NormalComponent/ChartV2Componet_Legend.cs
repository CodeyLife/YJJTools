using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ComponentDesc("图例组件")]
[ComponentOrder(1000)]
public class ChartV2Componet_Legend : ChartV2ComponetBaseWithoutGraphic
{
    [Title("图例设置")]
    public enum LegendAlignment
    {
        [LabelText("左上")]
        UpperLeft,
        [LabelText("上中")]
        UpperCenter,
        [LabelText("右上")]
        UpperRight
    }
    
    [LabelText("对齐方式")]
    public LegendAlignment alignment = LegendAlignment.UpperCenter;
    
    [LabelText("偏移量")]
    public Vector2 offset = new Vector2(0, 10);
    
    [LabelText("文本颜色")]
    public Color textColor = Color.white;
    
    [LabelText("字体大小")]
    public float fontSize = 14;
    
    [HorizontalGroup("hor")]
    [LabelText("图例宽度")]
    public int legendWidth = 30;
    
    [HorizontalGroup("hor")]
    [LabelText("图例高度")]
    public int legendHeight = 10;
    
    [Title("布局设置")]
    [LabelText("水平间距")]
    public float horizontalSpacing = 5f;

    [LabelText("垂直间距")]
    public float verticalSpacing = 5f;
    
    [LabelText("每行最大列数")]
    public int maxColumns = 3;
    
    [Title("图例控制")]
    [LabelText("自定义系列名称")]
    public List<string> customSeriesNames = new List<string>();
    
    [LabelText("是否静态图例")]
    [PropertyTooltip("启用后，图例在SetGraph时不会重新生成，需要手动调用生成")]
    public bool isStaticLegend = false;
    
    [Button("生成图例")]
    [ShowIf("isStaticLegend")]
    private void GenerateLegendManually()
    {
        GenerateLegend();
    }
    
    /// <summary>
    /// 手动生成图例（公共方法）
    /// </summary>
    public void GenerateLegend()
    {
        if (_v2Base != null)
        {
            GenerateLegendItems();
        }
    }

    public override void InitGraph(ChartV2Base chart)
    {
        base.InitGraph(chart);
    }
    
    public override void SetGraph()
    {
        base.SetGraph();
        
        if (_v2Base == null || _v2Base.datas == null || _v2Base.datas.Count == 0)
        {
            Debug.LogWarning("ChartV2Base 或数据系列为空，无法生成图例");
            return;
        }

        // 设置图例容器位置
        SetupLegendContainer();
        
        // 如果不是静态图例，则自动生成图例项
        if (!isStaticLegend || (Application.isEditor && !Application.isPlaying))
        {
            GenerateLegendItems();
        }
    }

    private void SetupLegendContainer()
    {
        // 根据对齐方式设置图例容器位置
        var rect = transform.rectTransform();
        
        switch (alignment)
        {
            case LegendAlignment.UpperLeft:
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 0);
                break;
            case LegendAlignment.UpperCenter:
                rect.anchorMin = new Vector2(0.5f, 1);
                rect.anchorMax = new Vector2(0.5f, 1);
                rect.pivot = new Vector2(0.5f, 0);
                break;
            case LegendAlignment.UpperRight:
                rect.anchorMin = new Vector2(1, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 0);
                break;
        }
        
        rect.anchoredPosition = offset;
        rect.sizeDelta = Vector2.zero; // 使用锚点定义大小，sizeDelta应该为0
    }

    private void GenerateLegendItems()
    {
        // 优先使用自定义系列名称的数量，否则使用ChartV2Base的数据系列数量
        int seriesCount = 0;
        
        if (customSeriesNames != null && customSeriesNames.Count > 0)
        {
            seriesCount = customSeriesNames.Count;
        }
        else if (_v2Base != null && _v2Base.datas != null)
        {
            seriesCount = _v2Base.datas.Count;
        }
        
        if (seriesCount == 0)
        {
            Debug.LogWarning("没有数据系列或自定义系列名称，无法生成图例");
            return;
        }
        
        // 计算布局参数
        float itemWidth = legendWidth + 100; // 颜色块 + 文本宽度
        float itemHeight = legendHeight; // 图例项高度，不包含间距
        int itemsPerRow = Mathf.Min(maxColumns, seriesCount);
        int totalRows = Mathf.CeilToInt((float)seriesCount / itemsPerRow);
        
        // 根据alignment计算容器内的布局起始位置
        float startX = CalculateContainerStartX(seriesCount, itemsPerRow, itemWidth);
        
        for (int i = 0; i < seriesCount; i++)
        {
            var item = transform.GetOrCreatUIChild<RectTransform>(i.ToString());
            
            // 计算行列位置
            int row = i / itemsPerRow;
            int col = i % itemsPerRow;
            
            // 图例项始终使用左上角对齐
            item.anchorMin = new Vector2(0, 1);
            item.anchorMax = new Vector2(0, 1);
            item.pivot = new Vector2(0, 1);
            
            // 根据alignment计算x位置
            float xPosition = CalculateItemXPosition(col, startX, itemWidth);
            float yPosition = row * (itemHeight + verticalSpacing); // 往上布局，Y值累加，包含垂直间距
            
            item.anchoredPosition = new Vector2(xPosition, yPosition);
            item.sizeDelta = new Vector2(itemWidth, itemHeight);

            // 创建颜色指示器
            var image = item.GetOrCreatUIChild<Image>("img", (img) =>
            {
                var ir = img.rectTransform;
                ir.anchorMin = new Vector2(0, 0.5f);
                ir.anchorMax = ir.anchorMin;
                ir.pivot = new Vector2(0, 0.5f);
                ir.anchoredPosition = Vector2.zero;
            });
            
            image.rectTransform.sizeDelta = new Vector2(legendWidth, legendHeight);
            
            // 使用 ChartV2Base 的颜色配置
            if (_v2Base != null && _v2Base.set != null && _v2Base.set.colors != null && i < _v2Base.set.colors.Count)
            {
                image.color = _v2Base.set.colors[i];
            }
            else
            {
                // 如果没有配置颜色，使用默认颜色
                image.color = Color.HSVToRGB((float)i / seriesCount, 0.8f, 0.9f);
            }

            // 创建文本标签
            var text = item.transform.GetOrCreatUIChild<TextMeshProUGUI>("Title", (t) =>
            {
                t.rectTransform.anchorMin = new Vector2(0, 0.5f);
                t.rectTransform.anchorMax = new Vector2(1, 0.5f);
                t.rectTransform.pivot = new Vector2(0, 0.5f);
                t.rectTransform.anchoredPosition = new Vector2(legendWidth + 10, 0);
                t.rectTransform.sizeDelta = new Vector2(-legendWidth - 10, legendHeight);
            });
            
            // 使用 ChartV2Base 的字体配置
            if (_v2Base.set != null && _v2Base.set.font != null)
            {
                text.font = _v2Base.set.font;
            }
            
            text.color = textColor;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            
            // 生成系列名称
            string seriesName = GenerateSeriesName(i);
            text.text = seriesName;
            text.fontSize = fontSize;
        }

        // 清理多余的子对象
        CleanupExcessItems(seriesCount);
    }
    
    /// <summary>
    /// 计算容器内的布局起始位置
    /// </summary>
    /// <param name="totalItems">总项目数</param>
    /// <param name="itemsPerRow">每行项目数</param>
    /// <param name="itemWidth">项目宽度</param>
    /// <returns>起始X位置</returns>
    private float CalculateContainerStartX(int totalItems, int itemsPerRow, float itemWidth)
    {
        // 计算第一行的实际项目数（最后一行可能不满）
        int firstRowItems = Mathf.Min(totalItems, itemsPerRow);
        
        // 计算第一行的总宽度（包括水平间距）
        float totalWidth = firstRowItems * itemWidth + (firstRowItems - 1) * horizontalSpacing;
        
        switch (alignment)
        {
            case LegendAlignment.UpperLeft:
                return 0; // 从容器左边开始
            case LegendAlignment.UpperCenter:
                return -totalWidth * 0.5f; // 在容器内居中
            case LegendAlignment.UpperRight:
                return -totalWidth; // 从容器右边开始
            default:
                return -totalWidth * 0.5f; // 默认居中
        }
    }
    
    /// <summary>
    /// 计算单个图例项的X位置
    /// </summary>
    /// <param name="col">列索引</param>
    /// <param name="startX">起始X位置</param>
    /// <param name="itemWidth">项目宽度</param>
    /// <returns>X位置</returns>
    private float CalculateItemXPosition(int col, float startX, float itemWidth)
    {
        return startX + col * (itemWidth + horizontalSpacing);
    }
    
    /// <summary>
    /// 生成数据系列名称
    /// </summary>
    /// <param name="seriesIndex">系列索引</param>
    /// <returns>系列名称</returns>
    private string GenerateSeriesName(int seriesIndex)
    {
        // 优先使用组件自定义的系列名称
        if (customSeriesNames != null && seriesIndex < customSeriesNames.Count && !string.IsNullOrEmpty(customSeriesNames[seriesIndex]))
        {
            return customSeriesNames[seriesIndex];
        }
        
        // 其次使用ChartV2Base配置的系列名称
        if (_v2Base.set != null && _v2Base.set.seriesNames != null && seriesIndex < _v2Base.set.seriesNames.Count)
        {
            return _v2Base.set.seriesNames[seriesIndex];
        }
        
        // 最后使用默认命名规则
        return $"系列 {seriesIndex + 1}";
    }

    private void CleanupExcessItems(int expectedCount)
    {
        if (Application.isPlaying)
        {
            for (int i = expectedCount; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        else
        {
            while (transform.childCount > expectedCount)
            {
                DestroyImmediate(transform.GetChild(expectedCount).gameObject);
            }
        }
    }
}
