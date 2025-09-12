using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YJJTool;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class Button3D : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerClickHandler,IPointerMoveHandler
{
    public float interactionDuration = 0.25f;
    //public Vector4 hoverLightPos = new Vector4(-5.31f, 3.47f, -6.37f, 0);
    public float ScaleFactor = 0.75f;
    RectTransform _rect;
    Material mat;
    Vector4 _orignSize;
    Vector4 _boxSize;
    Vector4 _orignLightPos;
    Vector4 _LightPos;
    Vector3 scale;


    public RectTransform Rect { get
        {
            if(_rect == null)
            {
                _rect = gameObject.GetOrAddComponent<RectTransform>();
            }
            return _rect;
        }
        set => _rect = value; }

    public Vector4 BoxSize { get => _boxSize; set

        {

            _boxSize = value;
            mat.SetVector("_BoxSize",_boxSize);
        }
    }

    public Vector3 LightPos { get => _LightPos; set
        {
            _LightPos = value;
            mat.SetVector("_LightPos", _LightPos);
        }
    }

    private void Awake()
    {
        Init();
    }

    //Matrix4x4 m;
    private void Init()
    {
        var image = GetComponent<Image>();
        image.material = new Material(image.material);
        mat = image.material;
        _orignSize = mat.GetVector("_BoxSize");
        _boxSize = _orignSize;
        _orignLightPos = mat.GetVector("_LightPos");
        _LightPos = _orignLightPos;
        var _AnchorX = mat.GetFloat("_AnchorX");
        var _AnchorY = mat.GetFloat("_AnchorY");
        var _AnchorZ = mat.GetFloat("_AnchorZ");
        scale = new Vector3(Get(_AnchorX), Get(_AnchorY),Get(_AnchorZ));

        //var center = new Vector3(_boxSize.x * _AnchorX, _boxSize.y * _AnchorY, _boxSize.z * _AnchorZ);
        //Vector3 eye = mat.GetVector("_ViewPos");
        //m = Matrix4x4.LookAt(eye, center, Vector3.up);

        float Get(float property)
        {
            return 1f - MathF.Abs(property) * (1- ScaleFactor);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //var target = new Vector4(_orignSize.x, _orignSize.y * 1.25f, _orignSize.z, 1);
        //DOTween.To(() => _boxSize,
        //                 (value) => BoxSize = value,
        //                 target,
        //                 interactionDuration);
        //DOTween.To(() => _LightPos,
        //             (value) => LightPos = value,
        //             hoverLightPos,
        //             interactionDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
         //DOTween.To(() => _boxSize,
         //                (value) => BoxSize = value,
         //                _orignSize,
         //                interactionDuration);
        DOTween.To(() => _LightPos,
                (value) => LightPos = value,
                _orignLightPos,
                0.5f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var target = new Vector4(_orignSize.x* scale.x, _orignSize.y *scale.y, _orignSize.z * scale.z, 1);
        var sequece = DOTween.Sequence();
        sequece.Append(DOTween.To(() => _boxSize,
                         (value) => {
                             BoxSize = value;
                         },
                         target,
                         interactionDuration).SetEase(Ease.OutBounce));
        sequece.Append(DOTween.To(() => _boxSize,
                     (value) =>
                     {
                         BoxSize = value;
                     },
                     _orignSize,
                     interactionDuration).SetEase(Ease.InOutQuad));

    }



    public void OnPointerMove(PointerEventData eventData)
    {
        ResetLightPos(eventData.position);
    }

    void ResetLightPos(Vector2 mouPos)
    {
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(Rect, mouPos, null, out Vector2 localPoint))
        {
           // var vec4 = mat.GetVector("_ViewPos");
            LightPos = new Vector3(localPoint.x, localPoint.y, -50); 
        }
    }
}
