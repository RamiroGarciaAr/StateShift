using Entities.Controllers;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField]
    private Transform cameraHolder;

    [SerializeField]
    private float mouseSensitivityX = 0.15f;

    [SerializeField]
    private float mouseSensitivityY = 0.15f;

    [SerializeField]
    private float maxPitch = 85f,
        minPitch = -85f;

    private float _pitch;

    private void OnEnable()
    {
        PlayerInput.OnLook += HandleLook;
    }

    private void OnDisable()
    {
        PlayerInput.OnLook -= HandleLook;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void HandleLook(Vector2 lookInput)
    {
        float mouseX = lookInput.x * mouseSensitivityX;
        float mouseY = lookInput.y * mouseSensitivityY;

        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        cameraHolder.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX, Space.World);
    }
}
