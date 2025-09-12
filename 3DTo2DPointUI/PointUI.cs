using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace YJJTool
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class PointUI : Graphic
    {
        #region 枚举
        public enum Aligin
        {
            居左对齐,
            居右对齐,
            居中对齐
        }
        #endregion
        [LabelText("根据距离缩放"), OnValueChanged("ScaleChange")]
        public bool scaleWithDistance = false;
        private void ScaleChange()
        {
            if (!scaleWithDistance)
            {
                transform.localScale = Vector3.one;
            }
        }
        [ShowIf("scaleWithDistance")]
        public float perfectDistance = 100;
        //[InfoBox("通过快捷键<color=red>Alt + G</color>可以快速调整image位置 按<color=red>G</color>取消Inspector锁定 在Inpector调整请点击<color=red>手动调整</color>按钮")]
        [Header("3D场景的位置")]
        public Transform point;
        public List<Vector2> offsets = new List<Vector2>();
        [LabelText("画线")]
        public bool drawLine = true;
        [ShowIf("drawLine")]
        public float width = 0.3f;
        [ShowIf("drawLine")]
        public Color lineColor = Color.white;
        [EnumToggleButtons]
        public Aligin aligin = Aligin.居右对齐;
        [LabelText("显示的内容")]
        public RectTransform controllImage;
        [Header("内容基于第几个数据点偏移")]
        public int imageOffsetIndex = 1;
        [LabelText("内容偏移值")]
        public Vector2 imageOffset = Vector2.zero;
        [LabelText("自动计算横轴长度")]
        public bool autoLength = true;
        [Header("指向点image")]
        public Image pointImage;

        [ShowInInspector, ReadOnly]
        private Vector2 scale = Vector2.one;
        private static Camera _uiCamera;
        //动画
        public bool openAnimation = false;
        [ShowIf("openAnimation")]
        public float animationTime = 2f;
        private float animationT = 1;
        private List<Vector2> _temps = new List<Vector2>();
        private CanvasGroup _canvasGroup;
        public static Camera UiCamera
        {
            get
            {
                if (_uiCamera == null)
                {
                    _uiCamera = Camera.main;
                }
                return _uiCamera;
            }
            set => _uiCamera = value;
        }

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = transform.GetOrAddComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
            set => _canvasGroup = value;
        }

        public List<Vector2> Temps { get => _temps; set => _temps = value; }

        protected override void Awake()
        {
            base.Awake();

        }
        protected override void Start()
        {
            base.Start();
            SetScale();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            SetGraph();
            if (Application.isPlaying && openAnimation)
            {
                this.FadeIn(animationTime, (t) =>
                {
                    animationT = t;
                    controllImage.GetOrAddComponent<CanvasGroup>().alpha = t;
                    SetVerticesDirty();
                });
            }
            UpdateGraph();
        }


        //映射比例
        private void SetScale()
        {
            if (GetCanvasRect(out var rect))
            {
                //Debug.Log($"{Screen.width},{Screen.height}");
                scale = rect.sizeDelta / new Vector2(Screen.width, Screen.height);
            }
            else
            {
                Debug.Log("没有获取到point缩放", gameObject);
            }
        }
        private bool GetCanvasRect(out RectTransform resultRect)
        {
            resultRect = null;
            var tranparent = transform.parent;
            if (tranparent == null) return false;
            while (tranparent.GetComponent<Canvas>() == null)
            {
                if (tranparent.parent == null)
                {
                    return false;
                }
                tranparent = tranparent.parent;
            }
            resultRect = tranparent.rectTransform();
            return true;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            // base.OnPopulateMesh(vh);
            vh.Clear();
            if (drawLine)
            {
                Yjj_ChartUtility.DrawLineSmooth(vh, Temps, width, lineColor);
            }
        }

        private void Update()
        {
            UpdateGraph();
        }

        protected void UpdateGraph()
        {
            if (point == null) return;
            //判断定位点是否在相机后方
            var dir = point.position - UiCamera.transform.position;
            if (Application.isPlaying)
            {
                if (Vector3.Dot(UiCamera.transform.forward, dir) <= 0)
                {
                    CanvasGroup.alpha = 0;
                    return;
                }
                else
                {
                    if (CanvasGroup.alpha == 0)
                    {
                        CanvasGroup.alpha = 1;
                    }
                }
            }
            else
            {
                CanvasGroup.alpha = 1;
            }

            var ps = 0f;
            Vector2 p = UiCamera.WorldToScreenPoint(point.position);
            if (scaleWithDistance)
            {
                ps = perfectDistance / Vector3.Distance(Camera.main.transform.position, point.position);
                rectTransform.localScale = new Vector3(ps, ps, ps);
                p /= ps;
            }
            p *= scale;
            //更新指向标记点
            if (pointImage != null)
            {
                pointImage.rectTransform.anchoredPosition = p;
            }
            Temps.Clear();
            Temps.Add(p);  //第一个点是原点
            for (int i = 0; i < offsets.Count; i++)
            {
                Vector2 pp = Vector2.zero;
                if (i > 0)
                {
                    if (autoLength && i == imageOffsetIndex)
                    {
                        //如果是自动长度  添加自动长度的向量
                        pp = Temps[i] + new Vector2(l, 0);
                    }
                    else
                    {
                        var add = aligin == Aligin.居右对齐 ? offsets[i] : new Vector2(offsets[i].x * -1, offsets[i].y);
                        pp += Temps[i] + add;
                    }
                }
                else
                {
                    pp = p + offsets[i];
                }
                Temps.Add(pp);
            }
            if (animationT < 1)
            {
                var arr = new List<Vector2>();
                arr.Add(Temps[0]);
                List<float> lengths = new List<float>();
                for (int i = 1; i < Temps.Count; i++)
                {
                    lengths.Add(Vector2.Distance(Temps[i], Temps[i - 1]));
                }
                float all = lengths.Sum();
                float t = 0; //已经绘制的比例
                for (int i = 0; i < lengths.Count; i++)
                {
                    var ct = lengths[i] / all; //当前数据所占比例
                    if (t + ct < animationT)
                    {
                        //如果当前比例+之后 还是小于动画占比 直接添加这个点
                        arr.Add(Temps[i + 1]);
                    }
                    else
                    {
                        var percent = animationT - t;
                        var tempPos = Vector2.Lerp(Temps[i], Temps[i + 1], percent * all / lengths[i]);
                        arr.Add(tempPos);
                        Temps = arr;
                        break;
                    }
                    t += ct;
                }
            }
            if (aligin == Aligin.居右对齐)
            {
                var index = imageOffsetIndex >= Temps.Count ? Temps.Count - 1 : imageOffsetIndex;
                controllImage.anchoredPosition = Temps[index] + imageOffset;
            }
            else
            {
                var index = imageOffsetIndex >= Temps.Count ? Temps.Count - 1 : imageOffsetIndex;
                controllImage.anchoredPosition = Temps[index] + new Vector2(imageOffset.x * -1, imageOffset.y);
            }
            SetVerticesDirty();
        }

        /// <summary>
        /// 自动长度
        /// </summary>
        private float l;
        public void SetGraph()
        {
            if (point == null) return;
            Vector2 p = UiCamera.WorldToScreenPoint(point.position);
            p *= scale;
            if (pointImage != null)
            {
                pointImage.rectTransform.anchoredPosition = p;
            }
            Temps.Clear();
            Temps.Add(p);
            if (aligin == Aligin.居右对齐)
            {
                controllImage.pivot = Vector2.zero;
            }
            else if (aligin == Aligin.居左对齐)
            {
                controllImage.pivot = new Vector2(1, 0);
            }
            else
            {
                controllImage.pivot = new Vector2(0.5f, 0);
            }
            for (int i = 0; i < offsets.Count; i++)
            {
                Vector2 pp = Vector2.zero;
                if (i > 0)
                {
                    //自动长度
                    if (autoLength && i == imageOffsetIndex)
                    {
                        var text = controllImage.GetComponent<Text>();
                        var tpro = controllImage.GetComponent<TextMeshProUGUI>();
                        if (text != null)
                        {
                            l = text.preferredWidth;
                        }
                        else if (tpro != null)
                        {
                            l = tpro.fontSize * tpro.text.Length;
                        }
                        else
                        {
                            l = controllImage.rectTransform().sizeDelta.x;
                        }
                        l = aligin == Aligin.居右对齐 ? l + imageOffset.x * 2 : -(l + imageOffset.x * 2);
                        pp = Temps[i] + new Vector2(l, 0);
                    }
                    else
                    {
                        var add = aligin == Aligin.居右对齐 ? offsets[i] : new Vector2(offsets[i].x * -1, offsets[i].y);
                        pp += Temps[i] + add;
                    }
                }
                else
                {
                    pp = p + offsets[i];
                }
                Temps.Add(pp);
            }

            if (aligin == Aligin.居右对齐)
            {
                controllImage.anchoredPosition = Temps[imageOffsetIndex] + imageOffset;
            }
            else
            {
                controllImage.anchoredPosition = Temps[imageOffsetIndex] + new Vector2(imageOffset.x * -1, imageOffset.y);
            }
            SetVerticesDirty();
        }
#if UNITY_EDITOR
        #region Inspector
        [OnInspectorInit]
        public void Init()
        {
            if (transform.parent != null && transform.parent.GetComponent<Canvas>() == null)
            {
                if (GetCanvasRect(out var canvasRect))
                {
                    var rect = transform.parent.GetComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.zero;
                    rect.pivot = Vector2.one * 0.5f;
                    rect.sizeDelta = canvasRect.sizeDelta;
                    rect.anchoredPosition = rect.sizeDelta * 0.5f;
                    UnityEditor.EditorUtility.SetDirty(rect);
                }
            }
            transform.rectTransform().anchorMin = Vector2.zero;
            transform.rectTransform().anchorMax = Vector2.zero;
            transform.rectTransform().anchoredPosition = Vector2.zero;
            UnityEditor.EditorUtility.SetDirty(transform.rectTransform());
            if (controllImage == null && transform.childCount > 0)
            {
                controllImage = transform.GetChild(0).rectTransform();
                if (controllImage != null)
                {
                    controllImage.rectTransform().pivot = Vector2.zero;
                }
                UnityEditor.EditorUtility.SetDirty(controllImage.rectTransform()); ;
            }
            SetGraph();
        }
        //public void EditorEvent()
        //{
        //    UnityEditor.EditorApplication.update += EditorUpdate;
        //}
        //[OnInspectorDispose]
        //private void EditorDisable()
        //{
        //    UnityEditor.EditorApplication.update -= EditorUpdate;
        //}
        private void EditorUpdate()
        {
            Vector2 p = UiCamera.WorldToScreenPoint(point.position);
            p *= scale;
            RectTransform rect = controllImage.rectTransform();
            var delta = rect.anchoredPosition - p;
            if (aligin == Aligin.居右对齐)
            {
                delta -= imageOffset;
            }
            else
            {
                delta -= new Vector2(imageOffset.x * -1, imageOffset.y);
            }
            offsets[0] = delta;
            if (delta.x < -1)
            {
                aligin = Aligin.居左对齐;
            }
            else
            {
                aligin = Aligin.居右对齐;
            }
            SetGraph();
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
        [OnInspectorGUI]
        private void GUIchange()
        {
            if (GUI.changed)
            {
                this.Delay((() => SetGraph()));
            }
        }
        [ButtonGroup]
        [Button("关闭定点物体")]
        public void ClosePointObject()
        {
            if (point != null)
            {
                UnityEditor.Undo.RecordObject(point.gameObject, "enable");
                point.gameObject.SetActive(false);
            }
        }
        [ButtonGroup]
        [Button("开启定点物体")]
        public void OpenPoint()
        {
            if (point != null)
            {
                UnityEditor.Undo.RecordObject(point.gameObject, "enable");
                point.gameObject.SetActive(true);
            }
        }
        [Button("把定位点移动到定位点父物体下")]
        private void MovePoint(Transform moveTargetPath)
        {
            if (moveTargetPath != null)
            {
                UnityEditor.Undo.SetTransformParent(point, moveTargetPath, "ChangeParent");
                // point.SetParent(moveTargetPath);
            }
        }
        [Button("选中定位物体", ButtonSizes.Large), GUIColor(0, 1, 0)]
        private void SelectPoint()
        {
            if (point == null) return;
            UnityEditor.Selection.activeTransform = point;
        }

        #endregion
#endif
    }
}