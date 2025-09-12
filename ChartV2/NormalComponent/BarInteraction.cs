using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 柱状图交互组件
/// </summary>
public class BarInteraction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private ChartV2Component_Bar barChart;
    private string barKey;
    private int dataIndex;
    private int barIndex;
    private Vector2 position;
    private float value;

    /// <summary>
    /// 初始化交互组件
    /// </summary>
    public void Initialize(ChartV2Component_Bar chart, string key, int dataIdx, int barIdx, Vector2 pos, float dataValue)
    {
        barChart = chart;
        barKey = key;
        dataIndex = dataIdx;
        barIndex = barIdx;
        position = pos;
        value = dataValue;
    }

    /// <summary>
    /// 获取世界坐标位置
    /// </summary>
    private Vector2 GetWorldPosition()
    {
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            return rectTransform.position;
        }
        return position;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (barChart != null)
        {
            barChart.OnBarHoverEnter(barKey, dataIndex, barIndex, GetWorldPosition(), value);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (barChart != null)
        {
            barChart.OnBarHoverExit(barKey, dataIndex, barIndex, GetWorldPosition(), value);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (barChart != null)
        {
            barChart.OnBarClick(barKey, dataIndex, barIndex, GetWorldPosition(), value);
        }
    }
}
