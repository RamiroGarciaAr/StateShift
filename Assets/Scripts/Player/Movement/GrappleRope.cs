using UnityEngine;
using Core.Utils; // Assuming your Spring class is here

public class GrappleRope : MonoBehaviour
{
    [Header("Grapple Rope Settings")]
    [SerializeField] private float ropeSpeed = 15.0f;
    [SerializeField] private int quality = 40;
    [SerializeField] AnimationCurve animationCurve;
    
    [Header("Spring Settings")]
    [SerializeField] private float damper = 4.5f;
    [SerializeField] private float strength = 60.0f;
    [SerializeField] private float initialVelocity = 8.0f;
    [SerializeField] private float waveCount = 3.5f;
    [SerializeField] private float waveHeight = 1.5f;
    
    [Header("Travel Animation")]
    [SerializeField] private float ropeTravelSpeed = 25.0f; // Speed rope travels to target
    [SerializeField] private float impactShake = 0.3f; // Subtle shake on impact
    
    private Vector3 _currentGrapplePosition;
    private PlayerGrapple _playerGrapple;
    private Spring _spring;
    private LineRenderer _lineRenderer;
    private float _ropeProgress; // 0 to 1, how far rope has traveled
    private float _shakeAmount;
    private bool _ropeHasConnected;

    private void Awake()
    {
        _playerGrapple = GetComponent<PlayerGrapple>();
        if (_playerGrapple == null) Debug.LogError("[Grapple Rope] PlayerGrapple could not be found.");
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer == null) Debug.LogError("[Grapple Rope] LineRenderer could not be found.");
        
        _spring = new Spring();
        _spring.Strength = strength;
        _spring.Damper = damper;
        _spring.Target = 0f;
        _spring.Value = 0f;
    }

    void LateUpdate()
    {
        DrawRope();
    }

    void DrawRope()
    {
        if (_playerGrapple.IsGrappling == false)
        {
            if (_lineRenderer.positionCount > 0)
            {
                _lineRenderer.positionCount = 0;
            }
            _spring.Reset();
            _currentGrapplePosition = _playerGrapple.transform.position;
            _ropeProgress = 0f;
            _shakeAmount = 0f;
            _ropeHasConnected = false;
            return;
        }

        if (_lineRenderer.positionCount == 0)
        {
            // Initialize rope travel
            _spring.Value = 0f;
            _spring.Velocity = 0f;
            _spring.Target = 0f;
            _lineRenderer.positionCount = quality + 1;
            _currentGrapplePosition = _playerGrapple.transform.position;
            _ropeProgress = 0f;
            _shakeAmount = 0f;
            _ropeHasConnected = false;
        }

        var grapplePosition = _playerGrapple.GrapplePoint;
        var grappleStartPosition = _playerGrapple.transform.position;
        var distanceToTarget = Vector3.Distance(grappleStartPosition, grapplePosition);

        // Animate rope traveling to target
        if (_ropeProgress < 1f)
        {
            _ropeProgress += (ropeTravelSpeed / distanceToTarget) * Time.deltaTime;
            _ropeProgress = Mathf.Clamp01(_ropeProgress);
            
            // Trigger spring animation when rope connects
            if (_ropeProgress >= 1f && !_ropeHasConnected)
            {
                _ropeHasConnected = true;
                _spring.Value = 1.2f;
                _spring.Velocity = initialVelocity;
                _spring.Target = 0f;
                _shakeAmount = impactShake;
            }
        }

        // Update spring physics (only after connection)
        if (_ropeHasConnected)
        {
            _spring.Damper = damper;
            _spring.Strength = strength;
            _spring.Update();
            _shakeAmount = Mathf.Lerp(_shakeAmount, 0f, Time.deltaTime * 6f);
        }

        var ropeDirection = grapplePosition - grappleStartPosition;
        var up = Vector3.Cross(ropeDirection.normalized, Vector3.up);
        if (up.magnitude < 0.1f) up = Vector3.right;
        up.Normalize();

        _currentGrapplePosition = Vector3.Lerp(_currentGrapplePosition, grapplePosition, Time.deltaTime * ropeSpeed);

        for (int i = 0; i < quality + 1; i++)
        {
            var delta = i / (float)quality;
            
            // Only draw rope up to current progress
            var actualDelta = delta * _ropeProgress;
            
            Vector3 position;
            
            if (actualDelta <= _ropeProgress && _ropeProgress < 1f)
            {
                // Rope is still traveling - animate with sin wave
                var travelWave = Mathf.Sin(actualDelta * waveCount * Mathf.PI) * waveHeight * 0.5f * animationCurve.Evaluate(actualDelta);
                var travelOffset = up * travelWave;
                position = Vector3.Lerp(grappleStartPosition, grapplePosition, actualDelta) + travelOffset;
            }
            else
            {
                // Rope has connected - apply spring-based wave animation
                var wave = Mathf.Sin(delta * waveCount * Mathf.PI) * _spring.Value * animationCurve.Evaluate(delta);
                var shake = Random.insideUnitSphere * _shakeAmount * (1f - delta) * 0.2f;
                var offset = up * waveHeight * wave + shake;
                
                position = Vector3.Lerp(grappleStartPosition, _currentGrapplePosition, delta) + offset;
            }

            _lineRenderer.SetPosition(i, position);
        }
    }
}