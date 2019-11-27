using UnityEngine;
using DeformDynamics;

[AddComponentMenu("Deform Dynamics/Deform Collider/Deform Collider Plane")]
public class DeformColliderPlane : DeformCollider
{
    protected override void UpdateMeshes()
    {
        base.UpdateMeshes();

        if (colliderMesh) Primitives.CreatePlaneMesh(10 * transform.lossyScale.z, 10 * transform.lossyScale.x, 2, 2, colliderMesh);
        if (inGameColliderMesh) Primitives.CreatePlaneMesh(10 * transform.lossyScale.z, 10 * transform.lossyScale.x, 2, 2, inGameColliderMesh);

        // APPLY BIAS HERE

        Vector3[] biasedVertices = colliderMesh.vertices;

        for (int i = 0; i < biasedVertices.Length; i++)
        {
            biasedVertices[i].y += bias;
        }

        colliderMesh.vertices = biasedVertices;
    }

    public override void AddToSimulation()
    {
        DeformPlugin.Collider.CreatePlaneCollider(transform.up.x, transform.up.y, transform.up.z, kineticFriction, staticFriction, out id);
        DeformPlugin.Collider.MoveCollider(id, transform.position.x + transform.up.x * bias, 
											   transform.position.y + transform.up.y * bias, 
											   transform.position.z + transform.up.z * bias);

        SaveTransform();
    }

    public override void UpdateInSimulation()
    {
        // Rotation
        Vector3 axis;
        float angle;

        Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(oldRotation);
        rotationDelta.ToAngleAxis(out angle, out axis);

        Vector3 rc = oldPosition;

        DeformPlugin.Collider.RotateCollider(id, -angle * Mathf.Deg2Rad, axis.x, axis.y, axis.z,
                         rc.x, rc.y, rc.z);

        //Translation
        Vector3 translationDelta = transform.position - oldPosition;

        DeformPlugin.Collider.MoveCollider(id, translationDelta.x, translationDelta.y, translationDelta.z);

        SaveTransform();
    }
}
