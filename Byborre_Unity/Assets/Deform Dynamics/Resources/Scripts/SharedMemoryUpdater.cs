using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class SharedMemoryUpdater : MonoBehaviour {

    public Vector3 gravity;

    [DllImport("deform_plugin")]
    private static extern void InitSharedMemoryServer();

    [DllImport("deform_plugin")]
    private static extern int ShutdownSharedMemoryServer();
    
    [DllImport("deform_plugin")]
    private static extern int InitializePluginSM(string path);

    [DllImport("deform_plugin")]
    private static extern int CreateDeformableObjectSM(Vector3[] vertices, uint numVertices,
										               int[] indices, uint numIndices, out int objectID);

    [DllImport("deform_plugin")]
    private static extern int SetSelfCollisionSM(bool active, bool ignoreIntersectingParticles);

    [DllImport("deform_plugin")]
    private static extern int StartSimulationSM();

    [DllImport("deform_plugin")]
    private static extern int UpdateSimulationSM();

    [DllImport("deform_plugin")]
    private static extern int MoveObjectSM(int objectID, float x, float y, float z);

    [DllImport("deform_plugin")]
    private static extern int RotateObjectSM(int objectID, float angle, float x, float y, float z);

    [DllImport("deform_plugin")]
    private static extern int ScaleObjectSM(int objectID, float x, float y, float z);

    [DllImport("deform_plugin")]
    private static extern int SetBendingStiffnessSM(int objectID, float value);

    [DllImport("deform_plugin")]
    private static extern int FixParticleSM(int objectID, int particleID);
    
    [DllImport("deform_plugin")]
    private static extern int GetObjectRenderVerticesSM(int objectID, IntPtr data);

    [DllImport("deform_plugin")]
    private static extern int SetGravitySM(float x, float y, float z);

    private Vector3[] vertices;

    private GCHandle verticesHandle;
    private IntPtr verticesPtr;

    private bool initialized = false;

    private int objectID;

	void Start () {
        Mesh m = GetComponent<MeshFilter>().sharedMesh;

        vertices = new Vector3[m.vertexCount];

        verticesHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
        verticesPtr = verticesHandle.AddrOfPinnedObject();

		// Initialize shared memory
        InitSharedMemoryServer();

        // Initialize plugin
        InitializePluginSM(Application.dataPath + "/Plugins/deform_config.xml");
        
        CreateDeformableObjectSM(m.vertices, (uint) m.vertexCount, m.triangles, m.GetIndexCount(0), out objectID);

        Vector3 scale = transform.lossyScale;
        ScaleObjectSM(objectID, scale.x, scale.y, scale.z);

        Vector3 axis;
        float angle;
        transform.rotation.ToAngleAxis(out angle, out axis);
        RotateObjectSM(objectID, -(Mathf.Deg2Rad * angle), axis.x, axis.y, axis.z);

        Vector3 pos = transform.position;
        MoveObjectSM(objectID, pos.x, pos.y, pos.z);
        
        FixParticleSM(objectID, 0);
        FixParticleSM(objectID, 60);

        SetSelfCollisionSM(true, true);

        SetGravitySM(gravity.x, gravity.y, gravity.z);

        StartSimulationSM();

        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        initialized = true;
	}

    private void OnDestroy()
    {
        if (initialized)
        {
            int result = ShutdownSharedMemoryServer();
            print("Shut down the shared memory server, result: " + result);
        }

        verticesHandle.Free();
    }

    void Update () {
        // Get vertices from shared memory and update mesh
        if (!initialized) return;

        if (UpdateSimulationSM() == 0)
        {
            print("The Deform plugin has crashed");
            initialized = false;
            return;
        }

        GetObjectRenderVerticesSM(objectID, verticesPtr);
        SetGravitySM(gravity.x, gravity.y, gravity.z);

        GetComponentInChildren<MeshFilter>().mesh.vertices = vertices;
        GetComponentInChildren<MeshFilter>().mesh.RecalculateNormals();
        GetComponentInChildren<MeshFilter>().mesh.RecalculateBounds();
    }
}
