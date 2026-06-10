using UnityEngine;
using System;

[System.Serializable]
public class WeaponRecoilModule : WeaponAnimationModule
{
    [Header("Profile")]
    [SerializeField] 
    private WeaponRecoilProfileSO _profile;

    [Header("Springs")]
    [SerializeField] 
    private SpringVector3 _recoilSpringRotation = new SpringVector3();
    
    [SerializeField] 
    private SpringVector3 _recoilSpringPosition = new SpringVector3();

    public static event Action<Vector3, Vector3> OnRecoilImpulse;

    public override Pose AnimationPose => new Pose(_recoilSpringPosition.Value, Quaternion.Euler(_recoilSpringRotation.Value));

    public void ApplyRecoil()
    {
        if (_profile == null) return;

        Vector3 rotImpulse = new Vector3(
            -_profile.RecoilAmount,
            UnityEngine.Random.Range(-_profile.RecoilShakeAmount, _profile.RecoilShakeAmount),
            0f
        );

        Vector3 posImpulse = new Vector3(0f, 0f, -_profile.KickbackAmount);

        _recoilSpringRotation.AddImpulse(rotImpulse);
        _recoilSpringPosition.AddImpulse(posImpulse);

        OnRecoilImpulse?.Invoke(rotImpulse, posImpulse);
    }

    public override void Tick(float deltaTime)
    {
        _recoilSpringRotation.Update(deltaTime);
        _recoilSpringPosition.Update(deltaTime);
    }
}
