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
    public GameObject[] RotationBox;
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
        foreach (GameObject Box in RotationBox)
        {
            Box.SetActive(true);
        }
    }
    public void UpdateLines(int index)
    {
        int TargetLine = FindClosest(index);
        lineRenderer.SetPosition(0, Patterns[index].BoxTarget.GetComponent<Transform>().position);
        lineRenderer.SetPosition(1, Patterns[index].UItarget[TargetLine].GetComponent<Transform>().position);
    }

    int FindClosest(int index)
    {
        Vector3 a = Patterns[index].UItarget[0].GetComponent<Transform>().position;
        Vector3 b = Patterns[index].UItarget[1].GetComponent<Transform>().position;
        Vector3 Target = FabricBox[index].GetComponent<Transform>().position;
        float dist_a = Vector3.Distance(a, Target);
        float dist_b = Vector3.Distance(b, Target);
        int value = 0;
        if(dist_a<dist_b)
        {
            value = 0;
        }
        if(dist_b<dist_a)
        {
            value = 1;
        }
        return value;
    }
}
