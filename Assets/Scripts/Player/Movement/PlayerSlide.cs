using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GroundChecker))] // <-- AÑADIDO
public class PlayerSlide : MonoBehaviour
{
    [Header("Slide Settings")]
    [SerializeField] private float slideImpulse = 15f; // Renombrado (antes 'slideForce')
    [SerializeField] private float slideDuration = 1f;
    [SerializeField] private float slideDrag = 0.1f; // Renombrado (antes 'slideDecelerationRate')
    [SerializeField] private float slopeSlideForce = 20f; // ¡NUEVO! Fuerza para deslizar en pendientes
    [SerializeField] private float minSlideSpeed = 2f;
    [SerializeField] private float slideSpeedThreshold = 7f; 

    private Rigidbody _rb;
    private GroundChecker _groundChecker; // <-- AÑADIDO
    private bool _isSliding = false;
    private float _slideTimer = 0f;
    private Vector3 _slideDirection = Vector3.zero;

    public bool IsSliding => _isSliding;
    public bool CanSlide { get; private set; } = true;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _groundChecker = GetComponent<GroundChecker>(); // <-- AÑADIDO
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

        // ¡NUEVO! Aplicar fuerza de gravedad de la pendiente
        if (_groundChecker.IsOnWalkableSlope)
        {
            // SlopeDir es el vector que apunta HACIA ABAJO de la pendiente
            _rb.AddForce(_groundChecker.SlopeDir * slopeSlideForce, ForceMode.Acceleration);
        }

        // Comprobar si el slide debe terminar
        Vector3 horizontalVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        if (_slideTimer >= slideDuration || horizontalVelocity.magnitude <= minSlideSpeed)
        {
            EndSlide();
        }
    }

    public void EndSlide()
    {
        if (!_isSliding) return; // Evitar llamadas múltiples

        _isSliding = false;
        _slideTimer = 0f;
        
        // Restaurar el drag a 0 para que el freno de PlayerMovement actúe si es necesario
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