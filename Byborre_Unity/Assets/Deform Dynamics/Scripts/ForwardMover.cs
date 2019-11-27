using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForwardMover : MonoBehaviour {

    public float speed;

    bool movingForward;
    
    // Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (movingForward)
        {
            transform.position += transform.forward * Time.deltaTime * speed;
        }
        else
        {
            transform.position += -transform.forward * Time.deltaTime * speed;
        }

        if (transform.position.z > 1.5f)
        {
            movingForward = false;
        }
        else if(transform.position.z < -2f)
        {
            movingForward = true;
        }
    }
}
