using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Sirenix.Utilities;
using System.Linq;
using System;
using TMPro;
using YJJTool;

[RequireComponent(typeof(Image))]
public class ChartV2Base : MonoBehaviour, IPointerEnterHandler, IPointerMoveHandler, IDragHandler, IPointerExitHandler, IScrollHandler
{
    #region 参数
    [PropertyTooltip("在canvans模式是ScreenSpace-Camera时需要指定Canvas的Camera")]
    public Camera uiCamera;
    [InlineButton("CreatNewBaseSet", Label = "CreatNew"), InlineEditor/*(InlineEditorModes.FullEditor)*/, PropertyTooltip("注意这个是配置文件，修改会影响到使用同一配置文件的图表")]
    public V2BaseSet set;
    [HorizontalGroup("数据")]
    public List<MultipleData> datas = new List<MultipleData>();
    [HorizontalGroup("数据")]
    public List<string> names = new List<string>();

    [ListDrawerSettings(CustomAddFunction = "AddChartComponent", CustomRemoveElementFunction = "RemoveItem"), InlineEditor/*(InlineEditorModes.FullEditor)*/]
    public List<ChartV2ComponetBase> components = new List<ChartV2ComponetBase>();
    [ListDrawerSettings(CustomAddFunction = "AddNoramlChartComponent", CustomRemoveElementFunction = "RemoveNoramlItem"), InlineEditor/*(InlineEditorModes.GUIOnly)*/]
    public List<ChartV2ComponetBaseWithoutGraphic> normalComponents = new List<ChartV2ComponetBaseWithoutGraphic>();

    /// <summary>
    /// 注意这是基于中心点的位置，要基于左下角对齐 需要加上width*0.5 和height *0.5
    /// </summary>
    [FoldoutGroup("事件", Expanded = false)]
    public UnityEvent<Vector2> OnHoverEvent = new UnityEvent<Vector2>();
    [FoldoutGroup("事件")]
    public UnityEvent<float> OnDragEvent = new UnityEvent<float>();
    [FoldoutGroup("事件")]
    public UnityEvent<float> InitAnimationEvent = new UnityEvent<float>();
    [FoldoutGroup("事件")]
    public UnityEvent OnPointerEnterEvent = new UnityEvent();
    [FoldoutGroup("事件")]
    public UnityEvent OnPointerExitEvent = new UnityEvent();
    [FoldoutGroup("事件")]
    public UnityEvent<float> OnWheelScrollEvent = new UnityEvent<float>();
    #endregion

    #region 不暴露的数据
    [HideInInspector]
    public float width, height;  //图标宽高
    [HideInInspector]
    public float max, min; //最大最小值
    protected RectTransform _rect;
    protected float xOffset = 0;  //drag使用的x轴偏移量
    protected bool _haveMaxAndMin = false;  //是否已经计算过最大最小值
    protected bool _haveXpos = false;
    private bool canDrag = false; //是否可以拖动
    protected float dragMax;
    protected bool isPlaying = false; //是否再播放动画
    protected bool _isInit = false;  //是否已经通过脚本设置了绘制 来判断awake的时候需不需要重置
    private int _hoverDataIndex;
    private Vector2 hoverPos;
    private string _unit = null;

    protected List<float> xList = new List<float>(); //数据的x轴位置

    public void OverrideDrag(float dragMax)
    {
        this.Delay(() =>
        {
            this.dragMax = dragMax;
            canDrag = dragMax > width;
        });
    }

    /// <summary>
    /// 第index个数据在图表里X轴的位置
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public float DataPositionInX(int index)
    {
        if (XList.Count > index)
        {
            return xList[index];
        }
        
        // 如果启用了中心位置模式
        if (set.useCenterPosition)
        {
            // 每个数据点占据固定宽度，x位置位于宽度中心
            return set.distanceFromLeft + (index * set.dataMinDistance) + (set.dataMinDistance * 0.5f);
        }
        
        // 原有逻辑
        return (index * (canDrag ? set.dataMinDistance : (width - set.distanceFromLeft - set.distanceFromRight) / (names.Count - 1))) + set.distanceFromLeft;
    }
    public List<List<Vector2>> DataList { get => dataList; set => dataList = value; }


    protected List<List<Vector2>> dataList = new List<List<Vector2>>();    //数据的位置

    protected RectTransform Rect { get { if (_rect == null) _rect = transform.rectTransform(); return _rect; } set => _rect = value; }

    public List<float> XList { get => xList; set => xList = value; }
    public float XOffset { get => xOffset; set => xOffset = value; }
    public bool CanDrag { get => canDrag; }
    public int HoverDataIndex { get => _hoverDataIndex; set => _hoverDataIndex = value; }
    public Vector2 HoverPos { get => hoverPos; set => hoverPos = value; }

    /// <summary>
    /// 获取图表单位
    /// </summary>
    public string Unit
    {
        get
        {
            if (_unit == null)
            {
                var ruler = components.FirstOrDefault(x => x.GetType() == typeof(ChartV2Componet_RulerAndAxis));
                if (ruler == null)
                {
                    _unit = "";
                }
                else
                {
                    _unit = ((ChartV2Componet_RulerAndAxis)ruler).unit;
                }
            }
            return _unit;
        }
        set => _unit = value;
    }


    #endregion

    #region 编辑器方法
#if UNITY_EDITOR
    [OnInspectorInit]
    private void Init()
    {
        //UnPackPrefab();
        if (transform.parent == null)
        {
            return;
        }
        if (Application.isPlaying)
        {
            return;
        }
        //GC.Collect();
        //Rect.anchorMin = Vector2.zero;
        //Rect.anchorMax = Vector2.zero;
        UnityEditor.EditorApplication.update += ChangeSize;

        //更新UIcamera
        var parent = transform.parent;
        Canvas canvas = null;
        while( canvas == null  && parent != null)
        {
            canvas = parent.GetComponent<Canvas>();
            parent = parent.parent;
          
        }
        if (canvas != null)
        {
            if(canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                uiCamera = null;
            }
            else
            {
                uiCamera = canvas.worldCamera;
            }
        }

        SetGraph(false);

        
        // components.ForEach(x => x?.InitGraph(this));
        //components.ForEach(x => x?.SetGraph());
        // normalComponents.ForEach(x => x?.InitGraph(this));
        //normalComponents.ForEach(x => x?.SetGraph());
    }
    [OnInspectorGUI]
    protected void OnGUIChange()
    {
        if (GUI.changed)
        {
            this.Delay(() => SetGraph(false));
        }
    }
    [OnInspectorDispose]
    private void Dis()
    {
        UnityEditor.EditorApplication.update -= ChangeSize;
    }

    //添加组件选择菜单
    private void AddChartComponent()
    {
        UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();

        // 获取所有继承自ChartV2ComponetBase的类型
        System.Type baseType = typeof(ChartV2ComponetBase);
        List<System.Type> derivedTypes = AssemblyUtilities.GetTypes(AssemblyCategory.Scripts)
            .Where(type => type.IsSubclassOf(baseType) && !type.IsAbstract)
            .ToList();

        // 为每个类型添加菜单项
        foreach (System.Type type in derivedTypes)
        {
            var name = type.Name;
            var desc = type.GetAttribute<ComponentDescAttribute>();
            if (desc != null)
            {
                name = desc.desc;
            }
            menu.AddItem(new GUIContent(name), false, () =>
            {
                // 在场景中创建GameObject
                GameObject newGameObject = new GameObject(name);
                newGameObject.transform.SetParent(transform);

                // 添加选定的脚本
                ChartV2ComponetBase chartComponent = newGameObject.AddComponent(type) as ChartV2ComponetBase;

                // 将新的GameObject添加到List中
                components.Add(chartComponent);

                //初始化位置
                InitLocalRect(newGameObject.transform.rectTransform());
                chartComponent.OnCreat();
                SortComponet();
                SetGraph();
            });
        }

        // 显示菜单
        menu.ShowAsContext();
    }
    private void AddNoramlChartComponent()
    {
        UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();

        // 获取所有继承自ChartV2ComponetBase的类型
        System.Type baseType = typeof(ChartV2ComponetBaseWithoutGraphic);
        List<System.Type> derivedTypes = AssemblyUtilities.GetTypes(AssemblyCategory.Scripts)
            .Where(type => type.IsSubclassOf(baseType) && !type.IsAbstract)
            .ToList();

        // 为每个类型添加菜单项
        foreach (System.Type type in derivedTypes)
        {
            var name = type.Name;
            var desc = type.GetAttribute<ComponentDescAttribute>();
            if (desc != null)
            {
                name = desc.desc;
            }
            menu.AddItem(new GUIContent(name), false, () =>
            {
                // 在场景中创建GameObject
                GameObject newGameObject = new GameObject(name);
                newGameObject.transform.SetParent(transform, false);

                // 添加选定的脚本
                ChartV2ComponetBaseWithoutGraphic chartComponent = newGameObject.AddComponent(type) as ChartV2ComponetBaseWithoutGraphic;

                // 将新的GameObject添加到List中
                normalComponents.Add(chartComponent);

                //初始化位置
                InitLocalRect(newGameObject.transform.GetOrAddComponent<RectTransform>());
                chartComponent.OnCreat();
                SortComponet();
                SetGraph();
            });
        }

        // 显示菜单
        menu.ShowAsContext();
    }

    private void SortComponet()
    {
        List<Transform> childs = new List<Transform>();
        foreach (Transform child in transform)
        {
            childs.Add(child);
        }
        childs.Select((t) =>
        {
            var normal = t.GetComponent<ChartV2ComponetBaseWithoutGraphic>();
            int value = 0;
            if (normal != null)
            {
                var order = normal.GetType().GetAttribute<ComponentOrderAttribute>();
                if (order != null)
                {
                    value = order.order;
                }
            }
            else
            {
                var grphic = t.GetComponent<ChartV2ComponetBase>();
                if (grphic != null)
                {
                    var order = grphic.GetType().GetAttribute<ComponentOrderAttribute>();
                    if (order != null)
                    {
                        value = order.order;
                    }
                }
            }
            return new { order = value, transform = t };
        }).OrderBy(x => x.order).ForEach(x => x.transform.SetAsLastSibling());
    }

    private void RemoveItem(ChartV2ComponetBase obj)
    {
        if (obj != null)
        {
            DestroyImmediate(obj.gameObject);
        }
        components.Remove(obj);
    }
    private void RemoveNoramlItem(ChartV2ComponetBaseWithoutGraphic obj)
    {
        if (obj != null)
        {
            DestroyImmediate(obj.gameObject);
        }
        if (obj.GetType() == typeof(ChartV2Component_Bar))
        {
            var mask = GetComponent<Mask>();
            if (mask != null)
            {
                DestroyImmediate(mask);
            }
        }
        normalComponents.Remove(obj);
    }
    protected void ChangeSize()
    {
        if (this == null) return;
        if (width != Rect.rect.width || height != Rect.rect.height)
        {
            width = Rect.rect.width;
            height = Rect.rect.height;
            this.Delay(() => SetGraph());
        }

    }

    protected void CreatNewBaseSet()
    {
        var set = V2BaseSet.CreatNew();
        if (set != null)
        {
            this.set = set;
        }
    }
#endif
    #endregion

    #region UI操作事件
    public void OnPointerEnter(PointerEventData eventData)
    {
        var pos = Sceen2LocalPos(eventData.position);
        OnPointerEnterEvent?.Invoke();
        CaculateHoverData(pos);
        OnHoverEvent?.Invoke(pos);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        var pos = Sceen2LocalPos(eventData.position);
        CaculateHoverData(pos);
        OnHoverEvent?.Invoke(pos);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerExitEvent?.Invoke();
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaying || !canDrag) return;
        XOffset -= eventData.delta.x;
        xOffset = Mathf.Clamp(xOffset, 0, dragMax);
        OnDragEvent?.Invoke(XOffset);
    }
    protected void CaculateHoverData(Vector2 pos)
    {
        // pos 是已经通过 Sceen2LocalPos 转换的局部坐标（相对于图表中心）
        // 需要转换为图表坐标系（基于左下角）
        hoverPos = new Vector2(width * 0.5f, height * 0.5f) + pos;
        //判断x轴距离
        var currentX = hoverPos.x + XOffset;
        _hoverDataIndex = FindClosestIndex(xList, currentX);
    }

    private int FindClosestIndex(List<float> sortedList, float target)
    {
        if (sortedList == null || sortedList.Count == 0) return -1;
        int low = 0;
        int high = sortedList.Count - 1;

        // 边界快速判断
        if (target <= sortedList[low]) return low;
        if (target >= sortedList[high]) return high;

        while (low <= high)
        {
            int mid = (low + high) >> 1;
            float midVal = sortedList[mid];
            if (Mathf.Approximately(midVal, target))
            {
                return mid;
            }
            if (midVal < target)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        // low 是第一个大于 target 的位置，high 是最后一个小于 target 的位置
        // 返回更接近 target 的那个索引
        if (low >= sortedList.Count) return high;
        if (high < 0) return low;
        return (target - sortedList[high]) <= (sortedList[low] - target) ? high : low;
    }
    #endregion

    #region Rect相关方法
    //屏幕坐标转为图表局部坐标
    protected Vector2 Sceen2LocalPos(Vector2 sceenPos)
    {
        // 确保 uiCamera 正确设置
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, sceenPos, uiCamera, out var local))
        {
            return local;
        }
 
        return sceenPos;
    }
    //使rect基于左下角对齐
    public void InitLocalRect(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.zero;
        r.pivot = Vector2.zero;
        r.transform.localScale = Vector3.one;
        r.transform.localPosition = Vector3.zero;
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta = new Vector2(width, height);
    }
    public static void RenderText(TMP_TextInfo obj)
    {
        var text = obj.textComponent;
        if (text.transform.childCount > 0)
        {
            text.transform.GetChild(0).GetComponent<TMP_SubMeshUI>().maskable = false;
        }
        text.OnPreRenderText -= RenderText;
    }
    #endregion

    protected void Awake()
    {
        if (!_isInit) SetGraph();
    }
    protected void OnEnable()
    {
        xOffset = 0;
        OnDragEvent?.Invoke(xOffset);
        Play();
    }

    /// <summary>
    /// 计算数据的最大最小值
    /// </summary>
    public void ComputeMaxAndMin()
    {
        if (_haveMaxAndMin) return;
        if (set.autoMin || set.autoMax)
        {
            var (maxValue, minValue) = Yjj_ChartUtility.ComputeMaxAndMin(datas);
            max = set.autoMax ? maxValue : set.max;
            min = set.autoMin ? minValue : set.min;
        }
        _haveMaxAndMin = true;
    }

    /// <summary>
    /// 计算数据的X轴位置 返回是否可以拖拽
    /// </summary>
    /// <param name="withTime"></param>
    public bool ComputeDataPos(bool withTime)
    {
        if (_haveXpos) return canDrag;
        //判断是否根据时间取距离
        if (withTime)
        {
            Debug.LogError("根据时间算距离还在开发");
        }
        else
        {
            if (set.useCenterPosition)
            {
                // 中心位置模式：每个数据点占据固定宽度
                var least = names.Count * set.dataMinDistance + set.distanceFromLeft + set.distanceFromRight;
                dragMax = least - width;
                canDrag = least > width;
                
                xList.Clear();
                if (xList.Capacity < names.Count) xList.Capacity = names.Count;
                
                // 每个数据点的x位置位于其占据宽度的中心
                for (int i = 0; i < names.Count; i++)
                {
                    xList.Add(set.distanceFromLeft + (i * set.dataMinDistance) + (set.dataMinDistance * 0.5f));
                }
            }
            else
            {
                // 原有逻辑
                //判断所用最小距离是否大于图表宽度
                var least = (names.Count - 1) * set.dataMinDistance + set.distanceFromLeft + set.distanceFromRight;
                dragMax = least - width;
                canDrag = least > width;
                var space = canDrag ? set.dataMinDistance : (width - set.distanceFromLeft - set.distanceFromRight) / (names.Count - 1);
                xList.Clear();
                if (xList.Capacity < names.Count) xList.Capacity = names.Count;
                xList.Add(set.distanceFromLeft);
                for (int i = 1; i < names.Count; i++)
                {
                    xList.Add((i * space) + set.distanceFromLeft);
                }
            }
        }
        _haveXpos = true;

        //计算所有数据位置
        var h = height - set.distanceFromButtom - set.distanceFromTop;
        dataList.Clear();
        if (dataList.Capacity < datas.Count) dataList.Capacity = datas.Count;
        for (int i = 0; i < datas.Count; i++)
        {
            var multiplData = datas[i];
            var count = multiplData.datas.Count;
            var arr = new List<Vector2>(count);
            dataList.Add(arr);
            for (int j = 0; j < count; j++)
            {
                var y = YjjUtility.SmoothLerp(min, max, multiplData.datas[j]) * h + set.distanceFromButtom;
                var x = DataPositionInX(j);
                arr.Add(new Vector2(x, y));
            }
        }
        return canDrag;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="expend">前后多绘制一个数据</param>
    public void GetDragDataIndex(ref int start, ref int end, bool expend = true)
    {
        int n = xList.Count;
        if (n == 0)
        {
            start = end = 0; return;
        }
        // 使用二分查找找到第一个 >= xOffset 的索引
        int low = 0, high = n - 1, ans = n - 1;
        while (low <= high)
        {
            int mid = (low + high) >> 1;
            if (xList[mid] >= xOffset)
            {
                ans = mid; high = mid - 1;
            }
            else low = mid + 1;
        }
        start = ans;
        for (int i = n - 1; i > 0; i--)
        {
            if (XList[i] - xOffset <= width + 0.1f)
            {
                end = i; break;
            }
        }
        if (expend)
        {
            start = start == 0 ? start : start - 1;
        }
        end++;
    }

    public void SetGraph(List<List<float>> data, List<string> titles)
    {
        SetGraph(MultipleData.GetDatas(data), titles);
    }

    /// <summary>
    /// 把数据提交给图表,可以通过 MultipleData.GetDatas(List<float>数据，List<float>数据2 ... )来快速转换
    /// </summary>
    /// <param name="data"></param>
    /// <param name="titles"></param>
    public void SetGraph(List<MultipleData> data, List<string> titles)
    {
        StopAllCoroutines();
        datas = data;
        names = titles;

        SetGraph();
    }
    protected void SetGraph(bool playAnimation = true)
    {
        _haveMaxAndMin = false;
        _haveXpos = false;
        OnHoverEvent.RemoveAllListeners();
        OnDragEvent.RemoveAllListeners();
        InitAnimationEvent.RemoveAllListeners();
        OnPointerEnterEvent.RemoveAllListeners();
        OnPointerExitEvent.RemoveAllListeners();
        for (int i = 0; i < components.Count; i++)
        {
            var c = components[i];
            if (c != null) c.InitGraph(this);
        }
        for (int i = 0; i < normalComponents.Count; i++)
        {
            var c = normalComponents[i];
            if (c != null) c.InitGraph(this);
        }
        
        // 调用所有组件的SetGraph方法
        for (int i = 0; i < components.Count; i++)
        {
            var c = components[i];
            if (c != null) c.SetGraph();
        }
        for (int i = 0; i < normalComponents.Count; i++)
        {
            var c = normalComponents[i];
            if (c != null) 
            {
                c.SetGraph();
            }
        }
        _isInit = true;
        if (playAnimation)
        {
            Play();
        }
    }

    /// <summary>
    /// 修改数据后刷新图表
    /// </summary>
    public void RefreshGraph(bool drag2end = false)
    {
        SetGraph(false);
        if (drag2end && canDrag)
        {
            xOffset = dragMax;
            OnDragEvent?.Invoke(xOffset);
        }
    }

    public void Play()
    {
        //播放动画
        if (set.openAnimation && Application.isPlaying && gameObject.activeInHierarchy)
        {
            isPlaying = true;
            this.FadeIn(set.fadeInTime, (t) =>
            {
                InitAnimationEvent?.Invoke(t);

            }, endAction: () => isPlaying = false, curve: set.curve);
        }
    }


	public float ValueToLocalY(float value)
	{
		var h = height - set.distanceFromButtom - set.distanceFromTop;
		return YjjUtility.SmoothLerp(min, max, value) * h + set.distanceFromButtom;
	}

    public void OnScroll(PointerEventData eventData)
    {
        // 触发滚轮事件
        OnWheelScrollEvent?.Invoke(eventData.scrollDelta.y);
    }

}
