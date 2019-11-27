using UnityEngine;
using System.Runtime.InteropServices;

public class DemoAnchorToCapsule : Demo {

	private int frame = 0;

    public override void Load()
    {
        base.Load();

        demoTag = "DemoAnchorToCapsule";

        // Manager
        var g0 = new GameObject(demoTag + " Manager");
        var manager = g0.AddComponent<DeformManager>();
        manager.airFriction = 0.005f;
        manager.selfCollisions = true;
        
        // Capsule collider
        var g1 = new GameObject(demoTag + " Capsule Collider");
        var collider1 = g1.AddComponent<DeformColliderCapsule>();

        collider1.length = 6 / 4.0f;
        collider1.transform.rotation = Quaternion.Euler(0, 0, -90);
        
        collider1.radiusA = 0.25f / 4.0f;
        collider1.radiusB = 0.25f / 4.0f;

        collider1.transform.position = new Vector3(3 / 4.0f, 0, 0);
        
        // Cloth
        uint res = 51;
        uint multiplier = 3;

        var g2 = new GameObject(demoTag + " Cloth");
        var patchCreator = g2.AddComponent<DeformPatchCreator>();

        patchCreator.size = new Vector2(4 / 4.0f, (4 * multiplier) / 4.0f);
        patchCreator.resolution = res * multiplier;
        patchCreator.Create();

        g2.AddComponent<DeformBody>();
                
        AnchorToCollider atc = g2.AddComponent<AnchorToCollider>();
        atc.anchorCollider = collider1;
        atc.anchorAtStart = true;

        g2.transform.rotation = Quaternion.Euler(90, 0, 0);
        g2.transform.position = new Vector3(3 / 4.0f, -6 / 4.0f, 0);

        g2.GetComponent<MeshRenderer>().sharedMaterial = m0;
        g2.AddComponent<Wireframe>();
    }

    // Update is called once per frame
    void Update()
    {
		frame++;

		if (frame > 100)
		{
			GameObject g = GameObject.Find(demoTag + " Capsule Collider");
			DeformCollider c = g.GetComponent<DeformCollider>();

			c.transform.RotateAround(Vector3.zero, Vector3.up, Time.deltaTime * -200);
		}  
    }
}