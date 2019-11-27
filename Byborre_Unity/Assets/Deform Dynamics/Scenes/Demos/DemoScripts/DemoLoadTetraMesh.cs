using UnityEngine;

public class DemoLoadTetraMesh : Demo
{
    public override void Load()
    {
        base.Load();

        demoTag = "DemoLoadTetraMesh";        

        var g = new GameObject(demoTag + " Manager");
        var manager = g.AddComponent<DeformManager>();
        manager.selfCollisions = true;
        manager.ignoreIntersectingParticles = true;

        GameObject g0 = new GameObject(demoTag + " Armadillo");
        g0.AddComponent<MeshFilter>();
        g0.AddComponent<MeshRenderer>();
        DeformBodyVolumetric armadillo = g0.AddComponent<DeformBodyVolumetric>();

        armadillo.selectedPath = 2; // Hack to make sure that UI corresponds with selected path
        armadillo.SetPath("/armadillo_8k.mesh");
        //armadillo.SetMaterial(m0);
        armadillo.distanceStiffness = 1.0f;
        armadillo.bendingStiffness = 0.009f;
        armadillo.volumeStiffness = 1.0f;

        g0.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        g0.transform.rotation = Quaternion.Euler(0, 180, 0);
        g0.transform.position = new Vector3(0, 1, 0);

        g0.AddComponent<Wireframe>();

        GameObject g1 = new GameObject(demoTag + " Plane Collider");
        g1.AddComponent<DeformColliderPlane>();
    }
}