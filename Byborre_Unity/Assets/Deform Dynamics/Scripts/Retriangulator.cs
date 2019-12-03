using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyButtons;
using System.Runtime.InteropServices;
using DeformDynamics;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
public class Retriangulator : MonoBehaviour
{
    public float triangleSize;
    public bool reverse;
    public bool use_z;

    [Button]
    void Execute()
    {
        // Retrieve contour points
        Mesh inputMesh = GetComponent<MeshFilter>().sharedMesh;

        DeformPlugin.Utils.FindMeshContours(inputMesh.vertices, inputMesh.vertices.Length,
                                            inputMesh.triangles,
                                            inputMesh.triangles.Length,
                                            0, 0, 0, 0, 0, 0,
                                            out int numContours,
                                            out int numContourPoints);

        int [] contourPointsArray = new int[numContourPoints];
        var cviHandle = GCHandle.Alloc(contourPointsArray, GCHandleType.Pinned);
        var cviPtr = cviHandle.AddrOfPinnedObject();

        int[] contourPointCountsArray = new int[numContours];
        var cpcHandle = GCHandle.Alloc(contourPointCountsArray, GCHandleType.Pinned);
        var cpcPtr = cpcHandle.AddrOfPinnedObject();

        DeformPlugin.Utils.RetrieveMeshContours(cviPtr, cpcPtr);

        cviHandle.Free();
        cpcHandle.Free();

        Vector2[] points = new Vector2[contourPointsArray.Length];
        Vector3[] vertices = inputMesh.vertices;
        
        for (int i = 0; i < contourPointsArray.Length; i++)
        {
            Vector3 v = vertices[contourPointsArray[i]];

            if (reverse)
            {
                points[contourPointsArray.Length - 1 - i] = new Vector2(use_z ? v.z : v.x, v.y);
            }
            else
            {
                points[i] = new Vector2(use_z ? v.z : v.x, v.y);
            }
        }
                
        // Triangulate
        int numVertices, numOutputTriangles;

        DeformPlugin.DXF.TriangulateShape(points, contourPointsArray.Length, numContours, contourPointCountsArray, triangleSize,
                         25, out numVertices, out numOutputTriangles);

        // Add check if numVertices is above 65535

        var meshVertices = new Vector3[numVertices];
        var meshVerticesHandle = GCHandle.Alloc(meshVertices, GCHandleType.Pinned);
        var meshVerticesPtr = meshVerticesHandle.AddrOfPinnedObject();

        var meshTriangles = new int[numOutputTriangles];
        var meshTrianglesHandle = GCHandle.Alloc(meshTriangles, GCHandleType.Pinned);
        var meshTrianglesPtr = meshTrianglesHandle.AddrOfPinnedObject();

        DeformPlugin.DXF.RetrieveTriangulatedShape(meshVerticesPtr, meshTrianglesPtr);

        meshVerticesHandle.Free();
        meshTrianglesHandle.Free();

        Mesh result = new Mesh();
        result.vertices = meshVertices;
        result.triangles = meshTriangles;
        result.RecalculateNormals();
        result.RecalculateBounds();
        result.name = inputMesh.name + "_r";
        
        var savePath = "Assets/M-XR/Models/Garment/DD/" + result.name + ".asset";

        AssetDatabase.CreateAsset(result, savePath);
    }
}
