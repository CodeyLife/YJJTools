using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemHorizontal : MonoBehaviour
{
    [LabelText("item预制体")]
    public GameObject prefab;
    [LabelText("预制体生成根节点")]
    public Transform root;
    public int perCount = 3;
    public int count;

    [Title("位置设置", TitleAlignment = TitleAlignments.Centered)]
    public int distanceFromTop;
    public int distanceFromLeft;
    public int distanceFromRight;
    public float space = 5;
    [FoldoutGroup("动画")]
    public bool openAnimation = false;
    [FoldoutGroup("动画")]
    public float animationTime = 2f;
    [FoldoutGroup("动画")]
    public float spaceTime = 5f;

    private HorizontalLayoutGroup _layout;
    private float _width;
    RectTransform _rect;

    public HorizontalLayoutGroup Layout { get
        {
            if(_layout == null)
            {
                var mask = transform.GetOrCreatUIChild<RectTransform>("Mask", (rect) =>
                 {
                     rect.anchorMin = Vector2.zero;
                     rect.anchorMax = Vector2.one;
                     rect.sizeDelta = Vector2.zero;
                     rect.anchoredPosition = Vector2.zero;
                     rect.GetOrAddComponent<Image>();
                     var m = rect.GetOrAddComponent<Mask>();
                     m.showMaskGraphic = false;
                    
                 });
                root.transform.SetParent(mask);
                var content = root.GetOrAddComponent<ContentSizeFitter>();
                content.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                _layout = root.GetOrAddComponent<HorizontalLayoutGroup>();
            }
            return _layout;
        }
        set => _layout = value; }

    public RectTransform Rect { get
        {
            if(_rect == null)
            {
                _rect = root.rectTransform();
            }
            return _rect;
        }
        set => _rect = value; }

    private void Awake()
    {
        _width = prefab.transform.rectTransform().sizeDelta.x;
    }
    #region   Inspector
    [OnInspectorGUI]
    private void Changed()
    {
        if (GUI.changed)
        {
            StartCoroutine(YjjUtility.DeLay(()=> { SetGraph(); }));
        }
    }
    #endregion
    public void SetGraph(Action<int, GameObject> action = null)
    {
        //删除旧物体
        int num = root.childCount;
        var rect = root.rectTransform();
        rect.anchorMin = new Vector2(0, 0.5f);
        rect.anchorMax = new Vector2(0, 0.5f);
        rect.pivot = new Vector2(0, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        for (int i = 0; i < num; i++)
        {
            DestroyImmediate(root.GetChild(0).gameObject);
        }
        //设置layoutgroup
        Layout.padding.top = distanceFromTop;
        Layout.padding.left = distanceFromLeft;
        Layout.padding.right = distanceFromRight;
        Layout.spacing = space;
        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, root);
            go.SetActive(true);
            go.name = i.ToString();
            action?.Invoke(i, go);
        }
        PlayAnimation();
    }
    private void OnEnable()
    {
        PlayAnimation();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private void PlayAnimation()
    {
        InitPosition();
        if (openAnimation && Application.isPlaying)
        {
            StartCoroutine(Animation());
        }
    }
    IEnumerator Animation(int index = 0)
    {
        yield return new WaitForSeconds(spaceTime);
        float start = index * _width + distanceFromLeft;
        float end = (index + 1) * _width + distanceFromLeft;
        yield return StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
         {
             Layout.padding.left = (int)Mathf.Lerp(start, end, t);
         }));
        index++;
        index = index == count ? 0 : index;
        StartCoroutine(Animation(index));
    }

    private void InitPosition()
    {
        Layout.padding.left = distanceFromLeft;
    }

    public void SetGraph(int num,Action<int,GameObject> action)
    {
        count = num;
        SetGraph(action);
    }
}
