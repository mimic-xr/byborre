using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DeformPattern), true), CanEditMultipleObjects]
public class DeformPatternEditor : Editor {

	private string buttonLabel = "Triangulate";

	private bool needsTriangulation = false;
	private bool needsRetriangulation = false;
	private Color oldColor;

    public override void OnInspectorGUI()
    {
		if (Application.isPlaying) GUI.enabled = false;

		serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space();

		// If the object doesn't exist in the scene, we shouldn't draw the custom GUI
		foreach (var targetObject in serializedObject.targetObjects)
		{
			var pattern = (DeformPattern)targetObject;

			// The object is invalid if it doesn't exist in the scene, i.e. is only a prefab
			// in the project window
			if (!pattern.gameObject.scene.IsValid()) return;
		}

		// If any of objects have not been triangulated yet
		foreach (var targetObject in serializedObject.targetObjects)
		{
			var pattern = (DeformPattern)targetObject;

			needsTriangulation = !pattern.triangulated;

			if (needsTriangulation) break;
		}

		// If no object needs triangulation, check if any object needs retriangulation
		// The pattern is dirty if someone has changed the scale
		if (!needsTriangulation)
		{
			foreach (var targetObject in serializedObject.targetObjects)
			{
				var pattern = (DeformPattern)targetObject;

				needsRetriangulation = pattern.triangulationDirty;

				if (needsRetriangulation) break;
			}
		}

		// If no object needs triangulation and no object is dirty, check if any object needs retriangulation due to
		// the size of the triangles being changed
		if (!needsTriangulation && !needsRetriangulation)
		{
			foreach (var targetObject in serializedObject.targetObjects)
			{
				var pattern = (DeformPattern)targetObject;

				needsRetriangulation = pattern.oldNumTriangles != pattern.numTriangles;

				if (needsRetriangulation) break;
			}
		}
		

		oldColor = GUI.backgroundColor;

		if (needsTriangulation)
		{
			buttonLabel = "Triangulate";
			GUI.backgroundColor = Color.yellow;
			GUI.enabled = true;

		}
        else if (needsRetriangulation)
		{
			buttonLabel = "Retriangulate";
			GUI.backgroundColor = Color.yellow;
			GUI.enabled = true;

		}
        else
		{
			buttonLabel = "Retriangulate";	// If it reaches this, it has been triangulated at some point
			GUI.backgroundColor = oldColor;
			GUI.enabled = false;
		}

		if (GUILayout.Button(buttonLabel, "LargeButton"))
        {
            foreach (var targetObject in serializedObject.targetObjects)
            {
                var pattern = (DeformPattern)targetObject;

				Undo.RecordObject(pattern, "Triangulated");

				pattern.Triangulate();
			}
        }

		GUI.enabled = true;
    }
}
