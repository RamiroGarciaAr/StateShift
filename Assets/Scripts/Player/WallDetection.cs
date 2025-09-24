
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;


[System.Serializable]
public struct WallHitInfo
{
    public bool hasWall;
    public Vector3 wallNormal;
    public Vector3 wallPoint;
    public float wallDistance;
    public Collider wallCollider;
    public Vector3 wallDirection;
    public static WallHitInfo None => new WallHitInfo { hasWall = false };
    public bool IsValid => hasWall && wallCollider != null;
}
[System.Serializable]
public class WallDetection : MonoBehaviour
{
    [Header("Wall Detection Settings")]
    [SerializeField] private float wallCheckDistance = 0.6f;
    [SerializeField] private float wallRunDistance = 0.8f;
    [SerializeField] private LayerMask wallMask; // Nose si es necesario
    [SerializeField] private int rayCount = 5;
    [SerializeField] private float raySpreadAngle = 30f; // grados

    [Header("Wall Validation")]
    [SerializeField] private float minWallAngle = 10f; // grados
    [SerializeField] private float minWallHeight = 1.5f; // metros
    [SerializeField] private float minSpeedForWallRun = 5f; // m/s

    [Header("Height Check Settings")]
    [SerializeField] private float wallNormalOffset = 0.1f; // Separación de la pared para evitar self-collision
    [SerializeField] private float heightCheckBuffer = 1f;  // Buffer extra para verificación de altura

    private RaycastHit[] raycastHits = new RaycastHit[1];

    public WallHitInfo DetectCurrentWall(Transform player, Vector3 wallNormal)
    {
        Vector3 rayDirection = -wallNormal; 
        Vector3 rayOrigin = player.position + Vector3.up * 1f;
        
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, 
            wallRunDistance * 1.5f, wallMask, QueryTriggerInteraction.Ignore))
        {
            if (IsValidWall(hit, rayOrigin))
            {
                return CreateWallInfo(hit);
            }
        }

        return WallHitInfo.None;
    }
    private Vector3 CalculateRayDirection(Vector3 moveDir, int index)
    {
        // Calcular ángulo para este ray
        float angleStep = raySpreadAngle / Mathf.Max(1, rayCount - 1);
        float currentAngle = -raySpreadAngle * 0.5f + (index * angleStep);

        // Rotar la dirección de movimiento
        return Quaternion.Euler(0, currentAngle, 0) * moveDir.normalized;
    }

    private bool IsValidWall(RaycastHit hit, Vector3 rayOrigin)
    {
        float dotUp = Vector3.Dot(hit.normal, Vector3.up);
        bool isPerpendicularEnough = Mathf.Abs(dotUp) < Mathf.Cos(minWallAngle * Mathf.Deg2Rad);
        bool isTallEnough = CheckWallHeight(hit,rayOrigin);
        return isPerpendicularEnough && isTallEnough;
    }
    private bool CheckWallHeight(RaycastHit hit, Vector3 rayOrigin)
    {
        Vector3 upwardStart = hit.point + hit.normal * wallNormalOffset; // pequeño offset para evitar colisiones
        float heightToCheck = Mathf.Min(minWallHeight, Vector3.Distance(rayOrigin, hit.point) + heightCheckBuffer);

        return !Physics.Raycast(upwardStart, Vector3.up, heightToCheck, wallMask, QueryTriggerInteraction.Ignore);
    }
    private WallHitInfo CreateWallInfo(RaycastHit hit)
    {
        Vector3 wallRunDirection = Vector3.Cross(hit.normal, Vector3.up).normalized;
        
        return new WallHitInfo
        {
            hasWall = true,
            wallNormal = hit.normal,
            wallPoint = hit.point,
            wallDistance = hit.distance,
            wallCollider = hit.collider,
            wallDirection = wallRunDirection
        };
    }

}
