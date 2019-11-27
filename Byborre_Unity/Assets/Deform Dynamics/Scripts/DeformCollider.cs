using System.Runtime.InteropServices;
using UnityEngine;
using DeformDynamics;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[DisallowMultipleComponent]
public abstract class DeformCollider : MonoBehaviour {

	protected Mesh colliderMesh;
    protected Mesh inGameColliderMesh;

    /**
     * Whether the collider bounds should be visible in the editor or not.
     **/
    public bool showColliderInEditor = true;

    /**
     * Whether the collider bounds should be visible in game or not.
     **/
    public bool showColliderInGame = false;

    /**
     * The Material of the render mesh.
     **/
    public Material inGameMaterial;

	/**
	* Test
	**/
	public bool sticky = false;

	private bool showColliderInEditorOld;
    private bool showColliderInGameOld;
    private Material customMaterialOld;

    private float biasOld;

    /**
     * How much smaller the render mesh should be compared to the actual collider bounds.
     **/
    [Range(0, 1)]
    public float kineticFriction;

    [Range(0, 10)]
    public float staticFriction;

    [Range(0, 0.5f)]
    public float bias = 0;
    
    protected int id;
    private Material[] materials;

    private Material wireframeMaterial;
	//private Material wireframeMaterialSticky;

	private int _instanceId;

    /**
     * The last known position of the DeformCollider. Used for calculating deltas when updating the collider in the physics simulation.
     **/ 
    [HideInInspector]
    public Vector3 oldPosition;
    
    /**
     * The last known rotation of the DeformCollider. Used for calculating deltas when updating the collider in the physics simulation.
     **/
    [HideInInspector]
    public Quaternion oldRotation;

    private void OnEnable()
    {
        DeformManager.OnAddColliders += PrepareForSimulation;
        DeformManager.PreSimulationUpdated += UpdateInSimulation;
    }

    private void OnDisable()
    {
        DeformManager.OnAddColliders -= PrepareForSimulation;
        DeformManager.PreSimulationUpdated -= UpdateInSimulation;
    }

    private void Reset()
    {
		if (sticky)
		{
			wireframeMaterial = Resources.Load<Material>("Wireframe/Examples/Materials/Wireframe-TransparentCulledYellow");
		} else
		{
			wireframeMaterial = Resources.Load<Material>("Wireframe/Examples/Materials/Wireframe-TransparentCulled");
		}

		CreateMeshes();
        UpdateMeshes();
    }

    private void CreateMeshes()
    {
        colliderMesh = new Mesh();
        colliderMesh.MarkDynamic();

        inGameColliderMesh = new Mesh();
        inGameColliderMesh.MarkDynamic();
    }

    private void PrepareForSimulation()
    {
        if (!Application.isPlaying) return;

        OnValidate();
        SaveTransform();

        AddToSimulation();

		DeformPlugin.Collider.SetColliderSticky(id, sticky);
	}

    private void OnValidate()
    {
		if (sticky)
		{
			wireframeMaterial = Resources.Load<Material>("Wireframe/Examples/Materials/Wireframe-TransparentCulledYellow");
		} else
		{
			wireframeMaterial = Resources.Load<Material>("Wireframe/Examples/Materials/Wireframe-TransparentCulled");
		}
        
        CreateMeshes();
        UpdateMeshes();
    }

    // Detect if the shape has changed in any way, then update meshes.
    void Update()
    {
        UpdateTransform();
        
        if (bias != biasOld && !Application.isPlaying)
        {
            biasOld = bias;
        }

        if (!Application.isPlaying)
        {
            UpdateMeshes();
        }

        if (!Application.isPlaying && showColliderInEditor)
        {
            for (int i = 0; i < colliderMesh.subMeshCount; i++)
            {
                Graphics.DrawMesh(colliderMesh, Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one),
                wireframeMaterial, 0, null, i);
            }            
        }

        if (showColliderInGame)
        {
            Graphics.DrawMesh(inGameColliderMesh, Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one), 
                inGameMaterial != null ? inGameMaterial : wireframeMaterial, 0);
        }
		
    }

    protected virtual void UpdateTransform()
    {
        //transform.localScale = Vector3.one;
    }

    protected virtual bool HasChangedShape()
    {
        bool result = false;

        result = transform.hasChanged || 
                 showColliderInEditorOld != showColliderInEditor ||
                 showColliderInGameOld != showColliderInGame ||
                 customMaterialOld != inGameMaterial;

        transform.hasChanged = false;
        showColliderInEditorOld = showColliderInEditor;
        showColliderInGameOld = showColliderInGame;
        customMaterialOld = inGameMaterial;

        return result;
    }

    protected virtual void UpdateMeshes()
    {
        colliderMesh.Clear();
    }
    
    /**
     * Adds the collider to the physics simulation. This is automatically called by the DeformManager at the simulation initialization.
     **/
    public abstract void AddToSimulation();

    /**
     * Takes any change made to the collider in the Unity user interface and passes it to the physics engine.
     **/
    public abstract void UpdateInSimulation();
    
    /**
     * Returns the simulation id of this DeformCollider.
     **/
    public int GetId()
    {
        return id;
    }

    /**
     * Sets the simulation id of this DeformCollider.
     **/
    public void SetId(int colliderId)
    {
        id = colliderId;
    }

    /**
     * Returns the rotation in axis-angle format. The x, y, z components of the returned Vector4 constitute the axis, and w stores the angle.
     **/
    public Vector4 GetRotation()
    {
        Vector3 axis;
        float angle;
        transform.rotation.ToAngleAxis(out angle, out axis);
        return new Vector4(axis.x, axis.y, axis.z, Mathf.Deg2Rad * angle);
    }

    /**
     * Stores the latest transform. This is used for calculating deltas needed for updating the physics simulation.
     **/
    public void SaveTransform()
    {
        oldPosition = transform.position;
        oldRotation = transform.rotation;
    }
}
