using UnityEngine;

public class DemoStitching : Demo
{
    public override void Load()
    {
        demoTag = "DemoStitching";

        base.Load();

        // Manager
        var g = new GameObject(demoTag + " Manager");
        var manager = g.AddComponent<DeformManager>();
        manager.selfCollisions = true;

        uint res = 140;        

        // Cloth 1
        GameObject g0 = new GameObject(demoTag + " Cloth 1");
        DeformPatchCreator patchCreator0 = g0.AddComponent<DeformPatchCreator>();

        patchCreator0.size = new Vector2(7, 1.4f);
        patchCreator0.resolution = res;
        patchCreator0.Create();

        DeformBody body0 = g0.AddComponent<DeformBody>();

        body0.bendingStiffness = 0.0012f;
        body0.SetAllKFrictionVertices(0.01f);
        //body0.SetMaterial(m0);
        
        g0.transform.rotation = Quaternion.Euler(90, 0, 0);
        g0.transform.position = new Vector3(0, 0, 0.6f);
        g0.AddComponent<Wireframe>();

        // Cloth 2
        GameObject g1 = new GameObject(demoTag + " Cloth 2");
        DeformPatchCreator patchCreator1 = g1.AddComponent<DeformPatchCreator>();

        patchCreator1.size = new Vector2(7, 1.4f);
        patchCreator1.resolution = res;
        patchCreator1.Create();

        DeformBody body1 = g1.AddComponent<DeformBody>();

        body1.bendingStiffness = 0.0012f;
        body1.SetAllKFrictionVertices(0.01f);
        //body1.SetMaterial(m1);

        g1.transform.rotation = Quaternion.Euler(90, 0, 0);
        g1.transform.position = new Vector3(0, 0, -0.6f);
        g1.AddComponent<Wireframe>();

        // Capsule collider
        GameObject g2 = new GameObject(demoTag + " Capsule Collider");
        DeformColliderCapsule collider = g2.AddComponent<DeformColliderCapsule>();

        collider.capsuleType = DeformColliderCapsule.CapsuleType.Length;
        collider.length = 8;

        collider.radiusA = 0.25f;
        collider.radiusB = 0.5f;

        collider.bias = 0.04f;
        collider.inGameMaterial = m3;
        collider.showColliderInGame = true;

        collider.transform.rotation = Quaternion.Euler(0, 0, -90);
        
        //Seams
        GameObject g3 = new GameObject(demoTag + " Top Seam");
        DeformSeam topSeam = g3.AddComponent<DeformSeam>();
        topSeam.bodyA = body0;
        topSeam.bodyB = body1;
        
        topSeam.bodyASeamBegin = 0;
        topSeam.bodyASeamEnd = 139;

        topSeam.bodyBSeamBegin = 0;
        topSeam.bodyBSeamEnd = 139;

        topSeam.Create();

        GameObject g4 = new GameObject(demoTag + " Bottom Seam");
        DeformSeam bottomSeam = g4.AddComponent<DeformSeam>();
        bottomSeam.bodyA = body0;
        bottomSeam.bodyB = body1;
        
        bottomSeam.bodyASeamBegin = (140 * 27) + 139;
        bottomSeam.bodyASeamEnd = 140 * 27;

        bottomSeam.bodyBSeamBegin = (140 * 27) + 139;
        bottomSeam.bodyBSeamEnd = 140 * 27;

        bottomSeam.Create();

        // Plane collider
        GameObject g5 = new GameObject(demoTag + " Plane Collider");
        DeformColliderPlane planeCollider = g5.AddComponent<DeformColliderPlane>();
        planeCollider.bias = 0.005f;
    }
}