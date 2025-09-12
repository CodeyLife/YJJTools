using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SingleBarHeatMap : MonoBehaviour
{
    [LabelText("bar预制体"),Required]
    public GameObject prefab;
    public float maxHeight = 10;
    public float barScale = 1;
    public List<Vector3> pos = new List<Vector3>();
    public List<float> datas = new List<float>();
    public float offset;
    public float animationTime = 2;
    float? _max = null;

    public float Max { get
        {
            if(_max == null)
            {
                _max = datas.Max();
            }
            return _max.Value;
        }
        set => _max = value; }
    #region Inspecotr
    [OnInspectorGUI]
    private void GuiSet()
    {
        if (GUI.changed)
        {
            if (!gameObject.activeInHierarchy) return;
            StartCoroutine(YjjUtility.DeLay(() =>
            {
                SetGraph();
            }));
        }
    }
    [Button("随机生产数据")]
    private void RandomData(int count = 100)
    {
        pos.Clear();
        datas.Clear();

        for (int i = 0; i < count; i++)
        {
            pos.Add(new Vector3(RandomValue(), 0, RandomValue()));
            datas.Add(RandomValue(0));
        }
        SetGraph();
    }
    private float RandomValue(float min = -100, float max = 100)
    {
        return Random.Range(min, max);
    }
    #endregion
    public void SetData(List<Vector3> data,List<float> values,float? offset = null)
    {
        this.pos = data;
        if (offset != null)
        {
            this.offset = offset.Value;
        }
        this.datas = values;
        SetGraph();
    }
    private void SetGraph()
    {
        if(datas.Count == 0)
        {
            ChartBase.DeleteAllChild(transform);
            return;
        }
        var childCount = transform.childCount;
        Vector3 offset = new Vector3(0, this.offset, 0);
        Max = datas.Max();
        for (int i = 0; i < pos.Count; i++)
        {
            var p = pos[i] + offset;
            var scal = new Vector3(barScale, Mathf.Lerp(0, maxHeight, datas[i] / Max), barScale);
            if (i < childCount)
            {
                var c = transform.GetChild(i);
                c.position = p;
                c.localScale =scal;
            }
            else
            {
                var go = Instantiate(prefab, this.transform);
                go.transform.position = p;
                go.transform.localScale = scal;
            }
        }
        while (transform.childCount > pos.Count)
        {
            DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
        }
        PlayAnimation();
    }
    private void OnEnable()
    {
        PlayAnimation();
    }
    public void PlayAnimation()
    {
        if (!gameObject.activeInHierarchy || datas.Count == 0) return;
        StopAllCoroutines();
        var count = datas.Count;
        float[] targets = datas.Select(x => x / Max * maxHeight).ToArray();
        StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
        {
            for (int i = 0; i < count; i++)
            {
                var trans = transform.GetChild(i);
                trans.localScale = new Vector3(barScale, Mathf.Lerp(0, targets[i], t), barScale);
            }
        }));
    }

}
