using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace YJJTools.UI
{
    /// <summary>
    /// 折叠按钮配置
    /// </summary>
    [CreateAssetMenu(fileName = "FoldableButtonConfig", menuName = "YJJTools/FoldableButtonConfig")]
    public class FoldableButtonConfig : ScriptableObject
    {
        [Header("样式配置")]
        [LabelText("按钮预制体"),InfoBox("展开与收缩图标绑定子物体名 <color=green><size=25>ExpandIcon</size></color> ")]
        public GameObject buttonPrefab;
        
        [LabelText("展开图标")]
        public Sprite expandSprite;

        [LabelText("收折时旋转")]
        public bool rotateExpand = true;
        [LabelText("收起图标"),HideIf("rotateExpand")]
        public Sprite collapseSprite;
        
        
        [Header("尺寸配置")]
        [LabelText("按钮宽度")]
        public float buttonWidth = 200f;
        
        [LabelText("按钮高度")]
        public float buttonHeight = 40f;
        
        [LabelText("按钮间距")]
        public float buttonSpacing = 5f;
        
        [LabelText("层级缩进")]
        public float levelIndent = 20f;
        
        [Header("动画配置")]
        [LabelText("高度调整动画时间")]
        public float heightAdjustAnimationTime = 0.2f;
        
        [LabelText("使用动画")]
        public bool useAnimation = true;
        
        [Header("功能配置")]
        [LabelText("选中时调整其他按钮高度")]
        public bool adjustOtherButtonHeight = true;
        
        [LabelText("选中按钮高度倍数")]
        [ShowIf("adjustOtherButtonHeight")]
        public float selectedButtonHeightMultiplier = 1;
        
        [LabelText("其他按钮高度倍数")]
        [ShowIf("adjustOtherButtonHeight")]
        public float otherButtonHeightMultiplier = 0.8f;
        
        
        /// <summary>
        /// 重置为默认值
        /// </summary>
        [Button("重置为默认值")]
        public void ResetToDefaults()
        {
            
            // 重置尺寸
            buttonWidth = 200f;
            buttonHeight = 40f;
            buttonSpacing = 5f;
            levelIndent = 20f;
            
            // 重置动画
            heightAdjustAnimationTime = 0.2f;
            useAnimation = true;
            
            // 重置功能
            adjustOtherButtonHeight = true;
            selectedButtonHeightMultiplier = 1f;
            otherButtonHeightMultiplier = 0.8f;
            
        }
    }
}
