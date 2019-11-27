using UnityEngine;
using DeformDynamics;

[AddComponentMenu("Deform Dynamics/Deform Collider/Deform Collider Capsule")]
public class DeformColliderCapsule : DeformCollider
{
    /**
     * Determines how the capsule is defined. CapsuleType.Length uses the position and rotation of the Transform, 
     * together with length of the cylinder and the radii of the two half-spheres. CapsuleType.Transforms instead 
     * uses two input Transforms for determining the locations of the half-spheres, together with their radii.
     **/
    public enum CapsuleType { Length, Transforms };

    /**
     * The CapsuleType of this collider.
     **/ 
    [Header("Capsule Properties")]
    public CapsuleType capsuleType;

    /**
     * Determines the location of the first half-sphere.
     **/
    public Transform transformA;

    /**
     * Determines the location of the second half-sphere.
     **/
    public Transform transformB;

    /**
     * Length of the cylinder between the two half-spheres.
     **/
    public float length = 1;

    /**
     * Radius of the first half-sphere.
     **/
    public float radiusA = 0.5f;

    /**
     * Radius of the second half-sphere.
     **/
    public float radiusB = 0.5f;
    
    private Vector3 _transformAOld;
    private Vector3 _transformBOld;

    private float _lengthOld;

    private float _radiusAOld;
    private float _radiusBOld;

    protected override void UpdateMeshes()
    {
        base.UpdateMeshes();

        length = Mathf.Max(0, length);

        if (!colliderMesh) return;

        Vector3 aToB;

        float mBias = (1 + bias);
        
        if (capsuleType == CapsuleType.Transforms && transformA && transformB)
        {
            aToB = transformA.position - transformB.position;
            Primitives.CreateCapsuleMesh(radiusA * mBias, radiusB * mBias, 
                aToB.magnitude + radiusA * mBias + radiusB * mBias, 
                colliderMesh);

            Primitives.CreateCapsuleMesh(radiusA, radiusB, 
                aToB.magnitude + radiusA + radiusB, 
                inGameColliderMesh);
        }
        else
        {
            float maxRadiusScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
            float meshRadiusA = radiusA * maxRadiusScale * mBias;
            float meshRadiusB = radiusB * maxRadiusScale * mBias;

            Primitives.CreateCapsuleMesh(meshRadiusA, meshRadiusB, 
                (length + (meshRadiusA + meshRadiusB)) * Mathf.Abs(transform.lossyScale.y), 
                colliderMesh);

            Primitives.CreateCapsuleMesh(radiusA * maxRadiusScale, radiusB * maxRadiusScale, 
                (length + (radiusA + radiusB)) * Mathf.Abs(transform.lossyScale.y), 
                inGameColliderMesh);
        }
    }

    protected override bool HasChangedShape()
    {
        bool hasChanged = false;

        if (transformA && transformB)
        {
            hasChanged = !(_transformAOld.Equals(transformA.position)) || 
                         !(_transformBOld.Equals(transformB.position));

            _transformAOld = transformA.position;
            _transformBOld = transformB.position;
        }
        
        hasChanged = hasChanged || 
                     _lengthOld != length ||
                     _radiusAOld != radiusA || 
                     _radiusBOld != radiusB || 
                     base.HasChangedShape();
        
        _lengthOld = length;
        _radiusAOld = radiusA;
        _radiusBOld = radiusB;

        return hasChanged;
    }

    protected override void UpdateTransform()
    {
        Vector3 aToB;
        Vector3 middle;
        Vector3 delta = Vector3.zero;

        if (capsuleType == CapsuleType.Transforms && transformA && transformB)
        {
            aToB = transformA.position - transformB.position;
            middle = transformA.position - aToB / 2;

            transform.up = aToB;
            transform.position = middle;
        }
    }

    public override void AddToSimulation()
    {
        if (capsuleType == CapsuleType.Length)
        {
            float mBias = (1 + bias);

            float maxRadiusScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
            float meshRadiusA = radiusA * maxRadiusScale * mBias;
            float meshRadiusB = radiusB * maxRadiusScale * mBias;

            Primitives.CreateCapsuleMesh(meshRadiusA, meshRadiusB, 
                (length + (radiusA + radiusB)) * Mathf.Abs(transform.lossyScale.y), 
                colliderMesh);

            Vector3 a_pos = transform.position + (length / 2) * transform.up;
            Vector3 b_pos = transform.position + (length / 2) * -transform.up;

            DeformPlugin.Collider.CreateCapsuleCollider(meshRadiusA, meshRadiusB, a_pos.x, a_pos.y, a_pos.z, b_pos.x, b_pos.y, b_pos.z, kineticFriction, staticFriction, out id);
        }
        else
        {
            Vector3 a_pos = transformA.position;
            Vector3 b_pos = transformB.position;

            DeformPlugin.Collider.CreateCapsuleCollider(radiusA, radiusB, a_pos.x, a_pos.y, a_pos.z, b_pos.x, b_pos.y, b_pos.z, kineticFriction, staticFriction, out id);
        }

        SaveTransform();
    }

    public override void UpdateInSimulation()
    {
        Vector3 axis;
        float delta_angle;

        Quaternion rotationDelta = Quaternion.Inverse(oldRotation) * transform.rotation;
        rotationDelta.ToAngleAxis(out delta_angle, out axis);

        delta_angle *= -Mathf.Deg2Rad; // Convert to radians

        Vector3 a_pos, b_pos;

        if (capsuleType == CapsuleType.Length)
        {
            a_pos = transform.position + (length / 2) * transform.up;
            b_pos = transform.position + (length / 2) * -transform.up;
        }
        else
        {
            a_pos = transformA.position;
            b_pos = transformB.position;
        }        

        Vector3 position = a_pos + (b_pos - a_pos) / 2;

        if (delta_angle != 0)
        {
            DeformPlugin.Collider.RotateCollider(id, delta_angle, axis.x, axis.y, axis.z, position.x, position.y, position.z);
        }

        if (HasChangedShape())
        {
            DeformPlugin.Collider.UpdateCapsuleCollider(id, radiusA, radiusB, a_pos.x, a_pos.y, a_pos.z, b_pos.x, b_pos.y, b_pos.z);
        }

        SaveTransform();
    }
}
