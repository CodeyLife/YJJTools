using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YJJTool
{

    [RequireComponent(typeof(CanvasRenderer))]
    public class GridDrawer : Graphic
    {
        [HideInInspector]
        public Yjj_GridGraph grid;

        private RectTransform _rect;
        public RectTransform Rect
        {
            get
            {
                if (_rect == null)
                {
                    _rect = transform.parent.rectTransform();
                }
                return _rect;
            }
            set => _rect = value;
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            vh.Clear();
            //垂直轴
            int verticalCount = grid.datas.Count;
            //水平轴
            int horizontalCount = grid.datas[0].datas.Count;
            float height = Rect.sizeDelta.y / (verticalCount);
            float length = Rect.sizeDelta.x / (horizontalCount);
            for (int i = 0; i < verticalCount; i++)
            {
                for (int j = 0; j < horizontalCount; j++)
                {
                    Vector2 d = new Vector2(j * length, i * height);
                    var b = d + new Vector2(length, height);
                    var a = new Vector2(b.x - length, b.y);
                    var c = new Vector2(d.x + length, d.y);
                    a += new Vector2(grid.space, -grid.space);
                    b += new Vector2(-grid.space, -grid.space);
                    c += new Vector2(-grid.space, +grid.space);
                    d += new Vector2(grid.space, +grid.space);
                    Color color;
                    var value = grid.datas[i].datas[j];
                    if (value <= grid.values[0])
                    {
                        color = grid.colors[0];
                    }
                    else if (value >= grid.values[grid.values.Count - 1])
                    {
                        color = grid.colors[grid.values.Count - 1];
                    }
                    else
                    {
                        int index = 1;
                        while (index < grid.colors.Count - 1 && value > grid.values[index])
                        {
                            index++;
                        }
                        color = Color.Lerp(grid.colors[index - 1], grid.colors[index], YjjUtility.SmoothLerp(grid.values[index - 1], grid.values[index], value));
                    }

                    Yjj_ChartUtility.DrawRoundQuad(vh, a, b, c, d, grid.radiu, color);
                }
            }
        }
    }
}