using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public GameObject Manager;
    public int Index;
    void Start()
    {
        Manager = GameObject.Find("Manager");
    }
    private void OnTriggerEnter(Collider other)
    {
        Manager.GetComponent<LightController>().Switch(Index);
    }
}
