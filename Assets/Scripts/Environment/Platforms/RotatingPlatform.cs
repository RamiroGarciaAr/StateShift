using UnityEngine;
using System.Collections.Generic;

public class RotatingPlatform : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float rotationSpeed = 30f; // grados por segundo
    [SerializeField] private bool useLocalSpace = true;

    [Header("Detection")]
    [SerializeField] private LayerMask riderMask = -1;
    [SerializeField] private float detectionRadius = 0.5f;
    [SerializeField] private float detectionHeight = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private HashSet<IPlatformRider> currentRiders = new HashSet<IPlatformRider>();

    void Start()
    {
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        ApplyRotation();
        
        DetectRiders();
        
        Vector3 deltaPosition = transform.position - previousPosition;
        Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(previousRotation);
        
        if (deltaPosition != Vector3.zero || deltaRotation != Quaternion.identity)
        {
            foreach (var rider in currentRiders)
            {
                rider.OnPlatformMove(deltaPosition, deltaRotation);
            }
        }
        
        previousPosition = transform.position;
        previousRotation = transform.rotation;
    }

    private void ApplyRotation()
    {
        Vector3 axis = useLocalSpace ? transform.TransformDirection(rotationAxis) : rotationAxis;
        transform.Rotate(axis, rotationSpeed * Time.fixedDeltaTime, Space.World);
    }

    private void DetectRiders()
    {
        Vector3 center = transform.position + Vector3.up * (detectionHeight * 0.5f);
        
        Collider[] colliders = Physics.OverlapCapsule(
            center - Vector3.up * (detectionHeight * 0.5f),
            center + Vector3.up * (detectionHeight * 0.5f),
            detectionRadius,
            riderMask
        );

        HashSet<IPlatformRider> detectedRiders = new HashSet<IPlatformRider>();
        
        foreach (var collider in colliders)
        {
           var rider = collider.attachedRigidbody ? collider.attachedRigidbody.GetComponent<IPlatformRider>() : collider.GetComponentInParent<IPlatformRider>();
            if (rider != null)
            {
                detectedRiders.Add(rider);
                Debug.Log($"Detected rider: {rider}");
                if (!currentRiders.Contains(rider))
                {
                    rider.OnPlatformEnter(transform);
                }
            }
        }
        
        foreach (var rider in currentRiders)
        {
            if (!detectedRiders.Contains(rider))
            {
                rider.OnPlatformExit(transform);
            }
        }
        
        currentRiders = detectedRiders;
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebug) return;
        
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + Vector3.up * (detectionHeight * 0.5f);
        
        // Dibujar cilindro de detección
        Gizmos.DrawWireSphere(center + Vector3.up * (detectionHeight * 0.5f), detectionRadius);
        Gizmos.DrawWireSphere(center - Vector3.up * (detectionHeight * 0.5f), detectionRadius);
        
        // Eje de rotación
        Gizmos.color = Color.red;
        Vector3 axis = useLocalSpace ? transform.TransformDirection(rotationAxis) : rotationAxis;
        Gizmos.DrawRay(transform.position, axis.normalized * 2f);
    }
}
