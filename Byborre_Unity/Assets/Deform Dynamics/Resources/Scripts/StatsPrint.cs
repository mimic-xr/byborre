using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsPrint : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
	{
		int v_count = GetComponent<MeshFilter>().sharedMesh.vertexCount;
		int i_count = GetComponent<MeshFilter>().sharedMesh.triangles.Length;

		Debug.Log("[StatsPrint : " + name + "] vertex count: " + v_count + " index count: " + i_count);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
