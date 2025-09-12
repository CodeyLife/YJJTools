using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Yjj_AnimationTool : MonoBehaviour
{
    public List<string> animations = new List<string>();
    private Animator _controll;
    public Animator Controll { get
        {
            if(_controll == null)
            {
                _controll = GetComponent<Animator>();
            }
            return _controll;
        }set => _controll = value; }
#if UNITY_EDITOR
    [Button]
    private void GetAnimations()
    {
        animations.Clear();
        var state = ((UnityEditor.Animations.AnimatorController)Controll.runtimeAnimatorController).layers[0].stateMachine;
        
        foreach(var a in state.states)
        {
            animations.Add(a.state.name);
        }
    }
#endif
    /// <summary>
    /// 播放动画
    /// </summary>
    /// <param name="index"></param>
    public void PlayAnimation(int index)
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        Controll.Play(animations[index]);
    }

    /// <summary>
    /// 播放到动画最后一帧
    /// </summary>
    /// <param name="index"></param>
    public void SetEndAnimation(int index)
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
        Controll.Play(animations[index],0,1);
       // Controll.
    }
}
