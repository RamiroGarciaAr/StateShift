using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerSlide : MonoBehaviour
{
    [Header("Slide Settings")]
    [SerializeField] private float slideForce = 15f;
    [SerializeField] private float slideDuration = 1f;
    [SerializeField] private float slideDecelerationRate = 5f;
    [SerializeField] private float minSlideSpeed = 2f;
    [SerializeField] private float slideSpeedThreshold = 7f; // Velocidad mínima para poder hacer slide

    private Rigidbody _rb;
    private bool _isSliding = false;
    private float _slideTimer = 0f;
    private Vector3 _slideDirection = Vector3.zero;

    public bool IsSliding => _isSliding;
    public bool CanSlide { get; private set; } = true;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
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

        Vector3 slideVelocity = _slideDirection * slideForce;
        _rb.velocity = new Vector3(slideVelocity.x, _rb.velocity.y, slideVelocity.z);
    }

    private void UpdateSlide()
    {
        _slideTimer += Time.fixedDeltaTime;

        Vector3 currentVelocity = _rb.velocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        Vector3 deceleratedVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            _slideDirection * minSlideSpeed,
            slideDecelerationRate * Time.fixedDeltaTime
        );

        _rb.velocity = new Vector3(deceleratedVelocity.x, currentVelocity.y, deceleratedVelocity.z);
        if (_slideTimer >= slideDuration || deceleratedVelocity.magnitude <= minSlideSpeed)
        {
            EndSlide();
        }
    }

    public void EndSlide()
    {
        _isSliding = false;
        _slideTimer = 0f;
    }
    public void CancelSlide()
    {
        if (_isSliding)
        {
            EndSlide();
        }
    }
}
