using UnityEngine;

public class DemoLoadTriMesh : Demo
{
    public override void Load()
    {
        base.Load();

        demoTag = "DemoLoadTriMesh";

        var g = new GameObject(demoTag + " Manager");
        g.AddComponent<DeformManager>();

        GameObject g0 = new GameObject(demoTag + " Sphere");
        DeformBody sphere = g0.AddComponent<DeformBody>();

        GameObject meshObject = Resources.Load<GameObject>("Meshes/sphere_with_uvs");
        Mesh meshToSpawn = meshObject.GetComponentInChildren<MeshFilter>().sharedMesh;

        //sphere.SetMesh(meshToSpawn);
        //sphere.SetMaterial(m0);
        sphere.distanceStiffness = 0.9f;
        sphere.bendingStiffness = 0.9f;

        sphere.fixedVertices[0] = true;

        g0.transform.position = new Vector3(0, 5, 0);
        g0.transform.localScale = new Vector3(2, 2, 2);

        g0.AddComponent<Wireframe>();
    }
}