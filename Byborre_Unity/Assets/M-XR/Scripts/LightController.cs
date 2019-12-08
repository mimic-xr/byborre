using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [System.Serializable]
    public class LightGroup
    {
        public GameObject[] Light;
    }
    public LightGroup[] LightGroups;

    public void Switch(int index)
    {
        int i = 0;
        foreach(LightGroup group in LightGroups)
        {
            foreach (GameObject light in LightGroups[i].Light)
            {
                light.SetActive(false);
            }
            i++;
        }
        foreach (GameObject light in LightGroups[index].Light)
        {
            light.SetActive(true);
        }
    }
}
