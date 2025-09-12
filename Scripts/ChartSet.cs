#region 设置类
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
/// <summary>
/// 基础设置
/// </summary>
[Serializable]
public class BaseSet
{
    #region 属性

    [LabelText("自动控制数据标题数量")]
    public bool autoSpace = false;
    [LabelText("标题最小间距像素"), ShowIf("autoSpace")]
    public float minSpace = 2;

    [LabelText("多少数据显示一次数据标题"), HideIf("autoSpace"), MinValue(1)]
    public int nameSpace = 1;
    [Range(0, 10), LabelText("水平线宽"), TabGroup("轴线设置")]
    public float lineWidth = 2;
    [TabGroup("轴线设置"), LabelText("水平轴颜色")]
    public Color lineColor = Color.gray;
    [TabGroup("轴线设置"), LabelText("水平轴sprite"), PreviewField]
    public Sprite horizatalSprite;
    [TabGroup("轴线设置"), Range(0, 10), LabelText("垂直线宽")]
    public float verticalLineWidth = 2;
    [TabGroup("轴线设置"), LabelText("垂直轴颜色")] public Color verticalColor = Color.gray;
    [TabGroup("轴线设置"), PreviewField, LabelText("垂直轴sprite")]
    public Sprite verticalSprite;

    [LabelText("轴线高度"), TabGroup("轴线设置"), HideInInspector]
    public float hight = 400;
    [TabGroup("轴线设置"), LabelText("轴线宽度"), HideInInspector]
    public float width = 800;
    [TabGroup("轴线设置"), InlineEditor(InlineEditorModes.LargePreview)]
    public Material lineMaterial;
    //数据标尺属性
    [TabGroup("标尺设置")]
    [LabelText("标尺距离x轴")]
    public int ruler_distanceFromX = 0;
    [TabGroup("标尺设置")]
    [LabelText("标尺距离Y轴")]
    public int ruler_distanceFromTop = 0;
    [TabGroup("标尺设置")]
    [LabelText("数据最大值占比(%)")]
    [Range(1, 100)]
    public float dataPercent = 100;
    [TabGroup("标尺设置")]
    [LabelText("标尺颜色")]
    public Color rulerColor = Color.white;
    [TabGroup("标尺设置")]
    [LabelText("标尺字体颜色")]
    public Color ruler_textColor = Color.white;
    [TabGroup("标尺设置")]
    [LabelText("标尺线宽")]
    [Range(0, 10)]
    public float rulerLineWidth = 1;
    [TabGroup("标尺设置")]
    [LabelText("标尺宽度是否根据图表变动")]
    public bool rulerWidthDependGrah = true;
    [TabGroup("标尺设置")]
    [HideIf("rulerWidthDependGrah")]
    [LabelText("标尺宽度")]
    public float rulerWidth = 5;
    [TabGroup("标尺设置")]
    [LabelText("标尺字体大小")]
    public float ruler_textSize = 20f;
    [LabelText("标尺文字距离轴距离")]
    [TabGroup("标尺设置")]
    public float ruler_textPos = 10;
    [TabGroup("标尺设置")]
    public TMP_FontAsset font;
    [TabGroup("标尺设置")]
    [Range(2, 10)]
    [LabelText("标尺数量")] public int count = 5;
    [TabGroup("标尺设置")]
    public List<RulerSet> rulerSet = new List<RulerSet>() { new RulerSet() };
    #endregion
}
[Serializable]
public class AnimationSet
{
    [LabelText("渐入时间"), SuffixLabel("秒")]
    public float fadeInTime = 1;
    [LabelText("渐入曲线")]
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [LabelText("单次循环时间"), SuffixLabel("秒")]
    public float loopTime = 3;
    [LabelText("轮播曲线")]
    public AnimationCurve loopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [LabelText("循环渐入时间"), SuffixLabel("秒")]
    public float loopFadeTime = 0.3f;
}
public enum RulerPos
{
    Left,
    Right
}
/// <summary>
/// 标尺设置
/// </summary>
[Serializable]
public class RulerSet
{
    [EnumToggleButtons]
    public RulerPos pos = RulerPos.Left;
    [Header("标尺单位")]
    public string unit = "";
    [ShowIf("haveUnit"), LabelText("标尺中显示单位")]
    public bool shwoUnitInText = false;
    [ShowIf("haveUnit"), HideIf("shwoUnitInText")]
    public Vector2 unit_pos = Vector2.zero;
    [ShowIf("haveUnit"), HideIf("shwoUnitInText")]
    public float unit_sizeOffset = 0;
    [LabelText("自动设置最小值")]
    public bool autoSetMinValue = false;
    [HideIf("autoSetMinValue")]
    [LabelText("标尺最小值")] public float min = 0;
    [Header("最大值为0时调整为该值"), HideIf("autoSetMinValue")]
    public float zero2Max = 100;
    private float _max;
    // [HideInInspector] public float max;
    public void SetMaxValue(float value, BaseSet set)
    {
        value = value * 100 / set.dataPercent;
        _max = value;
        var minest = min + set.count - 1;
        if (_max < minest)
        {
            _max = minest;
        }
    }
    public bool haveUnit { get => !string.IsNullOrEmpty(unit); }
    public float max
    {
        get => _max;

    }
    //  public bool isViaturlLine = false;

}

/// <summary>
/// 数据标题设置
/// </summary>
[Serializable]
public class DataSet
{

    public List<string> names = new List<string>();
    [LabelText("与轴线距离"), BoxGroup("位置")]
    public float font_DistanceFomrAsix = 0;

    [LabelText("与左侧距离"), BoxGroup("位置")] public float distanceFormLeft = 20;
    [LabelText("与右侧距离"), BoxGroup("位置")] public float distanceFormRight = 20;
    [BoxGroup("字体")]
    public TMP_FontAsset font;
    [BoxGroup("字体")]
    public float fontSize = 25f;
    [BoxGroup("字体")]
    public Color fontColor = Color.white;
    [BoxGroup("字体")]
    public Vector2 rectSize = new Vector2(200, 50);
    [BoxGroup("字体")]
    public TextAlignmentOptions alignment = TextAlignmentOptions.Center;
    [LabelText("是否使用斜线")]
    [BoxGroup("字体")]
    public bool isBias = false;
    [ShowIf("isBias")]
    [LabelText("斜线角度")]
    [BoxGroup("字体")]
    public float biasAngle = 30;
    [ShowIf("isBias")]
    [LabelText("斜线距离")]
    [BoxGroup("字体")]
    public float biasHorDis = -5;
}
[Serializable]
public class TextSet
{
    public bool show = false;
    [ShowIf("show")]
    public TMP_FontAsset font;
    [ShowIf("show")]
    public float textSize = 24;
    [ShowIf("show")]
    public Color textColor = Color.white;
    [ShowIf("show")]
    public TextAlignmentOptions alignment = TextAlignmentOptions.Center;
    [ShowIf("show")]
    public Vector2 textOffset = Vector2.zero;
    [ShowIf("show")]
    public string unit = "";
}
[Serializable]
public class LineSet
{

    [LabelText("是否为曲线")]
    public bool isCurve = false;
    [LabelText("细分")]
    [Range(1, 16)]
    [ShowIf("isCurve")]
    public int smooth = 3;
    public float width = 2;
    public List<Color> colors = new List<Color>();
    [LabelText("线段材质")]
    [InlineEditor(InlineEditorModes.LargePreview)]
    public Material material;
    [LabelText("数据点样式")]
    [PreviewField]
    public Sprite sprite;
    [ShowIf("HaveSprite")]
    [LabelText("数据点颜色")]
    public Color spriteColor = Color.white;
    [LabelText("数据点缩放")]
    [ShowIf("HaveSprite")]
    public float scale = 0.1f;
    [LabelText("数据标注字体")]
    public TMP_FontAsset font;
    [ShowIf("HaveText")]
    [LabelText("字体大小")]
    public float fontSize = 10;
    [ShowIf("HaveText")]
    [LabelText("字体偏移")]
    public Vector2 fontOffeset = Vector2.up;
    [ShowIf("HaveText")]
    [LabelText("字体颜色")]
    public Color fontColor = Color.white;
    [LabelText("单位")]
    public string unit = "";
    public bool HaveSprite { get => sprite != null; }
    public bool HaveText { get => font != null; }
}
#endregion