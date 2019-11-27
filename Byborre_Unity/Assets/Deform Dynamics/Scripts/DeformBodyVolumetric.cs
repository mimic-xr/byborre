using System.Runtime.InteropServices;
using UnityEngine;
using DeformDynamics;

[AddComponentMenu("Deform Dynamics/Deform Body Volumetric")]
[RequireComponent(typeof(MeshFilter))]
public class DeformBodyVolumetric : DeformBody
{
    /**
     * Which volumetric mesh under StreamingAssets to use. This mechanism will be changed in future releases.
     **/
    public int selectedPath;

    /**
     * The actual path to the selected volumetric mesh.
     **/
    [SerializeField]
    private string path;

    [Range(0, 1)]
    public float volumeStiffness;

    [SerializeField]
    private string _oldPath;
    
    protected override void Initialize()
    {
        if (GetComponent<MeshFilter>().sharedMesh == null)
        {
            enabled = false;
            return;
        }

        UpdateInternalMesh();

        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("No mesh selected");
            return;
        }
        else
        {
            DeformPlugin.Object.CreateVolumetricDeformableObject(Application.streamingAssetsPath + path, out id);
        }

        PostInitialize();

        DeformPlugin.Object.SetVolumeStiffness(id, volumeStiffness);
        DeformPlugin.Object.SetBendingStiffness(id, bendingStiffness);
        DeformPlugin.Object.SetDistanceStiffness(id, distanceStiffness);

        initialized = true;
    }

    public override void OnValidate()
    {
        base.OnValidate();

        if (path != _oldPath && !Application.isPlaying)
        {
            UpdateInternalMesh();
            _oldPath = path;
        }

        GetComponent<MeshFilter>().hideFlags = HideFlags.HideInInspector;

        //if (Application.isPlaying && initialized)
        //{
        //    SetVolumeStiffnessSM(id, volumeStiffness);
        //}
    }
    
    protected void UpdateInternalMesh()
    {
        if (string.IsNullOrEmpty(path)) return;

        int numVertices, numIndices;

        DeformPlugin.Object.ReadTetramesh(Application.streamingAssetsPath + path, out numVertices, out numIndices);

        if (numVertices <= 0 && numIndices <= 0) return;

        vertices = new Vector3[numVertices];
        int[] triangles = new int[numIndices];

        var verticesHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
        var verticesPtr = verticesHandle.AddrOfPinnedObject();

        var trianglesHandle = GCHandle.Alloc(triangles, GCHandleType.Pinned);
        var trianglesPtr = trianglesHandle.AddrOfPinnedObject();

        DeformPlugin.Object.RetrieveTetramesh(verticesPtr, trianglesPtr);

        verticesHandle.Free();
        trianglesHandle.Free();

        renderMesh = new Mesh
        {
            name = "Volumetric"
        };

        renderMesh.MarkDynamic();

        renderMesh.vertices = vertices;
        renderMesh.triangles = triangles;

        renderMesh.RecalculateBounds();
        renderMesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = renderMesh;

        if (_oldPath == path) return;

        _oldPath = path;

        AllocateVertexArrays(renderMesh);
    }

    public void SetPath(string p)
    {
        if (p != _oldPath && !p.Equals("None"))
        {
            path = p;
            UpdateInternalMesh();
            _oldPath = path;
        }
    }
}
