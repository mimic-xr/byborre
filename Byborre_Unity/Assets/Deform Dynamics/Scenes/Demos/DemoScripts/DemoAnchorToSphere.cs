using UnityEngine;

public class DemoAnchorToSphere : Demo {
    
    private float omega = 0;

    private Material[] materials;
    
    public override void Load()
    {
        base.Load();

        demoTag = "DemoAnchorToSphere";

        // Manager
        var g0 = new GameObject(demoTag + " Manager");
        var manager = g0.AddComponent<DeformManager>();
        manager.selfCollisions = true;
        manager.ignoreIntersectingParticles = true;
        manager.gravity = Vector3.zero;
		manager.solverIterations = 2;

        // Sphere collider
        GameObject g1 = new GameObject(demoTag + " Sphere Collider");
        DeformColliderSphere collider1 = g1.AddComponent<DeformColliderSphere>();
        collider1.radius = 0.84f;

        collider1.inGameMaterial = m2;
        collider1.bias = 0.05f;
        
        // Skirt
        GameObject g2 = new GameObject(demoTag + " Skirt");
        DeformBody skirt = g2.AddComponent<DeformBody>();

        GameObject meshObject = Resources.Load<GameObject>("Meshes/custom_skirt2");
        Mesh meshToSpawn = meshObject.GetComponentInChildren<MeshFilter>().sharedMesh;

        //skirt.SetMesh(meshToSpawn);
        //skirt.SetMaterial(m0);

        skirt.bendingStiffness = 0.00012f;

        AnchorToCollider atc = g2.AddComponent<AnchorToCollider>();
        atc.anchorCollider = collider1;

        atc.anchorAtStart = true;

        g2.transform.localScale = new Vector3(4, 4, 4);
        g2.AddComponent<Wireframe>();
    }

    private void Start()
    {
        materials = new Material[4];

        materials[0] = Resources.Load<Material>("Cloth");
        materials[1] = Resources.Load<Material>("Cloth 1");
        materials[2] = Resources.Load<Material>("Cloth 2");
        materials[3] = Resources.Load<Material>("Cloth 3");
    }

    // Update is called once per frame
    void Update()
    {
        GameObject c = GameObject.Find(demoTag + " Sphere Collider");

        c.transform.position = new Vector3(0, Mathf.Sin(omega * 0.5f) * 0.5f, 0);
        c.transform.RotateAround(c.transform.position, Vector3.up, Mathf.Sin(omega * 0.5f) * 7.16f); //0.125 radians ~= 7.16 degrees

        omega += 0.016f;
    }
}