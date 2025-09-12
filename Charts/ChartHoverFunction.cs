using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ChartHoverFunction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public HoverEvent EnterEvent = new HoverEvent();
    public UnityEvent ExitEvent = new UnityEvent();
    public int index = 0;
    public void OnPointerEnter(PointerEventData eventData)
    {
        EnterEvent?.Invoke(index, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ExitEvent?.Invoke();
    }
}
[System.Serializable]
public class HoverEvent : UnityEvent<int, Vector2> { };
