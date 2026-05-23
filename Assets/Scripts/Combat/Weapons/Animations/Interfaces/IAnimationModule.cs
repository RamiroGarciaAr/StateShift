using UnityEngine;

public interface IAnimationModule
{
    public Pose AnimationPose { get; }

    public void Tick(float deltaTime);
}
