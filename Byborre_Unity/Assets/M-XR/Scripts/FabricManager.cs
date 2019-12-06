using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FabricManager : MonoBehaviour
{
    public LineRenderer lineRenderer;
    [System.Serializable]
    public class Pattern
    {
        public GameObject[] Pannel;
        public Material[] Fabric;
        public GameObject[] UItarget;
        public GameObject BoxTarget;
    }
    public Pattern[] Patterns;
    public GameObject[] FabricBox;
    public Material[] FabricBox_mats;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<LineRenderer>().enabled = true;
        lineRenderer = GetComponent<LineRenderer>();
        GetComponent<LineRenderer>().enabled = false;
        foreach (GameObject box in FabricBox)
        {
            box.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetMaterial(int pannel_index,int fabric_index)
    {
        //Changes the pannel to the the new material
        foreach (GameObject fabric in Patterns[pannel_index].Pannel)
        {
            fabric.GetComponent<Renderer>().material = Patterns[pannel_index].Fabric[fabric_index];
        }
        //changes the fabric box to the new material
        FabricBox[pannel_index].transform.GetChild(0).gameObject.GetComponent<Renderer>().material = FabricBox_mats[fabric_index];
        foreach(GameObject Box in FabricBox)
        {
            Box.SetActive(false);
        }
    }
    public void UpdateLines(int index)
    {
        lineRenderer.SetPosition(0, Patterns[index].BoxTarget.GetComponent<Transform>().position);
        lineRenderer.SetPosition(1, Patterns[index].UItarget[0].GetComponent<Transform>().position);
    }
}
