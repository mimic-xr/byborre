using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DropdownHandler : MonoBehaviour {

    public Dropdown dropdown;

    float startTime;

	// Use this for initialization
	void Start () {
		var optionDataList = new List<Dropdown.OptionData>();
 
         for(int i = 0; i < SceneManager.sceneCountInBuildSettings; ++i) {
             string name = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
             optionDataList.Add(new Dropdown.OptionData(name));
         }
 
         startTime = Time.realtimeSinceStartup;
            
         dropdown.ClearOptions();
         dropdown.AddOptions(optionDataList);

         dropdown.value = SceneManager.GetActiveScene().buildIndex;
	}

    public void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (!Camera.current) return;
        
            MouseOrbit camController = 
                Camera.current.gameObject.GetComponent<MouseOrbit>();
        
            if (camController) camController.enabled = true;
        }
    }

    public void OnSceneSelected()
    {
        if (Time.realtimeSinceStartup - startTime < 1) return;

        int selectedScene = dropdown.value;
        
        SceneManager.LoadScene(selectedScene);
    }

    public void OnModificationStart()
    {
        if (!Camera.current) return;
        
        MouseOrbit camController = 
            Camera.current.gameObject.GetComponent<MouseOrbit>();
        
        if (camController) camController.enabled = false;
    }

    public void OnModificationEnd()
    {
        if (!Camera.current) return;
        
        MouseOrbit camController = 
            Camera.current.gameObject.GetComponent<MouseOrbit>();
        
        if (camController) camController.enabled = true;
    }
}
