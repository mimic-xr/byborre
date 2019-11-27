using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomEditor(typeof(Demo), true), CanEditMultipleObjects]
public class DemoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Demo demo = (Demo) target;

        EditorGUI.BeginChangeCheck();

        if (Application.isPlaying) GUI.enabled = false;
        
        if (!demo.shouldLoad)
        {
            demo.shouldLoad = GUILayout.Toggle(demo.shouldLoad, new GUIContent("Load demo"), "LargeButton");
        }
        else
        {
            demo.shouldLoad = GUILayout.Toggle(demo.shouldLoad, new GUIContent("Unload demo"), "LargeButton");
        }

        GUI.enabled = true;

        if (EditorGUI.EndChangeCheck())
        {
            if (demo.shouldLoad)
            {
                demo.Load();
            } 
            else
            {
                demo.Unload();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
