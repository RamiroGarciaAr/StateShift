using UnityEngine;

public abstract class WeaponAnimationModule
{
    public abstract Pose AnimationPose { get; }
    public abstract void Tick(float deltaTime);
}
