using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 按钮初始化模式枚举
/// </summary>
public enum ButtonInitializationMode
{
    [LabelText("默认状态 - 不执行任何操作，保持原始状态")]
    Default,
    [LabelText("启动时选中 - 游戏开始时自动选中此按钮")]
    StartSelected,
    [LabelText("启动时选中并执行 - 游戏开始时选中并执行点击事件")]
    StartSelectedAndExecute,
    [LabelText("每次激活时选中 - 每次显示时自动选中")]
    SelectOnEnable
}

[AddComponentMenu("_YjjTool/ButtonGroupContent")]
[RequireComponent(typeof(Button))]
public class ButtonGroupContent : MonoBehaviour, IPointerEnterHandler,IPointerExitHandler
{
    #region 参数
    [LabelText("按钮按下切换的sprite")]
    public Sprite targetSprite;
    [LabelText("切换sprite还原尺寸")]
    public bool needSetNativeSize = false;
    private Sprite oldSprite;
    [LabelText("启用颜色变化 - 按钮状态改变时改变颜色"),InlineButton("ResetColor")]
    public bool changeColor = false;
    [ShowIf("changeColor"),LabelText("未选中颜色"),OnValueChanged("@SetUnClickColorInEditor(unClickColor)")]
    public Color unClickColor = Color.white;
#if UNITY_EDITOR
    private void SetUnClickColorInEditor(Color c)
    {
        Image.color = c;
    }
    [ContextMenu("重置显隐控制")]
    public void EditorInit()
    {
        //如果物体关闭了，全部关闭
        if (!gameObject.activeInHierarchy)
        {
            foreach(var go in clickShow)
            {
                go.SetActive(false);
            }
        }
        else
        {
            // 根据初始化模式决定是否激活
            bool shouldActivate = initializationMode == ButtonInitializationMode.StartSelected || 
                                 initializationMode == ButtonInitializationMode.StartSelectedAndExecute;
            clickShow.ForEach(x => x.SetActive(shouldActivate));
        }
    }
#endif
    private void ResetColor()
    {
        Image.color = unClickColor;
    }
    [ShowIf("changeColor"),LabelText("悬停颜色"), OnValueChanged("@SetUnClickColorInEditor(hoverColor)")]
    public Color hoverColor = Color.white;
    [ShowIf("changeColor"),LabelText("选中颜色"), OnValueChanged("@SetUnClickColorInEditor(clickColor)")]
    public Color clickColor = Color.white;
    [LabelText("按钮初始化模式")]
    public ButtonInitializationMode initializationMode = ButtonInitializationMode.Default;
    
    [LabelText("允许重复点击 - 允许点击已选中的按钮")]
    public bool supportMultipleClick = false;

    public Sprite hoverSprite;
    [LabelText("改变字体颜色")]
    public bool changeTextColor = false;
    [ShowIf("changeTextColor")]
    public Color textColor = Color.white;
    [ShowIf("changeTextColor")]
    public Color hoverTextColor = Color.white;
    #endregion
    //  public ClickObject clickObject;
    private Image _image;
    #region 显隐控制
    [LabelText("先执行事件再显示 - 点击时先触发事件再显示/隐藏物体")]
    public bool eventBeforShow = false;
    [Title("激活按钮时显示的物体",TitleAlignment = TitleAlignments.Centered)]
    public List<GameObject> clickShow = new List<GameObject>();
    [ShowIf("@clickShow.Count>0"),LabelText("取消选中时隐藏 - 按钮取消选中时隐藏显示的物体")]
    public bool cancel2Hide = true;
    [Title("激活按钮时关闭的物体", TitleAlignment = TitleAlignments.Centered)]
    public List<GameObject> clickHide = new List<GameObject>();
    [ShowIf("@clickHide.Count>0"),LabelText("取消选中时显示 - 按钮取消选中时显示隐藏的物体")]
    public bool cancel2Show = true;
    #endregion
    #region 事件
    [FoldoutGroup("事件")]
    public UnityEvent clickEvent = new UnityEvent();
    [FoldoutGroup("事件")]
    public UnityEvent cancelEvent = new UnityEvent();
    [FoldoutGroup("事件")]
    public BoolEvent stateEvent = new BoolEvent();
    [FoldoutGroup("事件")]
    public UnityEvent initEvent = new UnityEvent();
    [FoldoutGroup("事件")]
    public UnityEvent<bool> HoverEvent = new UnityEvent<bool>();
    #endregion
    private ButtonGroup buttonGroup;
    private Color oldTextColor;
    public ButtonGroup ButtonGroup { get
        {
            if(buttonGroup == null)
            {
                InitButton();
            }
            return buttonGroup;
        }
        set => buttonGroup = value; }
    public Image Image { get { if (_image == null) _image = GetComponent<Image>();return _image; } set => _image = value; }

    private void Awake()
    {

        
        if(buttonGroup == null)
        {
            InitButton();
        }
    }
    
    private void InitButton()
    {
        oldSprite = Image.sprite;
        GetComponent<Button>().onClick.AddListener(delegate { OnClick(); });
        if (transform.parent == null)
        {
            Debug.Log("按钮父节点报错", transform.gameObject);
            return;
        }
        buttonGroup = transform.parent.GetOrAddComponent<ButtonGroup>();
        if (ButtonGroup == null)
        {
            Debug.LogError("在父物体未找到buttonGroup脚本", gameObject);
        }
        if (changeTextColor)
        {
            var text = transform.GetComponentInChildren<Text>();
            if (text != null)
            {
                oldTextColor = text.color;
            }
            else
            {
                var pro = transform.GetComponentInChildren<TextMeshProUGUI>();
                oldTextColor = pro.color;
            }
        }
        if (changeColor)
        {
            Image.color = unClickColor;
        }
    }
    private void OnEnable()
    {
        // 只有非默认状态才执行初始化
        if (initializationMode != ButtonInitializationMode.Default)
        {
            ExecuteInitialization(initializationMode);
        }
    }


    public bool State { get => buttonGroup.Last == this; }
    
    /// <summary>
    /// 根据初始化模式执行相应的初始化逻辑
    /// </summary>
    /// <param name="mode">初始化模式</param>
    private void ExecuteInitialization(ButtonInitializationMode mode)
    {
        switch (mode)
        {
            case ButtonInitializationMode.Default:
                // 保持默认状态，不进行任何初始化
                break;
                
            case ButtonInitializationMode.StartSelected:
                // 启动时选中，不触发事件
                Change2Click();
                initEvent?.Invoke();
                break;
                
            case ButtonInitializationMode.StartSelectedAndExecute:
                // 启动时选中并执行点击事件
                OnClick(true);
                break;

            case ButtonInitializationMode.SelectOnEnable:
                // 每次激活时自动选中
                Change2Click();
                initEvent?.Invoke();
                break;
        }
    }

    /// <summary>
    /// 按钮点击处理
    /// </summary>
    /// <param name="isInit">是否为初始化调用</param>
    public void OnClick(bool isInit = false)
    {
        //按钮取消流程
        if (ButtonGroup.supportCancel && !isInit)
        {
            if(ButtonGroup.Last == this)
            {
                ButtonGroup.Last = null;
                Cancel();
                return;
            }
        }
        //重复点击流程
        if(!supportMultipleClick && ButtonGroup.Last == this)
        {
            return;
        }
        //初始化
        if (isInit)
        {
            // 初始化时总是触发事件
            OnClickWithoutCheck();
            return;
        }
        //普通点击
        OnClickWithoutCheck();
    }
    /// <summary>
    /// 直接切换到点击状态 不做检测
    /// </summary>
    public void OnClickWithoutCheck()
    {
        if (eventBeforShow)
        {
            clickEvent?.Invoke();
            stateEvent?.Invoke(true);
            Change2Click();
        }
        else
        {
            Change2Click();
            clickEvent?.Invoke();
            stateEvent?.Invoke(true);
        }
        
    }
    public void Cancel()
    {
        ResetImage();
        if (cancel2Hide)
        {
            foreach (var go in clickShow)
            {
                if (go == null) continue;
                go.SetActive(false);
            }
        }
        if (cancel2Show)
        {
            foreach(var go in clickHide)
            {
                if (go == null) continue;
                go.SetActive(true);
            }
        }
        cancelEvent?.Invoke();
        stateEvent?.Invoke(false);
        // Only set Last to null if this button is still the current Last
        // This prevents overwriting when switching to a different button
        if (buttonGroup.Last == this)
        {
            buttonGroup.Last = null;
        }
    }

    /// <summary>
    /// 切换到按下状态，改变控制的物体显隐，不执行事件
    /// </summary>
    public void Change2Click()
    {
        ButtonGroup.Last = this;
        foreach (var go in clickShow)
        {
            if (go == null) continue;
            go.SetActive(true);
        }
        foreach (var go in clickHide)
        {
            if (go == null) continue;
            go.SetActive(false);
        }
        Change2TargetSprite();

    }

    public void SetStateWithoutEvent(bool state)
    {
        if (state)
        {
            Change2Click();
        }
        else
        {
            ResetImage();
            if (cancel2Hide)
            {
                foreach (var go in clickShow)
                {
                    if (go == null) continue;
                    go.SetActive(false);
                }
            }
            if (cancel2Show)
            {
                foreach (var go in clickHide)
                {
                    if (go == null) continue;
                    go.SetActive(true);
                }
            }
        }
    }
    private void Change2TargetSprite() //切换为按下
    {
        if (changeTextColor)
        {
            SetColor(textColor);
        }
        if (changeColor)
        {
            Image.color = clickColor;
        }
        if (targetSprite == null) return;
        Image.sprite = targetSprite;
        if (needSetNativeSize)
        {
            Image.SetNativeSize();
        }
    }
    public void ResetImage()  //重设sprite
    {

        if (changeTextColor)
        {
            SetColor(oldTextColor);
        }
        if (changeColor)
        {
            Image.color = unClickColor;
        }
        Image.sprite = oldSprite;
        if (needSetNativeSize)
        {
            Image.SetNativeSize();
        }
    }

    /// <summary>
    /// 设置文本颜色
    /// </summary>
    /// <param name="c"></param>
    private void SetColor(Color c)
    {
        var text = transform.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.color = c;
        }
        else
        {
            var pro = GetComponentInChildren<TextMeshProUGUI>();
            pro.color = c;
        }
    }


    #region hover逻辑
    public void OnPointerExit(PointerEventData eventData)
    {
        if(ButtonGroup.Last != this)
        {
            HoverEvent?.Invoke(false);
        }
        if (changeColor)
        {
            if (ButtonGroup.Last == this)
            {
                Image.color = clickColor;
            }
            else
            {
                Image.color = unClickColor;
                if (hoverSprite != null)
                {
                    hoverSprite = oldSprite;
                }
            }
        }
        if (changeTextColor)
        {
            if (ButtonGroup.Last == this)
            {
                SetColor(textColor);
            }
            else
            {
                SetColor(oldTextColor); ;
            }
        }
        if (hoverSprite != null)
        {
            if (ButtonGroup.Last == this)
            {
                Image.sprite = targetSprite;
            }
            else
            {
                Image.sprite = oldSprite;
            }

            if (needSetNativeSize)
            {
                Image.SetNativeSize();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (ButtonGroup.Last == this) return;
        HoverEvent?.Invoke(true);
        if (changeColor)
        {
            Image.color = hoverColor;
          
        }
        if (changeTextColor)
        {
            SetColor(hoverTextColor);
        }
        if(hoverSprite != null)
        {
            Image.sprite = hoverSprite;
            if (needSetNativeSize)
            {
                Image.SetNativeSize();
            }
        }
    }
    #endregion
}
