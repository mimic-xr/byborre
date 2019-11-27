using UnityEngine;
using DeformDynamics;

[RequireComponent(typeof(DeformBody))]
public class DeformPicking : MonoBehaviour
{
    public bool pickingEnabled = true;
	public bool limitPullDistance = false;

	[Range(0, 10)]
    public float maxPullDistance = 1.0f;

	private bool oldPickingEnabled;
    private float oldMaximumPullDistance;

    private void OnEnable()
    {
        DeformManager.OnSimulationStarted += OnSimulationStarted;
        DeformManager.OnSimulationUpdated += OnSimulationUpdated;
    }

    private void OnDisable()
    {
        DeformManager.OnSimulationStarted -= OnSimulationStarted;
        DeformManager.OnSimulationUpdated -= OnSimulationUpdated;
    }

	private void OnSimulationStarted()
	{
		DeformBody body = GetComponent<DeformBody>();

		DeformPlugin.Interaction.SetPickingEnabled(body.id, pickingEnabled);

		if (limitPullDistance) {
			DeformPlugin.Interaction.SetMaximumPullDistance(body.id, maxPullDistance);
		}

        oldPickingEnabled = pickingEnabled;
        oldMaximumPullDistance = maxPullDistance;
    }

    private void OnSimulationUpdated()
    {
        DeformBody body = GetComponent<DeformBody>();

        if (pickingEnabled != oldPickingEnabled)
        {
            DeformPlugin.Interaction.SetPickingEnabled(body.id, pickingEnabled);
            oldPickingEnabled = pickingEnabled;
        }

        if ((maxPullDistance != oldMaximumPullDistance) && limitPullDistance)
        {
            DeformPlugin.Interaction.SetMaximumPullDistance(body.id, maxPullDistance);
            oldMaximumPullDistance = maxPullDistance;
        }
    }
}
