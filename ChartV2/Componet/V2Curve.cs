using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YJJTool
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class V2Curve : MaskableGraphic
    {
        public List<Vector2> list;
        private float height;
        public void Draw(Color c, List<Vector2> pos, float h)
        {
            color = c;
            list = pos;
            height = h;
            raycastTarget = false;
            SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            vh.Clear();
            //每两个点之间画方块
            for (int j = 1; j < list.Count; j++)
            {
                var current = list[j];
                var last = list[j - 1];
                var lastDown = new Vector2(last.x, 0);
                var currentDown = new Vector2(current.x, 0);
                var vlast = Yjj_ChartUtility.GetVertex(last, new Vector2(0.5f, last.y / height), color);
                var vlastDown = Yjj_ChartUtility.GetVertex(lastDown, Vector2.zero, color);
                var vcurrent = Yjj_ChartUtility.GetVertex(current, new Vector2(0.5f, current.y / height), color);
                var vcurrentDown = Yjj_ChartUtility.GetVertex(currentDown, Vector2.zero, color);

                Yjj_ChartUtility.DrawQuad(vh, vlast, vcurrent, vcurrentDown, vlastDown);
            }
        }
    }
}