using UnityEngine;

public class DemoPillow : Demo
{
    public override void Load()
    {
        base.Load();

        demoTag = "DemoPillow";

        var g = new GameObject(demoTag + " Manager");
        var manager = g.AddComponent<DeformManager>();
        manager.selfCollisions = true;
        manager.ignoreIntersectingParticles = true;
        manager.gravity = new Vector3(0, -1f, 0);
        manager.kineticFriction = 0.01f;
        manager.staticFriction = 0.02f;
        manager.airFriction = 0.005f;

        GameObject g0 = new GameObject(demoTag + " Pillow");

        //DeformBodyVolumetric pillow = g0.AddComponent<DeformBodyVolumetric>();
        //pillow.selectedPath = 3;
        //pillow.SetPath("/big_pillow_21kt.mesh");
        ////pillow.SetMaterial(m2);
        //pillow.distanceStiffness = 0.09f;
        //pillow.bendingStiffness = 0.009f;
        //pillow.volumeStiffness = .1f;

        //pillow.SetAllKFrictionVertices(0.25f);
        
        g0.transform.rotation = Quaternion.Euler(11.2f, 0, 0);
        g0.transform.position = new Vector3(0, 5f, 0);
        g0.AddComponent<Wireframe>();

        //Pillow case
        uint res = 60;
        Vector2 size = new Vector2(12, 8);

        GameObject g1 = new GameObject(demoTag + " Pillow Case Front");
        DeformPatchCreator patchCreator1 = g1.AddComponent<DeformPatchCreator>();
        patchCreator1.resolution = res;
        patchCreator1.size = size;
        patchCreator1.Create();

        DeformBody pillowCaseFront = g1.AddComponent<DeformBody>();

        //pillowCaseFront.SetMaterial(m3);
        pillowCaseFront.distanceStiffness = 0.8f;
        pillowCaseFront.bendingStiffness = 0.0012f;
        pillowCaseFront.SetAllKFrictionVertices(0.25f);

        g1.transform.rotation = Quaternion.Euler(90, 0, 0);
        g1.transform.position = new Vector3(0, 5f, 1.4f);
        g1.AddComponent<Wireframe>();

        GameObject g2 = new GameObject(demoTag + " Pillow Case Back");
        DeformPatchCreator patchCreator2 = g2.AddComponent<DeformPatchCreator>();
        patchCreator2.resolution = res;
        patchCreator2.size = size;
        patchCreator2.Create();

        DeformBody pillowCaseBack = g2.AddComponent<DeformBody>();

        //pillowCaseBack.SetMaterial(m3);
        pillowCaseBack.distanceStiffness = 0.8f;
        pillowCaseBack.bendingStiffness = 0.0012f;
        pillowCaseBack.SetAllKFrictionVertices(0.25f);

        g2.transform.rotation = Quaternion.Euler(90, 0, 0);
        g2.transform.position = new Vector3(0, 5f, -1.4f);
        g2.AddComponent<Wireframe>();

        GameObject g3 = new GameObject(demoTag + "Pillow Case Seam");
        DeformSeam seam = g3.AddComponent<DeformSeam>();
        seam.bodyA = pillowCaseFront;
        seam.bodyB = pillowCaseBack;
        
        seam.bodyASeamBegin = 1;
        seam.bodyASeamEnd = 0;

        seam.bodyBSeamBegin = 1;
        seam.bodyBSeamEnd = 0;

        seam.Create();

        //Plane collider
        GameObject g4 = new GameObject(demoTag + " Plane Collider");
        DeformColliderPlane planeCollider = g4.AddComponent<DeformColliderPlane>();
        planeCollider.kineticFriction = 0.01f;
        planeCollider.staticFriction = 0.02f;
        
        planeCollider.transform.position = new Vector3(0, 0.01f, 0);
    }
}