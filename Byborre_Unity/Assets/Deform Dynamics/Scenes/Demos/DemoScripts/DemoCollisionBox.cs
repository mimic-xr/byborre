using UnityEngine;

public class DemoCollisionBox : Demo
{
    public override void Load()
    {
        base.Load();

        demoTag = "DemoCollisionBox";

        uint res = 81;

        // Manager
        var g = new GameObject(demoTag + " Manager");
        var manager = g.AddComponent<DeformManager>();
        manager.selfCollisions = true;
        manager.ignoreIntersectingParticles = true;

        // Bottom cloth
        GameObject g0 = new GameObject(demoTag + " Cloth 1");
        DeformPatchCreator patchCreator0 = g0.AddComponent<DeformPatchCreator>();

        patchCreator0.resolution = res;
        patchCreator0.size = new Vector2(4, 4);
        patchCreator0.Create();

        DeformBody body0 = g0.AddComponent<DeformBody>();

        //body0.SetMaterial(m0);
        body0.SetAllKFrictionVertices(0.25f);

        g0.transform.position = new Vector3(0, 2, 0);
        g0.transform.rotation = Quaternion.Euler(0, 27, 0);
        g0.AddComponent<Wireframe>();

        // Middle cloth
        GameObject g1 = new GameObject(demoTag + " Cloth 2");
        DeformPatchCreator patchCreator1 = g1.AddComponent<DeformPatchCreator>();

        patchCreator1.resolution = res;
        patchCreator1.size = new Vector2(4, 4);
        patchCreator1.Create();

        DeformBody body1 = g1.AddComponent<DeformBody>();

        //body1.SetMaterial(m2);
        //body1.SetAllKFrictionVertices(0.25f);

        g1.transform.position = new Vector3(0, 3, 0);
        g1.transform.rotation = Quaternion.Euler(0, 54, 0);
        g1.AddComponent<Wireframe>();

        // Top cloth
        GameObject g2 = new GameObject(demoTag + " Cloth 3");
        DeformPatchCreator patchCreator2 = g2.AddComponent<DeformPatchCreator>();

        patchCreator2.resolution = res;
        patchCreator2.size = new Vector2(4, 4);
        patchCreator2.Create();

        DeformBody body2 = g2.AddComponent<DeformBody>();

        //body2.SetMaterial(m3);
        body2.SetAllKFrictionVertices(0.25f);

        g2.transform.position = new Vector3(0, 4, 0);
        g2.transform.rotation = Quaternion.Euler(0, 81, 0);
        g2.AddComponent<Wireframe>();

        // Box collider

        GameObject g3 = new GameObject(demoTag + " Box Collider");
        DeformColliderBox collider0 = g3.AddComponent<DeformColliderBox>();
        
        collider0.bias = 0.04f;
        collider0.inGameMaterial = m1;

        g3.transform.localScale = new Vector3(3, 0.5f, 3);
        g3.transform.position = new Vector3(0, 0.25f, 0);

        // Plane collider

        GameObject g4 = new GameObject(demoTag + " Plane Collider");
        DeformColliderPlane collider1 = g4.AddComponent<DeformColliderPlane>();

        collider1.transform.position = new Vector3(0, 0.04f, 0);
    }
}