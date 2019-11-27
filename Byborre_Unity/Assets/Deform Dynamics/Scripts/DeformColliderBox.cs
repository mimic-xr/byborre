using UnityEngine;
using DeformDynamics;

[AddComponentMenu("Deform Dynamics/Deform Collider/Deform Collider Box")]
public class DeformColliderBox : DeformCollider {
    
    protected override void UpdateMeshes()
    {
        base.UpdateMeshes();

        if (colliderMesh) Primitives.CreateBoxMesh(colliderMesh, transform.lossyScale + Vector3.one * bias);
        if (inGameColliderMesh) Primitives.CreateBoxMesh(inGameColliderMesh, transform.lossyScale + Vector3.one * (bias / 2));
    }

    protected override void UpdateTransform()
    {
        // Do nothing, eg. allow all transform changes from editor
    }

    public override void AddToSimulation()
    {
        Vector3 size = transform.lossyScale + Vector3.one * (bias * 2);

		DeformPlugin.Collider.CreateObjectOrientedBoxCollider(size.x, size.y, size.z, kineticFriction, staticFriction, out id);

        transform.rotation.ToAngleAxis(out float angle, out Vector3 axis);

        Vector3 rc = transform.position;

        DeformPlugin.Collider.RotateCollider(id, -angle * Mathf.Deg2Rad, axis.x, axis.y, axis.z, 0, 0, 0);
        DeformPlugin.Collider.MoveCollider(id, rc.x, rc.y, rc.z);

        SaveTransform();
    }

    public override void UpdateInSimulation()
    {
        // Rotation
        Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(oldRotation);
        rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);

        DeformPlugin.Collider.RotateCollider(id, -angle * Mathf.Deg2Rad, axis.x, axis.y, axis.z, 0, 0, 0);

        //Translation
        Vector3 translationDelta = transform.position - oldPosition;

        DeformPlugin.Collider.MoveCollider(id, translationDelta.x, translationDelta.y, translationDelta.z);

        SaveTransform();
    }
}
