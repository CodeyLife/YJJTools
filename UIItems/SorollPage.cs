using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(EventTrigger))]
public class SorollPage:MonoBehaviour
{
    private ScrollRect _scroll;
    public RectTransform view;
    public RectTransform content;
    public float speed = 1;
    public float minDrag = 5;  //拖动超过这个值就翻页
    public int pageCount = 6;
    public float animationTime = 0.15f;
    [LabelText("显示page效果的UI根节点")]
    public Transform pageUIRoot;
    public GameObject uiPrefab;
    public GameObject pagePrefab;
    public bool changeColor = true;
    [ShowIf("changeColor")]
    public Color targetColor = Color.white;

    private Color oldColor;
    private int maxCount; //数量
    private int maxPage; // 最大页数
    private int index = 0;// 当前页数
    private float pageValue; //每一页的value
    private EventTrigger trigger;
    private float maxY;
    private bool isInit = false;

    private void Awake()
    {
        //注册事件
        var drag = new Entry();
        drag.eventID = EventTriggerType.Drag;
        drag.callback.AddListener(OnDrag);
        var endDrag = new Entry();
        endDrag.eventID = EventTriggerType.EndDrag;
        endDrag.callback.AddListener(OnEndDrag);
        trigger = GetComponent<EventTrigger>();
        trigger.triggers.Add(drag);
        trigger.triggers.Add(endDrag);
        if (!isInit)
        {
            oldColor = pagePrefab.GetComponent<Image>().color;
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            Caculate();
        }
    }
    private void Caculate()
    {
        oldColor = pagePrefab.GetComponent<Image>().color;
        maxCount = content.childCount;
        maxPage = UnityEngine.Mathf.CeilToInt((float)maxCount / pageCount);
        maxY = content.sizeDelta.y - view.sizeDelta.y;
        pageValue = maxY / maxPage;
        isInit = true;
    }
    public void SetGraph(int length,Action<int,Transform> action)
    {
        //删除旧物体
        int count = content.childCount;
        while (count > 0)
        {
            DestroyImmediate(content.GetChild(0).gameObject);
            count--;
        }
        count = pageUIRoot.childCount;
        while (count > 0)
        {
            DestroyImmediate(pageUIRoot.GetChild(0).gameObject);
            count--;
        }
        for (int i = 0; i < length; i++)
        {
            var trans = Instantiate(uiPrefab, content).transform;
            trans.gameObject.SetActive(true);
            action?.Invoke(i, trans);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Caculate();
        for (int i = 0; i < maxPage; i++)
        {
            var go = Instantiate(pagePrefab, pageUIRoot);
            go.SetActive(true);
        }
        pageUIRoot.GetChild(0).GetComponent<Image>().color = targetColor;
        index = 0;

    }

    private void OnDrag(BaseEventData arg0)
    {
        var data = arg0 as PointerEventData;
        content.anchoredPosition += new Vector2(0, data.delta.y)*speed;
    }

    public  void OnEndDrag(BaseEventData arg0)
    {
        //垂直翻页
        var current = content.anchoredPosition;
        float last = index * pageValue;
        var delta = content.anchoredPosition.y - last;
        //小于最低限制

        if (Mathf.Abs(delta) < minDrag)
        {
            var old = new Vector2(content.anchoredPosition.x, last);
            StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
            {
                content.anchoredPosition = Vector2.Lerp(current, old, t);
            }));
            return;
        }
        pageUIRoot.GetChild(index).GetComponent<Image>().color = oldColor;
        var target = Vector2.zero;
        if (delta < 0 && index > 0)
        {
            index--;

        }
        else if (delta > 0 && index < maxPage - 1)
        {
            index++;
        }
        pageUIRoot.GetChild(index).GetComponent<Image>().color = targetColor;
        target = new Vector2(current.x, index * pageValue);
        StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
        {
            content.anchoredPosition = Vector2.Lerp(current, target, t);
        }));
    }
}
