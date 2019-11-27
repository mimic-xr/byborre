using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(DeformBodyVolumetric)), CanEditMultipleObjects]
public class DeformBodyVolumetricEditor : DeformBodyEditor
{
    public SerializedProperty
        volumeStiffness_Prop,
        disableRendering_Prop;

    DeformBodyVolumetric deformBodyVolumetric;

    protected override void OnEnable()
    {
        base.OnEnable();

        deformBodyVolumetric = (DeformBodyVolumetric)target;

        distanceStiffness_Prop = serializedObject.FindProperty("distanceStiffness");
        bendingStiffness_Prop = serializedObject.FindProperty("bendingStiffness");
        volumeStiffness_Prop = serializedObject.FindProperty("volumeStiffness");
    }

    // TODO: Is the vertex painting stuff needed?
    public override void OnInspectorGUI()
    {
        var files = Directory.GetFiles(Application.streamingAssetsPath);

        var options = new List<string>();

        char[] slashes = { '/', '\\' };

        for (var i = 0; i < files.Length; i++)
        {
            if (!Path.GetExtension(files[i]).Equals(".mesh")) continue;
            options.Add(files[i].Substring(files[i].LastIndexOfAny(slashes)).Trim());
        }

        deformBodyVolumetric.selectedPath = EditorGUILayout.Popup("Mesh", deformBodyVolumetric.selectedPath, options.ToArray());
        deformBodyVolumetric.SetPath(options[deformBodyVolumetric.selectedPath]);

        EditorGUILayout.PropertyField(distanceStiffness_Prop);
        EditorGUILayout.PropertyField(bendingStiffness_Prop);
        EditorGUILayout.PropertyField(volumeStiffness_Prop);

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        // Fixed vertices button
        GUIContent fixed_button = EditorGUIUtility.IconContent("ClothInspector.PaintTool") ?? new GUIContent();
        fixed_button.text = " Fixed vertices";

        GUILayout.BeginHorizontal();
        deformBodyVolumetric.paintFixedVertices = GUILayout.Toggle(deformBodyVolumetric.paintFixedVertices, fixed_button, "Button", GUILayout.ExpandWidth(true));

        if (deformBodyVolumetric.paintFixedVertices)
        {
            deformBodyVolumetric.paintKineticFriction = false;
            deformBodyVolumetric.paintStaticFriction = false;
        }

        // Kinetic friction button
        GUIContent kinetic_friction_button = EditorGUIUtility.IconContent("ClothInspector.PaintTool") ?? new GUIContent();
        kinetic_friction_button.text = " Kinetic friction";

        deformBodyVolumetric.paintKineticFriction = GUILayout.Toggle(deformBodyVolumetric.paintKineticFriction, kinetic_friction_button, "Button", GUILayout.ExpandWidth(true));

        if (deformBodyVolumetric.paintKineticFriction)
        {
            deformBodyVolumetric.paintFixedVertices = false;
            deformBodyVolumetric.paintStaticFriction = false;
        }

        // Static friction button
        GUIContent static_friction_button = EditorGUIUtility.IconContent("ClothInspector.PaintTool") ?? new GUIContent();
        static_friction_button.text = " Static friction";

        deformBodyVolumetric.paintStaticFriction = GUILayout.Toggle(deformBodyVolumetric.paintStaticFriction, static_friction_button, "Button", GUILayout.ExpandWidth(true));

        if (deformBodyVolumetric.paintStaticFriction)
        {
            deformBodyVolumetric.paintFixedVertices = false;
            deformBodyVolumetric.paintKineticFriction = false;
        }

        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            deformBodyVolumetric.vertexPaintingModeChanged = true;

            for (int i = 0; i < targets.Length; i++)
            {
                DeformBody t = (DeformBody)targets[i];

                t.paintFixedVertices = deformBodyVolumetric.paintFixedVertices;
                t.paintKineticFriction = deformBodyVolumetric.paintKineticFriction;
                t.paintStaticFriction = deformBodyVolumetric.paintStaticFriction;

                t.vertexPaintingModeChanged = true;
            }

            paintAmount = 0;
            SceneView.RepaintAll();
        }

        if (deformBodyVolumetric.paintKineticFriction)
        {
            GUILayout.BeginHorizontal();
            paintAmount = EditorGUILayout.Slider("Amount", paintAmount, 0, 1);
            GUILayout.EndHorizontal();
        }

        if (deformBodyVolumetric.paintStaticFriction)
        {
            GUILayout.BeginHorizontal();
            paintAmount = EditorGUILayout.Slider("Amount", paintAmount, 0, 5);
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();
    }

    [MenuItem("GameObject/Deform Dynamics/Deform Body Volumetric", false, 1)]
    static void CreateDeformBodyVolumetric(MenuCommand menuCommand)
    {
        GameObject c = new GameObject("DeformBodyVolumetric");
        GameObjectUtility.SetParentAndAlign(c, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(c, "Create Deform Body Volumetric");
        c.AddComponent<MeshFilter>();
        c.AddComponent<MeshRenderer>();
        c.AddComponent<DeformBodyVolumetric>();
        Selection.activeObject = c;
    }
}