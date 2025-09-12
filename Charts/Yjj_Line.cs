using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace YJJTool
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class Yjj_Line : Graphic, IPointerEnterHandler, IPointerExitHandler, ICanvasRaycastFilter
    {
        public LineSet lineSet;
        protected List<Vector2> pos;
        protected int colorIndex = 0;

        public int count = 0;

        private PolygonCollider2D _polygonCollider;

        public PolygonCollider2D PolygonCollider
        {
            get
            {
                if (_polygonCollider == null)
                {
                    _polygonCollider = gameObject.GetOrAddComponent<PolygonCollider2D>();
                }
                return _polygonCollider;
            }
            set => _polygonCollider = value;
        }

        [OnInspectorGUI]
        private void OnInspector()
        {
            if (GUI.changed)
            {
                this.Delay(() => SetVerticesDirty());
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (lineSet == null || pos == null)
            {
                return;
            }
            //var temp = pos.Take(count).ToList();
            Yjj_ChartUtility.DrawLineSmooth(vh, pos, lineSet.width, lineSet.colors[colorIndex]);
            //var triangles = workerMesh.triangles;
            //PolygonCollider.points = triangles.Select(x => (Vector2)workerMesh.vertices[x]).ToArray();


            //var index =  MeshUtility.AlphaShapesFromDelaunay(workerMesh.vertices.Select(x=>(Vector2)x).ToArray());
            //PolygonCollider.points = index.Select(x => (Vector2)workerMesh.vertices[x]).ToArray();
        }
        protected void CheckColor(LineSet set, int index)
        {
            if (set.colors.Count <= index)
            {
                set.colors.Add(Color.white);
            }
        }
        public virtual void SetGraph(List<Vector2> arr, LineSet set, int index = 0, List<float> datas = null)
        {
            CheckColor(set, index);
            if (set.isCurve)
            {
                pos = Yjj_ChartUtility.GetCurvePosFroJob(arr, (arr.Count - 1) * set.smooth);
            }
            else
            {
                pos = arr;
            }

            colorIndex = index;

            lineSet = set;
            material = set.material;
            if (set.sprite != null)
            {
                for (int i = 0; i < pos.Count; i++)
                {
                    var image = transform.GetOrCreatUIChild("image" + i, typeof(Image)).GetComponent<Image>();
                    image.sprite = set.sprite;
                    image.rectTransform.anchorMin = Vector2.zero;
                    image.rectTransform.anchorMax = Vector2.zero;
                    image.rectTransform.anchoredPosition = pos[i];
                    image.transform.localScale = Vector3.one * set.scale;
                    image.color = set.spriteColor;
                }
            }
            if (set.font != null && datas != null)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    var text = transform.GetOrCreatUIChild("text" + i, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
                    text.font = set.font;
                    text.rectTransform.pivot = Vector2.zero;
                    text.rectTransform.anchorMax = Vector2.zero;
                    text.rectTransform.anchorMin = Vector2.zero;
                    text.fontSize = set.fontSize;
                    text.rectTransform.anchoredPosition = pos[i] + set.fontOffeset;
                    text.color = set.fontColor;
                    text.text = datas[i].ToString();
                    text.alignment = TextAlignmentOptions.Left;
                }
            }

            //OnTransformParentChanged();
            SetAllDirty();
        }
        public virtual void SetGraph(List<Vector2> arr, LineSet set, bool loseLeft, bool loseRight, int index = 0, List<float> datas = null)
        {
            CheckColor(set, index);
            if (set.isCurve)
            {
                pos = Yjj_ChartUtility.GetCurvePosFroJob(arr, arr.Count * set.smooth);
            }
            else
            {
                pos = arr;
            }
            colorIndex = index;
            lineSet = set;
            material = set.material;
            //删除
            int child = transform.childCount;
            for (int j = 0; j < child; j++)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
            //曲线
            if (datas != null && pos.Count > datas.Count + 2)
            {

                int length = datas.Count + 1;
                if (!loseLeft) length--;
                if (!loseRight) length--;
                var perLenth = (pos.Count) / length;
                if (set.sprite != null)
                {
                    for (int i = 0; i < datas.Count; i++)
                    {
                        var image = transform.GetOrCreatUIChild("image" + i, typeof(Image)).GetComponent<Image>();
                        image.sprite = set.sprite;
                        image.rectTransform.anchorMin = Vector2.zero;
                        image.rectTransform.anchorMax = Vector2.zero;
                        var posIndex = loseLeft ? i + 1 : i;
                        image.rectTransform.anchoredPosition = pos[posIndex * perLenth];
                        image.transform.localScale = Vector3.one * set.scale;
                        image.color = set.spriteColor;
                    }
                }
                if (set.font != null && datas != null)
                {
                    for (int i = 0; i < datas.Count; i++)
                    {
                        var text = transform.GetOrCreatUIChild("text" + i, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
                        text.font = set.font;
                        text.rectTransform.pivot = Vector2.zero;
                        text.rectTransform.anchorMax = Vector2.zero;
                        text.rectTransform.anchorMin = Vector2.zero;
                        text.fontSize = set.fontSize;
                        var posIndex = loseLeft ? i + 1 : i;
                        text.rectTransform.anchoredPosition = pos[posIndex * perLenth] + set.fontOffeset;
                        text.color = set.fontColor;
                        text.text = datas[i].ToString() + set.unit;
                        text.alignment = TextAlignmentOptions.Left;
                    }
                }
            }
            else
            {
                int i = loseLeft ? 1 : 0;
                int count = loseRight ? arr.Count - 1 : arr.Count;

                if (set.sprite != null)
                {
                    for (; i < count; i++)
                    {
                        var image = transform.GetOrCreatUIChild("image" + i, typeof(Image)).GetComponent<Image>();
                        image.sprite = set.sprite;
                        image.rectTransform.anchorMin = Vector2.zero;
                        image.rectTransform.anchorMax = Vector2.zero;
                        image.rectTransform.anchoredPosition = arr[i];
                        image.transform.localScale = Vector3.one * set.scale;
                        image.color = set.spriteColor;
                    }
                }

                if (set.font != null && datas != null)
                {
                    i = loseLeft ? 1 : 0;
                    for (; i < count; i++)
                    {
                        var text = transform.GetOrCreatUIChild("text" + i, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
                        text.font = set.font;
                        text.rectTransform.pivot = Vector2.zero;
                        text.rectTransform.anchorMax = Vector2.zero;
                        text.rectTransform.anchorMin = Vector2.zero;
                        text.fontSize = set.fontSize;
                        text.rectTransform.anchoredPosition = arr[i] + set.fontOffeset;
                        text.color = set.fontColor;
                        text.text = datas[i].ToString() + set.unit;
                        text.alignment = TextAlignmentOptions.Left;
                    }
                }
            }

            SetAllDirty();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log($"{gameObject.name}enter");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log($"{gameObject.name}exit");
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            bool overlap = PolygonCollider.OverlapPoint(sp);
            //Debug.Log($"开始检测点{sp}:{overlap}");
            return overlap;
        }
    }
}