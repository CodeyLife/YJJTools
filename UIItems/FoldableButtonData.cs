using System;
using System.Collections.Generic;
using UnityEngine;

namespace YJJTools.UI
{
    /// <summary>
    /// 折叠按钮数据项
    /// </summary>
    [Serializable]
    public class FoldableButtonData
    {
        public string displayName;
        public string description;
        
        public int level = 0;
        public FoldableButtonData parent;
        
        public bool isExpanded = false;
        public bool isSelected = false;

        public List<FoldableButtonData> children = new List<FoldableButtonData>();
        
        public FoldableButtonData()
        {
        }
        
        public FoldableButtonData(string name, int level = 0, FoldableButtonData parent = null)
        {
            this.displayName = name;
            this.level = level;
            this.parent = parent;
        }
        
        /// <summary>
        /// 添加子项
        /// </summary>
        public void AddChild(FoldableButtonData child)
        {
            child.parent = this;
            child.level = this.level + 1;
            children.Add(child);
        }
        
        /// <summary>
        /// 移除子项
        /// </summary>
        public bool RemoveChild(FoldableButtonData child)
        {
            if (children.Remove(child))
            {
                child.parent = null;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 查找子项（通过引用）
        /// </summary>
        public FoldableButtonData FindChild(FoldableButtonData target)
        {
            foreach (var child in children)
            {
                if (child == target)
                    return child;
                    
                var found = child.FindChild(target);
                if (found != null)
                    return found;
            }
            return null;
        }
        
        /// <summary>
        /// 查找子项（通过名称）
        /// </summary>
        public FoldableButtonData FindChildByName(string displayName)
        {
            foreach (var child in children)
            {
                if (child.displayName == displayName)
                    return child;
                    
                var found = child.FindChildByName(displayName);
                if (found != null)
                    return found;
            }
            return null;
        }
        
        /// <summary>
        /// 获取所有子项（递归）
        /// </summary>
        public List<FoldableButtonData> GetAllChildren()
        {
            var result = new List<FoldableButtonData>();
            foreach (var child in children)
            {
                result.Add(child);
                result.AddRange(child.GetAllChildren());
            }
            return result;
        }
        
        /// <summary>
        /// 是否有子项
        /// </summary>
        public bool HasChildren => children.Count > 0;
        
        /// <summary>
        /// 获取展开的子项数量
        /// </summary>
        public int GetExpandedChildrenCount()
        {
            int count = 0;
            foreach (var child in children)
            {
                count++; // 所有子项都计算在内
                if (child.isExpanded)
                {
                    count += child.GetExpandedChildrenCount();
                }
            }
            return count;
        }
    }
}
