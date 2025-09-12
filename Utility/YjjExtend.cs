using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class YjjExtend
{
    #region Transform
    /// <summary>
    /// 删除所有子节点
    /// </summary>
    /// <param name="parent"></param>
    public static void DelateAllChild(this Transform parent)
    {
        while (parent.childCount > 0)
        {
            GameObject.DestroyImmediate(parent.GetChild(0).gameObject);
        }
    }

    /// <summary>
    /// 最多只有count个child，多余的都删除
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="index"></param>
    public static void DelateChildByCount(this Transform parent, int count)
    {
        if (Application.isPlaying)
        {
            for (int i = count; i < parent.childCount; i++)
            {
                GameObject.Destroy(parent.GetChild(i).gameObject);
            }
        }
        else
        {
            while (parent.childCount > count)
            {
                GameObject.DestroyImmediate(parent.GetChild(count).gameObject);
            }
        }
    }

    /// <summary>
    /// 获取或者创建子节点
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="str"></param>
    /// <param name="components"></param>
    /// <returns></returns>
    public static Transform GetOrCreatUIChild(this Transform parent, string str, params Type[] components)
    {
        var t = parent.Find(str);
        if (t == null)
        {
            t = new GameObject(str, components).transform;
            t.SetParent(parent);
            t.localScale = Vector3.one;
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
        }
        return t;
    }
    public static Transform GetOrCreatUIChild(this Transform parent, string str, Action<Transform> initAction = null, params Type[] components)
    {
        var t = parent.Find(str);
        if (t == null)
        {
            t = new GameObject(str, components).transform;
            t.SetParent(parent);
            t.localScale = Vector3.one;
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
            initAction?.Invoke(t);
        }
        return t;
    }

    public static Transform GetOrCreatUIChild<T>(this Transform parent, int index,string name = "newGo", Action<T> CreatNewAction = null, params Type[] components) where T :Component
    {
        Transform t;
        if (parent.childCount > index)
        {
            t = parent.GetChild(index);
            t.name = name;
        }
        else
        {
            t = new GameObject(name, components).transform;
            t.SetParent(parent);
            t.localScale = Vector3.one;
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
            var go = t.gameObject.AddComponent<T>();
            CreatNewAction?.Invoke(go);

        }
        return t;
    }

    public static Transform GetOrCreatChild(this Transform parent,string str,GameObject prefab,Action<Transform> initAction = null)
    {
        var t = parent.Find(str);
        if(t == null)
        {
            t = GameObject.Instantiate(prefab, parent).transform;
            t.name = str;
            initAction?.Invoke(t);
        }
        return t;
    }

    public static Transform GetOrCreatUIChild(this Transform parent, string str, bool setAnchor, params Type[] components)
    {
        var t = parent.Find(str);
        if (t == null)
        {
            t = new GameObject(str, components).transform;
            t.SetParent(parent);
            t.localScale = Vector3.one;
            if (setAnchor)
            {
                var rect = t.GetOrAddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.zero;
                rect.pivot = Vector2.zero;
                rect.anchoredPosition = Vector2.zero;
            }
            t.localPosition = Vector3.zero;
        }
        return t;
    }

    public static void LoopChild(this Transform parent,bool deepChild,Action<Transform> action)
    {
        foreach (Transform t in parent)
        {
            ChildAction(t, action,deepChild);
        }
    }
    public static IEnumerable<Transform> Childs(this Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            yield return parent.GetChild(i);
        }
    }
    private static void ChildAction(Transform t,Action<Transform> action,bool deepChild)
    {
        action?.Invoke(t);
        if (deepChild)
        {
            foreach(Transform child in t)
            {
                ChildAction(child, action, deepChild);
            }
        }
    }

    public static T GetOrCreatUIChild<T>(this Transform parent, string str,Action<T> CreatNewAction = null,params Type[] components) where T:Component
    {
        var t = parent.Find(str);
        T go = null;
        if (t == null)
        {
            t = new GameObject(str,components).transform;
            t.SetParent(parent);
            t.localPosition = Vector3.zero;
            t.localScale = Vector3.one;
            t.localEulerAngles = Vector3.zero;
            go = t.gameObject.GetOrAddComponent<T>();
            CreatNewAction?.Invoke(go);
        }
        else
        {
            go = t.GetComponent<T>();
        }
        return go;
    }
    #endregion

    #region RectTransform
    public static RectTransform rectTransform(this Component r)
    {
        return r.GetComponent<RectTransform>();
    }

    /// <summary>
    /// 填满父节点
    /// </summary>
    /// <param name="rect"></param>
    public static void FullByParent(this RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one * 0.5f;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }
    #endregion

    public static void DestroyByRuntimeType(this GameObject obj)
    {
        if (Application.isPlaying)
        {
            GameObject.Destroy(obj);
        }
        else
        {
            GameObject.DestroyImmediate(obj);
        }
    }
    public static T GetOrAddComponent<T>(this GameObject obj,Action<T> addAction = null) where T : Component
    {
        T t = obj.GetComponent<T>();
        if (t != null)
        {
            return t;
        }
        else
        {
            t = obj.AddComponent<T>();
            addAction?.Invoke(t);
            return t;
        }
    }
    public static T GetOrAddComponent<T>(this Transform obj, Action<T> addAction = null) where T : Component
    {
        T t = obj.gameObject.GetComponent<T>();
        if (t != null)
        {
            return t;
        }
        else
        {
            t = obj.gameObject.AddComponent<T>();
            addAction?.Invoke(t);
            return t;
        }
    }
    /// <summary>
    /// 将浮点数转成限制长度的字符串
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="floatLenght">最多小数位数</param>
    /// <returns></returns>
    public static string ToLimitString(this float value,int floatLenght)
    {
        string str = value.ToString("G");
        string havePoint = floatLenght == 0 ?"": ".?";
        string patten = $"^-?\\d+{havePoint}\\d{{0,{floatLenght}}}";
        str = Regex.Match(str, patten).Value;
        return str;
    }

    /// <summary>
    /// 将浮点数转成限制长度的字符串
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="floatLenght">最多小数位数</param>
    /// <returns></returns>
    public static string ToAutoLimitString(this float value, int floatLenght)
    {
        string str = value.ToString("0.################");
        
        int firstNonZero = str.IndexOfAny(new[] { '1', '2', '3', '4', '5', '6', '7', '8', '9' });
        if (firstNonZero == -1) return "0";

        int decimalPointIndex = str.IndexOf('.');
        
        // 如果没有小数点，直接返回整数部分
        if (decimalPointIndex < 0)
        {
            return str;
        }
        
        // 如果floatLength为0，返回整数部分（去掉小数点）
        if (floatLenght == 0)
        {
            return str.Substring(0, decimalPointIndex);
        }
        
        int keepDecimals;
        if (firstNonZero <= decimalPointIndex)
        {
            // 第一个非零数字在小数点前，保留指定的小数位数
            keepDecimals = floatLenght;
        }
        else
        {
            // 第一个非零数字在小数点后，计算有效数字位置
            int significantPosition = firstNonZero - decimalPointIndex - 1;
            keepDecimals = significantPosition + floatLenght;
        }
        
        int maxLength = decimalPointIndex + keepDecimals + 1;
        return str.Substring(0, Math.Min(str.Length, maxLength));
    }

    #region Vector3

    /// <summary>
    /// 只改变x值
    /// </summary>
    /// <param name="v"></param>
    /// <param name="x"></param>
    public static void SetX(this ref Vector3 v,float x)
    {
        v.Set(x, v.y, v.z);
    }
    /// <summary>
    /// 只改变y值
    /// </summary>
    /// <param name="v"></param>
    /// <param name="y"></param>
    public static void SetY(this ref Vector3 v, float y)
    {
        v.Set(v.x, y, v.z);
    }
    /// <summary>
    /// 只改变z值
    /// </summary>
    /// <param name="v"></param>
    /// <param name="z"></param>
    public static void SetZ(this ref Vector3 v, float z)
    {
        v.Set(v.x, v.y, z);
    }
    #endregion
    /// <summary>
    /// 设置颜色透明度
    /// </summary>
    /// <param name="v"></param>
    /// <param name="a"></param>
    public static void ChangeAlpha(this ref Color v, float a)
    {
        v = new Color(v.r, v.g, v.b, a);
    }
    /// <summary>
    /// 获取更改透明度后的颜色
    /// </summary>
    /// <param name="v"></param>
    /// <param name="a"></param>
    public static Color SetAlpha(this Color v, float a)
    {
        v = new Color(v.r, v.g, v.b, a);
        return v;
    }
    #region SetText
    public static void SetText(this UnityEngine.UI.Text t, string msg)
    {
        t.text = msg;
        var roll = t.GetComponent<Roll>();
        roll?.InitAnimation();
    }
    public static void SetProText(this TextMeshProUGUI t, string msg)
    {
        t.text = msg;
        var roll = t.GetComponent<Roll>();
        roll?.InitAnimation();
        var animation = t.GetComponent<Yjj_ValueAnimationSingle>();
        animation?.SetData();
    }
    /// <summary>
    /// 设置Roll或者Textmeshpro或者text的文本
    /// </summary>
    /// <param name="t"></param>
    /// <param name="msg"></param>
    public static void SetText(this Transform t,string msg)
    {
        if (t == null) return;
        var roll = t.GetComponentInChildren<Roll>();
        if (roll != null)
        {
            roll.SetData(msg);
        }
        else
        {
            var pro = t.GetComponent<TextMeshProUGUI>();
            if(pro != null)
            {
                pro.text = msg;
                pro.GetComponent<Yjj_ValueAnimationSingle>()?.SetData();
            }
            else
            {
                var text = t.GetComponent<Text>();
                if(text!=null) text.text = msg;
            }
        }
    }
    public static void SetText(this Transform t,ValueType value)
    {
        t.SetText(value.ToString());
    }
    #endregion
    public static float ParseAnyway(this string s)
    {
        float value = 0;
        float.TryParse(s, out value);
        return value;
    }
    #region SetData   
    /// <summary>
    /// 根据字段名找寻给定位置下的同名物体，并设置文本为该值
    /// </summary>
    /// <param name="t"></param>
    /// <param name="info"></param>
    /// <param name="obj"></param>
    private static void SetData(this Transform t, FieldInfo info, System.Object obj)
    {
        string str = info.Name;
        Transform tt = t.Find(str);
        var value = info.GetValue(obj);
        //Debug.Log(tt);

        string data = value == null?"": value.ToString();
        if (tt != null)
        {
            var roll = tt.GetComponentInChildren<Roll>();
            if (roll != null)
            {
                roll.SetData(data);
                return;
            }
            var text = tt.GetComponent<UnityEngine.UI.Text>();
            if (text != null)
            {
                text.text = data;
                return;
            }
            var pro = tt.GetComponent<TextMeshProUGUI>();
            if (pro != null)
            {
                pro.text = data;
                return;
            }
            text = tt.GetComponentInChildren<UnityEngine.UI.Text>();
            if (text != null)
            {
                text.text = data;
                return;
            }
            pro = tt.GetComponentInChildren<TextMeshProUGUI>();
            if (pro != null)
            {
                pro.text = data;
                return;
            }
        }
    }
    private static void SetData(this Transform t, PropertyInfo info, System.Object obj)
    {
        string str = info.Name;
        Transform tt = t.Find(str);
        var value = info.GetValue(obj);
        //Debug.Log(tt);
        if (value == null)
        {
            return;
        }
        string data = value.ToString();
        if (tt != null)
        {
            var roll = tt.GetComponentInChildren<Roll>();
            if (roll != null)
            {
                roll.SetData(data);
                return;
            }
            var text = tt.GetComponent<UnityEngine.UI.Text>();
            if (text != null)
            {
                text.text = data;
                return;
            }
            var pro = tt.GetComponent<TextMeshProUGUI>();
            if (pro != null)
            {
                pro.text = data;
                return;
            }
            text = tt.GetComponentInChildren<UnityEngine.UI.Text>();
            if (text != null)
            {
                text.text = data;
                return;
            }
            pro = tt.GetComponentInChildren<TextMeshProUGUI>();
            if (pro != null)
            {
                pro.text = data;
                return;
            }
        }
    }
    /// <summary>
    /// 根据类型设置文本值
    /// </summary>
    /// <param name="t"></param>
    /// <param name="type"></param>
    /// <param name="obj"></param>
    public static void SetData(this Transform t, Type type, System.Object obj)
    {
        var fields = type.GetFields();
        for (int i = 0; i < fields.Length; i++)
        {
            //Debug.Log(fields[i]);
            t.SetData(fields[i], obj);
        }
        var propertys = type.GetProperties();
        for (int i = 0; i < propertys.Length; i++)
        {
            t.SetData(propertys[i], obj);
        }
    }
    #endregion
    #region UI重叠相关
    public static Rect WorldRect(this RectTransform rectTransform)
    {
        Vector2 sizeDelta = rectTransform.sizeDelta;
        float rectTransformWidth = sizeDelta.x * rectTransform.lossyScale.x;
        float rectTransformHeight = sizeDelta.y * rectTransform.lossyScale.y;

        Vector3 position = rectTransform.position;
        return new Rect(position.x + rectTransformWidth * rectTransform.pivot.x, position.y - rectTransformHeight * rectTransform.pivot.y, rectTransformWidth, rectTransformHeight);
    }
    /// <summary>
    /// 检查UI是否重叠
    /// </summary>
    /// <param name="r1"></param>
    /// <param name="r2"></param>
    /// <returns></returns>
    public static bool CheckOverlap(this RectTransform r1,RectTransform r2)
    {
        return r1.WorldRect().Overlaps(r2.WorldRect());
    }
    #endregion
    /// <summary>
    /// 小数转为百分数
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static string ToPercet(this float f)
    {
        f *= 100;
        string str = $"{f}%";
        return str;
    }

    /// <summary>
    /// 深拷贝到目标(不包含list等类型)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="target"></param>
    public static void DeepCopyByRefelect<T>(this T obj,T target)
    {
        var t = obj.GetType();
        if (target == null)
        {
            target = (T)Activator.CreateInstance(t);
        }

        var fields = t.GetFields();
        foreach (var field in fields)
        {
            if (field.FieldType.IsGenericType) continue;
            try
            {
                field.SetValue(target, field.GetValue(obj));
            }
            catch { }
        }
    }
    /// <summary>
    /// 把时间转为时间戳
    /// </summary>
    /// <param name="time"></param>
    /// 是否为十位时间戳
    /// <param name="ten"></param>
    public static long ToTimeStamp(this DateTime time, bool isTen = true)
    {
        var ts = time - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        long result;
        if (isTen)
        {
            result = Convert.ToInt64(ts.TotalSeconds);
        }
        else
        {
            result = Convert.ToInt64(ts.TotalMilliseconds);
        }
        return result;
    }


    /// <summary>
    /// 带动画得物体开关
    /// </summary>
    /// <param name="go"></param>
    /// <param name="active"></param>
    public static void SetActiveWithAnimation(this GameObject go,bool active)
    {
        if(go.TryGetComponent<Animation_Base>(out var animation))
        {
            if (active)
            {
                animation.FadeIn();
            }
            else
            {
                animation.FadeOut();
            }
        }
        else
        {
            go.SetActive(active);
        }
    }

    public static void FocusWithMesh(this Camera cam, Transform root,float distance = 0)
    {
        Bounds overallBounds = new Bounds(root.position, Vector3.zero);

        foreach (Renderer childRenderer in root.GetComponentsInChildren<Renderer>())
        {
            overallBounds.Encapsulate(childRenderer.bounds);
        }
        float frustumHeight = overallBounds.size.magnitude;
        frustumHeight = GetFrustumDistanceFromHeight(cam, frustumHeight);
        float frustumDistance = frustumHeight;
        if (frustumDistance < cam.nearClipPlane) frustumDistance = cam.nearClipPlane;
        Vector3 cameraPosition = overallBounds.center - cam.transform.forward * (frustumDistance + distance);
        cam.transform.position = cameraPosition;

    }
    private static float GetFrustumDistanceFromHeight(Camera camera, float frustumHeight)
    {
        return (frustumHeight * 0.5f) / Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
    }


    #region 集合扩展
    /// <summary>
    /// array 遍历执行
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="arr"></param>
    /// <param name="action"></param>
    public static void Foreach<T>(this T[] arr, Action<T> action)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            action?.Invoke(arr[i]);
        }
    }

    public static void Foreach<T>(this IList<T> list,Action<T,int> action)
    {
        for (int i = 0; i < list.Count; i++)
        {
            action?.Invoke(list[i],i);
        }
    }
    public static void Add<T>(this List<T> list, params T[] args)
    {
        list.AddRange(args);
    }
    #endregion
}

