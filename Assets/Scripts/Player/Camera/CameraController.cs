using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public bool invertY = false;
    
    [Header("Camera Limits")]
    public float maxLookAngle = 80f;
    public float minLookAngle = -80f;
    
    [Header("References")]
    public CinemachineVirtualCamera virtualCamera;
    
    // Private variables
    private CinemachinePOV povComponent;
    
    // Input variables
    private float mouseX;
    private float mouseY;

    void Start()
    {
        // Get the POV component from Cinemachine Virtual Camera
        if (virtualCamera != null)
        {
            povComponent = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
            if (povComponent != null)
            {
                // Configure POV settings for responsive camera
                povComponent.m_HorizontalAxis.m_MaxSpeed = mouseSensitivity * 10f;
                povComponent.m_VerticalAxis.m_MaxSpeed = mouseSensitivity * 10f;
                povComponent.m_VerticalAxis.m_MinValue = minLookAngle;
                povComponent.m_VerticalAxis.m_MaxValue = maxLookAngle;
                
                // Remove smoothing for instant response
                povComponent.m_HorizontalAxis.m_AccelTime = 0f;
                povComponent.m_HorizontalAxis.m_DecelTime = 0f;
                povComponent.m_VerticalAxis.m_AccelTime = 0f;
                povComponent.m_VerticalAxis.m_DecelTime = 0f;
                
                if (invertY)
                {
                    povComponent.m_VerticalAxis.m_InvertInput = true;
                }
            }
            
            // Configure body for immediate follow (no lag)
            var body = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (body != null)
            {
                // Remove all damping for instant camera follow
                body.Damping.x = 0f;
                body.Damping.y = 0f;
                body.Damping.z = 0f;
            }
        }
        
        // Lock cursor for FPS experience
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseInput();
        
        // Toggle cursor lock with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }
    
    void HandleMouseInput()
    {
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
    }
    
    void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    public void SetMouseSensitivity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
        if (povComponent != null)
        {
            povComponent.m_HorizontalAxis.m_MaxSpeed = mouseSensitivity * 10f;
            povComponent.m_VerticalAxis.m_MaxSpeed = mouseSensitivity * 10f;
        }
    }
    
    public void SetInvertY(bool invert)
    {
        invertY = invert;
        if (povComponent != null)
        {
            povComponent.m_VerticalAxis.m_InvertInput = invert;
        }
    }
    
    public void SetLookLimits(float minAngle, float maxAngle)
    {
        minLookAngle = minAngle;
        maxLookAngle = maxAngle;
        if (povComponent != null)
        {
            povComponent.m_VerticalAxis.m_MinValue = minLookAngle;
            povComponent.m_VerticalAxis.m_MaxValue = maxLookAngle;
        }
    }
}