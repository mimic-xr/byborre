using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FabricManager : MonoBehaviour
{
    [System.Serializable]
    public class Pattern
    {
        public GameObject[] Pannel;
        public Material[] Fabric;
        public GameObject[] UItarget;
        public GameObject BoxTarget;
    }
    public GameObject PatchClose;
    public GameObject PatchOpen;
    public GameObject Patches;
    public GameObject LightSwitches;
    public Pattern[] Patterns;
    public GameObject[] FabricBox;
    public Material[] FabricBox_mats;
    public GameObject[] RotationBox;
    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject box in FabricBox)
        {
            box.SetActive(false);
        }
    }

    void Update()
    {

    }

    public void HideArrows()
    {
        foreach (GameObject box in RotationBox)
        {
            box.SetActive(false);
        }
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
        CloseUI();
    }
    public void CloseUI()
    {
        PatchClose.SetActive(false);
        PatchOpen.transform.GetChild(0).gameObject.GetComponent<Renderer>().enabled = true;
        PatchOpen.transform.GetChild(1).gameObject.GetComponent<Renderer>().enabled = true;
        PatchOpen.GetComponent<PatchController>().Open = false;
        LightSwitches.SetActive(true);
        Patches.SetActive(false);
        foreach (GameObject Box in FabricBox)
        {
            Box.SetActive(false);
        }
        foreach (GameObject box in RotationBox)
        {
            box.SetActive(true);
        }
    }
}
