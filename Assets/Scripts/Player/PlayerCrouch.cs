using System;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class PlayerCrouch : MonoBehaviour
{
    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeightMultiplier = 0.5f;
    [SerializeField] private float crouchTransitionSpeed = 10f;
    [SerializeField] private Transform cameraTarget;
    private CapsuleCollider _collider;
    [SerializeField] private float _originalHeight;
    [SerializeField] private Vector3 _originalCenter;
    private Vector3 _originalCameraLocalPosition;
    private float _targetHeight;
    private Vector3 _targetCenter;
    private bool _isCrouching;
    public bool IsCrouching => _isCrouching;
    private void Awake()
    {
        _collider = GetComponent<CapsuleCollider>();
    }
    private void Start()
    {
        _targetHeight = _originalHeight;
        _targetCenter = _originalCenter;

        if (cameraTarget == null)
        {
            Debug.LogWarning("Camera target not assigned in PlayerCrouch script.");
        }
        else
        {
            _originalCameraLocalPosition = cameraTarget.localPosition;
        }


    }

    private void FixedUpdate()
    {
        UpdateColliderSize();
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        if (cameraTarget == null) return;

        // Calcular cuánto bajó el collider desde su posición original
        float heightDifference = _originalHeight - _collider.height;
        float centerOffset = (_originalCenter.y - _collider.center.y);

        // La cámara baja la misma cantidad que bajó la parte superior del collider
        Vector3 targetCameraPosition = _originalCameraLocalPosition - new Vector3(0, centerOffset, 0);

        cameraTarget.localPosition = Vector3.Lerp(
            cameraTarget.localPosition,
            targetCameraPosition,
            Time.fixedDeltaTime * crouchTransitionSpeed
        );
    }

    public void SetCrouching(bool crouch)
    {
        if (crouch)
        {
            _isCrouching = true;
            SetTargetCrouchSize();
        }
        else
        {
            if (CanStandUp())
            {
                _isCrouching = false;
                SetTargetStandSize();
            }
        }
    }



    private void SetTargetCrouchSize()
    {
        _targetHeight = _originalHeight * crouchHeightMultiplier;
        float heightDifference = _originalHeight - _targetHeight;
        _targetCenter = _originalCenter - new Vector3(0, heightDifference * 0.5f, 0);
    }

    private void SetTargetStandSize()
    {
        _targetHeight = _originalHeight;
        _targetCenter = _originalCenter;
    }

    public bool CanStandUp()
    {
        if (Mathf.Approximately(_collider.height, _originalHeight))
            return true;

        float heightDifference = _originalHeight - _collider.height;
        Vector3 rayOrigin = transform.position + Vector3.up * (_collider.height * 0.5f);

        bool hasObstacle = Physics.SphereCast(
            rayOrigin,
            _collider.radius * 0.9f,
            Vector3.up,
            out _,
            heightDifference
        );

        return !hasObstacle;
    }

    private void UpdateColliderSize()
    {
        _collider.height = Mathf.Lerp(
            _collider.height,
            _targetHeight,
            Time.fixedDeltaTime * crouchTransitionSpeed
        );

        _collider.center = Vector3.Lerp(
            _collider.center,
            _targetCenter,
            Time.fixedDeltaTime * crouchTransitionSpeed
        );
    }
}