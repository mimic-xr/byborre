using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class FixCollidingParticles : MonoBehaviour {

	[DllImport("deform_plugin")]
	private static extern int FixCollidingParticlesSM();

	// Use this for initialization
	void Start () {
		
	}

	/**
	 * Starts the recording process.
	 **/
	//public virtual void ExecuteFixCollidingParticles()
	//{
	//	FixCollidingParticlesSM();
	//}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.A))
		{
			FixCollidingParticlesSM();
		}
	}
}
