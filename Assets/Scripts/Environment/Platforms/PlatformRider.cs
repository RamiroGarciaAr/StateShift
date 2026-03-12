using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlatformRider : MonoBehaviour, IPlatformRider
{
    [Header("Settings")]
    [SerializeField] private bool maintainRelativePosition = true;
    [SerializeField] private bool applyRotation = true;
    [SerializeField] private float stickiness = 1f; 
    
    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    private Rigidbody rb;
    private Transform currentPlatform;
    private Vector3 relativePosition;
    private Quaternion relativeRotation;
    private bool isOnPlatform;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnPlatformEnter(Transform platform)
    {
        currentPlatform = platform;
        isOnPlatform = true;
        
        if (maintainRelativePosition)
        {
            relativePosition = platform.InverseTransformPoint(transform.position);
        }
        
        if (applyRotation)
        {
            relativeRotation = Quaternion.Inverse(platform.rotation) * transform.rotation;
        }
        
        if (showDebug)
            Debug.Log($"{name} entered platform {platform.name}");
    }

    public void OnPlatformExit(Transform platform)
    {
        if (currentPlatform == platform)
        {
            currentPlatform = null;
            isOnPlatform = false;
            
            if (showDebug)
                Debug.Log($"{name} exited platform {platform.name}");
        }
    }

    public void OnPlatformMove(Vector3 deltaPosition, Quaternion deltaRotation)
    {
        if (!isOnPlatform || currentPlatform == null) return;

        Vector3 targetPosition;
        
        if (maintainRelativePosition)
        {
            // Mantener posición relativa a la plataforma
            targetPosition = currentPlatform.TransformPoint(relativePosition);
        }
        else
        {
            // Solo aplicar el delta de movimiento
            targetPosition = transform.position + deltaPosition;
        }
        
        // Aplicar movimiento con stickiness
        Vector3 positionDelta = targetPosition - transform.position;
        rb.MovePosition(transform.position + positionDelta * stickiness);
        
        // Aplicar rotación si está habilitada
        if (applyRotation && deltaRotation != Quaternion.identity)
        {
            if (maintainRelativePosition)
            {
                // Mantener rotación relativa
                Quaternion targetRotation = currentPlatform.rotation * relativeRotation;
                rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, stickiness));
            }
            else
            {
                // Solo aplicar delta de rotación
                rb.MoveRotation(transform.rotation * deltaRotation);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebug || !isOnPlatform || currentPlatform == null) return;
        
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, currentPlatform.position);
        
        if (maintainRelativePosition)
        {
            Vector3 worldRelativePos = currentPlatform.TransformPoint(relativePosition);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(worldRelativePos, 0.1f);
        }
    }
}