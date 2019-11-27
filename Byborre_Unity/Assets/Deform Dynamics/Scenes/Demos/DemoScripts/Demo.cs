using UnityEngine;
using UnityEngine.UI;

public abstract class Demo : MonoBehaviour {

    public bool shouldLoad;
    public string demoTag = "Undefined"; // To be set by each demo

    [SerializeField]
    protected Material m0, m1, m2, m3, mw;

    public virtual void Load()
    {     
        m0 = Resources.Load<Material>("Materials/ClothYellow");
        m1 = Resources.Load<Material>("Materials/ClothGreen");
        m2 = Resources.Load<Material>("Materials/ClothBlue");
        m3 = Resources.Load<Material>("Materials/ClothRed");
    }

    public void Unload()
    {
        GameObject[] objects = FindObjectsOfType<GameObject>();

        foreach (GameObject o in objects)
        {
            if(o.name.Contains(demoTag))
            {
                DestroyImmediate(o);
            }
        }
    }
}