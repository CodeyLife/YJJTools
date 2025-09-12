using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemManager : MonoBehaviour
{
    [LabelText("ui预制体")]
    public GameObject itemPrefab;
    [Header("实例化ui的根节点")]
    public RectTransform root;
    [LabelText("刚显示时是否重新actvie")]
    public bool reActive = false;
    [LabelText("激活第一个button"),HorizontalGroup]
    public bool activeFirst = false;
    [ShowIf("activeFirst"), HorizontalGroup]
    public bool invokeEventOnEnable = true;
    [Header("每页显示的数量")]
    public int countPerPage = 4;
    [LabelText("间距")]
    public float space = 5;
    [LabelText("距离顶部距离")]
    public int distanceFromTop = 10;
    [LabelText("距离左侧距离")]
    public int distanceFromLeft = 10;
    [LabelText("item数量")]
    public int count;
    [Title("翻页按钮", TitleAlignment = TitleAlignments.Centered)]
    public GameObject lastButton;
    public GameObject nextButton;
    private void Awake()
    {
        if (lastButton != null)
        {
            lastButton.GetComponent<Button>().onClick.AddListener(OnLastPageClick);
        }
        if(nextButton != null)
        {
            nextButton.GetComponent<Button>().onClick.AddListener(OnNextPageClick);
        }
    }
    #region 动画设置
    [LabelText("翻页/滚动时间"), FoldoutGroup("动画设置")]
    public float pageTime = 2;
    [LabelText("是否开启自动滚动"), FoldoutGroup("动画设置")]
    public bool openLoop = false;
    [LabelText("滚动间隔时间"), FoldoutGroup("动画设置"), ShowIf("openLoop")]
    public float loopSpaceTime = 5f;
    public bool HaveNextPage
    {
        get
        {
            return root.childCount - (currentPage + 1) * countPerPage > 0;
        }
    }


    private int currentPage = 0;
    private VerticalLayoutGroup layout;
    #endregion
    private void OnEnable()
    {
        PlayAnimation();
    }
    public void PlayAnimation()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        StopAllCoroutines();
        InitPosition();
        if (openLoop)
        {
            if (count <= countPerPage)
            {
                return;
            }
            float distance = itemPrefab.GetComponent<RectTransform>().sizeDelta.y + space;
            float target = Layout.padding.top - distance;
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(Loop(distance, target, 0));
            }
        }
    }
    IEnumerator Loop(float distance,float target,int num)
    {
        yield return new WaitForSeconds(loopSpaceTime);
        if (num + countPerPage >= count)
        {
            //Debug.Log("触发重置");
            //var selections = new List<Transform>();
            //for (int i = 0; i < root.childCount; i++)
            //{
            //    var go = root.GetChild(i).gameObject;
            //    if (go.activeInHierarchy)
            //    {
            //        selections.Add(go.transform);
            //    }
            //}
            num = 0;
            target = distanceFromTop - distance;
            List<Transform> transList = new List<Transform>();
            int j = 0;
            int ii = -1;
            for (int i = count - countPerPage ; i < count; i++)
            {
                // transList.Add(selections[i]);
                for (; j < root.childCount; j++)
                {
                    var go = root.GetChild(j).gameObject;
                    if (go.activeInHierarchy)
                    {
                        ii++;
                        if(ii == i)
                        {
                            transList.Add(go.transform);
                            j++;
                            break;
                        }
                    }
                }

            }
            for(int i = 0; i < transList.Count; i++)
            {
                transList[i].SetSiblingIndex(i);
            }
          //  root.GetChild(childCount - 1).SetSiblingIndex(transList.Count);
            InitPosition();
        }
        float start = Layout.padding.top;
        //  Debug.Log(num,root.GetChild(num+ countPerPage).gameObject);
        if (reActive)
        {
            var go = root.GetChild(num + countPerPage).gameObject;
            go.SetActive(false);
            go.SetActive(true);
        }
        yield return StartCoroutine(YjjUtility.FadeIn(pageTime, (t) =>
        {
            Layout.padding.top = (int)Mathf.Lerp(start, target, t);
            Layout.SetLayoutVertical();

        }));
        target -= distance;
        num++;
        StartCoroutine(Loop(distance,target, num));
    }
    #region 编辑器方法
#if UNITY_EDITOR
    [OnInspectorInit]
    private void ChangeSize()
    {
        var mask = transform.Find("mask");
        if (mask != null)
        {
            var rect = mask.rectTransform();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }
         var content = root.GetOrAddComponent<ContentSizeFitter>();
        content.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
    [OnInspectorGUI]
    private void GuiChange()
    {
        if (GUI.changed)
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(Delay());
            }
            else
            {
                SetGraph();
            }
        }
    }
#endif
    IEnumerator Delay()
    {
        yield return null;
        SetGraph();
    }
    #endregion
    public void SetVerticalSize(int count)
    {
        float prefabH = itemPrefab.transform.rectTransform().sizeDelta.y;
        float all = prefabH * count + (count ) * space + distanceFromTop;
        RectTransform rect = root.rectTransform();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, all);
    }
    /// <summary>
    /// 生成UI item
    /// </summary>
    public void SetGraph(Action<int,Transform> action = null)
    {
        StopAllCoroutines();
        InitPosition();
        Layout.padding.top = distanceFromTop;
        Layout.padding.left = distanceFromLeft;
        Layout.spacing = space;
#if UNITY_EDITOR
        if (Layout != null)
        {
            UnityEditor.EditorUtility.SetDirty(Layout.gameObject);
        }
#endif
        int c = root.childCount;
        while (c > 0)
        {
            DestroyImmediate(root.GetChild(0).gameObject);
            c--;
        }
        for(int i = 0; i < count; i++)
        {
           var go =  Instantiate(itemPrefab, root);
            go.name = i.ToString();
            go.SetActive(true);
            var rect = go.GetComponent<RectTransform>();
            Vector2 anchor = new Vector2(0, 1);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0, 1);
            action?.Invoke(i, go.transform);
        }
        if (Application.isPlaying)
        {
            PlayAnimation();
        }
        var fillter = Layout.transform.GetComponent<ContentSizeFitter>();
        if (fillter != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(fillter.rectTransform());
        }
        if (activeFirst)
        {
            var but = root.GetChild(0).GetComponent<Button>();
            if (but != null)
            {
                but.onClick?.Invoke();
            }
            var content = root.GetChild(0).GetComponent<ButtonGroupContent>();
            if (content != null)
            {
                content.initializationMode = ButtonInitializationMode.InitializeWithEvent;
              //  content.initNotInvokeEvent = !invokeEventOnEnable;
            }
        }
    }
    public void SetGraph(int num, Action<int, Transform> action = null)
    {
        count = num;
        SetGraph(action);
    }

    /// <summary>
    /// 将item位置设为初始位置
    /// </summary>
    public void InitPosition()
    {
        currentPage = 0;
        if (lastButton != null)
        {
            lastButton.SetActive(false);
        }
        Layout.padding.top = distanceFromTop;
        Layout.padding.left = distanceFromLeft;
        Layout.spacing = space;
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(YjjUtility.DeLay(() =>
            {
                Layout.SetLayoutVertical();
            }));
        }
        currentTop = distanceFromTop;
    }

    //当前layout的top值
    private float currentTop = 0;

    protected VerticalLayoutGroup Layout { get { if (layout == null) layout = root.transform.GetOrAddComponent<VerticalLayoutGroup>();return layout; } set => layout = value; }

    //点击下一页
    public void OnNextPageClick()
    {
        if (!HaveNextPage) return;
        var size = itemPrefab.GetComponent<RectTransform>().sizeDelta.y;
        float  h = (size + space) * countPerPage;
        float start = currentTop ==0? Layout.padding.top:currentTop;
        float target = start - h;
        currentTop = target;
        StartCoroutine(YjjUtility.FadeIn(pageTime, (t) =>
        {
            Layout.padding.top = (int)Mathf.Lerp(start, target, t);
            Layout.SetLayoutVertical();
        }));
        currentPage++;
        lastButton.SetActive(true);
        if (!HaveNextPage)
        {
            nextButton.gameObject?.SetActive(false);
        }
    }
    //上一页
    public void OnLastPageClick()
    {
        if (currentPage > 0)
        {
            var size = itemPrefab.GetComponent<RectTransform>().sizeDelta.y;
            float h = (size + space) * countPerPage;
            float start = currentTop == 0 ? Layout.padding.top : currentTop;
            float target = start + h;
            currentTop = target;
            StartCoroutine(YjjUtility.FadeIn(pageTime, (t) =>
            {
                Layout.padding.top = (int)Mathf.Lerp(start, target, t);
                Layout.SetLayoutVertical();
            }));
            currentPage--;
        }
        nextButton?.SetActive(true);
        if(currentPage == 0)
        {
            lastButton?.SetActive(false);
        }
    }
}
