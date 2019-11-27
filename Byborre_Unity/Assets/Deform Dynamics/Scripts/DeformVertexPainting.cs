//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System.Runtime.InteropServices;

////#if UNITY_EDITOR
//[InitializeOnLoad]
//public class DeformVertexPainting : MonoBehaviour
//{
//    struct VertexPaintParticle
//    {
//        public Vector3 vertex;
//        public Vector3 color;
//    }

//    private const string MENU_NAME = "Deform Dynamics/Vertex Painting";

//    public static bool active = false;

//    public const int FIXED_LAYER = 0;
//    public const int KFRICTION_LAYER = 1;
//    public const int SFRICTION_LAYER = 2;

//    private const float ORIGINAL_PARTICLE_SIZE = 0.005f;

//    private const float MIN_PARTICLE_SIZE = 0.0005f;
//    private const float MAX_PARTICLE_SIZE = 0.05f;

//    static private float radius = 30;
//    private uint selectedLayer;

//    private static VertexPaintParticle[] vertexPaintParticles;

//    private List<int> paintedVertices;
//    private int[] visited;

//    // GUIContent in order to enable tooltips
//    private static readonly GUIContent[] layerStrings = { new GUIContent("Fix",       "Paint fixed vertices"),
//                                                          new GUIContent("KFriction", "Paint kinetic friction"),
//                                                          new GUIContent("SFriction", "Paint static friction")};
    
//    static int layer = 0;
    
//    static float particleSize = 0.005f;

//    static float paintAmount = 1.0f;

//    static bool shouldErase = false;
//    static bool onlyPaintVisibleParticles = false;

//    static bool shouldClearPaintedVertices;
//    static bool shouldApplyOnAllVertices;

//    List<int> recentlyPaintedVertices;

//    static Texture2D brushTexture;

//    static Rect uirect;

//    static Material particleMaterial;
//    static ComputeBuffer buffer;

//    static DeformBody[] bodies;

//    static DeformVertexPainting()
//    {
//        brushTexture = Resources.Load<Texture2D>("Images/Circle");
//        particleMaterial = Resources.Load<Material>("Materials/ParticleGeometryMaterial");

//        SceneView.onSceneGUIDelegate += onSceneGUI =>
//        {
//            if (!active || Application.isPlaying)
//            {
//                Menu.SetChecked(MENU_NAME, false);
//                return;
//            }

//            int guiControl = GUIUtility.GetControlID(FocusType.Passive);

//            Handles.BeginGUI();

//            GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);

//            if (Event.current.type == EventType.Repaint)
//            {
//                uirect = GUILayout.Window(0, uirect, DrawUIWindow, "Vertex paint editor");

//                //10 and 28 are magic values, since Screen size is not exactly right.
//                uirect.x = Screen.width / EditorGUIUtility.pixelsPerPoint - uirect.width - 10;
//                uirect.y = Screen.height / EditorGUIUtility.pixelsPerPoint - uirect.height - 28;

//                GUI.DrawTexture(new Rect(Event.current.mousePosition.x - radius,
//                                         Event.current.mousePosition.y - radius,
//                                         radius * 2, radius * 2), brushTexture);
//            }

//            if (Event.current.type == EventType.MouseMove)
//            {
//                SceneView.RepaintAll();
//            }

//            DrawParticles(guiControl);

//            GUILayout.Window(0, uirect, DrawUIWindow, "Vertex paint editor");
//            Handles.EndGUI();
//        };
//    }

//    static void DrawUIWindow(int windowId)
//    {
//        EditorGUI.BeginChangeCheck();

//        GUILayout.BeginHorizontal();
//        layer = GUILayout.Toolbar(layer, layerStrings);
//        GUILayout.EndHorizontal();

//        if (EditorGUI.EndChangeCheck())
//        {
//            //Regenerate particles when changing layer
//            //body.SetVertexPaintLayer(layer);
//            //body.UpdateRenderedVertices(layer, false);
//        }

//        GUILayout.BeginHorizontal();
//        GUILayout.Label("Brush radius");
//        radius = GUILayout.HorizontalSlider(radius, 10, 100, GUILayout.Width(100));
//        GUILayout.EndHorizontal();

//        GUILayout.BeginHorizontal();
//        GUILayout.Label("Particle size");
//        //body.particleSize = GUILayout.HorizontalSlider(body.particleSize, MIN_PARTICLE_SIZE, MAX_PARTICLE_SIZE, GUILayout.Width(100));
//        GUILayout.EndHorizontal();

//        if (layer == KFRICTION_LAYER)
//        {
//            GUILayout.BeginHorizontal();
//            GUILayout.Label("Friction: " + paintAmount.ToString("F2"));
//            paintAmount = GUILayout.HorizontalSlider(paintAmount, 0, 1, GUILayout.Width(100));
//            GUILayout.EndHorizontal();
//        }
//        else if (layer == SFRICTION_LAYER)
//        {
//            GUILayout.BeginHorizontal();
//            GUILayout.Label("Friction: " + paintAmount.ToString("F2"));
//            paintAmount = GUILayout.HorizontalSlider(paintAmount, 0, 5, GUILayout.Width(100));
//            GUILayout.EndHorizontal();
//        }
//        else
//        {
//            GUILayout.BeginVertical();
//            GUILayout.Space(18);
//            GUILayout.EndVertical();
//        }

//        //GUILayout.BeginHorizontal();
//        //GUILayout.Label("Only paint visible");
//        //onlyPaintVisibleParticles = GUILayout.Toggle(onlyPaintVisibleParticles, "");
//        //GUILayout.EndHorizontal();

//        GUILayout.BeginHorizontal();
//        GUILayout.Label("Erase                 ");

//        shouldErase = GUILayout.Toggle(shouldErase, new GUIContent("", "tooltip"));

//        GUILayout.EndHorizontal();

//        GUILayout.BeginHorizontal();
//        shouldApplyOnAllVertices = GUILayout.Button("Apply on all vertices");
//        GUILayout.EndHorizontal();

//        GUILayout.BeginHorizontal();
//        shouldClearPaintedVertices = GUILayout.Button("Clear layer");
//        GUILayout.EndHorizontal();
//    }

//    static void UpdateVertexPaintParticles()
//    {
//        if (vertexPaintParticles == null)
//        {
//            vertexPaintParticles = new VertexPaintParticle[1000];
//        }

//        VertexPaintParticle vpp;

//        for (int i = 0; i < 1000; i++)
//        {
//            vpp = vertexPaintParticles[i];

//            vpp.vertex = new Vector3(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
//            vpp.color = Vector3.one;
//        }
//    }

//    static void DrawParticles(int guiControl)
//    {
//        bodies = FindObjectsOfType<DeformBody>();

//        for (int i = 0; i < bodies.Length; i++)
//        {
//            Vector3[] vertices = bodies[i].GetVertices();
            
//            if (vertices != null)
//            {
//                for (int j = 0; j < vertices.Length; j++)
//                {
//                    Handles.SphereHandleCap(guiControl, vertices[j], Quaternion.identity, 0.01f, EventType.Repaint);
//                }
//            }
//        }

//        //UpdateVertexPaintParticles();
//        //
//        //if (vertexPaintParticles.Length > 0)
//        //{
//        //    buffer = new ComputeBuffer(vertexPaintParticles.Length, Marshal.SizeOf(typeof(VertexPaintNode)), ComputeBufferType.Default);
//        //    buffer.SetData(vertexPaintParticles);
//        //
//        //    particleMaterial.SetBuffer("points", buffer);
//        //    particleMaterial.SetFloat("_Size", particleSize);
//        //
//        //    particleMaterial.SetPass(0);
//        //    Graphics.DrawProcedural(MeshTopology.Points, vertexPaintParticles.Length);
//        //
//        //    buffer.Release();
//        //}
//    }

//    [MenuItem(MENU_NAME)]
//    private static void SetSewingActive()
//    {
//        active = !active;

//        // Set checkmark on menu item
//        Menu.SetChecked(MENU_NAME, active);
//        // Saving editor state
//        EditorPrefs.SetBool(MENU_NAME, active);

//        SceneView.RepaintAll();
//    }
//}
////#endif