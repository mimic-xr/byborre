using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[AddComponentMenu("Deform Dynamics/Utilities/Deform Modifier Bend")]
public class DeformModifierBend : MonoBehaviour
{ 

    public enum Axis { X, Y, Z };
    public enum AxisWithNone { X, Y, Z, None }
    public Axis bendAxis = Axis.X;
    
    public AxisWithNone symmetryAxis = AxisWithNone.None;

    public float limit = 0;

    [SerializeField, HideInInspector]
    public Vector3 bendAxisVector;

    [SerializeField, HideInInspector]
    public Vector3 symmetryAxisVector;

    [Range(-1080, 1080)]
    public float angle = 0.0f;

    [SerializeField, HideInInspector]
    public Mesh original_mesh;

    [SerializeField, HideInInspector]
    private Vector3[] original_vertices;

    [SerializeField, HideInInspector]
    public bool[] paintedVertices;

    [SerializeField, HideInInspector]
    private bool initialized;
    
    [SerializeField, HideInInspector]
    public Vector3 original_center;

    [SerializeField, HideInInspector]
    public Material previewMaterial;

    [SerializeField, HideInInspector]
    public Mesh preview;

    static bool exitingGame;
    
    private void OnValidate()
    {
		UpdatePreview();
    }

    public void UpdatePreview()
    {
        limit = Mathf.Max(limit, 0);

		if (Application.isPlaying) return;

		if (exitingGame)
		{
			exitingGame = false;
			return;
		}

        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

        if (!mesh) return;

        Vector3[] vertices = mesh.vertices;

        // Store original mesh to recreate original rest-values
        if (original_mesh == null)
		{
			original_mesh = Instantiate(mesh);
        }

        if (!initialized)
        {
            preview = new Mesh();
            preview.vertices = vertices;
            preview.triangles = mesh.triangles;
            previewMaterial = Resources.Load<Material>("Materials/WireframeOverlayPreview");

            original_vertices = vertices;
            original_center = mesh.bounds.center;

            initialized = true;

			paintedVertices = new bool[mesh.vertices.Length];

			for (int i = 0; i < mesh.vertices.Length; i++)
			{
				paintedVertices[i] = true;
			}
		}

        switch (bendAxis)
        {
            case (Axis.X): bendAxisVector = Vector3.right; break;
            case (Axis.Y): bendAxisVector = Vector3.up; break;
            case (Axis.Z): bendAxisVector = Vector3.forward; break;
        }

        switch (symmetryAxis)
        {
            case (AxisWithNone.X): symmetryAxisVector = Vector3.right; break;
            case (AxisWithNone.Y): symmetryAxisVector = Vector3.up; break;
            case (AxisWithNone.Z): symmetryAxisVector = Vector3.forward; break;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            if (paintedVertices != null && !paintedVertices[i]) continue;

            Vector3 v = original_vertices[i];

            Vector3 a = v - original_center;
            float d = Vector3.Dot(v, bendAxisVector);
            float distance_to_center = (v - (original_center + bendAxisVector * d)).magnitude;

            if (distance_to_center < limit) continue;

            int sign = 1;

            if (symmetryAxis != AxisWithNone.None)
            {
                sign = Vector3.Dot(v - original_center, symmetryAxisVector) < 0 ? -1 : 1;
            }

            vertices[i] = Quaternion.AngleAxis(sign * angle * (distance_to_center - limit), bendAxisVector) * v;
        }

        preview.vertices = vertices;
        preview.RecalculateBounds();
        preview.RecalculateNormals();
    }
    
    public void ApplyBending()
    {
        GetComponent<MeshFilter>().sharedMesh = Instantiate(preview);        
        angle = 0;

        // Set this to reinitialize using new configuration
        initialized = false;
    }

    private void OnApplicationQuit()
    {
        exitingGame = true;
    }

    public Vector3[] GetVertices()
    {
        return original_vertices;
    }

    public int GetVertexCount()
    {
        return original_vertices.Length;
    }

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (GetComponent<MeshFilter>().sharedMesh != null &&
			original_mesh != null &&
			!MeshUtils.MeshTriangleListIdentical(original_mesh, GetComponent<MeshFilter>().sharedMesh))
		{
			original_mesh = null;
			initialized = false;

			UpdatePreview();
		}
	}
#endif

}

