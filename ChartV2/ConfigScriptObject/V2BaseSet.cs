using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class V2BaseSet : YJJScritableSingletion<V2BaseSet>
{
    public TMP_FontAsset font;
    [LabelText("与顶部距离"),LabelWidth(100),HorizontalGroup("distance")]
    public float distanceFromTop = 0;
    [LabelText("与底部距离"), LabelWidth(100), HorizontalGroup("distance")]
    public float distanceFromButtom = 0;
 
    #region 最大最小值设置
    [HorizontalGroup("auto")]
    public bool autoMax = true;
    [HideIf("autoMax"), HorizontalGroup("autoData"), LabelWidth(100)]
    public float max = 100;
    [HorizontalGroup("auto")]
    public bool autoMin = true;
    [HideIf("autoMin"), HorizontalGroup("autoData"), LabelWidth(100)]
    public float min = 0;
    #endregion
    [LabelText("数据最小间隔像素")]
    public float dataMinDistance = 100;
    [LabelText("启用中心位置模式"), PropertyTooltip("启用后，每个数据点占据固定宽度，x位置位于宽度中心")]
    public bool useCenterPosition = false;
    public float distanceFromLeft = 0;
    public float distanceFromRight = 0;
    public List<Color> colors = new List<Color> { Color.blue, Color.red };
    
    [Title("图例设置", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("数据系列名称"), PropertyTooltip("用于图例显示的数据系列名称，如果为空则使用默认命名")]
    public List<string> seriesNames = new List<string>();

    [Title("动画参数", TitleAlignment = TitleAlignments.Centered)]
    public bool openAnimation = true;
    [LabelText("渐入时间"),ShowIf("openAnimation")]
    public float fadeInTime = 2f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}
