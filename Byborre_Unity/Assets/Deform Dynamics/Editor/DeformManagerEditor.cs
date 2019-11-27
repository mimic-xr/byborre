using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DeformManager), true), CanEditMultipleObjects]
public class DeformManagerEditor : Editor
{
    public override void OnInspectorGUI()
	{
		serializedObject.Update();

		DrawDefaultInspector();

		EditorGUILayout.Space();

		DeformManager manager = (DeformManager) target;

        EditorGUI.BeginChangeCheck();

		if (!manager.isPaused)
		{
			GUILayout.Toggle(manager.isPaused, new GUIContent("Pause Simulation (P)"), "LargeButton");
		}
		else
		{
			GUILayout.Toggle(manager.isPaused, new GUIContent("Resume Simulation (P)"), "LargeButton");
		}

        if (EditorGUI.EndChangeCheck())
        {
            manager.TogglePaused();
        }

		if (manager.isPaused && Application.isPlaying)
		{
			var button = GUILayout.Button(new GUIContent("Step Simulation (O)", "Advances the simulation one frame"), "LargeButton");

			if (button)
			{
				manager.UpdateSimulation();
			}
		}

        serializedObject.ApplyModifiedProperties();
	}

    [MenuItem("GameObject/Deform Dynamics/Deform Manager", false, 7)]
    static void CreateDeformManager(MenuCommand menuCommand)
    {
        GameObject c = new GameObject("DeformManager");
        GameObjectUtility.SetParentAndAlign(c, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(c, "Create Deform Manager");
        c.AddComponent<DeformManager>();
        Selection.activeObject = c;
    }
}
