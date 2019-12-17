using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    public GameObject Manager;
    public int Index;
    public Animator anim;
    public Animator A_anim;
    public Animator B_anim;

    void Start()
    {
        Manager = GameObject.Find("Manager");
    }
    private void OnTriggerEnter(Collider other)
    {
        Manager.GetComponent<LightController>().Switch(Index);
        anim.SetBool("TurnOn", true);
        A_anim.SetBool("TurnOff", true);
        B_anim.SetBool("TurnOff", true);
        anim.SetBool("TurnOff", false);
        A_anim.SetBool("TurnOn", false);
        B_anim.SetBool("TurnOn", false);
    }
}
