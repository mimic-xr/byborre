using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(DeformFBXExporter)), CanEditMultipleObjects]
public class DeformFBXRecorderEditor : Editor
{
	public SerializedProperty path_Prop;

	void OnEnable()
    {
        path_Prop = serializedObject.FindProperty("filePath");
	}

    public override void OnInspectorGUI()
    {
		DeformFBXExporter exporter = (DeformFBXExporter) target;

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(path_Prop);
        GUI.enabled = true;
		

		if (GUILayout.Button("...", GUILayout.Width(40), GUILayout.Height(15)))
        {
            string s = EditorUtility.SaveFilePanel("Exported FBX location", ".", "deform_mesh", "fbx");
            if (s.Length > 0) exporter.filePath = s;
        }

        EditorGUILayout.EndHorizontal();

		GUILayout.Space(10);

        if (!exporter.shouldRecord)
        {
            exporter.shouldRecord = GUILayout.Toggle(exporter.shouldRecord, new GUIContent("Record animation to .fbx"), "LargeButton");
        }
        else
        {
            exporter.shouldRecord = GUILayout.Toggle(exporter.shouldRecord, new GUIContent("Stop recording"), "LargeButton");
        }
        
        //exporter.shouldTakeSnapshot = GUILayout.Button(new GUIContent("Create .fbx snapshot"), "LargeButton");
        
		if(GUILayout.Button(new GUIContent("Create .fbx snapshot"), "LargeButton"))
		{
			exporter.FBXSnapshot();
		}

        // Recording logic
        //if (!exporter.capturedObject) return;
        
        if (!exporter.shouldRecord && exporter.recordingStarted) // Recording was started but has now stopped
        {
            if (exporter.filePath.Length == 0)
            {
                Debug.LogError("Filename not set, cannot export FBX file");
                return;
            }

            exporter.StopRecording();
            exporter.recordingStarted = false;
        }
    }
}

#endif