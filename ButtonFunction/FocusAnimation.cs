using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[AddComponentMenu("_YjjTool/FocusAnimation")]
public class FocusAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("如果为空动画目标为自己")]
    public Transform target;
    [SuffixLabel("秒")]
    public float animationTime = 0.25f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public Vector3 scaler = new Vector3(1.2f, 1.2f, 1.2f);
    private Vector3 oldScale;
    private bool isEnter = false;
    private void Awake()
    {
        if(target == null)
        {
            oldScale = transform.localScale;
        }
        else
        {
            oldScale = target.localScale;
        }
    }
    private void OnDisable()
    {
        transform.localScale = oldScale;
        isEnter = false;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isEnter) return;
        isEnter = true;
        if (target != null)
        {
            AnimationIn(target);
        }
        else
        {
            AnimationIn(transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isEnter) return;
        isEnter = false;
        if (target != null)
        {
            AnimationOut(target);
        }
        else
        {
            AnimationOut(transform);
        }
    }

    private void AnimationIn(Transform tar)
    {
        StopAllCoroutines();
        Vector3 targetscale = new Vector3(oldScale.x * scaler.x, oldScale.y*scaler.y, oldScale.z * scaler.z);
        StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
         {
             tar.localScale = Vector3.Lerp(oldScale, targetscale, t);
         }, null, curve));
    }
    private void AnimationOut(Transform tar)
    {
        StopAllCoroutines();
        Vector3 current = tar.localScale;
        StartCoroutine(YjjUtility.FadeIn(animationTime, (t) =>
         {
             tar.localScale = Vector3.Lerp(current, oldScale, t);
         }, null, curve));
    }
}
