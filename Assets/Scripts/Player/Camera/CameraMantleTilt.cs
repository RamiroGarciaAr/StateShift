using UnityEngine;

/// <summary>
/// Applies a subtle view nod (and optional roll) while the player is mantling, driven by the
/// mantle's normalized progress so the tilt eases in as the vault starts and eases back out as it
/// finishes. This component is the single writer of its own transform's local rotation, keeping the
/// mantle nod decoupled from the camera holder, recoil, shake, and wall-run roll pivots.
/// </summary>
public class CameraMantleTilt : MonoBehaviour
{
    [Header("Tilt")]
    [Tooltip("Peak forward pitch (degrees) applied at the strongest point of the mantle. Positive pitches the view down toward the ledge.")]
    [SerializeField] private float _maxPitchAngle = 8f;

    [Tooltip("Peak roll (degrees) applied at the strongest point of the mantle. Leave at 0 for a pure nod.")]
    [SerializeField] private float _maxRollAngle = 0f;

    [Tooltip("Lerp speed used to blend the nod in and out for smoothness.")]
    [SerializeField] private float _tiltSpeed = 10f;

    [Tooltip("Maps mantle progress [0,1] to tilt strength [0,1]. A bump shape nods in then out over the vault.")]
    [SerializeField] private AnimationCurve _tiltProfile = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f));

    private PlayerMantle _playerMantle;
    private float _currentStrength;

    private void Awake()
    {
        _playerMantle = GetComponentInParent<PlayerMantle>();
        if (_playerMantle == null)
        {
            Debug.LogError(
                "[CameraMantleTilt] No PlayerMantle found in parents. Disabling component.",
                this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        float targetStrength = _playerMantle.IsMantling
            ? _tiltProfile.Evaluate(_playerMantle.MantleProgress01)
            : 0f;

        _currentStrength = Mathf.Lerp(_currentStrength, targetStrength, Time.deltaTime * _tiltSpeed);
        transform.localRotation = Quaternion.Euler(
            _currentStrength * _maxPitchAngle,
            0f,
            _currentStrength * _maxRollAngle);
    }
}
