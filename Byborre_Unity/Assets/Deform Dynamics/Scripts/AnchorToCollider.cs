using UnityEngine;
using System.Collections;
using DeformDynamics;

[AddComponentMenu("Deform Dynamics/Utilities/Anchor To Collider")]
public class AnchorToCollider : MonoBehaviour {
	
    /**
     * The collider to anchor to
     **/
	public DeformCollider anchorCollider;

    /**
     * Whether to disable the collision handling of the anchorCollider or not.
     **/ 
    public bool disableCollider;

    /**
     * Whether the anchoring should be automatically executed when the scene begins.
     **/ 
	public bool anchorAtStart;

    public float anchorAfterSeconds = -1.0f;

	private DeformBody deformableObject;

    private void OnEnable()
    {
        //DeformManager.OnSimulationStarted += Anchor;
    }

    private void OnDisable()
    {
        //DeformManager.OnSimulationStarted -= Anchor;
    }
    
    /**
     * Anchors the vertices of the deformable object that are inside the collider to the collider.
     **/
	public void Anchor()
    {
		deformableObject = GetComponent<DeformBody>();

		if (deformableObject == null || anchorCollider == null) return;

		int deformableObjectId = deformableObject.GetId();
        int colliderId = anchorCollider.GetId();

		DeformPlugin.Object.AnchorObjectToCollider(deformableObjectId, colliderId);

        // Make sure that anchoring only happens once
        anchorAtStart = false;
	}

    void Start()
    {
        if (anchorCollider && disableCollider)
        {
			DeformPlugin.Collider.SetColliderEnabled(anchorCollider.GetId(), false);
        }

        if (anchorAtStart)
        {
			Anchor();
        }
        else if (anchorAfterSeconds > 0)
        {
            StartCoroutine(AnchorAtTime(anchorAfterSeconds));
        }
    }

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.G))
		{
			Anchor();
		}
	}

    IEnumerator AnchorAtTime(float time)
    {
        yield return new WaitForSeconds(time);
		Anchor();
    }
}
