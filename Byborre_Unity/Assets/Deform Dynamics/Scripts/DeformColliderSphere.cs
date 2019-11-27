using System;
using System.Runtime.InteropServices;
using UnityEngine;
using DeformDynamics;

[AddComponentMenu("Deform Dynamics/Deform Collider/Deform Collider Sphere")]
public class DeformColliderSphere : DeformCollider
{
    /**
     * The radius of the sphere of this collider.
     **/
    public float radius = 0.5f;

    protected override void UpdateMeshes()
    {
        base.UpdateMeshes();

        float maxScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z)));

        if (colliderMesh) Primitives.CreateSphereMesh(radius * maxScale * (1 + bias), colliderMesh);
        if (inGameColliderMesh) Primitives.CreateSphereMesh(radius * maxScale, inGameColliderMesh);
    }

    protected override void UpdateTransform()
    {
        //transform.localScale = Vector3.one * radius;
    }

    public override void AddToSimulation()
    {
        float maxScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z)));

        DeformPlugin.Collider.CreateSphereCollider(radius * maxScale * (1 + bias), kineticFriction, staticFriction, out id);
        
        transform.rotation.ToAngleAxis(out float angle, out Vector3 axis);

        Vector3 rc = transform.position;

        DeformPlugin.Collider.RotateCollider(id, -angle * Mathf.Deg2Rad, axis.x, axis.y, axis.z, rc.x, rc.y, rc.z);        
        DeformPlugin.Collider.MoveCollider(id, rc.x, rc.y, rc.z);

        SaveTransform();   
    }

    public override void UpdateInSimulation()
    {
        // Rotation
        Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(oldRotation);
        rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
        
        DeformPlugin.Collider.RotateCollider(id, -angle * Mathf.Deg2Rad, axis.x, axis.y, axis.z,
                                                 transform.position.x, transform.position.y, transform.position.z);

        //Translation
        Vector3 translationDelta = transform.position - oldPosition;

        DeformPlugin.Collider.MoveCollider(id, translationDelta.x, translationDelta.y, translationDelta.z);

        SaveTransform();
    }
}
