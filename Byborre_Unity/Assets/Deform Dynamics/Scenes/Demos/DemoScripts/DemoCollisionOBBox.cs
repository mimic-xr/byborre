using UnityEngine;

public class DemoCollisionOBBox : Demo {

    public override void Load()
    {
        base.Load();

        demoTag = "DemoCollisionOBBox";

        var g = new GameObject(demoTag + " Manager");
        var manager = g.AddComponent<DeformManager>();
        manager.selfCollisions = true;

        uint res = 101;

        GameObject g0 = new GameObject(demoTag + " Cloth");
        DeformPatchCreator patchCreator0 = g0.AddComponent<DeformPatchCreator>();

        patchCreator0.resolution = res;
        patchCreator0.size = new Vector2(4, 4);
        patchCreator0.Create();

        DeformBody body0 = g0.AddComponent<DeformBody>();

        //body0.SetMaterial(m2);
        body0.bendingStiffness = 0.0012f;
        body0.SetAllKFrictionVertices(0.25f);

        g0.transform.position = new Vector3(0, 1.25f, 0);
        g0.AddComponent<Wireframe>();

        GameObject g1 = new GameObject(demoTag + " Collider");
        DeformColliderBox collider = g1.AddComponent<DeformColliderBox>();
        
        collider.bias = 0.04f;
        collider.inGameMaterial = m1;

        g1.transform.localScale = new Vector3(3, 0.25f, 3);
    }
}