using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

[ComponentDesc("波形图")]
public class ChartV2Component_CurveChart : ChartV2ComponetBaseWithoutGraphic
{
    public bool useAllData = true;
    [HideIf("useAllData")]
    public List<int> DataIndex = new List<int> { 0 };
    public Material material;
    public bool isCurve = false;
    [ShowIf("isCurve"), Range(3, 9)]
    public int smooth = 6;
    public bool withTime = false;
    public float lineWidth = 1;
    public Color topColor = Color.blue;
    public Color buttomColor = Color.white;
    [Range(0, 1)]
    public float alpha = 0.7f;

    public Material lineMat;

    private float animationPos = 1;
    private List<List<Vector2>> postions = new List<List<Vector2>>();
    private List<List<Vector2>> animationPosList = new List<List<Vector2>>();

#if UNITY_EDITOR
    public override void OnCreat()
    {
        base.OnCreat();
        var mat = UnityEditor.AssetDatabase.FindAssets("UV_x透明抗锯齿")[0];
        lineMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(mat));
        mat = UnityEditor.AssetDatabase.FindAssets("t:material V2Curve")[0];
        material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(mat));
    }
#endif
    public override void InitGraph(ChartV2Base chart)
    {
        base.InitGraph(chart);
        postions.Clear();
        var canDrag = _v2Base.ComputeDataPos(withTime);
        if (Application.isPlaying)
        {
            _v2Base.InitAnimationEvent.AddListener(PlayAnimation);
            if (canDrag)
            {
                _v2Base.OnDragEvent.AddListener(OnDrag);
            }
            else
            {
                _v2Base.OnDragEvent.RemoveListener(OnDrag);
            }
        }

        int start = 0;
        int end = _v2Base.names.Count;
        //_v2Base.GetDragDataIndex(ref start, ref end);
        var h = _v2Base.height - _v2Base.set.distanceFromButtom - _v2Base.set.distanceFromTop;

        if (useAllData)
        {
            for (int i = 0; i < _v2Base.datas.Count; i++)
            {
                var multiplData = _v2Base.datas[i];
                ProcessData(i, multiplData);
            }
        }
        else
        {
            for (int i = 0; i < DataIndex.Count; i++)
            {
                ProcessData(i, _v2Base.datas[DataIndex[i]]);
            }
        }
        while (postions.Count > _v2Base.datas.Count)
        {
            postions.RemoveAt(postions.Count - 1);
        }
        animationPosList.Clear();
        for (int i = 0; i < postions.Count; i++)
        {
            animationPosList.Add(new List<Vector2>(postions[i].Count));
        }

        //offset = chart.XOffset;
        ComputeAnimationData();

        void ProcessData(int i, MultipleData data)
        {
            if (postions.Count <= i)
            {
                postions.Add(new List<Vector2>());
            }
            var arr = postions[i];
            arr.Clear();
            //    bool canBreak = false; //当数据超过mask 下一个数据就会中断绘制
            for (int j = start; j < data.datas.Count; j++)
            {
                if (j > end) break;
                var y = YjjUtility.SmoothLerp(_v2Base.min, _v2Base.max, data.datas[j]) * h + _v2Base.set.distanceFromButtom;

                var x = _v2Base.DataPositionInX(j);
                arr.Add(new Vector2(x/* - _v2Base.XOffset*/, y));
            }
            if (isCurve)
            {
                var count = smooth * arr.Count;
                postions[i] = Yjj_ChartUtility.GetCurvePosFroJob(arr, count, true, true);
            }
        }
    }
    private void PlayAnimation(float arg0)
    {
        animationPos = arg0;
        ComputeAnimationData();
    }
    public override void SetGraph()
    {
        base.SetGraph();
        ComputeAnimationData();
    }
    private void OnDrag(float arg0)
    {
        ComputeAnimationData();
    }

    private void ComputeAnimationData()
    {

        //UnityEngine.Profiling.Profiler.BeginSample("chartdebug");
        var offset = new Vector2(_v2Base.XOffset, 0);
        var colors = _v2Base.set.colors;
        for (int i = 0; i < postions.Count; i++)
        {
            Color color = Color.white;
            if (useAllData)
            {
                color = colors.Count > i ? colors[i] : Color.white;
            }
            else
            {
                var colorIndex = DataIndex[i];
                color = colors.Count > colorIndex ? colors[colorIndex] : Color.white;
            }
            //Yjj_ChartUtility.DrawLineSmooth(vh, postions[i], lineWidth, color);continue;
            var arr = postions[i];
            var length = _v2Base.width /** animationPos*/;
            var animations = animationPosList[i];
            animations.Clear();
            bool addBefor = false;//插值小于0的数据
            for (int z = 0; z < arr.Count; z++)
            {
                var data = arr[z] - offset;
                if (data.x < 0)
                {
                    addBefor = true;
                    continue;
                }
                //animations.Add(data);
                if (data.x > length)
                {

                    //计算超出动画的长度
                    //插值
                    var t = YjjUtility.SmoothLerp(arr[z - 1].x - _v2Base.XOffset, data.x, length);
                    //Debug.Log($"{z}:{arr[z-1].x}到{arr[z].x},长度{length}");
                    var pos = (Vector2.Lerp(arr[z - 1], arr[z], t) - offset);
                    animations.Add(new Vector2(pos.x, pos.y * animationPos));
                    break;
                }
                else
                {
                    //判断需不需要插值前面的数据
                    if (addBefor)
                    {
                        addBefor = false;
                        var lastPos = arr[z - 1] - offset;
                        var lastT = YjjUtility.SmoothLerp(lastPos.x, data.x, 0);
                        var lerpPos = Vector2.Lerp(lastPos, data, lastT);
                        animations.Add(new Vector2(lerpPos.x, lerpPos.y * animationPos));

                    }
                    animations.Add(new Vector2(data.x, data.y * animationPos));
                }

            }

            //画波形
            var curve = transform.GetOrCreatUIChild<V2Curve>($"curve_{i}", (t) =>
            {
                t.rectTransform.anchorMin = Vector2.zero;
                t.rectTransform.anchorMax = Vector2.zero;
                t.rectTransform.pivot = Vector2.zero;
                t.rectTransform.anchoredPosition = Vector2.zero;
                t.material = material;
            });

            curve.material.SetColor("_buttomColor", buttomColor);
            curve.material.SetColor("_topColor", topColor);
            curve.material.SetFloat("_alpha", alpha);
            curve.Draw(color, animations, _v2Base.height);

            //画线
            var line = transform.GetOrCreatUIChild<V2Line>(i.ToString(), CreatNewAction: (t) =>
            {
                t.rectTransform.anchorMin = Vector2.zero;
                t.rectTransform.anchorMax = Vector2.zero;
                t.rectTransform.pivot = Vector2.zero;
                t.rectTransform.anchoredPosition = Vector2.zero;
                t.material = lineMat;
            });
            line.Draw(color, lineWidth, animations);

        }
        var index = postions.Count;
        var delateLine = transform.Find(index.ToString());
        while (delateLine != null)
        {
            DestroyImmediate(delateLine.gameObject);
            DestroyImmediate(transform.Find($"curve_{index}").gameObject);
            index++;
            delateLine = transform.Find(index.ToString());
        }
    }


}
