using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
[Serializable]
public class UIMultiBarSetting 
{
    [FoldoutGroup("总长度")]
    public float tolWidth=100.0f;
    [FoldoutGroup("间隔高度")]
    public float spaceHight = 30.0f;
    [FoldoutGroup("高度")]
    public float hight = 25.0f;
    [FoldoutGroup("间隔相关设置")]
    public float space=5.0f;
    public TMP_FontAsset font;
    [FoldoutGroup("标题大小")]
    public float titleFontSize=24;
    [FoldoutGroup("标题颜色")]
    public Color titleFontColor = Color.white;
    [FoldoutGroup("标题偏移")]
    public float titlefontOffsetX = 0;
    [FoldoutGroup("标题偏移")]
    public float titlefontOffsetY = 0;
    [FoldoutGroup("数值大小")]
    public float valueFontSize = 24;
    [FoldoutGroup("数值颜色")]
    public Color valueFontColor = Color.white;
    [FoldoutGroup("数值偏移")]
    public float valueFontOffsetX = 200;
    [FoldoutGroup("数值偏移")]
    public float valueFontOffsetY = 0;
}
