using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace YJJTool
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class Yjj_SpecailEffect : Graphic
    {
        public List<float> datas;
        public float sizeScale = 0.2f; //缩放
        public float maxFill = 0.8f; // 最大fill
        public float lineWidth = 2; //线宽
        public int smooth = 24;
        public float radius = 500f;

        #region 
        [OnInspectorGUI]
        void OnGuiChange()
        {
            if (GUI.changed)
            {
                this.Delay(() => SetGraph());
            }
        }
        #endregion

        protected override void OnEnable()
        {
            base.OnEnable();
            StartCoroutine(Animation());
        }
        IEnumerator Animation()
        {
            maxFill = 0.2f;
            while (true)
            {
                maxFill += 0.001f;
                SetGraph();
                yield return null;
            }
        }
        public void SetGraph()
        {

            SetAllDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            for (int i = 0; i < datas.Count; i++)
            {
                Compute(vh, datas[i], datas[0], radius * (1 - i * sizeScale));
            }
        }
        private void Compute(VertexHelper vh, float value, float maxValue, float radius)
        {
            float t = value / maxValue * maxFill;
            int count = Mathf.CeilToInt(t * smooth);
            //Debug.Log(string.Format("t:{0},count:{1}", t, count));
            float angle = 360f * t * Mathf.Deg2Rad;
            Vector2[] posArr = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float x = Mathf.Sin(angle * i) * radius;
                float y = Mathf.Cos(angle * i) * radius;
                posArr[i] = new Vector2(x, y);
            }
            Yjj_ChartUtility.DrawLineSmooth(vh, posArr, lineWidth, Color.white);
        }
    }
}