using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AxisRotator : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    [Header("Rotation Settings")]
    [SerializeField] private Axis rotationAxis = Axis.Y;
    [SerializeField] private float rotationSpeed = 45f; 
    private Rigidbody _rb;
    private Quaternion _prevRotation;
    public Vector3 AngularVelocityWorld { get; private set; } 

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _prevRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        Vector3 axisVec = Vector3.zero;
        switch (rotationAxis)
        {
            case Axis.X: axisVec = Vector3.right; break;
            case Axis.Y: axisVec = Vector3.up; break;
            case Axis.Z: axisVec = Vector3.forward; break;
        }

        float angleThisStep = rotationSpeed * Time.fixedDeltaTime; 
        Quaternion deltaRot = Quaternion.AngleAxis(angleThisStep, axisVec);
        Quaternion newRot = _rb.rotation * deltaRot;
        _rb.MoveRotation(newRot);

        // calcular la velocidad angular en el mundo
        Quaternion delta = newRot * Quaternion.Inverse(_prevRotation);
        float angle;
        Vector3 axis;
        delta.ToAngleAxis(out angle, out axis);
        // pasar grados->radianes
        angle *= Mathf.Deg2Rad;
        AngularVelocityWorld = axis * (angle / Time.fixedDeltaTime);

        _prevRotation = newRot;
    }
}
