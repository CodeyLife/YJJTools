using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace YJJTool
{

    [RequireComponent(typeof(CanvasRenderer))]
    public class Yjj_MultistageChart : Graphic
    {
        [ListDrawerSettings(AddCopiesLastElement = true)]
        public List<Color> colors = new List<Color>();
        public List<MultipleData> datas = new List<MultipleData>();
        //  public List<Yjj_MultistageData> datas = new List<Yjj_MultistageData>();
        public float line_width = 3;
        [ReadOnly]
        public float width = 100;
        [LabelText("上下间距")]
        public float distance = 20;
        [LabelText("渐入动画时间")]
        public float animation_time = 1;

#if UNITY_EDITOR
        #region Inspector
        [OnInspectorInit]
        private void OnInit()
        {
            rectTransform.pivot = new Vector2(0, 0.5f);
            width = rectTransform.sizeDelta.x;
            UnityEditor.EditorApplication.update += UpdateWidth;
        }

        private void UpdateWidth()
        {
            if (width != rectTransform.sizeDelta.x)
            {
                width = rectTransform.sizeDelta.x;
                SetAllDirty();
            }
        }
        [OnInspectorDispose]
        private void Dis()
        {
            UnityEditor.EditorApplication.update -= UpdateWidth;
        }

        #endregion
#endif
        public void SetData(List<MultipleData> list)
        {
            datas = list;
            SetVerticesDirty();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (Application.isPlaying)
            {
                PlayAnimation();
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Compute(vh);
        }
        private void Compute(VertexHelper vh)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                Vector3 startPos = new Vector3(0, (i - ((datas.Count - 1) * 0.5f)) * distance, 0);
                ComputeData(vh, datas[i].datas, startPos);
            }
        }
        private void ComputeData(VertexHelper vh, List<float> data, Vector3 startPos)
        {
            float max = data.Sum();
            Vector3 endPos = startPos;
            for (int i = 0; i < data.Count; i++)
            {
                float length = width * data[i] / max;
                Vector3 tempPos = endPos + new Vector3(length, 0, 0);
                Yjj_ChartUtility.DrawLineSmooth(vh, endPos, tempPos, line_width, colors[i]);
                endPos = tempPos;
            }

            Yjj_ChartUtility.DrawCircle(vh, startPos, line_width, colors[0]);
            Yjj_ChartUtility.DrawCircle(vh, startPos + Vector3.right * width, line_width, colors[data.Count - 1]);
        }
        float temp;
        public void PlayAnimation()
        {
            temp = width;
            StartCoroutine(YjjUtility.FadeIn(animation_time, (t) =>
             {
                 width = temp * t;
                 SetVerticesDirty();
             }));
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            width = temp == 0 ? width : temp;
        }
    }
}