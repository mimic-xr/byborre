using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchController : MonoBehaviour
{
    public bool Open;

    public GameObject PatchSystem;
    public GameObject CloseButton;
    public GameObject LightSwitches;
    public GameObject Manager;
    public GameObject[] FabricBox;

    void Start()
    {
        Manager = GameObject.Find("Manager");
        PatchSystem.SetActive(false);
        CloseButton.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(Open==false)
        {
            OpenPatch();
        }
    }

    public void OpenPatch()
    {
        PatchSystem.SetActive(true);
        CloseButton.SetActive(true);
        gameObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().enabled = false;
        gameObject.transform.GetChild(1).gameObject.GetComponent<Renderer>().enabled = false;
        LightSwitches.SetActive(false);
        Open = true;
        Manager.GetComponent<FabricManager>().HideArrows();
        foreach (GameObject box in Manager.GetComponent<FabricManager>().FabricBox)
        {
            box.SetActive(true);
        } 
    }
    public void ClosePatch()
    {
        Manager.GetComponent<FabricManager>().CloseUI();
    }
}
