using UnityEngine;

public class DemoRibbon : Demo {
    public override void Load()
    {
        base.Load();

        demoTag = "DemoRibbon";

        var g = new GameObject(demoTag + " Manager");
        var manager = g.AddComponent<DeformManager>();
        manager.selfCollisions = true;
        manager.airFriction = 0.005f;

        uint res = 400;

        GameObject g0 = new GameObject(demoTag + " Cloth");
        DeformPatchCreator patchCreator = g0.AddComponent<DeformPatchCreator>();

        patchCreator.resolution = res;
        patchCreator.size = new Vector2(1, 16);
        patchCreator.Create();

        DeformBody body0 = g0.AddComponent<DeformBody>();
        //body0.SetMaterial(m0);
        
        g0.transform.rotation = Quaternion.Euler(85, 0, 0);
        g0.transform.position = new Vector3(0, 9, 0);
        g0.AddComponent<Wireframe>();

        GameObject g1 = new GameObject(demoTag + " Plane Collider");
        g1.AddComponent<DeformColliderPlane>();
        
        g1.transform.position = new Vector3(0, 0.01f, 0);
    }
}
