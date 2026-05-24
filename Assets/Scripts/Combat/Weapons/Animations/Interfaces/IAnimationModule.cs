using UnityEngine;

public interface IAnimationModule
{
    public Pose AnimationPose { get; }

    public void Tick(float deltaTime);
}

[System.Serializable]
public struct ModuleLayer
{
    public IAnimationModule Module;
    public Transform modulePivot;

    public ModuleLayer(IAnimationModule module, Transform modulePivot)
    {
        Module = module;
        this.modulePivot = modulePivot;
    }
}
