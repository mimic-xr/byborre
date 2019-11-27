using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DeformDynamics;

public class DeformSeam : MonoBehaviour
{	
	[SerializeField, HideInInspector]
	public int seamId;

	[SerializeField, HideInInspector]
	public bool fromSerialization = false;

	[HideInInspector, SerializeField]
	public int contourA = -1;

	[HideInInspector, SerializeField]
	public int contourB = -1;

	public int priority = 0;
	
	public bool damping = true;

    public float distanceStiffness = 1.0f;    
	public float bendingStiffness = 0.05f;

    public bool showLinesInEditor = true;

	[SerializeField, HideInInspector]
	private bool oldPointsSet = false;

	/**
     * The first DeformBody used for this seam.
     **/
	[HideInInspector, SerializeField]
	public DeformBody bodyA;

	[HideInInspector, SerializeField]
	public bool bodyASewingDirection = false;

	/**
     * Vertex index of the beginning of the seam of the first DeformBody. 
     **/
	[HideInInspector, SerializeField]
	public int bodyASeamBegin;

	[SerializeField, HideInInspector]
	public Vector3 bodyASeamBeginPointOld;

	/**
     * Vertex index of the end of the seam of the first DeformBody. 
     **/
	[HideInInspector, SerializeField]
	public int bodyASeamEnd;

	[SerializeField, HideInInspector]
	public Vector3 bodyASeamEndPointOld;

	/**
     * The second DeformBody used for this seam.
     **/
	[HideInInspector, SerializeField]
	public DeformBody bodyB;

	[HideInInspector, SerializeField]
	public bool bodyBSewingDirection = false;

	[SerializeField, HideInInspector]
	public Vector3 bodyBSeamBeginPointOld;

	/**
     * Vertex index of the beginning of the seam of the second DeformBody. 
     **/
	[HideInInspector, SerializeField]
	public int bodyBSeamBegin;

	[SerializeField, HideInInspector]
	public Vector3 bodyBSeamEndPointOld;

	/**
     * Vertex index of the end of the seam of the second DeformBody. 
     **/
	[HideInInspector, SerializeField]
	public int bodyBSeamEnd;

	[SerializeField, HideInInspector]
	public List<int> seamIndices;


	[SerializeField, HideInInspector]
	private bool sewExecuted;
	
    /**
     * Decides the iteration order of the contour points
     **/
    [HideInInspector]
    public bool shortest_path;

	[SerializeField, HideInInspector]
	private Vector3[] bodyAVertices;

	[SerializeField, HideInInspector]
	private Vector3[] bodyBVertices;

	[SerializeField, HideInInspector]
	private Mesh currentMeshA;  // The mesh that was used when this seam was created

    List<Vector3> bodyANormals;
    List<Vector3> bodyBNormals;

    [SerializeField, HideInInspector]
	private Mesh currentMeshB;  // The mesh that was used when this seam was created

	protected void OnEnable()
	{
		DeformManager.PreSimulationStarted += Sew;
        DeformManager.PostSimulationUpdated += UpdateNormals;
	}

	protected void OnDisable()
	{
		DeformManager.PreSimulationStarted -= Sew;
        DeformManager.PostSimulationUpdated -= UpdateNormals;
    }

	private void Start()
	{
        bodyANormals = new List<Vector3>();
        bodyBNormals = new List<Vector3>();

        if (!sewExecuted)
		{
			Sew();
		}
	}

	/**
     * Computes the seam information needed to sew the provided DeformBody objects together.
     **/
	public void Create() // Just pass lists of vertices determined from UI here?
    {
        if (bodyA == null || bodyB == null) return;
        if (bodyASeamBegin == -1 || bodyBSeamBegin == -1) return;
        if (bodyASeamEnd == -1 || bodyBSeamEnd == -1) return;

        currentMeshA = Instantiate(bodyA.renderMesh);
        currentMeshB = Instantiate(bodyB.renderMesh);
        
        if (!oldPointsSet)
		{
			bodyASeamBeginPointOld = currentMeshA.vertices[bodyA.contourPointsArray[bodyASeamBegin]];
			bodyASeamEndPointOld = currentMeshA.vertices[bodyA.contourPointsArray[bodyASeamEnd]];

			bodyBSeamBeginPointOld = currentMeshB.vertices[bodyB.contourPointsArray[bodyBSeamBegin]];
			bodyBSeamEndPointOld = currentMeshB.vertices[bodyB.contourPointsArray[bodyBSeamEnd]];

			oldPointsSet = true;
		}
	}

    /**
     * Sews the two DeformBody objects together.
     **/
    private void Sew()
    {
        if (sewExecuted || bodyA == null || bodyB == null || !bodyA.enabled || !bodyB.enabled ||
			seamIndices == null || seamIndices.Count == 0) return;

		var v = seamIndices.ToArray();

		DeformPlugin.Seam.CreateSeam(bodyA.GetId(), bodyB.GetId(), v, v.Length, out seamId);
		DeformPlugin.Seam.SetSeamDistanceStiffness(seamId, distanceStiffness);
		DeformPlugin.Seam.SetSeamBendingStiffness(seamId, bendingStiffness);
		DeformPlugin.Seam.SetSeamDamping(seamId, damping);
		DeformPlugin.Seam.SetSeamPriority(seamId, priority);

		sewExecuted = true;
    }
	
	private void RecalculateIndices()
	{
		// Recalculate the start and end point of both bodies
		bodyASeamBegin = CalculateClosestIndexToPoint(bodyASeamBeginPointOld,
													  bodyA.contourPointsArray,
													  bodyA.renderMesh.vertices); // TODO: Change to bodyA.GetVertices() to support simulation mesh

		bodyASeamEnd = CalculateClosestIndexToPoint(bodyASeamEndPointOld,
													bodyA.contourPointsArray,
                                                    bodyA.renderMesh.vertices); // TODO: Change to bodyA.GetVertices() to support simulation mesh

        bodyBSeamBegin = CalculateClosestIndexToPoint(bodyBSeamBeginPointOld,
													  bodyB.contourPointsArray,
                                                      bodyB.renderMesh.vertices); // TODO: Change to bodyA.GetVertices() to support simulation mesh

        bodyBSeamEnd = CalculateClosestIndexToPoint(bodyBSeamEndPointOld,
													bodyB.contourPointsArray,
                                                    bodyB.renderMesh.vertices); // TODO: Change to bodyA.GetVertices() to support simulation mesh
    }

	// Finds the closest point in the mesh, to the reference point, and returns the index of the closest point
	private int CalculateClosestIndexToPoint(Vector3 referencePos, int[] contourIndices, Vector3[] vertices)
	{
		float closestDistance = float.MaxValue;
		int closestIndex = -1;
		int test = -1;

		for (int i = 0; i < contourIndices.Length; i++)
		{
			int contourIndex = contourIndices[i];
			Vector3 currentPos = vertices[contourIndex];
			float dist = Vector3.Distance(referencePos, currentPos);
			if (dist < closestDistance)
			{
				closestIndex = contourIndex;
				closestDistance = dist;
				test = i;
			}
		}

		return test;
	}

	private void RedoStitching()
	{
		if (Application.isPlaying) return;

		if (bodyA == null || bodyB == null || seamIndices == null) return;

		// TODO: This is needed when we have two patches (not patterns) which are retriangulated
		// using the patch creator. We should be able to do something more sophisticated
		bodyA.UpdateMeshContourPoints();
		bodyB.UpdateMeshContourPoints();

		if (bodyASeamBegin == -1 || bodyBSeamBegin == -1) return;

		currentMeshA = Instantiate(bodyA.renderMesh);
		currentMeshB = Instantiate(bodyB.renderMesh);
		
		RecalculateIndices();           // Recalculate start/end indices

#if UNITY_EDITOR
        DeformSewing.FillSeamIndices(this);

		DrawSeams();
		SceneView.RepaintAll();
#endif
	}

#if UNITY_EDITOR
    private void DrawSeams()
	{
		if (Application.isPlaying || !showLinesInEditor) return;

		if (bodyA == null || bodyB == null || seamIndices == null) return;
    
		bodyAVertices = bodyA.renderMesh.vertices;
		bodyBVertices = bodyB.renderMesh.vertices;

        for (int i = 0; i < seamIndices.Count; i += 2)
		{
			if (Selection.Contains(gameObject))
			{
				Gizmos.color = Color.magenta;
			}
			else
			{
				Gizmos.color = Color.gray;
			}

			if (bodyAVertices.Length > seamIndices[i] && bodyBVertices.Length > seamIndices[i + 1])
			{
				Gizmos.DrawLine(bodyA.transform.TransformPoint(bodyAVertices[seamIndices[i]]),
								bodyB.transform.TransformPoint(bodyBVertices[seamIndices[i + 1]]));
			}
		}
	}
#endif    

    void UpdateNormals()
    {
        //if (bodyA == null || bodyB == null || seamIndices == null) return;

        //bodyA.GetNormals(bodyANormals);
        //bodyB.GetNormals(bodyBNormals);

        //// TODO: Do in parallel
        //for (int i = 0; i < seamIndices.Count / 2; i++)
        //{
        //    Vector3 avg = (bodyANormals[seamIndices[i * 2 + 0]] + bodyBNormals[seamIndices[i * 2 + 1]]) / 2.0f;

        //    if (avg.magnitude < 0.9f) continue;

        //    bodyANormals[seamIndices[i * 2 + 0]] = avg;
        //    bodyBNormals[seamIndices[i * 2 + 1]] = avg;
        //}

        //bodyA.SetNormals(bodyANormals);
        //bodyB.SetNormals(bodyBNormals);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (bodyA == null || bodyB == null)
        {
            DestroyImmediate(gameObject);  // If one of the bodies have been destroyed, remove this seam
            return;
        } 
        else
        {
            bodyA.UpdateInputMeshes();
            bodyB.UpdateInputMeshes();
        }

        if (!fromSerialization)
        {
            if (bodyA.renderMesh != null && !MeshUtils.MeshesIdentical(currentMeshA, bodyA.renderMesh) &&
                !MeshUtils.MeshTriangleListIdentical(currentMeshA, bodyA.renderMesh))
            {
                RedoStitching();
            }
            else if (bodyB.renderMesh != null && !MeshUtils.MeshesIdentical(currentMeshB, bodyB.renderMesh) &&
                !MeshUtils.MeshTriangleListIdentical(currentMeshB, bodyB.renderMesh))
            {
                RedoStitching();
            }
        }

        DrawSeams();
    }
#endif

}
