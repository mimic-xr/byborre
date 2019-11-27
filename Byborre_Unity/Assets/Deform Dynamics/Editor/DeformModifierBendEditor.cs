using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(DeformModifierBend)), CanEditMultipleObjects]
public class DeformModifierBendEditor : Editor
{
    public SerializedProperty
        bendAxis_Prop,
        symmetryAxis_Prop,
        limit_Prop,
        angle_Prop;

    DeformModifierBend deformBender;
    VertexPaint vertexPaint;

    bool paintVertices;
    List<int> recentlyPaintedVertices;

    Texture2D brushTexture;

    protected virtual void OnEnable()
    {
        deformBender = (DeformModifierBend)target;

        vertexPaint = CreateInstance<VertexPaint>();
        vertexPaint.SetParticleMaterial(Resources.Load<Material>("Materials/ParticleGeometryMaterial"));
        vertexPaint.shouldErase = false;

        bendAxis_Prop = serializedObject.FindProperty("bendAxis");
        symmetryAxis_Prop = serializedObject.FindProperty("symmetryAxis");
        limit_Prop = serializedObject.FindProperty("limit");
        angle_Prop = serializedObject.FindProperty("angle");

        brushTexture = Resources.Load<Texture2D>("Images/Circle");

        int num_vertices = deformBender.GetVertexCount();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(bendAxis_Prop);
        EditorGUILayout.PropertyField(symmetryAxis_Prop);

        EditorGUILayout.PropertyField(limit_Prop);
        EditorGUILayout.PropertyField(angle_Prop);

        GUI.enabled = !Application.isPlaying;

        EditorGUI.BeginChangeCheck();

        GUIContent fixed_button = EditorGUIUtility.IconContent("ClothInspector.PaintTool");
        fixed_button.text = " Select vertices to bend";

        paintVertices = GUILayout.Toggle(paintVertices, fixed_button, "Button", GUILayout.ExpandWidth(true));

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Apply bending", "LargeButton"))
        {
            deformBender.ApplyBending();
        }

        GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        if (Application.isPlaying) return;

        if (paintVertices && vertexPaint != null)
        {
            vertexPaint.SetData(deformBender.GetVertices(), deformBender.GetComponent<MeshFilter>().transform,
                                deformBender.paintedVertices, new Vector3(0, 1, 1));

            recentlyPaintedVertices = vertexPaint.HandlePaintEvents(deformBender, true, true, brushTexture);

            if (recentlyPaintedVertices != null && recentlyPaintedVertices.Count > 0)
            {
                foreach (int i in recentlyPaintedVertices)
                {
                    deformBender.paintedVertices[i] = vertexPaint.shouldErase ? false : true;
                }
            }

            if (vertexPaint.shouldApplyOnAllVertices)
            {
                for (int i = 0; i < deformBender.GetVertexCount(); i++)
                {
                    deformBender.paintedVertices[i] = vertexPaint.shouldErase ? false : true;
                }
            }
            else if (vertexPaint.shouldClearPaintedVertices)
            {
                for (int i = 0; i < deformBender.GetVertexCount(); i++)
                {
                    deformBender.paintedVertices[i] = false;
                }
            }

            deformBender.UpdatePreview();
        }

        // Draw bend preview mesh
        deformBender.previewMaterial.SetPass(0);
        Graphics.DrawMeshNow(deformBender.preview, deformBender.transform.localToWorldMatrix);
    }
}
