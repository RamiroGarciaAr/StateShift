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
    [SerializeField, Range(0.5f, 10f), Tooltip("Frequency of the roll spring.")]
    private float _rollF = 3f;

    [SerializeField, Range(0f, 2f), Tooltip("Damping coefficient of the roll spring.")]
    private float _rollZ = 0.5f;

    [SerializeField, Range(-2f, 4f), Tooltip("Initial response of the roll spring.")]
    private float _rollR = 2f;

    public float InertiaAmount => _inertiaAmount;
    public float RollAmount => _rollAmount;
    public float RollF => _rollF;
    public float RollZ => _rollZ;
    public float RollR => _rollR;
}
