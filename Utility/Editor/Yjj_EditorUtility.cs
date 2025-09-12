using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class Yjj_EditorUtility : Editor
{
    public static void BeginFoldOut(ref bool property, string title)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        property = EditorGUILayout.BeginFoldoutHeaderGroup(property, title);
    }
    public static void EndFoldOut()
    {
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.EndVertical();
    }

    public static void DrawInspector(Type tp, ref Dictionary<string,bool> folderDic,SerializedObject so,UnityEngine.Object obj)
    {
        so.Update();
        bool isFoldBegin = false;
        FieldInfo[] fields = tp.GetFields();
        bool write = true;
        foreach(var f in fields)
        {
           // EditorGUILayout.Space();
            FolderEndAttribute foldEnd = f.GetCustomAttribute<FolderEndAttribute>();
            if (foldEnd != null)
            {
                if (!isFoldBegin)
                {
                    continue;
                }
                EndFoldOut();
                write = true;
                isFoldBegin = false;
            }
            FolderAttribute folder =  f.GetCustomAttribute<FolderAttribute>();
            if(folder != null)
            {
                if (isFoldBegin)
                {
                    EndFoldOut();
                }
                if (!folderDic.ContainsKey(folder.name))
                {
                    folderDic.Add(folder.name, true);
                }
                folderDic.TryGetValue(folder.name, out bool isFold);
                write = isFold;
                BeginFoldOut(ref isFold, folder.name);
                folderDic[folder.name] = isFold;
                isFoldBegin = true;
            }
            if (!write)
            {
                continue;
            }
            MyInspectorAttribute my = f.GetCustomAttribute<MyInspectorAttribute>();
            if (my != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(my.name,GUILayout.MaxWidth(120));
                var property = so.FindProperty(f.Name);
                EditorGUILayout.PropertyField(property,true);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                var property = so.FindProperty(f.Name);
                EditorGUILayout.PropertyField(property,true);
            }
        }
        if (isFoldBegin)
        {
            EndFoldOut();
        }
        if (GUI.changed)
        {
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(obj);
        }

    }
    public static bool InScene(Transform t)
    {
        return t.parent != null;
    }

}
