using Health;

public readonly struct DamageDealtEvent
{
    public readonly float dmgEffectiveness; // 1.0 = neutral, <1 resisted, >1 effective
    public readonly BodyPart bodyPart;
    public readonly bool isKillShot;

    public DamageDealtEvent(float dmgEffectiveness, BodyPart bodyPart, bool isKillShot)
    {
        this.bodyPart = bodyPart;
        this.isKillShot = isKillShot;
        this.dmgEffectiveness = dmgEffectiveness;
    }
}
