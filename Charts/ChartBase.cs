using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YJJTool;

public class ChartBase : MonoBehaviour
{
    [Header("没有读取数据时awake是否播放动画"),FoldoutGroup("基础设置")]
    public bool setWithoutSetData = true;
    protected bool isInit = false;

    public virtual void Awake()
    {
        if (setWithoutSetData)
        {
            SetGraph();
        }
    }
    public virtual void OnEnable()
    {
        if (!isInit) return;
        PlayAnimation();
    }
#if UNITY_EDITOR
    [OnInspectorInit]
    private void Init()
    {
        UnPackPrefab();
        UnityEditor.EditorApplication.update += ChangeSize;
        if(transform.parent == null)
        {
            return;
        }
        if (Application.isPlaying)
        {
            return;
        }
        SetGraph();
;
    }
#endif
    [OnInspectorDispose]
    private void OnDispose()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= ChangeSize;
#endif
    }

    public virtual void SetGraph()
    {
        isInit = true;

        StopAllCoroutines();
    }
    public virtual void SetData(List<float> data,List<string> Names)
    {
        StopAllCoroutines();
    }
    public virtual void SetData(List<MultipleData> data, List<string> Names)
    {
        StopAllCoroutines();
    }
    public virtual void SetGraph(bool playAnimation) 
    {
        SetGraph();
        if (gameObject.activeInHierarchy)
        {
            PlayAnimation();
        }
    }
    [Button("播放动画"),ShowIf("@UnityEngine.Application.isPlaying")]
    public virtual void PlayAnimation() { }
#if UNITY_EDITOR
    [OnInspectorGUI]
    public virtual void OnGuiChanged()
    {
        if (GUI.changed)
        {
            this.Delay(() => SetGraph());
        }
    }
#endif
    /// <summary>
    /// 检查名字和数据
    /// </summary>
    /// <param name="dataset"></param>
    /// <param name="floatDatas"></param>
    /// <param name="datas"></param>
    /// <returns></returns>
    protected bool CheckData(DataSet dataset,List<float> floatDatas = null, List<MultipleData> datas = null)
    {
        var count = datas == null ? floatDatas.Count : datas.Count;
        if (count < 1)
        {
            return false;
        }
        else
        {
            int delat;
            if (floatDatas != null)
            {
                delat =   floatDatas.Count - dataset.names.Count;
            }
            else
            {
                delat = datas[0].datas.Count - dataset.names.Count;
            }
            for(int i = 0;i< delat; i++)
            {
                dataset.names.Add("defaltName");
            }
            return true;
        }
    }
    /// <summary>
    /// 在Editor下自动解除预制体
    /// </summary>
    protected void UnPackPrefab()
    {
#if UNITY_EDITOR
        if (UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
        {
            UnityEditor.PrefabUtility.UnpackPrefabInstance(gameObject, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
        }
#endif
    }
    #region 设置最大最小值
    /// <summary>
    /// 设置单组数据最大最小值
    /// </summary>
    /// <param name="set"></param>
    /// <param name="data"></param>
    protected void SetMaxAndMin(BaseSet set,List<float> data)
    {
        for(int i = 0; i < set.rulerSet.Count; i++)
        {
            if (set.rulerSet[i].autoSetMinValue)
            {
                var arr = Yjj_ChartUtility.GetMaxAndMinData(data);
                set.rulerSet[i].min = arr[0];
                set.rulerSet[i].SetMaxValue(arr[1], set);
            }
            else
            {
                set.rulerSet[i].SetMaxValue(Yjj_ChartUtility.GetMaxData(data), set);
            }
        }
    }
    /// <summary>
    /// 如果只有一个标尺，根据所有数据最大最小值设置，如果有多个标尺，根据标尺对应的数据位置设置最大最小值
    /// </summary>
    /// <param name="set"></param>
    /// <param name="data"></param>
    protected void SetMaxAndMinForTwoData(BaseSet set, List<MultipleData> data)
    {
        int count = set.rulerSet.Count;
        if (count > 1)
        {
            for (int i = 0; i < count; i++)
            {
                if (set.rulerSet[i].autoSetMinValue)
                {
                    var arr = Yjj_ChartUtility.GetMaxAndMinData(data[i].datas);
                    set.rulerSet[i].min = arr[0];
                    set.rulerSet[i].SetMaxValue(arr[1], set);
                }
                else
                {
                    set.rulerSet[i].SetMaxValue(Yjj_ChartUtility.GetMaxData(data[i].datas), set);
                    if(set.rulerSet[i].max == 0)
                    {
                        set.rulerSet[i].SetMaxValue(set.rulerSet[i].zero2Max, set);
                    }
                }
            }
        }else if(count > 0)
        {
            int min = 0;
            int max = 0;
            if (set.rulerSet[0].autoSetMinValue)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    var arr = Yjj_ChartUtility.GetMaxAndMinData(data[i].datas);
                    min = arr[0] < min ? arr[0] : min;
                    max = arr[1] > max ? arr[1] : max;
                }
                set.rulerSet[0].min = min;
                set.rulerSet[0].SetMaxValue(max, set);
            }
            else
            {
                for (int i = 0; i < data.Count; i++)
                {
                    var temMax = Yjj_ChartUtility.GetMaxData(data[i].datas);
                    max = temMax > max ? temMax : max;
                }
                set.rulerSet[0].SetMaxValue(max, set);
                if (set.rulerSet[0].max == 0)
                {
                    set.rulerSet[0].SetMaxValue(set.rulerSet[0].zero2Max, set);
                }
            }
        }

    }
    #endregion
    protected virtual void FadeList(float t, ref List<List<Vector2>> temp, List<List<Vector2>> target)
    {
        for (int i = 0; i < temp.Count; i++)
        {
            for (int j = 0; j < target[i].Count; j++)
            {
                if (temp[i].Count <= j)
                {
                    temp[i].Add(new Vector2(target[i][j].x, Mathf.Lerp(0, target[i][j].y, t)));
                }
                else
                {
                    temp[i][j] = new Vector2(target[i][j].x, Mathf.Lerp(0, target[i][j].y, t));
                }
            }
        }
    }
    protected  void ChangeSize()
    {
        if (this == null) return;
        var type = this.GetType();
        var set = type.GetField("set");
        if (set != null)
        {
            BaseSet baseSet = set.GetValue(this) as BaseSet;
            var rect = transform.GetComponent<RectTransform>();
           
            float x = rect.sizeDelta.x;
            float y = rect.sizeDelta.y;
            if (x != baseSet.width || y != baseSet.hight)
            {
                baseSet.width = x;
                baseSet.hight = y;
                SetGraph();
            }
        }
    }
    /// <summary>
    /// 删除所有子物体
    /// </summary>
    /// <param name="t"></param>
    public static void DeleteAllChild(Transform t)
    {
        while (t.childCount > 0)
        {
            DestroyImmediate(t.GetChild(0).gameObject);
        }
    }

    public static void ParseDataByStr(List<string> timeStr,BaseSet set,DataSet dataSet)
    {
        var times = timeStr.Select(x => System.DateTime.Parse(x)).ToArray();
        ParseDataByTime(times, set, dataSet);
    }
    public static void ParseDataByTime(IList<System.DateTime> times,BaseSet set, DataSet dataset)
    {

    }

}
