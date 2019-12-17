using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReset : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Reset();
    }
    private void Reset()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
