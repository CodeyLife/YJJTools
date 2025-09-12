using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace YJJTool
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class V2Line : MaskableGraphic
    {
        public float width = 1;
        public List<Vector2> list;
        public void Draw(Color c, float width, List<Vector2> pos)
        {
            color = c;
            this.width = width;
            list = pos;
            raycastTarget = false;
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            vh.Clear();
            Yjj_ChartUtility.DrawLineSmooth(vh, list, width, color);
        }
    }
}