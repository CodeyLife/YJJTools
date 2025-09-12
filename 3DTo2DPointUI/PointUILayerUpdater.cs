using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YJJTool
{
    public class PointUILayerUpdater : MonoBehaviour
    {
        private PointUI[] points;

        private void Awake()
        {
            points = transform.GetComponentsInChildren<PointUI>();
        }

        private void OnEnable()
        {
#if Use_CameraController
            CameraController.Instance.OnMove.AddListener(UpdateLayer);
#endif
        }
        private void OnDisable()
        {
#if Use_CameraController
            CameraController.Instance.OnMove.RemoveListener(UpdateLayer);
#endif
        }

        private void UpdateLayer()
        {
            points = points.OrderByDescending(x => Vector3.Distance(PointUI.UiCamera.transform.position, x.point.position)).ToArray();
            for (int i = 0; i < points.Length; i++)
            {
                points[i].transform.SetSiblingIndex(i);
            }
        }
    }
}