using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using DeformDynamics;

[CustomEditor(typeof(DeformBody), true), CanEditMultipleObjects]
public class DeformBodyEditor : Editor
{
    public SerializedProperty distanceStiffness_Prop, bendingStiffness_Prop, mergeCloseVertices_Prop, simulationMesh_Prop;

    private const string FIXED_MENU_NAME = "Deform/Paint/Fixed vertices";
    private const string KINETIC_FRICTION_MENU_NAME = "Deform/Paint/Kinetic friction";
    private const string STATIC_FRICTION_MENU_NAME = "Deform/Paint/Static friction";

    DeformBody deformBody;
	protected VertexPaint vertexPaint;

	protected List<int> recentlyPaintedVertices;

    Texture2D brushTexture;
    
	protected float paintAmount = 0;
    protected float bendingProxy = 0;

    bool layer0, layer1, layer2, layer3;

    Vector3[] colors = { new Vector3(1, 1, 1), // White
                         new Vector3(0.5f, 1, 0.5f), // Green
                         new Vector3(0.5f, 0.5f, 1), // Blue
                         new Vector3(1, 0.5f, 0.5f), // Red
                         new Vector3(1, 1, 0.5f) }; // Yellow

    protected virtual void OnEnable()
    {
        deformBody = (DeformBody) target;

        distanceStiffness_Prop = serializedObject.FindProperty("distanceStiffness");
        bendingStiffness_Prop = serializedObject.FindProperty("bendingStiffness");
        mergeCloseVertices_Prop = serializedObject.FindProperty("mergeCloseVertices");
        simulationMesh_Prop = serializedObject.FindProperty("simulationMesh");

        if (!Application.isPlaying)
        {
            deformBody.UpdateInputMeshes();
        }

        vertexPaint = VertexPaint.Instance;
        vertexPaint.SetParticleMaterial(Resources.Load<Material>("Materials/ParticleGeometryMaterial"));
        
        recentlyPaintedVertices = new List<int>();

        brushTexture = Resources.Load<Texture2D>("Images/Circle");
    }

    protected virtual void OnDisable()
    {
        for (int i = 0; i < targets.Length; i++)
        {
            DeformBody t = (DeformBody)targets[i];

            if (t)
            {
                t.paintFixedVertices = false;
                t.paintKineticFriction = false;
                t.paintStaticFriction = false;
                t.paintSelfCollisionMask = false;
                t.paintExternalCollisionMask = false;
                t.hasPaintedThisFrame = false;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        deformBody = (DeformBody) target;

        if (!Application.isPlaying)
        {
            deformBody.UpdateInputMeshes();
        }

        EditorGUILayout.PropertyField(distanceStiffness_Prop);
        EditorGUILayout.PropertyField(bendingStiffness_Prop);

        //EditorGUI.BeginChangeCheck();
        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.PrefixLabel("Bending Stiffness");

        //bendingProxy = GUILayout.HorizontalSlider(bendingProxy, 0, 1, GUILayout.ExpandWidth(true));

        //if (EditorGUI.EndChangeCheck())
        //{
        //    deformBody.bendingStiffness = Mathf.Pow(bendingProxy, 10);

        //    if (Application.isPlaying)
        //    {
        //        DeformPlugin.Object.SetBendingStiffness(deformBody.id, deformBody.bendingStiffness);
        //    }
        //}

        //string bending = deformBody.bendingStiffness.ToString();

        //if (bending.Contains("E"))
        //{
        //    bending = bending.Substring(0, 3) + bending.Substring(bending.IndexOf('E'));
        //}

        //EditorGUI.BeginChangeCheck();
        //bending = EditorGUILayout.TextField(bending, GUILayout.MaxWidth(50));

        //if (EditorGUI.EndChangeCheck())
        //{
        //    if (float.TryParse(bending, out float value))
        //    {
        //        deformBody.bendingStiffness = value;
        //    }

        //    bendingProxy = Mathf.Pow(deformBody.bendingStiffness, 0.1f);

        //    if (Application.isPlaying)
        //    {
        //        DeformPlugin.Object.SetBendingStiffness(deformBody.id, deformBody.bendingStiffness);
        //    }
        //}

        //EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(mergeCloseVertices_Prop);
        EditorGUILayout.PropertyField(simulationMesh_Prop);

        EditorGUILayout.Space();
        GUI.enabled = !Application.isPlaying;
        EditorGUI.BeginChangeCheck();

        // Fixed vertices button
        GUIContent fixed_button = EditorGUIUtility.IconContent("ClothInspector.PaintTool") ?? new GUIContent();
        fixed_button.text = " Fixed vertices";

        GUILayout.BeginHorizontal();
        deformBody.paintFixedVertices = GUILayout.Toggle(deformBody.paintFixedVertices, fixed_button, "Button", GUILayout.ExpandWidth(true));

        if (deformBody.paintFixedVertices)
        {
            deformBody.paintKineticFriction = false;
            deformBody.paintStaticFriction = false;
        }

        // Kinetic friction button
        GUIContent kinetic_friction_button = EditorGUIUtility.IconContent("ClothInspector.PaintTool") ?? new GUIContent();
        kinetic_friction_button.text = " Kinetic friction";

        deformBody.paintKineticFriction = GUILayout.Toggle(deformBody.paintKineticFriction, kinetic_friction_button, "Button", GUILayout.ExpandWidth(true));

        if (deformBody.paintKineticFriction)
        {
            deformBody.paintFixedVertices = false;
            deformBody.paintStaticFriction = false;
        }

        // Static friction button
        GUIContent static_friction_button = EditorGUIUtility.IconContent("ClothInspector.PaintTool") ?? new GUIContent();
        static_friction_button.text = " Static friction";

        deformBody.paintStaticFriction = GUILayout.Toggle(deformBody.paintStaticFriction, static_friction_button, "Button", GUILayout.ExpandWidth(true));

        if (deformBody.paintStaticFriction)
        {
            deformBody.paintFixedVertices = false;
            deformBody.paintKineticFriction = false;
        }

        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            deformBody.vertexPaintingModeChanged = true;

            for (int i = 0; i < targets.Length; i++)
            {
                DeformBody t = (DeformBody) targets[i];

                t.paintFixedVertices = deformBody.paintFixedVertices;
                t.paintKineticFriction = deformBody.paintKineticFriction;
                t.paintStaticFriction = deformBody.paintStaticFriction;

                t.vertexPaintingModeChanged = true;
            }

            paintAmount = 0;
            SceneView.RepaintAll();
        }

        GUI.enabled = true;

        if (deformBody.paintKineticFriction)
        {
            GUILayout.BeginHorizontal();
            paintAmount = EditorGUILayout.Slider("Amount", paintAmount, 0, 1);
            GUILayout.EndHorizontal();
        }

        if (deformBody.paintStaticFriction)
        {
            GUILayout.BeginHorizontal();
            paintAmount = EditorGUILayout.Slider("Amount", paintAmount, 0, 5);
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        GUI.enabled = !Application.isPlaying;
        EditorGUILayout.BeginHorizontal();

        GUIContent selfCollMaskButton = EditorGUIUtility.IconContent("ClothInspector.PaintTool") ?? new GUIContent();
        selfCollMaskButton.text = " Self collision mask";

        deformBody.paintSelfCollisionMask = GUILayout.Toggle(deformBody.paintSelfCollisionMask, selfCollMaskButton, "Button", GUILayout.ExpandWidth(true));

        if (deformBody.paintSelfCollisionMask)
        {
            deformBody.paintExternalCollisionMask = false;
        }

        GUIContent externalCollMaskButton = EditorGUIUtility.IconContent("ClothInspector.PaintTool") ?? new GUIContent();
        externalCollMaskButton.text = " External collision mask";

        deformBody.paintExternalCollisionMask = GUILayout.Toggle(deformBody.paintExternalCollisionMask, externalCollMaskButton, "Button", GUILayout.ExpandWidth(true));

        if (deformBody.paintExternalCollisionMask)
        {
            deformBody.paintSelfCollisionMask = false;
        }

        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }

        if (deformBody.paintSelfCollisionMask || deformBody.paintExternalCollisionMask) 
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

            deformBody.currentCollisionMaskColor = layer0 ? 1 : layer1 ? 2 : layer2 ? 3 : layer3 ? 4 : 0;

            EditorGUILayout.EndHorizontal();
            GUI.color = originalColor;
        }

        GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }
    
    public void OnSceneGUI()
    {
        deformBody = (DeformBody) target;

        bool lastObject = true;
        bool firstObject = true;

        if (!Application.isPlaying)
        {
            deformBody.UpdateInputMeshes();
        }

        if (deformBody.paintFixedVertices || 
            deformBody.paintKineticFriction || 
            deformBody.paintStaticFriction  ||
            deformBody.paintSelfCollisionMask ||
            deformBody.paintExternalCollisionMask)
        {
            UpdateVertexPaintingColors(deformBody);
            recentlyPaintedVertices.Clear();            

            Object[] objects = Selection.GetFiltered(typeof(DeformBody), SelectionMode.TopLevel);

            for (int i = 0; i < objects.Length; i++)
            {
                DeformBody db = (DeformBody)objects[i];

                if (db == deformBody) continue;

                firstObject &= !db.hasPaintedThisFrame;
                lastObject &= db.hasPaintedThisFrame;   
            }
            
            recentlyPaintedVertices = vertexPaint.HandlePaintEvents(deformBody, firstObject, lastObject, brushTexture);

            if (lastObject)
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    DeformBody db = (DeformBody)objects[i];
                    db.hasPaintedThisFrame = false;
                }
            }
            else
            {
                deformBody.hasPaintedThisFrame = true;
            }            
        }

        if (vertexPaint.shouldApplyOnAllVertices)
        {
            ApplyVertexPaintToAllVertices(deformBody);
            return;
        }
        else if (vertexPaint.shouldClearPaintedVertices)
        {
            ClearVertexPaint(deformBody);
            return;
        }
        else if (vertexPaint.shouldExit)
        {
            DeformBody t = (DeformBody)target;

            if (t.paintFixedVertices || t.paintKineticFriction || t.paintStaticFriction || t.paintSelfCollisionMask || t.paintExternalCollisionMask)
            {
                t.paintFixedVertices = false;
                t.paintKineticFriction = false;
                t.paintStaticFriction = false;
                t.paintSelfCollisionMask = false;
                t.paintExternalCollisionMask = false;
                t.hasPaintedThisFrame = false;

                if (lastObject) vertexPaint.shouldExit = false;
            }
                        
            return;
        }

        ApplyVertexPaint(deformBody);
    }
    
    void UpdateVertexPaintingColors(DeformBody body)
    {
        if (deformBody.paintFixedVertices)
        {
            vertexPaint.SetData(body.GetVertices(), body.transform,
                                body.fixedVertices, new Vector3(1, 0, 0));
		}
        else if (deformBody.paintKineticFriction)
		{
			vertexPaint.SetData(body.GetVertices(), body.transform,
                                body.kfriction, 1, new Vector3(0, 1, 0));
		}
        else if (deformBody.paintStaticFriction)
		{
			vertexPaint.SetData(body.GetVertices(), body.transform,
                                body.sfriction, 5, new Vector3(0, 0, 1));
		}
        else if (deformBody.paintSelfCollisionMask)
        {
            vertexPaint.SetData(body.GetVertices(), body.transform,
                                body.selfCollisionMask, colors);
        }
        else if (deformBody.paintExternalCollisionMask)
        {
            vertexPaint.SetData(body.GetVertices(), body.transform,
                                body.externalCollisionMask, colors);
        }
    }

    void ApplyVertexPaint(DeformBody body)
    {
        foreach (int i in recentlyPaintedVertices)
        {
            if (deformBody.paintFixedVertices)
            {
                body.fixedVertices[i] = vertexPaint.shouldErase ? false : true;
            }
            else if (deformBody.paintKineticFriction)
            {
                body.kfriction[i] = vertexPaint.shouldErase ? 0 : paintAmount;
            }
            else if (deformBody.paintStaticFriction)
            {
                body.sfriction[i] = vertexPaint.shouldErase ? 0 : paintAmount;
            }
            else if (deformBody.paintSelfCollisionMask)
            {
                body.selfCollisionMask[i] = vertexPaint.shouldErase ? 0 : body.currentCollisionMaskColor;
            }
            else if (body.paintExternalCollisionMask)
            {
                body.externalCollisionMask[i] = vertexPaint.shouldErase ? 0 : body.currentCollisionMaskColor;
            }
        }
    }

    void ApplyVertexPaintToAllVertices(DeformBody body)
    {
        for (int i = 0; i < body.GetVertexCount(); i++)
        {
            if (deformBody.paintFixedVertices)
            {
                body.fixedVertices[i] = vertexPaint.shouldErase ? false : true;
            }
            else if (deformBody.paintKineticFriction)
            {
                body.kfriction[i] = vertexPaint.shouldErase ? 0 : paintAmount;
            }
            else if (deformBody.paintStaticFriction)
            {
                body.sfriction[i] = vertexPaint.shouldErase ? 0 : paintAmount;
            }
            else if (deformBody.paintSelfCollisionMask)
            {
                body.selfCollisionMask[i] = vertexPaint.shouldErase ? 0 : body.currentCollisionMaskColor;
            }
            else if (deformBody.paintExternalCollisionMask)
            {
                body.externalCollisionMask[i] = vertexPaint.shouldErase ? 0 : body.currentCollisionMaskColor;
            }
        }

        UpdateVertexPaintingColors(body);
    }

    void ClearVertexPaint(DeformBody body)
    {
        for (int i = 0; i < body.GetVertexCount(); i++)
        {
            if (deformBody.paintFixedVertices)
            {
                body.fixedVertices[i] = false;
            }
            else if (deformBody.paintKineticFriction)
            {
                body.kfriction[i] = 0;
            }
            else if (deformBody.paintStaticFriction)
            {
                body.sfriction[i] = 0;
            }
            else if (deformBody.paintSelfCollisionMask)
            {
                body.selfCollisionMask[i] = 0;
            }
            else if (deformBody.paintExternalCollisionMask)
            {
                body.externalCollisionMask[i] = 0;
            }
        }

        UpdateVertexPaintingColors(body);
    }

	[MenuItem("GameObject/Deform Dynamics/Deform Body", false, 0)]
    static void CreateDeformBody(MenuCommand menuCommand)
    {
        GameObject c = new GameObject("DeformBody");
        GameObjectUtility.SetParentAndAlign(c, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(c, "Create Deform Body");
        c.AddComponent<MeshFilter>();
        c.AddComponent<MeshRenderer>();
        c.AddComponent<DeformBody>();
        Selection.activeObject = c;
    }
    
    private enum VertexPaintingMode { FIXED, KINETIC_FRICTION, STATIC_FRICTION };

    // Activate a vertex painting mode for all DeformBody objects in the scene.
    private static void ActivateVertexPaintingMode(VertexPaintingMode mode)
    {
        DeformBody[] deformBodies = FindObjectsOfType<DeformBody>();
        GameObject[] gameObjects = new GameObject[deformBodies.Length];

        for (int i = 0; i < deformBodies.Length; i++)
        {
            deformBodies[i].paintFixedVertices = false;
            deformBodies[i].paintKineticFriction = false;
            deformBodies[i].paintStaticFriction = false;

            gameObjects[i] = deformBodies[i].gameObject;
        }
        
        Selection.objects = gameObjects;
        Selection.selectionChanged();

        for (int i = 0; i < Selection.objects.Length; i++)
        {
            GameObject g = (GameObject)Selection.objects[i];
            DeformBody body = g.GetComponent<DeformBody>();

            switch (mode)
            {
                case VertexPaintingMode.FIXED: body.paintFixedVertices = true; break;
                case VertexPaintingMode.KINETIC_FRICTION: body.paintKineticFriction = true; break;
                case VertexPaintingMode.STATIC_FRICTION: body.paintStaticFriction = true; break;
            }

            body.vertexPaintingModeChanged = true;
        }

        SceneView.RepaintAll();
    }

    [MenuItem(FIXED_MENU_NAME)]
    private static void SetPaintFixedVerticesActive()
    {
        ActivateVertexPaintingMode(VertexPaintingMode.FIXED);
    }

    [MenuItem(KINETIC_FRICTION_MENU_NAME)]
    private static void SetPaintKineticFrictionActive()
    {
        ActivateVertexPaintingMode(VertexPaintingMode.KINETIC_FRICTION);
    }

    [MenuItem(STATIC_FRICTION_MENU_NAME)]
    private static void SetPaintStaticFrictionActive()
    {
        ActivateVertexPaintingMode(VertexPaintingMode.STATIC_FRICTION);
    }
}
