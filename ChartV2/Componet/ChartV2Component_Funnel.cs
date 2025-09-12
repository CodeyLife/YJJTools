using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using YJJTool;
using Sirenix.OdinInspector;

namespace YJJTool.ChartV2
{
    [ComponentDesc("漏斗图组件")]
    [ComponentOrder(6)]
    public class ChartV2Component_Funnel : ChartV2ComponetBase
    {
        [Title("漏斗图设置")]
        [LabelText("漏斗类型")]
        public FunnelType funnelType = FunnelType.Trapezoid;
        
        [LabelText("漏斗间距")]
        [Range(0f, 50f)]
        public float funnelSpacing = 5f;
        
        [LabelText("最小宽度比例")]
        [Range(0.01f, 0.5f)]
        public float minWidthRatio = 0.1f;
        
        [LabelText("最大宽度比例")]
        [Range(0.1f, 1f)]
        public float maxWidthRatio = 1f;
        
        [Title("标签设置")]
        [LabelText("显示标签")]
        public bool showLabels = true;
        
        [LabelText("标签位置")]
        public LabelPosition labelPosition = LabelPosition.Center;

        [LabelText("标签大小"),Range(0.1f,1)]
        public float labelScale = 0.85f;

        [LabelText("标签偏移")]
        public Vector2 labelOffset = Vector2.zero;
        
        [LabelText("标签颜色")]
        public Color labelColor = Color.white;
        
        [LabelText("显示数值")]
        public bool showValue = true;
        
        [LabelText("显示百分比")]
        public bool showPercentage = true;
        
        [Title("动画设置")]
        [LabelText("启用动画")]
        public bool enableAnimation = true;
        
        [LabelText("动画类型")]
        [ShowIf("enableAnimation")]
        public AnimationType animationType = AnimationType.Sequential;
        
        [Title("悬停效果")]
        [LabelText("悬停颜色")]
        public Color hoverColor = Color.white;
        
        [LabelText("悬停缩放")]
        [Range(1f, 1.5f)]
        public float hoverScale = 1.1f;
        
        public enum FunnelType
        {
            Trapezoid,  // 梯形
            Triangle,   // 三角形
            Rectangle   // 矩形
        }
        
        public enum LabelPosition
        {
            Top,
            Center,
            Bottom,
            Left,
            Right
        }
        
        public enum AnimationType
        {
            Sequential,   // 顺序动画
            Simultaneous, // 同时动画
            Wave         // 波浪动画
        }
        
        private List<FunnelSegment> _funnelSegments = new List<FunnelSegment>();
        private List<FunnelDataItem> _sortedDataItems = new List<FunnelDataItem>();
        private float _totalValue = 0f;
        private float _animationTime = 0f;
        private int _hoveredSegmentIndex = -1;
        
        [System.Serializable]
        public class FunnelSegment
        {
            public string name;
            public float value;
            public float percentage;
            public Color color;
            public Rect rect;
            public int index;
            public float scale;
            public bool visible;
        }
        
        [System.Serializable]
        public class FunnelDataItem
        {
            public string name;
            public float value;
            public Color color;
        }
        
        public override void InitGraph(ChartV2Base chart)
        {
            base.InitGraph(chart);
            _v2Base = chart;
            raycastTarget = true;
            
            // 注册事件
            _v2Base.InitAnimationEvent.AddListener(OnAnimationUpdate);
            _v2Base.OnHoverEvent.AddListener(OnHover);
            _v2Base.OnPointerEnterEvent.AddListener(OnPointerEnter);
            _v2Base.OnPointerExitEvent.AddListener(OnPointerExit);
            
            CalculateFunnelData();
        }
        
        public override void SetGraph()
        {
            CalculateFunnelData();
            SetVerticesDirty();
        }
        
        public override void OnCreat()
        {
            CalculateFunnelData();
        }
        
        protected override void OnDestroy()
        {
            if (_v2Base != null)
            {
                _v2Base.InitAnimationEvent.RemoveListener(OnAnimationUpdate);
                _v2Base.OnHoverEvent.RemoveListener(OnHover);
                _v2Base.OnPointerEnterEvent.RemoveListener(OnPointerEnter);
                _v2Base.OnPointerExitEvent.RemoveListener(OnPointerExit);
            }

        }
        
        private void CalculateFunnelData()
        {
            if (_v2Base == null || _v2Base.datas == null || _v2Base.datas.Count == 0) return;
            
            PrepareDataItems(_v2Base.datas[0].datas);
            SortDataItems();
            CalculateTotalValue();
            CreateFunnelSegments();
            CalculateLayout();
            
            // 创建和更新文本标签
            if (showLabels)
            {
                CreateOrUpdateAllLabels();
            }
        }
        
        private void PrepareDataItems(List<float> dataSeries)
        {
            _sortedDataItems.Clear();
            
            for (int i = 0; i < dataSeries.Count; i++)
            {
                var dataItem = new FunnelDataItem
                {
                    name = _v2Base.names != null && i < _v2Base.names.Count ? _v2Base.names[i] : $"数据{i + 1}",
                    value = dataSeries[i],
                    color = _v2Base.set.colors != null && i < _v2Base.set.colors.Count ? _v2Base.set.colors[i] : Color.HSVToRGB(i * 0.1f, 0.8f, 0.9f)
                };
                
                _sortedDataItems.Add(dataItem);
            }
        }
        
        private void SortDataItems()
        {
            // 漏斗图按照数据值从小到大排序，确保从上到下变窄（最小的在上面，最大的在下面）
            _sortedDataItems = _sortedDataItems.OrderBy(item => item.value).ToList();
        }
        
        private void CalculateTotalValue()
        {
            _totalValue = 0f;
            foreach (var item in _sortedDataItems)
            {
                _totalValue += item.value;
            }
        }
        
        private void CreateFunnelSegments()
        {
            _funnelSegments.Clear();
            
            // 数据已经按从小到大排序，直接创建段
            for (int i = 0; i < _sortedDataItems.Count; i++)
            {
                var dataItem = _sortedDataItems[i];
                var segment = new FunnelSegment
                {
                    name = dataItem.name,
                    value = dataItem.value,
                    percentage = _totalValue > 0 ? (dataItem.value / _totalValue) * 100f : 0f,
                    color = dataItem.color,
                    rect = new Rect(),
                    index = i,
                    scale = 1f,
                    visible = true
                };
                
                _funnelSegments.Add(segment);
            }
        }
        
        private void CalculateLayout()
        {
            if (_funnelSegments.Count == 0) return;
            
            float chartWidth = _v2Base.width;
            float chartHeight = _v2Base.height;
            float segmentHeight = (chartHeight - funnelSpacing * (_funnelSegments.Count - 1)) / _funnelSegments.Count;
            float currentY = 0f;
            
            // 计算最小值和最大值
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;
            foreach (var segment in _funnelSegments)
            {
                if (segment.value < minValue) minValue = segment.value;
                if (segment.value > maxValue) maxValue = segment.value;
            }
            
            
            // 计算每个段的宽度，现在_funnelSegments已经按数据值从小到大排列
            for (int i = 0; i < _funnelSegments.Count; i++)
            {
                var segment = _funnelSegments[i];
                // 使用索引来计算宽度，确保从上到下变窄
                float normalizedIndex = _funnelSegments.Count > 1 ? (float)i / (_funnelSegments.Count - 1) : 0f;
                float topWidth = Mathf.Lerp(minWidthRatio, maxWidthRatio, normalizedIndex) * chartWidth;
                
                // 使用顶部宽度作为矩形宽度，底部宽度在绘制时计算
                segment.rect = new Rect(
                    (chartWidth - topWidth) * 0.5f,
                    currentY,
                    topWidth,
                    segmentHeight
                );
                
                currentY += segmentHeight + funnelSpacing;
            }
        }
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            
            for (int i = 0; i < _funnelSegments.Count; i++)
            {
                var segment = _funnelSegments[i];
                if (!segment.visible) continue;
                
                // 应用动画和悬停缩放
                float scale = segment.scale;
                if (i == _hoveredSegmentIndex)
                {
                    scale *= hoverScale;
                }
                
                Rect scaledRect = new Rect(
                    segment.rect.center.x - segment.rect.width * scale * 0.5f,
                    segment.rect.center.y - segment.rect.height * scale * 0.5f,
                    segment.rect.width * scale,
                    segment.rect.height * scale
                );
                
                // 应用悬停颜色
                Color color = segment.color;
                if (i == _hoveredSegmentIndex)
                {
                    color = Color.Lerp(segment.color, hoverColor, 0.3f);
                }
                
                // 绘制漏斗形状
                DrawFunnelShape(vh, scaledRect, color, i);
            }
        }
        
        private void DrawFunnelShape(VertexHelper vh, Rect rect, Color color, int index)
        {
            switch (funnelType)
            {
                case FunnelType.Trapezoid:
                    DrawTrapezoid(vh, rect, color, index);
                    break;
                case FunnelType.Triangle:
                    DrawTriangle(vh, rect, color, index);
                    break;
                case FunnelType.Rectangle:
                    DrawRectangle(vh, rect, color, index);
                    break;
            }
        }
        
        private void DrawTrapezoid(VertexHelper vh, Rect rect, Color color, int index)
        {
            // 计算梯形参数
            float topWidth = rect.width;
            float bottomWidth;
            
            // 计算底部宽度 - 现在_funnelSegments已经按数据值从小到大排列
            if (index < _funnelSegments.Count - 1)
            {
                // 不是最后一个段，底部宽度是下一个段的顶部宽度
                bottomWidth = _funnelSegments[index + 1].rect.width;
            }
            else
            {
                // 最后一个段（数据值最大的），使用最大宽度
                bottomWidth = maxWidthRatio * _v2Base.width;
            }
            
            // 检查是否应该绘制为三角形（第一个段，即数据值最小的段）
            if (index == 0)
            {
                // 数据值最小的段绘制为三角形
                DrawTriangle(vh, rect, color, index);
            }
            else
            {
                // 计算底部位置（居中）
                float bottomX = rect.center.x - bottomWidth * 0.5f;
                
                Vector2 topLeft = new Vector2(rect.x, rect.y);
                Vector2 topRight = new Vector2(rect.x + topWidth, rect.y);
                Vector2 bottomLeft = new Vector2(bottomX, rect.y + rect.height);
                Vector2 bottomRight = new Vector2(bottomX + bottomWidth, rect.y + rect.height);
                
                DrawSolidTrapezoid(vh, topLeft, topRight, bottomLeft, bottomRight, color);
            }
        }

        private void DrawTriangle(VertexHelper vh, Rect rect, Color color, int index)
        {
            // 对于漏斗图的第一个段，绘制为三角形（顶部尖，底部宽）
            Vector2 topCenter = new Vector2(rect.center.x, rect.y);
            Vector2 bottomLeft = new Vector2(rect.x, rect.y + rect.height);
            Vector2 bottomRight = new Vector2(rect.x + rect.width, rect.y + rect.height);
            
            // 绘制三角形
            vh.AddVert(topCenter, color, Vector2.zero);
            vh.AddVert(bottomLeft, color, Vector2.zero);
            vh.AddVert(bottomRight, color, Vector2.zero);
            
            int baseIndex = vh.currentVertCount - 3;
            vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
        }

        private void DrawRectangle(VertexHelper vh, Rect rect, Color color, int index)
        {
            DrawRect(vh, rect, color);
        }
        
        private void DrawSolidTrapezoid(VertexHelper vh, Vector2 topLeft, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomRight, Color color)
        {
            // 添加四个顶点
            vh.AddVert(topLeft, color, Vector2.zero);
            vh.AddVert(topRight, color, Vector2.zero);
            vh.AddVert(bottomRight, color, Vector2.zero);
            vh.AddVert(bottomLeft, color, Vector2.zero);
            
            // 添加两个三角形
            int baseIndex = vh.currentVertCount - 4;
            vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
        }
        
        private void DrawRect(VertexHelper vh, Rect rect, Color color)
        {
            Vector2 p0 = new Vector2(rect.x, rect.y);
            Vector2 p1 = new Vector2(rect.x + rect.width, rect.y);
            Vector2 p2 = new Vector2(rect.x + rect.width, rect.y + rect.height);
            Vector2 p3 = new Vector2(rect.x, rect.y + rect.height);
            
            Yjj_ChartUtility.DrawQuad(vh, p0, p1, p2, p3, color);
        }
        
        private void OnHover(Vector2 localPos)
        {
            // 将基于图表中心点的位置转换为基于左下角的位置
            Vector2 bottomLeftPos = new Vector2(
                localPos.x + _v2Base.width * 0.5f,
                localPos.y + _v2Base.height * 0.5f
            );
            
            int newHoveredIndex = GetSegmentAtPosition(bottomLeftPos);
            if (newHoveredIndex != _hoveredSegmentIndex)
            {
                _hoveredSegmentIndex = newHoveredIndex;
                SetVerticesDirty();
            }
        }
        
        private void OnPointerEnter()
        {
            // 鼠标进入图表区域
        }
        
        private void OnPointerExit()
        {
            if (_hoveredSegmentIndex != -1)
            {
                _hoveredSegmentIndex = -1;
                SetVerticesDirty();
            }
        }
        
        private int GetSegmentAtPosition(Vector2 localPos)
        {
            for (int i = 0; i < _funnelSegments.Count; i++)
            {
                if (_funnelSegments[i].rect.Contains(localPos))
                {
                    return i;
                }
            }
            return -1;
        }
        
        private void OnAnimationUpdate(float progress)
        {
            if (!enableAnimation) return;
            
            _animationTime = progress;
            
            for (int i = 0; i < _funnelSegments.Count; i++)
            {
                var segment = _funnelSegments[i];
                
                switch (animationType)
                {
                    case AnimationType.Sequential:
                        // 顺序动画
                        float segmentProgress = Mathf.Clamp01((progress * _funnelSegments.Count) - i);
                        segment.scale = segmentProgress;
                        segment.visible = segmentProgress > 0f;
                        break;
                        
                    case AnimationType.Simultaneous:
                        // 同时动画
                        segment.scale = progress;
                        segment.visible = progress > 0f;
                        break;
                        
                    case AnimationType.Wave:
                        // 波浪动画
                        if (_funnelSegments.Count > 1)
                        {
                            float waveOffset = (float)i / (_funnelSegments.Count - 1);
                            float waveProgress = Mathf.Sin((progress * Mathf.PI * 2) - (waveOffset * Mathf.PI)) * 0.5f + 0.5f;
                            
                            // 平滑过渡到结束状态，避免突然跳跃
                            if (progress > 0.8f)
                            {
                                // 在80%-100%之间平滑过渡到完整状态
                                float transitionProgress = (progress - 0.8f) / 0.2f;
                                float finalScale = Mathf.Lerp(waveProgress, 1f, transitionProgress);
                                segment.scale = finalScale;
                                segment.visible = finalScale > 0.1f;
                            }
                            else
                            {
                                segment.scale = waveProgress;
                                segment.visible = waveProgress > 0.1f;
                            }
                        }
                        else
                        {
                            segment.scale = progress;
                            segment.visible = progress > 0f;
                        }
                        break;
                }
            }
            
            // 更新文本标签的可见性
            if (showLabels)
            {
                UpdateLabelsVisibility();
            }
            
            SetVerticesDirty();
        }
        
        private void CreateOrUpdateAllLabels()
        {
            // 更新所有标签
            for (int i = 0; i < _funnelSegments.Count; i++)
            {
                var segment = _funnelSegments[i];
                var text = GetLaybel(i);
                text.text = GetLabelText(segment);
                text.transform.SetAsFirstSibling();
                text.color = labelColor;
                text.rectTransform.anchoredPosition = GetLabelPosition(segment.rect, i);
                text.font = _v2Base.set.font;
                text.fontSize = segment.rect.height * labelScale;
                text.gameObject.SetActive(segment.visible);
            }
            transform.DelateChildByCount(_funnelSegments.Count);
        }

        private TextMeshProUGUI GetLaybel(int index)
        {
            return transform.GetOrCreatUIChild<TextMeshProUGUI>($"Label{index}",CreatNewAction:(t)=>
            {
                t.enableWordWrapping = false;
                t.alignment = TextAlignmentOptions.Center;
            });
        }
        
        private void UpdateLabelsVisibility()
        {
            for (int i = 0; i < _funnelSegments.Count;i++)
            {
                var segment = _funnelSegments[i];
                var text = GetLaybel(i);
                
                if (text != null)
                {
                    text.gameObject.SetActive(segment.visible);
                }
            }
        }
      
        
        private TextMeshProUGUI GetOrCreateLabel(int index)
        {
            var text = transform.GetOrCreatUIChild<TextMeshProUGUI>($"FunnelLabel_{index}", (t) =>
            {
                t.fontSize = 12f;
                t.color = labelColor;
                t.alignment = TextAlignmentOptions.Center;
                t.raycastTarget = false;
                
                // 使用V2BaseSet中的字体设置
                if (_v2Base != null && _v2Base.set != null && _v2Base.set.font != null)
                {
                    t.font = _v2Base.set.font;
                }
                
                var rectTransform = t.rectTransform;
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.zero;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(100, 20);
            });
            
            return text;
        }
        
        private string GetLabelText(FunnelSegment segment)
        {
            var parts = new List<string>();
            
            parts.Add(segment.name);
            
            if (showValue)
            {
                parts.Add(segment.value.ToString("F1"));
            }
            
            if (showPercentage)
            {
                parts.Add($"{segment.percentage:F1}%");
            }
            
            return string.Join(" ", parts);
        }
        
        private Vector2 GetLabelPosition(Rect rect, int segmentIndex)
        {
            Vector2 position = Vector2.zero;
            
            // 将图表坐标系转换为UI坐标系
            // 图表坐标系：左下角为原点，Y轴向上
            // UI坐标系：中心为原点，Y轴向上
            float chartCenterX = _v2Base.width * 0.5f;
            float chartCenterY = _v2Base.height * 0.5f;
            
            switch (labelPosition)
            {
                case LabelPosition.Top:
                    position = new Vector2(rect.center.x - chartCenterX, rect.y + rect.height - chartCenterY);
                    break;
                case LabelPosition.Center:
                    position = new Vector2(rect.center.x - chartCenterX, rect.center.y - chartCenterY);
                    break;
                case LabelPosition.Bottom:
                    position = new Vector2(rect.center.x - chartCenterX, rect.y - chartCenterY);
                    break;
                case LabelPosition.Left:
                    position = new Vector2(rect.x - chartCenterX, rect.center.y - chartCenterY);
                    break;
                case LabelPosition.Right:
                    position = new Vector2(rect.x + rect.width - chartCenterX, rect.center.y - chartCenterY);
                    break;
            }
            
            return position + labelOffset;
        }
        

    }
}
