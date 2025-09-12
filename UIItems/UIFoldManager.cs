using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 调用流程> SetGraph > Generate > Endgraph
/// </summary>
public class UIFoldManager : MonoBehaviour
{
    public GameObject firstPrefab;
    [OnValueChanged("SecondPrefabRectSet")]
    public GameObject secondPrefab;
    public RectOffset verticalOffset;
    public float spacing = 0;
    public RectOffset secondOffset;
    public float secondSpacing = 0;
    [LabelText("默认展开第一个一级按钮")]
    public bool isFoldStart = true;
    [LabelText("默认点击第一个二级按钮"), HorizontalGroup]
    public bool isSecondStart = false;
    [HorizontalGroup,ShowIf("isSecondStart")]
    public bool invokeEventOnEnable = true;
    [InfoBox("如果没有实时数据，勾选该选项")]
    public bool InitAtAwake = true;

    private VerticalLayoutGroup _vertical;
    public RectTransform Root;
    public VerticalLayoutGroup Vertical { get
        {
            if(_vertical == null)
            {
                _vertical = Root.GetOrAddComponent<VerticalLayoutGroup>();
                _vertical.childForceExpandHeight = false;
            }
            return _vertical;
        }
        set => _vertical = value; }

    #region Inspector
    public int firstCount = 5;
    public int secondCount = 3;

    //改变次级Rect锚点
    private void SecondPrefabRectSet()
    {
        var rect = secondPrefab.transform.rectTransform();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.right;
    }

    [OnInspectorGUI]
    private void GuiChange()
    {
        if (GUI.changed)
        {
            StartCoroutine(YjjUtility.DeLay(() =>
            {
                SetGraph();
                for(int i = 0; i < firstCount; i++)
                {
                    Generate(secondCount, null,null) ;
                }
                EndGraph();
            }));
        }
    }
    #endregion
    private void Awake()
    {
        if (InitAtAwake)
        {
            SetGraph();
            for (int i = 0; i < 5; i++)
            {
                Generate(3, null, null);
            }
            EndGraph();
        }
    }
    public void SetGraph()
    {
        InitAtAwake = false;
        var group =  Root.GetOrAddComponent<ButtonGroup>();
        Vertical.padding = verticalOffset;
        Vertical.spacing = spacing;
        while (Root.childCount > 0)
        {
            DestroyImmediate(Root.GetChild(0).gameObject);
        }
    }
    public void EndGraph()
    {
        if (isFoldStart)
        {
            var button = Root.GetChild(0).GetComponent<ButtonGroupContent>();
            button.initializationMode = ButtonInitializationMode.InitializeWithEvent;
            if (!Application.isPlaying)
            {
                var child = Root.GetChild(0);
                var zj = child.GetChild(child.childCount - 1).rectTransform();
                zj.gameObject.SetActive(true);
                var rect = child.rectTransform();
                rect.sizeDelta += new Vector2(0, GetSize(zj.GetComponent<VerticalLayoutGroup>()));
            }

        }
    }
    private float GetSize(VerticalLayoutGroup ver)
    {
        var trans = ver.transform;
        float total = 0;
        foreach(Transform child in trans)
        {
            total += child.rectTransform().sizeDelta.y;
        }
        int count = trans.childCount;
        total += (count - 1) * ver.spacing + ver.padding.top;
        total = ver.padding.bottom < 0 ? total - ver.padding.bottom : total;
        return total;
    }

    public void Generate(int count,Action<Transform>firstAction, Action<int,Transform> secondAction)
    {
        //父节点
        var go = Instantiate(firstPrefab, Root).transform;
        go.gameObject.SetActive(true);
        go.GetOrAddComponent<Button>();
        var buttonContent = go.GetOrAddComponent<ButtonGroupContent>();

        firstAction?.Invoke(go);
        var vertical = go.GetComponentInChildren<VerticalLayoutGroup>();
        if(vertical == null)
        {
            var child = new GameObject("子级菜单").transform.GetOrAddComponent<RectTransform>();
            child.SetParent(go);
            child.localPosition = Vector3.zero;
            child.localScale = Vector3.one;
            vertical = child.GetOrAddComponent<VerticalLayoutGroup>();
            vertical.childControlWidth = false;
            vertical.childControlHeight = false;
            vertical.childForceExpandWidth = false;
            vertical.childForceExpandHeight = false;
            child.GetOrAddComponent<ContentSizeFitter>();
            var cgroup =  child.GetOrAddComponent<ButtonGroup>();
            cgroup.clearOnDisabel = true;
        }
        vertical.padding = secondOffset;
        vertical.spacing = secondSpacing;

        //子级菜单节点
        var trans = go.GetComponentInChildren<VerticalLayoutGroup>().transform;
        var sizeFiltter = trans.GetComponent<ContentSizeFitter>();
        sizeFiltter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        for (int i = 0; i < count; i++)
        {
            var c = Instantiate(secondPrefab, trans).transform;
            c.gameObject.SetActive(true);
            secondAction?.Invoke(i, c);
        }
        if (isSecondStart)
        {
            var sc = trans.GetChild(0)?.GetComponent<ButtonGroupContent>();
            if (sc != null)
            {
                sc.isStartButton = true;
                sc.initAtEnable = true;
                sc.invokeEventAtStart = true;
              //  sc.initNotInvokeEvent = !invokeEventOnEnable;
            }
        }
        trans.gameObject.SetActive(false);
        var rect = trans.rectTransform();
        rect.pivot = new Vector2(0, 1);
        rect.anchorMin = new Vector2(0.5f, 1);
        rect.anchorMax = new Vector2(0.5f, 1);
        rect.anchoredPosition = new Vector2(0, -firstPrefab.transform.rectTransform().sizeDelta.y);
        //一级按钮rect
        var firstRect = go.rectTransform();
        float size = GetSize(vertical);
        buttonContent.clickEvent.AddListener(() =>
        {
            rect.gameObject.SetActive(true);
            firstRect.sizeDelta += new Vector2(0, size);
        });
        buttonContent.cancelEvent.AddListener(() =>
        {
            rect.gameObject.SetActive(false);
            firstRect.sizeDelta -= new Vector2(0, size);
        });
    }
}
