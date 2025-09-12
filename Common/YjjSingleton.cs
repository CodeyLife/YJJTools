using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class YjjSingleton<T> : MonoBehaviour where T:YjjSingleton<T>
{
    protected static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                //if (_instance != null)
                //{
                //    DontDestroyOnLoad(_instance.gameObject);
                //}
            }
            return _instance;
        }
        set => _instance = value;
    }
    protected virtual void Awake()
    {
        if(_instance == null)
        {
            _instance = (T)this;
          //  DontDestroyOnLoad(_instance.gameObject);
        }
    }
}

