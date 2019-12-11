using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armature : MonoBehaviour
{
    // Start is called before the first frame update
    public float AngDrag;
    public Transform Hips;
    public Vector3 HipStart;
    public float Drag;
    public float Mass;
    public bool reset;
    public float speed;
    public bool cw;
    public bool ccw;
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
        HipStart = Hips.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Hips.transform.position = HipStart;
        if(reset==true)
        {
            DefultPose();
        }
        if(cw==true)
        {
            RotateCW();
        }
        if (ccw == true)
        {
            RotateCCW();
        }
    }

    void RotateCW()
    {
        bone[0].GetComponent<Transform>().RotateAround(transform.position, transform.up, Time.deltaTime * 60f);
    }
    void RotateCCW()
    {
        bone[0].GetComponent<Transform>().RotateAround(transform.position, transform.up, Time.deltaTime * -60f);
    }
    void DefultPose()
    {
        int i = 0;
        while (i<6)
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
