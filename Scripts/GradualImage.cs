using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("_YjjTool/UI渐变组件")]
public class GradualImage : MonoBehaviour
{
    [OnValueChanged("SetShader")]
    public Color beginColor = Color.white;
    [OnValueChanged("SetShader")]
    public Color endColor = Color.white;
    [OnValueChanged("SetMaterial")]
    public Material material;
    private Material _mat;

    public Material Mat { get
        {
            if(_mat == null)
            {
                _mat = GetComponent<Image>().material;
            }
            return _mat;
        }
        set => _mat = value; }
    /// <summary>
    /// 改变渐变色,如果直接改颜色会没有提交给材质
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    public void SetColor(Color? begin = null, Color? end = null)
    {
        if (begin != null)
        {
            beginColor = begin.Value;
        }
        if (end != null)
        {
            endColor = end.Value;
        }
        SetShader();
    }
    private void SetShader()
    {
        Mat.SetColor("_Color", beginColor);
        Mat.SetColor("_EndColor", endColor);
    }
#if UNITY_EDITOR
    [OnInspectorInit]
    private void InspectorInit()
    {
        if (material == null)
        {
            var guid = UnityEditor.AssetDatabase.FindAssets("UI渐变材质")[0];
            material = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
        }
        GetComponent<Image>().material = material;
    }
    private void SetMaterial()
    {
        GetComponent<Image>().material = material;
    }
#endif
}
