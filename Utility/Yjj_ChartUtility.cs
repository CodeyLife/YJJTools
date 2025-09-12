#undef USEJOB
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
#if USEJOB
using Unity.Mathematics;
#endif
using UnityEngine;
using UnityEngine.UI;



public class ComponentDescAttribute : Attribute
{
    public string desc;
    public ComponentDescAttribute(string str)
    {
        desc = str;
    }
}


namespace YJJTool
{
    public static class Yjj_ChartUtility
    {
        #region 缓存数组
        //抗锯齿逻辑 边缘vector.right 中心zero
        public static UIVertex[] vertexArr7 = new UIVertex[7];
        public static UIVertex[] vertexArr4 = new UIVertex[4];
        public static int[] triangle18 = new int[18];
        public static int[] triangle6 = new int[6];
        public static void SetArr(UIVertex u0, UIVertex u1, UIVertex u2, UIVertex u3, UIVertex u4, UIVertex u5, UIVertex u6)
        {
            vertexArr7[0] = u0; vertexArr7[1] = u1; vertexArr7[2] = u2; vertexArr7[3] = u3; vertexArr7[4] = u4; vertexArr7[5] = u5; vertexArr7[6] = u6;
        }
        public static void SetTriangles(int t0, int t1, int t2, int t3, int t4, int t5, int t6, int t7, int t8, int t9, int t10, int t11, int t12, int t13, int t14, int t15, int t16, int t17)
        {
            triangle18[0] = t0; triangle18[1] = t1; triangle18[2] = t2; triangle18[3] = t3; triangle18[4] = t4; triangle18[5] = t5;
            triangle18[6] = t6; triangle18[7] = t7; triangle18[8] = t8; triangle18[9] = t9; triangle18[10] = t10; triangle18[11] = t11;
            triangle18[12] = t12; triangle18[13] = t13; triangle18[14] = t14; triangle18[15] = t15; triangle18[16] = t16; triangle18[17] = t17;
        }
        #endregion
        #region 画线
        /// <summary>
        /// Draw a line. 画直线
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="startPoint">起点</param>
        /// <param name="endPoint">终点</param>
        /// <param name="width">线宽</param>
        /// <param name="color">颜色</param>
        public static void DrawLine(VertexHelper vh, Vector3 startPoint, Vector3 endPoint, float width, Color32 color)
        {
            if (startPoint == endPoint || width == 0) return;
            Vector3 v = Vector3.Cross(endPoint - startPoint, Vector3.forward).normalized * width;
            UIVertex[] s_Vertex = new UIVertex[4];
            s_Vertex[0].position = startPoint - v;
            s_Vertex[1].position = endPoint - v;
            s_Vertex[2].position = endPoint + v;
            s_Vertex[3].position = startPoint + v;

            for (int j = 0; j < 4; j++)
            {
                s_Vertex[j].color = color;
                s_Vertex[j].uv0 = Vector2.zero;
            }
            vh.AddUIVertexQuad(s_Vertex);
        }

        public static void DrawLineSmooth(VertexHelper vh, Vector3 startPoint, Vector3 endPoint, float width, Color color)
        {
            if (startPoint == endPoint || width == 0) return;

            DrawLineSmooth(vh, new Vector2[] { startPoint, endPoint }, width, color);

        }

        public static Vector2 DrawLineSmoothWithLerp(VertexHelper vh, IList<Vector2> arr, float width, Color color,float t)
        {
            if(t == 1)
            {
                DrawLineSmooth(vh, arr, width, color);
                return arr[^1];
            }
            float length = 0;
            Vector2 result = Vector2.zero;
            for (int i = 1; i < arr.Count; i++)
            {
                length += Vector3.Distance(arr[i], arr[i - 1]);
            }
            //当前应该绘制到的长度为 
            float leaveLength = length * t;
            List<Vector2> drawList = new List<Vector2>() { arr[0] };

        
            for (int i = 1; i < arr.Count; i++)
            {
                var ilength = Vector3.Distance(arr[i], arr[i - 1]);
                if (leaveLength >= ilength)
                {
                    leaveLength -= ilength;
                    drawList.Add(arr[i]);
                    result = arr[i];
                }
                else
                {
                    var lerpValue = leaveLength / ilength;
                    result = Vector3.Lerp(arr[i - 1], arr[i], lerpValue);
                    drawList.Add(result);
                    break;
                }
            }
            DrawLineSmooth(vh,drawList, width, color);
            return result;
        }

        public static void DrawLineSmooth(VertexHelper vh, IList<Vector2> arr, float width, Color color, bool isCicle = false)
        {
            if (arr.Count < 2 || width == 0)
            {
                return;
            }

            Vector2 v = Vector3.Cross(arr[1] - arr[0], Vector3.forward).normalized * width;//向下的向量

            //记录上一个点结束的两个端点
            Vector2 v0 = arr[0] - v;
            Vector2 v1 = arr[0] + v;

            //用来存放当前线段结束的两个端点及中点
            UIVertex currentUp = new UIVertex();
            UIVertex currentDown = new UIVertex();
            UIVertex currenCenter = new UIVertex();
            //用来存放下一个线段开始的端点
            UIVertex nextUp = new UIVertex();
            UIVertex nextDown = new UIVertex();
            UIVertex nextCenter = new UIVertex();


            UIVertex cicleUp = new UIVertex();
            UIVertex cicleDonw = new UIVertex();


            if (isCicle)
            {
                if (GetLinePos(arr[arr.Count - 2], arr[0], arr[1], width, ref v0, ref v1))
                {
                    //角度过小 处理
                    ComplateMinAngle(vh, arr[arr.Count - 2], arr[0], arr[1], v0, v1, width, color, ref currentUp, ref currenCenter, ref currentDown, ref nextUp, ref nextCenter, ref nextDown, false);
                    cicleUp = currentUp;
                    cicleDonw = currentDown;
                }
                else
                {
                    //正常角度
                    nextUp = GetVertex(v0, Vector2.one, color);
                    nextDown = GetVertex(v1, Vector2.one, color);
                    nextCenter = GetVertex((v0 + v1) * 0.5f, Vector2.zero, color);
                    cicleUp = nextUp;
                    cicleDonw = nextDown;
                    //DrawCircle(vh, nextUp.position, 2, Color.red);
                }

            }
            else
            {
                nextUp = GetVertex(v0, Vector2.one, color);
                nextDown = GetVertex(v1, Vector2.one, color);
                nextCenter = GetVertex((v0 + v1) * 0.5f, Vector2.zero, color);
            }

            for (int i = 1; i < arr.Count; i++)
            {
                var last = arr[i - 1];
                var current = arr[i];
                if (i == arr.Count - 1)
                {
                    //最后一个数据
                    if (isCicle)
                    {
                        DrawQuadSmooth(vh, nextUp, nextDown, cicleUp, cicleDonw, color, (last + current) * 0.5f);
                    }
                    else
                    {
                        v = Vector3.Cross(current - last, Vector3.forward).normalized * width;
                        var up = current - v;
                        var down = current + v;
                        currentUp = GetVertex(up, color, Vector2.one);
                        currentDown = GetVertex(down, color, Vector2.one);
                        DrawQuadSmooth(vh, nextUp, nextDown, currentUp, currentDown, color, (last + current) * 0.5f);
                    }
                }
                else
                {

                    var next = arr[i + 1];
                    if (GetLinePos(last, current, next, width, ref v0, ref v1))
                    {
                        //角度过小，开始优化

                        ComplateMinAngle(vh, last, current, next, v0, v1, width, color, ref currentUp, ref currenCenter, ref currentDown, ref nextUp, ref nextCenter, ref nextDown, true, isBegin: i == 1);
                        //DrawQuadSmooth(vh, beginUp, beginDowon, currentUp, currentDown, color, (last + current) * 0.5f);
                    }
                    else
                    {
                        currentUp = GetVertex(v0, Vector2.one, color);
                        currentDown = GetVertex(v1, Vector2.one, color);
                        currenCenter = GetVertex((v0 + v1) * 0.5f, Vector2.zero, color);

                        if (i == 1)
                        {
                            DrawQuadSmoothWithCenter(vh, nextUp, nextCenter, nextDown, currentUp, currenCenter, currentDown, color, (last + current) * 0.5f);
                        }
                        else
                        {
                            DrawQuadSmoothWithCenter(vh, currentUp, currenCenter, currentDown, color, (last + current) * 0.5f);
                        }
                        nextUp = currentUp;
                        nextDown = currentDown;
                        nextCenter = currenCenter;
                    }
                }

            }
        }

        /// <summary>
        /// 角度过小 补齐
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="lastPos"></param>
        /// <param name="currentPos"></param>
        /// <param name="nextPos"></param>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <param name="currentUp"></param>
        /// <param name="currentDown"></param>
        /// <param name="nextUp"></param>
        /// <param name="nextDonw"></param>
        private static void ComplateMinAngle(VertexHelper vh, Vector2 lastPos, Vector2 currentPos, Vector2 nextPos, Vector2 dir1, Vector2 dir2, float width, Color color
            , ref UIVertex currentUp, ref UIVertex currentCenter, ref UIVertex currentDown, ref UIVertex nextUp, ref UIVertex nextCenter, ref UIVertex nextDown, bool draw, bool isBegin = false)
        {
            var dir = (dir1 + dir2).normalized;

            var angle = Vector2.Angle(dir1, dir2);
            var length = width / Mathf.Sin(Mathf.Deg2Rad * angle * 0.5f);
            var half = (Vector2.Distance(lastPos, currentPos) + Vector2.Distance(currentPos, nextPos)) * 0.5f;
            if (length > half)
            {
                length = half;
            }
            var x = currentPos - dir * length;  //内圈交点
            var vx = GetVertex(x, Vector2.one, color);
            //DrawCircle(vh, x, 3, Color.white, 3);

            Vector2 line1Dir = Vector3.Cross(currentPos - lastPos, Vector3.forward).normalized * width;
            Vector2 line2Dir = Vector3.Cross(nextPos - currentPos, Vector3.forward).normalized * width;


            //夹角在上方 ^ 字型
            if (MeshUtility.ToTheLeft(currentPos, lastPos, nextPos))
            {

                //右上角的点
                var b = currentPos - line2Dir;
                //向外挤出
                var a = currentPos - line1Dir;
                var oa = a + dir1 * width;
                var voa = GetVertex(oa, Vector2.one, color);

                var lc = (oa + x) * 0.5f;
                var vlc = GetVertex(lc, Vector2.zero, color);
                var ob = b + dir2 * width;
                var vob = GetVertex(ob, Vector2.one, color);
                //DrawCircle(vh, currentPos, 1, Color.green, 5);
                var rc = (ob + x) * 0.5f;
                var centenr = currentPos;
                //SegmentsInterPoint(lastPos, lc, nextPos, rc, ref centenr);
                var vc = GetVertex(centenr, Vector2.zero, color);


                var vrc = GetVertex(rc, Vector2.zero, color);


                if (draw)
                {

                    if (vh.currentVertCount < 3 || isBegin)
                    {
                        // DrawQuadSmoothWithCenter(vh,nextUp,nextCenter,nextDown,voa, vlc, vx, color, (lastPos + currentPos) * 0.5f);
                        DrawQuadSmoothWithCenter(vh, nextUp, nextCenter, nextDown, voa, vlc, vx, color, (lastPos + currentPos) * 0.5f);
                    }
                    else
                    {
                        DrawQuadSmoothWithCenter(vh, voa, vlc, vx, color, (lastPos + currentPos) * 0.5f);
                    }
                }
                else
                {
                    vh.AddVert(voa); vh.AddVert(vlc); vh.AddVert(vx);
                }
                currentUp = voa;
                currentDown = vx;
                currentCenter = vlc;

                nextUp = vob;
                nextDown = vx;
                nextCenter = vrc;
                var index = vh.currentVertCount;
                vh.AddVert(vc);
                vh.AddVert(vob);
                vh.AddVert(vrc);
                vh.AddVert(vx);

                vh.AddTriangle(index, index - 2, index - 3);
                vh.AddTriangle(index, index - 3, index + 1);
                vh.AddTriangle(index, index + 1, index + 2);
                vh.AddTriangle(index, index + 2, index + 3);
                vh.AddTriangle(index, index + 3, index - 2);

            }
            else
            {

                //V字型
                //左下角的点
                var a = currentPos + line1Dir;
                //右下角的点
                var b = currentPos + line2Dir;
                var oa = a + dir1 * width;
                var ob = b + dir2 * width;
                var voa = GetVertex(oa, Vector2.one, color);
                var vob = GetVertex(ob, Vector2.one, color);

                var vc = GetVertex(currentPos, Vector2.zero, color);
                //DrawCircle(vh, currentPos, 1, Color.green, 5);
                //DrawCircle(vh, vob.position, 1, Color.red, 5);

                var lc = (oa + x) * 0.5f;
                var rc = (ob + x) * 0.5f;

                var vlc = GetVertex(lc, Vector2.zero, color);
                var vrc = GetVertex(rc, Vector2.zero, color);



                if (draw)
                {
                    if (vh.currentIndexCount < 3 || isBegin)
                    {
                        DrawQuadSmoothWithCenter(vh, nextUp, nextCenter, nextDown, vx, vlc, voa, color, (lastPos + currentPos) * 0.5f);
                    }
                    else
                    {
                        DrawQuadSmoothWithCenter(vh, vx, vlc, voa, color, (lastPos + currentPos) * 0.5f);
                    }
                }
                else
                {
                    vh.AddVert(vx); vh.AddVert(vlc); vh.AddVert(voa);
                }
                currentUp = vx;
                currentDown = voa;
                currentCenter = vlc;
                var i = vh.currentVertCount;
                //增加顶点
                vh.AddVert(vc); vh.AddVert(vx); vh.AddVert(vrc); vh.AddVert(vob);
                vh.AddTriangle(i, i - 2, i + 1);
                vh.AddTriangle(i, i + 1, i + 2);
                vh.AddTriangle(i, i + 2, i + 3);
                vh.AddTriangle(i, i + 3, i - 1);
                vh.AddTriangle(i, i - 1, i - 2);


                nextUp = vx;
                nextDown = vob;
                nextCenter = vrc;
            }

        }
        /// <summary>
        /// 返回角度是否过小，需要补齐,如果需要补齐，会设置两个线的向量
        /// </summary>
        /// <param name="lastPos"></param>
        /// <param name="currentPos"></param>
        /// <param name="nextPos"></param>
        /// <param name="width"></param>
        /// <param name="up"></param>
        /// <param name="down"></param>
        /// <returns></returns>
        private static bool GetLinePos(Vector2 lastPos, Vector2 currentPos, Vector2 nextPos, float width, ref Vector2 up, ref Vector2 down)
        {
            var dir1 = (currentPos - lastPos).normalized;
            var dir2 = (currentPos - nextPos).normalized;
            var angle = Vector2.Angle(dir1, dir2);
            if (angle == 180)
            {
                Vector2 dir = Vector3.Cross(dir2, Vector3.forward).normalized;
                var unit = dir * width;
                up = currentPos + unit;
                down = currentPos - unit;
                return false;
            }
            if (angle > 60)
            {
                var length = width / Mathf.Sin(Mathf.Deg2Rad * angle * 0.5f);
                var dir = (dir1 + dir2).normalized;
                var unit = dir * length;
                //判断夹角方向
                if (MeshUtility.ToTheLeft(currentPos, lastPos, nextPos))
                {
                    up = currentPos + unit;
                    down = currentPos - unit;
                }
                else
                {
                    up = currentPos - unit;
                    down = currentPos + unit;
                }
                return false;
            }
            else
            {
                up = dir1; down = dir2;
                return true;
            }
        }
        // 三维空间中的左转判断（在XZ平面上）
        private static bool IsLeftTurn(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 bc = c - b;

            // 计算叉积的Y分量（在XZ平面）
            float cross = ab.z * bc.x - ab.x * bc.z;
            return cross > 0;
        }

        public static void DrawLines(VertexHelper vh, IList<Vector2> arr, float width, Color color)
        {
            for (int i = 1; i < arr.Count; i++)
            {
                DrawLine(vh, arr[i], arr[i - 1], width, color);
                DrawCircle(vh, arr[i], 0.1f, color, 4);
            }
        }

        /// <summary>
        /// 求两个直线的交点，注意角度过小时交点过远的影响,返回是否有交点
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool SegmentsInterPoint(Vector2 a, Vector2 b, Vector2 c, Vector2 d, ref Vector2 p)
        {
            float a1 = b.y - a.y;
            float b1 = a.x - b.x;
            float c1 = a.x * b.y - b.x * a.y;
            float a2 = d.y - c.y;
            float b2 = c.x - d.x;
            float c2 = c.x * d.y - d.x * c.y;
            float t = (a1 * b2 - a2 * b1);
            if (t == 0)
            {
                p = Vector2.zero;
                return false;
            }
            else
            {
                float x = (c1 * b2 - c2 * b1) / t;
                float y = (a1 * c2 - a2 * c1) / t;
                p = new Vector2(x, y);
                return true;
            }
        }
        private static bool LineIntersection(Vector2 line1Point1, Vector2 line1Point2, Vector2 line2Point1, Vector2 line2Point2, ref Vector2 p)
        {
            // 获取直线1的方向向量
            Vector2 line1Direction = line1Point2 - line1Point1;

            // 获取直线2的方向向量
            Vector2 line2Direction = line2Point2 - line2Point1;

            // 计算直线2相对于直线1的方向向量
            Vector2 relativeDirection = line2Point1 - line1Point1;

            // 计算直线交点的参数t
            float t = (relativeDirection.x * line2Direction.y - relativeDirection.y * line2Direction.x) /
                      (line1Direction.x * line2Direction.y - line1Direction.y * line2Direction.x);

            // 计算直线交点
            p = line1Point1 + line1Direction * t;
            return true;
        }
        #endregion


        #region 三角形
        /// <summary>
        /// 画三角形
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        public static void DrawTriangle(VertexHelper vh, UIVertex v1, UIVertex v2, UIVertex v3)
        {
            int index = vh.currentVertCount;
            vh.AddVert(v1);
            vh.AddVert(v2);
            vh.AddVert(v3);
            vh.AddTriangle(index, index + 1, index + 2);
        }
        public static void DrawTriangle(VertexHelper vh, Vector3 p0, Vector3 p1, Vector3 p2, Color? color = null)
        {
            DrawTriangle(vh, GetVertex(p0, color: color), GetVertex(p1, color: color), GetVertex(p2, color: color));
        }

        /// <summary>
        /// 动态添加顶点和三角形
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="triangles"></param>
        /// <param name="postions"></param>
        public static void DrawTriangles(VertexHelper vh, UIVertex[] postions, int[] triangles)
        {
            var start = vh.currentVertCount;
            for (int i = 0; i < postions.Length; i++)
            {
                vh.AddVert(postions[i]);
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                vh.AddTriangle(start + triangles[i], start + triangles[i + 1], start + triangles[i + 2]);
            }
        }
        public static void DrawTrianglesDyna(VertexHelper vh, UIVertex[] postions, params int[] triangles)
        {
            var start = vh.currentVertCount;
            for (int i = 0; i < postions.Length; i++)
            {
                vh.AddVert(postions[i]);
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                vh.AddTriangle(start + triangles[i], start + triangles[i + 1], start + triangles[i + 2]);
            }
        }

        #endregion

        #region 四边面mesh
        /// <summary>
        /// 顺时针顺序给四个点
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public static void DrawQuad(VertexHelper vh, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Color? color = null)
        {
            var p0u = GetVertex(p0, color);
            var p1u = GetVertex(p1, color);
            var p2u = GetVertex(p2, color);
            var p3u = GetVertex(p3, color);
            DrawQuad(vh, p0u, p1u, p2u, p3u);
        }
        public static void DrawQuad(VertexHelper vh, UIVertex p0, UIVertex p1, UIVertex p2, UIVertex p3)
        {
            vertexArr4[0] = p0;
            vertexArr4[1] = p1;
            vertexArr4[2] = p2;
            vertexArr4[3] = p3;
            triangle6[0] = 0;
            triangle6[1] = 1;
            triangle6[2] = 2;
            triangle6[3] = 0;
            triangle6[4] = 2;
            triangle6[5] = 3;
            DrawTriangles(vh, vertexArr4, triangle6);
        }

        /// <summary>
        /// 绘制四边形 非顺时针（带抗锯齿）
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public static void DrawQuadSmooth(VertexHelper vh, UIVertex p0, UIVertex p1, UIVertex p2, UIVertex p3, Color color, Vector2 center)
        {
            Vector2 v4 = (p0.position + p1.position) * 0.5f;
            Vector2 v5 = (p2.position + p3.position) * 0.5f;
            UIVertex c = GetVertex(center, new Vector2(0, 0.5f), color);
            UIVertex v44 = GetVertex(v4, Vector2.zero, color);
            UIVertex v55 = GetVertex(v5, Vector2.up, color);

            //添加顶点
            SetArr(p0, p1, p2, p3, v44, v55, c);
            SetTriangles(6, 4, 0,
                6, 0, 2,
                6, 2, 5,
                6, 5, 3,
                6, 3, 1,
                6, 1, 4);
            DrawTriangles(vh, vertexArr7, triangle18);

            //画三角形
            /*
             * 
             *     v00 |--------------|v22
             *         | \          / |
             *         |   \      /   |
             *      v44|-------v6/-----|v55
             *         |      /  \    |
             *         |    /      \  |
             *    v11  |--------------|v33
             */

        }

        /// <summary>
        /// 先给左侧的点，再给右侧的点，带中点
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="lastUp"></param>
        /// <param name="lastDown"></param>
        /// <param name="currentUp"></param>
        /// <param name="p3"></param>
        /// <param name="color"></param>
        /// <param name="center"></param>
        public static void DrawQuadSmoothWithCenter(VertexHelper vh, UIVertex lastUp, UIVertex lastCenter, UIVertex lastDown, UIVertex currentUp, UIVertex currentCenter, UIVertex currentDown, Color color, Vector3 center)
        {
            UIVertex c = GetVertex(center, new Vector2(0, 0.5f), color);
            SetArr(lastUp, lastCenter, lastDown, c, currentUp, currentCenter, currentDown);
            SetTriangles(3, 1, 0,
                3, 0, 4,
                3, 4, 5,
                3, 5, 6,
                3, 6, 2,
                3, 2, 1);
            //添加顶点
            DrawTriangles(vh, vertexArr7, triangle18);

            //画三角形
            /*
             * 
             *     v00 |--------------|v4
             *         | \          / |
             *         |   \      /   |
             *      v1|-------v3/-----|v55
             *         |      /  \    |
             *         |    /      \  |
             *    v2   |--------------|v6
             */

        }
        /// <summary>
        /// 左侧的点用上三个点
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="currentUp"></param>
        /// <param name="currentCenter"></param>
        /// <param name="currentDown"></param>
        /// <param name="color"></param>
        /// <param name="center"></param>
        private static void DrawQuadSmoothWithCenter(VertexHelper vh, UIVertex currentUp, UIVertex currentCenter, UIVertex currentDown, Color color, Vector3 center)
        {
            UIVertex c = GetVertex(center, new Vector2(0, 0.5f), color);
            //添加顶点

            var index = vh.currentVertCount;
            if (index < 3)
            {
                Debug.LogError($"这里出错了,{index}");
            }
            vh.AddVert(c);
            vh.AddVert(currentUp);
            vh.AddVert(currentCenter);
            vh.AddVert(currentDown);
            vh.AddTriangle(index, index - 2, index - 3);
            vh.AddTriangle(index, index - 3, index + 1);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
            vh.AddTriangle(index, index + 3, index - 1);
            vh.AddTriangle(index, index - 1, index - 2);

            //画三角形
            /*
             * 
             *     -3  |--------------|1
             *         | \          / |
             *         |   \      /   |
             *      -2 |-------0/-----|2
             *         |      /  \    |
             *         |    /      \  |
             *     -1  |--------------|3
             */
        }

        /// <summary>
        /// 左上角开始，顺时针四个点，圆角矩形
        /// </summary>
        public static void DrawRoundQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, float r, Color? colorDefault = null, int smooth = 8)
        {
            var color = colorDefault == null ? Color.white : colorDefault.Value;
            Vector2 rightDic = (b - a).normalized * r;
            Vector2 leftDownDic = (d - a).normalized * r;
            var rightDownDic = (c - b).normalized * r;

            Vector2 a_b = a + rightDic;
            Vector2 b_a = b - rightDic;
            Vector2 b_c = b + rightDownDic;
            Vector2 c_b = c - rightDownDic;
            Vector2 c_d = c - rightDic;
            Vector2 d_c = d + rightDic;
            Vector2 d_a = d - leftDownDic;
            Vector2 a_d = a + leftDownDic;
            //中心交点 mnpq (圆角圆心)
            Vector2 m = a_b + leftDownDic;
            Vector2 n = b_a + rightDownDic;
            Vector2 p = c_d - rightDownDic;
            Vector2 q = d_c - leftDownDic;
            //矩形
            var mv = GetVertex(m, color);
            var nv = GetVertex(n, color);
            var pv = GetVertex(p, color);
            var qv = GetVertex(q, color);
            //画中心矩形
            DrawQuad(vh, m, n, p, q, color);
            //上面矩形
            DrawQuad(vh, GetVertex(a_b, color, Vector2.right), GetVertex(b_a, color, Vector2.right), nv, mv);
            //右
            DrawQuad(vh, GetVertex(b_c, color, Vector2.right), GetVertex(c_b, color, Vector2.right), pv, nv);
            //下
            DrawQuad(vh, GetVertex(c_d, color, Vector2.right), GetVertex(d_c, color, Vector2.right), qv, pv);
            //左
            DrawQuad(vh, GetVertex(d_a, color, Vector2.right), GetVertex(a_d, color, Vector2.right), mv, qv);

            //圆角
            var buttomAnlge = Vector2.Angle(rightDownDic, rightDic);
            var topAngle = 180 - buttomAnlge;

            //左上
            DrawHalfCicleForDir(vh, m, r, a - b, color, smooth, angle: topAngle);
            //右上                                   
            DrawHalfCicleForDir(vh, n, r, b - c, color, smooth, angle: topAngle);
            //右下                                    
            DrawHalfCicleForDir(vh, p, r, c - d, color, smooth, angle: buttomAnlge);
            //左下                                   
            DrawHalfCicleForDir(vh, q, r, d - a, color, smooth, angle: buttomAnlge);
        }

        //public static void DrawOneSideRoundQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, float r,bool roundOnRight, Color? colorDefault = null, int smooth = 8)
        //{
        //    //a----b
        //    //------
        //    //d----c
        //    var color = colorDefault == null ? Color.white : colorDefault.Value;
        //    Vector2 rightDic = (b - a).normalized * r;
        //    Vector2 leftDownDic = (d - a).normalized * r;
        //    var rightDownDic = (c - b).normalized * r;

        //    //向圆角半径内敛  （数据太小会出问题）
        //    //Vector2 a_b = a + rightDic;
        //    //Vector2 b_a = b - rightDic;
        //    //Vector2 b_c = b + rightDownDic;
        //    //Vector2 c_b = c - rightDownDic;
        //    //Vector2 c_d = c - rightDic;
        //    //Vector2 d_c = d + rightDic;
        //    //Vector2 d_a = d - leftDownDic;
        //    //Vector2 a_d = a + leftDownDic;


        //    //中心交点 mnpq (圆角圆心)
        //    Vector2 m = a_b + leftDownDic;
        //    Vector2 n = b_a + rightDownDic;
        //    Vector2 p = c_d - rightDownDic;
        //    Vector2 q = d_c - leftDownDic;

        //    //最中心四个点  从左到右
        //    //var c1 = (a + d) * 0.5f;
        //    //var c2 = (a_b + d_c) * 0.5f;
        //    //var c3 = (b_a + c_d) * 0.5f;
        //    //var c4 = (b + c) * 0.5f;
        //    var c1 = (a_b + d_c) * 0.5f;
        //    var c2 = (a + d) * 0.5f;
        //    var c3 = (b + c) * 0.5f;
        //    var c4 = (b_a + c_d) * 0.5f;

        //    //圆角
        //    var buttomAnlge = Vector2.Angle(rightDownDic, rightDic);
        //    var topAngle = 180 - buttomAnlge;

        //    //uv占比
        //    float t = 1-(r / (Vector2.Distance(a, d) * 0.5f));
        //    var uv = new Vector2(t, t);

        //    //矩形

        //    //如果圆角在右边
        //    if (roundOnRight)
        //    {
        //        DrawQuad(vh, GetVertex(a, Vector2.one, color), GetVertex(b_a, Vector2.one, color), GetVertex(c3, Vector2.zero, color), GetVertex(c1, Vector2.zero, color));
        //        DrawQuad(vh, GetVertex(c1, Vector2.zero, color), GetVertex(c3, Vector2.zero, color), GetVertex(c_d, Vector2.one, color), GetVertex(d, Vector2.one, color));
        //        //右上                                   
        //        DrawHalfCicleForDir(vh, n, r, b - c, color, smooth, angle: topAngle,centerUV: uv);
        //        //右下                                    
        //        DrawHalfCicleForDir(vh, p, r, c - d, color, smooth, angle: buttomAnlge, centerUV: uv);

        //        DrawQuad(vh, GetVertex(n,uv,color), GetVertex(b_c, Vector2.one, color),GetVertex(c_b,Vector2.one,color),GetVertex(p,uv,color));
        //    }
        //    else
        //    {
        //        DrawQuad(vh, GetVertex(a_b, Vector2.one, color), GetVertex(b, Vector2.one, color), GetVertex(c4, Vector2.zero, color), GetVertex(c2, Vector2.zero, color));
        //        DrawQuad(vh, GetVertex(c2, Vector2.zero, color), GetVertex(c4, Vector2.zero, color), GetVertex(c, Vector2.one, color), GetVertex(d_c, Vector2.one, color));
        //        //左上
        //        DrawHalfCicleForDir(vh, m, r, a - b, color, smooth, angle: topAngle,uv);
        //        //左下                                   
        //        DrawHalfCicleForDir(vh, q, r, d - a, color, smooth, angle: buttomAnlge,uv);

        //        DrawQuad(vh, GetVertex(a_d, Vector2.one, color), GetVertex(m, uv, color), GetVertex(q, uv, color), GetVertex(d_a, Vector2.one, color));
        //    }

        //}
        public static void DrawOneSideRoundQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, float r, bool roundOnRight, Color? colorDefault = null, int smooth = 8)
        {
            //a----b
            //------
            //d----c
            var color = colorDefault == null ? Color.white : colorDefault.Value;
            Vector2 rightDic = (b - a).normalized * r;
            Vector2 leftDownDic = (d - a).normalized * r;
            var rightDownDic = (c - b).normalized * r;

            //外扩                        
            Vector2 a_b = a - rightDic;
            Vector2 b_a = b + rightDic;
            Vector2 b_c = b + rightDownDic;
            Vector2 c_b = c - rightDownDic;
            Vector2 c_d = c + rightDic;
            Vector2 d_c = d - rightDic;
            Vector2 d_a = d - leftDownDic;
            Vector2 a_d = a + leftDownDic;

            //中心交点 mnpq (圆角圆心)
            Vector2 m = a_b + leftDownDic;
            Vector2 n = b_a + rightDownDic;
            Vector2 p = c_d - rightDownDic;
            Vector2 q = d_c - leftDownDic;

            //最中心四个点  从左到右
            //var c1 = (a_b + d_c) * 0.5f;
            //var c2 = (a + d) * 0.5f;
            //var c3 = (b + c) * 0.5f;
            //var c4 = (b_a + c_d) * 0.5f;

            //圆角
            var buttomAnlge = Vector2.Angle(rightDownDic, rightDic);
            var topAngle = 180 - buttomAnlge;

            //uv占比
            float t = 1 - (r / (Vector2.Distance(a, d) * 0.5f));
            var uv = new Vector2(t, t);

            //矩形

            //如果圆角在右边
            if (roundOnRight)
            {
                //右上                                   
                DrawHalfCicleForDir(vh, b_c, r, b - c, color, smooth, angle: topAngle, centerUV: uv);
                //右下                                    
                DrawHalfCicleForDir(vh, c_b, r, c - d, color, smooth, angle: buttomAnlge, centerUV: uv);

                DrawQuad(vh, GetVertex(b_c, uv, color), GetVertex(n, Vector2.one, color), GetVertex(p, Vector2.one, color), GetVertex(c_b, uv, color));
            }
            else
            {
                //左上
                DrawHalfCicleForDir(vh, a_d, r, a - b, color, smooth, angle: topAngle, uv);
                //左下                                   
                DrawHalfCicleForDir(vh, d_a, r, d - a, color, smooth, angle: buttomAnlge, uv);

                DrawQuad(vh, GetVertex(a_d, uv, color), GetVertex(d_a, uv, color), GetVertex(q, Vector2.one, color), GetVertex(m, Vector2.one, color));
            }

        }
        
        /// <summary>
        /// 绘制四边形，支持自定义UV
        /// </summary>
        private static void DrawQuad(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color, Vector2 uvA, Vector2 uvB, Vector2 uvC, Vector2 uvD)
        {
            UIVertex v1 = GetVertex(a, uvA, color);
            UIVertex v2 = GetVertex(b, uvB, color);
            UIVertex v3 = GetVertex(c, uvC, color);
            UIVertex v4 = GetVertex(d, uvD, color);
            
            vh.AddVert(v1);
            vh.AddVert(v2);
            vh.AddVert(v3);
            vh.AddVert(v4);
            
            int baseIndex = vh.currentVertCount - 4;
            vh.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
            vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
        }
        #endregion
        #region 三角形mesh
        public static void DrawTriangleMesh(VertexHelper vh, Vector3 one, Vector3 two, Vector3 three, float high, bool drawLeft = true, bool drawRight = true, Color? color = null)
        {
            //top
            var z = new Vector3(0, 0, high * 0.5f);
            one += z;two += z;three += z;
            DrawTriangle(vh, GetVertex(one, color: color), GetVertex(two, color: color), GetVertex(three, color: color));
            var h = Vector3.Cross((three - one), (two - one)).normalized * high;
            var oneDown = one - h;
            var twoDown = two - h;
            var threeDown = three - h;
            if (drawLeft)
            {
                DrawQuad(vh, twoDown, two, one, oneDown, color);
            }
            if (drawRight)
            {
                DrawQuad(vh, three, threeDown, oneDown, one, color);
            }
            //front
            DrawQuad(vh, two, twoDown, threeDown, three, color);
            //buttom
            //  DrawTriangle(vh, oneDown, threeDown, twoDown, color);
            DrawTriangle(vh,   threeDown, twoDown, oneDown, color);
        }
        #endregion
        public static void DrawBar(VertexHelper vh, Vector2 pos, float width, float height, Color color, Matrix4x4? mar = null)
        {
            //底面四个点
            Vector3 nl = new Vector3(pos.x - width, pos.y, -width);
            Vector3 nr = new Vector3(pos.x + width, pos.y, -width);
            Vector3 fl = new Vector3(pos.x - width, pos.y, width);
            var fr = new Vector3(pos.x + width, pos.y, width);
            //顶面四个点
            Vector3 tnl = new Vector3(pos.x - width, pos.y + height, -width);
            Vector3 tnr = new Vector3(pos.x + width, pos.y + height, -width);
            Vector3 tfl = new Vector3(pos.x - width, pos.y + height, width);
            var tfr = new Vector3(pos.x + width, pos.y + height, width);
            if (mar != null)
            {
                nl = mar.Value.MultiplyPoint(nl);
                nr = mar.Value.MultiplyPoint(nr);
                fl = mar.Value.MultiplyPoint(fl);
                fr = mar.Value.MultiplyPoint(fr);
                tnl = mar.Value.MultiplyPoint(tnl);
                tnr = mar.Value.MultiplyPoint(tnr);
                tfl = mar.Value.MultiplyPoint(tfl);
                tfr = mar.Value.MultiplyPoint(tfr);
            }
            //底面
            DrawQuad(vh, nl, fl, fr, nr, color);
            //顶面
            DrawQuad(vh, tnl, tfl, tfr, tnr, color);
            //左右
            DrawQuad(vh, tnl, tfl, fl, nl, color);
            DrawQuad(vh, tnr, tfr, fr, nr, color);
            //前后                      
            DrawQuad(vh, tfl, tfr, fr, fl, color);
            DrawQuad(vh, tnl, tnr, nr, nl, color);
        }
        #region 半圆和圆
#if USEJOB
        #region job方法
    /// <summary>
    /// 画半圆
    /// </summary>
    /// <param name="vh"></param>
    /// <param name="pos">中心点</param>
    /// <param name="width">半径</param>
    /// <param name="dir">方向，与半圆垂直的方向</param>
    private static void DrawHalfCircle(VertexHelper vh, Vector3 pos, float width, Vector3 dir, Color color, int smooth = 24, float angle = 180)
    {
        Vector3 v = Vector3.Cross(dir, Vector3.forward).normalized;
        DrawHalfCicleForDir(vh, pos, width, v, color, smooth, angle);
    }
    /// <summary>
    /// job里获取半圆使用
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="width"></param>
    /// <param name="v"></param>
    /// <param name="color"></param>
    /// <param name="smooth"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    private static NativeList<UIVertex> GetHalfCiclePos(Vector3 pos, float width, Vector3 v, Color color, int smooth = 24, float angle = 180)
    {
        var arr = new NativeList<UIVertex>(Allocator.Temp);
        float startAngle = Vector3.Angle(v, Vector3.up);
        if (Vector3.Cross(v, Vector3.up).z < 0)
        {
            startAngle = 360 - startAngle;
        }
        //初始角度 
        startAngle *= Mathf.Deg2Rad;
        float perAngle = angle / smooth * Mathf.Deg2Rad;
        UIVertex c = GetVertex(pos, Vector2.zero, color);
        for (int i = 0; i <= smooth; i++)
        {
            float a = startAngle + i * perAngle;
            Vector2 n = new Vector2(pos.x + Mathf.Sin(a) * width, pos.y + Mathf.Cos(a) * width);
            arr.Add(GetVertex(n, Vector2.one, color));
        }
        var list = new NativeList<UIVertex>(Allocator.Temp);
        for (int i = 0; i < arr.Length - 1; i++)
        {
            list.Add(c);
            list.Add(arr[i + 1]);
            list.Add(arr[i]);
        }
        arr.Dispose();
        return list;
    }
    public static void DrawHalfCicleForDir(VertexHelper vh, Vector3 pos, float width, Vector3 v, Color color, int smooth = 24, float angle = 180)
    {
        float startAngle = Vector3.Angle(v, Vector3.up);
        if (Vector3.Cross(v, Vector3.up).z < 0)
        {
            startAngle = 360 - startAngle;
        }
        //初始角度 
        startAngle *= Mathf.Deg2Rad;
        float perAngle = angle / smooth * Mathf.Deg2Rad;
        UIVertex c = GetVertex(pos, Vector2.zero, color);
        UIVertex[] arr = new UIVertex[smooth + 1];

        //for (int i = 0; i <= smooth; i++)
        //{
        //    float a = startAngle + i * perAngle;
        //    Vector2 n = new Vector2(pos.x + Mathf.Sin(a) * width, pos.y + Mathf.Cos(a) * width);
        //    arr[i] = GetVertex(n, Vector2.one, color);
        //}
        var nArr = new NativeArray<UIVertex>(arr.Length, Allocator.TempJob);
        var job = new HalfCicleJob()
        {
            startAngle = startAngle,
            perAngle = perAngle,
            arr = nArr,
            pos = pos,
            width = width,
            color = color,
        };
        var handle = job.Schedule(smooth + 1, new JobHandle());
        handle.Complete();
        arr = nArr.ToArray();
        nArr.Dispose();

        for (int i = 0; i < arr.Length - 1; i++)
        {
            DrawTriangle(vh, c, arr[i + 1], arr[i]);
        }

    }
    [BurstCompile]
    struct HalfCicleJob : IJobFor
    {
        public float startAngle;
        public float perAngle;
        public NativeArray<UIVertex> arr;
        public Vector3 pos;
        public float width;
        public Color color;
        public void Execute(int i)
        {
            float a = startAngle + (i) * perAngle;
            float2 n = new float2(pos.x + math.sin(a) * width, pos.y + math.cos(a) * width);
            var v = new UIVertex()
            {
                position = new float3(n.x, n.y, 0),
                uv0 = new Vector2(1, 1),
                color = color,
            };
            arr[i] = v;
        }
    }
        #endregion
#else
        /// <summary>
        /// 画半圆
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="pos">中心点</param>
        /// <param name="width">半径</param>
        /// <param name="dir">方向，与半圆垂直的方向</param>
        public static void DrawHalfCircle(VertexHelper vh, Vector3 pos, float width, Vector3 dir, Color color, int smooth = 24, float angle = 180)
        {
            Vector3 v = Vector3.Cross(dir, Vector3.forward).normalized;
            DrawHalfCicleForDir(vh, pos, width, v, color, smooth, angle);
        }
        public static void DrawHalfCicleForDir(VertexHelper vh, Vector3 pos, float width, Vector3 v, Color color, int smooth = 24, float angle = 180, Vector2? centerUV = null)
        {
            float startAngle = Vector3.Angle(v, Vector3.up);
            if (Vector3.Cross(v, Vector3.up).z < 0)
            {
                startAngle = 360 - startAngle;
            }
            //Debug.Log(startAngle);
            //  pos = pos - v * width * 1;
            //初始角度 
            startAngle *= Mathf.Deg2Rad;
            float perAngle = angle / smooth * Mathf.Deg2Rad;
            var tempUV = centerUV == null ? Vector2.zero : centerUV.Value;
            UIVertex c = GetVertex(pos, tempUV, color);

            int index = vh.currentVertCount;
            vh.AddVert(c);
            //UIVertex[] arr = new UIVertex[smooth + 1];
            for (int i = 0; i <= smooth; i++)
            {
                float a = startAngle + i * perAngle;
                Vector2 n = new Vector2(pos.x + Mathf.Sin(a) * width, pos.y + Mathf.Cos(a) * width);
                vh.AddVert(GetVertex(n, Vector2.one, color));
            }

            for (int i = 0; i < smooth; i++)
            {
                //  DrawTriangle(vh, c, arr[i + 1], arr[i]);
                vh.AddTriangle(index, index + i + 2, index + i + 1);
            }
        }
#endif
        public static void DrawCircle(VertexHelper vh, Vector3 center, float radius, Color color, int smooth = 36)
        {
            UIVertex c = GetVertex(center, Vector2.zero, color);
            var index = vh.currentVertCount;
            vh.AddVert(c);
            float angle = 360f / smooth * Mathf.Deg2Rad;
            for (int i = 0; i < smooth; i++)
            {
                float a = i * angle;
                vh.AddVert(GetVertex(new Vector3(Mathf.Sin(a) * radius + center.x, Mathf.Cos(a) * radius + center.y, 0), Vector2.right, color));
            }
            for (int i = 0; i < smooth - 1; i++)
            {
                vh.AddTriangle(index, index + i + 1, index + i + 2);
            }
            vh.AddTriangle(index, index + smooth - 1, index + 1);
        }
        #endregion
        #region 圆环

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="s">起始点</param>
        /// <param name="e">结束点</param>
        /// <param name="center">圆环中心点</param>
        /// <param name="width">圆环宽度</param>
        /// <param name="c">颜色</param>
        /// <param name="roundRadiu">圆角半径</param>
        /// <param name="which">1是左边，2是右边</param>
        public static void DrawRingSmoothForRound(VertexHelper vh, Vector2 s, Vector2 e, Vector2 center, float width, Color c, float roundRadiu, int which = 0)
        {
            Vector2 s1 = (center - s).normalized * width + s;  //开始近点
            Vector2 e1 = (center - e).normalized * width + e;   //结束近点
            Vector2 centerL = (s + s1) * 0.5f;
            Vector2 centerR = (e + e1) * 0.5f;
            var vs = GetVertex(s, Vector2.one, c);
            var vcl = GetVertex(centerL, Vector2.zero, c);
            var vs1 = GetVertex(s1, Vector2.one, c);
            var ve = GetVertex(e, Vector2.one, c);
            var vcr = GetVertex(centerR, Vector2.zero, c);
            var ve1 = GetVertex(e1, Vector2.one, c);

            // old function

            if (which == 0)
            {
                DrawTriangle(vh, vs, vcr, vcl);
                DrawTriangle(vh, vs, ve, vcr);
                DrawTriangle(vh, vcl, vcr, ve1);
                DrawTriangle(vh, vcl, ve1, vs1);
            }
            else if (which == 2)
            {
                DrawOneSideRoundQuad(vh, s, e, e1, s1, roundRadiu, true, c);
            }
            else
            {
                DrawOneSideRoundQuad(vh, s, e, e1, s1, roundRadiu, false, c);
            }
            //DrawRoundQuad(vh, s,e,e1,s1,1,c);
        }

        public static void DrawRingSmooth(VertexHelper vh, Vector2 s, Vector2 e, Vector2 center, float width, Color c)
        {
            Vector2 s1 = (center - s).normalized * width + s;  //开始近
            Vector2 e1 = (center - e).normalized * width + e;   //结束近点
            Vector2 centerL = (s + s1) * 0.5f;
            Vector2 centerR = (e + e1) * 0.5f;
            var vs = GetVertex(s, Vector2.one, c);
            var vcl = GetVertex(centerL, Vector2.zero, c);
            var vs1 = GetVertex(s1, Vector2.one, c);
            var ve = GetVertex(e, Vector2.one, c);
            var vcr = GetVertex(centerR, Vector2.zero, c);
            var ve1 = GetVertex(e1, Vector2.one, c);
            ///////////////////////////////////0    1    2   3    4    5
            DrawTrianglesDyna(vh, new UIVertex[] { vs, vcr, vcl, ve, ve1, vs1 },
                0, 1, 2,
                0, 3, 1,
                2, 1, 4,
                2, 4, 5);
        }
        public static void DrawRingSmooth(VertexHelper vh, IList<Vector2> list, Vector2 center, float width, Color c)
        {
            var length = list.Count - 1;
            for (int i = 0; i < length; i++)
            {
                DrawRingSmooth(vh, list[i], list[i + 1], center, width, c);
            }
        }
        public static void DrawRingRoundSmooth(VertexHelper vh, IList<Vector2> list, Vector2 center, float width, Color c, float roundRadiu)
        {
            var length = list.Count - 1;
            for (int i = 0; i < length; i++)
            {
                DrawRingSmooth(vh, list[i], list[i + 1], center, width, c);

                if (i == 0)
                {
                    DrawRingSmoothForRound(vh, list[i], list[i + 1], center, width, c, roundRadiu, 1);
                }
                else if (i == length - 1)
                {
                    DrawRingSmoothForRound(vh, list[i], list[i + 1], center, width, c, roundRadiu, 2);
                }
            }
        }
        #endregion
        #region 获取vertex
        /// <summary>
        /// 根据坐标和uv获得UIVertex
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="uv"></param>
        /// <returns></returns>
        public static UIVertex GetVertex(Vector3 pos, Vector2 uv)
        {
            UIVertex v = new UIVertex();
            v.position = pos;
            v.uv0 = uv;
            v.color = Color.white;
            return v;
        }
        public static UIVertex GetVertex(Vector3 pos, Color? color = null, Vector2? uv = null, Vector3? normal = null)
        {
            UIVertex v = new UIVertex();
            v.position = pos;
            v.color = color == null ? Color.white : color.Value;
            v.uv0 = uv == null ? Vector2.zero : uv.Value;
            v.normal = normal == null ? Vector3.up : normal.Value;
            return v;
        }
        public static UIVertex GetVertex(Vector3 pos, Vector2 uv, Color c)
        {
            UIVertex v = new UIVertex();
            v.position = pos;
            v.uv0 = uv;
            v.color = c;
            return v;
        }

        #endregion
        #region 获取最大最小值
        public static int GetMaxData(List<float> datas)
        {
            float value = datas.Max();
            int ceil = Mathf.CeilToInt(value);
            string str = ceil.ToString();
            int max = (int)Mathf.Pow(10, str.Length - 1);
            max = Mathf.CeilToInt(ceil / (float)max) * max;
            max = max == 0 ? 1 : max;
            return max;
        }
        public static int GetMaxData(float value)
        {
            int ceil = Mathf.CeilToInt(value);
            string str = ceil.ToString();
            int max = (int)Mathf.Pow(10, str.Length - 1);
            max = Mathf.CeilToInt(ceil / (float)max) * max;
            return max;
        }
        public static int GetMinData(List<float> datas)
        {
            float value = 0;
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i] < value)
                {
                    value = datas[i];
                }
            }
            int ceil = Mathf.CeilToInt(value);
            ceil = Mathf.Abs(ceil);
            string str = ceil.ToString();
            int min = (int)Mathf.Pow(10, str.Length - 1);
            min = Mathf.CeilToInt(ceil / (float)min) * min;
            min = value < 0 ? min * -1 : min;
            return min;
        }
        public static int GetMaxData(List<MultipleData> datas)
        {
            float value = 0;
            for (int i = 0; i < datas.Count; i++)
            {
                for (int j = 0; j < datas[i].datas.Count; j++)
                    if (datas[i].datas[j] > value)
                    {
                        value = datas[i].datas[j];
                    }
            }
            int ceil = Mathf.CeilToInt(value);
            string str = ceil.ToString();
            int max = (int)Mathf.Pow(10, str.Length - 1);
            max = Mathf.CeilToInt(ceil / (float)max) * max;
            return max;
        }
        public static int GetMinData(List<MultipleData> datas)
        {
            float value = 0;
            for (int i = 0; i < datas.Count; i++)
            {
                for (int j = 0; j < datas[i].datas.Count; j++)
                    if (datas[i].datas[j] < value)
                    {
                        value = datas[i].datas[j];
                    }
            }
            int ceil = Mathf.CeilToInt(value);
            ceil = Mathf.Abs(ceil);
            string str = ceil.ToString();
            int min = (int)Mathf.Pow(10, str.Length - 1);
            min = Mathf.CeilToInt(ceil / (float)min) * min;
            min = value < 0 ? min * -1 : min;
            return min;
        }
        /// <summary>
        /// 返回最大最小值数组，数组0为最小值，数组1为最大值
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static int[] GetMaxAndMinData(List<float> datas)
        {
            float maxValue = float.MinValue;
            float minValue = float.MaxValue;
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i] > maxValue)
                {
                    maxValue = datas[i];
                }
                if (datas[i] < minValue)
                {
                    minValue = datas[i];
                }
            }
            int ceil = Mathf.CeilToInt(maxValue);
            string str = ceil.ToString();
            int max = (int)Mathf.Pow(10, str.Length - 1);
            max = Mathf.CeilToInt(ceil / (float)max) * max;
            ceil = Mathf.FloorToInt(minValue);
            ceil = Mathf.Abs(ceil);
            str = ceil.ToString();
            int min = (int)Mathf.Pow(10, str.Length - 1);
            min = Mathf.FloorToInt(ceil / (float)min) * min;
            min = minValue < 0 ? min * -1 : min;
            //Debug.Log($"{minValue}:{min}");
            return new int[] { min, max };
        }
        /// <summary>
        /// 传入set，自动设置最大最小值
        /// </summary>
        /// <param name="set"></param>
        /// <param name="datas"></param>
        public static void SetMaxAndMinData(BaseSet set, List<MultipleData> datas)
        {
            if (datas.Count < set.rulerSet.Count)
            {
                Debug.LogError("数据太少");
                return;
            }
            if (set.rulerSet.Count == 1)
            {
                if (set.rulerSet[0].autoSetMinValue)
                {
                    var arr = GetMaxAndMinData(datas[0].datas);
                    var arr2 = GetMaxAndMinData(datas[1].datas);
                    arr[0] = arr[0] < arr2[0] ? arr[0] : arr2[0];
                    arr[1] = arr[1] > arr2[1] ? arr[1] : arr2[1];
                    set.rulerSet[0].SetMaxValue(arr[1], set);
                    set.rulerSet[0].min = arr[0];
                }
                else
                {
                    int max = 0;
                    for (int i = 0; i < datas.Count; i++)
                    {
                        var tempMax = GetMaxData(datas[i].datas);
                        max = max > tempMax ? max : tempMax;
                    }
                    set.rulerSet[0].SetMaxValue(max, set);
                }
            }
            else
            {
                if (set.rulerSet[0].autoSetMinValue)
                {
                    var arr = GetMaxAndMinData(datas[0].datas);
                    set.rulerSet[0].SetMaxValue(arr[1], set);
                    set.rulerSet[0].min = arr[0];
                }
                else
                {
                    set.rulerSet[0].SetMaxValue(GetMaxData(datas[0].datas), set);
                }
                if (set.rulerSet[1].autoSetMinValue)
                {
                    var arr = GetMaxAndMinData(datas[1].datas);
                    set.rulerSet[1].SetMaxValue(arr[1], set);
                    set.rulerSet[1].min = arr[0];
                }
                else
                {
                    set.rulerSet[1].SetMaxValue(GetMaxData(datas[1].datas), set);
                }
            }
        }
        #endregion
        #region 获取曲线方法

        /// <summary>
        /// 获取曲线数组
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Vector2[] GetCurveArr(IList<Vector2> list)
        {
            var arr = new Vector2[list.Count + 2];
            arr[0] = list[0] * 2 - list[1];
            for (int i = 0; i < list.Count; i++)
            {
                arr[i + 1] = list[i];
            }
            arr[arr.Length - 1] = list[list.Count - 1] * 2 - list[list.Count - 2];
            return arr;
        }
        /// <summary>
        /// 曲线
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector2 GetCurvePos(IList<Vector2> arr, float t)
        {
            var pos = GetCurvePosUnClamp(arr, t);
            pos = pos.y < 0 ? new Vector2(pos.x, 0) : pos;
            return pos;
        }
        /// <summary>
        /// 获取没有限制Y轴的曲线
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector2 GetCurvePosUnClamp(IList<Vector2> arr, float t)
        {
            //曲线
            int num = arr.Count - 3;
            int current = Mathf.Min(Mathf.FloorToInt(t * num), num - 1);
            float u = t * num - current;

            Vector2 a = arr[current];
            Vector2 b = arr[current + 1];
            Vector2 c = arr[current + 2];
            Vector2 d = arr[current + 3];
            Vector2 pos = 0.5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
            return pos;
        }
        /// <summary>
        /// 通过job获取曲线
        /// </summary>
        /// <param name="list">曲线经过的点</param>
        /// <param name="count">生产的曲线的点的个数</param>
        /// <returns></returns>
        public static List<Vector2> GetCurvePosFroJob(IList<Vector2> list, int count, bool clampY = true, bool needCreatArr = true)
        {
#if USEJOB
        var job = new CurveJob();
        var result = new NativeArray<Vector2>(count + 1, Allocator.TempJob);
        job.result = result;
        var arr = new NativeArray<Vector2>(list.Count + 2, Allocator.TempJob);
        arr[0] = list[0] * 2 - list[1];
        for (int i = 0; i < list.Count; i++)
        {
            arr[i + 1] = list[i];
        }
        arr[arr.Length - 1] = list[list.Count - 1] * 2 - list[list.Count - 2];
        job.arr = arr;
        job.clamp = clampY;
        var handle = job.Schedule(count + 1, new JobHandle());
        handle.Complete();
        var returnList = new List<Vector2>();
        for (int i = 0; i < result.Length; i++)
        {
            returnList.Add(result[i]);
        }
        result.Dispose();
        arr.Dispose();
        return returnList;
#else
            Vector2[] arr = null;
            if (needCreatArr)
            {
                arr = GetCurveArr(list);
            }
            else
            {
                arr = list.ToArray();
            }
            var results = new List<Vector2>();
            for (float i = 0; i <= count; i++)
            {
                if (clampY)
                {
                    results.Add(GetCurvePos(arr, i / count));
                }
                else
                {
                    results.Add(GetCurvePosUnClamp(arr, i / count));
                }
            }
            return results;
#endif
        }
#if USEJOB
    [BurstCompile, NoAlias]
    struct CurveJob : IJobFor
    {
        [ReadOnly]
        public NativeArray<Vector2> arr;
        [WriteOnly]
        public NativeArray<Vector2> result;
        [NoAlias] public bool clamp;
        public void Execute(int index)
        {
            float t = index / (float)(result.Length - 1);
            //曲线
            int num = arr.Length - 3;
            int current = math.min((int)math.floor(t * num), num - 1);
            float u = t * num - current;

            float2 a = arr[current];
            float2 b = arr[current + 1];
            float2 c = arr[current + 2];
            float2 d = arr[current + 3];
            float2 pos = 0.5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
            if (clamp && pos.y < 0)
            {
                pos = new Vector2(pos.x, 0);
            }
            result[index] = pos;
        }
    }
#endif
        #endregion
        /// <summary>
        /// 返回数组的中心点
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Vector2 GetCenter(List<Vector2> list)
        {
            var result = Vector2.zero;
            for (int i = 0; i < list.Count; i++)
            {
                result += list[i];
            }
            return result / list.Count;
        }
        /// <summary>
        /// 用于新版数据 list<float>
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="set"></param>
        /// <param name="dataSet"></param>
        /// <param name="index">rulerset 的索引</param>
        /// <param name="customDistance"></param>
        /// <param name="BuQuan"></param>
        /// <returns></returns>
        public static List<Vector2> GetPosFromData(List<float> datas, BaseSet set, DataSet dataSet, int index, bool customDistance = false, bool BuQuan = true)
        {
            List<Vector2> list = new List<Vector2>();
            //if (datas.Count < 2) { Debug.Log("数据太少"); return list; }
            //每个数据间的间距
            float unit = 0;
            bool addAtfist = (BuQuan && dataSet.distanceFormLeft > 0) || !customDistance;
            if (addAtfist)
            {
                list.Add(Vector2.zero);
            }
            var per = (set.hight - set.ruler_distanceFromTop - set.ruler_distanceFromX) / (set.rulerSet[index].max - set.rulerSet[index].min);
            if (!customDistance)
            {
                unit = (set.width) / (datas.Count + 1);
            }
            else if (datas.Count > 1)
            {
                unit = (set.width - dataSet.distanceFormLeft - dataSet.distanceFormRight) / (datas.Count - 1);
            }
            for (int i = 0; i < datas.Count; i++)
            {
                float x; float y;
                if (datas[i] < set.rulerSet[index].min)
                {
                    y = datas[i] / set.rulerSet[index].min * set.ruler_distanceFromX;
                }
                else
                {
                    // y = datas[i].dataValue / set.rulerSet[index].max * (set.hight - set.ruler_distanceFromTop - set.ruler_distanceFromX);

                    y = (datas[i] - set.rulerSet[index].min) * per + set.ruler_distanceFromX;
                }
                if (!customDistance)
                {
                    x = unit * (i + 1);
                }
                else
                {
                    x = unit * i + dataSet.distanceFormLeft;
                }
                //arr[i] = new Vector2(x, y);
                list.Add(new Vector2(x, y));
            }
            if (addAtfist)
            {
                float y = list[1].y * 2 - list[2].y;
                y = y < 0 ? 0 : y;
                y = y > set.hight ? set.hight : y;
                list[0] = new Vector2(0, y);
            }
            if ((BuQuan && dataSet.distanceFormRight > 0) || !customDistance)
            {
                float y = list[list.Count - 1].y * 2 - list[list.Count - 2].y;
                y = y < 0 ? 0 : y;
                y = y > set.hight ? set.hight : y;
                list.Add(new Vector2(set.width, y));
            }
            return list;
        }
        /// <summary>
        /// 带权重的数据
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="set"></param>
        /// <param name="dataSet"></param>
        /// <param name="index"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static List<Vector2> GetPosBaseWeight(List<float> datas, BaseSet set, DataSet dataSet, int index, List<float> weights)
        {
            List<Vector2> list = new List<Vector2>();
            if (datas.Count < 2) { Debug.Log("数据太少"); return list; }
            Vector2[] arr = new Vector2[datas.Count];
            //每个数据间的间距
            float unit;
            var per = (set.hight - set.ruler_distanceFromTop - set.ruler_distanceFromX) / (set.rulerSet[index].max - set.rulerSet[index].min);
            unit = (set.width - dataSet.distanceFormLeft - dataSet.distanceFormRight);
            float allW = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                allW += weights[i];
            }
            unit = unit / allW;
            float currentDistance = 0;
            for (int i = 0; i < datas.Count; i++)
            {
                float x; float y;
                if (datas[i] < set.rulerSet[index].min)
                {
                    y = datas[i] / set.rulerSet[index].min * set.ruler_distanceFromX;
                }
                else
                {
                    // y = datas[i].dataValue / set.rulerSet[index].max * (set.hight - set.ruler_distanceFromTop - set.ruler_distanceFromX);

                    y = (datas[i] - set.rulerSet[index].min) * per + set.ruler_distanceFromX;
                }
                float distance = unit * weights[i];
                x = distance + currentDistance + dataSet.distanceFormLeft;
                currentDistance += distance;
                arr[i] = new Vector2(x, y);
            }
            list.AddRange(arr);
            return list;
        }
        /// <summary>
        /// 用于横向表格
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="set"></param>
        /// <param name="dataSet"></param>
        /// <param name="index"></param>
        /// <param name="customDistance"></param>
        /// <param name="BuQuan"></param>
        /// <returns></returns>
        public static List<Vector2> GetPosFromDataHorizatal(List<float> datas, BaseSet set, DataSet dataSet, int index, bool customDistance = false, bool BuQuan = true)
        {
            List<Vector2> list = new List<Vector2>();
            if (datas.Count < 2) { Debug.Log("数据太少"); return list; }
            Vector2[] arr = new Vector2[datas.Count];
            var per = (set.width - set.ruler_distanceFromTop - set.ruler_distanceFromX) / (set.rulerSet[index].max - set.rulerSet[index].min);
            //每个数据间的间距
            float unit;
            if (!customDistance)
            {
                unit = (set.hight) / (datas.Count + 1);
            }
            else
            {
                unit = (set.hight - dataSet.distanceFormLeft - dataSet.distanceFormRight) / (datas.Count - 1);
            }
            for (int i = 0; i < datas.Count; i++)
            {
                float x; float y;
                if (datas[i] < set.rulerSet[index].min)
                {
                    y = datas[i] / set.rulerSet[index].min * set.ruler_distanceFromX;
                }
                else
                {
                    // y = datas[i].dataValue / set.rulerSet[index].max * (set.hight - set.ruler_distanceFromTop - set.ruler_distanceFromX);

                    y = (datas[i] - set.rulerSet[index].min) * per + set.ruler_distanceFromX;
                }
                if (!customDistance)
                {
                    x = unit * (i + 1);
                }
                else
                {
                    x = unit * i + dataSet.distanceFormLeft;
                }

                arr[i] = new Vector2(y, x);
            }
            if ((BuQuan && dataSet.distanceFormLeft > 0) || !customDistance)
            {
                float y = arr[0].y * 2 - arr[1].y;
                y = y < 0 ? 0 : y;
                y = y > set.hight ? set.hight : y;
                list.Add(new Vector2(0, y));
            }
            foreach (var v2 in arr)
            {
                list.Add(v2);
            }
            if ((BuQuan && dataSet.distanceFormRight > 0) || !customDistance)
            {
                float y = arr[arr.Length - 1].y * 2 - arr[arr.Length - 2].y;
                y = y < 0 ? 0 : y;
                y = y > set.hight ? set.hight : y;
                list.Add(new Vector2(set.width, y));
            }
            return list;
        }
        /// <summary>
        /// 基于中心的横向表格
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="set"></param>
        /// <param name="dataSet"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static List<Vector2> GetPosFromDataHorizatalCenter(List<float> datas, BaseSet set, DataSet dataSet, int index)
        {
            List<Vector2> list = new List<Vector2>();
            if (datas.Count < 2) { Debug.Log("数据太少"); return list; }
            float unit = (set.hight - dataSet.distanceFormLeft - dataSet.distanceFormRight) / (datas.Count - 1);  //Y轴高度单位
            float unitX = (set.width - set.ruler_distanceFromX - set.ruler_distanceFromTop) / (set.rulerSet[index].max - set.rulerSet[index].min) * 0.5f;
            for (int i = 0; i < datas.Count; i++)
            {
                float x; float y;
                x = (datas[i] - set.rulerSet[index].min) * unitX;
                y = unit * i + dataSet.distanceFormLeft;

                list.Add(new Vector2(x, y));
            }
            return list;
        }
        #region 数组转NativeArray
#if USEJOB
    public static NativeArray<T> ToNativeArray<T>(this List<T> datas, Allocator allocator = Allocator.TempJob) where T : struct
    {
        var native = new NativeArray<T>(datas.Count, allocator);
        for (int i = 0; i < datas.Count; i++)
        {
            native[i] = datas[i];
        }
        return native;
    }
    public static NativeArray<T> ToNativeArray<T>(this T[] datas, Allocator allocator = Allocator.TempJob) where T : struct
    {
        var native = new NativeArray<T>(datas.Length, allocator);
        for (int i = 0; i < datas.Length; i++)
        {
            native[i] = datas[i];
        }
        return native;
    }
#endif
        #endregion
    }
}
