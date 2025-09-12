using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[ComponentDesc("hover弹窗")]
[ComponentOrder(3000)]
public class ChartV2Component_HoverWindow : ChartV2ComponetBaseWithoutGraphic
{
    #region 参数
    [PropertyTooltip("弹窗的根节点,可以选中该物体改变image的颜色和sprite来更换效果")]
    public RectTransform window;
    [PropertyTooltip("默认自动生成，也可以手动创建自己想要的文本预制体后在这里赋值")]
    [LabelText("文本的预制体")]
    public GameObject prefab;
    [PropertyTooltip("控制是否在hover时显示线条，线条的宽度和颜色可以选中线条物体来改变image的颜色和宽度")]
    public bool showLine = true;
    [ShowIf("showLine")]
    public RectTransform hoverLine;
#if UNITY_EDITOR
    [OnValueChanged("ReBuild")]
#endif
    public float fontSize = 24;
    public Color fontColor = Color.white;
#if UNITY_EDITOR
    [OnValueChanged("ReBuild")]
#endif
    public float space = 20;
    [PropertyTooltip("可以设置富文本,content会动态的替换"),LabelText("标题文本样式")]
    public string titleRichStr = @"<color=red>标题</color>:<color=white>content</color>";
    [PropertyTooltip("与titleRichStr同理,可以设置富文本,content会动态的替换"),LabelText("数据文本样式")]
    public List<string> dataRichList = new List<string>();
    [PropertyTooltip("弹窗根节点与鼠标的偏移量")]
    public Vector2 offset = Vector2.one * 25;
    #endregion
    [HideInInspector]
    /// <summary>
    /// 在hover的dataIndex变更的时候会调用该事件
    /// </summary>
    public UnityEvent<int, Transform> HoverDataEvent = new UnityEvent<int, Transform>();


    private int currentIndex = -1;  //当前hover的dataIndex
    private Vector2 targetPos;
    private void OnEnable()
    {
        currentIndex = -1;
        hoverLine?.gameObject.SetActive(false);
        window.gameObject.SetActive(false);
    }
    private void Update()
    {
        if (window.gameObject.activeInHierarchy)
        {
            window.anchoredPosition = Vector2.Lerp(window.anchoredPosition, targetPos, Time.deltaTime * 10);
        }
    }

#if UNITY_EDITOR
    [Button("重新生成prefab及window",ButtonHeight = 25),GUIColor(0,1,0)]
    protected void ReBuild()
    {
        if (prefab != null)
        {
            DestroyImmediate(prefab.gameObject);
        }
        if (window != null)
        {
            DestroyImmediate(window.gameObject);
        }
        Generate();
    }
    protected void Generate()
    {
        if(window == null)
        {
            var go = new GameObject();
            go.transform.SetParent(transform, false);
            window = go.GetOrAddComponent<RectTransform>();
            window.name = "window";
            var addImg = window.gameObject.AddComponent<Image>();
            //addImg.maskable = false;
            var paths = UnityEditor.AssetDatabase.FindAssets("t:texture 图表弹窗");
            if(paths.Length > 0)
            {
                addImg.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(UnityEditor.AssetDatabase.GUIDToAssetPath(paths[0]));
            }
        }
        transform.SetAsLastSibling();
        var img = window.GetComponent<Image>();
        img.raycastTarget = false;

        var title =  window.GetOrCreatUIChild<TextMeshProUGUI>("title",(t)=>
        {
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            t.rectTransform.anchorMin = new Vector2(0.5f, 1);
            t.rectTransform.anchorMax = new Vector2(0.5f, 1);
            t.rectTransform.pivot = new Vector2(0.5f, 1);
            //t.OnPreRenderText +=ChartV2Base.RenderText;
        });
        if (_v2Base.set.font != null)
        {
            title.font = _v2Base.set.font;
        }
        title.text = titleRichStr;
        title.color = fontColor;
        title.fontSize = fontSize;
        title.maskable = true;
        //title.maskable = false;
        title.ForceMeshUpdate();
        title.rectTransform.sizeDelta = new Vector2(title.preferredWidth, title.preferredHeight);
        title.rectTransform.anchoredPosition = new Vector2(0, -space);
        GenerateDataText(0);
        if (showLine)
        {
            hoverLine = transform.rectTransform().GetOrCreatUIChild<Image>("hoverLine", (t) =>
             {
                 t.rectTransform.anchorMin = Vector2.zero;
                 t.rectTransform.anchorMax = Vector2.zero;
                 t.rectTransform.pivot = new Vector2(0.5f, 0);
                 t.rectTransform.anchoredPosition = new Vector2(100,0);
                 t.rectTransform.sizeDelta = new Vector2(1.5f, _v2Base.height);
                 t.raycastTarget = false;
             }).rectTransform;
            hoverLine.sizeDelta = new Vector2(hoverLine.sizeDelta.x, _v2Base.height);
        }
        else
        {
            if (hoverLine != null)
            {
                DestroyImmediate(hoverLine.gameObject);
            }
        }
    }

  
#endif
    public override void InitGraph(ChartV2Base chart)
    {
        base.InitGraph(chart);
        if (Application.isPlaying)
        {
            _v2Base.OnHoverEvent.AddListener(Hover);
            _v2Base.OnPointerEnterEvent.AddListener(Enter);
            _v2Base.OnPointerExitEvent.AddListener(Exit);
        }
#if UNITY_EDITOR
        Generate();
#endif
    }

    private void Exit()
    {
        currentIndex = -1;
        window.gameObject.SetActive(false);
        hoverLine?.gameObject.SetActive(false);
    }

    private void Enter()
    {
   
        window.gameObject.SetActive(true);
        hoverLine?.gameObject.SetActive(true);
    }

    private void Hover(Vector2 arg0)
    {
        int dataIndex = _v2Base.HoverDataIndex;
        
        // 获取最近数据点的x轴位置
        float dataXPosition = GetDataXPosition(dataIndex);
        
        // 将鼠标y坐标转换为图表坐标系
        Vector2 mousePos = arg0 + new Vector2(_v2Base.width * 0.5f, _v2Base.height * 0.5f);
        
        // 使用数据点的x位置，但保持鼠标的y位置
        Vector2 snapPos = new Vector2(dataXPosition, mousePos.y);
        
        // 计算弹窗位置（基于数据点位置而不是鼠标位置）
        var x = snapPos.x > _v2Base.width * 0.5f ? 1 : 0;
        var y = snapPos.y > _v2Base.height * 0.5f ? 1 : 0;
        var pivot = new Vector2(x, y);
        window.anchorMin = Vector2.zero;
        window.anchorMax = Vector2.zero;
        window.pivot = pivot;
        targetPos = snapPos + new Vector2(x == 1 ? -offset.x : offset.x, y == 1 ? -offset.y : offset.y);

        var title = window.Find("title").GetComponent<TextMeshProUGUI>();
        title.text = titleRichStr.Replace("content", _v2Base.names[dataIndex]);
        title.rectTransform.sizeDelta = new Vector2(title.preferredWidth, title.preferredHeight);
     
        GenerateDataText(dataIndex);
        if (hoverLine != null)
        {
            hoverLine.anchoredPosition = new Vector2(dataXPosition, 0);
        }

        if (dataIndex != currentIndex)
        {
            HoverDataEvent?.Invoke(dataIndex, window);
            currentIndex = dataIndex;
        }
    }
    
    /// <summary>
    /// 获取指定数据索引的x轴位置
    /// </summary>
    /// <param name="dataIndex">数据索引</param>
    /// <returns>数据点的x轴位置</returns>
    private float GetDataXPosition(int dataIndex)
    {
        if (dataIndex < 0 || dataIndex >= _v2Base.XList.Count)
        {
            return 0f;
        }
        
        // 从ChartV2Base获取数据点的x位置，并减去拖拽偏移量
        float dataX = _v2Base.XList[dataIndex] - _v2Base.XOffset;
        
        // 确保位置在图表范围内
        dataX = Mathf.Clamp(dataX, 0, _v2Base.width);
        
        return dataX;
    }

    private void GenerateDataText(int index)
    {
        //生成prefab
        var datas = _v2Base.datas;
        if (prefab == null)
        {
            var go = new GameObject("defulatPrefab");
            go.transform.SetParent(transform, false);

            var gorect = go.GetOrAddComponent<RectTransform>();
            //生成image
            var grid = gorect.GetOrCreatUIChild<Image>("image", (t) =>
             {
                 t.rectTransform.sizeDelta = Vector2.one * space;
                 t.rectTransform.anchorMin = new Vector2(0, 0.5f);
                 t.rectTransform.anchorMax = new Vector2(0, 0.5f);
                 t.rectTransform.pivot = new Vector2(0, 0.5f);
                 t.rectTransform.anchoredPosition = new Vector2(space, 0);
             });
            var text = gorect.GetOrCreatUIChild<TextMeshProUGUI>("value");
            text.rectTransform.anchorMin = new Vector2(0, 0.5f);
            text.rectTransform.anchorMax = new Vector2(0, 0.5f);
            text.rectTransform.pivot = new Vector2(0, 0.5f);
            text.rectTransform.anchoredPosition = new Vector2(space *3,0);
            text.raycastTarget = false;
            text.maskable = true;
            //var text = go.AddComponent<TextMeshProUGUI>();
            text.enableWordWrapping = false;
            if (_v2Base.set.font != null)
            {
                text.font = _v2Base.set.font;
            }
            text.color = fontColor;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            prefab = go;
            text.text = "defaultDataText";
            text.ForceMeshUpdate();
            text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight+10);
            gorect.anchorMin = new Vector2(0, 1);
            gorect.anchorMax = new Vector2(0, 1);
            gorect.pivot = new Vector2(0, 1);
            gorect.sizeDelta = new Vector2(150, Mathf.Max(text.preferredHeight,space));
            go.gameObject.SetActive(false);
        }
        var tittelRect = window.Find("title").GetComponent<TextMeshProUGUI>() ;
        //tittelRect.OnPreRenderText += ChartV2Base.RenderText;
        float titleHeight = tittelRect.rectTransform.sizeDelta.y == 0? space : tittelRect.rectTransform.sizeDelta.y + space * 2;
        var prefabRect = prefab.transform.rectTransform();
        float maxWidth = 0;
        var titleWidth = tittelRect.preferredWidth + space * 2;
        maxWidth = maxWidth > titleWidth ? maxWidth : titleWidth;

        for (int i = 0; i < datas.Count; i++)
        {
            var go = window.GetOrCreatChild(i.ToString(), prefab);
            var image = go.transform.Find("image").GetComponent<Image>();
            image.color = _v2Base.set.colors.Count>i ? _v2Base.set.colors[i]:Color.white;
            //image.maskable = /*false*/;
            var valueText = go.Find("value").GetComponent<TextMeshProUGUI>();
            var list = datas[i].datas;
            if (list.Count > index)
            {
                go.gameObject.SetActive(true);
                if (dataRichList.Count > i)
                {
                    valueText.text = dataRichList[i].Replace("content", list[index].ToString() + _v2Base.Unit);
                }
                else
                {
                    valueText.text = list[index].ToString() + _v2Base.Unit ; 
                }
                if (_v2Base.set.font != null)
                {
                    valueText.font = _v2Base.set.font;
                }
                //valueText.OnPreRenderText += ChartV2Base.RenderText;
                valueText.ForceMeshUpdate();
                valueText.rectTransform.sizeDelta = new Vector2(valueText.preferredWidth, valueText.rectTransform.sizeDelta.y);
                var width = valueText.rectTransform.anchoredPosition.x + valueText.preferredWidth + space;
                var height =  - i * (prefabRect.sizeDelta.y+space) - titleHeight;
                go.rectTransform().anchoredPosition = new Vector2(0, height);
                
                maxWidth = maxWidth < width ? width : maxWidth;

            }
            else
            {
                go.gameObject.SetActive(false);
            }
        }
        window.sizeDelta = new Vector2(maxWidth /*+ space *2*/, (titleHeight + (datas.Count) * (prefabRect.sizeDelta.y + space)));
        var delateIndex = datas.Count;
        var delateGO = window.Find(delateIndex.ToString());
        while (delateGO != null)
        {
            DestroyImmediate(delateGO.gameObject);
            delateIndex++;
            delateGO = window.Find(delateIndex.ToString());
        }
    }
}
