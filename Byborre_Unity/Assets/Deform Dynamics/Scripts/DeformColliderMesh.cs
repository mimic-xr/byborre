using System.Collections.Generic;
using UnityEngine;
using DeformDynamics;
using System.Runtime.InteropServices;
using System;

[AddComponentMenu("Deform Dynamics/Deform Collider/Deform Collider Mesh")]
public class DeformColliderMesh : DeformCollider
{
	[SerializeField]
    bool skinned = false;

    [SerializeField, HideInInspector]
    public int[] collisionMask;

    [HideInInspector]
    public int currentCollisionMaskColor = 0;

    [HideInInspector]
    public bool paintCollisionMask;

    Vector3[] vertexArray;
    Vector3[] normalArray;

    List<Vector3> vertexList;
    List<Vector3> normalList;

    GCHandle verticesHandle;
    IntPtr verticesPtr;

    GCHandle normalsHandle;
    IntPtr normalsPtr;

    Mesh oldMesh;

    [SerializeField]
    SkinnedMeshRenderer skinnedMeshRenderer;
    
    public enum RigType { Generic, Humanoid };
    public RigType rigType = RigType.Generic;

	public bool remesh = false;
	public int numRemeshedVertices = 6000;

    private bool initialized = false;
    private bool meshErrorShown = false;

    protected override void UpdateMeshes()
	{
		base.UpdateMeshes();

        if (GetComponent<SkinnedMeshRenderer>())
        {
            skinned = true;
            skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

            Mesh currentMesh = skinnedMeshRenderer.sharedMesh;

            if (currentMesh && !currentMesh.isReadable)
            {
                if (!meshErrorShown)
                {
                    Debug.LogError("The supplied mesh (" + currentMesh.name + ") is not readable and cannot be used by DeformColliderMesh. " +
                                   "Please enable 'Read/Write Enabled' in the mesh import settings.");
                    meshErrorShown = true;
                }

                initialized = false;
                return;
            }

            if (currentMesh != null && !currentMesh.Equals(oldMesh))
            {
                int numVertices = currentMesh.vertexCount;

                vertexList = new List<Vector3>(numVertices);
                normalList = new List<Vector3>(numVertices);

                vertexArray = new Vector3[numVertices];
                normalArray = new Vector3[numVertices];

                if (!Application.isPlaying && oldMesh != null)
                {
                    collisionMask = new int[numVertices];
                }

                oldMesh = currentMesh;
            }

            UpdateSkinnedMesh();
        }
        else if (GetComponent<MeshFilter>())
        {
            if (GetComponent<MeshFilter>().sharedMesh && !GetComponent<MeshFilter>().sharedMesh.Equals(oldMesh))
            {
                colliderMesh = Instantiate(GetComponent<MeshFilter>().sharedMesh);

                vertexArray = colliderMesh.vertices;
                normalArray = colliderMesh.normals;
                
                if (!Application.isPlaying && oldMesh != null)
                {
                    collisionMask = new int[colliderMesh.vertexCount];
                }

                oldMesh = colliderMesh;
            }

            for (int i = 0; i < vertexArray.Length; i++)
            {
                vertexArray[i].x *= transform.lossyScale.x;	// TODO: Move to scale collider?
                vertexArray[i].y *= transform.lossyScale.y;
                vertexArray[i].z *= transform.lossyScale.z;
            }
        }
        else
        {
            initialized = false;
            return;
        }

        colliderMesh.vertices = vertexArray;
        colliderMesh.RecalculateBounds();

        initialized = true;
    }

	public override void AddToSimulation()
	{
        if (!initialized) return;

        verticesHandle = GCHandle.Alloc(vertexArray, GCHandleType.Pinned);
        verticesPtr = verticesHandle.AddrOfPinnedObject();

        normalsHandle = GCHandle.Alloc(normalArray, GCHandleType.Pinned);
        normalsPtr = normalsHandle.AddrOfPinnedObject();

        int numIndices = 0;

        for (int i = 0; i < colliderMesh.subMeshCount; i++)
        {
            numIndices += (int)colliderMesh.GetIndexCount(i);
        }

        if (remesh)
		{
            DeformPlugin.Collider.CreateMeshColliderRemeshed(colliderMesh.vertices, colliderMesh.normals, colliderMesh.vertexCount,
										                     colliderMesh.triangles, numIndices, numRemeshedVertices, bias, 
                                                             kineticFriction, staticFriction, out id);
            DeformPlugin.Collider.UpdateMeshCollider(id, verticesPtr, normalsPtr, colliderMesh.vertexCount);
            
        }
		else
		{
            DeformPlugin.Collider.CreateMeshCollider(colliderMesh.vertices, colliderMesh.normals, colliderMesh.vertexCount,
							                         colliderMesh.triangles, numIndices, bias, kineticFriction, staticFriction, out id);
		}

        if (collisionMask == null || collisionMask.Length == 0)
        {
            collisionMask = new int[colliderMesh.vertexCount];
        }

        DeformPlugin.Collider.SetMeshColliderMask(id, collisionMask);
        
		float angle;
		Vector3 axis;

		transform.rotation.ToAngleAxis(out angle, out axis);
		angle *= -Mathf.Deg2Rad;

        DeformPlugin.Collider.RotateCollider(id, angle, axis.x, axis.y, axis.z, 0, 0, 0);
        DeformPlugin.Collider.MoveCollider(id, transform.position.x, transform.position.y, transform.position.z);
	}

	public override void UpdateInSimulation()
	{
        if (initialized && skinned)
        {
            UpdateSkinnedMesh();

            if (rigType == RigType.Humanoid)
            {
                DeformPlugin.Collider.SetMeshColliderTransform(id, transform.position, Quaternion.Inverse(transform.rotation), Vector3.zero, transform.lossyScale);
            }

            DeformPlugin.Collider.UpdateMeshCollider(id, verticesPtr, normalsPtr, bias);
        }
    }

	private void UpdateSkinnedMesh()
	{
		if (skinnedMeshRenderer == null || colliderMesh == null || vertexList == null) return;

        skinnedMeshRenderer.BakeMesh(colliderMesh);

        colliderMesh.GetVertices(vertexList);
		vertexList.CopyTo(vertexArray);

		colliderMesh.GetNormals(normalList);
		normalList.CopyTo(normalArray);

        if (!Application.isPlaying)
        {
            for (int i = 0; i < vertexArray.Length; i++)
            {
                vertexArray[i] += normalArray[i].normalized * bias;
            }
        }
    }

    public Vector3[] GetVertices()
    {
        return colliderMesh.vertices;
    }

    public int GetVertexCount()
    {
        return colliderMesh.vertexCount;
    }
}
