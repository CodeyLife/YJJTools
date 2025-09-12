using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private static Transform _cam;
    public bool isBillbord = false;
    public bool reverse = false;
 //   private static Vector3 rotateValue = new Vector3(0, 180, 0);
    private void Awake()
    {
        if(_cam == null)
        {
            _cam = Camera.main.transform;
        }
    }
    private void OnEnable()
    {
        Caculate();
    }
    void Update()
    {
        Caculate();
    }
    private void Caculate()
    {
        if (isBillbord)
        {
            transform.forward = _cam.forward;
            if (!reverse)
            {
                transform.Rotate(transform.up, 180, Space.World);
            }
        }
        else
        {
            transform.LookAt(_cam);
            if (reverse)
            {
                transform.Rotate(transform.up, 180, Space.World);
            }
        }
    }
}
