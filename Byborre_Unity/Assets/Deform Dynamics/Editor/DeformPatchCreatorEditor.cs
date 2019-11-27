using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DeformPatchCreator), true), CanEditMultipleObjects]
public class DeformPatchCreatorEditor : Editor {

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space();

        GUI.enabled = !Application.isPlaying;

        if (GUILayout.Button("Create patch", "LargeButton"))
        {
            foreach (var targetObject in serializedObject.targetObjects)
            {
                var creator = (DeformPatchCreator)targetObject;

                creator.Create();
            }
        }

        GUI.enabled = true;
    }

    [MenuItem("GameObject/Deform Dynamics/Deform Patch Creator", false, 2)]
    static void CreateDeformPatchCreator(MenuCommand menuCommand)
    {
        GameObject c = new GameObject("DeformPatch");
        GameObjectUtility.SetParentAndAlign(c, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(c, "Create Deform Patch Creator");
        c.AddComponent<MeshFilter>();
        c.AddComponent<MeshRenderer>();
        c.AddComponent<DeformBody>();
        c.AddComponent<DeformPatchCreator>();        
        Selection.activeObject = c;
    }
}
