using System;
using UnityEngine;

[System.Serializable]
public class DamageDropoff
{
    [SerializeField]
    private float midRange = 50f;

    [SerializeField]
    private float longRange = 100f;
    public float MidRange => midRange;
    public float LongRange => longRange;

    [SerializeField]
    [Range(0f, 100f)]
    private float longRangeDamage = 50f;
    public float LongRangeDamage => longRangeDamage;

    public float GetDamageMultiplierAtDistance(float distance)
    {
        if (distance <= MidRange)
            return 1f; // No damage drop-off within shortrange
        else if (distance >= LongRange)
            return LongRangeDamage / 100f; // Convert percentage to multiplier
        float t = Mathf.InverseLerp(MidRange, LongRange, distance); // Normalized distance between mid and long range
        return Mathf.Lerp(1f, LongRangeDamage / 100f, t); // Linearly interpolate between mid and long range damage multipliers
    }

    public void Validate()
    {
        if (MidRange <= 0f)
        {
            Debug.LogWarning($"Mid range distance ({MidRange}) cannot be zero or negative.");
        }
        if (LongRange <= 0f)
        {
            Debug.LogWarning($"Long range distance ({LongRange}) cannot be zero or negative.");
        }
        if (LongRange < MidRange)
        {
            Debug.LogWarning(
                $"Long range distance ({LongRange}) should be greater than or equal to mid range distance ({MidRange})."
            );
        }
    }
}
