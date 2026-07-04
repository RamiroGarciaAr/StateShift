using UnityEngine;

[CreateAssetMenu(fileName = "WeaponInertiaConfig", menuName = "Animations/Weapon/InertiaConfig")]
public class WeaponInertiaConfigSO : ScriptableObject
{
    [Header("Inertia Settings")]
    [SerializeField, Tooltip("Amount of positional inertia applied based on velocity.")]
    private float _inertiaAmount = 0.02f;

    [SerializeField, Tooltip("Amount of roll rotation applied based on horizontal velocity.")]
    private float _rollAmount = 10f;

    [Header("Roll Dynamics (Second Order)")]
    [SerializeField, Range(0.5f, 10f), Tooltip("Frequency (f): Determines the speed at which the system responds to changes. Higher values make the motion faster and snappier.")]
    private float _rollF = 3f;

    [SerializeField, Range(0f, 2f), Tooltip("Damping (z): Controls how the system oscillates and settles. 0 is no damping, 1 is critical damping (no overshoot), and >1 is overdamped.")]
    private float _rollZ = 0.5f;

    [SerializeField, Range(-2f, 4f), Tooltip("Initial Response (r): Controls the initial acceleration or 'kick' of the system. Positive values cause overshoot/anticipation, negative values cause a delayed start.")]
    private float _rollR = 2f;

    public float InertiaAmount => _inertiaAmount;
    public float RollAmount => _rollAmount;
    public float RollF => _rollF;
    public float RollZ => _rollZ;
    public float RollR => _rollR;
}
