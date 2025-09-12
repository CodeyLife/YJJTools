using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaterChart : ChartBase
{
    public enum ShowType
    {
        显示百分比,
        显示原始数据
    }
    public float data = 50;
    public float maxValue = 100;
    [EnumToggleButtons]
    public ShowType type = ShowType.显示百分比;
    [LabelText("文本保留小数位数"),Range(0,4)]
    public int floatCount = 1;
    private Material _material;
    [LabelText("渐入动画时间")]
    public float fadeInTime = 1;
    [Title("材质表现", TitleAlignment = TitleAlignments.Centered)]
    [LabelText("波浪运动速度")]
    public float speed = 30;
    [LabelText("振幅"),Range(0,0.3f)]
    public float am = 0.037f;
    [LabelText("颜色")]
    public Color waterColor = Color.white;
    [LabelText("边线颜色")]
    public Color lineColor = Color.white;
    [LabelText("边线宽度"),Range(0,0.2f)]
    public float lineWidth = 0.05f;

    private TextMeshProUGUI valueText;

    private Image _waterImage;
    public Material Material { get { if (_material == null) { _material =WaterImage.material; }return _material;  }set => _material = value; }

    public Image WaterImage { get { if (_waterImage == null) _waterImage = transform.Find("Water").GetComponent<Image>(); return _waterImage; } set => _waterImage = value; }

    public void SetData(float data,float max)
    {
        this.data = data;
        this.maxValue = max;
        SetGraph();
    }
    public override void SetGraph()
    {
        base.SetGraph();
        if (Application.isPlaying)
        {
            Material = Instantiate(Material);
        }
        Material.SetFloat("_height", data / maxValue);
        Material.SetColor("_lineColor", lineColor);
        Material.SetFloat("_lineWidth", lineWidth);
        Material.SetFloat("_speed", speed);
        Material.SetFloat("_am", am);
        WaterImage.color = waterColor;
        valueText = GetComponentInChildren<TextMeshProUGUI>();
        if (valueText != null)
        {
            if(type == ShowType.显示百分比)
            {
                valueText.text = string.Format("{0}%", (data / maxValue * 100).ToString("f"+floatCount));
            }
            else
            {
                valueText.text = data.ToString("f" + floatCount);
            }
        }
    }
    public override void PlayAnimation()
    {
        base.PlayAnimation();
        var target = data / maxValue;
        var zf = Material.GetFloat("_am");
        StartCoroutine(YjjUtility.FadeIn(fadeInTime, (t) =>
         {
             Material.SetFloat("_am", zf * t);
             Material.SetFloat("_height", target*t);
             if(valueText != null)
             {
                 if(type == ShowType.显示百分比)
                 {
                     valueText.text = (target * 100 * t).ToString("f" + floatCount) + "%";
                 }
                 else
                 {
                     valueText.text = (data * t).ToString("f" + floatCount);
                 }
             }
         }));
    }
}
