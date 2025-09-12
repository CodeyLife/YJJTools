#if DOTWEEN
using DG.Tweening;
using Sirenix.OdinInspector;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YJJTool;

[RequireComponent(typeof(CanvasRenderer))]
public class Pie3DComponent : Graphic,ICanvasRaycastFilter,IPointerEnterHandler,IPointerExitHandler
{
    List<Vector2[]> arrList;
    Yjj_3DPieChart chart;
    Mesh _mesh;
    int dataIndex;
    //用来做hover效果的值 1 - set定的值
    float hoverValue = 1;
   public void FillData(List<Vector2[]> postions,int index,Yjj_3DPieChart c)
    {
        arrList = postions;
        chart = c;
        dataIndex = index;
        SetVerticesDirty();
    }

    Ray _debugRay;
    public void OnDrawGizmos()
    {
        Debug.DrawRay(_debugRay.origin, _debugRay.direction * 10000, Color.yellow);

    }

    #region Hover判定
    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (_mesh == null)
            return false;

        //进入矩形范围内才触发这个函数，所以下面的暂时屏蔽
        //// 快速边界矩形检测   
        //if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, sp, eventCamera))
        //{
        //    Debug.Log("没在recttransform内");
        //    return false;
        //}
        // 创建射线
        Ray ray = GetRayFromScreenPoint(sp, eventCamera);
        _debugRay = new Ray(ray.origin, ray.direction);
        // 转换射线到局部空间
        Ray localRay = TransformRayToLocalSpace(ray);

        //// 边界球预检测
        //if (!CheckBoundingSphere(localRay.origin))
        //    return false;


        // 检查射线与网格的碰撞
        return CheckRayMeshIntersection(localRay);
    }

    private Ray GetRayFromScreenPoint(Vector2 screenPoint, Camera eventCamera)
    {
        if (eventCamera == null)
        {
            // ScreenSpace-Overlay 模式
            return new Ray(new Vector3(screenPoint.x, screenPoint.y,-1000), Vector3.forward);
        }

        // ScreenSpace-Camera 模式
        return eventCamera.ScreenPointToRay(screenPoint);
    }

    private Ray TransformRayToLocalSpace(Ray worldRay)
    {
        Vector3 localOrigin = transform.worldToLocalMatrix.MultiplyPoint(worldRay.origin);
        Vector3 localDirection = transform.worldToLocalMatrix.MultiplyVector(worldRay.direction).normalized;

        return new Ray(localOrigin, localDirection);
    }

    private bool CheckRayMeshIntersection(Ray localRay)
    {
        // 详细三角形检测
        bool raycast = CheckTrianglesIntersection(localRay);
        if (raycast)
        {
            
        }
        return raycast;
    }


    private bool CheckBoundingSphere(Vector3 localOrigin)
    {
        // 计算扇形的大致边界球半径
        float maxRadius = 0;
        foreach (var pos in arrList)
        {
            maxRadius = Mathf.Max(maxRadius, pos[0].magnitude, pos[1].magnitude);
        }

        // 添加厚度影响
        float boundingRadius = maxRadius + chart.pieDepth * 0.5f;

        // 检查是否在边界球内
        return localOrigin.magnitude <= boundingRadius * 1.2f;
    }


    private bool CheckTrianglesIntersection(Ray localRay)
    {
        // 获取网格三角形
        Vector3[] vertices = _mesh.vertices;
        int[] triangles = _mesh.triangles;

        // 遍历所有三角形
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = vertices[triangles[i]];
            Vector3 v1 = vertices[triangles[i + 1]];
            Vector3 v2 = vertices[triangles[i + 2]];

            if (MeshUtility.RayIntersectsTriangle(localRay, v0, v1, v2))
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (arrList == null) return;
        base.OnPopulateMesh(vh);
        vh.Clear();
        for (int j = 0; j < arrList.Count; j++)
        {
            var arr = arrList[j];
            bool drawLeft = j == 0;
            bool drawRight = j == arrList.Count - 1;
            var hover0 = arr[0] * hoverValue;
            var hover1 = arr[1] * hoverValue;
            Yjj_ChartUtility.DrawTriangleMesh(vh, Vector3.zero, hover0, hover1, chart.pieDepth * hoverValue, drawLeft, drawRight, color: chart.colors[dataIndex]);
        }
        _mesh = MeshUtility.GenerateMesh(_mesh, vh);
        MeshUtility.ReadMesh2VH(_mesh, vh, color);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        chart.OnPieEnter(dataIndex);
#if DOTWEEN
        DOTween.To(() => hoverValue,
            x => hoverValue = x,
            chart.hoverScale,
            chart.hoverDuration)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() => SetVerticesDirty());
#endif
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        chart.OnPieExit(dataIndex);
#if DOTWEEN
        DOTween.To(() => hoverValue,
         x => hoverValue = x,
         1,
         chart.hoverDuration)
         .SetEase(Ease.OutCubic)
         .OnUpdate(() => SetVerticesDirty());
#endif
    }
}
