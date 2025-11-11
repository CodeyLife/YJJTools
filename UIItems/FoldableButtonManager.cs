using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.Events;

namespace YJJTools.UI
{
    /// <summary>
    /// 多级折叠按钮管理器
    /// </summary>
    [AddComponentMenu("_YjjTool/FoldableButtonManager")]
    public class FoldableButtonManager : MonoBehaviour
    {
        [Header("UI设置")]


        [LabelText("滚动视图")]
        public ScrollRect scrollRect;
        
        [Header("配置")]
        [LabelText("配置资源"),InlineEditor,InlineButton("CreateNew")]
        public FoldableButtonConfig config;
        
        private void CreateNew()
        {
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<FoldableButtonConfig>();
                config.name = "New FoldableButtonConfig";
                
                #if UNITY_EDITOR
                string path = UnityEditor.EditorUtility.SaveFilePanelInProject(
                    "保存配置资源", 
                    "NewFoldableButtonConfig", 
                    "asset", 
                    "选择保存位置");
                
                if (!string.IsNullOrEmpty(path))
                {
                    UnityEditor.AssetDatabase.CreateAsset(config, path);
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                    UnityEditor.EditorUtility.SetDirty(this);
                }
                #endif
            }
            else
            {
                Debug.LogWarning("配置资源已存在！");
            }
        }
        
        
        [Header("数据")]
        [LabelText("根数据列表")]
        public List<FoldableButtonData> rootData = new List<FoldableButtonData>();
        
        [Header("事件")]
        [FoldoutGroup("事件")]
        public UnityEvent<FoldableButtonData> OnItemSelected = new UnityEvent<FoldableButtonData>();
        
        [FoldoutGroup("事件")]
        public UnityEvent<FoldableButtonData> OnItemExpanded = new UnityEvent<FoldableButtonData>();
        
        [FoldoutGroup("事件")]
        public UnityEvent<FoldableButtonData> OnItemCollapsed = new UnityEvent<FoldableButtonData>();
        
        [FoldoutGroup("事件")]
        public UnityEvent<FoldableButtonData> OnItemClicked = new UnityEvent<FoldableButtonData>();
        
        [FoldoutGroup("事件")]
        public UnityEvent OnDataChanged = new UnityEvent();

        // 私有字段
        private RectTransform contentRoot;
        private Dictionary<FoldableButtonData, FoldableButtonItem> buttonItems = new Dictionary<FoldableButtonData, FoldableButtonItem>();
        private HashSet<FoldableButtonData> allData = new HashSet<FoldableButtonData>();
        private Dictionary<FoldableButtonData, ButtonGroup> buttonGroups = new Dictionary<FoldableButtonData, ButtonGroup>();
        private FoldableButtonData selectedItem;
        private bool isInitialized = false;
        
        // 协程管理
        private Coroutine scaleAdjustCoroutine;
        private Coroutine scaleRestoreCoroutine;
        
        // 性能优化
        private List<FoldableButtonItem> cachedOrderedButtons;
        private bool isOrderDirty = true;
        private Dictionary<FoldableButtonData, int> buttonIndexCache = new Dictionary<FoldableButtonData, int>();
        
        private void Awake()
        {
            InitializeLayout();
        }
        
        private void Start()
        {
            if (!isInitialized)
            {
                Initialize();
            }
        }
        
        private void OnDestroy()
        {
            CleanupResources();
        }
        
        private void OnDisable()
        {
            StopAllCoroutines();
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        private void CleanupResources()
        {
            // 停止所有协程
            StopAllCoroutines();
            scaleAdjustCoroutine = null;
            scaleRestoreCoroutine = null;
            
            // 清理事件监听器
            foreach (var buttonItem in buttonItems.Values)
            {
                if (buttonItem != null)
                {
                    buttonItem.OnItemClick.RemoveListener(OnItemClickedInternal);
                    buttonItem.OnItemExpand.RemoveListener(OnItemExpandedWrapper);
                    buttonItem.OnItemCollapse.RemoveListener(OnItemCollapsedWrapper);
                    buttonItem.OnItemSelect.RemoveListener(OnItemSelectedInternal);
                }
            }
            
            // 清理字典
            buttonItems.Clear();
            allData.Clear();
            buttonGroups.Clear();
            buttonIndexCache.Clear();
            
            // 清理UI
            ClearAllButtons();
            
            selectedItem = null;
            isInitialized = false;
        }
        
        /// <summary>
        /// 初始化布局
        /// </summary>
        public void InitializeLayout()
        {
            if (contentRoot == null)
            {
                if (scrollRect != null && scrollRect.content != null)
                {
                    contentRoot = scrollRect.content;
                }
                else
                {
                    Debug.LogError("ScrollRect 或 content 未设置！", this);
                    return;
                }
            }
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            if (config == null)
            {
                Debug.LogError("配置资源未设置！", this);
                return;
            }
            
            if (config.buttonPrefab == null)
            {
                Debug.LogError("按钮预制体未设置！", this);
                return;
            }
            
            if (scrollRect == null)
            {
                
                Debug.LogError("内容根节点未设置！", this);
                return;
            }
            contentRoot = scrollRect.content;
            
            ClearAllButtons();
            BuildButtonHierarchy();
            
            // 更新所有按钮位置并计算正确的Content高度
            UpdateAllButtonPositions();
            
            isInitialized = true;
        }
        
        /// <summary>
        /// 构建按钮层次结构
        /// </summary>
        private void BuildButtonHierarchy()
        {
            allData.Clear();
            buttonItems.Clear();
            buttonGroups.Clear();
            
            // 构建所有数据的字典
            foreach (var rootItem in rootData)
            {
                BuildDataDictionary(rootItem);
            }
            
            // 只创建根级按钮
            foreach (var rootItem in rootData)
            {
                CreateButtonItem(rootItem, 0);
            }
            
            OnDataChanged?.Invoke();
        }
        
        /// <summary>
        /// 构建数据字典
        /// </summary>
        private void BuildDataDictionary(FoldableButtonData data)
        {
            allData.Add(data);
            foreach (var child in data.children)
            {
                BuildDataDictionary(child);
            }
        }
        
        /// <summary>
        /// 创建按钮项
        /// </summary>
        private FoldableButtonItem CreateButtonItem(FoldableButtonData data, int level)
        {
            GameObject buttonObj = Instantiate(config.buttonPrefab, contentRoot);
            buttonObj.name = $"Button_{data.displayName}";
            
            FoldableButtonItem buttonItem = buttonObj.GetOrAddComponent<FoldableButtonItem>();
      
            buttonItem.SetData(data);
            buttonItems[data] = buttonItem;
            
            
            // 设置按钮大小和位置
            RectTransform rectTransform = buttonObj.GetOrAddComponent<RectTransform>();
            SetupButtonRectTransform(rectTransform);
            
            // 标记顺序需要重新计算
            isOrderDirty = true;
            buttonIndexCache.Clear(); // 清空索引缓存
            
            // 更新所有按钮位置（确保新按钮位置正确）
            UpdateAllButtonPositions();
            
            // 设置 ButtonGroup（用于同级互斥）
            SetupButtonGroup(buttonItem, data);

            // 绑定事件
            buttonItem.OnItemExpand.AddListener(OnItemExpandedWrapper);
            buttonItem.OnItemCollapse.AddListener(OnItemCollapsedWrapper);
            buttonItem.OnItemSelect.AddListener(OnItemSelectedInternal);

            if (!data.HasChildren)
            {
                buttonItem.OnItemClick.AddListener(OnItemClickedInternal);
                buttonItem.buttonGroupContent.eventBeforShow = false;
                buttonItem.buttonGroupContent.clickEvent.AddListener(()=>buttonItem.buttonGroupContent.ButtonGroup.ClearWithoutEvent());
            }

            return buttonItem;
        }
        
        /// <summary>
        /// 设置按钮的ButtonGroup
        /// </summary>
        private void SetupButtonGroup(FoldableButtonItem buttonItem, FoldableButtonData data)
        {
            ButtonGroup buttonGroup = null;
            
            // 首先，这个按钮的buttongroup应该是他父级按钮上挂载的buttongroup
            FoldableButtonData parent = data.parent;
            if (parent != null && buttonGroups.TryGetValue(parent, out buttonGroup))
            {
                // 使用父级的ButtonGroup
            }
            else
            {
                // 如果没有父级ButtonGroup，使用根级的ButtonGroup
                buttonGroup = contentRoot.GetComponent<ButtonGroup>();
                if (buttonGroup == null)
                {
                    buttonGroup = contentRoot.gameObject.AddComponent<ButtonGroup>();
                    buttonGroup.supportCancel = true; // 支持点击同一个按钮取消
                }
            }
            
            // 然后，如果这个按钮有子级按钮，他身上也add一个buttongroup
            if (data.HasChildren)
            {
                // 为有子级的按钮创建独立的ButtonGroup
                ButtonGroup childButtonGroup = buttonItem.gameObject.AddComponent<ButtonGroup>();
                childButtonGroup.supportCancel = true; // 支持点击同一个按钮取消
                buttonGroups[data] = childButtonGroup;
            }
           
            // 设置ButtonGroupContent的group引用
            if (buttonItem.buttonGroupContent != null)
            {
                buttonItem.buttonGroupContent.ButtonGroup = buttonGroup;
            }
        }
        
        /// <summary>
        /// 查找父级数据
        /// </summary>
        private FoldableButtonData FindParentData(FoldableButtonData childData)
        {
            return childData.parent;
        }
        
        /// <summary>
        /// 清除所有按钮
        /// </summary>
        private void ClearAllButtons()
        {
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(contentRoot.GetChild(i).gameObject);
            }
        }
        
        /// <summary>
        /// 处理同级互斥
        /// </summary>
        public void HandleSameLevelExclusion(FoldableButtonData data)
        {

            CollapseSameLevelSiblings(data);
        }
        
        /// <summary>
        /// 项目展开时调用
        /// </summary>
        public void OnItemExpandedInternal(FoldableButtonItem item)
        {
            if (item.data == null) return;
            
            // 显示子项
            ShowChildren(item.data);
            
            // 标记顺序需要重新计算
            isOrderDirty = true;
            buttonIndexCache.Clear(); // 清空索引缓存
            
            // 更新所有按钮位置
            UpdateAllButtonPositions();
            
            // 调整缩放
            if (config.adjustOtherButtonHeight)
            {
                // 停止之前的协程
                if (scaleAdjustCoroutine != null)
                {
                    StopCoroutine(scaleAdjustCoroutine);
                }
                scaleAdjustCoroutine = StartCoroutine(AdjustButtonScales(item));
            }
            
            OnItemExpanded?.Invoke(item.data);
        }
        
        /// <summary>
        /// 项目收起时调用
        /// </summary>
        public void OnItemCollapsedInternal(FoldableButtonItem item)
        {
            if (item.data == null) return;
            
            // 隐藏子项
            HideChildren(item.data);
            
            // 标记顺序需要重新计算
            isOrderDirty = true;
            buttonIndexCache.Clear(); // 清空索引缓存
            
            // 更新所有按钮位置
            UpdateAllButtonPositions();
            
            // 恢复缩放
            if (config.adjustOtherButtonHeight)
            {
                // 停止之前的协程
                if (scaleRestoreCoroutine != null)
                {
                    StopCoroutine(scaleRestoreCoroutine);
                }
                scaleRestoreCoroutine = StartCoroutine(RestoreButtonScales());
            }
            
            OnItemCollapsed?.Invoke(item.data);
        }
        
        /// <summary>
        /// 收起同级兄弟项
        /// </summary>
        private void CollapseSameLevelSiblings(FoldableButtonData data)
        {
            if (data.level == 0)
            {
                // 根级项目：收起其他根级项目
                foreach (var rootItem in rootData)
                {
                    if (rootItem != data && rootItem.isExpanded)
                    {
                        if (buttonItems.TryGetValue(rootItem, out var buttonItem))
                        {
                            buttonItem.Collapse();
                        }
                    }
                }
            }
            else
            {
                // 非根级项目：找到父项并收起同级兄弟项
                FoldableButtonData parent = data.parent;
                if (parent == null) return;
                
                foreach (var sibling in parent.children)
                {
                    if (sibling != data && sibling.isExpanded)
                    {
                        if (buttonItems.TryGetValue(sibling, out var buttonItem))
                        {
                            buttonItem.Collapse();
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 显示子项
        /// </summary>
        private void ShowChildren(FoldableButtonData data)
        {
            // 找到当前按钮在层次结构中的位置
            int currentIndex = GetButtonIndex(data);
            int insertIndex = currentIndex + 1;
            
            foreach (var child in data.children)
            {
                FoldableButtonItem buttonItem;
                
                // 如果按钮不存在，创建它
                if (!buttonItems.TryGetValue(child, out buttonItem))
                {
                    buttonItem = CreateButtonItem(child, child.level);
                    // 将子级按钮插入到正确位置
                    buttonItem.transform.SetSiblingIndex(insertIndex);
                    insertIndex++;
                }
                
                // 显示按钮
                buttonItem.SetVisible(true);
                
                // 注意：子项默认不展开，只显示当前层级
                // 如果需要展开子项，需要用户手动点击
            }
        }
        
        /// <summary>
        /// 获取按钮在层次结构中的索引
        /// </summary>
        private int GetButtonIndex(FoldableButtonData data)
        {
            // 先尝试从缓存获取
            if (buttonIndexCache.TryGetValue(data, out int cachedIndex))
            {
                return cachedIndex;
            }
            
            // 重新计算索引
            for (int i = 0; i < contentRoot.childCount; i++)
            {
                var child = contentRoot.GetChild(i);
                var buttonItem = child.GetComponent<FoldableButtonItem>();
                if (buttonItem != null && buttonItem.data == data)
                {
                    // 缓存结果
                    buttonIndexCache[data] = i;
                    return i;
                }
            }
            return -1;
        }
        
        /// <summary>
        /// 更新所有按钮位置（统一的位置管理方法）
        /// </summary>
        private void UpdateAllButtonPositions()
        {
            float currentY = 0f;
            
            // 按照层次结构顺序排列按钮
            var orderedButtons = GetOrderedButtonList();
            
            foreach (var buttonItem in orderedButtons)
            {
                if (buttonItem == null || buttonItem.data == null) 
                    continue;
                
                RectTransform rectTransform = buttonItem.GetComponent<RectTransform>();
                if (rectTransform == null) continue;
                
                // 计算X位置（缩进）
                float xPosition = buttonItem.data.level * config.levelIndent;
                
                // 设置位置
                rectTransform.anchoredPosition = new Vector2(xPosition, currentY);
                
                // 统一设置锚点和轴心
                SetupButtonRectTransform(rectTransform);
                
                // 计算实际高度和间距（考虑缩放）
                float actualHeight = config.buttonHeight * buttonItem.transform.localScale.y;
                float actualSpacing = config.buttonSpacing * buttonItem.transform.localScale.y;
                
                // 累加高度和间距
                currentY -= (actualHeight + actualSpacing);
            }
            
            // 更新Content高度以适应内容
            UpdateContentHeight(currentY);
        }
        
        /// <summary>
        /// 更新Content高度以适应内容
        /// </summary>
        private void UpdateContentHeight(float totalHeight)
        {
            if (contentRoot == null) return;
            
            // 计算Content需要的高度（取绝对值，因为currentY是负数）
            float contentHeight = Mathf.Abs(totalHeight);
            
            // 确保最小高度
            contentHeight = Mathf.Max(contentHeight, 100f);
            
            // 更新Content的sizeDelta
            contentRoot.sizeDelta = new Vector2(contentRoot.sizeDelta.x, contentHeight);
        }
        
        /// <summary>
        /// 设置按钮的RectTransform属性
        /// </summary>
        private void SetupButtonRectTransform(RectTransform rectTransform)
        {
            // 统一锚点设置：顶部对齐，左对齐
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            
            // 设置大小：使用配置的宽度和高度
            rectTransform.sizeDelta = new Vector2(config.buttonWidth, config.buttonHeight);
        }
        
        /// <summary>
        /// 获取按层次结构排序的按钮列表
        /// </summary>
        private List<FoldableButtonItem> GetOrderedButtonList()
        {
            if (isOrderDirty || cachedOrderedButtons == null)
            {
                cachedOrderedButtons = CalculateOrderedButtonList();
                isOrderDirty = false;
            }
            return cachedOrderedButtons;
        }
        
        /// <summary>
        /// 计算排序的按钮列表
        /// </summary>
        private List<FoldableButtonItem> CalculateOrderedButtonList()
        {
            var orderedList = new List<FoldableButtonItem>();
            
            // 递归遍历根数据，按层次结构顺序添加按钮
            foreach (var rootDataItem in rootData)
            {
                if (rootDataItem != null)
                {
                    AddButtonToListRecursive(rootDataItem, orderedList);
                }
            }
            
            return orderedList;
        }
        
        /// <summary>
        /// 递归添加按钮到列表
        /// </summary>
        private void AddButtonToListRecursive(FoldableButtonData data, List<FoldableButtonItem> list)
        {
            // 添加当前按钮
            if (buttonItems.TryGetValue(data, out var buttonItem))
            {
                list.Add(buttonItem);
            }
            
            // 如果展开状态，递归添加子按钮
            if (data.isExpanded)
            {
                foreach (var child in data.children)
                {
                    AddButtonToListRecursive(child, list);
                }
            }
        }
        
        
        /// <summary>
        /// 隐藏子项
        /// </summary>
        private void HideChildren(FoldableButtonData data)
        {
            foreach (var child in data.children)
            {
                if (buttonItems.TryGetValue(child, out var buttonItem))
                {
                    // 先递归隐藏子项
                    HideChildren(child);
                    
                    // 重置子项的展开状态
                    child.isExpanded = false;
                    
                    // 移除ButtonGroup引用
                    if (buttonGroups.ContainsKey(child))
                    {
                        buttonGroups.Remove(child);
                    }
                    
                    // 销毁按钮
                    DestroyImmediate(buttonItem.gameObject);
                    buttonItems.Remove(child);
                }
            }
        }
        
        /// <summary>
        /// 调整按钮缩放
        /// </summary>
        private IEnumerator AdjustButtonScales(FoldableButtonItem selectedItem)
        {
            if (!config.useAnimation)
            {
                ApplyScaleAdjustment(selectedItem);
                yield break;
            }
            
            float elapsedTime = 0f;
            var startScales = new Dictionary<FoldableButtonData, Vector3>();
            var targetScales = new Dictionary<FoldableButtonData, Vector3>();
            var buttonItemsSnapshot = new Dictionary<FoldableButtonData, FoldableButtonItem>(buttonItems);
            
            // 获取选中按钮及其父级和子级
            var selectedButtonData = GetSelectedButtonHierarchy(selectedItem.data);
            
            // 记录初始缩放和目标缩放
            foreach (var kvp in buttonItemsSnapshot)
            {
                if (kvp.Value != null)
                {
                    startScales[kvp.Key] = kvp.Value.transform.localScale;
                    
                    if (selectedButtonData.Contains(kvp.Key))
                    {
                        targetScales[kvp.Key] = new Vector3(1f, config.selectedButtonHeightMultiplier, 1f);
                    }
                    else
                    {
                        targetScales[kvp.Key] = new Vector3(1f, config.otherButtonHeightMultiplier, 1f);
                    }
                }
            }
            
            // 执行动画
            while (elapsedTime < config.heightAdjustAnimationTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / config.heightAdjustAnimationTime;
                
                foreach (var kvp in buttonItemsSnapshot)
                {
                    if (kvp.Value != null && startScales.ContainsKey(kvp.Key) && targetScales.ContainsKey(kvp.Key))
                    {
                        Vector3 startScale = startScales[kvp.Key];
                        Vector3 targetScale = targetScales[kvp.Key];
                        kvp.Value.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                    }
                }
                
                // 在动画过程中更新位置，确保间距正确
                UpdateAllButtonPositions();
                
                yield return null;
            }
            
            // 确保最终缩放正确
            ApplyScaleAdjustment(selectedItem);
        }
        
        /// <summary>
        /// 恢复按钮缩放
        /// </summary>
        private IEnumerator RestoreButtonScales()
        {
            if (!config.useAnimation)
            {
                RestoreAllButtonScales();
                yield break;
            }
            
            float elapsedTime = 0f;
            var startScales = new Dictionary<FoldableButtonData, Vector3>();
            var buttonItemsSnapshot = new Dictionary<FoldableButtonData, FoldableButtonItem>(buttonItems);
            
            // 记录初始缩放
            foreach (var kvp in buttonItemsSnapshot)
            {
                if (kvp.Value != null)
                {
                    startScales[kvp.Key] = kvp.Value.transform.localScale;
                }
            }
            
            // 执行动画
            while (elapsedTime < config.heightAdjustAnimationTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / config.heightAdjustAnimationTime;
                
                foreach (var kvp in buttonItemsSnapshot)
                {
                    if (kvp.Value != null && startScales.ContainsKey(kvp.Key))
                    {
                        Vector3 startScale = startScales[kvp.Key];
                        Vector3 targetScale = Vector3.one;
                        kvp.Value.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                    }
                }
                
                // 在动画过程中更新位置，确保间距正确
                UpdateAllButtonPositions();
                
                yield return null;
            }
            
            // 确保最终缩放正确
            RestoreAllButtonScales();
        }
        
        /// <summary>
        /// 获取选中按钮的层次结构（包括父级和子级）
        /// </summary>
        private HashSet<FoldableButtonData> GetSelectedButtonHierarchy(FoldableButtonData selectedData)
        {
            var selectedDataSet = new HashSet<FoldableButtonData>();
            
            // 添加当前选中的按钮
            selectedDataSet.Add(selectedData);
            
            // 添加所有父级
            AddParentButtons(selectedData, selectedDataSet);
            
            // 添加所有子级
            AddChildButtons(selectedData, selectedDataSet);
            
            return selectedDataSet;
        }
        
        /// <summary>
        /// 递归添加父级按钮
        /// </summary>
        private void AddParentButtons(FoldableButtonData data, HashSet<FoldableButtonData> selectedData)
        {
            FoldableButtonData parent = data.parent;
            if (parent != null)
            {
                selectedData.Add(parent);
                AddParentButtons(parent, selectedData); // 递归添加更高级的父级
            }
        }
        
        /// <summary>
        /// 递归添加子级按钮
        /// </summary>
        private void AddChildButtons(FoldableButtonData data, HashSet<FoldableButtonData> selectedData)
        {
            foreach (var child in data.children)
            {
                selectedData.Add(child);
                AddChildButtons(child, selectedData); // 递归添加更深层的子级
            }
        }
        
        /// <summary>
        /// 应用缩放调整
        /// </summary>
        private void ApplyScaleAdjustment(FoldableButtonItem selectedItem)
        {
            var selectedButtonData = GetSelectedButtonHierarchy(selectedItem.data);
            
            foreach (var kvp in buttonItems)
            {
                if (selectedButtonData.Contains(kvp.Key))
                {
                    kvp.Value.transform.localScale = new Vector3(1f, config.selectedButtonHeightMultiplier, 1f);
                }
                else
                {
                    kvp.Value.transform.localScale = new Vector3(1f, config.otherButtonHeightMultiplier, 1f);
                }
            }
            
            // 更新位置以适应新的缩放
            UpdateAllButtonPositions();
        }
        
        /// <summary>
        /// 恢复所有按钮缩放
        /// </summary>
        private void RestoreAllButtonScales()
        {
            foreach (var kvp in buttonItems)
            {
                kvp.Value.transform.localScale = Vector3.one;
            }
            
            // 更新位置以适应新的缩放
            UpdateAllButtonPositions();
        }
        
        /// <summary>
        /// 选中项目
        /// </summary>
        public void SelectItem(FoldableButtonData data)
        {
            if (data != null && allData.Contains(data))
            {
                // 取消之前选中的项目
                if (selectedItem != null && buttonItems.TryGetValue(selectedItem, out var prevButton))
                {
                    prevButton.Deselect();
                }
                
                // 选中新项目
                selectedItem = data;
                if (buttonItems.TryGetValue(data, out var buttonItem))
                {
                    buttonItem.Select();
                }
                
                OnItemSelected?.Invoke(data);
            }
        }
        
        /// <summary>
        /// 获取选中的项目
        /// </summary>
        public FoldableButtonData GetSelectedItem()
        {
            return selectedItem;
        }
        
        /// <summary>
        /// 添加根级数据
        /// </summary>
        public void AddRootData(FoldableButtonData data)
        {
            if (data == null)
            {
                Debug.LogWarning("尝试添加空数据！", this);
                return;
            }
            
            // 检查循环引用
            if (HasCircularReference(data))
            {
                Debug.LogError($"检测到循环引用！数据: {data.displayName}", this);
                return;
            }
            
            rootData.Add(data);
            if (isInitialized)
            {
                CreateButtonItem(data, 0);
            }
        }
        
        /// <summary>
        /// 检查循环引用
        /// </summary>
        private bool HasCircularReference(FoldableButtonData data)
        {
            if (data == null) return false;
            
            var visited = new HashSet<FoldableButtonData>();
            return CheckCircularReferenceRecursive(data, visited);
        }
        
        /// <summary>
        /// 递归检查循环引用
        /// </summary>
        private bool CheckCircularReferenceRecursive(FoldableButtonData data, HashSet<FoldableButtonData> visited)
        {
            if (visited.Contains(data))
            {
                return true; // 发现循环引用
            }
            
            visited.Add(data);
            
            foreach (var child in data.children)
            {
                if (CheckCircularReferenceRecursive(child, visited))
                {
                    return true;
                }
            }
            
            visited.Remove(data);
            return false;
        }
        
        /// <summary>
        /// 移除数据
        /// </summary>
        public bool RemoveData(FoldableButtonData data)
        {
            if (data != null && allData.Contains(data))
            {
                // 从按钮字典中移除
                if (buttonItems.TryGetValue(data, out var buttonItem))
                {
                    DestroyImmediate(buttonItem.gameObject);
                    buttonItems.Remove(data);
                }
                
                // 从数据字典中移除
                allData.Remove(data);
                
                // 从父项中移除
                if (data.parent != null)
                {
                    data.parent.RemoveChild(data);
                }
                else
                {
                    // 如果是根项，从根列表中移除
                    rootData.Remove(data);
                }
                
                OnDataChanged?.Invoke();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 刷新显示
        /// </summary>
        public void Refresh()
        {
            if (isInitialized)
            {
                Initialize();
            }
        }

        #region 编辑器方法
#if UNITY_EDITOR
        [OnInspectorGUI]
        private void OnGuiChange()
        {
            if (GUI.changed)
            {
                this.Delay(()=>Initialize());
            }
        }

        [Button("初始化")]
        private void EditorInitialize()
        {
            Initialize();
        }
        
        [Button("清除所有")]
        private void EditorClearAll()
        {
            ClearAllButtons();
            buttonItems.Clear();
            allData.Clear();
            selectedItem = null;
        }
        
        [Button("测试数据")]
        private void CreateTestData()
        {
            rootData.Clear();
            
            // 创建测试数据 - 多级层次结构
            var root1 = new FoldableButtonData("根项目1", 0);
            var child1_1 = new FoldableButtonData("子项目1-1", 1);
            var child1_2 = new FoldableButtonData("子项目1-2", 1);
            var child1_3 = new FoldableButtonData("子项目1-3", 1);
            
            // 第三级
            var child1_1_1 = new FoldableButtonData("子项目1-1-1", 2);
            var child1_1_2 = new FoldableButtonData("子项目1-1-2", 2);
            var child1_2_1 = new FoldableButtonData("子项目1-2-1", 2);
            var child1_2_2 = new FoldableButtonData("子项目1-2-2", 2);
            var child1_3_1 = new FoldableButtonData("子项目1-3-1", 2);
            
            // 第四级
            var child1_1_1_1 = new FoldableButtonData("子项目1-1-1-1", 3);
            var child1_1_1_2 = new FoldableButtonData("子项目1-1-1-2", 3);
            var child1_1_2_1 = new FoldableButtonData("子项目1-1-2-1", 3);
            var child1_2_1_1 = new FoldableButtonData("子项目1-2-1-1", 3);
            var child1_2_1_2 = new FoldableButtonData("子项目1-2-1-2", 3);
            var child1_2_2_1 = new FoldableButtonData("子项目1-2-2-1", 3);
            
            // 第五级
            var child1_1_1_1_1 = new FoldableButtonData("子项目1-1-1-1-1", 4);
            var child1_1_1_1_2 = new FoldableButtonData("子项目1-1-1-1-2", 4);
            var child1_1_2_1_1 = new FoldableButtonData("子项目1-1-2-1-1", 4);
            var child1_2_1_1_1 = new FoldableButtonData("子项目1-2-1-1-1", 4);
            
            // 构建层次结构
            child1_1_1_1.AddChild(child1_1_1_1_1);
            child1_1_1_1.AddChild(child1_1_1_1_2);
            child1_1_2_1.AddChild(child1_1_2_1_1);
            child1_2_1_1.AddChild(child1_2_1_1_1);
            
            child1_1_1.AddChild(child1_1_1_1);
            child1_1_1.AddChild(child1_1_1_2);
            child1_1_2.AddChild(child1_1_2_1);
            child1_2_1.AddChild(child1_2_1_1);
            child1_2_1.AddChild(child1_2_1_2);
            child1_2_2.AddChild(child1_2_2_1);
            
            child1_1.AddChild(child1_1_1);
            child1_1.AddChild(child1_1_2);
            child1_2.AddChild(child1_2_1);
            child1_2.AddChild(child1_2_2);
            child1_3.AddChild(child1_3_1);
            
            root1.AddChild(child1_1);
            root1.AddChild(child1_2);
            root1.AddChild(child1_3);
            
            // 第二个根项目
            var root2 = new FoldableButtonData("根项目2", 0);
            var child2_1 = new FoldableButtonData("子项目2-1", 1);
            var child2_2 = new FoldableButtonData("子项目2-2", 1);
            var child2_3 = new FoldableButtonData("子项目2-3", 1);
            
            // 第三级
            var child2_1_1 = new FoldableButtonData("子项目2-1-1", 2);
            var child2_1_2 = new FoldableButtonData("子项目2-1-2", 2);
            var child2_2_1 = new FoldableButtonData("子项目2-2-1", 2);
            var child2_3_1 = new FoldableButtonData("子项目2-3-1", 2);
            var child2_3_2 = new FoldableButtonData("子项目2-3-2", 2);
            
            // 第四级
            var child2_1_1_1 = new FoldableButtonData("子项目2-1-1-1", 3);
            var child2_1_2_1 = new FoldableButtonData("子项目2-1-2-1", 3);
            var child2_2_1_1 = new FoldableButtonData("子项目2-2-1-1", 3);
            var child2_3_1_1 = new FoldableButtonData("子项目2-3-1-1", 3);
            
            // 构建层次结构
            child2_1_1.AddChild(child2_1_1_1);
            child2_1_2.AddChild(child2_1_2_1);
            child2_2_1.AddChild(child2_2_1_1);
            child2_3_1.AddChild(child2_3_1_1);
            
            child2_1.AddChild(child2_1_1);
            child2_1.AddChild(child2_1_2);
            child2_2.AddChild(child2_2_1);
            child2_3.AddChild(child2_3_1);
            child2_3.AddChild(child2_3_2);
            
            root2.AddChild(child2_1);
            root2.AddChild(child2_2);
            root2.AddChild(child2_3);
            
            // 第三个根项目 - 简单结构
            var root3 = new FoldableButtonData("根项目3", 0);
            var child3_1 = new FoldableButtonData("子项目3-1", 1);
            var child3_2 = new FoldableButtonData("子项目3-2", 1);
            
            root3.AddChild(child3_1);
            root3.AddChild(child3_2);
            
            // 添加所有根项目
            rootData.Add(root1);
            rootData.Add(root2);
            rootData.Add(root3);
            
            Initialize();
        }

#endif
        /// <summary>
        /// 项目点击事件包装方法
        /// </summary>
        private void OnItemClickedInternal(FoldableButtonData data)
        {
            OnItemClicked?.Invoke(data);
        }

        /// <summary>
        /// 项目选中事件包装方法
        /// </summary>
        private void OnItemSelectedInternal(FoldableButtonData data)
        {
            OnItemSelected?.Invoke(data);
        }

        /// <summary>
        /// 项目展开事件包装方法
        /// </summary>
        private void OnItemExpandedWrapper(FoldableButtonData data)
        {
            if (buttonItems.TryGetValue(data, out var buttonItem))
            {
                OnItemExpandedInternal(buttonItem);
            }
        }

        /// <summary>
        /// 项目收起事件包装方法
        /// </summary>
        private void OnItemCollapsedWrapper(FoldableButtonData data)
        {
            if (buttonItems.TryGetValue(data, out var buttonItem))
            {
                OnItemCollapsedInternal(buttonItem);
            }
        }
        #endregion
    }
}
