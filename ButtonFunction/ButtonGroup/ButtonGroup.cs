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
            // 保存旧的按钮引用，避免在更新后丢失
            var previousLast = last;
            
            // 先更新 last 的值，这样可以打破递归循环
            // 当 Cancel() 中检查 buttonGroup.Last == this 时，已经是最新值了
            last = value;
            
            if (previousLast != null)
            {
                // 只有当切换到一个不同的新按钮时，才需要取消旧按钮
                // 如果 value == null，说明是取消操作，不应该再调用 Cancel()，避免无限递归
                // 如果 previousLast == value，说明是同一个按钮，不需要取消
                if (previousLast != value && value != null)
                {
                    previousLast.Cancel();
                }
                
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
