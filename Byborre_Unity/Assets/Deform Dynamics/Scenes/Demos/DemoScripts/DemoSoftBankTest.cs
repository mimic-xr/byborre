using UnityEngine;

public class DemoSoftBankTest : Demo {
    public override void Load()
    {
        base.Load();

        demoTag = "DemoSoftBankTest";

        var g = new GameObject(demoTag + " Manager");
        var manager = g.AddComponent<DeformManager>();
        manager.selfCollisions = true;
        manager.ignoreIntersectingParticles = true;

        GameObject g0 = new GameObject(demoTag + " Shirt");
        DeformBody shirt = g0.AddComponent<DeformBody>();

        GameObject meshObject = Resources.Load<GameObject>("Meshes/sleeves_cut_edited");
        Mesh mesh = meshObject.GetComponentInChildren<MeshFilter>().sharedMesh;

        //shirt.SetMesh(mesh);
        //shirt.SetMaterial(m0);
        shirt.bendingStiffness = 0.005f;

        g0.transform.position = new Vector3(0, -4, 0);
        g0.transform.localScale = new Vector3(10, 10, 10);
        g0.AddComponent<Wireframe>();

        // Plane collider
        GameObject g1 = new GameObject(demoTag + " Plane Collider");
        DeformColliderPlane collider1 = g1.AddComponent<DeformColliderPlane>();

        collider1.transform.position = new Vector3(0, 0.001f, 0);
    }
}