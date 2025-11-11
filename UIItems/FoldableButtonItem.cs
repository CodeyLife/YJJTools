using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine.Events;

namespace YJJTools.UI
{
    /// <summary>
    /// 折叠按钮单项组件
    /// </summary>
    [AddComponentMenu("_YjjTool/FoldableButtonItem")]
    [RequireComponent(typeof(ButtonGroupContent))]
    public class FoldableButtonItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI引用")]
        [LabelText("按钮组件")]
        public Button button;
        
        [LabelText("按钮组内容")]
        public ButtonGroupContent buttonGroupContent;
        
        [LabelText("文本组件")]
        public TextMeshProUGUI textComponent;
        
        [LabelText("展开/收起图标")]
        public Image expandIcon;
        
        [LabelText("选中状态背景")]
        public Image selectedBackground;
        
        [Header("数据")]
        [LabelText("关联的数据")]
        public FoldableButtonData data;
        
        [Header("事件")]
        [FoldoutGroup("事件")]
        public UnityEvent<FoldableButtonData> OnItemClick = new UnityEvent<FoldableButtonData>();
        
        [FoldoutGroup("事件")]
        public UnityEvent<FoldableButtonData> OnItemExpand = new UnityEvent<FoldableButtonData>();
        
        [FoldoutGroup("事件")]
        public UnityEvent<FoldableButtonData> OnItemCollapse = new UnityEvent<FoldableButtonData>();
        
        [FoldoutGroup("事件")]
        public UnityEvent<FoldableButtonData> OnItemSelect = new UnityEvent<FoldableButtonData>();
        
        [FoldoutGroup("事件")]
        public UnityEvent<FoldableButtonData> OnItemDeselect = new UnityEvent<FoldableButtonData>();
        
        [FoldoutGroup("事件")]
        public UnityEvent<FoldableButtonData> OnItemHover = new UnityEvent<FoldableButtonData>();
        
        [FoldoutGroup("事件")]
        public UnityEvent<FoldableButtonData> OnItemHoverExit = new UnityEvent<FoldableButtonData>();
        
        private FoldableButtonManager manager;
        private bool isHovering = false;

        private void Start()
        {
           
            // 绑定 ButtonGroupContent 事件
            if (buttonGroupContent != null)
            {
                buttonGroupContent.clickEvent.AddListener(OnButtonClick);
                buttonGroupContent.cancelEvent.AddListener(OnButtonCancel);
                buttonGroupContent.stateEvent.AddListener(OnButtonStateChanged);

            }

        }
        
        /// <summary>
        /// 设置数据
        /// </summary>
        public void SetData(FoldableButtonData newData)
        {
            Init();
            data = newData;
            UpdateDisplay();
        }
        void Init()
        {
            manager = GetComponentInParent<FoldableButtonManager>();

            if (expandIcon == null)
                expandIcon = transform.Find("ExpandIcon")?.GetComponent<Image>();
            if (textComponent == null)
                textComponent = GetComponentInChildren<TextMeshProUGUI>();
            if (button == null)
                button = GetComponent<Button>();

            if (buttonGroupContent == null)
                buttonGroupContent = GetComponent<ButtonGroupContent>();

            if (selectedBackground == null)
                selectedBackground = GetComponent<Image>();
        }
        
        /// <summary>
        /// 更新显示
        /// </summary>
        public void UpdateDisplay()
        {
            if (data == null) return;
            
            // 更新文本
            if (textComponent != null)
            {
                textComponent.text = data.displayName;
            }
            
            // 更新展开图标
            UpdateExpandIcon();

        }
        
        /// <summary>
        /// 更新展开图标
        /// </summary>
        private void UpdateExpandIcon()
        {
            if (expandIcon == null || data == null || manager == null || manager.config == null) return;
            
            if (data.HasChildren)
            {
                expandIcon.gameObject.SetActive(true);
                
                if (manager.config.rotateExpand)
                {
                    // 通过旋转控制状态
                    expandIcon.sprite = manager.config.expandSprite;
                    expandIcon.transform.localEulerAngles = data.isExpanded ? new Vector3(0, 0, -90) : Vector3.zero;
                }
                else
                {
                    // 通过切换图标控制状态
                    expandIcon.sprite = data.isExpanded ? manager.config.collapseSprite : manager.config.expandSprite;
                    expandIcon.transform.localEulerAngles = Vector3.zero;
                }
            }
            else
            {
                expandIcon.gameObject.SetActive(false);
            }
        }
        

        
        /// <summary>
        /// 按钮状态改变事件
        /// </summary>
        private void OnButtonStateChanged(bool isSelected)
        {
            if (data != null)
            {
                data.isSelected = isSelected;
              
                if (isSelected)
                {
                    OnItemSelect?.Invoke(data);
                }
                else
                {
                    OnItemDeselect?.Invoke(data);
                }
            }
        }
        
        /// <summary>
        /// 按钮点击事件
        /// </summary>
        private void OnButtonClick()
        {
            if (data == null) return;
            
            // 触发点击事件
            OnItemClick?.Invoke(data);
            
            if (manager != null)
            {
                // 处理同级互斥：先折叠同级的其他已展开按钮
                manager.HandleSameLevelExclusion(data);
                
                // 如果有子项，展开
                if (data.HasChildren && !data.isExpanded)
                {
                    Expand();
                }
            }
        }
        
        /// <summary>
        /// 按钮取消事件
        /// </summary>
        private void OnButtonCancel()
        {
            if (data == null) return;
            
            // 如果有子项，收缩
            if (data.HasChildren && data.isExpanded)
            {
                Collapse();
            }
        }
        
        /// <summary>
        /// 展开
        /// </summary>
        public void Expand()
        {
            if (data == null || !data.HasChildren) return;
            
            data.isExpanded = true;
            UpdateExpandIcon();
            OnItemExpand?.Invoke(data);
            
            if (manager != null)
            {
                manager.OnItemExpandedInternal(this);
            }
        }
        
        /// <summary>
        /// 收起
        /// </summary>
        public void Collapse()
        {
            if (data == null) return;
            
            data.isExpanded = false;
            UpdateExpandIcon();
            OnItemCollapse?.Invoke(data);
            
            if (manager != null)
            {
                manager.OnItemCollapsedInternal(this);
            }
        }
        
        /// <summary>
        /// 选中
        /// </summary>
        public void Select()
        {
            if (data == null || buttonGroupContent == null) return;
            
            // 使用 ButtonGroupContent 的选中功能
            buttonGroupContent.Change2Click();
        }
        
        /// <summary>
        /// 取消选中
        /// </summary>
        public void Deselect()
        {
            if (data == null || buttonGroupContent == null) return;
            
            // 使用 ButtonGroupContent 的取消选中功能
            buttonGroupContent.Cancel();
        }
        
        /// <summary>
        /// 设置可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            // 注意：isVisible 字段已从 FoldableButtonData 中移除
            // 可见性现在只通过 GameObject.SetActive 控制
            gameObject.SetActive(visible);
        }
        
        #region 鼠标悬停事件
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            if (data != null && !data.isSelected)
            {

                OnItemHover?.Invoke(data);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            if (data != null)
            {
       
                OnItemHoverExit?.Invoke(data);
            }
        }
        #endregion
        
        #region 编辑器方法
#if UNITY_EDITOR
        [Button("测试展开")]
        private void TestExpand()
        {
            if (data != null && data.HasChildren)
            {
                Expand();
            }
        }
        
        [Button("测试收起")]
        private void TestCollapse()
        {
            if (data != null)
            {
                Collapse();
            }
        }
        
        [Button("测试选中")]
        private void TestSelect()
        {
            Select();
        }
        
        [Button("测试取消选中")]
        private void TestDeselect()
        {
            Deselect();
        }
#endif
        #endregion
    }
}
