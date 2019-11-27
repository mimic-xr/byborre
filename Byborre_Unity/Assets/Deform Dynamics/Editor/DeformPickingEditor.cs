using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomEditor(typeof(DeformPicking), true), CanEditMultipleObjects]
public class DeformPickingEditor : Editor
{
	public SerializedProperty pickingEnabledProp;
	public SerializedProperty limitPullDistanceProp;
	public SerializedProperty maxPullDistanceProp;
	private DeformPicking picker;

	private void OnEnable()
	{
		picker = (DeformPicking)target;

		pickingEnabledProp = serializedObject.FindProperty("pickingEnabled");
		limitPullDistanceProp = serializedObject.FindProperty("limitPullDistance");
		maxPullDistanceProp = serializedObject.FindProperty("maxPullDistance");

	}

	public override void OnInspectorGUI()
	{
		picker = (DeformPicking)target;

		serializedObject.Update();

		EditorGUILayout.PropertyField(pickingEnabledProp);
		EditorGUILayout.PropertyField(limitPullDistanceProp);

		if (picker.limitPullDistance)
		{
			EditorGUILayout.PropertyField(maxPullDistanceProp);
		}

		serializedObject.ApplyModifiedProperties();
	}
}
