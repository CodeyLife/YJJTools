using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Sirenix.Utilities;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class YjjUtility
{

    /// <summary>
    /// 用于从0到1的动画
    /// </summary>
    /// <param name="totalTime">动画时长</param>
    /// <param name="action">根据t执行的插值行为</param>
    /// <param name="endAction">动画结束时执行的行为</param>
    /// <param name="curve">动画曲线</param>
    /// <returns></returns>
    public static IEnumerator FadeIn(float totalTime, Action<float> action, Action endAction = null, AnimationCurve curve = null)
    {
        if(totalTime == 0)
        {
            action?.Invoke(1);
        }
        else
        {
            float current = 0;
            float value;
            action?.Invoke(0);
            yield return null;
            while (current < totalTime)
            {
                current += Time.deltaTime;
                value = current / totalTime;
                value = Mathf.Clamp(value, 0, 1);
                if (curve != null)
                {
                    value = curve.Evaluate(value);
                }
                action?.Invoke(value);
                yield return null;
            }
        }
        endAction?.Invoke();
    }
    public static Coroutine FadeIn(this MonoBehaviour mono, float totalTime, Action<float> action, Action endAction = null, AnimationCurve curve = null)
    {
        var cor =  mono.StartCoroutine(FadeIn(totalTime, action, endAction, curve));
        return cor;
    }
    /// <summary>
    /// 用于从1到0的动画
    /// </summary>
    /// <param name="totalTime">动画时长</param>
    /// <param name="action">根据t执行的插值行为</param>
    /// <param name="endAction">动画结束时执行的行为</param>
    /// <param name="curve">动画曲线</param>
    /// <returns></returns>
    public static IEnumerator FadeOut(float totalTime, Action<float> action, Action endAction = null, AnimationCurve curve = null)
    {
        float current = totalTime;
        float value;
        while (current > 0)
        {
            current -= Time.deltaTime;
            value = current / totalTime;
            value = Mathf.Clamp(value, 0, 1);
            if (curve != null)
            {
                value = curve.Evaluate(value);
            }
            action?.Invoke(value);
            yield return null;
        }
        endAction?.Invoke();
    }
    public static Coroutine FadeOut(this MonoBehaviour mono, float totalTime, Action<float> action, Action endAction = null, AnimationCurve curve = null)
    {
        return mono.StartCoroutine(FadeOut(totalTime, action, endAction, curve));
    }
    /// <summary>
    /// 延迟一帧执行
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerator DeLay(Action action)
    {
        yield return null;
        action?.Invoke();
    }
    public static Coroutine Delay(this MonoBehaviour mono,Action action)
    {
        return mono.StartCoroutine(DeLay(action));
    }
    public static IEnumerator DeLay(float time,Action action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }
    public static Coroutine Delay(this MonoBehaviour mono,float time, Action action)
    {
        return mono.StartCoroutine(DeLay(time,action));
    }
    public static IEnumerator DeLay(Func<bool> func,Action action)
    {
        yield return null;
        while (!func.Invoke())
        {
            yield return null;
        }
        action?.Invoke();
    }
    public static IEnumerator DelayWhileWithTimeOut(float timer, Func<bool> func, Action action)
    {
        yield return null;
        while (!func.Invoke())
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                yield break;
            }
            yield return null;
        }
        action?.Invoke();
    }
    public static Coroutine DelayWhile(this MonoBehaviour mono, Func<bool> func, Action action)
    {
        return mono.StartCoroutine(DeLay(func, action));
    }

    /// <summary>
    /// 等到fucn为true执行action，在timer计时结束跳出
    /// </summary>
    /// <param name="mono"></param>
    /// <param name="timer"></param>
    /// <param name="func"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Coroutine DelayWhileWithTimeout(this MonoBehaviour mono,float timer, Func<bool> func, Action action)
    {
        return mono.StartCoroutine(DelayWhileWithTimeOut(timer,func, action));
    }

    public static Coroutine DoWhile(this MonoBehaviour mono,Func<bool> func, Action action)
    {
        return mono.StartCoroutine(DoWhile(func, action));
    }
    private static IEnumerator DoWhile(Func<bool> func, Action action)
    {
        while (func.Invoke())
        {
            action?.Invoke();
            yield return null;
        }
    }
    
    /// <summary>
    /// 在协程之后执行
    /// </summary>
    /// <param name="mono"></param>
    /// <param name="cor"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static Coroutine Delay(this MonoBehaviour mono, IEnumerator cor,Action action)
    {
        return mono.StartCoroutine(InvokeAfterIenmerator(mono, cor, action));
    }

    private static IEnumerator InvokeAfterIenmerator(MonoBehaviour mono,IEnumerator cor, Action action)
    {
        yield return mono.StartCoroutine(cor);
        action?.Invoke();
    }

#if UNITY_EDITOR
    private static Stopwatch sw;
#endif
    public static void InspectObject(object obj)
    {
#if UNITY_EDITOR
        Debug.Log(obj);
        Sirenix.OdinInspector.Editor.OdinEditorWindow window = Sirenix.OdinInspector.Editor.OdinEditorWindow.InspectObject(obj);
        window.position = Sirenix.Utilities.Editor.GUIHelper.GetEditorWindowRect().AlignCenter(600, 1000);
#endif
    }
    public static IEnumerator DelayFrame(int frame,Action action)
    {
        for(int i = 0; i < frame; i++)
        {
            yield return null;
        }
        action?.Invoke();
    }
    public  static Coroutine DelayFrame(this MonoBehaviour mono,int frame, Action action)
    {
        return mono.StartCoroutine(DelayFrame(frame, action));
    }
    public static void BeginSample()
    {
#if UNITY_EDITOR
        sw = new Stopwatch();
        sw.Start();
#endif
    }
    public static void EndSample(string actionName = "")
    {
#if UNITY_EDITOR
        sw.Stop();
        Debug.Log($"{actionName}:{sw.ElapsedMilliseconds}毫秒");
#endif
    }

    #region 随机

    /// <summary>
    /// 概率
    /// </summary>
    /// <param name="probability">0到1的概率范围</param>
    /// <returns></returns>
    public static bool Probability(float chance)
    {
        return Random.Range(0, 1f) < chance;
    }
    #endregion
    public static float SmoothLerp(float min,float max,float current)
    {
        return (current - min) / (max - min);
    }

    public static void Log(string str, Color? c)
    {
#if UNITY_EDITOR
        if (c.HasValue)
        {
             var html = ColorUtility.ToHtmlStringRGBA(c.Value);
            str = $"<color={html}>str";
        }
#else
        Debug.Log(str);
#endif
    }

    /// <summary>
    /// 通过序列化深度复制
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T DeepCopyUsingBinarySerialization<T>(T obj)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;
            return (T)formatter.Deserialize(ms);
        }
    }

}
