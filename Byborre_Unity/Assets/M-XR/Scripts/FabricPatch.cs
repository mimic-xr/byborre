using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FabricPatch : MonoBehaviour
{
    public int Index;
    public GameObject Manager;
    public Vector3 Startp;
    public Quaternion Sangle;
    public bool debug;
    void Start()
    {
        //Startp = GetComponent<Transform>().transform.position;
        //Sangle = GetComponent<Transform>().transform.rotation;
        Manager = GameObject.Find("Manager");
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
            //GetComponent<Transform>().transform.position = Startp;
            //GetComponent<Transform>().transform.rotation = Sangle;
        }
        if(other.name=="Target")
        {
            Manager.GetComponent<LineRenderer>().enabled = true;
            int ID = other.GetComponent<Transform>().parent.gameObject.GetComponent<MatBox>().Index;
            Manager.GetComponent<FabricManager>().UpdateLines(ID);
        }
        if (other.name == "FabricZone")
        {
            foreach (GameObject box in Manager.GetComponent<FabricManager>().FabricBox)
            {
                box.SetActive(false);
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Target")
        {
            int ID = other.GetComponent<Transform>().parent.gameObject.GetComponent<MatBox>().Index;
            Manager.GetComponent<LineRenderer>().enabled = false;
        }
        if (other.name == "FabricZone")
        {
            foreach (GameObject box in Manager.GetComponent<FabricManager>().RotationBox)
            {
                box.SetActive(false);
            }

            foreach (GameObject box in Manager.GetComponent<FabricManager>().FabricBox)
            {
                box.SetActive(true);
            }    
        }
    }
}
