using Entities.Controllers;
using UnityEngine;

/// <summary>
/// Central hub managing weapon animation modules and applying summed offsets to the weapon pivot.
/// </summary>
public class WeaponAnimationController : MonoBehaviour
{
    [Header("Pivots")]
    [Tooltip("The single consolidated pivot for all weapon procedural animations.")]
    [SerializeField]
    private Transform _weaponPivot;

    [Header("Modules (Concrete)")]
    [Header("Weapon Sway")]
    [Tooltip("The sway module handles weapon sway based on player look input.")]
    [SerializeField]
    private WeaponSwayModule _swayModule;

    [Header("Weapon Recoil")]
    [Tooltip("The recoil module handles weapon recoil based on shooting actions.")]
    [SerializeField]
    private WeaponRecoilModule _recoilModule;

    [Header("Weapon Bob")]
    [Tooltip("The bob module handles vertical and horizontal bobbing based on player movement.")]
    [SerializeField]
    private WeaponBobModule _bobModule;

    [Header("Weapon Inertia")]
    [Tooltip("The inertia module handles the inertia effect of the weapon during movement.")]
    [SerializeField]
    private WeaponInertiaModule _inertiaModule;

    [Header("Weapon ADS")]
    [Tooltip("The ADS module blends the weapon between hip and aimed poses.")]
    [SerializeField]
    private WeaponAdsModule _adsModule;

    [Header("Dependencies")]
    [SerializeField]
    private PlayerMovement _playerMovement;

    [SerializeField]
    private Animator _animator;

    private void OnEnable()
    {
        _adsModule.ResetState();

        PlayerInput.OnLook += HandleLook;
        PlayerInput.OnAim += HandleAim;
        WeaponBase.OnShoot += HandleShoot;
        WeaponBase.OnReloadAnimation += HandleReload;
    }

    private void OnDisable()
    {
        PlayerInput.OnLook -= HandleLook;
        PlayerInput.OnAim -= HandleAim;
        WeaponBase.OnShoot -= HandleShoot;
        WeaponBase.OnReloadAnimation -= HandleReload;
    }

    private void LateUpdate()
    {
        if (_weaponPivot == null || _playerMovement == null)
            return;

        float deltaTime = Time.deltaTime;
        Vector3 worldVelocity = _playerMovement.Rigidbody.velocity;

        // 1. Feed driving inputs into the movement-driven modules.
        _bobModule.UpdateBob(worldVelocity, _playerMovement.transform);
        _inertiaModule.UpdateInertia(worldVelocity, _playerMovement.transform);

        // 2. Advance every module.
        _swayModule.Tick(deltaTime);
        _bobModule.Tick(deltaTime);
        _inertiaModule.Tick(deltaTime);
        _recoilModule.Tick(deltaTime);
        _adsModule.Tick(deltaTime);

        // 3. ADS steadiness scales down the modules that would jostle the sight.
        float adsWeight = _adsModule.Weight;
        float swayScale = GetSteadinessScale(adsWeight, _adsModule.SwaySteadiness);
        float bobScale = GetSteadinessScale(adsWeight, _adsModule.BobSteadiness);
        float inertiaScale = GetSteadinessScale(adsWeight, _adsModule.InertiaSteadiness);
        float recoilScale = GetSteadinessScale(adsWeight, _adsModule.RecoilSteadiness);

        // 4. Accumulate the scaled offsets. ADS drives the pivot to the aim pose at full weight.
        Vector3 totalPosition = Vector3.zero;
        Quaternion totalRotation = Quaternion.identity;

        Accumulate(ref totalPosition, ref totalRotation, _swayModule.AnimationPose, swayScale);
        Accumulate(ref totalPosition, ref totalRotation, _bobModule.AnimationPose, bobScale);
        Accumulate(ref totalPosition, ref totalRotation, _inertiaModule.AnimationPose, inertiaScale);
        Accumulate(ref totalPosition, ref totalRotation, _recoilModule.AnimationPose, recoilScale);
        Accumulate(ref totalPosition, ref totalRotation, _adsModule.AnimationPose, 1f);

        // 5. Apply to pivot.
        _weaponPivot.localPosition = totalPosition;
        _weaponPivot.localRotation = totalRotation;
    }

    /// <summary>Blends a module's contribution scale from 1 (hip) toward (1 - steadiness) at full ADS.</summary>
    private static float GetSteadinessScale(float adsWeight, float steadiness) =>
        1f - (adsWeight * steadiness);

    /// <summary>Adds a scaled pose onto the running position/rotation accumulators.</summary>
    private static void Accumulate(
        ref Vector3 totalPosition,
        ref Quaternion totalRotation,
        Pose pose,
        float scale
    )
    {
        totalPosition += pose.position * scale;
        totalRotation *= Quaternion.Slerp(Quaternion.identity, pose.rotation, scale);
    }

    private void HandleLook(Vector2 lookInput) => _swayModule.ApplySway(lookInput);

    private void HandleAim(bool isAiming) => _adsModule.SetAiming(isAiming);

    private void HandleShoot() => _recoilModule.ApplyRecoil();

    private void HandleReload(float animationSpeed)
    {
        if (_animator != null)
        {
            _animator.speed = animationSpeed;
            _animator.SetTrigger("Reload");
        }
    }
}
