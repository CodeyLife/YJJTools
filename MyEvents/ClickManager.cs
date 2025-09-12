using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickManager : YjjSingleton<ClickManager>
{
    private float _clickTime;
#if UNITY_EDITOR
    public bool _debug = false;
#endif
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            _clickTime = Time.realtimeSinceStartup;
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (Time.realtimeSinceStartup - _clickTime<=0.15f )
            {
                if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var raycast,float.MaxValue))
                {
#if UNITY_EDITOR
                    if (_debug)
                    {
                        var go = raycast.collider.gameObject;
                        Debug.Log($"点击到了{go.name}", go);
                    }
#endif
                    var clicker = raycast.collider.gameObject.GetComponent<Event_SceneClicker>();
                    if (clicker != null)
                    {
                        clicker.OnClick();
                    }
                }

            }
        }
    }
}
