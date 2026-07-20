using UnityEngine;

/// <summary>
/// Applies a subtle Perlin-noise positional shake to its own pivot transform while wall
/// running. This component is the single writer of its transform's local position.
/// </summary>
public class CameraWallRunShake : MonoBehaviour
{
    [Header("Shake")]
    [SerializeField]
    [Tooltip("Maximum positional offset (metres) applied at full wall-run weight.")]
    private float _amplitude = 0.03f;

    [SerializeField]
    [Tooltip("Speed at which the Perlin noise is sampled.")]
    private float _frequency = 12f;

    [SerializeField]
    [Tooltip("Lerp speed used to blend the shake weight in and out.")]
    private float _blendSpeed = 10f;

    private PlayerWallRun _playerWallRun;
    private float _weight;
    private float _seed;

    private void Awake()
    {
        _playerWallRun = GetComponentInParent<PlayerWallRun>();
        if (_playerWallRun == null)
        {
            Debug.LogError(
                "[CameraWallRunShake] No PlayerWallRun found in parents. Disabling component.",
                this
            );
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        float targetWeight = _playerWallRun.IsWallRunning ? 1f : 0f;
        _weight = Mathf.Lerp(_weight, targetWeight, Time.deltaTime * _blendSpeed);

        _seed += Time.deltaTime * _frequency;

        float scale = _amplitude * _weight;
        float x = (Mathf.PerlinNoise(_seed, 0f) * 2f - 1f) * scale;
        float y = (Mathf.PerlinNoise(0f, _seed) * 2f - 1f) * scale;

        transform.localPosition = new Vector3(x, y, 0f);
    }
}
