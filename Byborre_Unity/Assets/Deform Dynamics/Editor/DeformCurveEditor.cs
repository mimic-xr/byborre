//using UnityEditor;
//using UnityEngine;
//using DeformDynamics;
//using System.Collections.Generic;

//#if UNITY_EDITOR
//[InitializeOnLoad]
//public class DeformCurveEditor : MonoBehaviour
//{
//    private static readonly int NUM_SAMPLES = 1024;
//    private static Mesh mesh;
//    private static Material material;
    
//    static DeformCurveEditor()
//    {
//        SceneView.duringSceneGui += onSceneGUI =>
//        {
//            DrawCurves();
//        };
//    }

//    static void DrawCurves()
//    {
//        DeformPlugin.InitializePlugin(Application.dataPath + "/Plugins/deform_config.xml");

//        int numPaths = DeformPlugin.BezierPath.GetNumPaths();

//        if (numPaths == 0) mesh = null;

//        if (mesh == null && numPaths > 0)
//        {
//            mesh = new Mesh();
//            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

//            Vector3[] vertices = new Vector3[numPaths * NUM_SAMPLES];
//            List<int> indices = new List<int>();

//            for (int i = 0; i < numPaths; i++)
//            {
//                for (int j = 0; j < NUM_SAMPLES; j++)
//                {
//                    float t = (float) j / (NUM_SAMPLES - 1);

//                    DeformPlugin.BezierPath.GetPointOnPathUniformDistance(i, t, out vertices[i * NUM_SAMPLES + j].x, 
//                                                                                out vertices[i * NUM_SAMPLES + j].y, 
//                                                                                out vertices[i * NUM_SAMPLES + j].z);
//                }

//                for (int j = 0; j < NUM_SAMPLES; j++)
//                {
//                    indices.Add(i * NUM_SAMPLES + j);

//                    if (j == 0 || j == (NUM_SAMPLES - 1)) continue;

//                    indices.Add(i * NUM_SAMPLES + j);
//                }
//            }

//            mesh.vertices = vertices;
            
//            mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
//            mesh.RecalculateBounds();
//        }

//        if (material == null)
//        {
//            material = Resources.Load<Material>("Materials/CurveMaterial");
//        }

//        if (mesh != null)
//        {
//            material.SetPass(0);
//            Graphics.DrawMeshNow(mesh, Vector3.zero, Quaternion.identity);            
//        }
//    }
//}
//#endif