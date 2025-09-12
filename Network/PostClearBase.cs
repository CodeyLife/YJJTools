using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PostClearBase : MonoBehaviour
{
    protected List<UnityWebRequest> _requests = new List<UnityWebRequest>();
    protected void OnDisable()
    {
        foreach(var r in _requests)
        {   
            if (!r.isDone)
            {
                r.Abort();
                r.Dispose();
            }
        }
        _requests.Clear();
    }
}
