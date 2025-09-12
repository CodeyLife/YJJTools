using Sirenix.OdinInspector;
using UnityEngine;

public class Chart3dLight : MonoBehaviour
{
#if UNITY_EDITOR

    [OnInspectorInit]
    void OnInit()
    {
        transform.name = "Chart3DLight";
    }
    [OnInspectorGUI]
    void OnInspectorGUI()
    {
        Shader.SetGlobalVector("_Chart3DLight", transform.position);
    }
#endif
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 10);
    }
}
