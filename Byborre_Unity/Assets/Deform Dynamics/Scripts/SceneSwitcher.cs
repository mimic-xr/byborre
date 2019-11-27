using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour {
    // Use this for initialization
	void Start ()
    {

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            currentScene = currentScene == 0 ? SceneManager.sceneCountInBuildSettings - 1 : currentScene - 1;
            SceneManager.LoadScene(currentScene);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            int currentScene = SceneManager.GetActiveScene().buildIndex;
            currentScene = (currentScene + 1) % SceneManager.sceneCountInBuildSettings;
            SceneManager.LoadScene(currentScene);
        }
    }
}
