using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class Animation_Alpha : Animation_Base
{
    private CanvasGroup _group;

    public CanvasGroup Group { get
        {
            if(_group == null)
            {
                _group = GetComponent<CanvasGroup>();
            }
            return _group;
        }
        set => _group = value; }

    public override void FadeIn()
    {
        base.FadeIn();
        this.FadeIn(during, (t) =>
         {
             Group.alpha = t;
         });
    }
    public override void FadeOut()
    {
        base.FadeOut();
        this.FadeOut(during, (t) =>
         {
             Group.alpha = t;
         }, () => gameObject.SetActive(false));
    }
}
