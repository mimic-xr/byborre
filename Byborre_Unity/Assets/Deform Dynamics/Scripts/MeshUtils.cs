using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using DeformDynamics;

public class MeshUtils : MonoBehaviour {

    private const int MAX_VERTICES_PER_MESH = 65000;
    private const int MAX_CUBES_PER_MESH = 2708; // 65000 / 24
    private const int VERTICES_PER_CUBE = 24;
    private const int INDICES_PER_CUBE = 36;

	/**
     * Given the size and resolution of a patch, retrieves the number of vertices, number of triangles and the exact resolutions in both axes.
     **/
    public static void GetPatchInfo(Vector2 size, uint res, out uint numVertices, out uint numIndices, out uint xRes, out uint yRes)
    {
        xRes = Mathf.Max(size.x, size.y) == size.x ? res : (uint)(res * (size.x / size.y) + 0.5f);
        yRes = Mathf.Max(size.x, size.y) == size.y ? res : (uint)(res * (size.y / size.x) + 0.5f);

        numVertices = xRes * yRes;
        numIndices = (xRes - 1) * (yRes - 1) * 6;
    }

    /**
     * Given the size and resolution of a patch, retrieves the number of vertices.
     */
    public static uint NumVerticesOfPatch(Vector2 size, uint res)
    {
        uint numVertices, numIndices, xRes, yRes;
        GetPatchInfo(size, res, out numVertices, out numIndices, out xRes, out yRes);
        return numVertices;
    }

    /**
     * Given the size and resolution of a patch, creates a patch mesh and stores it in "mesh".
     **/
    public static void CreateClothMesh(Vector2 size, uint resolution, Mesh mesh)
    {
        uint numVertices, numIndices, xRes, yRes;

        
        GetPatchInfo(size, resolution, out numVertices, out numIndices, out xRes, out yRes);

		if(numVertices > 65535)
		{
			Debug.LogWarning("Too many vertices. Try lowering the resolution of the cloth");
			return;
		}

		mesh.Clear();

		Vector3[] vertices = new Vector3[numVertices];
        Vector3[] normals  = new Vector3[numVertices];
        Vector2[] uvs      = new Vector2[numVertices];
        int[] triangles    = new int[numIndices];

        GCHandle verticesHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
        IntPtr verticesPtr = verticesHandle.AddrOfPinnedObject();

        GCHandle trianglesHandle = GCHandle.Alloc(triangles, GCHandleType.Pinned);
        IntPtr trianglesPtr = trianglesHandle.AddrOfPinnedObject();
		
		DeformPlugin.Utils.CreatePatch(size.x, size.y, resolution, verticesPtr, trianglesPtr);

		verticesHandle.Free();
        trianglesHandle.Free();
        
        int texCoordColumn = -1;
        int texCoordRow = -1;

        for (int i = 0; i < numVertices; i++)
        {
            normals[i] = Vector3.up;

            if (i % xRes == 0)
            {
                texCoordColumn = 0;
                texCoordRow++;
            }

            float uvX = (float)texCoordColumn++ / (float)(xRes - 1);
            float uvY = (float)texCoordRow      / (float)(yRes - 1);

            uvs[i] = new Vector2(uvX, uvY);
        }
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.name = "Deform Patch";
        
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
    }

	public static bool MeshesIdentical(Mesh a, Mesh b)
    {
        if (a == null || b == null) return false;

        a.RecalculateBounds();
        b.RecalculateBounds();

        return a.vertexCount == b.vertexCount &&
               a.GetIndexCount(0) == b.GetIndexCount(0) &&
               a.bounds == b.bounds;
    }

	public static bool MeshTriangleListIdentical(Mesh a, Mesh b)
	{
		if (a == null || b == null) return false;

		if (a.triangles.Length != b.triangles.Length) return false;

		return Enumerable.SequenceEqual(a.triangles, b.triangles);
	}
}
