using System.Collections.Generic;
using Health;
using UnityEngine;

public class BodyPartModifier : IDamageModifier
{
    private readonly IReadOnlyList<HealthModifierData> _table;

    public int Priority => 0;

    public BodyPartModifier(IReadOnlyList<HealthModifierData> table) => _table = table;

    public float ModifyDamage(float damage, DamageInfo info)
    {
        //we do the look up O(4)
        foreach (var entry in _table)
            if (entry.bodyPart == info.BodyPart)
                return damage * entry.modifier;
        return damage; // no entry = neutral (silent, by design)
    }
}
