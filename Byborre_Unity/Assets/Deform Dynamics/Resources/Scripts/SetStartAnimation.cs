using System.Collections;
using UnityEngine;

public class SetStartAnimation : MonoBehaviour {
	

	Animator ac;

	// Use this for initialization
	void Start () {
		ac = GetComponent<Animator>();

        if (!ac) return;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.N))
		{
			if (ac)
			{
				ac.SetBool("StartAnim", true);
			}
			
		}
	}
}
