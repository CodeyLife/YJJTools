using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using YJJTool;

public class VoronoiCalculator
{
    private DelaunayCalculator calculator;
    /// <summary>
    /// voronoi 的边，根据site分组排序
    /// 顺时针
    /// </summary>
    public readonly List<Edge> Edges;

    /// <summary>
    /// 每个site的第一条边的index
    /// </summary>
    public readonly List<int> FirstEdgeBySite;
    /// <summary>
    /// 点
    /// </summary>
    public readonly IList<Vector2> Verts;
	
	/// <summary>
	/// 三角形外接圆圆心
	/// </summary>
    public readonly List<Vector2> Centers;
	List<PointTriangle> pointTriangles;

	public VoronoiCalculator(DelaunayCalculator c)
    {
        calculator = c;
        Edges = new List<Edge>();
        FirstEdgeBySite = new List<int>();
		Verts = c.postions;
        Centers = new List<Vector2>();
		pointTriangles = new List<PointTriangle>();
		//cmp = new PTComparer();
		Calculate();
	}

	/// <summary>
	/// 计算Vornoi图形
	/// </summary>
	private void Calculate()
	{
		var verts = calculator.postions; //顶点
		var tris = calculator.triangles; // 三角形索引
		var centers = Centers; //三角形质心
		var edges = Edges;//边


		if (tris.Count > pointTriangles.Capacity) { pointTriangles.Capacity = tris.Count; }
		if (tris.Count > edges.Capacity) { edges.Capacity = tris.Count; }


		//计算三角形外接圆圆心
		for (int ti = 0; ti < tris.Count; ti += 3)
		{
			var p0 = verts[tris[ti]];
			var p1 = verts[tris[ti + 1]];
			var p2 = verts[tris[ti + 2]];

			// 检测是否为顺时针
			Debug.Assert(MeshUtility.ToTheLeft(p2, p0, p1));

			centers.Add(MeshUtility.CircumcircleCenter(p0, p1, p2));
		}

		//保存点 及点所在的三角形索引
		for (int ti = 0; ti < tris.Count; ti += 3)
		{
			pointTriangles.Add(new PointTriangle(tris[ti], ti));
			pointTriangles.Add(new PointTriangle(tris[ti + 1], ti));
			pointTriangles.Add(new PointTriangle(tris[ti + 2], ti));
		}

		var cmp = new PTComparer();
		cmp.tris = tris;
		cmp.verts = verts;

		Profiler.BeginSample("Sorting");
		// 点排序
		pointTriangles.Sort(cmp);
		Profiler.EndSample();

		cmp.tris = null;
		cmp.verts = null;

		//遍历点
		for (int i = 0; i < pointTriangles.Count; i++)
		{
			FirstEdgeBySite.Add(edges.Count);

			var start = i;
			var end = -1;

			//从当前的下一个点开始遍历 点
			for (int j = i + 1; j < pointTriangles.Count; j++)
			{
				//如果该点的索引不是比较的索引 中断遍历
				if (pointTriangles[i].Point != pointTriangles[j].Point)
				{
					end = j - 1;
					break;
				}
			}

			if (end == -1)
			{
				end = pointTriangles.Count - 1;
			}

			i = end;

			var count = end - start;

			Debug.Assert(count >= 0);

			for (int ptiCurr = start; ptiCurr <= end; ptiCurr++)
			{
				bool isEdge;

				var ptiNext = ptiCurr + 1;

				if (ptiNext > end) ptiNext = start;

				var ptCurr = pointTriangles[ptiCurr];
				var ptNext = pointTriangles[ptiNext];

				var tiCurr = ptCurr.Triangle;
				var tiNext = ptNext.Triangle;

				var p0 = verts[ptCurr.Point];

				var v2nan = new Vector2(float.NaN, float.NaN);

				if (count == 0)
				{
					isEdge = true;
				}
				else if (count == 1)
				{

					var cCurr = MeshUtility.TriangleCentroid(verts[tris[tiCurr]], verts[tris[tiCurr + 1]], verts[tris[tiCurr + 2]]);
					var cNext = MeshUtility.TriangleCentroid(verts[tris[tiNext]], verts[tris[tiNext + 1]], verts[tris[tiNext + 2]]);

					isEdge = MeshUtility.ToTheLeft(cCurr, p0, cNext);
				}
				else
				{
					isEdge = !SharesEdge(tris, tiCurr, tiNext);
				}

				if (isEdge)
				{
					Vector2 v0, v1;

					if (ptCurr.Point == tris[tiCurr])
					{
						v0 = verts[tris[tiCurr + 2]] - verts[tris[tiCurr + 0]];
					}
					else if (ptCurr.Point == tris[tiCurr + 1])
					{
						v0 = verts[tris[tiCurr + 0]] - verts[tris[tiCurr + 1]];
					}
					else
					{
						Debug.Assert(ptCurr.Point == tris[tiCurr + 2]);
						v0 = verts[tris[tiCurr + 1]] - verts[tris[tiCurr + 2]];
					}

					if (ptNext.Point == tris[tiNext])
					{
						v1 = verts[tris[tiNext + 0]] - verts[tris[tiNext + 1]];
					}
					else if (ptNext.Point == tris[tiNext + 1])
					{
						v1 = verts[tris[tiNext + 1]] - verts[tris[tiNext + 2]];
					}
					else
					{
						Debug.Assert(ptNext.Point == tris[tiNext + 2]);
						v1 = verts[tris[tiNext + 2]] - verts[tris[tiNext + 0]];
					}

					edges.Add(new Edge(
						EdgeType.RayCCW,
						ptCurr.Point,
						tiCurr / 3,
						-1,
					    MeshUtility.RotateRightAngle(v0)
					));

					edges.Add(new Edge(
						EdgeType.RayCW,
						ptCurr.Point,
						tiNext / 3,
						-1,
						MeshUtility.RotateRightAngle(v1)
					));
				}
				else
				{
					if (!AreCoincident(centers[tiCurr / 3], centers[tiNext / 3]))
					{
						edges.Add(new Edge(
							EdgeType.Segment,
							ptCurr.Point,
							tiCurr / 3,
							tiNext / 3,
							v2nan
						));
					}
				}
			}
		}
	}

	List<Vector2> pointsIn = new List<Vector2>();
	List<Vector2> pointsOut = new List<Vector2>();
	/// <summary>
	/// 传入需要裁剪的点 返回裁剪后的点集
	/// </summary>
	/// <param name="polygon"></param>
	/// <param name="site"></param>
	/// <param name="clipped"></param>
	public void ClipSite(IList<Vector2> polygon, int site, ref List<Vector2> clipped)
	{
		pointsIn.Clear();

		pointsIn.AddRange(polygon);

		int firstEdge, lastEdge;

		if (site == Verts.Count - 1)
		{
			firstEdge =FirstEdgeBySite[site];
			lastEdge = Edges.Count - 1;
		}
		else
		{
			firstEdge = FirstEdgeBySite[site];
			lastEdge = FirstEdgeBySite[site + 1] - 1;
		}

		for (int ei = firstEdge; ei <= lastEdge; ei++)
		{
			pointsOut.Clear();

			var edge =Edges[ei];

			Vector2 lp, ld;

			if (edge.Type == EdgeType.RayCCW || edge.Type ==EdgeType.RayCW)
			{
				lp = Centers[edge.Vert0];
				ld = edge.Direction;

				if (edge.Type == EdgeType.RayCW)
				{
					ld *= -1;
				}
			}
			else if (edge.Type == EdgeType.Segment)
			{
				var lp0 = Centers[edge.Vert0];
				var lp1 = Centers[edge.Vert1];

				lp = lp0;
				ld = lp1 - lp0;
			}
			else if (edge.Type == EdgeType.Line)
			{
				throw new NotSupportedException("还没有实现voronoi半平面");
			}
			else
			{
				Debug.Assert(false);
				return;
			}

			for (int pi0 = 0; pi0 < pointsIn.Count; pi0++)
			{
				var pi1 = pi0 == pointsIn.Count - 1 ? 0 : pi0 + 1;

				var p0 = pointsIn[pi0];
				var p1 = pointsIn[pi1];

				var p0Inside = MeshUtility.ToTheLeft(p0, lp, lp + ld);
				var p1Inside = MeshUtility.ToTheLeft(p1, lp, lp + ld);

				if (p0Inside && p1Inside)
				{
					pointsOut.Add(p1);
				}
				else if (!p0Inside && !p1Inside)
				{
					// Do nothing, both are outside
				}
				else
				{
					var intersection = MeshUtility.LineLineIntersection(lp, ld.normalized, p0, (p1 - p0).normalized);

					if (p0Inside)
					{
						pointsOut.Add(intersection);
					}
					else if (p1Inside)
					{
						pointsOut.Add(intersection);
						pointsOut.Add(p1);
					}
					else
					{
						Debug.Assert(false);
					}
				}
			}

			var tmp = pointsIn;
			pointsIn = pointsOut;
			pointsOut = tmp;
		}

		if (clipped == null)
		{
			clipped = new List<Vector2>();
		}
		else
		{
			clipped.Clear();
		}

		clipped.AddRange(pointsIn);
	}
	static bool AreCoincident(Vector2 a, Vector2 b)
	{
		return (a - b).magnitude < 0.000001f;
	}
	/// <summary>
	/// 判断两个三角形是否共边
	/// </summary>
	/// <param name="tris"></param>
	/// <param name="ti0"></param>
	/// <param name="ti1"></param>
	/// <returns></returns>
	static bool SharesEdge(List<int> tris, int ti0, int ti1)
	{
		var x0 = tris[ti0];
		var x1 = tris[ti0 + 1];
		var x2 = tris[ti0 + 2];

		var y0 = tris[ti1];
		var y1 = tris[ti1 + 1];
		var y2 = tris[ti1 + 2];

		var n = 0;

		if (x0 == y0 || x0 == y1 || x0 == y2) n++;
		if (x1 == y0 || x1 == y1 || x1 == y2) n++;
		if (x2 == y0 || x2 == y1 || x2 == y2) n++;

		Debug.Assert(n != 3,"出现了重复三角形,请检查原因");

		return n >= 2;
	}
    #region 结构
    public enum EdgeType
    {
        /// <summary>
        /// 由两点表示无线距离的线
        /// </summary>
        Line,
        /// <summary>
        /// 一个点和方向组成的，逆时针
        /// </summary>
        RayCCW,
        RayCW,
        /// <summary>
        /// 有个点组成的线段
        /// </summary>
        Segment
    }

    /// <summary>
    /// 泰森图的一条边
    /// </summary>
    public struct Edge
    {

        readonly public EdgeType Type;

        /// <summary>
        /// 与边关联的siteIndex
        /// </summary>
        readonly public int Site;

        /// <summary>
        /// 边的第一个点
        ///
        /// 如果是无线长的线 该点是线段中的一个点
        /// 如果边是一个点一个方向组成，该点为有位置的点
        /// 如果是有长度的边，这个点是其中一个点
        /// </summary>
        readonly public int Vert0;

        /// <summary>
        /// 第二个点
        /// 只有定长边会用到，其他类型为-1
        /// </summary>
        readonly public int Vert1;

        /// <summary>
        /// 边的方向，不是单位向量
        /// </summary>
        public Vector2 Direction;

        public Edge(EdgeType type, int site, int vert0, int vert1, Vector2 direction)
        {
            this.Type = type;
            this.Site = site;
            this.Vert0 = vert0;
            this.Vert1 = vert1;
            this.Direction = direction;
        }

        public override string ToString()
        {
            if (Type == EdgeType.Segment)
            {
                return string.Format("泰森边(Segment, {0}, {1}, {2})",
                        Site, Vert0, Vert1);
            }
            else if (Type == EdgeType.Segment)
            {
                return string.Format("泰森边(Line, {0}, {1}, {2})",
                        Site, Vert0, Direction);
            }
            else
            {
                return string.Format("泰森边(Ray, {0}, {1}, ({2}, {3}))",
                        Site, Vert0, Direction.x, Direction.y);
            }
        }
    }
	struct PointTriangle
	{
		public readonly int Point;
		public readonly int Triangle;

		public PointTriangle(int point, int triangle)
		{
			this.Point = point;
			this.Triangle = triangle;
		}

		public override string ToString()
		{
			return string.Format("PointTriangle({0}, {1})", Point, Triangle);
		}
	}
	class PTComparer : IComparer<PointTriangle>
	{
		public IList<Vector2> verts;
		public List<int> tris;

		public int Compare(PointTriangle pt0, PointTriangle pt1)
		{
			if (pt0.Point < pt1.Point)
			{
				return -1;
			}
			else if (pt0.Point > pt1.Point)
			{
				return 1;
			}
			else if (pt0.Triangle == pt1.Triangle)
			{
				Debug.Assert(pt0.Point == pt1.Point);
				return 0;
			}
			else
			{
				return CompareAngles(pt0, pt1);
			}
		}

		int CompareAngles(PointTriangle pt0, PointTriangle pt1)
		{
			Debug.Assert(pt0.Point == pt1.Point);

			// "reference" point
			var rp = verts[pt0.Point];

			// triangle centroids in "reference point space"
			var p0 = Centroid(pt0) - rp;
			var p1 = Centroid(pt1) - rp;

			// quadrants. false for 1,2, true for 3,4.
			var q0 = ((p0.y < 0) || ((p0.y == 0) && (p0.x < 0)));
			var q1 = ((p1.y < 0) || ((p1.y == 0) && (p1.y < 0)));

			if (q0 == q1)
			{
				// p0 and p1 are within 180 degrees of each other, so just
				// use cross product to find out if pt1 is to the left of
				// p0.
				var cp = p0.x * p1.y - p0.y * p1.x;

				if (cp > 0)
				{
					return -1;
				}
				else if (cp < 0)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}
			else
			{

				// if q0 != q1, q1 is true, then p0 is in quadrants 1 or 2,
				// and p1 is in quadrants 3 or 4. Hence, pt0 < pt1. If q1
				// is not true, vice versa.
				return q1 ? -1 : 1;
			}
		}

		Vector2 Centroid(PointTriangle pt)
		{
			var ti = pt.Triangle;
			return MeshUtility.TriangleCentroid(verts[tris[ti]], verts[tris[ti + 1]], verts[tris[ti + 2]]);
		}
	}
    #endregion
}
