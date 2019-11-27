using UnityEngine;
using DeformDynamics;

[RequireComponent(typeof(DeformBody))]
public class DeformSkinning : MonoBehaviour
{
    public DeformColliderMesh meshCollider;
    
    public bool useGlobalSkinning;
    
    private bool oldUseGlobalSkinning;
    private bool simulationStarted = false;

    [HideInInspector]
    public bool paintSkinnedVertices;

    [HideInInspector]
    public bool[] skinnedVertices;
    
    private void OnEnable()
    {
        DeformManager.OnSkin += Initialize;
        DeformManager.OnSimulationStarted += OnSimulationStarted;
    }

    private void OnDisable()
    {
        DeformManager.OnSkin -= Initialize;
        DeformManager.OnSimulationStarted -= OnSimulationStarted;
    }

    private void Reset()
    {
        DeformBody deformBody = GetComponent<DeformBody>();
        int numVertices = deformBody.GetVertexCount();

        if (skinnedVertices == null && numVertices != 0)
        {
            skinnedVertices = new bool[numVertices];
        }
    }

    void Initialize()
    {
        oldUseGlobalSkinning = useGlobalSkinning;

        DeformBody deformBody = GetComponent<DeformBody>();

        if (skinnedVertices == null)
        {
            skinnedVertices = new bool[deformBody.GetVertexCount()];
        }

        int[] skinned = new int[skinnedVertices.Length];

        for (int i = 0; i < skinned.Length; i++)
        {
            skinned[i] = skinnedVertices[i] ? 1 : 0;
        }

        if (meshCollider && meshCollider.enabled)
        {
            DeformPlugin.Collider.SkinToMeshCollider(deformBody.id, skinned, skinnedVertices.Length, meshCollider.GetId());
        }
    }

    void OnSimulationStarted()
    {
        paintSkinnedVertices = false;

        DeformBody deformBody = GetComponent<DeformBody>();

        if (meshCollider && meshCollider.enabled)
        {
            DeformPlugin.Collider.SetGlobalSkinningEnabled(deformBody.id, useGlobalSkinning);
        }

        simulationStarted = true;
    }

    private void OnValidate()
    {
        if (useGlobalSkinning != oldUseGlobalSkinning && simulationStarted)
        {
            ToggleSkinning();
            oldUseGlobalSkinning = useGlobalSkinning;
        }
    }

    public void ToggleSkinning()
    {
        DeformBody deformBody = GetComponent<DeformBody>();

        if (meshCollider && meshCollider.enabled)
        {
            DeformPlugin.Collider.SetGlobalSkinningEnabled(deformBody.id, useGlobalSkinning);
        }
    }
}
