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
    [LabelText("不初始化 - 按钮保持默认状态")]
    None,
    [LabelText("初始化但不触发事件 - 设置为按下状态但不执行点击事件")]
    InitializeOnly,
    [LabelText("初始化并触发事件 - 设置为按下状态并执行点击事件")]
    InitializeWithEvent,
    [LabelText("每次Enable都重置为未选中状态 - 每次OnEnable时取消选中")]
    ReinitializeOnEnable,
    [LabelText("每次Enable都重置为选中状态 - 每次OnEnable时自动选中")]
    ReinitializeToSelectedOnEnable
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
    [LabelText("改变sprite颜色"),InlineButton("ResetColor")]
    public bool changeColor = false;
    [ShowIf("changeColor"),OnValueChanged("@SetUnClickColorInEditor(unClickColor)")]
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
            bool shouldActivate = initializationMode == ButtonInitializationMode.InitializeOnly || 
                                 initializationMode == ButtonInitializationMode.InitializeWithEvent;
            clickShow.ForEach(x => x.SetActive(shouldActivate));
        }
    }
#endif
    private void ResetColor()
    {
        Image.color = unClickColor;
    }
    [ShowIf("changeColor"), OnValueChanged("@SetUnClickColorInEditor(hoverColor)")]
    public Color hoverColor = Color.white;
    [ShowIf("changeColor"), OnValueChanged("@SetUnClickColorInEditor(clickColor)")]
    public Color clickColor = Color.white;
    [LabelText("按钮初始化模式")]
    public ButtonInitializationMode initializationMode = ButtonInitializationMode.None;
    
    // 兼容性字段 - 用于处理Unity场景文件中的旧序列化数据
    [System.Obsolete("请使用initializationMode替代")]
    [HideInInspector]
    public bool isStartButton = false;
    
    [System.Obsolete("请使用initializationMode替代")]
    [HideInInspector]
    public bool invokeEventAtStart = true;
    
    [System.Obsolete("请使用initializationMode替代")]
    [HideInInspector]
    public bool initAtEnable = false;
    
    [LabelText("支持多次点击")]
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
    [LabelText("事件先于显隐")]
    public bool eventBeforShow = false;
    [Title("激活按钮时显示的物体",TitleAlignment = TitleAlignments.Centered)]
    public List<GameObject> clickShow = new List<GameObject>();
    [ShowIf("@clickShow.Count>0"),LabelText("按钮取消时隐藏")]
    public bool cancel2Hide = true;
    [Title("激活按钮时关闭的物体", TitleAlignment = TitleAlignments.Centered)]
    public List<GameObject> clickHide = new List<GameObject>();
    [ShowIf("@clickHide.Count>0"),LabelText("按钮取消时显示")]
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
        // 迁移旧参数到新枚举
        MigrateOldParameters();
        
        if(buttonGroup == null)
        {
            InitButton();
        }
    }
    
    /// <summary>
    /// 迁移旧的布尔参数到新的枚举参数
    /// </summary>
    private void MigrateOldParameters()
    {
        // 如果initializationMode还是默认值，但旧参数有值，则进行迁移
        if (initializationMode == ButtonInitializationMode.None)
        {
            if (initAtEnable)
            {
                initializationMode = ButtonInitializationMode.ReinitializeOnEnable;
            }
            else if (isStartButton)
            {
                if (invokeEventAtStart)
                {
                    initializationMode = ButtonInitializationMode.InitializeWithEvent;
                }
                else
                {
                    initializationMode = ButtonInitializationMode.InitializeOnly;
                }
            }
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
        ExecuteInitialization(initializationMode);
    }
    private void Start()
    {
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
            case ButtonInitializationMode.None:
                // 不进行任何初始化
                break;
                
            case ButtonInitializationMode.InitializeOnly:
                // 只设置为按下状态，不触发事件
                Change2Click();
                initEvent?.Invoke();
                break;
                
            case ButtonInitializationMode.InitializeWithEvent:
                // 设置为按下状态并触发事件
                OnClick(true);
                break;
                
            case ButtonInitializationMode.ReinitializeOnEnable:
                // 每次Enable都重新初始化（重置为未选中状态）
                Cancel();
                ButtonGroup.SetLastClick(null);
                initEvent?.Invoke();
                break;
                
            case ButtonInitializationMode.ReinitializeToSelectedOnEnable:
                // 每次Enable都重置为选中状态
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
    }

    /// <summary>
    /// 切换到按下状态，改变控制的物体显隐，不执行事件
    /// </summary>
    public void Change2Click()
    {
        ButtonGroup.SetLastClick(this);
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
