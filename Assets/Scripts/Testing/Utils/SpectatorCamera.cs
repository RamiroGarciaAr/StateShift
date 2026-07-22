using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Debug-only spectator (noclip) camera.
/// 
/// SETUP:
///   1. Create an empty GameObject in your test scene, name it "SpectatorCamera".
///   2. Attach a Camera component + this script to it.
///   3. Assign the playerRoot field (the root GameObject of your player) in the Inspector.
///   4. Make sure your player scene has NO Cinemachine Brain on the spectator camera — 
///      this camera drives itself via Transform, fully bypassing Cinemachine.
///   5. If your scene has a CinemachineBrain on Main Camera, tag THIS camera as something
///      other than "MainCamera" to avoid conflicts, or set the Brain's UpdateMethod to 
///      Manual (the Brain only controls its own camera, so standalone cameras are unaffected).
///
/// CONTROLS:
///   WASD       — Move horizontally (camera-relative)
///   Q / E      — Move down / up (world Y axis)
///   Mouse      — Look (pitch + yaw)
///   Shift      — Speed boost multiplier
///   Scroll     — Adjust move speed on the fly
///   Escape     — Toggle cursor lock
/// </summary>
[RequireComponent(typeof(Camera))]
public class SpectatorCamera : MonoBehaviour
{
    // ─── Constants ────────────────────────────────────────────────────────────

    private const float SCROLL_SPEED_STEP       = 1f;
    private const float SPEED_MIN               = 1f;
    private const float SPEED_MAX               = 100f;
    private const float PITCH_CLAMP_DEGREES     = 89f;

    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("Player Reference")]
    [Tooltip("Root GameObject of the player. Will be disabled on Awake.")]
    [SerializeField] private GameObject playerRoot;

    [Header("Movement")]
    [SerializeField] private float moveSpeed         = 10f;
    [SerializeField] private float sprintMultiplier  = 3f;
    [SerializeField] private float moveSmoothTime    = 0.08f;  // seconds to reach target velocity

    [Header("Look")]
    [SerializeField] private float mouseSensitivity  = 0.15f;  // degrees per pixel
    [SerializeField] private bool  invertY            = false;

    // ─── Private State ────────────────────────────────────────────────────────

    private float   _yaw;           // horizontal rotation (world Y)
    private float   _pitch;         // vertical rotation (local X)

    private Vector3 _currentVelocity;      // actual move velocity this frame
    private Vector3 _velocitySmoothRef;    // ref for SmoothDamp

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        DisablePlayer();
        InitRotationFromTransform();
        LockCursor();
    }

    private void Update()
    {
        HandleCursorToggle();

        // Only move/look when cursor is locked (i.e. user is in control)
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleLook();
            HandleMovement();
            HandleSpeedScroll();
        }
    }

    // ─── Initialization ───────────────────────────────────────────────────────

    /// <summary>
    /// Disable the player root so its physics, input, and cameras don't interfere.
    /// We disable rather than destroy so you can re-enable it from another editor script if needed.
    /// </summary>
    private void DisablePlayer()
    {
        if (playerRoot == null)
        {
            // Fallback: try to find by tag — not reliable if tag isn't set, but better than nothing.
            playerRoot = GameObject.FindWithTag("Player");
        }

        if (playerRoot != null)
        {
            playerRoot.SetActive(false);
            Debug.Log($"[SpectatorCamera] Disabled player: '{playerRoot.name}'");
        }
        else
        {
            Debug.LogWarning("[SpectatorCamera] No playerRoot assigned and no GameObject tagged 'Player' found. " +
                             "Assign playerRoot in the Inspector to suppress this warning.");
        }
    }

    /// <summary>
    /// Seed yaw/pitch from the transform's current rotation so the camera
    /// doesn't snap on the first frame.
    /// </summary>
    private void InitRotationFromTransform()
    {
        _yaw   = transform.eulerAngles.y;
        _pitch = transform.eulerAngles.x;

        // eulerAngles returns 0–360; remap pitch to -180–180 so clamp works correctly
        if (_pitch > 180f) _pitch -= 360f;
    }

    // ─── Look ─────────────────────────────────────────────────────────────────

    private void HandleLook()
    {
        // Use the New Input System's Mouse delta — frame-rate independent, no Input Manager dependency.
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        _yaw   += mouseDelta.x * mouseSensitivity;
        _pitch += mouseDelta.y * mouseSensitivity * (invertY ? 1f : -1f);

        // Clamp pitch so the camera can't flip upside-down
        _pitch = Mathf.Clamp(_pitch, -PITCH_CLAMP_DEGREES, PITCH_CLAMP_DEGREES);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    // ─── Movement ─────────────────────────────────────────────────────────────

    private void HandleMovement()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        // Build desired direction in camera-local space, then project to world.
        // Q/E handle vertical movement in world space (feels more natural for a fly cam).
        Vector3 inputDir = Vector3.zero;

        if (kb.wKey.isPressed) inputDir += transform.forward;
        if (kb.sKey.isPressed) inputDir -= transform.forward;
        if (kb.dKey.isPressed) inputDir += transform.right;
        if (kb.aKey.isPressed) inputDir -= transform.right;
        if (kb.eKey.isPressed) inputDir += Vector3.up;
        if (kb.qKey.isPressed) inputDir -= Vector3.up;

        // Normalize so diagonal movement isn't faster, then scale by speed
        float speed     = moveSpeed * (kb.leftShiftKey.isPressed ? sprintMultiplier : 1f);
        Vector3 targetVelocity = inputDir.normalized * speed;

        // Smooth the velocity to avoid snappy start/stop — helps when observing physics
        _currentVelocity = Vector3.SmoothDamp(
            _currentVelocity,
            targetVelocity,
            ref _velocitySmoothRef,
            moveSmoothTime
        );

        transform.position += _currentVelocity * Time.deltaTime;
    }

    // ─── Speed Scroll ─────────────────────────────────────────────────────────

    private void HandleSpeedScroll()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll == 0f) return;

        // Scroll adjusts base move speed; clamped to sane range
        moveSpeed = Mathf.Clamp(
            moveSpeed + Mathf.Sign(scroll) * SCROLL_SPEED_STEP,
            SPEED_MIN,
            SPEED_MAX
        );
    }

    // ─── Cursor ───────────────────────────────────────────────────────────────

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    private static void HandleCursorToggle()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            bool isLocked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = isLocked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible   = isLocked;
        }
    }

    // ─── Editor Gizmo ─────────────────────────────────────────────────────────

#if UNITY_EDITOR
    /// <summary>
    /// Draws a visual indicator in Scene view so you can spot the spectator cam quickly.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
    }
#endif
}