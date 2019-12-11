using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateY : MonoBehaviour
{
    public float speed;
    public Vector3 position;

	// Use this for initialization
	void Start ()
    {
        position = GetComponent<Transform>().position;
	}
	
	// Update is called once per frame
	void Update ()
    {
        transform.RotateAround(position, Vector3.up, speed * Time.deltaTime);
    }
}
