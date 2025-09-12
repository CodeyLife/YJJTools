using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class BoolEvent : UnityEvent<bool> { }
public class YjjToggle : MonoBehaviour, IPointerClickHandler
{
    public enum ChangeType
    {
        changeBackground,
        changeToggle,
    }
    public ChangeType chanegType = YjjToggle.ChangeType.changeBackground;
    public bool needDoubleSure = false;
    [ShowIf("needDoubleSure")]
    public string sureDialog = "二次确认的文本";
    public bool state = false;
    public Color openColor = Color.blue;
    public Color closeColor = Color.grey;
    [LabelText("文本")]
    public GameObject Text;
    [ShowIf("@Text!=null")]
    public string openDesc = "开";
    [ShowIf("@Text != null")]
    public string closeDesc = "关";

    [LabelText("在显示隐藏时调用事件")]
    public bool showOrDisable2InvokeEvent = false;

    public UnityEvent openEvent = new UnityEvent();
    public UnityEvent closeEvent = new UnityEvent();
    public BoolEvent StateEvent = new BoolEvent();
    private Image _image;
    private Image _child;

    #region Inspector

    [OnInspectorGUI]
    private void OnValuechange()
    {
        if (GUI.changed)
        {
            if (!gameObject.activeInHierarchy) return;
            this.Delay(() => ChangeState(false));
        }
    }
    [OnInspectorInit]
    private void OnInit()
    {
        ChangeState(false);
    }
    #endregion
    private void Awake()
    {
        ChangeState(false);
    }

    private void OnEnable()
    {
        if (showOrDisable2InvokeEvent && state)
        {
            StateEvent.Invoke(true);
        }
    }
    private void OnDisable()
    {
        if (showOrDisable2InvokeEvent && state)
        {
            StateEvent.Invoke(false);
        }
    }

    public Image MImage
    {
        get
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }
            return _image;
        }
        set => _image = value;
    }

    public Image Child
    {
        get
        {
            if (_child == null)
            {
                _child = transform.GetChild(0).GetComponent<Image>();
            }
            return _child;
        }
        set => _child = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (needDoubleSure)
        {
            DoubleSure.Instance.RequestSure((sure) =>
            {
                if (sure)
                {
                    state = !state;
                    if (state)
                    {
                        ChangeState(true);
                    }
                    else
                    {
                        ChangeState(true);
                    }
                }
            },sureDialog);
        }
        else
        {
            state = !state;
            if (state)
            {
                ChangeState(true);
            }
            else
            {
                ChangeState(true);
            }
        }

    }
    /// <summary>
    /// 改变toggle状态并且不执行对应事件
    /// </summary>
    /// <param name="state"></param>
    public void SetStateWithoutInvokeEvent(bool state)
    {
        this.state = state;
        ChangeState(false);
    }
    /// <summary>
    /// 改变状态 并触发事件
    /// </summary>
    /// <param name="state"></param>
    public void SetState(bool state)
    {
        this.state = state;
        ChangeState(true);
    }
    /// <summary>
    /// 切换到当前state的对应状态
    /// </summary>
    /// <param name="invokeEvent">切换后是否执行事件</param>
    private void ChangeState(bool invokeEvent)
    {
        string desc;
        if (state)
        {
            if(chanegType == ChangeType.changeBackground)
            {
                MImage.color = openColor;
            }
            else
            {
                Child.color = openColor;
            }
            Child.rectTransform.anchorMin = new Vector2(1, 0.5f);
            Child.rectTransform.anchorMax = new Vector2(1, 0.5f);
            Child.rectTransform.pivot = new Vector2(1, 0.5f);
            Child.rectTransform.anchoredPosition = Vector2.zero;
            desc = openDesc;
            if (invokeEvent)
            {
                openEvent?.Invoke();
            }
        }
        else
        {
            if (chanegType == ChangeType.changeBackground)
            {
                MImage.color = closeColor;
            }
            else
            {
                Child.color = closeColor;
            }
            Child.rectTransform.anchorMin = new Vector2(0, 0.5f);
            Child.rectTransform.anchorMax = new Vector2(0, 0.5f);
            Child.rectTransform.pivot = new Vector2(0, 0.5f);
            Child.rectTransform.anchoredPosition = Vector2.zero;
            desc = closeDesc;
            if (invokeEvent)
            {
                closeEvent?.Invoke();
            }
        }
        if (Text != null)
        {
            var text = Text.GetComponent<Text>();
            if (text != null)
            {
                text.text = desc;
            }
            else
            {
                text.GetComponent<TextMeshProUGUI>().text = desc;
            }
        }
        if (invokeEvent)
        {
            StateEvent?.Invoke(this.state);
        }
    }
}
