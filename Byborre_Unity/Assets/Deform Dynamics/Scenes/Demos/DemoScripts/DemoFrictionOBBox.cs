using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoFrictionOBBox : Demo {

	public override void Load()
    {
        base.Load();

        demoTag = "DemoFrictionOBB";

        uint res = 51;

        var g = new GameObject(demoTag + " Manager");
        g.AddComponent<DeformManager>();

        for (int i = -1; i <= 1; i += 1)
        {
            GameObject patch = new GameObject(demoTag + " Patch " + i);
            DeformPatchCreator dpc = patch.AddComponent<DeformPatchCreator>();

            dpc.size = new Vector2(3.7f, 3.7f);
            dpc.resolution = res;
            dpc.Create();

            DeformBody body0 = patch.AddComponent<DeformBody>();

            //body0.SetMaterial(m0);
            body0.bendingStiffness = 0.012f;

            patch.transform.position = new Vector3(i * 4, 1, -0.6f);
            patch.AddComponent<Wireframe>();
        }

        float[] mu_k = { 0.1f, 0.25f, 1.0f };

        for (int i = -1; i <= 1; i += 1)
        {
            GameObject g3 = new GameObject(demoTag + " Box Collider" + i);
            DeformColliderBox collider0 = g3.AddComponent<DeformColliderBox>();
            collider0.kineticFriction = mu_k[i + 1];

            collider0.bias = 0.04f;
            collider0.inGameMaterial = m1;

            g3.transform.localScale = new Vector3(3, 0.25f, 3f);
            g3.transform.position = new Vector3(4 * i, 0, 0);
            g3.transform.rotation = Quaternion.Euler(-10, 0, 0);
        }
    }
}
