using Core;
using UnityEngine;

[System.Serializable]
public struct StanceEntry
{
    public MovementState movementState;
    public WeaponStanceConfig stanceConfig;
}

//TODO: Consider making the fields private and exposing them through properties to encapsulate the data and prevent unintended modifications from outside the class.
[System.Serializable]
public struct WeaponStanceConfig
{
    public Vector3 PositionOffset;
    public Vector3 RotationOffset;

    [Range(0.1f, 10f)]
    public float poseF;

    [Range(0.1f, 10f)]
    public float poseZ;

    [Range(0.1f, 10f)]
    public float poseR;

    public WeaponStanceConfig(
        Vector3 positionOffset,
        Vector3 rotationOffset,
        float poseF,
        float poseZ,
        float poseR
    )
    {
        PositionOffset = positionOffset;
        RotationOffset = rotationOffset;
        this.poseF = poseF;
        this.poseZ = poseZ;
        this.poseR = poseR;
    }
}
