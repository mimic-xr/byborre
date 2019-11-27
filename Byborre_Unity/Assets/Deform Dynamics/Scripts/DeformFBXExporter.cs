using System.Collections.Generic;
using UnityEngine;
using DeformDynamics;

[AddComponentMenu("Deform Dynamics/Deform FBX Exporter")]
public class DeformFBXExporter : MonoBehaviour {
    
	private int exporterId = -1;

	SkinnedMeshRenderer smr;

	Mesh mesh;

	public GameObject meshCollider;

    /**
     * The path where the recorded .fbx file will be stored.
     **/
	public string filePath;

    /**
     * Whether this DeformExporter should record or not.
     **/
    [HideInInspector]
    public bool shouldRecord;

    /**
     * Whether the recording process has started.
     **/
    [HideInInspector]
    public bool recordingStarted;

    /**
     * The DeformBody to record.
     **/
    [HideInInspector]
    public DeformBody capturedObject;

    /**
     * Whether a snapshot should be taken or not. 
     **/
    [HideInInspector]
    public bool shouldTakeSnapshot;

    /**
     * Whether the game is exiting or not. Used for stopping the recording. 
     **/
    [HideInInspector]
    public static bool exitingGame;
	
    protected int capturedObjectId;
    protected int exporter_id;
    protected Vector3 preSimPosition;
    protected Quaternion preSimRotation;
    protected Vector3 preSimScale;

	private bool skinned;

    private void OnEnable()
    {
        DeformManager.OnSimulationUpdated += UpdateRecording;
		DeformManager.OnRecordingEnded += StopRecording;

		// TODO: Add OnSimulationEnded stopRecording (FIRST CHECK IF IT IS WORKING DUE TO DESTROY)
	}

	private void OnDisable()
    {
        DeformManager.OnSimulationUpdated -= UpdateRecording;
		DeformManager.OnRecordingEnded -= StopRecording;

		// TODO: Add OnSimulationEnded stopRecording
	}

    private void Awake()
    {
        preSimPosition = transform.position;
        preSimRotation = transform.rotation;
        preSimScale = transform.localScale;
    }

    // Use this for initialization
    void Start ()
    {
		if (GetComponent<MeshFilter>() != null)
		{
			skinned = false;
			mesh = GetComponent<MeshFilter>().sharedMesh;


			// Mirror the x-axis
			List<Vector3> v = new List<Vector3>(mesh.vertices);

			for (int i = 0; i < v.Count; i++)
			{
				Vector3 scale_res = new Vector3(v[i].x * preSimScale.x, v[i].y * preSimScale.y, v[i].z * preSimScale.z);

				v[i] = scale_res;
				v[i] = preSimRotation * v[i];
				v[i] = v[i] + preSimPosition;

				// negate the x-axis here?
			}


			DeformPlugin.IO.BuildFBXFromData(name, v.ToArray(), mesh.triangles, mesh.vertexCount, (int)mesh.GetIndexCount(0), out exporterId);

		} else if (GetComponent<SkinnedMeshRenderer>() != null)
		{
			skinned = true;
			mesh = new Mesh();
			smr = GetComponent<SkinnedMeshRenderer>();
			smr.BakeMesh(mesh);

			// Mirror the x-axis
			List<Vector3> v = new List<Vector3>(mesh.vertices);

			for (int i = 0; i < v.Count; i++)
			{
				Vector3 scale_res = new Vector3(v[i].x * preSimScale.x, v[i].y * preSimScale.y, v[i].z * preSimScale.z);

				v[i] = scale_res;
				v[i] = preSimRotation * v[i];
				v[i] = v[i] + preSimPosition;

				// negate the x-axis here?
			}

			DeformPlugin.IO.BuildFBXFromData(name, v.ToArray(), mesh.triangles, mesh.vertexCount, (int)mesh.GetIndexCount(0), out exporterId);
		}
	}

    /**
     * Starts the recording process.
     **/
    public void StartRecording()
    {
        // Detect if mesh is FBX, then use ImportFBX
        // Else use BuildFBX

        shouldRecord = true;
    }

    /**
     * Stops the recording process and stores the result in the specified .fbx file.
     **/
    public void StopRecording()
    {
		if (recordingStarted)
		{
			if (filePath.Length == 0)
			{
				Debug.LogError("Filename not set, cannot export FBX file");
				return;
			}

			DeformPlugin.IO.ExportFBXFromData(exporterId, filePath);

			recordingStarted = false;
			shouldRecord = false;
		}
    }

    private void UpdateRecording()
	{
		if (shouldRecord)
		{
			if (!recordingStarted)
			{
				recordingStarted = true;
			}

			Vector3 t = preSimPosition;
			Quaternion r = preSimRotation;
			Vector3 s = preSimScale;

			if (skinned)
			{
				smr = GetComponent<SkinnedMeshRenderer>();
				smr.BakeMesh(mesh);
			} else
			{
				mesh = GetComponent<MeshFilter>().sharedMesh;
			}

			DeformPlugin.IO.RecordFrameFBXFromData(exporterId, mesh.vertices, mesh.normals, mesh.vertexCount,
													   t.x, t.y, t.z,
													   r.x, r.y, r.z, r.w,
													   s.x, s.y, s.z,
													   true, false, false);
		}
	}

    /**
     * Used for taking a single-frame snapshot. Note that each new snapshot will overwrite the previous one with the same file path.
     **/
    public void FBXSnapshot()
    {
		int snapshotId = -1;

		// Create a new exporter just for the snapshot
		if (GetComponent<MeshFilter>() != null)
		{
			skinned = false;
			mesh = GetComponent<MeshFilter>().sharedMesh;

			// Mirror the x-axis
			List<Vector3> v = new List<Vector3>(mesh.vertices);

			for (int i = 0; i < v.Count; i++)
			{
				Vector3 scale_res = new Vector3(v[i].x * preSimScale.x, v[i].y * preSimScale.y, v[i].z * preSimScale.z);

				v[i] = scale_res;
				v[i] = preSimRotation * v[i];
				v[i] = v[i] + preSimPosition;

				// negate the x-axis here?
			}

			DeformPlugin.IO.BuildFBXFromData(name, v.ToArray(), mesh.triangles, mesh.vertexCount, (int)mesh.GetIndexCount(0), out snapshotId);

		}
		else if (GetComponent<SkinnedMeshRenderer>() != null)
		{
			skinned = true;
			mesh = new Mesh();
			smr = GetComponent<SkinnedMeshRenderer>();
			smr.BakeMesh(mesh);

			// Mirror the x-axis
			List<Vector3> v = new List<Vector3>(mesh.vertices);

			for (int i = 0; i < v.Count; i++)
			{
				Vector3 scale_res = new Vector3(v[i].x * preSimScale.x, v[i].y * preSimScale.y, v[i].z * preSimScale.z);

				v[i] = scale_res;
				v[i] = preSimRotation * v[i];
				v[i] = v[i] + preSimPosition;

				// negate the x-axis here?
			}

			DeformPlugin.IO.BuildFBXFromData(name, v.ToArray(), mesh.triangles, mesh.vertexCount, (int)mesh.GetIndexCount(0), out snapshotId);
		}

		if(snapshotId > -1)
		{
			DeformPlugin.IO.ExportFBXFromData(snapshotId, filePath);
		}
	}
}
