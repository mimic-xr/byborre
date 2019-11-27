using UnityEngine;

[AddComponentMenu("Deform Dynamics/Utilities/Deform Patch Creator")]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DeformPatchCreator : MonoBehaviour
{
    /**
     * The size in meters of the patch to create.
     **/
    public Vector2 size;

	/**
     * The resolution in terms of vertices. The value represents the number of vertices along the longest axis of the patch to create.
     **/
	public uint resolution;

    /**
     * Creates the patch specified by the size and resolution parameters.
     **/
    public void Create()
    {
        Mesh patch = new Mesh();

        MeshUtils.CreateClothMesh(size, resolution, patch);

        GetComponent<MeshFilter>().sharedMesh = patch;
	}
}
