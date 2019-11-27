using UnityEngine;

public class DemoCollisionRotatingCapsule : Demo
{
    float delta = 0.04f;

    public override void Load()
    {
        base.Load();

        demoTag = "DemoCollisionRotatingCapsule";

        var g = new GameObject(demoTag + " Manager");
        g.AddComponent<DeformManager>();

        // Cloth
        uint res = 71;

        GameObject g0 = new GameObject(demoTag + " Cloth");
        DeformPatchCreator patchCreator0 = g0.AddComponent<DeformPatchCreator>();

        patchCreator0.resolution = res;
        patchCreator0.size = new Vector2(4, 4);
        patchCreator0.Create();

        DeformBody body0 = g0.AddComponent<DeformBody>();

        //body0.SetMaterial(m0);
        body0.bendingStiffness = 0.0012f;
        body0.SetAllKFrictionVertices(0.05f);

        for (int i = 0; i < res; i++)
        {
            body0.fixedVertices[i] = true;
        }

        g0.transform.position = new Vector3(0, 0, 2);
        g0.AddComponent<Wireframe>();

        // Capsule collider
        GameObject g1 = new GameObject(demoTag + " Collider");
        DeformColliderCapsule collider = g1.AddComponent<DeformColliderCapsule>();

        collider.length = 8;

        collider.radiusA = 0.25f;
        collider.radiusB = 0.5f;

        collider.bias = 0.04f;
        collider.inGameMaterial = m1;

        g1.transform.position = new Vector3(0, -2, 0);
        g1.transform.rotation = Quaternion.Euler(0, 0, -90);
    }

    // Update is called once per frame
    void Update()
    {
        GameObject g = GameObject.Find(demoTag + " Collider");

        if (g.transform.position.z >= 4 || g.transform.position.z <= -4)
        {
            delta = -delta;
        }

        g.transform.position += new Vector3(0, 0, delta);
    }
}