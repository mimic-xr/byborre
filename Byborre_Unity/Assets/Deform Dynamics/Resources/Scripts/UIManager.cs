using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public Slider distanceStiffnessSlider;
    public Slider bendingStiffnessSlider;

    public Slider gravityXSlider;
    public Slider gravityYSlider;
    public Slider gravityZSlider;

    public Slider windXSlider;
    public Slider windYSlider;
    public Slider windZSlider;

    public Slider airFrictionSlider;

    public Text valueText;

    DeformManager deformManager;

    float delay = 2.5f;
    float lastUIUpdate;
    Vector3 currentGravity;
    Vector3 currentWind;

	// Use this for initialization
	void Start ()
    {
        deformManager = FindObjectOfType<DeformManager>();
        DeformBody firstBody = FindObjectOfType<DeformBody>();

        if(!deformManager)
        {
            enabled = false;
            return;
        }

        currentGravity = deformManager.gravity;
        currentWind = deformManager.wind;

		gravityXSlider.value = currentGravity.x;
        gravityYSlider.value = currentGravity.y;
        gravityZSlider.value = currentGravity.z;

        windXSlider.value = currentWind.x;
        windYSlider.value = currentWind.y;
        windZSlider.value = currentWind.z;

        airFrictionSlider.value = deformManager.airFriction;

        if (firstBody)
        {
            distanceStiffnessSlider.value = firstBody.distanceStiffness;
            bendingStiffnessSlider.value = firstBody.bendingStiffness;
        }

        lastUIUpdate = -delay;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (!deformManager) return;

        currentGravity = deformManager.gravity;

		if ((Time.realtimeSinceStartup - lastUIUpdate) > delay)
        {
            valueText.text = "";
            lastUIUpdate = Time.realtimeSinceStartup; 
        }
	}

    public void SetDistanceStiffness()
    {
        if(!deformManager) return;

        deformManager.SetDistanceStiffness(distanceStiffnessSlider.value);
        valueText.text = distanceStiffnessSlider.value.ToString("F2");
    }

    public void SetBendingStiffness()
    {
        if(!deformManager) return;

        deformManager.SetBendingStiffness(bendingStiffnessSlider.value);
        valueText.text = bendingStiffnessSlider.value.ToString("F2");
    }

    public void SetGravityX()
    {
        if(!deformManager) return;

        deformManager.gravity.x = gravityXSlider.value;
        valueText.text = gravityXSlider.value.ToString("F2");
    }

    public void SetGravityY()
    {
        if(!deformManager) return;

        deformManager.gravity.y = gravityYSlider.value;
        valueText.text = gravityYSlider.value.ToString("F2");
    }

    public void SetGravityZ()
    {
        if(!deformManager) return;

        deformManager.gravity.z = gravityZSlider.value;
        valueText.text = gravityZSlider.value.ToString("F2");
    }

    public void SetWindX()
    {
        if(!deformManager) return;

        deformManager.wind.x = windXSlider.value;
        valueText.text = windXSlider.value.ToString("F2");
    }

    public void SetWindY()
    {
        if(!deformManager) return;

        deformManager.wind.y = windYSlider.value;
        valueText.text = windYSlider.value.ToString("F2");
    }

    public void SetWindZ()
    {
        if(!deformManager) return;

        deformManager.wind.z = windZSlider.value;
        valueText.text = windZSlider.value.ToString("F2");
    }

    public void SetAirFriction()
    {
        if(!deformManager) return;

        deformManager.airFriction = airFrictionSlider.value;
        valueText.text = airFrictionSlider.value.ToString("F2");
    }
    
    public void SetLastUIUpdate()
    {
        lastUIUpdate = Time.realtimeSinceStartup; 
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
