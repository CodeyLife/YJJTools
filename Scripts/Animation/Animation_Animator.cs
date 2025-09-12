using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animation_Animator : Animation_Base
{
    private Animator _controller;

    public string fadeInStateName;
    public string fadeOutStateName;

    public Animator Controller { get
        {
            if(_controller == null)
            {
                _controller = GetComponent<Animator>();
            }
            return _controller;
        }
        set => _controller = value; }

    [Button]
    public override void FadeIn()
    {
        base.FadeIn();
        Controller.Play(fadeInStateName);

    }

    [Button]
    public override void FadeOut()
    {
        base.FadeOut();
        Controller.Play(fadeOutStateName);
    }
}
