using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[AddComponentMenu("Deform Dynamics/Utilities/Animation Synchronizer")]
public class AnimationSynchronizer : MonoBehaviour
{
	Animator animator;
    bool paused = false;

	private void OnEnable()
	{
		DeformManager.PreSimulationUpdated += UpdateAnimation;
	}

	private void OnDisable()
	{
		DeformManager.PreSimulationUpdated -= UpdateAnimation;
	}

	// Start is called before the first frame update
	void Start()
	{
		if (animator = GetComponent<Animator>())
		{
			animator.enabled = false;
		}		
	}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            paused = !paused;
        }
    }

    void UpdateAnimation()
	{
		animator.speed = 1.0f;
		if (!paused) animator.Update(0.016f);
		animator.speed = 0.0f;
	}
}