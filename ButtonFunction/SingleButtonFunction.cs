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
                if (changeSpriteColor)
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
            if (OldSprite1 == null)
            {
                OldSprite1 = MImage.sprite;
            }
            return OldSprite1;
        }
        set => OldSprite1 = value;
    }

    public Sprite OldSprite1 { get => _oldSprite; set => _oldSprite = value; }

    private void Awake()
    {
        if (OldSprite1 == null)
        {
            OldSprite1 = MImage.sprite;
        }
        var pro = transform.GetComponentInChildren<TextMeshProUGUI>();
        if (pro != null)
        {
            oldColor = pro.color;
        }
        else
        {
            var text = transform.GetComponentInChildren<Text>();
            if (text != null)
            {
                oldColor = text.color;
            }
        }
        transform.GetComponent<Button>().onClick.AddListener(OnClick);
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
        if (OldSprite1 == null)
        {
            OldSprite1 = MImage.sprite;
        }
        isClick = state;
        if (isClick)
        {
            if (clickSprite != null)
            {
                MImage.sprite = clickSprite;
            }
            for (int i = 0; i < shows.Count; i++)
            {
                shows[i].gameObject.SetActive(true);
            }
        }
        else
        {
            if (OldSprite != null)
            {
                MImage.sprite = OldSprite;
            }
            for (int i = 0; i < shows.Count; i++)
            {
                shows[i].gameObject.SetActive(false);
            }
        }
        if (setNativesize)
        {
            MImage.SetNativeSize();
        }
    }
    /// <summary>
    /// 如果已经是该状态 则直接退出，如果不是执行对应事件并切换状态
    /// </summary>
    /// <param name="state"></param>
    public void SetState(bool state)
    {
        if (OldSprite1 == null)
        {
            OldSprite1 = MImage.sprite;
        }
        if (isClick == state) return;
        SetStateForce(state);
    }
    public void SetStateForce(bool state)
    {
        StateEvent?.Invoke(state);
        isClick = state;
        if (isClick)
        {
            if (clickSprite != null)
            {
                MImage.sprite = clickSprite;
            }
            for (int i = 0; i < hides.Count; i++)
            {
                hides[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < shows.Count; i++)
            {
                shows[i].gameObject.SetActive(true);
            }
            if (changeSpriteColor)
            {
                MImage.color = spriteColor;
            }
            ClickEvent?.Invoke();
        }
        else
        {
            if (changeSpriteColor)
            {
                MImage.color = oldSpriteColor;
            }
            if (OldSprite != null)
            {
                MImage.sprite = OldSprite;
            }
            if (showHidesOnClose)
            {
                for (int i = 0; i < hides.Count; i++)
                {
                    hides[i].gameObject.SetActive(true);
                }
            }
            if (hideShowsOnClose)
            {
                for (int i = 0; i < shows.Count; i++)
                {
                    shows[i].gameObject.SetActive(false);
                }
            }
            CancelEvent?.Invoke();
        }
        if (setNativesize)
        {
            MImage.SetNativeSize();
        }
    }

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
        var text = transform.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.color = c;
        }
        else
        {
            var pro = GetComponentInChildren<TextMeshProUGUI>();
            if (pro != null) pro.color = c;
        }
    }
}
