using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotUtil : MonoBehaviour
{
    public KeyCode key;
    bool shoot = false;
    private int count;

    // Start is called before the first frame update
    void Start()
    {
        count = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            shoot = !shoot;
        }

        if (shoot)
        {
            ScreenCapture.CaptureScreenshot("screenshot" + count + ".png", 8);
            count++;
        }
    }
}
