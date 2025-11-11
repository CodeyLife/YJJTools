using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace YJJTool
{
    public class BarHeatMap : MonoBehaviour
    {
        public GameObject prefab;
        public Transform startGO;
        public Transform endGO;
#if UNITY_EDITOR
        [MinValue("@GetMinValue()")]
#endif
        public float barWidth;
#if UNITY_EDITOR
        [LabelText("最大box数量")]
        public int errorNum = 50000;
#endif
        [Range(0.05f, 1), Header("衰减")]
        public float damping = 0.1f;
        [Header("权重低于该阈值不生成box"), Range(0, 1)]
        public float threshold = 0.1f;
        public float maxHeight = 10;
        public float minHeight = 1f;
        [Header("box权重曲线")]
        public AnimationCurve heightCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public List<Vector3> posList = new List<Vector3>();
        public List<float> dataList = new List<float>();
        MaterialPropertyBlock block;
        private void Awake()
        {
            SetGraph();
        }
        private List<CancellationTokenSource> cancels = new List<CancellationTokenSource>();
        #region Inspector
        [OnInspectorGUI]
        private void GuiChange()
        {
            if (GUI.changed)
            {
                SetGraph();
            }
        }
        [FoldoutGroup("生成测试数据"), SerializeField]
        private int testDataCount = 100;
        [Button("生成测试数据"), FoldoutGroup("生成测试数据")]
        private void GenerateTestData()
        {
            posList.Clear();
            dataList.Clear();
            var s = startGO.position;
            var e = endGO.position;
            for (int i = 0; i < testDataCount; i++)
            {
                posList.Add(new Vector3(Random.Range(s.x, e.x), s.y, Random.Range(s.z, e.z)));
                dataList.Add(Random.Range(0, 100));
            }
            SetGraph();
        }
        #endregion
#if UNITY_EDITOR
        private float GetMinValue()
        {
            Vector3 start = startGO.position;
            Vector3 end = endGO.position;
            float ab = (end.z - start.z) * (end.x - start.x);
            float value = Mathf.Sqrt(ab / errorNum);
            return value;
        }
#endif
        [OnInspectorInit]
        public async void SetGraph()
        {
            if (startGO == null || endGO == null) return;
            block = new MaterialPropertyBlock();

            //删除旧物体
            //var cube = transform.Find("Cube");
            //while (cube != null)
            //{
            //    DestroyImmediate(cube.gameObject);
            //    cube = transform.Find("Cube");
            //}

            Vector3 start = startGO.position;
            Vector3 end = endGO.position;
            int height = Mathf.CeilToInt((end.z - start.z) / barWidth);
            int width = Mathf.CeilToInt((end.x - start.x) / barWidth);
            //#if UNITY_EDITOR
            //        int num = height * width;
            //        if (num > errorNum)
            //        {
            //            if (UnityEditor.EditorUtility.DisplayDialog("性能爆炸预警!", "当前参数生成box数量为" + num + "个,是否取消操作", "取消生成","继续生成"))
            //            {
            //                return;
            //            }
            //        }
            //#endif
            //  Debug.Log(string.Format("宽:{0},高{1}", width, height));
            float[,] weights = new float[width, height];
            float max = Yjj_ChartUtility.ComputeMaxAndMin(dataList).maxValue;
            foreach (var c in cancels)
            {
                c.Cancel();
            }
            var cancel = new CancellationTokenSource();
            var token = cancel.Token;
            cancels.Add(cancel);
            var task = Task.Run(() =>
            {
                for (int i = 0; i < dataList.Count; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    float r = dataList[i] / max;
                //if (r < threshold)
                //{
                //    continue;
                //}
                int length = Mathf.CeilToInt(r / damping);
                    int orignX = Mathf.RoundToInt((posList[i].x - start.x) / barWidth);
                    int orignY = Mathf.RoundToInt((posList[i].z - start.z) / barWidth);
                    int startX = orignX - length;
                    startX = startX < 0 ? 0 : startX;
                    int endX = orignX + length;
                    endX = endX >= width ? width - 1 : endX;
                    int startY = orignY - length;
                    startY = startY < 0 ? 0 : startY;
                    int endY = orignY + length;
                    endY = endY >= height ? height - 1 : endY;
                    for (int x = startX; x < endX; x++)
                    {
                        for (int y = startY; y < endY; y++)
                        {
                            float weight = weights[x, y];
                            float distance = Vector2.Distance(new Vector2(orignX, orignY), new Vector2(x, y));
                            float currentWeight = r - distance * damping;
                            weight = weight == 0 ? currentWeight : weight;
                            currentWeight = currentWeight > weight ? (currentWeight + weight) * 0.5f : weight;
                            weights[x, y] = currentWeight;
                        }
                    }
                }
            }, token);
            try
            {
                await task;
                int childIndex = 0;
                if (startGO.parent == transform) childIndex++;
                if (endGO.parent == transform) childIndex++;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        float h = weights[i, j];
                        if (h < threshold)
                        {
                            continue;
                        }
                        Transform trans = null;
                        if (transform.childCount > childIndex)
                        {
                            trans = transform.GetChild(childIndex);
                        }
                        else
                        {
                            trans = Instantiate(prefab, transform).transform;
                        }
                        var mat = trans.GetComponent<MeshRenderer>();
                        trans.position = start + (new Vector3(barWidth * i, 0, barWidth * j)) + new Vector3(0.5f, 0, 0.5f) * barWidth;
                        block.SetFloat("weight", h);
                        mat.SetPropertyBlock(block);
                        h = heightCurve.Evaluate(h);
                        h *= maxHeight;
                        h = h <= minHeight ? minHeight : h;
                        trans.localScale = new Vector3(barWidth, h, barWidth);
                        childIndex++;
                    }
                }
                List<GameObject> des = new List<GameObject>();
                for (int i = childIndex; i < transform.childCount; i++)
                {
                    des.Add(transform.GetChild(i).gameObject);
                }
                for (int i = 0; i < des.Count; i++)
                {
                    DestroyImmediate(des[i]);
                }
            }
            catch
            {
                Debug.Log("任务取消");
            }
            finally
            {
                cancels.Remove(cancel);
                cancel.Dispose();
            }
        }
    }
}