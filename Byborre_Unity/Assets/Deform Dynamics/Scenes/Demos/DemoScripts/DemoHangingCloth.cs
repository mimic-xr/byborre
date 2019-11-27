using UnityEngine;

public class DemoHangingCloth : Demo {
    public override void Load()
    {
        base.Load();

        demoTag = "DemoHangingCloth";

        uint res = 61;

        var g = new GameObject(demoTag + " Manager");
        var manager = g.AddComponent<DeformManager>();
        manager.selfCollisions = true;

        GameObject g0 = new GameObject(demoTag + " Cloth");
        DeformPatchCreator patchCreator0 = g0.AddComponent<DeformPatchCreator>();

        patchCreator0.resolution = res;
        patchCreator0.size = new Vector2(4, 4);
        patchCreator0.Create();

        DeformBody body0 = g0.AddComponent<DeformBody>();

        //body0.SetMaterial(m0);
        body0.fixedVertices[0] = true;
        body0.fixedVertices[res - 1] = true;

        g0.transform.position = new Vector3(0, 2, 0);
        g0.AddComponent<Wireframe>();
    }
}