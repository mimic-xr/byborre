using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

	public Vector3 axis = Vector3.up;
	public float velocity = 0.01f;
	public bool stopEnabled = false;

	private int frames = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//transform.Rotate(axis, velocity);

		if (stopEnabled)
		{
			frames++;
			if(frames > 450)
			{
				return;
			}
		}
		
		transform.RotateAround(transform.position, axis.normalized, velocity);

		
	}
}
