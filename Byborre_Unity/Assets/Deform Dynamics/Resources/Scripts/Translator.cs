using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translator : MonoBehaviour
{
	public Vector3 endPosition = new Vector3(0, 0, 0);
	public float speed = 1.0f;
	private bool positiveDir = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < endPosition.y && positiveDir)
		{
			positiveDir = false;
		}

		if (positiveDir)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y - speed, transform.position.z);
		}

		if (!positiveDir)
		{
			transform.position = new Vector3(transform.position.x, transform.position.y + speed, transform.position.z);
		}
	}
}
