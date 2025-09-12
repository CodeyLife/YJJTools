using Sirenix.OdinInspector;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIItemOptimization : MonoBehaviour
{
    [Header("UI预制体")]
    public GameObject itemPrefab;
    [LabelText("生成UI根节点")]
    public Transform root;
    [LabelText("与左侧距离")]
    public float distanceFormLeft;
    [LabelText("与顶部距离")]
    public float distanceFromTop;
    [LabelText("间距")]
    public float space = 5;
    [LabelText("每页显示UI数量"), ReadOnly]
    public int countPerPage = 5;
    public int allCount = 0;

    #region 动画设置
    [LabelText("翻页/滚动时间"), FoldoutGroup("动画设置")]
    public float pageTime = 2;
    [LabelText("是否开启自动滚动"), FoldoutGroup("动画设置")]
    public bool openLoop = false;
    [LabelText("滚动间隔时间"), FoldoutGroup("动画设置"), ShowIf("openLoop")]
    public float loopSpaceTime = 2f;
    [FoldoutGroup("动画设置")]
    public UnityEvent LoopOnceEnd = new UnityEvent();

    #endregion
    private ScrollRect _scroll;
    private void OnEnable()
    {
        PlayAnimation();
    }

    Action<int, Transform> action = null; //移动ui事件
    float record; //计算的UI滑块位置
    float _height;
    bool optimizatoin = true;

    bool _init = false;
    private void Awake()
    {
        itemPrefab.gameObject.SetActive(false);
        Init();
    }
    /// <summary>
    /// 预制体的高度加间隔
    /// </summary>
    public float Height { get { if (_height <= 0) { _height = itemPrefab.transform.rectTransform().sizeDelta.y + space; } return _height; } set => _height = value; }

    public ScrollRect Scroll { get { 
            if (_scroll == null) 
            { 
                _scroll = root.parent.parent.GetComponent<ScrollRect>();
            } 
            return _scroll;
        } set => _scroll = value; }

    private RectTransform _scrollRect;
    #region 编辑器方法
#if UNITY_EDITOR
    [ButtonGroup("but"), Button("编辑prefab")]
    private void Edit()
    {
        itemPrefab.gameObject.SetActive(true);
        root.gameObject.SetActive(false);
        UnityEditor.Selection.activeGameObject = itemPrefab;
    }
    [ButtonGroup("but"), Button("结束编辑")]
    private void EndEdit()
    {
        itemPrefab.gameObject.SetActive(false);
        root.gameObject.SetActive(true);

    }
    [OnInspectorGUI]
    private void ValueChange()
    {
        if (GUI.changed)
        {
            StartCoroutine(YjjUtility.DeLay(() =>
            {
                CacluteCountPerPage();
                SetGraph(allCount, (i, t) =>
                   {
                   });

            }));
        }
    }
    private void CacluteCountPerPage()
    {
        _height = itemPrefab.transform.rectTransform().rect.height + space;
        float count = (transform.rectTransform().rect.height - distanceFromTop+space) / Height;
        countPerPage = Mathf.FloorToInt(count);
    }
    [OnInspectorInit]
    private void InspectorInit()
    {
        if (Application.isPlaying) return;
        if (Scroll == null && root != null)
        {
            var rect = root.rectTransform();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }
        else if (Scroll != null)
        {
            var tempRect = Scroll.rectTransform();
            tempRect = _scroll.rectTransform();
            tempRect.anchorMin = Vector2.zero;
            tempRect.anchorMax = Vector2.one;
            tempRect.anchoredPosition = Vector2.zero;
            tempRect.sizeDelta = Vector2.zero;
            _scroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            _scroll.horizontal = false;
            var mask = transform.GetComponent<Mask>();
            if (mask != null)
            {
                DestroyImmediate(mask);
            }
            var image = transform.GetComponent<Image>();
            if (image != null)
            {
                DestroyImmediate(image);
            }
        }
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(YjjUtility.DeLay(() =>
            {
                CacluteCountPerPage();
                SetGraph(allCount, (i, t) =>
                {

                });
            }));
        }
    }
#endif
    #endregion
    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    /// <param name="moveItemAction"></param>
    /// <param name="keepH">保持scrollView的滑动条高度</param>
    public void SetGraph(int count, Action<int, Transform> moveItemAction, bool keepH = false)
    {
        action = moveItemAction;
        allCount = count;
        SetGraph(keepH);
    }
    /// <summary>
    /// 更新列表的content数量 更新后由scollview会保持原位置
    /// </summary>
    /// <param name="value">增减数量</param>
    public void ChangeItem(int value)
    {
        allCount += value;
        SetGraph(true);
    }
    float unitH;
    private void ScrollValueChange(Vector2 value)
    {
        _waitAnimateTime = loopSpaceTime;
        float h = _scrollRect.anchoredPosition.y;
        while (Mathf.Abs(h - record) > unitH)
        {
            RectTransform t = null;
            int index = 0;
            Transform last = null;
            if (h > record)
            {
                //  当前值大于记录值 滑块下移
                record += unitH;
                t = root.GetChild(0).rectTransform();
                last = root.GetChild(root.childCount - 1);
                t.SetAsLastSibling();
                index = int.Parse(last.gameObject.name) + 1;
                t.gameObject.name = index.ToString();
                t.anchoredPosition = last.rectTransform().anchoredPosition - new Vector2(0, Height);

            }
            else
            {
                //当前值小于记录值 滑块上移
                record -= unitH;
                t = root.GetChild(root.childCount - 1).rectTransform();
                last = root.GetChild(0);
                t.SetAsFirstSibling();
                index = int.Parse(last.gameObject.name) - 1;
                t.gameObject.name = index.ToString();
                t.anchoredPosition = last.rectTransform().anchoredPosition + new Vector2(0, Height);
            }
            if (index >= 0 && index < allCount)
            {
                var bg = t.GetComponentInChildren<ButtonGroupContent>();
                if (bg != null)
                {
                    bg.clickEvent.RemoveAllListeners();
                    bg.cancelEvent.RemoveAllListeners();
                    if (bg.ButtonGroup.Last == bg)
                    {
                        bg.ButtonGroup.Last = null;
                        bg.ResetImage();
                    }
                }
                action?.Invoke(index, t.transform);
            }
        }
        //if (openLoop)
        //{
        //    StopAllCoroutines();
        //    var targetIndex = GetDataIndex();
        //    this.Delay(_waitAnimateTime, () =>
        //     {
        //         StartCoroutine(LoopAnimation(targetIndex, root.rectTransform()));
        //     });
        //}
    }
 
    private void SetGraph(bool keepH)
    {
        StopAllCoroutines();
        int c = root.childCount;
        while (c > 0)
        {
            DestroyImmediate(root.GetChild(0).gameObject);
            c--;
        }
        _init = false;
        var currentH = root.rectTransform().anchoredPosition.y;
        Init();
        int count = countPerPage;
        //当前总数超过显示
        //如果有优化 从每页数量 * -1 开始 否则为0
        int start = optimizatoin ? -1 : 0;
        //如果有优化，从每页数量 *2结束，否则为总数量
        int end = optimizatoin ? count + 1 : allCount;
        //Debug.Log($"{action == null} {start}:{end} / {allCount}");
        for (int i = start; i < end; i++)
        {
            var go = Instantiate(itemPrefab, root);
            go.name = i.ToString();
            go.SetActive(true);
            var rect = go.GetComponent<RectTransform>();
            Vector2 anchor = new Vector2(0, 1);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(distanceFormLeft, -i * Height + distanceFromTop);
            int index = i;
            if (index >= 0 && allCount != 0)
            {
                index %= allCount;
                action?.Invoke(index, go.transform);
            }
        }
       
        if (Application.isPlaying)
        {
            PlayAnimation(currentH);
        }
    }
    private void Init()
    {
        //if (_init) return;
        optimizatoin = action != null && allCount > countPerPage;
        if (Scroll != null)
        {
            _scroll.verticalNormalizedPosition = 1;
            _scrollRect = root.rectTransform();
            //改变根节点总高度
            _height = itemPrefab.transform.rectTransform().sizeDelta.y + space;
            _scrollRect.sizeDelta = new Vector2(_scrollRect.sizeDelta.x, allCount * Height - distanceFromTop - space);
            unitH = Height;
            //滑块移动事件
            record = 0;
            if (Application.isPlaying)
            {
                _scroll.onValueChanged.RemoveListener(ScrollValueChange);
                if (optimizatoin)
                {
                    _scroll.onValueChanged.AddListener(ScrollValueChange);
                }

            }
        }
        _init = true;
    }

    //播放动画
    public void PlayAnimation(float begin = 0)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        StopAllCoroutines();
        if (openLoop && allCount > countPerPage)
        {
            InitPosition(true,begin);
            //return;
            var rect = root.rectTransform();
            StartCoroutine(LoopAnimation(countPerPage + 1, rect));
        }
        else if(!openLoop && allCount > countPerPage)
        {
            InitPosition(true,begin);
        }
    }

    private float _waitAnimateTime;
    IEnumerator LoopAnimation(int index, RectTransform rect)
    {
        yield return StartCoroutine(WatiForSpace());
        Vector2 start = rect.anchoredPosition;
        Vector2 end = Vector2.zero;
        if (_scroll != null)
        {
            index = GetDataIndex();
            end = new Vector2(start.x, distanceFromTop + (index - countPerPage) * Height);
        }
        else
        {
            end = start + new Vector2(0, Height);
        }
        yield return StartCoroutine(YjjUtility.FadeIn(pageTime, (t) =>
        {
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
        }));
        if (/*optimizatoin && */_scroll == null)
        {
            var go = root.GetChild(0).transform;
            var last = root.GetChild(root.childCount - 1);
            go.SetAsLastSibling();
            int i = index % allCount;
            go.name = i.ToString();
            go.rectTransform().anchoredPosition = last.rectTransform().anchoredPosition - new Vector2(0, Height);
            var bg = go.GetComponentInChildren<ButtonGroupContent>();
            if (bg != null)
            {
                bg.clickEvent.RemoveAllListeners();
                bg.cancelEvent.RemoveAllListeners();
            }
            action?.Invoke(i, go);
            StartCoroutine(YjjUtility.DeLay(loopSpaceTime, () =>
            {
                go.gameObject.SetActive(false);
                go.gameObject.SetActive(true);
            }));
        }
        LoopOnceEnd?.Invoke();
        index++;
        var maxCount = Scroll == null? countPerPage + allCount + 1:allCount +1;
        //Debug.Log($"{gameObject.name}-{index}/{maxCount}");
        if (index >= maxCount)
        {
            index = countPerPage + 1;
            if(Scroll != null)
            {
                yield return StartCoroutine(WatiForSpace());
                var current = Scroll.verticalNormalizedPosition;
                yield return this.FadeIn(pageTime, (t) =>
                 {
                     Scroll.verticalNormalizedPosition = Mathf.Lerp(current, 1, t);
                 });
            }
            else
            {
                InitPosition(false);
            }

        }
        StartCoroutine(LoopAnimation(index, rect));
    }
    //等待动画间隔时间
    IEnumerator WatiForSpace()
    {
        _waitAnimateTime = loopSpaceTime;
        while (_waitAnimateTime > 0)
        {
            yield return null;
            _waitAnimateTime -= Time.deltaTime;
        }
    }

    //判断当前的下一个数据index
    [Button]
    private int GetDataIndex()
    {
        var value = (_scrollRect.anchoredPosition.y - distanceFromTop) / Height;
        var index = Mathf.CeilToInt(value);
        index += countPerPage;
        if (MathF.Round(value) > 0)
        {
            index++;
        }
        if(index>countPerPage + allCount)
        {
            index = countPerPage + 1;
        }
        return index;
    }

    //初始化位置
    //[Button]
    private void InitPosition(bool callAction = true,float begin = 0)
    {
        if (Scroll != null)
        {
            if(_scrollRect == null)
            {
                _scrollRect = root.rectTransform();
            }
            Scroll.verticalNormalizedPosition = (1 - begin / (_scrollRect.rect.height - transform.rectTransform().rect.height));
            ScrollValueChange(Scroll.normalizedPosition);
            //Scroll.verticalNormalizedPosition = 1;

        }
        else
        {
            root.rectTransform().anchoredPosition = Vector2.zero;
            ////暂时放到这里
            int count = root.childCount;
            int optimizationCount = optimizatoin ? 1 : 0;
            float optimizationH = optimizationCount * Height;
            for (int i = 0; i < count; i++)
            {
                var rect = root.GetChild(i).rectTransform();
                rect.anchoredPosition = new Vector2(distanceFormLeft, -i * Height + optimizationH + distanceFromTop);
                int index = i - optimizationCount;
                if (callAction && optimizatoin && index >= 0 && allCount > 0)
                {
                    index %= allCount;
                    var bg = rect.GetComponent<ButtonGroupContent>();
                    if (bg != null)
                    {
                        bg.clickEvent?.RemoveAllListeners();
                        bg.cancelEvent?.RemoveAllListeners();
                    }
                    action?.Invoke(index, rect);
                }
                rect.gameObject.name = index.ToString();
            }
        }
       
    }
}
