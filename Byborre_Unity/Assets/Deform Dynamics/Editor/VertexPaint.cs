using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;

public struct RenderedVertex
{
	public Vector3 vertex;
	public Vector3 color;
}

public enum Brush { Constant, Linear };

public sealed class VertexPaint : Editor
{
    private static VertexPaint instance = null;
    private static readonly object padlock = new object();

    private const float ORIGINAL_PARTICLE_SIZE = 0.005f;

    private const float MIN_PARTICLE_SIZE = 0.0005f;
    private const float MAX_PARTICLE_SIZE = 0.05f;

    private float radius = 30;

    float particleSize = 0.005f;
    
    private List<int> paintedVertices;

    // TODO: Change these into some kind of messages?
    public bool shouldErase = false;
    public bool shouldClearPaintedVertices = false;
    public bool shouldApplyOnAllVertices = false;
    public bool shouldExit = false;

    public Brush brush = Brush.Constant;
    public float[] weights;

    Rect uirect;
    
    private RenderedVertex[] renderedVertices;

    private Material particleMaterial;

    public static VertexPaint Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = CreateInstance<VertexPaint>();
                    instance.paintedVertices = new List<int>();
                }

                return instance;
            }
        }
    }

    public List<int> HandlePaintEvents(Object paintedObject, bool firstObject, bool lastObject, Texture2D brushTexture)
    {
        if (weights == null || weights.Length != renderedVertices.Length)
        {
            weights = new float[renderedVertices.Length];
        }

        if (renderedVertices != null && renderedVertices.Length > 0)
        {
            ComputeBuffer buffer = new ComputeBuffer(renderedVertices.Length, Marshal.SizeOf(typeof(RenderedVertex)), ComputeBufferType.Default);
            buffer.SetData(renderedVertices);

            particleMaterial.SetBuffer("points", buffer);
            particleMaterial.SetFloat("_Size", particleSize);

            particleMaterial.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Points, renderedVertices.Length);

            buffer.Release();
        }

        Handles.BeginGUI();

        GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);        

        if (Event.current.type == EventType.Repaint)
        {
            uirect = GUILayout.Window(0, uirect, DrawUIWindow, "Vertex paint options");

            //10 and 28 are magic values, since Screen size is not exactly right.
            uirect.x = Screen.width / EditorGUIUtility.pixelsPerPoint - uirect.width - 10;
            uirect.y = Screen.height / EditorGUIUtility.pixelsPerPoint - uirect.height - 28;

            GUI.DrawTexture(new Rect(Event.current.mousePosition.x - radius,
                                     Event.current.mousePosition.y - radius,
                                     radius * 2, radius * 2), brushTexture);
        }

        paintedVertices.Clear();

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            if (firstObject)
            {
                int guiControl = GUIUtility.GetControlID(FocusType.Passive);
                GUIUtility.hotControl = guiControl;
            }

            if (!Application.isPlaying)
            {
                Undo.RecordObject(paintedObject, "Painted vertices");
            }
            
            PaintParticles();

            if (lastObject)
            {
                GUIUtility.hotControl = 0;
                Event.current.Use();
            }
        }

        if (Event.current.type == EventType.MouseMove)
        {
            SceneView.RepaintAll();
        }

        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            PaintParticles();
            if (lastObject) Event.current.Use();
        }
        
        GUILayout.Window(0, uirect, DrawUIWindow, "Vertex paint editor");
        Handles.EndGUI();
        
        return paintedVertices;        
    }

    void PaintParticles()
    {
        Vector2 mousePos = Event.current.mousePosition;

        Camera cam = Camera.current;
        Vector3 camForward = cam.transform.forward;

        for (int i = 0; i < renderedVertices.Length; i++)
        {
            Vector3 pos = renderedVertices[i].vertex;
            
            Vector2 guiPos = HandleUtility.WorldToGUIPoint(pos);
            float distance = (mousePos - guiPos).magnitude;

            if (distance < radius)
            {
                paintedVertices.Add(i);

                if (brush == Brush.Constant)
                {
                    weights[i] = 1;
                }
                else if (brush == Brush.Linear)
                {
                    weights[i] = Mathf.Max(weights[i], 1 - (distance / radius));
                }                
            }
        }
    }

    void DrawUIWindow(int windowId)
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Brush radius");
        radius = GUILayout.HorizontalSlider(radius, 10, 100, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Particle size");
        particleSize = GUILayout.HorizontalSlider(particleSize, MIN_PARTICLE_SIZE, MAX_PARTICLE_SIZE, GUILayout.Width(100));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Erase                 ");
        shouldErase = GUILayout.Toggle(shouldErase, "");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        shouldApplyOnAllVertices = GUILayout.Button("Apply on all vertices");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        shouldClearPaintedVertices = GUILayout.Button("Clear layer");
        GUILayout.EndHorizontal();

        GUILayout.Space(18);

        GUILayout.BeginHorizontal();
        shouldExit = GUILayout.Button("Close vertex paint editor");
        GUILayout.EndHorizontal();
    }

    public void SetData(Vector3[] meshVertices, Transform t, float[] data, float range, Vector3 base_color)
    {
        renderedVertices = new RenderedVertex[meshVertices.Length];
        
        Vector3 c = Vector3.one - base_color;

        for (int i = 0; i < renderedVertices.Length; i++)
        {
            renderedVertices[i].vertex = t.TransformPoint(meshVertices[i]);
            renderedVertices[i].color = Vector3.one - (data[i] / range) * c.normalized;
        }
    }

    public void SetData(Vector3[] meshVertices, Transform t, int[] data, Vector3 base_color)
    {
        renderedVertices = new RenderedVertex[meshVertices.Length];

        Vector3 c = Vector3.one - base_color;

        for (int i = 0; i < renderedVertices.Length; i++)
        {
            renderedVertices[i].vertex = t.TransformPoint(meshVertices[i]);
            renderedVertices[i].color = Vector3.one - (data[i] * c.normalized);
        }
    }

    public void SetData(Vector3[] meshVertices, int[] data, Vector3 base_color)
    {
        renderedVertices = new RenderedVertex[meshVertices.Length];

        Vector3 c = Vector3.one - base_color;

        for (int i = 0; i < renderedVertices.Length; i++)
        {
            renderedVertices[i].vertex = meshVertices[i];
            renderedVertices[i].color = Vector3.one - (data[i] * c.normalized);
        }
    }

    public void SetData(Vector3[] meshVertices, Transform t, int[] data, Vector3[] base_colors)
    {
        renderedVertices = new RenderedVertex[meshVertices.Length];

        for (int i = 0; i < renderedVertices.Length; i++)
        {
            Vector3 c = Vector3.one - base_colors[data[i]];
            renderedVertices[i].vertex = t.TransformPoint(meshVertices[i]);
            renderedVertices[i].color = Vector3.one - (data[i] * c.normalized);
        }
    }

    public void SetData(Vector3[] meshVertices, int[] data, Vector3[] base_colors)
    {
        renderedVertices = new RenderedVertex[meshVertices.Length];

        for (int i = 0; i < renderedVertices.Length; i++)
        {
            Vector3 c = Vector3.one - base_colors[data[i]];
            renderedVertices[i].vertex = meshVertices[i];
            renderedVertices[i].color = Vector3.one - (data[i] * c.normalized);
        }
    }

    public void SetData(Vector3[] meshVertices, Transform t, bool[] data, Vector3 base_color)
    {
        renderedVertices = new RenderedVertex[meshVertices.Length];
        
        Vector3 c = Vector3.one - base_color;

        for (int i = 0; i < renderedVertices.Length; i++)
        {
            renderedVertices[i].vertex = t.TransformPoint(meshVertices[i]);
            renderedVertices[i].color = Vector3.one - (data[i] ? 1 : 0) * c.normalized;
        }
    }
    
    public void SetParticleMaterial(Material material)
    {
        particleMaterial = material;
    }
}
