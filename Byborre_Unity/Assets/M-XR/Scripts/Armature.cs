using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armature : MonoBehaviour
{
    // Start is called before the first frame update
    public float AngDrag;
    public float Drag;
    public float Mass;
    public bool reset;
    public float speed;
    public GameObject[] bone;
    public Quaternion[] StartRotation;
    void Awake()
    {
        int i = 0;
        while (i<6)
        {
            bone[i].GetComponent<Rigidbody>().drag = Drag;
            bone[i].GetComponent<Rigidbody>().drag = Drag;
            bone[i].GetComponent<Rigidbody>().angularDrag = AngDrag;
            bone[i].GetComponent<Rigidbody>().mass = Mass;
            StartRotation[i] = bone[i].GetComponent<Transform>().rotation;
            i = i + 1;
        }

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(reset==true)
        {
            DefultPose();
        }
    }

    void DefultPose()
    {
        int i = 0;
        while (i < 6)
        {
            LerpRotation(bone[i].GetComponent<Transform>(), StartRotation[i]);
            i = i + 1;
        }
    }

    void LerpRotation(Transform trans,Quaternion destination)
    { 
        trans.rotation = Quaternion.Slerp(trans.rotation, destination, speed/100);
    }
}
