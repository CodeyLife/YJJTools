using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace YJJTool
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class Yjj_3dBarDrawer : Graphic
    {
        [System.Serializable]
        public class Bar3DSet
        {
            public float width = 10;
            public List<Color> colors = new List<Color>();
            public float rotation = 30;
            public Material mat;
        }
        public Bar3DSet set;
        private List<List<Vector2>> datas;
        private Vector2 data;
        private int index;
        public void SetGraph(Bar3DSet set, List<List<Vector2>> datas)
        {
            this.set = set;
            this.datas = datas;
            SetAllDirty();
        }
        public void SetGraph(Bar3DSet set, Vector2 data, int index)
        {
            this.index = index;
            this.set = set;
            this.data = data;
            SetAllDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            vh.Clear();
            //if(datas == null)
            //{
            //    return;
            //}
            //for (int i = 0; i < datas.Count; i++)
            //{
            //    for (int j = 0; j < datas[i].Count; j++)
            //    {
            //        var data = datas[i][j];
            //         var mar = Matrix4x4.Rotate(Quaternion.AngleAxis(set.rotation,Vector3.up));
            //        Yjj_ChartUtility.DrawBar(vh, new Vector2(data.x, 0), set.width, data.y, set.colors[i], mar);
            //    }
            //}
            Yjj_ChartUtility.DrawBar(vh, Vector2.zero, set.width, data.y, set.colors[index]);
            var mesh = MeshUtility.GenerateMesh(null, vh);
            MeshUtility.ReadMesh2VH(mesh, vh, set.colors[index]);
        }
    }
}