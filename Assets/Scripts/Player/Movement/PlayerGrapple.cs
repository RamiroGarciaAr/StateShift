using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerGrapple : MonoBehaviour
{
    [Header("Grapple Settings")]
    [SerializeField] private float maxGrappleDistance = 50f;
    [SerializeField] private float grappleSpeed = 20f;
    [SerializeField] private float grappleAcceleration = 30f;
    [SerializeField] private LayerMask grappleLayerMask;
    [SerializeField] private float grappleCooldown = 1f;
    
    [Header("Grapple Behavior")]
    [SerializeField] private float minDistanceToTarget = 2f;
    [SerializeField] private float momentumGain = 0.2f;
    [Range(0, 1)]
    [SerializeField] private float velocityRetention = 0.5f;
    [SerializeField] private float grappleDelayTime = 0.3f;

    private Rigidbody _rb;
    private Camera _mainCamera;
    private bool _isGrappling = false;
    private Vector3 _grapplePoint;
    private float _cooldownTimer = 0f;
    private Vector3 _grappleDirection;

    #region Properties
    public bool IsGrappling => _isGrappling;
    public bool CanGrapple => _cooldownTimer <= 0 && !_isGrappling;
    public Vector3 GrapplePoint => _grapplePoint;
    public float CooldownProgress => 1f - (_cooldownTimer / grappleCooldown);
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (_isGrappling)
        {
            UpdateGrapple();
        }
    }

    public bool TryStartGrapple()
    {
        if (!CanGrapple) return false;

        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance, grappleLayerMask))
        {
            _grapplePoint = hit.point;
            Invoke(nameof(StartGrapple),grappleDelayTime);
            return true;
        }

        return false;
    }

    private void StartGrapple()
    {
        _isGrappling = true;
        _grappleDirection = (_grapplePoint - transform.position).normalized;

        // Retain some current velocity
        Vector3 currentVel = _rb.velocity;
        _rb.velocity = currentVel * velocityRetention;
    }

    private void UpdateGrapple()
    {
        Vector3 toTarget = _grapplePoint - transform.position;
        float distance = toTarget.magnitude;

        // Stop grapple if close enough
        if (distance <= minDistanceToTarget)
        {
            EndGrapple();
            return;
        }

        // Apply force towards grapple point
        Vector3 direction = toTarget.normalized;
        Vector3 targetVelocity = direction * grappleSpeed;
        Vector3 velocityChange = targetVelocity - _rb.velocity;
        
        _rb.AddForce(velocityChange * grappleAcceleration, ForceMode.Acceleration);
    }

    private void EndGrapple()
    {
        _isGrappling = false;
        _cooldownTimer = grappleCooldown;
    }

    public void CancelGrapple()
    {
        if (_isGrappling)
        {
            EndGrapple();
        }
    }

    public float GetMomentumGain()
    {
        return momentumGain;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !_isGrappling) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _grapplePoint);
        Gizmos.DrawSphere(_grapplePoint, 0.3f);
    }
}