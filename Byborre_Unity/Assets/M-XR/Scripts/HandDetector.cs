using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandDetector : MonoBehaviour
{
    public bool touching;
    public bool started;
    public SceneReset S_Reset;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer==LayerMask.NameToLayer("Hands"))
        {
            S_Reset.No_Activity = false;
            started = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hands"))
        {
            S_Reset.No_Activity = true;
        }
    }
}
