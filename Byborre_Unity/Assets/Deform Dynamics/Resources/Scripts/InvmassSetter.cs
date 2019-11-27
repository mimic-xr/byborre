using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using DeformDynamics;

public class InvmassSetter : MonoBehaviour
{
	public float startInvmass = 0;
	public float endInvmass = 1;
	public int numFrames = 0;

	private int currFrame;
	private DeformBody body;
	//private MeshRenderer mRenderer;

    // Start is called before the first frame update
    void Start()
    {
		body = GetComponent<DeformBody>();

		//body.disableRendering = true;

		//mRenderer = GetComponent<MeshRenderer>();

		DeformPlugin.Object.SetObjectInvmass((uint) body.GetId(), startInvmass);
		//mRenderer.enabled = false;
		currFrame = 0;
    }

    // Update is called once per frame
    void Update()
    {
		currFrame++;

		if(currFrame == numFrames)
		{
			//body.disableRendering = false;
			DeformPlugin.Object.SetObjectInvmass((uint) body.GetId(), endInvmass);
		}
    }
}
