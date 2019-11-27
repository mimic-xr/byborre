using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnchorToCollider)), CanEditMultipleObjects]
public class AnchorToColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("Anchor", "LargeButton"))
        {
            foreach (AnchorToCollider atc in serializedObject.targetObjects)
            {
                atc.Anchor();
            }
        }
    }
}
