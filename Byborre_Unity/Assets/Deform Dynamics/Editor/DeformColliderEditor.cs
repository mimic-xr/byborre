using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(DeformCollider), true), CanEditMultipleObjects]
public class DeformColliderEditor : Editor
{
    public SerializedProperty
        kineticFrictionProp,
        staticFrictionProp,
        sticky,
        showColliderInEditorProp,
        showColliderInGameProp,
        inGameMaterialProp,
        biasProp,
        capsuleTypeProp,
        capsuleLengthProp,
        capsuleTransformAProp,
        capsuleTransformBProp,
        capsuleRadiusAProp,
        capsuleRadiusBProp,
        sphereRadiusProp,
        rigTypeProp,
        remeshProp,
        numRemeshedVerticesProp;

    DeformCollider collider;

    protected VertexPaint vertexPaint;
    protected List<int> recentlyPaintedVertices;
    Texture2D brushTexture;

    bool layer0, layer1, layer2, layer3;

    Vector3[] colors = { new Vector3(1, 1, 1), // White
                         new Vector3(0.5f, 1, 0.5f), // Green
                         new Vector3(0.5f, 0.5f, 1), // Blue
                         new Vector3(1, 0.5f, 0.5f), // Red
                         new Vector3(1, 1, 0.5f) }; // Yellow

    void OnEnable()
    {
        collider = (DeformCollider)target;

        kineticFrictionProp = serializedObject.FindProperty("kineticFriction");
        staticFrictionProp = serializedObject.FindProperty("staticFriction");
        sticky = serializedObject.FindProperty("sticky");
        showColliderInEditorProp = serializedObject.FindProperty("showColliderInEditor");
        showColliderInGameProp = serializedObject.FindProperty("showColliderInGame");
        inGameMaterialProp = serializedObject.FindProperty("inGameMaterial");
        biasProp = serializedObject.FindProperty("bias");

        if (collider is DeformColliderCapsule)
        {
            capsuleTypeProp = serializedObject.FindProperty("capsuleType");
            capsuleLengthProp = serializedObject.FindProperty("length");
            capsuleTransformAProp = serializedObject.FindProperty("transformA");
            capsuleTransformBProp = serializedObject.FindProperty("transformB");
            capsuleRadiusAProp = serializedObject.FindProperty("radiusA");
            capsuleRadiusBProp = serializedObject.FindProperty("radiusB");
        }
        else if (collider is DeformColliderSphere)
        {
            sphereRadiusProp = serializedObject.FindProperty("radius");
        }
        else if (collider is DeformColliderMesh)
        {
            remeshProp = serializedObject.FindProperty("remesh");
            numRemeshedVerticesProp = serializedObject.FindProperty("numRemeshedVertices");
            rigTypeProp = serializedObject.FindProperty("rigType");

            vertexPaint = VertexPaint.Instance;
            vertexPaint.SetParticleMaterial(Resources.Load<Material>("Materials/ParticleGeometryMaterial"));

            recentlyPaintedVertices = new List<int>();

            brushTexture = Resources.Load<Texture2D>("Images/Circle");
        }
    }

    public override void OnInspectorGUI()
    {
        collider = (DeformCollider)target;

        serializedObject.Update();

        if (collider is DeformColliderCapsule)
        {
            var capsuleCollider = collider as DeformColliderCapsule;

            EditorGUILayout.PropertyField(capsuleTypeProp);

            if (capsuleCollider.capsuleType == DeformColliderCapsule.CapsuleType.Length)
            {
                EditorGUILayout.PropertyField(capsuleLengthProp);
            }
            else
            {
                EditorGUILayout.PropertyField(capsuleTransformAProp);
                EditorGUILayout.PropertyField(capsuleTransformBProp);
            }

            EditorGUILayout.PropertyField(capsuleRadiusAProp);
            EditorGUILayout.PropertyField(capsuleRadiusBProp);
        }
        else if (collider is DeformColliderSphere)
        {
            EditorGUILayout.PropertyField(sphereRadiusProp);
        }

        EditorGUILayout.PropertyField(kineticFrictionProp);
        EditorGUILayout.PropertyField(staticFrictionProp);

        if (!(collider is DeformColliderMesh))
        {
            EditorGUILayout.PropertyField(sticky);
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(showColliderInEditorProp);

        if (!(collider is DeformColliderMesh))
        {
            EditorGUILayout.PropertyField(showColliderInGameProp);
        }

        if (collider.showColliderInGame)
        {
            EditorGUILayout.PropertyField(inGameMaterialProp);
        }

        EditorGUILayout.PropertyField(biasProp);

        if (collider is DeformColliderMesh)
        {
            EditorGUILayout.Space();

            var meshCollider = collider as DeformColliderMesh;

            EditorGUILayout.PropertyField(rigTypeProp);

            EditorGUILayout.PropertyField(remeshProp);

            if (meshCollider.remesh)
            {
                EditorGUILayout.PropertyField(numRemeshedVerticesProp);
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            GUIContent paint_button = EditorGUIUtility.IconContent("ClothInspector.PaintTool") ?? new GUIContent();
            paint_button.text = " Collision mask";

            meshCollider.paintCollisionMask = GUILayout.Toggle(meshCollider.paintCollisionMask, paint_button, "Button", GUILayout.ExpandWidth(true));

            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            if (meshCollider.paintCollisionMask)
            {
                EditorGUILayout.BeginHorizontal();

                Color originalColor = GUI.color;

                GUI.color = new Color(0.5f, 1, 0.5f);
                layer0 = GUILayout.Toggle(layer0, "", "Button", GUILayout.ExpandWidth(true));

                if (layer0)
                {
                    layer1 = false;
                    layer2 = false;
                    layer3 = false;
                }

                GUI.color = new Color(0.5f, 0.5f, 1);
                layer1 = GUILayout.Toggle(layer1, "", "Button", GUILayout.ExpandWidth(true));

                if (layer1)
                {
                    layer0 = false;
                    layer2 = false;
                    layer3 = false;
                }

                GUI.color = new Color(1, 0.5f, 0.5f);
                layer2 = GUILayout.Toggle(layer2, "", "Button", GUILayout.ExpandWidth(true));

                if (layer2)
                {
                    layer1 = false;
                    layer0 = false;
                    layer3 = false;
                }

                GUI.color = new Color(1, 1, 0.5f);
                layer3 = GUILayout.Toggle(layer3, "", "Button", GUILayout.ExpandWidth(true));

                if (layer3)
                {
                    layer1 = false;
                    layer2 = false;
                    layer0 = false;
                }

                meshCollider.currentCollisionMaskColor = layer0 ? 1 : layer1 ? 2 : layer2 ? 3 : layer3 ? 4 : 0;

                EditorGUILayout.EndHorizontal();
                GUI.color = originalColor;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        collider = (DeformCollider)target;

        if (collider is DeformColliderMesh)
        {
            DeformColliderMesh meshCollider = (DeformColliderMesh)collider;

            if (!meshCollider.paintCollisionMask || vertexPaint == null) return;

            if (meshCollider.collisionMask == null || meshCollider.collisionMask.Length == 0)
            {
                int vertexCount = meshCollider.GetVertexCount();

                if (vertexCount > 0)
                {
                    meshCollider.collisionMask = new int[vertexCount];
                }
                else
                {
                    return;
                }
            }

            vertexPaint.SetData(meshCollider.GetVertices(), meshCollider.transform, meshCollider.collisionMask, colors);

            recentlyPaintedVertices = vertexPaint.HandlePaintEvents(meshCollider, true, true, brushTexture);

            if (recentlyPaintedVertices != null && recentlyPaintedVertices.Count > 0)
            {
                foreach (int i in recentlyPaintedVertices)
                {
                    meshCollider.collisionMask[i] = vertexPaint.shouldErase ? 0 : meshCollider.currentCollisionMaskColor;
                }
            }

            if (vertexPaint.shouldApplyOnAllVertices)
            {
                for (int i = 0; i < meshCollider.GetVertexCount(); i++)
                {
                    meshCollider.collisionMask[i] = vertexPaint.shouldErase ? 0 : meshCollider.currentCollisionMaskColor;
                }
            }
            else if (vertexPaint.shouldClearPaintedVertices)
            {
                for (int i = 0; i < meshCollider.GetVertexCount(); i++)
                {
                    meshCollider.collisionMask[i] = 0;
                }
            }
            else if (vertexPaint.shouldExit)
            {
                meshCollider.paintCollisionMask = false;
                vertexPaint.shouldExit = false;
            }
        }
    }

    [MenuItem("GameObject/Deform Dynamics/Deform Collider/Sphere", false, 3)]
    static void CreateDeformBodyPatch(MenuCommand menuCommand)
    {
        GameObject c = new GameObject("DeformColliderSphere");
        GameObjectUtility.SetParentAndAlign(c, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(c, "Create Deform Collider (Sphere)");
        c.AddComponent<DeformColliderSphere>();
        Selection.activeObject = c;
    }

    [MenuItem("GameObject/Deform Dynamics/Deform Collider/Capsule", false, 4)]
    static void CreateDeformBodyTriangular(MenuCommand menuCommand)
    {
        GameObject c = new GameObject("DeformColliderCapsule");
        GameObjectUtility.SetParentAndAlign(c, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(c, "Create Deform Collider (Capsule)");
        c.AddComponent<DeformColliderCapsule>();
        Selection.activeObject = c;
    }

    [MenuItem("GameObject/Deform Dynamics/Deform Collider/Box", false, 5)]
    static void CreateDeformManager(MenuCommand menuCommand)
    {
        GameObject c = new GameObject("DeformColliderBox");
        GameObjectUtility.SetParentAndAlign(c, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(c, "Create Deform Collider (Box)");
        c.AddComponent<DeformColliderBox>();
        Selection.activeObject = c;
    }

    [MenuItem("GameObject/Deform Dynamics/Deform Collider/Plane", false, 6)]
    static void CreateDeformBodyTetrahedral(MenuCommand menuCommand)
    {
        GameObject c = new GameObject("DeformColliderPlane");
        GameObjectUtility.SetParentAndAlign(c, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(c, "Create Deform Collider (Plane)");
        c.AddComponent<DeformColliderPlane>();
        Selection.activeObject = c;
    }
}
