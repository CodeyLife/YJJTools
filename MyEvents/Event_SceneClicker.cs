using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Event_SceneClicker : MonoBehaviour
{
    public UnityEvent clickEvent = new UnityEvent();
    public void OnClick()
    {
        clickEvent?.Invoke();
    }
}
