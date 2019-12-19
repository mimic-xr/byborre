using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReset : MonoBehaviour
{
    public float Wait_Time;
    public float timer;
    public bool No_Activity=false;

    private void Update()
    {
        if(No_Activity==true)
        {
            timer += Time.deltaTime;
            if(timer>Wait_Time)
            {
                No_Activity = false;
                timer = 0;
                Reset();
            }
        }
        if(No_Activity==false)
        {
            timer = 0;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Reset();
    }
    private void Reset()
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
