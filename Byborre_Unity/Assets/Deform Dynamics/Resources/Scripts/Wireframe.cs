using UnityEngine;

public class Wireframe : MonoBehaviour
{
    public bool showWireframe = true;

    [SerializeField, HideInInspector]
    private bool oldShowWireframe;

    private Material[] originalMaterials;

    // Start is called before the first frame update
    void Start()
    {
        ToggleWireframe();
    }

    private void OnValidate()
    {
        originalMaterials = GetComponent<MeshRenderer>().sharedMaterials;

        if (showWireframe != oldShowWireframe)
        {
            ToggleWireframe();
            oldShowWireframe = showWireframe;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            showWireframe = !showWireframe;
            ToggleWireframe();
            oldShowWireframe = showWireframe;
        }
    }

    void ToggleWireframe()
    {
        MeshRenderer mr = GetComponent<MeshRenderer>();

        if (showWireframe)
        {
            Material wireFrameMaterial = Instantiate(Resources.Load<Material>("Materials/WireframeOverlay"));

            Material[] materials = { mr.sharedMaterial, wireFrameMaterial };

            if (materials[0].shader.name.Equals("Deform Dynamics/Two Sided") ||
                materials[0].shader.name.Equals("Deform Dynamics/Two Sided Same Color"))
            {
                materials[1].SetFloat("_NormalBias", materials[0].GetFloat("_NormalBias"));
                materials[1].SetFloat("_CameraBias", materials[0].GetFloat("_CameraBias"));
            }

            mr.materials = materials;
        }
        else
        {
            Material[] materials = { mr.sharedMaterials[0] };

            mr.materials = materials;
        }
    }

    private void OnDestroy()
    {
        GetComponent<MeshRenderer>().materials = originalMaterials;
    }
}
