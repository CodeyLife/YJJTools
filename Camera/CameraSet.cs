#if Use_CameraController
using Sirenix.OdinInspector;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CameraSet : YJJScritableSingletion<CameraSet>
{
    [LabelText("镜头旋转速度")]
    public float rotateSpeed = 2;
    [LabelText("镜头远近速度")]
    public float moveSpeed = 2;
    public float moveSmoothTime = 0.2f;
    public float rotateDamping = 3;

    public float forwardDamping = 3;
    [LabelText("与地面最低高度")]
    public float minHeigh = 1;
    [LabelText("镜头最低角度")]
    [ProgressBar(0, 89)]
    public float minAngle = 10;
    [LabelText("镜头与地面最近距离")]
    public float minDistance = 10;
    [LabelText("聚焦时间")]
    public float focusTime = 2f;
    [LabelText("聚焦距离")]
    public float focusDistance = 50;
    [Header("镜头远近距离阈值，低于该阈值速度衰减")]
    public float nearDistanceThreshold = 100;
}
#endif