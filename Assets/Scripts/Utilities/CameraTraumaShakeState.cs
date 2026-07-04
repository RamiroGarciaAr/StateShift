using UnityEngine;

/// <summary>
/// Allocation-free trauma + Perlin-noise accumulator implementing Squirrel Eiserloh's
/// trauma-squared-plus-noise camera shake model. Holds only runtime scalars; all tuning
/// lives on the owning MonoBehaviour. This is a plain (non-serialized) class so it can be
/// unit-tested in Edit Mode. It mirrors the API shape of <see cref="SpringVector3"/> only
/// (private state, a deltaTime-stepped method, values exposed via properties) and is
/// intentionally not serializable because it carries no configuration.
/// </summary>
public class CameraTraumaShakeState
{
    // Distinct constant seeds per axis so pitch/yaw/roll noise decorrelates.
    private const float PitchSeed = 0f;
    private const float YawSeed = 100f;
    private const float RollSeed = 200f;

    // Runtime state only. Never serialized (debug visibility comes from the Trauma property).
    private float _trauma;
    private float _noiseTime;

    /// <summary>Current trauma scalar in [0, 1]. Read-only, for tests and debug.</summary>
    public float Trauma => _trauma;

    /// <summary>Current noise time cursor. Read-only, exposed for tests to assert reset behavior.</summary>
    public float NoiseTime => _noiseTime;

    /// <summary>True while trauma has fully decayed, letting the owner early-out when idle.</summary>
    public bool IsAtRest => _trauma <= 0f;

    /// <summary>
    /// Adds a fixed kick of trauma, accumulating on rapid fire and clamped to 1.
    /// </summary>
    /// <param name="amount">Trauma to add this shot.</param>
    public void AddTrauma(float amount)
    {
        _trauma = Mathf.Clamp01(_trauma + amount);
    }

    /// <summary>
    /// Advances the state by one frame: decays trauma linearly toward 0 and scrolls the noise
    /// time. When trauma reaches rest, noise time is reset to 0 to avoid long-run float drift.
    /// A single call keeps the update site reading one deltaTime.
    /// </summary>
    /// <param name="decayPerSecond">Linear trauma decay rate per second.</param>
    /// <param name="noiseFrequency">Perlin scroll speed for the noise time axis.</param>
    /// <param name="deltaTime">Time step for this frame.</param>
    public void Tick(float decayPerSecond, float noiseFrequency, float deltaTime)
    {
        _trauma = Mathf.Max(0f, _trauma - decayPerSecond * deltaTime);

        if (_trauma <= 0f)
        {
            _noiseTime = 0f;
        }
        else
        {
            _noiseTime += noiseFrequency * deltaTime;
        }
    }

    /// <summary>
    /// Returns the additive per-axis rotation offset (pitch, yaw, roll) in degrees,
    /// each equal to maxAngle * shake * signedNoise(axisSeed).
    /// </summary>
    /// <param name="maxAnglesDegrees">Max pitch/yaw/roll magnitude in degrees.</param>
    /// <param name="exponent">Trauma exponent (2 = punchy, 3 = snappier).</param>
    public Vector3 GetRotationOffset(Vector3 maxAnglesDegrees, float exponent)
    {
        float shake = Shake(exponent);
        return new Vector3(
            maxAnglesDegrees.x * shake * SignedNoise(PitchSeed),
            maxAnglesDegrees.y * shake * SignedNoise(YawSeed),
            maxAnglesDegrees.z * shake * SignedNoise(RollSeed)
        );
    }

    /// <summary>Shake magnitude for the current trauma raised to the given exponent.</summary>
    /// <param name="exponent">Trauma exponent.</param>
    public float Shake(float exponent) => Mathf.Pow(_trauma, exponent);

    /// <summary>
    /// Samples Perlin noise for an axis seed and remaps [0, 1] to [-1, 1]. The clamp guards
    /// against PerlinNoise occasionally returning values slightly outside [0, 1], making the
    /// +/-maxAngle bound a hard guarantee.
    /// </summary>
    private float SignedNoise(float seed)
    {
        return Mathf.Clamp(Mathf.PerlinNoise(seed, _noiseTime) * 2f - 1f, -1f, 1f);
    }
}
