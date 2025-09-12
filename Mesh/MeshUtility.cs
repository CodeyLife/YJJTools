using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace YJJTool
{
    public static class MeshUtility
    {
        #region 基础几何
        /// <summary>
        /// 判断点是否在多边形范围内
        /// </summary>
        /// <param name="p"></param>
        /// <param name="vertexs"></param>
        /// <returns></returns>
        public static bool IsPointInPolygon(Vector2 p, List<Vector2> vertexs)
        {
            int crossNum = 0;
            int vertexCount = vertexs.Count;

            for (int i = 0; i < vertexCount; i++)
            {
                Vector2 v1 = vertexs[i];
                Vector2 v2 = vertexs[(i + 1) % vertexCount];

                if (((v1.y <= p.y) && (v2.y > p.y))
                    || ((v1.y > p.y) && (v2.y <= p.y)))
                {
                    if (p.x < v1.x + (p.y - v1.y) / (v2.y - v1.y) * (v2.x - v1.x))
                    {
                        crossNum += 1;
                    }
                }
            }

            if (crossNum % 2 == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 返回多边形的有符号区域。顺时针多边形返回正数面积，逆时针CW 多边形返回负面积
        /// </summary>
        public static float Area(IList<Vector2> polygon)
        {
            var area = 0.0f;

            var count = polygon.Count;

            for (int i = 0; i < count; i++)
            {
                var j = (i == count - 1) ? 0 : (i + 1);

                var p0 = polygon[i];
                var p1 = polygon[j];

                area += p0.x * p1.y - p1.y * p1.x;
            }

            return 0.5f * area;
        }

        // 计算多边形面积
        public static float Area(IList<Vector3> polyVerts)
        {
            float a = 0.0f;
            int n = polyVerts.Count;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                a += polyVerts[p].x * polyVerts[q].y - polyVerts[q].x * polyVerts[p].y;
            }
            return a * 0.5f;
        }


        /// <summary>
        /// 点p在从l0到l1的直线的左边吗？(Vector2)
        /// </summary>
        public static bool ToTheLeft(Vector2 p, Vector2 l0, Vector2 l1)
        {
            return ((l1.x - l0.x) * (p.y - l0.y) - (l1.y - l0.y) * (p.x - l0.x)) >= 0;
        }


        /// <summary>
        /// 点P在三角形内吗
        /// 点为顺时针)
        /// </summary>
        public static bool PointInTriangle(Vector2 p, Vector2 c0, Vector2 c1, Vector2 c2)
        {
            return ToTheLeft(p, c0, c1)
                && ToTheLeft(p, c1, c2)
                && ToTheLeft(p, c2, c0);
        }

        // 判断点是否在三角形内
        public static bool PointInTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
        {
            Vector3 v0 = C - A, v1 = B - A, v2 = P - A;
            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            return (u >= 0) && (v >= 0) && (u + v < 1);
        }

        /// <summary>
        /// 点p在由c0、c1和c2构成的外接圆内吗？
        /// </summary>
        public static bool InsideCircumcircle(Vector2 p, Vector2 c0, Vector2 c1, Vector2 c2)
        {
            var ax = c0.x - p.x;
            var ay = c0.y - p.y;
            var bx = c1.x - p.x;
            var by = c1.y - p.y;
            var cx = c2.x - p.x;
            var cy = c2.y - p.y;

            return (
                    (ax * ax + ay * ay) * (bx * cy - cx * by) -
                    (bx * bx + by * by) * (ax * cy - cx * ay) +
                    (cx * cx + cy * cy) * (ax * by - bx * ay)
            ) > 0.000001f;
        }


        /// <summary>
        /// 两条直线相交算法 直线由方向和直线上的一个点定义，out值表示该交点在给出的直线上的点和方向的距离
        /// X = P0+ V0*M0 X=P1+V1*M1
        /// </summary>
        /// <param name="p0">直线0上的某一点</param>
        /// <param name="v0">直线0的方向</param>
        /// <param name="p1"></param>
        /// <param name="v1"></param>
        /// <param name="m0">交点到直线0的距离</param>
        /// <param name="m1">交点到直线1的距离</param>
        /// <returns></returns>
        public static bool LineLineIntersection(Vector2 p0, Vector2 v0, Vector2 p1, Vector2 v1, out float m0, out float m1)
        {
            var det = (v0.x * v1.y - v0.y * v1.x);

            if (Mathf.Abs(det) < 0.001f)
            {
                m0 = float.NaN;
                m1 = float.NaN;

                return false;
            }
            else
            {
                m0 = ((p0.y - p1.y) * v1.x - (p0.x - p1.x) * v1.y) / det;

                if (Mathf.Abs(v1.x) >= 0.001f)
                {
                    m1 = (p0.x + m0 * v0.x - p1.x) / v1.x;
                }
                else
                {
                    m1 = (p0.y + m0 * v0.y - p1.y) / v1.y;
                }

                return true;
            }
        }


        /// <summary>
        /// 返回直线交点  直线由方向和直线上的一个点定义
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="v0"></param>
        /// <param name="p1"></param>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static Vector2 LineLineIntersection(Vector2 p0, Vector2 v0, Vector2 p1, Vector2 v1)
        {
            float m0, m1;

            if (LineLineIntersection(p0, v0, p1, v1, out m0, out m1))
            {
                return p0 + m0 * v0;
            }
            else
            {
                return new Vector2(float.NaN, float.NaN);
            }
        }


        /// <summary>
        /// 将向量向左旋转90度
        /// </summary>
        public static Vector2 RotateRightAngle(Vector2 v)
        {
            var x = v.x;
            v.x = -v.y;
            v.y = x;

            return v;
        }


        /// <summary>
        /// 返回由三个点定义的三角形的外接圆圆心
        /// c1 and c2) on its edge.
        /// </summary>
        public static Vector2 CircumcircleCenter(Vector2 c0, Vector2 c1, Vector2 c2)
        {
            var mp0 = 0.5f * (c0 + c1);
            var mp1 = 0.5f * (c1 + c2);

            var v0 = RotateRightAngle(c0 - c1);
            var v1 = RotateRightAngle(c1 - c2);

            float m0, m1;

            LineLineIntersection(mp0, v0, mp1, v1, out m0, out m1);

            return mp0 + m0 * v0;
        }


        /// <summary>
        /// 质心
        /// </summary>
        public static Vector2 TriangleCentroid(Vector2 c0, Vector2 c1, Vector2 c2)
        {
            var val = (1.0f / 3.0f) * (c0 + c1 + c2);
            return val;
        }

        /// <summary>
        /// 求p点到p1p2的距离
        /// </summary>
        /// <param name="p"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private static float DistanceFromPoint2Line(Vector3 p, Vector3 p1, Vector3 p2)
        {
            // 求A2B的距离
            float p2pDistance = Vector3.Distance(p2, p);    // 或者使用 p2p.magnitude
            Vector3 p2p1 = p2 - p1;
            Vector3 p2p = p2 - p;
            // 求p2p1·p2p
            float dotResult = Vector3.Dot(p2p1, p2p);
            // 求θ
            float seitaRad = Mathf.Acos(dotResult / (p2p1.magnitude * p2pDistance));
            // 求p点到p1p2的距离
            float distance = p2pDistance * Mathf.Sin(seitaRad);
            return distance;
        }

        /// <summary>
        /// 返回直线和平面的交点
        /// </summary>
        /// <param name="point"></param>
        /// <param name="direct"></param>
        /// <param name="planeNormal"></param>
        /// <param name="planePoint"></param>
        /// <returns></returns>
        public static Vector3 GetIntersectWithLineAndPlane(Vector3 point, Vector3 direct, Vector3 planeNormal, Vector3 planePoint)
        {
            float d = Vector3.Dot(planePoint - point, planeNormal) / Vector3.Dot(direct.normalized, planeNormal);
            var pos = d * direct.normalized + point;
            return pos;
        }

        // 计算两条直线的交点
        public static Vector3? GetIntersectionPoint(Vector3 line1Start, Vector3 line1Direction, Vector3 line2Start, Vector3 line2Direction)
        {
            // 计算方向向量的叉乘
            Vector3 crossDir = Vector3.Cross(line1Direction, line2Direction);
            float determinant = crossDir.sqrMagnitude; // 叉乘的模长的平方

            if (determinant < Mathf.Epsilon) // 如果叉乘接近于0，说明直线平行或重合
            {
                return null; // 不相交
            }

            // 计算从line1Start到line2Start的向量
            Vector3 line1ToLine2Start = line2Start - line1Start;

            // 计算交点在line1Direction上的参数t
            float t = Vector3.Dot(line1ToLine2Start, crossDir) / determinant;

            // 计算交点在line2Direction上的参数u
            float u = Vector3.Dot(line1ToLine2Start, Vector3.Cross(crossDir, line1Direction)) / determinant;

            // 检查参数是否在0到1之间，以确保交点在两条直线的线段上
            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                // 计算交点
                Vector3 intersectionPoint = line1Start + line1Direction * t;
                return intersectionPoint;
            }

            // 如果参数不在0到1之间，直线不相交
            return null;
        }


        #endregion

        #region UI Mesh
        public static void ReadMesh2VH(Mesh mesh, VertexHelper vh, Color c)
        {
            vh.Clear();
            var vertices = mesh.vertices;
            var uvs = mesh.uv;
            var normals = mesh.normals;
            var colors = mesh.colors;
            //Debug.Log($"顶点数:{vertices.Length},uv:{uvs.Length},normals:{normals.Length},三角形:{triangles.Length}");
            List<UIVertex> vertexs = new List<UIVertex>();
            for (int i = 0; i < vertices.Length; i++)
            {
                var uv = uvs.Length > i ? uvs[i] : Vector2.zero;
                var color = colors.Length > i ? colors[i] : c;
                var vertex = Yjj_ChartUtility.GetVertex(vertices[i], color, uv, normals[i]);
                vertexs.Add(vertex);
            }
            vh.AddUIVertexStream(vertexs, mesh.triangles.ToList());
        }
        public static Mesh GenerateMesh(Mesh mesh, VertexHelper vh)
        {
            if (mesh == null)
            {
                mesh = new Mesh();
            }
            vh.FillMesh(mesh);
            mesh.RecalculateNormals();
            return mesh;
        }
        #endregion

        /// <summary>
        ///  返回离散点边界(每条线的两个点) 暴力求解,可以使用优化更好的AlphaShapesFromDelaunay
        /// </summary>
        /// <param name="list"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static List<Vector2> AlphaShapes(IList<Vector2> list, float radius)
        {
            int count = list.Count;
            var radius2 = radius * 2;
            var result = new List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                var p1 = list[i];
                for (int j = 0; j < count; j++)
                {
                    if (i == j) continue;
                    var p2 = list[j];
                    var distance = Vector2.Distance(p1, p2);
                    if (distance > radius2) continue;
                    //圆心到p1-p2中心的距离
                    var dist = Mathf.Sqrt(Mathf.Pow(radius, 2) - 0.25f * Mathf.Pow(distance, 2));
                    var centerP = (p1 + p2) * 0.5f;
                    Vector2 dir = Vector3.Cross(p1 - p2, Vector3.forward).normalized;
                    var center1 = centerP + dist * dir;
                    var center2 = centerP - dist * dir;
                    if (AlpahShapesChecked(center1, center2, list, i, j))
                    {
                        result.Add(p1);
                        result.Add(p2);
                    }
                }
            }
            return result;
            bool AlpahShapesChecked(Vector2 c1, Vector2 c2, IList<Vector2> datas, int f, int s)
            {
                bool c1Check = true;
                bool c2Check = true;
                for (int i = 0; i < datas.Count; i++)
                {
                    var p = datas[i];
                    if (i == f || i == s) continue;
                    if (c1Check && Vector2.Distance(c1, p) < radius)
                    {
                        c1Check = false;
                    }
                    if (c2Check && Vector2.Distance(c2, p) < radius)
                    {
                        c2Check = false;
                    }
                    if (!c1Check && !c2Check) break;
                }
                return c1Check || c2Check;
            }
        }

        /// <summary>
        /// 返回边界每个线段两个端点的Index 不给半径参数会自动计算每个点间距的平均值（推荐使用多线程计算）
        /// </summary>
        /// <param name="list"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static List<int> AlphaShapesFromDelaunay(IList<Vector2> list, float? radius = null)
        {
            var caculator = Delaunay(list);
            var verts = caculator.postions;
            var trianlges = caculator.triangles;
            float tempRadius;
            if (radius == null)
            {
                var p00 = verts[0];
                float all = 0;
                for (int i = 1; i < verts.Count; i++)
                {
                    all += Vector2.Distance(p00, verts[i]);
                }
                tempRadius = all / (verts.Count - 1);
            }
            else
            {
                tempRadius = radius.Value;
            }

            var sqrRadius = Mathf.Pow(tempRadius, 2);
            var radius2 = tempRadius * 2;
            var tempTriangles = new List<int>();

            for (int i = 0; i < trianlges.Count; i += 3)
            {
                var a = verts[trianlges[i]];
                var b = verts[trianlges[i + 1]];
                var c = verts[trianlges[i + 2]];
                var distAB = Vector2.Distance(a, b);
                var distAC = Vector2.Distance(a, c);
                var distBC = Vector2.Distance(b, c);
                if (distAB > radius2 || distAC > radius2 || distBC > radius2)
                {
                    continue;
                }
                if (Check(a, b, distAB))
                {
                    tempTriangles.Add(trianlges[i], trianlges[i + 1]);
                }
                if (Check(a, c, distAC))
                {
                    tempTriangles.Add(trianlges[i], trianlges[i + 2]);
                }
                if (Check(b, c, distBC))
                {
                    tempTriangles.Add(trianlges[i + 1], trianlges[i + 2]);
                }
            }
            return tempTriangles;
            bool Check(Vector2 p1, Vector2 p2, float dist)
            {
                var halfDistance = dist * 0.5f;
                var dir1 = RotateRightAngle(p1 - p2).normalized;
                var dir2 = RotateRightAngle(p2 - p1).normalized;
                var cetenrDist = Mathf.Sqrt(Mathf.Pow(tempRadius, 2) - Mathf.Pow(halfDistance, 2));
                var center = (p1 + p2) * 0.5f;
                var c1 = center + dir1 * cetenrDist;
                var c2 = center + dir2 * cetenrDist;
                bool result = true;
                for (int i = 0; i < list.Count; i++)
                {
                    var p = list[i];
                    if (p == p1 || p == p2) continue;
                    if ((c1 - p).sqrMagnitude <= sqrRadius)
                    {
                        result = false;
                        break;
                    }
                }
                if (result) return true;
                for (int i = 0; i < list.Count; i++)
                {
                    var p = list[i];
                    if (p == p1 || p == p2) continue;
                    if ((c2 - p).sqrMagnitude <= sqrRadius)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// 返回有序凸包顶点
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Vector2> ConvexHull(IList<Vector2> list)
        {
            var result = new List<Vector2>();
            list = list.OrderBy(x => x.x).ToList();
            Vector2 left = list.First();
            Vector2 right = list.Last();
            var dir = right - left;
            var top = new List<Vector2>();
            var down = new List<Vector2>();
            for (int i = 0; i < list.Count; i++)
            {
                var x = list[i];
                var cross = Vector3.Cross(x - left, dir).z;
                if (cross < 0)
                {
                    top.Add(x);
                }
                else if (cross > 0)
                {
                    down.Add(x);
                }
            }
            result.Add(left);
            HullCalculate(top, left, right);
            result.Add(right);
            HullCalculateButtom(down, left, right, true);
            result.Add(left);
            return result;
            void HullCalculate(IList<Vector2> datas, Vector2 tempLeft, Vector2 tempRight, bool isDown = false)
            {
                //  Debug.Log(datas.Count);
                if (datas.Count <= 1)
                {
                    result.AddRange(datas);
                    return;
                }

                var max = datas.Select(x => new { distance = DistanceFromPoint2Line(x, tempLeft, tempRight), value = x }).OrderByDescending(x => x.distance).First().value;
                var leftDic = max - tempLeft;
                var rightDidc = max - tempRight;
                List<Vector2> leftTop = new List<Vector2>();
                var rightTop = new List<Vector2>();
                for (int i = 0; i < datas.Count; i++)
                {
                    var x = datas[i];
                    var leftCross = Vector3.Cross(leftDic, x - tempLeft).z;
                    var rightCross = Vector3.Cross(rightDidc, x - tempRight).z;
                    if ((leftCross > 0 && !isDown) || (leftCross < 0 && isDown))
                    {
                        leftTop.Add(x);
                    }
                    if ((rightCross < 0 && !isDown) || (rightCross > 0 && isDown))
                    {
                        rightTop.Add(x);
                    }
                }
                HullCalculate(leftTop, tempLeft, max, isDown);
                result.Add(max);
                HullCalculate(rightTop, max, tempRight, isDown);

            }
            void HullCalculateButtom(IList<Vector2> datas, Vector2 tempLeft, Vector2 tempRight, bool isDown = false)
            {
                //  Debug.Log(datas.Count);
                if (datas.Count <= 1)
                {
                    result.AddRange(datas);
                    return;
                }
                if (!result.Contains(tempRight))
                {
                    result.Add(tempRight);
                }

                var max = datas.Select(x => new { distance = DistanceFromPoint2Line(x, tempLeft, tempRight), value = x }).OrderByDescending(x => x.distance).First().value;
                var leftDic = max - tempLeft;
                var rightDidc = max - tempRight;
                List<Vector2> leftTop = new List<Vector2>();
                var rightTop = new List<Vector2>();
                for (int i = 0; i < datas.Count; i++)
                {
                    var x = datas[i];
                    var leftCross = Vector3.Cross(leftDic, x - tempLeft).z;
                    var rightCross = Vector3.Cross(rightDidc, x - tempRight).z;
                    if ((leftCross > 0 && !isDown) || (leftCross < 0 && isDown))
                    {
                        leftTop.Add(x);
                    }
                    if ((rightCross < 0 && !isDown) || (rightCross > 0 && isDown))
                    {
                        rightTop.Add(x);
                    }
                }
                HullCalculateButtom(rightTop, max, tempRight, isDown);
                result.Add(max);
                HullCalculateButtom(leftTop, tempLeft, max, isDown);
                if (!result.Contains(tempLeft))
                {
                    result.Add(tempLeft);
                }
            }
        }

        #region Delaunay 三角面

        /// <summary>
        /// Delaunay 三角面
        /// </summary>
        /// <param name="verts"></param>
        /// <returns></returns>
        public static DelaunayCalculator Delaunay(IList<Vector2> verts)
        {
            var caculator = new DelaunayCalculator();
            caculator.CalculateTriangulation(verts);
            return caculator;
        }
        /// <summary>
        /// 泰森多边形(根据点位分割图形)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static VoronoiCalculator Voronoi(IList<Vector2> list)
        {
            var delaunay = Delaunay(list);
            VoronoiCalculator calculator = new VoronoiCalculator(delaunay);
            return calculator;
        }
        #endregion

        public static Texture2D DrawPolygonTex(Vector2[] vertices,Color lineColor,int lineWidth,Color fillColor, int width = 512, int height = 512)
        {
           float worldMinX; // 世界坐标最小X
           float worldMaxX; // 世界坐标最大X
           float worldMinY; // 世界坐标最小Y
           float worldMaxY; // 世界坐标最大Y

            worldMinX = vertices.Min(x => x.x);
            worldMaxX = vertices.Max(x => x.x);
            worldMinY = vertices.Min(x => x.y);
            worldMaxY = vertices.Max(x => x.y);

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Vector2[] textureVertices = vertices.Select(v => WorldToTextureCoordinates(v)).ToArray();
            for (int i = 0; i < textureVertices.Length; i++)
            {
                Vector2 start = textureVertices[i];
                Vector2 end = textureVertices[(i + 1) % textureVertices.Length];
                DrawLine(start, end, lineColor,lineWidth);
            }

            FillPolygon(textureVertices, fillColor);

            // 应用Texture更改
            texture.Apply();

            return texture;

            // 将世界坐标转换为Texture坐标
            Vector2 WorldToTextureCoordinates(Vector2 worldPosition)
            {
                float textureX = (worldPosition.x - worldMinX) / (worldMaxX - worldMinX) * width;
                float textureY = (worldPosition.y - worldMinY) / (worldMaxY - worldMinY) * height;
                return new Vector2(textureX, textureY);
            }

            // 绘制多边形边
            void DrawLine(Vector2 start, Vector2 end, Color color, int lineWidth )
            {
                int x0 = (int)start.x;
                int y0 = (int)start.y;
                int x1 = (int)end.x;
                int y1 = (int)end.y;

                int dx = Mathf.Abs(x1 - x0);
                int dy = Mathf.Abs(y1 - y0);
                int sx = (x0 < x1) ? 1 : -1;
                int sy = (y0 < y1) ? 1 : -1;
                int err = ((dx > dy) ? dx : -dy) / 2;
                int e2;

                while (true)
                {
                    // 绘制当前点及其周围的点
                    for (int i = -lineWidth; i <= lineWidth; i++)
                    {
                        for (int j = -lineWidth; j <= lineWidth; j++)
                        {
                            int px = x0 + i;
                            int py = y0 + j;
                            if (px >= 0 && px < width && py >= 0 && py < height)
                            {
                                texture.SetPixel(px, py, color);
                            }
                        }
                    }

                    if (x0 == x1 && y0 == y1) break;
                    e2 = err;
                    if (e2 > -dx)
                    {
                        err -= dy;
                        x0 += sx;
                    }
                    if (e2 < dy)
                    {
                        err += dx;
                        y0 += sy;
                    }
                }
            }

            // 填充多边形内部
            void FillPolygon(Vector2[] vertices, Color color)
            {
                int minY = (int)vertices.Min(v => v.y);
                int maxY = (int)vertices.Max(v => v.y);

                for (int y = minY; y <= maxY; y++)
                {
                    List<int> intersections = new List<int>();

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector2 v1 = vertices[i];
                        Vector2 v2 = vertices[(i + 1) % vertices.Length];

                        if (v1.y <= y && v2.y > y || v2.y <= y && v1.y > y)
                        {
                            float x = v1.x + (y - v1.y) * (v2.x - v1.x) / (v2.y - v1.y);
                            intersections.Add((int)x);
                        }
                    }

                    intersections.Sort();

                    for (int i = 0; i < intersections.Count; i += 2)
                    {
                        for (int x = intersections[i]; x <= intersections[i + 1]; x++)
                        {
                            if (x >= 0 && x < width && y >= 0 && y < height)
                            {
                                texture.SetPixel(x, y, color);
                            }
                        }
                    }
                }
            }
        }

        // Möller-Trumbore 射线-三角形相交算法
        public static bool RayIntersectsTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            const float EPSILON = 0.0000001f;

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 h = Vector3.Cross(ray.direction, edge2);

            float a = Vector3.Dot(edge1, h);
            if (a > -EPSILON && a < EPSILON)
                return false; // 射线与三角形平行

            float f = 1.0f / a;
            Vector3 s = ray.origin - v0;
            float u = f * Vector3.Dot(s, h);

            if (u < 0.0 || u > 1.0)
                return false;

            Vector3 q = Vector3.Cross(s, edge1);
            float v = f * Vector3.Dot(ray.direction, q);

            if (v < 0.0 || u + v > 1.0)
                return false;

            // 计算交点距离
            float t = f * Vector3.Dot(edge2, q);
            return t > EPSILON;
        }
    }
}