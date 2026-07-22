using UnityEngine;

/// <summary>
/// Rolls a dedicated pivot transform toward the wall during a wall run and drives an
/// additive FOV kick through <see cref="DynamicFOV"/>. This component is the single writer
/// of its own transform's local rotation, keeping roll decoupled from the camera holder.
/// </summary>
public class CameraWallRunTilt : MonoBehaviour
{
    [Header("Tilt")]
    [SerializeField]
    [Tooltip("Maximum roll angle (degrees) applied toward the wall while wall running.")]
    private float _maxTiltAngle = 12f;

    [SerializeField]
    [Tooltip("Lerp speed used to blend the roll and FOV kick in and out.")]
    private float _tiltSpeed = 8f;

    [Header("FOV Kick")]
    [SerializeField]
    [Tooltip("Additive field of view boost (degrees) applied while wall running.")]
    private float _fovBoost = 10f;

    [SerializeField]
    [Tooltip(
        "DynamicFOV that receives the wall-run FOV kick. Assign the world-view camera's "
            + "DynamicFOV. Falls back to the first DynamicFOV found in children if left unassigned."
    )]
    private DynamicFOV _dynamicFOV;

    private PlayerWallRun _playerWallRun;
    private float _currentTilt;
    private float _currentFovBoost;

    private void Awake()
    {
        _playerWallRun = GetComponentInParent<PlayerWallRun>();
        if (_playerWallRun == null)
        {
            Debug.LogError(
                "[CameraWallRunTilt] No PlayerWallRun found in parents. Disabling component.",
                this
            );
            enabled = false;
            return;
        }

        _dynamicFOV = _dynamicFOV != null ? _dynamicFOV : GetComponentInChildren<DynamicFOV>();
        if (_dynamicFOV == null)
        {
            Debug.LogError(
                "[CameraWallRunTilt] No DynamicFOV found in children. Disabling component.",
                this
            );
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        float targetTilt = GetTargetTilt(
            _playerWallRun.IsWallRunning,
            _playerWallRun.IsWallRight,
            _playerWallRun.IsWallLeft,
            _maxTiltAngle
        );

        _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, Time.deltaTime * _tiltSpeed);
        transform.localRotation = Quaternion.Euler(0f, 0f, _currentTilt);

        float targetBoost = _playerWallRun.IsWallRunning ? _fovBoost : 0f;
        _currentFovBoost = Mathf.Lerp(_currentFovBoost, targetBoost, Time.deltaTime * _tiltSpeed);
        _dynamicFOV.SetWallRunFOVBoost(_currentFovBoost);
    }

    /// <summary>
    /// Selects the target roll angle from wall-run state. Isolated so the sign/mapping
    /// convention is easy to flip during tuning. Mirrors the legacy tilt-toward-wall mapping.
    /// </summary>
    private static float GetTargetTilt(
        bool isWallRunning,
        bool isWallRight,
        bool isWallLeft,
        float maxTiltAngle
    )
    {
        if (!isWallRunning)
            return 0f;

        if (isWallRight)
            return maxTiltAngle;

        if (isWallLeft)
            return -maxTiltAngle;

        return 0f;
    }
}
