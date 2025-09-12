using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_Base : MonoBehaviour
{
    [InfoBox("用于UI的动画组件")]
    public float during = 1;

    protected bool allowAnimation = true;

    public virtual  void FadeIn()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();

    }
    /// <summary>
    /// 关闭物体需要手动关闭
    /// </summary>
    public virtual void FadeOut()
    {
        StopAllCoroutines();
    }
}
