using UnityEngine;

public class DemoSurfaceVertexCollision : Demo {

    float x_size = 8;
	float y_size = 4;

	float x_size_2 = 4;
	float y_size_2 = 8;

	uint low_res = 40;
	uint hi_res = 80;

	int id_object_2;
	float speed = 0.05f;
	int counter;

    DeformBody movingBody;
    DeformColliderSphere movingSphere0, movingSphere1;

	public override void Load()
    {
        base.Load();

        demoTag = "DemoSurfaceVertexCollision";

        var g = new GameObject(demoTag + " Manager");
        var manager = g.AddComponent<DeformManager>();
        manager.selfCollisions = true;
        manager.sampleSurface = true;
        manager.targetFrameRate = -1;

        GameObject g0 = new GameObject(demoTag + " Cloth");
        DeformPatchCreator patchCreator0 = g0.AddComponent<DeformPatchCreator>();

        patchCreator0.resolution = low_res;
        patchCreator0.size = new Vector2(x_size, y_size);
        patchCreator0.Create();

        DeformBody body0 = g0.AddComponent<DeformBody>();

        //body0.SetMaterial(m0);
        body0.fixedVertices[0] = true;
        body0.fixedVertices[low_res - 1] = true;

        g0.transform.position = new Vector3(0, 2.0f, 2.0f);
        g0.AddComponent<Wireframe>();

        GameObject g1 = new GameObject(demoTag + " Cloth");
        DeformPatchCreator patchCreator1 = g1.AddComponent<DeformPatchCreator>();

        patchCreator1.resolution = hi_res;
        patchCreator1.size = new Vector2(x_size_2, y_size_2);
        patchCreator1.Create();

        movingBody = g1.AddComponent<DeformBody>();

        //movingBody.SetMaterial(m1);
        movingBody.fixedVertices[0] = true;
        movingBody.fixedVertices[39] = true;

        g1.transform.position = new Vector3(0, 3, -1.5f);
        g1.AddComponent<Wireframe>();

        GameObject g2 = new GameObject(demoTag + " Collider 1");

        movingSphere0 = g2.AddComponent<DeformColliderSphere>();
        movingSphere0.radius = 0.05f;
        
        g2.transform.position = new Vector3(-2, 3, -5.5f);

        GameObject g3 = new GameObject(demoTag + " Collider 2");

        movingSphere1 = g3.AddComponent<DeformColliderSphere>();
        movingSphere1.radius = 0.05f;

        g3.transform.position = new Vector3(2, 3, -5.5f);

        AnchorToCollider atc0 = g1.AddComponent<AnchorToCollider>();

        atc0.anchorCollider = movingSphere0;
        atc0.anchorAtStart = true;

        AnchorToCollider atc1 = g1.AddComponent<AnchorToCollider>();

        atc1.anchorCollider = movingSphere1;
        atc1.anchorAtStart = true;
    }
	
	// Update is called once per frame
	void Update ()
    {
        foreach (DeformColliderSphere sphereCollider in FindObjectsOfType<DeformColliderSphere>())
        {
            sphereCollider.transform.position += Vector3.forward * speed;
        }

        if (++counter % 250 == 0)
        {
            speed *= -1;
        }
    }
}
