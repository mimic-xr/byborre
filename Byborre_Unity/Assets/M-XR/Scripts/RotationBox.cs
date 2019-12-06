using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationBox : MonoBehaviour
{
    public bool ccw;
    public GameObject Manager;

    void Start()
    {
        Manager = GameObject.Find("Man");
    }
    private void OnTriggerEnter(Collider other)
    {
        if(ccw==true)
        {
            Manager.GetComponent<Armature>().ccw = true;
            Manager.GetComponent<Armature>().cw = false;
        }
        else
        {
            Manager.GetComponent<Armature>().cw = true;
            Manager.GetComponent<Armature>().ccw = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Manager.GetComponent<Armature>().ccw = false;
        Manager.GetComponent<Armature>().cw = false;
    }
}
