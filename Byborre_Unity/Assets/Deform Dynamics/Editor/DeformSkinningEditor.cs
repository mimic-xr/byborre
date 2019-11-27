using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DeformSkinning))]
public class DeformSkinningEditor : Editor
{
    VertexPaint vertexPaint;
    List<int> recentlyPaintedVertices;
    Texture2D brushTexture;

    protected virtual void OnEnable()
    {
        vertexPaint = VertexPaint.Instance;
        vertexPaint.SetParticleMaterial(Resources.Load<Material>("Materials/ParticleGeometryMaterial"));
        
        brushTexture = Resources.Load<Texture2D>("Images/Circle");
    }

    protected virtual void OnDisable()
    {
        DeformSkinning skinning = (DeformSkinning)target;

        skinning.paintSkinnedVertices = false;
    }

    public override void OnInspectorGUI()
    {
        DeformSkinning skinning = (DeformSkinning)target;

        DrawDefaultInspector();

        GUI.enabled = !Application.isPlaying;

        GUIContent paint_button = EditorGUIUtility.IconContent("ClothInspector.PaintTool") ?? new GUIContent();
        paint_button.text = " Skinned vertices";

        EditorGUI.BeginChangeCheck();
        skinning.paintSkinnedVertices = GUILayout.Toggle(skinning.paintSkinnedVertices, paint_button, "Button", GUILayout.ExpandWidth(true));
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }

        GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        DeformSkinning skinning = (DeformSkinning)target;
        DeformBody deformBody = skinning.GetComponent<DeformBody>();
        
        if (skinning.paintSkinnedVertices && vertexPaint != null)
        {
            int bodyVertexCount = deformBody.GetVertexCount();

            if (skinning.skinnedVertices == null || skinning.skinnedVertices.Length != bodyVertexCount)
            {
                skinning.skinnedVertices = new bool[bodyVertexCount];
            }

            vertexPaint.SetData(deformBody.GetVertices(), deformBody.transform,
                                skinning.skinnedVertices, new Vector3(0, 1, 1));

            recentlyPaintedVertices = vertexPaint.HandlePaintEvents(skinning, true, true, brushTexture);

            if (recentlyPaintedVertices != null && recentlyPaintedVertices.Count > 0)
            {
                foreach (int i in recentlyPaintedVertices)
                {
                    skinning.skinnedVertices[i] = vertexPaint.shouldErase ? false : true;
                }
            }

            if (vertexPaint.shouldApplyOnAllVertices)
            {
                for (int i = 0; i < deformBody.GetVertexCount(); i++)
                {
                    skinning.skinnedVertices[i] = vertexPaint.shouldErase ? false : true;
                }
            }
            else if (vertexPaint.shouldClearPaintedVertices)
            {
                for (int i = 0; i < deformBody.GetVertexCount(); i++)
                {
                    skinning.skinnedVertices[i] = false;
                }
            }
        }
    }
}
