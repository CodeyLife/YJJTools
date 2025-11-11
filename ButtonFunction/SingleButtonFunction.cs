using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SingleButtonFunction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region 参数
    public Sprite clickSprite;
    [LabelText("需要二次确认")]
    public bool needDoubleSure = false;
    [Header("是否开启hover")]
    public bool openHover = false;
    [ShowIf("openHover")]
    public Color hoverTextColor = Color.white;
    [ShowIf("openHover")]
    public Sprite hoverSprite;

    [Header("改变sprite后设置为原大小")]
    public bool setNativesize = false;
    [Header("改变text颜色")]
    public bool changeTextColor = false;
    [ShowIf("changeTextColor")]
    public Color changeColor = Color.red;
    [Header("改变sprite颜色")]
    public bool changeSpriteColor = false;
    [ShowIf("changeSpriteColor")]
    public Color spriteColor = Color.blue;

    [Title("初始化设置")]
    [LabelText("初始是否为按下")]
    public bool firstIsClick = false;
    [LabelText("初始化是否执行事件")]
    public bool invokeEventAtStart = false;
    [LabelText("消失时切换初始状态")]
    public bool disable2Reset = false;
    [Header("重新显示时切回初始状态")]
    public bool enable2SetState = false;
    [LabelText("隐藏时切换成未点击")]
    public bool disable2UnClick = false;


    [LabelText("是否按下状态"),ReadOnly]
    public bool isClick = false;
    [Title("打开按钮显示", TitleAlignment = TitleAlignments.Centered)]
    public List<GameObject> shows = new List<GameObject>();
    [ShowIf("@shows.Count>0")]
    public bool hideShowsOnClose = true;
    [Title("打开关闭得物体", TitleAlignment = TitleAlignments.Centered)]
    public List<GameObject> hides = new List<GameObject>();
    [ShowIf("@hides.Count>0")]
    public bool showHidesOnClose = true;
    public UnityEvent ClickEvent = new UnityEvent();
    public UnityEvent CancelEvent = new UnityEvent();
    [LabelText("状态事件")]
    public BoolEvent StateEvent = new BoolEvent();
    public UnityEvent<bool> OnHoverChange = new UnityEvent<bool>();
    #endregion

    //私有属性
    private Sprite _oldSprite;
    private Image m_image;
    private Button _button;
    private Component _textComponent; // Text 或 TextMeshProUGUI
    private Color oldColor;
    private Color oldSpriteColor;
   
    private void OnDisable()
    {
        if (disable2Reset)
        {
            SetState(firstIsClick);
        }
        if (disable2UnClick)
        {
            SetState(false);
        }
    }
    private void OnEnable()
    {
        if (enable2SetState)
        {
            if (invokeEventAtStart)
            {
                SetStateForce(firstIsClick);
            }
            else
            {
                SetStateWithoutEvent(firstIsClick);
            }
        }
    }
    public Image MImage
    {
        get
        {
            if (m_image == null)
            {
                m_image = transform.GetComponent<Image>();
                if (changeSpriteColor && oldSpriteColor == default(Color))
                {
                    oldSpriteColor = m_image.color;
                }
            }
            return m_image;
        }
        set => m_image = value;
    }

    public Sprite OldSprite
    {
        get
        {
            if (_oldSprite == null)
            {
                _oldSprite = MImage.sprite;
            }
            return _oldSprite;
        }
        set => _oldSprite = value;
    }

    private void Awake()
    {
        // 缓存组件引用
        _button = GetComponent<Button>();
        
        // 缓存文本组件
        var pro = transform.GetComponentInChildren<TextMeshProUGUI>();
        if (pro != null)
        {
            _textComponent = pro;
            oldColor = pro.color;
        }
        else
        {
            var text = transform.GetComponentInChildren<Text>();
            if (text != null)
            {
                _textComponent = text;
                oldColor = text.color;
            }
        }
        
        // 初始化 oldSprite
        if (_oldSprite == null)
        {
            _oldSprite = MImage.sprite;
        }
        
        // 初始化 oldSpriteColor
        if (changeSpriteColor && oldSpriteColor == default(Color))
        {
            oldSpriteColor = MImage.color;
        }
        
        // 注册按钮点击事件
        if (_button != null)
        {
            _button.onClick.AddListener(OnClick);
        }
        
        // 初始化状态
        if (!enable2SetState)  //如果不在显示时执行状态切换 
        {
            if (invokeEventAtStart)
            {
                SetState(firstIsClick);
            }
            else
            {
                SetStateWithoutEvent(firstIsClick);
            }
        }
    }
    public void OnClick()
    {
        if (needDoubleSure)
        {
            DoubleSure.Instance.RequestSure((sure) =>
            {
                if (sure)
                {
                    OnClickWithOutDoubleSure();
                }
            });
        }
        else
        {
            OnClickWithOutDoubleSure();
        }

    }
    public void OnClickWithOutDoubleSure()
    {
        SetState(!isClick);
    }
    public void SetStateWithoutEvent(bool state)
    {
        EnsureOldSpriteInitialized();
        ApplyStateVisuals(state, false);
    }
    /// <summary>
    /// 如果已经是该状态 则直接退出，如果不是执行对应事件并切换状态
    /// </summary>
    /// <param name="state"></param>
    public void SetState(bool state)
    {
        EnsureOldSpriteInitialized();
        if (isClick == state) return;
        SetStateForce(state);
    }
    public void SetStateForce(bool state)
    {
        EnsureOldSpriteInitialized();
        StateEvent?.Invoke(state);
        ApplyStateVisuals(state, true);
        
        if (state)
        {
            ClickEvent?.Invoke();
        }
        else
        {
            CancelEvent?.Invoke();
        }
    }

    #region 辅助方法
    /// <summary>
    /// 更新 Sprite
    /// </summary>
    private void UpdateSprite(bool state)
    {
        if (state)
        {
            if (clickSprite != null)
            {
                MImage.sprite = clickSprite;
            }
        }
        else
        {
            if (_oldSprite != null)
            {
                MImage.sprite = _oldSprite;
            }
        }
    }

    /// <summary>
    /// 更新 GameObject 的显示/隐藏状态
    /// </summary>
    private void UpdateGameObjects(bool state, bool invokeEvents)
    {
        if (state)
        {
            // 点击状态：隐藏 hides，显示 shows
            for (int i = 0; i < hides.Count; i++)
            {
                if (hides[i] != null)
                {
                    hides[i].SetActive(false);
                }
            }
            for (int i = 0; i < shows.Count; i++)
            {
                if (shows[i] != null)
                {
                    shows[i].SetActive(true);
                }
            }
        }
        else
        {
            // 未点击状态：根据配置显示/隐藏
            if (invokeEvents)
            {
                // SetStateForce 时根据配置处理
                if (showHidesOnClose)
                {
                    for (int i = 0; i < hides.Count; i++)
                    {
                        if (hides[i] != null)
                        {
                            hides[i].SetActive(true);
                        }
                    }
                }
                if (hideShowsOnClose)
                {
                    for (int i = 0; i < shows.Count; i++)
                    {
                        if (shows[i] != null)
                        {
                            shows[i].SetActive(false);
                        }
                    }
                }
            }
            else
            {
                // SetStateWithoutEvent 时直接隐藏 shows
                for (int i = 0; i < shows.Count; i++)
                {
                    if (shows[i] != null)
                    {
                        shows[i].SetActive(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 更新颜色
    /// </summary>
    private void UpdateColors(bool state)
    {
        if (changeSpriteColor)
        {
            MImage.color = state ? spriteColor : oldSpriteColor;
        }
    }

    /// <summary>
    /// 应用状态视觉效果
    /// </summary>
    private void ApplyStateVisuals(bool state, bool invokeEvents)
    {
        isClick = state;
        UpdateSprite(state);
        UpdateGameObjects(state, invokeEvents);
        UpdateColors(state);
        
        if (setNativesize)
        {
            MImage.SetNativeSize();
        }
    }

    /// <summary>
    /// 确保 _oldSprite 已初始化
    /// </summary>
    private void EnsureOldSpriteInitialized()
    {
        if (_oldSprite == null)
        {
            _oldSprite = MImage.sprite;
        }
    }
    #endregion

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isClick)
        {
            OnHoverChange?.Invoke(false);
        }
       
        if (openHover)
        {
            if (hoverSprite != null)
            {
                var sprite = isClick ? clickSprite : OldSprite;
                MImage.sprite = sprite;
            }
            var color = oldColor;
            if(changeTextColor && isClick)
            {
                color = changeColor;
            }
            SetColor(color);
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isClick)
        {
            OnHoverChange?.Invoke(true);
        }
        if (openHover)
        {
            if (isClick) return;
            SetColor(hoverTextColor);
            if (hoverSprite != null)
            {
                MImage.sprite = hoverSprite;
            }
        }
    }

    private void SetColor(Color c)
    {
        if (_textComponent == null) return;
        
        if (_textComponent is Text text)
        {
            text.color = c;
        }
        else if (_textComponent is TextMeshProUGUI pro)
        {
            pro.color = c;
        }
    }
}
