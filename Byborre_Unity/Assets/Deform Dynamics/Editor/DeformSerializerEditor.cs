using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DeformSerializer), true), CanEditMultipleObjects]
public class DeformSerializerEditor : Editor
{
    public SerializedProperty path_Prop;

    void OnEnable()
    {
        path_Prop = serializedObject.FindProperty("fileName");
    }

    public override void OnInspectorGUI()
    {
        DeformSerializer deformBodySerializer = (DeformSerializer)target;

        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(path_Prop);
        GUI.enabled = true;

        if (GUILayout.Button("...", GUILayout.Width(40), GUILayout.Height(15)))
        {
			string defName = deformBodySerializer.gameObject.name;

			string s = EditorUtility.SaveFilePanel("", ".", defName, "def");
            if (s.Length > 0) deformBodySerializer.fileName = s;
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUI.enabled = !Application.isPlaying;

        if (GUILayout.Button("Load from file", "LargeButton"))
        {
            foreach (var targetObject in serializedObject.targetObjects)
            {
                var serializer = (DeformSerializer)targetObject;
                serializer.Load();
            }
        }

        GUI.enabled = Application.isPlaying;

        if (GUILayout.Button("Save to file", "LargeButton"))
        {
            foreach (var targetObject in serializedObject.targetObjects)
            {
                var serializer = (DeformSerializer)targetObject;
                serializer.Save();
            }
        }

        GUI.enabled = true;
    }

    [MenuItem("GameObject/Deform Dynamics/Deform Body Serializer", false, 3)]
    static void CreateDeformBodySerializer(MenuCommand menuCommand)
    {
        GameObject c = new GameObject("DeformBody (Serialized)");
        GameObjectUtility.SetParentAndAlign(c, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(c, "Create Deform Body (Serialized)");
        c.AddComponent<DeformBody>();
        c.AddComponent<DeformSerializer>();
        Selection.activeObject = c;
    }
}
