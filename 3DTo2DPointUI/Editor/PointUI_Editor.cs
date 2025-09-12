using System.Collections.Generic;
using UnityEditorInternal;
using System;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;


namespace YJJTool
{

    [CustomEditor(typeof(PointUI))]

    public class PointUI_Editor : OdinEditor
    {
        private PointUI Target;
        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();

        }
        private void OnSceneGUI()
        {
            Target = target as PointUI;
            if (Target == null) return;

            Vector2 lastPos = Vector2.zero;

            for (int i = 0; i < Target.offsets.Count; ++i)
            {
                var temp = lastPos;
                //Debug.Log(temp);
                var pos = Target.offsets[i] + lastPos;
                lastPos = pos;
                var worldPos = Target.transform.localToWorldMatrix.MultiplyPoint(pos + Target.Temps[0]);
                Handles.color = Color.yellow;
                Handles.Label(worldPos, i.ToString());
                EditorGUI.BeginChangeCheck();
                var newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(Target, "Change Pos");
                    var localPos = Target.transform.worldToLocalMatrix.MultiplyPoint(newWorldPos);
                    var v2 = new Vector2(localPos.x, localPos.y) - Target.Temps[0];
                    Target.offsets[i] = v2 - temp;
                    Target.SetGraph();
                }
            }

        }
    }
}