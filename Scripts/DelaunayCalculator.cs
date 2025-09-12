using System;
using System.Collections.Generic;
using UnityEngine;

namespace YJJTool
{
    public class DelaunayCalculator
    {
        /// <summary>
        /// 用于计算的点位
        /// </summary>
        public IList<Vector2> postions;
        /// <summary>
        /// 最终生成的三角形顶点索引
        /// </summary>
        public List<int> triangles;
        int highest = -1;
        List<TriangleNode> tempTriangles;
        List<int> indices;

        public DelaunayCalculator()
        {
            tempTriangles = new List<TriangleNode>();
            indices = new List<int>();
            postions = new List<Vector2>();
            triangles = new List<int>();
        }


        /// <summary>
        /// 计算三角面
        /// </summary>
        /// <param name="verts"></param>
        public void CalculateTriangulation(IList<Vector2> verts)
        {

            if (verts.Count < 3)
            {
                throw new ArgumentException("至少需要三个顶点");
            }

            postions = verts;

            highest = 0;

            for (int i = 0; i < verts.Count; i++)
            {
                if (Higher(highest, i))
                {
                    highest = i;
                }
            }

            // 添加第一个三角形, the bounding triangle.
            tempTriangles.Add(new TriangleNode(-2, -1, highest));

            RunBowyerWatson();
            GenerateResult();

        }

        //BowyerWatson算法
        void RunBowyerWatson()
        {
            //遍历每个顶点，找到包含它的三角形，拆分到树里
            for (int i = 0; i < postions.Count; i++)
            {
                var pi = i;

                if (pi == highest) continue;

                // 包含三角形的索引
                var ti = FindTriangleNode(pi);

                var t = tempTriangles[ti];

                //  三角形的顶点索引
                var p0 = t.P0;
                var p1 = t.P1;
                var p2 = t.P2;

                // 新三角形索引
                var nti0 = tempTriangles.Count;
                var nti1 = nti0 + 1;
                var nti2 = nti0 + 2;

                // 创建新三角形
                var nt0 = new TriangleNode(pi, p0, p1);
                var nt1 = new TriangleNode(pi, p1, p2);
                var nt2 = new TriangleNode(pi, p2, p0);


                // 设置相邻三角形索引. 

                nt0.A0 = t.A2;
                nt1.A0 = t.A0;
                nt2.A0 = t.A1;

                nt0.A1 = nti1;
                nt1.A1 = nti2;
                nt2.A1 = nti0;

                nt0.A2 = nti2;
                nt1.A2 = nti0;
                nt2.A2 = nti1;

                // 设置子节点
                t.C0 = nti0;
                t.C1 = nti1;
                t.C2 = nti2;

                tempTriangles[ti] = t;

                tempTriangles.Add(nt0);
                tempTriangles.Add(nt1);
                tempTriangles.Add(nt2);

                if (nt0.A0 != -1) LegalizeEdge(nti0, nt0.A0, pi, p0, p1);
                if (nt1.A0 != -1) LegalizeEdge(nti1, nt1.A0, pi, p1, p2);
                if (nt2.A0 != -1) LegalizeEdge(nti2, nt2.A0, pi, p2, p0);
            }
        }

        /// <summary>
        /// 筛选最终结果
        /// </summary>
        void GenerateResult()
        {

            triangles.Clear();


            for (int i = 1; i < tempTriangles.Count; i++)
            {
                var t = tempTriangles[i];

                if (t.IsLeaf && t.IsInner)
                {
                    triangles.Add(t.P0);
                    triangles.Add(t.P1);
                    triangles.Add(t.P2);
                }
            }

            tempTriangles = null;
        }

        /// <summary>
        /// 找到三角形[ti]树结构中包含给点边的底层三角形
        ///生成新三角形或翻转三角形时需要更新拥有同样边的三角形
        /// </summary>
        int LeafWithEdge(int ti, int e0, int e1)
        {
            Debug.Assert(tempTriangles[ti].HasEdge(e0, e1));

            while (!tempTriangles[ti].IsLeaf)
            {
                var t = tempTriangles[ti];

                if (t.C0 != -1 && tempTriangles[t.C0].HasEdge(e0, e1))
                {
                    ti = t.C0;
                }
                else if (t.C1 != -1 && tempTriangles[t.C1].HasEdge(e0, e1))
                {
                    ti = t.C1;
                }
                else if (t.C2 != -1 && tempTriangles[t.C2].HasEdge(e0, e1))
                {
                    ti = t.C2;
                }
                else
                {
                    throw new Exception("不应该找不到");
                }
            }

            return ti;
        }

        /// <summary>
        /// 边是否正确，以及是否需要翻转
        /// </summary>
        bool LegalEdge(int k, int l, int i, int j)
        {
            Debug.Assert(k != highest && k >= 0);

            var lMagic = l < 0;
            var iMagic = i < 0;
            var jMagic = j < 0;

            Debug.Assert(!(iMagic && jMagic));

            if (lMagic)
            {
                return true;
            }
            else if (iMagic)
            {
                Debug.Assert(!jMagic);

                var p = postions[l];
                var l0 = postions[k];
                var l1 = postions[j];

                return MeshUtility.ToTheLeft(p, l0, l1);
            }
            else if (jMagic)
            {
                Debug.Assert(!iMagic);

                var p = postions[l];
                var l0 = postions[k];
                var l1 = postions[i];

                return !MeshUtility.ToTheLeft(p, l0, l1);
            }
            else
            {
                Debug.Assert(k >= 0 && l >= 0 && i >= 0 && j >= 0);

                var p = postions[l];
                var c0 = postions[k];
                var c1 = postions[i];
                var c2 = postions[j];

                Debug.Assert(MeshUtility.ToTheLeft(c2, c0, c1));
                Debug.Assert(MeshUtility.ToTheLeft(c2, c1, p));

                return !MeshUtility.InsideCircumcircle(p, c0, c1, c2);
            }
        }

        /// <summary>
        /// 检测是否需要翻转边缘并执行
        /// 检查新插入的点生成的两个三角形是否需要翻转边，如果需要翻转生成两个新的三角形并继续检查</summary>
        /// <param name="newTriangle0"></param>
        /// <param name="newTriangle1"></param>
        /// <param name="newPoint"></param>
        /// <param name="edgePoint0"></param>
        /// <param name="edgePoint1"></param>
        void LegalizeEdge(int newTriangle0, int newTriangle1, int newPoint, int edgePoint0, int edgePoint1)
        {
            //三角形0是刚创建的，三角形1可能不是底层节点
            newTriangle1 = LeafWithEdge(newTriangle1, edgePoint0, edgePoint1);

            var t0 = tempTriangles[newTriangle0];
            var t1 = tempTriangles[newTriangle1];
            var qi = t1.OtherPoint(edgePoint0, edgePoint1);

            Debug.Assert(t0.HasEdge(edgePoint0, edgePoint1));
            Debug.Assert(t1.HasEdge(edgePoint0, edgePoint1));
            Debug.Assert(t0.IsLeaf);
            Debug.Assert(t1.IsLeaf);
            Debug.Assert(t0.P0 == newPoint || t0.P1 == newPoint || t0.P2 == newPoint);
            Debug.Assert(t1.P0 == qi || t1.P1 == qi || t1.P2 == qi);

            if (!LegalEdge(newPoint, qi, edgePoint0, edgePoint1))
            {
                var ti2 = tempTriangles.Count;
                var ti3 = ti2 + 1;

                var t2 = new TriangleNode(newPoint, edgePoint0, qi);
                var t3 = new TriangleNode(newPoint, qi, edgePoint1);

                t2.A0 = t1.Opposite(edgePoint1);
                t2.A1 = ti3;
                t2.A2 = t0.Opposite(edgePoint1);

                t3.A0 = t1.Opposite(edgePoint0);
                t3.A1 = t0.Opposite(edgePoint0);
                t3.A2 = ti2;

                tempTriangles.Add(t2);
                tempTriangles.Add(t3);

                var nt0 = tempTriangles[newTriangle0];
                var nt1 = tempTriangles[newTriangle1];

                nt0.C0 = ti2;
                nt0.C1 = ti3;

                nt1.C0 = ti2;
                nt1.C1 = ti3;

                tempTriangles[newTriangle0] = nt0;
                tempTriangles[newTriangle1] = nt1;

                if (t2.A0 != -1) LegalizeEdge(ti2, t2.A0, newPoint, edgePoint0, qi);
                if (t3.A0 != -1) LegalizeEdge(ti3, t3.A0, newPoint, qi, edgePoint1);
            }
        }

        /// <summary>
        /// 查询包含某个点的三角形树结构的底层三角形
        /// </summary>
        int FindTriangleNode(int pi)
        {
            var curr = 0;

            while (!tempTriangles[curr].IsLeaf)
            {
                var t = tempTriangles[curr];

                if (t.C0 >= 0 && PointInTriangle(pi, t.C0))
                {
                    curr = t.C0;
                }
                else if (t.C1 >= 0 && PointInTriangle(pi, t.C1))
                {
                    curr = t.C1;
                }
                else
                {
                    curr = t.C2;
                }
            }

            return curr;
        }

        /// <summary>
        /// 返回该点是否在三角形内
        /// </summary>
        bool PointInTriangle(int positionIndex, int triangleIndex)
        {
            var t = tempTriangles[triangleIndex];
            return ToTheLeft(positionIndex, t.P0, t.P1)
                && ToTheLeft(positionIndex, t.P1, t.P2)
                && ToTheLeft(positionIndex, t.P2, t.P0);
        }

        /// <summary>
        /// 判断点是否在边的左边
        /// </summary>
        bool ToTheLeft(int pi, int li0, int li1)
        {
            if (li0 == -2)
            {
                return Higher(li1, pi);
            }
            else if (li0 == -1)
            {
                return Higher(pi, li1);
            }
            else if (li1 == -2)
            {
                return Higher(pi, li0);
            }
            else if (li1 == -1)
            {
                return Higher(li0, pi);
            }
            else
            {
                Debug.Assert(li0 >= 0);
                Debug.Assert(li1 >= 0);

                return MeshUtility.ToTheLeft(postions[pi], postions[li0], postions[li1]);
            }
        }
        /// <summary>
        /// 如果比较的目标Y轴更小 返回true Y轴相同则比较x轴，-1表示值为空，-2表示最大值
        /// </summary>
        /// <param name="currentHigh"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        bool Higher(int currentHigh, int target)
        {
            if (currentHigh == -2)
            {
                return false;
            }
            else if (currentHigh == -1)
            {
                return true;
            }
            else if (target == -2)
            {
                return true;
            }
            else if (target == -1)
            {
                return false;
            }
            else
            {
                var p0 = postions[currentHigh];
                var p1 = postions[target];

                if (p0.y < p1.y)
                {
                    return true;
                }
                else if (p0.y > p1.y)
                {
                    return false;
                }
                else
                {
                    return p0.x < p1.x;
                }
            }
        }
        struct TriangleNode
        {
            // 三角形顶点
            public int P0;
            public int P1;
            public int P2;

            //子节点 -1表示没有子节点
            public int C0;
            public int C1;
            public int C2;

            // 邻近的三角形
            //A0表示与P0相对的三角形，即该三角形拥有（P1,P2边）
            // -1代表没有邻近三角形 只有边界上的三角形没有邻近三角形
            public int A0;
            public int A1;
            public int A2;

            /// <summary>
            /// 是否是底层节点
            /// </summary>
            public bool IsLeaf
            {
                get
                {
                    return C0 < 0 && C1 < 0 && C2 < 0;
                }
            }

            /// <summary>
            /// 是否是最终需要的三角形（即该三角形点的索引不在给的离散列表里）
            /// </summary>
            public bool IsInner
            {
                get
                {
                    return P0 >= 0 && P1 >= 0 && P2 >= 0;
                }
            }

            public TriangleNode(int P0, int P1, int P2)
            {
                this.P0 = P0;
                this.P1 = P1;
                this.P2 = P2;

                this.C0 = -1;
                this.C1 = -1;
                this.C2 = -1;

                this.A0 = -1;
                this.A1 = -1;
                this.A2 = -1;
            }


            /// <summary>
            /// 判断三角形是否包含某条边（参数为边的两点）
            /// </summary>
            public bool HasEdge(int indexA, int indexB)
            {
                if (indexA == P0)
                {
                    return indexB == P1 || indexB == P2;
                }
                else if (indexA == P1)
                {
                    return indexB == P0 || indexB == P2;
                }
                else if (indexA == P2)
                {
                    return indexB == P0 || indexB == P1;
                }

                return false;
            }


            /// <summary>
            /// 给出三角形的两个点，返回剩下的另一个点
            /// </summary>
            public int OtherPoint(int p0, int p1)
            {
                if (p0 == P0)
                {
                    if (p1 == P1) return P2;
                    if (p1 == P2) return P1;
                    throw new ArgumentException("p0 and p1 not on triangle");
                }
                if (p0 == P1)
                {
                    if (p1 == P0) return P2;
                    if (p1 == P2) return P0;
                    throw new ArgumentException("p0 and p1 not on triangle");
                }
                if (p0 == P2)
                {
                    if (p1 == P0) return P1;
                    if (p1 == P1) return P0;
                    throw new ArgumentException("p0 and p1 not on triangle");
                }

                throw new ArgumentException("p0 and p1 not on triangle");
            }


            /// <summary>
            /// 返回与给定点相对的边的邻近三角形
            /// </summary>
            public int Opposite(int p)
            {
                if (p == P0) return A0;
                if (p == P1) return A1;
                if (p == P2) return A2;
                throw new ArgumentException("该点不在三角形内");
            }
        }

    }
}