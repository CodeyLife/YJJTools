using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;

[ComponentDesc("折线")]
public class ChartV2Component_LineChart : ChartV2ComponetBase
{
    public bool useAllData = true;
    [HideIf("useAllData")]
    public List<int> DataIndex = new List<int> { 0 };

    public bool isCurve = false;
    [ShowIf("isCurve"),Range(3,9)]
    public int smooth = 6;
    [ReadOnly,PropertyTooltip("")]
    public bool withTime = false;
    public float lineWidth = 1;

    private float animationPos = 1;
    private List<List<Vector2>> postions = new List<List<Vector2>>();
    private List<List<Vector2>> animationPosList = new List<List<Vector2>>();

#if UNITY_EDITOR
    public override void OnCreat()
    {
        base.OnCreat();
        material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets($"t:material UV_x抗锯齿 清晰")[0]));
    }
#endif

    public override void InitGraph(ChartV2Base chart)
    {
        base.InitGraph(chart);
        var canDrag = _v2Base.ComputeDataPos(withTime);
        if (Application.isPlaying)
        {
            _v2Base.InitAnimationEvent.AddListener(PlayAnimation);
            if (canDrag)
            {
                _v2Base.OnDragEvent.RemoveListener(OnDrag);
                _v2Base.OnDragEvent.AddListener(OnDrag);
            }
            else
            {
                _v2Base.OnDragEvent.RemoveListener(OnDrag);
            }
        }
        SetGraph();
    }

    private void PlayAnimation(float arg0)
    {
        animationPos = arg0;
        SetVerticesDirty();
    }

    public override void SetGraph()
    {
        base.SetGraph();

        int start = 0;
        int end = _v2Base.names.Count;
        postions.Clear();
        if (useAllData)
        {
            for (int i = 0; i < _v2Base.datas.Count; i++)
            {
                var multiplData = _v2Base.datas[i];
                ProcessData(i,i, multiplData);
            }
        }
        else
        {
            for (int i = 0; i < DataIndex.Count; i++)
            {
                var index = DataIndex[i];
                ProcessData(i,index, _v2Base.datas[index]);
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

     
        SetVerticesDirty();

        void ProcessData(int i,int realIndex,MultipleData multiplData)
        {
         
            if (postions.Count <= i)
            {
                postions.Add(new List<Vector2>());
            }
            var arr = postions[i];
            arr.Clear();

            if (multiplData.datas.Count < 2) return;

            for (int j = start; j < multiplData.datas.Count; j++)
            {
                if (j > end) break;

                arr.Add(_v2Base.DataList[realIndex][j]);
            }
            if (isCurve)
            {
                var count = smooth * arr.Count;
                postions[i] = Yjj_ChartUtility.GetCurvePosFroJob(arr, count, true, true);
            }

        }
    }

    private void OnDrag(float arg0)
    {
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        if (_v2Base == null) return;

		var offset = new Vector2(_v2Base.XOffset, 0);
		var colors = _v2Base.set.colors;
		for (int i = 0; i < postions.Count; i++)
		{
			var colorIndex = useAllData ? i : DataIndex[i];
			var color = colors.Count > colorIndex ? colors[colorIndex] : Color.white;

			var arr = postions[i];
			if (arr == null || arr.Count == 0) continue;
			var length = _v2Base.width * animationPos;
			var animations = animationPosList[i];
			animations.Clear();

			// 视窗裁剪范围（基于原始 arr.x）: [XOffset, XOffset + length]
			float viewStart = _v2Base.XOffset;
			float viewEnd = _v2Base.XOffset + length;
			int startIdx = MathUtility. FindFirstIndexGE(arr, viewStart);
			int endIdx = MathUtility.FindLastIndexLE(arr, viewEnd);
			if (endIdx < 0 || startIdx >= arr.Count || startIdx > endIdx)
			{
				continue;
			}
			// 预留前后各1个用于插值，避免断线
			int iterStart = Mathf.Max(startIdx - 1, 0);
			int iterEnd = Mathf.Min(endIdx + 1, arr.Count - 1);
			int expected = iterEnd - iterStart + 1;
			if (animations.Capacity < expected + 2) animations.Capacity = expected + 2;

			bool addedHead = false;
			for (int z = iterStart; z <= iterEnd; z++)
			{
				var original = arr[z];
				var data = original - offset;
				if (z == iterStart && data.x < 0)
				{
					// 在左侧边界内插一个点到 x=0
					if (z + 1 <= iterEnd)
					{
						var next = arr[z + 1] - offset;
						var t0 = YjjUtility.SmoothLerp(data.x, next.x, 0);
						var p0 = Vector2.Lerp(data, next, t0);
						animations.Add(p0);
						addedHead = true;
					}
					continue;
				}
				if (data.x > length && z>0)
				{
					// 在右侧边界内插一个点到 x=length
					if (z - 1 >= iterStart)
					{
						var prev = arr[z - 1] - offset;
						var t1 = YjjUtility.SmoothLerp(prev.x, data.x, length);
						var p1 = Vector2.Lerp(prev, data, t1);
						animations.Add(p1);
					}
					break;
				}
				if (!addedHead && z > 0 && (arr[z - 1].x - _v2Base.XOffset) < 0)
				{
					// 确保第一个可见点前的插值被加入
					var lastPos = arr[z - 1] - offset;
					var t = YjjUtility.SmoothLerp(lastPos.x, data.x, 0);
					animations.Add(Vector2.Lerp(lastPos, data, t));
					addedHead = true;
				}
				animations.Add(data);
			}

			Yjj_ChartUtility.DrawLineSmooth(vh, animations, lineWidth, color);
		}
	}
}
