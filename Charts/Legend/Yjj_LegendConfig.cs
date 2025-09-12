using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using TMPro;

namespace YJJTool
{
    /// <summary>
    /// 图例配置ScriptableObject
    /// 用于配置图例的样式、布局和显示选项，并提供图例生成API
    /// </summary>
    [CreateAssetMenu(fileName = "LegendConfig", menuName = "YJJ Tools/Charts/Legend Config")]
    public class Yjj_LegendConfig : ScriptableObject
    {
        #region 参数
        [Title("图例基本设置", TitleAlignment = TitleAlignments.Centered)]
        [LabelText("图例标题")]
        public string legendTitle = "图例";
        
        [LabelText("显示图例标题")]
        public bool showTitle = true;

        public enum LegendDirection
        {
            [LabelText("垂直")]
            Vertical,
            [LabelText("水平")]
            Horizontal
        }
        
        [Title("图例项设置", TitleAlignment = TitleAlignments.Centered)]
        [LabelText("颜色块大小")]
        public Vector2 colorBlockSize = new Vector2(20, 20);
        
        [LabelText("颜色块与文本间距")]
        public float colorTextSpacing = 8f;
        
        [LabelText("文本字体大小")]
        public float fontSize = 14f;
        
        [LabelText("文本颜色")]
        public Color textColor = Color.black;
        
        [LabelText("文本字体")]
        public TMP_FontAsset font;
        
        [Title("布局设置", TitleAlignment = TitleAlignments.Centered)]

        [LabelText("图例方向")]
        public LegendDirection direction = LegendDirection.Vertical;

        [LabelText("每行最大项目数")]
        public int maxItemsPerRow = 3;
        
        [LabelText("图例项大小")]
        public Vector2 itemSize = new Vector2(150, 30);
        
        [LabelText("图例间距")]
        public Vector2 spacing = new Vector2(10, 10);
        
        [LabelText("图例边距")]
        public Vector4 margin = new Vector4(10, 10, 10, 10);
        
        [LabelText("图例背景")]
        public bool showBackground = false;
        
        [ShowIf("showBackground"), LabelText("背景颜色")]
        public Color backgroundColor = new Color(1, 1, 1, 0.8f);

        #endregion

        #region 图例生成API

        /// <summary>
        /// 生成图例
        /// </summary>
        /// <param name="container">图例容器RectTransform</param>
        /// <param name="colors">颜色列表</param>
        /// <param name="labels">标签列表</param>
        /// <param name="values">数值列表（可选）</param>
        /// <returns>生成的图例项列表</returns>
        public List<GameObject> GenerateLegend(RectTransform container, List<Color> colors, List<string> labels, List<float> values = null)
        {
            if (container == null)
            {
                Debug.LogError("图例容器为空！");
                return new List<GameObject>();
            }
            
            if (colors == null || labels == null)
            {
                Debug.LogError("颜色列表和标签列表数量不能为空！");
                return new List<GameObject>();
            }
            
            // 创建图例项容器
            RectTransform itemsContainer = CreateItemsContainer(container);
            
            // 设置图例项容器布局
            var size = SetupItemsContainerLayout(itemsContainer, colors.Count);
            
            List<GameObject> legendItems = new List<GameObject>();
            
            // 生成图例项
            for (int i = 0; i < labels.Count; i++)
            {
                GameObject item = CreateLegendItem(itemsContainer, i, colors.Count>i? colors[i]:Color.white, labels[i], values != null && i < values.Count ? values[i] :null);
                legendItems.Add(item);
            }
            itemsContainer.DelateChildByCount(legendItems.Count);
            
            // 执行最终生成逻辑
            return ExecuteFinalGeneration(container, itemsContainer, legendItems,size);
        }
        
        /// <summary>
        /// 执行最终生成逻辑
        /// </summary>
        /// <param name="container">主容器</param>
        /// <param name="itemsContainer">图例项容器</param>
        /// <param name="legendItems">图例项列表</param>
        /// <returns>生成的图例项列表</returns>
        private List<GameObject> ExecuteFinalGeneration(RectTransform container, RectTransform itemsContainer, List<GameObject> legendItems, Vector2 size)
        {
            // 计算图例项容器的实际大小
            float itemsHeight = size.y;
            float itemsWidth = size.x;
            
            // 计算标题高度
            float titleHeight = 0f;
            if (showTitle && !string.IsNullOrEmpty(legendTitle))
            {
                titleHeight = fontSize + spacing.y; // 字体大小 + 额外间距
            }
            
            // 计算总高度
            float totalHeight = itemsHeight + titleHeight + margin.y + margin.w;
            float totalWidth = itemsWidth + margin.x + margin.z;
            
            // 更新主容器大小
            RectTransform containerRect = container;
            containerRect.sizeDelta = new Vector2(totalWidth, totalHeight);

            // 重新调整图例项容器位置，为标题留出空间
            itemsContainer.offsetMin = new Vector2(margin.x, margin.y);
            itemsContainer.offsetMax = new Vector2(-margin.z, -margin.w - titleHeight);

            // 生成图例背景
            if (showBackground)
            {
                CreateLegendBackground(container);
            }
            
            // 生成图例标题
            if (showTitle && !string.IsNullOrEmpty(legendTitle))
            {
                CreateLegendTitle(container, titleHeight);
            }
            
            return legendItems;
        }
        
        /// <summary>
        /// 生成图例（简化版本）
        /// </summary>
        /// <param name="container">图例容器RectTransform</param>
        /// <param name="colorLabelPairs">颜色和标签的配对列表</param>
        /// <returns>生成的图例项列表</returns>
        public List<GameObject> GenerateLegend(RectTransform container, List<(Color color, string label)> colorLabelPairs)
        {
            List<Color> colors = new List<Color>();
            List<string> labels = new List<string>();
            
            foreach (var pair in colorLabelPairs)
            {
                colors.Add(pair.color);
                labels.Add(pair.label);
            }
            
            return GenerateLegend(container, colors, labels);
        }
        
        /// <summary>
        /// 更新图例项
        /// </summary>
        /// <param name="container">图例容器RectTransform</param>
        /// <param name="index">索引</param>
        /// <param name="color">新颜色</param>
        /// <param name="label">新标签</param>
        /// <param name="value">新数值</param>
        public void UpdateLegendItem(RectTransform container, int index, Color color, string label, float value = 0f)
        {
            if (container == null || index < 0) return;
            
            Transform itemTransform = container.GetChild(index);
            if (itemTransform == null) return;
            
            GameObject item = itemTransform.gameObject;
            
            // 更新颜色块
            Image colorBlock = item.transform.Find("ColorBlock")?.GetComponent<Image>();
            if (colorBlock != null)
            {
                colorBlock.color = color;
            }
            
            // 更新文本
            TextMeshProUGUI text = item.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = label;
            }
            
            // 更新数值（如果有）
            TextMeshProUGUI valueText = item.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
            if (valueText != null && value > 0f)
            {
                valueText.text = value.ToString("F1");
            }
        }
        
        
        /// <summary>
        /// 设置图例可见性
        /// </summary>
        /// <param name="container">图例容器RectTransform</param>
        /// <param name="visible">是否可见</param>
        public void SetLegendVisible(RectTransform container, bool visible)
        {
            if (container != null)
            {
                container.gameObject.SetActive(visible);
            }
        }
        
        #endregion
        
        #region 私有辅助方法
        
        
        private RectTransform CreateItemsContainer(RectTransform parent)
        {
            var itemsContainer = parent.GetOrCreatUIChild<RectTransform>("ItemsContainer", (rect) =>
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
            });
            return itemsContainer;
        }
        
        private Vector2 SetupItemsContainerLayout(RectTransform container, int totalItems)
        {
            // 添加GridLayoutGroup
            GridLayoutGroup gridLayout = container.GetOrAddComponent<GridLayoutGroup>();
            
            // 配置GridLayoutGroup
            gridLayout.cellSize = itemSize;
            gridLayout.spacing = spacing;
            gridLayout.padding = new RectOffset(0, 0, 0, 0); // 图例项容器不需要padding，由父容器处理

            // 根据方向设置约束 - 都使用maxItemsPerRow控制最大值
            int rowCount = 0, columnCount = 0;
            if (direction == LegendDirection.Horizontal)
            {
                // 水平方向：固定列数，自动换行
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                gridLayout.constraintCount = Mathf.Min(maxItemsPerRow, totalItems);

                columnCount = gridLayout.constraintCount;
                rowCount = Mathf.CeilToInt((float)totalItems/gridLayout.constraintCount);
            }
            else
            {
                // 垂直方向：固定行数，自动换列
                gridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                gridLayout.constraintCount = Mathf.Min(maxItemsPerRow, totalItems);
               
                 columnCount = Mathf.CeilToInt((float)totalItems / gridLayout.constraintCount);
                rowCount = gridLayout.constraintCount;
            }
            
            // 设置子项对齐方式
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            return GetSize(rowCount, columnCount);
   

            Vector2 GetSize(int row,int column)
            {
                var width = column * itemSize.x + (column - 1) * spacing.x;
                var height = row * itemSize.y + (row - 1) * spacing.y;
                //Debug.Log($"{row}行 {column}列   大小：{width}x{height}");
                return new Vector2(width, height);
            }
        }
        
        
        private void CreateLegendBackground(RectTransform container)
        {
            var backgroundImage = container.GetOrCreatUIChild<Image>("LegendBackground");
            backgroundImage.color = backgroundColor;

            // 手动设置背景布局 - 填满整个容器
            RectTransform backgroundRect = backgroundImage.rectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            
            // 确保背景在最底层
            backgroundImage.transform.SetAsFirstSibling();
        }
        
        private void CreateLegendTitle(RectTransform container, float titleHeight)
        {
            var titleText = container.GetOrCreatUIChild<TextMeshProUGUI>("LegendTitle", (text) =>
            {
                text.enableWordWrapping = false;
            });
           titleText.text = legendTitle;
           titleText.fontSize = fontSize + 2; // 标题比普通文本大一点
           titleText.color = textColor;
           titleText.font = font;
           titleText.alignment = TextAlignmentOptions.Top;
           titleText.fontStyle = FontStyles.Bold;
        
             // 手动设置标题布局 - 位于容器顶部
            RectTransform titleRect = titleText.rectTransform;
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);

            // 设置标题位置和大小 - 位于容器顶部
            //titleRect.offsetMin = new Vector2(margin.x, margin.y);
            //titleRect.offsetMax = new Vector2(-margin.z, container.rect.height - margin.y);
            titleRect.anchoredPosition = new Vector2(0, -margin.w);
        }
        
        private GameObject CreateLegendItem(RectTransform container, int index, Color color, string label, float? value)
        {
            var itemGO = container.GetOrCreatUIChild<RectTransform>($"LegendItem_{index}", (rect) =>
            {
                // GridLayoutGroup会自动设置大小，这里不需要手动设置
            }).gameObject;
            itemGO.transform.SetAsFirstSibling();
            // 直接创建颜色块Image
            var colorBlock = itemGO.transform.GetOrCreatUIChild<Image>("ColorBlock");
            colorBlock.color = color;
            // 设置颜色块布局
            RectTransform colorBlockRect = colorBlock.rectTransform;
            colorBlockRect.anchorMin = new Vector2(0, 0.5f);
            colorBlockRect.anchorMax = new Vector2(0, 0.5f);
            colorBlockRect.sizeDelta = colorBlockSize;
            colorBlockRect.anchoredPosition = new Vector2(colorBlockSize.x * 0.5f, 0);
            
            // 直接创建文本
            var text = itemGO.transform.GetOrCreatUIChild<TextMeshProUGUI>("Text", (txt) =>
            {
                txt.enableWordWrapping = false;
            });
            text.text = label;
            text.fontSize = fontSize;
            text.color = textColor;
            text.font = font;
            text.alignment = TextAlignmentOptions.MidlineLeft;

            // 设置文本布局
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(colorBlockSize.x + colorTextSpacing, 0);
            textRect.offsetMax = new Vector2(0, 0);

            // 创建数值文本（如果有数值）
            if (value.HasValue)
            {
                var valueText = itemGO.transform.GetOrCreatUIChild<TextMeshProUGUI>("ValueText", (txt) =>
                {
                    txt.enableWordWrapping = false;
                });

                valueText.text = value.Value.ToAutoLimitString(1);
                valueText.fontSize = fontSize * 0.9f;
                valueText.color = textColor;
                valueText.font = font;
                valueText.alignment = TextAlignmentOptions.MidlineRight;
                // 设置数值文本布局
                RectTransform valueTextRect = valueText.rectTransform;
                valueTextRect.anchorMin = new Vector2(1, 0);
                valueTextRect.anchorMax = new Vector2(1, 1);
                valueTextRect.sizeDelta = new Vector2(50, 30);
                valueTextRect.anchoredPosition = new Vector2(-25, 0);
            }
            else
            {
                var textGo = itemGO.transform.Find("ValueText");
                textGo?.gameObject.DestroyByRuntimeType();
            }
            return itemGO;
        }
        

        
        
        #endregion
    }
    
    
}
