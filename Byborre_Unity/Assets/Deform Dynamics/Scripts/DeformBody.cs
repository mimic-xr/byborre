using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using DeformDynamics;

[AddComponentMenu("Deform Dynamics/Deform Body")]
public class DeformBody : MonoBehaviour {

	/**
     * Determines the stiffness of the distance constraints.
     **/
	[Range(0, 1)]
	public float distanceStiffness = 1;

	/**
     * Determines the stiffness of the bending constraints.
     **/
	[Range(0, 1)]
	public float bendingStiffness = 0.005f;

    /**
     * If enabled, vertices in the same position will be merged
     **/
    [Tooltip("If enabled, vertices in the same position will be merged in the simulation")]
    public bool mergeCloseVertices = true;

    public Mesh simulationMesh;

    [HideInInspector]
    public int[] selfCollisionMask;

    [HideInInspector]
    public int[] externalCollisionMask;

    /**
     * Determines whether each vertex will be fixed (immobile) or not.
     **/
    [HideInInspector]
	public bool[] fixedVertices;

	/**
     * The kinetic friction coefficient for each particle
     **/
	[HideInInspector]
	public float[] kfriction;

	/**
	 * The kinetic friction coefficient for each particle
	 **/
	[HideInInspector]
	public float[] sfriction;
    
	/**
     * All points that belong to edges that are only referenced by one triangle. Used for sewing.
     **/
	[HideInInspector]
	public int[] contourPointsArray;

    /**
     * The number of points in each contour. Used for sewing.
     **/
    [HideInInspector]
    public int[] contourPointCountsArray;
    
	[SerializeField, HideInInspector]
	public int id;

	[SerializeField, HideInInspector]
	public Mesh renderMesh;

	[SerializeField, HideInInspector]
	public Mesh oldSimulationMesh;
    
    [SerializeField, HideInInspector]
    public bool vertexPaintingModeChanged = false;

    [SerializeField, HideInInspector]
    public bool paintSelfCollisionMask = false;

    [SerializeField, HideInInspector]
    public bool paintExternalCollisionMask = false;

    [SerializeField, HideInInspector]
    public int currentCollisionMaskColor = 0;

    [SerializeField, HideInInspector]
    public bool paintFixedVertices = false;

    [SerializeField, HideInInspector]
    public bool paintKineticFriction = false;

    [SerializeField, HideInInspector]
    public bool paintStaticFriction = false;

    [SerializeField, HideInInspector]
    public bool hasPaintedThisFrame = false;

    protected Vector3[] vertices;
	protected Vector3[] normals;
	protected int[] triangles;

	protected GCHandle verticesHandle;
	protected IntPtr verticesPtr;

	protected GCHandle normalsHandle;
	protected IntPtr normalsPtr;

	protected bool simulationHasEnded;
	protected bool initialized;

    public delegate void RenderMeshChangedAction(DeformBody body);
    public static event RenderMeshChangedAction OnRenderMeshChanged;

    public delegate void SimulationMeshChangedAction(DeformBody body);
    public static event SimulationMeshChangedAction OnSimulationMeshChanged;

    /**
     * A list of rest values of the distance constraints
     **/
    [SerializeField, HideInInspector]
	public List<float> rvDistance;

	/**
     * A list of rest values of the bending constraints
     **/
	[SerializeField, HideInInspector]
	public List<float> rvBending;

	/**
     * Register event handlers for the delegates
     **/
	protected void OnEnable()
    {
        DeformManager.PreSimulationStarted += Initialize;
        DeformManager.OnSimulationUpdated += OnSimulationUpdated;
        DeformManager.OnSimulationEnded += OnSimulationEnded;
    }

	/**
     * Unregister event handlers for the delegates
     **/
	protected void OnDisable()
    {
        DeformManager.PreSimulationStarted -= Initialize;
        DeformManager.OnSimulationUpdated -= OnSimulationUpdated;
        DeformManager.OnSimulationEnded -= OnSimulationEnded;
    }

	void Reset()
	{
        if (rvDistance == null || rvBending == null)
		{
			rvDistance = new List<float>();
			rvBending = new List<float>();
		}

        UpdateInputMeshes();
    }

    public virtual void OnValidate()
    {
		if (Application.isPlaying && initialized)
        {
			DeformPlugin.Object.SetDistanceStiffness(id, distanceStiffness);
			DeformPlugin.Object.SetBendingStiffness(id, bendingStiffness);
        }
    }
    
	void OnSimulationUpdated()
    {
        if (!initialized || !enabled) return;

		// Fetch the updated vertices
		DeformPlugin.Object.GetObjectRenderVerticesLocal(id, verticesPtr);
        //DeformPlugin.Object.GetObjectRenderVertices(id, verticesPtr);

        renderMesh.vertices = vertices;

        renderMesh.RecalculateNormals();
        renderMesh.RecalculateBounds();
    }

	void OnSimulationEnded()
    {
        simulationHasEnded = true;
    }

	private void OnDestroy()
    {
        if (initialized)
        {
            verticesHandle.Free();
        }

		initialized = false;
	}

	/**
     * Adds and initializes the DeformBody to the simulation engine
     **/
	protected virtual void Initialize()
	{
        // Disable vertex painting
        paintFixedVertices = false;
        paintKineticFriction = false;
        paintStaticFriction = false;
        paintSelfCollisionMask = false;
        paintExternalCollisionMask = false;
        
        UpdateInputMeshes();

        if (!renderMesh) return;

        InitializeRenderComponent();

        bool useProxyMesh = simulationMesh != null;

        if (simulationMesh && !simulationMesh.isReadable)
        {
            Debug.LogError("The supplied mesh (" + simulationMesh.name + ") is not readable and cannot be used by DeformBody. " +
                           "Please enable 'Read/Write Enabled' in the mesh import settings.");
            return;
        }
        else if (!simulationMesh)
        {
            // Use render mesh as simulation mesh
            simulationMesh = Instantiate(renderMesh);
            simulationMesh.name = "Simulation Mesh";
        }
        
        DeformModifierBend bend = GetComponentInChildren<DeformModifierBend>();
        
        if (bend != null)
        {
			// If a bend modifier exists, use its original mesh to create
			// a DeformBody in the simulation engine
            DeformPlugin.Object.CreateDeformableObject(
				bend.original_mesh.vertices, 
				bend.original_mesh.vertexCount,
				bend.original_mesh.triangles,
				bend.original_mesh.triangles.Length,
				out id
			);
        }
        else
        {
            // Create a DeformBody in the simulation engine
            DeformPlugin.Object.CreateDeformableObject(
				simulationMesh.vertices,
				simulationMesh.vertexCount,
				simulationMesh.triangles,
                simulationMesh.triangles.Length,
				out id
			);
        }            

		// If the rest value arrays have been set from anywhere
		// tell the engine to use those values
        if (rvDistance.Any() && rvBending.Any())
        {
            DeformPlugin.Object.SetObjectRestValues (
				id,
				rvDistance.ToArray(),
				(uint)rvDistance.Count,
				rvBending.ToArray(),
				(uint)rvBending.Count
			);
        }

        if (useProxyMesh)
        {
            DeformPlugin.Object.AddRenderMesh(id, renderMesh.vertices, renderMesh.vertexCount, renderMesh.triangles, renderMesh.triangles.Length);

            vertices = new Vector3[renderMesh.vertexCount];
        }
        else
        {
            vertices = new Vector3[simulationMesh.vertexCount];
        }

        if (mergeCloseVertices)
        {
            DeformPlugin.Seam.SewCloseVertices(id);
        }

        DeformPlugin.Collider.SetSelfCollisionMask(id, selfCollisionMask);
        DeformPlugin.Collider.SetExternalCollisionMask(id, externalCollisionMask);
        
		initialized = true;

        // Also used by DeformBodyVolumetric
        PostInitialize();
    }

    protected virtual void PostInitialize()
    {
        Transform t = transform;

        // Reset the transformation.
        // The Unity coordinates are in world space while the engine coordinates are in object space.
        if (t.lossyScale.x != 1 || t.lossyScale.y != 1 || t.lossyScale.z != 1)
        {
            DeformPlugin.Object.ScaleObject(id, t.lossyScale.x, t.lossyScale.y, t.lossyScale.z);
        }

        Vector4 rotation = GetRotation(t);

        // TODO: REPLACE WITH QUATERNION
        DeformPlugin.Object.RotateObject(id, rotation.w, rotation.x, rotation.y, rotation.z);
        DeformPlugin.Object.MoveObject(id, t.position.x, t.position.y, t.position.z);

        DeformPlugin.Object.SetDistanceStiffness(id, distanceStiffness);
        DeformPlugin.Object.SetBendingStiffness(id, bendingStiffness);

        DeformModifierBend bend = GetComponentInChildren<DeformModifierBend>();
        // If a bend modifier exists, set the vertices to the bent positions before starting the
        // simulation
        if (bend != null && renderMesh != null)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = t.TransformPoint(renderMesh.vertices[i]);

                DeformPlugin.Interaction.MoveObjectParticle(id, i, v.x, v.y, v.z);
            }
        }

        //t.position = Vector3.zero;
        //t.rotation = Quaternion.identity;
        //t.localScale = new Vector3(1, 1, 1);

        // Set the particle attributes such as invmass and friction
        for (int i = 0; i < fixedVertices.Length; i++)
        {
            if (fixedVertices[i])
            {
                DeformPlugin.Object.FixObjectParticle((uint)id, (uint)i);
            }
        }

        for (int i = 0; i < kfriction.Length; i++)
        {
            if (kfriction[i] > 0.0f)
            {
                DeformPlugin.Object.SetParticleFrictionKinetic(id, i, kfriction[i]);
            }
        }

        for (int i = 0; i < sfriction.Length; i++)
        {
            if (sfriction[i] > 0.0f)
            {
                DeformPlugin.Object.SetParticleFrictionStatic(id, i, sfriction[i]);
            }
        }

        verticesHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
        verticesPtr = verticesHandle.AddrOfPinnedObject();
    }

    public void UpdateInputMeshes()
    {
        if (Application.isPlaying) return;

        SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        Mesh inputMesh = null;

        // If SkinnedMeshRenderer exists, use that. Otherwise use MeshFilter if it exists.
        if (skinnedMeshRenderer != null)
        {
            inputMesh = RetrieveRenderMesh(skinnedMeshRenderer);
        }
        else if (meshFilter != null)
        {
            inputMesh = RetrieveRenderMesh(meshFilter);
        }

        if (inputMesh != null && !inputMesh.Equals(renderMesh))
        {
            renderMesh = inputMesh;
            renderMesh.MarkDynamic();

            if (!simulationMesh)
            {
                AllocateVertexArrays(renderMesh);
                UpdateMeshContourPoints();
                if (OnRenderMeshChanged != null) OnRenderMeshChanged(this);
            }
        }
        
        if (simulationMesh && simulationMesh != oldSimulationMesh)
        {
            AllocateVertexArrays(simulationMesh);
            UpdateMeshContourPoints();
            if (OnSimulationMeshChanged != null) OnSimulationMeshChanged(this);

            oldSimulationMesh = simulationMesh;

        }

        // Make sure that vertex arrays are allocated using render mesh when simulation mesh is removed
        if (renderMesh && !simulationMesh && simulationMesh != oldSimulationMesh)
        {
            AllocateVertexArrays(renderMesh);
            UpdateMeshContourPoints();
            if (OnRenderMeshChanged != null) OnRenderMeshChanged(this);
            oldSimulationMesh = simulationMesh;
        }
    }

    Mesh RetrieveRenderMesh(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        return skinnedMeshRenderer.sharedMesh;
    }

    Mesh RetrieveRenderMesh(MeshFilter meshFilter)
    {
        return meshFilter.sharedMesh;
    }

    void InitializeRenderComponent()
    {   
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        renderMesh = Instantiate(renderMesh);

        if (skinnedMeshRenderer != null)
        {
            skinnedMeshRenderer.sharedMesh = renderMesh;
        }
        else if (meshFilter != null)
        {
            meshFilter.mesh = renderMesh;
        }
    }

    /**
     * Allocate arrays that contain vertex data such as friction and invmass
     **/
    public void AllocateVertexArrays(Mesh mesh)
    {
        if (!mesh)
        {
            kfriction = new float[0];
            sfriction = new float[0];
            fixedVertices = new bool[0];

            selfCollisionMask = new int[0];
            externalCollisionMask = new int[0];

            vertices = new Vector3[0];
        }
        else
        {
            kfriction = new float[mesh.vertexCount];
            sfriction = new float[mesh.vertexCount];
            fixedVertices = new bool[mesh.vertexCount];

            selfCollisionMask = new int[mesh.vertexCount];
            externalCollisionMask = new int[mesh.vertexCount];

            vertices = new Vector3[mesh.vertexCount];
        }
    }
    
    /**
     * Returns the rotation in axis-angle format.
	 * The x, y, z components of the returned Vector4 constitute the axis, and w stores the angle.
     **/
    public Vector4 GetRotation(Transform t)
    {
        Vector3 axis;
        float angle;
        t.rotation.ToAngleAxis(out angle, out axis);
        return new Vector4(axis.x, axis.y, axis.z, -Mathf.Deg2Rad * angle);
    }

    /**
     *  Returns the simulation id of this DeformBody.
     **/
    public int GetId()
    {
        return id;
    }

	/**
	* Sets all vertices to either fixed or not fixed.
	**/
	public void SetAllFixedVertices(bool value)
	{
		for (var i = 0; i < fixedVertices.Length; i++)
		{
			fixedVertices[i] = value;
		}
	}

	/**
	 * Sets the friction of all vertices to a specified value.
	 **/
	public void SetAllKFrictionVertices(float value)
	{
		for (var i = 0; i < kfriction.Length; i++)
		{
			kfriction[i] = value;
		}
	}

	/**
     * Sets the friction of all vertices to a specified value.
     **/
	public void SetAllSFrictionVertices(float value)
	{
		for (var i = 0; i < sfriction.Length; i++)
		{
			sfriction[i] = value;
		}
	}

	public Vector3[] GetVertices()
    {
        UpdateInputMeshes();

		if (simulationMesh)
        {
            return simulationMesh.vertices;
        }
		else if (renderMesh)
        {
            return renderMesh.vertices;
        }
        else
        {
            return new Vector3[0];
        }
    }

    public void SetVertices(Vector3[] v)
    {
        renderMesh.vertices = v;
        renderMesh.RecalculateNormals();
    }

    public int[] GetTriangles()   
    {
        UpdateInputMeshes();
        return triangles;
    }
    
    public void GetNormals(List<Vector3> normals)
    {
        renderMesh.GetNormals(normals);
    }
    
    public void SetNormals(List<Vector3> n)
    {
        renderMesh.SetNormals(n);
    }

    public int GetVertexCount()
    {
        UpdateInputMeshes();

        if (simulationMesh)
        {
            return simulationMesh.vertexCount;
        }
        else if (renderMesh)
        {
            return renderMesh.vertexCount;
        }
        else
        {
            return 0;
        }
    }

    public int GetIndexCount()
    {
        return triangles.Length;
    }
    
    /**
     * Updates the mesh contour points
     **/
    public bool UpdateMeshContourPoints()
    {
        Mesh inputMesh = null;

        if (simulationMesh)
        {
            inputMesh = simulationMesh;
        }
        else if (renderMesh)
        {
            inputMesh = renderMesh;
        }
        else
        {
            return false;
        }

        DeformPlugin.Utils.FindMeshContours(inputMesh.vertices, inputMesh.vertices.Length,
                                            inputMesh.triangles,
                                            inputMesh.triangles.Length,
                                            0, 0, 0, 0, 0, 0,
                                            out int numContours,
                                            out int numContourPoints);

        contourPointsArray = new int[numContourPoints];
        var cviHandle = GCHandle.Alloc(contourPointsArray, GCHandleType.Pinned);
        var cviPtr = cviHandle.AddrOfPinnedObject();

        contourPointCountsArray = new int[numContours];
        var cpcHandle = GCHandle.Alloc(contourPointCountsArray, GCHandleType.Pinned);
        var cpcPtr = cpcHandle.AddrOfPinnedObject();

        DeformPlugin.Utils.RetrieveMeshContours(cviPtr, cpcPtr);
        
        cviHandle.Free();
        cpcHandle.Free();

        return true;
    }
}
