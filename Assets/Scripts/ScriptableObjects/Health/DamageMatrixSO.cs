using UnityEngine;
using Health;

[CreateAssetMenu(menuName = "Health/Damage Matrix")]
public class DamageMatrixSO : ScriptableObject
{
    [System.Serializable]
    public struct DamageMultiplierRow
    {
        public float Kinetic;
        public float Fire;
        public float Plasma;
        public float Energy;

        public float GetMultiplier(DamageType damageType)
        {
            return damageType switch
            {
                DamageType.Kinetic => Kinetic,
                DamageType.Fire => Fire,
                DamageType.Plasma => Plasma,
                DamageType.Energy => Energy,
                _ => 1f
            };
        }
    }

    [Header("Damage Multipliers by Health Type")]
    [Tooltip("Multipliers for Flesh health type (weak to Fire)")]
    public DamageMultiplierRow Flesh = new() { Kinetic = 1f, Fire = 1.5f, Plasma = 1f, Energy = 0.5f };

    [Tooltip("Multipliers for Exo health type (weak to Energy)")]
    public DamageMultiplierRow Exo = new() { Kinetic = 0.75f, Fire = 1f, Plasma = 1f, Energy = 1.5f };

    [Tooltip("Multipliers for Shield health type (weak to Plasma)")]
    public DamageMultiplierRow Shield = new() { Kinetic = 1f, Fire = 0.5f, Plasma = 2f, Energy = 1f };

    public float GetMultiplier(DamageType damageType, HealthType healthType)
    {
        if (healthType == HealthType.Player) return 1f;

        return healthType switch
        {
            HealthType.Flesh => Flesh.GetMultiplier(damageType),
            HealthType.Exo => Exo.GetMultiplier(damageType),
            HealthType.Shield => Shield.GetMultiplier(damageType),
            _ => 1f
        };
    }
}