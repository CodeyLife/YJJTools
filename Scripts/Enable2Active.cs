using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("_YjjTool/Enable2Active")]
public class Enable2Active : MonoBehaviour
{
    public List<GameObject> actives = new List<GameObject>();
    [LabelText("激活物体在该物体消失时关闭")]
    public bool disable2Close = false;
    public UnityEvent activeEvent = new UnityEvent();
    public List<GameObject> disables = new List<GameObject>();
    [LabelText("关闭物体在该物体消失时激活")]
    public bool activeInDisable = false;
    public UnityEvent disableEvent = new UnityEvent();

    private void OnEnable()
    {
        InvokeEnable();
    }
    /// <summary>
    /// 激活显示时事件
    /// </summary>
    public void InvokeEnable()
    {
        SetActive(true, actives);
        SetActive(false, disables);
        activeEvent?.Invoke();
    }

    private void SetActive(bool active,List<GameObject> list)
    {
        foreach (var go in list)
        {
            if (go != null)
            {
                go.SetActive(active);
            }
        }
    }
    private void OnDisable()
    {
        if (disable2Close)
        {
            SetActive(false,actives);
        }
        if (activeInDisable)
        {
            SetActive(true, disables);
        }
        disableEvent?.Invoke();
    }
}
