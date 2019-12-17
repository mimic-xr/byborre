using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FabricPatch : MonoBehaviour
{
    public int Index;
    public bool Grasp;
    public GameObject Manager;
    public Vector3 Startp;
    public Quaternion Sangle;
    public bool debug;
    public GameObject Patches;
    void Start()
    {
        Manager = GameObject.Find("Manager");
        Grasp = false;
        gameObject.SetActive(true);
    }
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.name=="MaterialBox")
        {
            int ID = other.GetComponent<MatBox>().Index;
            Manager.GetComponent<FabricManager>().SetMaterial(ID, Index);
            Patches.SetActive(false);
        }
        if(other.name=="Target")
        {
            Manager.GetComponent<LineRenderer>().enabled = true;
            int ID = other.GetComponent<Transform>().parent.gameObject.GetComponent<MatBox>().Index;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Target")
        {
            int ID = other.GetComponent<Transform>().parent.gameObject.GetComponent<MatBox>().Index;
        }
    }
}
