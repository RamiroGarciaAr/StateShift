using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GroundChecker))] // <-- AÑADIDO
public class PlayerSlide : MonoBehaviour
{
    [Header("Slide Settings")]
    [SerializeField] private float slideImpulse = 15f; 
    [SerializeField] private float slideDuration = 1f;
    [SerializeField] private float slideDrag = 0.1f; 
    [SerializeField] private float slopeSlideForce = 20f; 
    [SerializeField] private float minSlideSpeed = 2f;
    [SerializeField] private float slideSpeedThreshold = 7f; 

    private Rigidbody _rb;
    private GroundChecker _groundChecker; 
    private bool _isSliding = false;
    private float _slideTimer = 0f;
    private Vector3 _slideDirection = Vector3.zero;

    public bool IsSliding => _isSliding;
    public bool CanSlide { get; private set; } = true;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _groundChecker = GetComponent<GroundChecker>();
    }

    private void FixedUpdate()
    {
        if (_isSliding)
        {
            UpdateSlide();
        }
    }

    public bool TryStartSlide()
    {
        Vector3 horizontalVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        
        if (!_isSliding && CanSlide && horizontalVelocity.magnitude >= slideSpeedThreshold)
        {
            StartSlide();
            return true;
        }
        
        return false;
    }

    private void StartSlide()
    {
        _isSliding = true;
        _slideTimer = 0f;
        Vector3 horizontalVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        _slideDirection = horizontalVelocity.normalized;

        // Aplicar impulso inicial
        Vector3 slideVelocity = _slideDirection * slideImpulse;
        _rb.velocity = new Vector3(slideVelocity.x, _rb.velocity.y, slideVelocity.z);
        
        // Usar Drag para la fricción
        _rb.drag = slideDrag; 
    }

    private void UpdateSlide()
    {
        _slideTimer += Time.fixedDeltaTime;

        if (_groundChecker.IsOnWalkableSlope)
        {
            _rb.AddForce(_groundChecker.SlopeDir * slopeSlideForce, ForceMode.Acceleration);
        }
        Vector3 horizontalVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        if (_slideTimer >= slideDuration || horizontalVelocity.magnitude <= minSlideSpeed)
        {
            EndSlide();
        }
    }

    public void EndSlide()
    {
        if (!_isSliding) return;

        _isSliding = false;
        _slideTimer = 0f;
        _rb.drag = 0f; 
    }

    public void CancelSlide()
    {
        if (_isSliding)
        {
            EndSlide();
        }
    }
}