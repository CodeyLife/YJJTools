using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("_YjjTool/ButtonGroup")]
public class ButtonGroup : MonoBehaviour
{
    [ReadOnly,ShowInInspector]
    private ButtonGroupContent last;
    [LabelText("点击同一个按钮是否支持取消")]
    public bool supportCancel = false;
    [Header("enabel时取消已点击的按钮")]
    public bool clearOnEnabel = false;
    [Header("按钮被隐藏时取消已点击的按钮")]
    public bool clearOnDisabel = false;

    public UnityEvent HaveButtonClickEvent = new UnityEvent();
    public UnityEvent ClearEvent = new UnityEvent();

    public ButtonGroupContent Last { get => last; set
        {
            if (last != null)
            {
                last.Cancel();
                if(value == null)
                {
                    ClearEvent?.Invoke();
                }
            }else
            {
                if (value != null)
                {
                    HaveButtonClickEvent?.Invoke();
                }
            }

            last = value;
        }
    }

    private void OnEnable()
    {
        if (clearOnEnabel)
        {
            ClearCurrentButton();
        }
    }

    public void ClearCurrentButton()
    {
        if (Last != null)
        {
            Last.Cancel();
            Last = null;
        }
    }

    /// <summary>
    /// 取消当前按钮（不处罚buttongrou的clear事件）
    /// </summary>
    public void ClearWithoutEvent()
    {
        if (last != null)
        {
            last.Cancel();
        }
        last = null;
    }
    private void OnDisable()
    {
        if (clearOnDisabel)
        {
            ClearCurrentButton();
        }
    }
}
