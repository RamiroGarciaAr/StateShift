using UnityEngine;

[System.Serializable]
public class WeaponStanceModule : WeaponAnimationModule
{
    [SerializeField]
    private WeaponStanceConfigSO weaponStanceConfig;

    [SerializeField]
    private MovementStateVariableSO movementStateVariable;

    [SerializeField]
    private SecondOrderDynamics poseSpringPosition;

    [SerializeField]
    private SecondOrderDynamics poseSpringRotation;
    public override Pose AnimationPose =>
        new Pose(poseSpringPosition.Value, Quaternion.Euler(poseSpringRotation.Value));

    public override void Tick(float deltaTime)
    {
        if (weaponStanceConfig == null || movementStateVariable == null)
            return;

        WeaponStanceConfig stanceConfig = weaponStanceConfig.GetStanceConfig(
            movementStateVariable.Value
        );
        poseSpringPosition.ComputeConstants(
            stanceConfig.poseF,
            stanceConfig.poseZ,
            stanceConfig.poseR
        );
        poseSpringRotation.ComputeConstants(
            stanceConfig.poseF,
            stanceConfig.poseZ,
            stanceConfig.poseR
        );
        poseSpringPosition.Update(deltaTime, stanceConfig.PositionOffset);
        poseSpringRotation.Update(deltaTime, stanceConfig.RotationOffset);
    }
}
