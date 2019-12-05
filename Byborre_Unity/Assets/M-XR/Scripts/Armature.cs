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
    public float t=0f;
    public float speed;
    bool save;
    public GameObject[] bone;
    public Vector3[] StartRotation;
    public Vector3[] CurrentRotation;
    void Awake()
    {
        int i = 0;
        while (i<6)
        {
            bone[i].GetComponent<Rigidbody>().drag = Drag;
            bone[i].GetComponent<Rigidbody>().drag = Drag;
            bone[i].GetComponent<Rigidbody>().angularDrag = AngDrag;
            bone[i].GetComponent<Rigidbody>().mass = Mass;
            StartRotation[i] = bone[i].GetComponent<Transform>().eulerAngles;
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
            if(save==false)
            {
                SaveLerpStart();
                save = true;
            }

            DefultPose();
            t += (speed/100f) * Time.deltaTime;
            if(t>=1)
            {
                reset = false;
                save = false;
                t = 0;
            }
        }
    }

    void SaveLerpStart()
    {
        int i = 0;
        while (i < 6)
        {
            CurrentRotation[i] = bone[i].GetComponent<Transform>().eulerAngles;
            i = i + 1;
        }
    }

    void DefultPose()
    {
        int i = 0;
        while (i < 6)
        {
            LerpRotation(bone[i].GetComponent<Transform>(), CurrentRotation[i], StartRotation[i]);
            i = i + 1;
        }
    }

    void LerpRotation(Transform trans,Vector3 current,Vector3 destination)
    { 
        trans.eulerAngles = new Vector3(Mathf.Lerp(current.x, destination.x, t), Mathf.Lerp(current.y, destination.y, t), Mathf.Lerp(current.z, destination.z, t));
    }
}
