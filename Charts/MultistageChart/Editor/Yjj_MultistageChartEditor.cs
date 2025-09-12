using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YJJTool
{

    [CustomEditor(typeof(Yjj_MultistageChart))]
    public class Yjj_MultistageChartEditor : Editor
    {
        Yjj_MultistageChart t;
        private void OnEnable()
        {
            t = (Yjj_MultistageChart)target;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //if (GUILayout.Button("生成随机测试数据"))
            //{
            //    int count = Random.Range(3,7);
            //    List<List<Yjj_PieData>> datas = new List<List<Yjj_PieData>>();
            //    for(int i = 0; i < count; i++)
            //    {
            //        List<Yjj_PieData> data = new List<Yjj_PieData>();
            //        for(int j = 0; j < 3; j++)
            //        {
            //            Yjj_PieData d = new Yjj_PieData();
            //            d.dataValue = Random.Range(100f, 500f);
            //            d.pieColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1);
            //            data.Add(d);
            //        }
            //        datas.Add(data);
            //    }
            //    t.datas = datas;
            //    t.SetVerticesDirty();
            //    serializedObject.ApplyModifiedProperties();
            //}
            if (GUILayout.Button("播放动画"))
            {
                t.PlayAnimation();
            }
            if (GUI.changed)
            {

            }
        }
    }
}