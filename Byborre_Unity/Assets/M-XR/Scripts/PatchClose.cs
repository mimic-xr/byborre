using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchClose : MonoBehaviour
{
    public PatchController pcontroller;
    private void OnTriggerEnter(Collider other)
    {
        pcontroller.ClosePatch();
    }
}
