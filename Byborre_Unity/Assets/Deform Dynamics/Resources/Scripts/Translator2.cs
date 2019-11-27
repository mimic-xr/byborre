using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translator2 : MonoBehaviour
{
    public Vector3 dir;
    public float vel;

    private bool should_translate = false;
    private int frames = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        frames++;
        if (frames >= 300)
        {
            should_translate = true;
        }

        if (should_translate)
        {
            transform.position = new Vector3(transform.position.x + (dir.x * vel),
                                             transform.position.y + (dir.y * vel),
                                             transform.position.z + (dir.z * vel));
        }
        
    }
}
