using System.Collections.Generic;
using UnityEngine;

public class DemoAnchorToCapsuleRuntime : Demo {
    
    private float omega = 0.016f;

    private readonly Stack<DeformBody> _createdObjects = new Stack<DeformBody>();

    private Material[] materials;
    
    public override void Load()
    {
        base.Load();

        demoTag = "DemoAnchorToCapsuleRuntime";

        // Manager
        var g0 = new GameObject(demoTag + " Manager");
        var manager = g0.AddComponent<DeformManager>();
        manager.selfCollisions = true;
        manager.ignoreIntersectingParticles = true;

        // Capsule collider
        GameObject g1 = new GameObject(demoTag + " Capsule Collider");
        DeformColliderCapsule collider1 = g1.AddComponent<DeformColliderCapsule>();
        collider1.capsuleType = DeformColliderCapsule.CapsuleType.Length;

        collider1.radiusA = 0.25f;
        collider1.radiusB = 0.25f;

        collider1.length = 6;

        collider1.inGameMaterial = m2;
        collider1.bias = 0.05f;
        
        // Skirt
        GameObject g2 = new GameObject(demoTag + " Skirt");
        DeformBody skirt = g2.AddComponent<DeformBody>();

        GameObject meshObject = Resources.Load<GameObject>("Meshes/custom_skirt2");
        Mesh meshToSpawn = meshObject.GetComponentInChildren<MeshFilter>().sharedMesh;

        //skirt.SetMesh(meshToSpawn);
        //skirt.SetMaterial(m0);
        skirt.bendingStiffness = 0.00012f;

        g2.transform.position = new Vector3(0, 0.5f, 0);

        AnchorToCollider atc = g2.AddComponent<AnchorToCollider>();
        atc.anchorCollider = collider1;

        atc.anchorAtStart = true;
    }

    private void Start()
    {
        GameObject c = GameObject.Find(demoTag + " Skirt");

        _createdObjects.Push(c.GetComponent<DeformBody>());

        materials = new Material[4];

        materials[0] = m0;
        materials[1] = m1;
        materials[2] = m2;
        materials[3] = m3;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject c = GameObject.Find(demoTag + " Capsule Collider");

        c.transform.RotateAround(c.transform.position, Vector3.up, Mathf.Sin(omega * 0.5f) * 7.16f); //0.125 radians ~= 7.16 degrees

        omega += 0.016f;

        // Runtime add/removal of skirt
        if (Input.GetKeyDown(KeyCode.A))
        {
            var g = new GameObject();

            DeformBody body;

            GameObject meshObject = Resources.Load<GameObject>("Meshes/custom_skirt2");
            g.transform.position = new Vector3(0, 0.5f + (_createdObjects.Count * 0.5f), 0);

            Mesh meshToSpawn = meshObject.GetComponentInChildren<MeshFilter>().sharedMesh;

            if (meshToSpawn != null) // If meshToSpawn exists, spawn it.
            {
                body = g.AddComponent<DeformBody>();
                //body.SetMesh(meshToSpawn);
            }
            else
            {
                return;
            }
            
            //body.SetMaterial(materials[_createdObjects.Count % 4]);
            body.bendingStiffness = 0.00012f;           

            _createdObjects.Push(body);

            AnchorToCollider atc = g.AddComponent<AnchorToCollider>();
            atc.anchorCollider = c.GetComponent<DeformColliderCapsule>();
            atc.anchorAtStart = true;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (_createdObjects.Count <= 0) return;

            var toDelete = _createdObjects.Pop();
            
            //toDelete.disableRendering = true;
            toDelete.GetComponent<MeshRenderer>().enabled = false;
            Destroy(toDelete.gameObject);
        }
    }
}