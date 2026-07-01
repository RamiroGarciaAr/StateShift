using System.Collections.Generic;
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

    [Header("Dependencies")]
    [SerializeField]
    private PlayerMovement _playerMovement;

    [SerializeField]
    private Animator _animator;

    private readonly List<WeaponAnimationModule> _moduleStack = new List<WeaponAnimationModule>();

    private void Awake()
    {
        // Populate the stack in order of application
        if (_swayModule != null)
            _moduleStack.Add(_swayModule);
        if (_bobModule != null)
            _moduleStack.Add(_bobModule);
        if (_inertiaModule != null)
            _moduleStack.Add(_inertiaModule);
        if (_recoilModule != null)
            _moduleStack.Add(_recoilModule);
    }

    private void OnEnable()
    {
        PlayerInput.OnLook += HandleLook;
        WeaponBase.OnShoot += HandleShoot;
        WeaponBase.OnReloadAnimation += HandleReload;
    }

    private void OnDisable()
    {
        PlayerInput.OnLook -= HandleLook;
        WeaponBase.OnShoot -= HandleShoot;
        WeaponBase.OnReloadAnimation -= HandleReload;
    }

    private void LateUpdate()
    {
        if (_weaponPivot == null || _playerMovement == null)
            return;

        float dt = Time.deltaTime;
        Vector3 worldVelocity = _playerMovement.Rigidbody.velocity;

        // 1. Specific module updates (driving inputs)
        _bobModule.UpdateBob(worldVelocity, _playerMovement.transform);
        _inertiaModule.UpdateInertia(worldVelocity, _playerMovement.transform);

        // 2. Tick all modules and accumulate offsets
        Vector3 totalPos = Vector3.zero;
        Quaternion totalRot = Quaternion.identity;

        foreach (var module in _moduleStack)
        {
            module.Tick(dt);
            Pose pose = module.AnimationPose;
            totalPos += pose.position;
            totalRot *= pose.rotation;
        }

        // 3. Apply to pivot
        _weaponPivot.localPosition = totalPos;
        _weaponPivot.localRotation = totalRot;
    }

    private void HandleLook(Vector2 lookInput) => _swayModule.ApplySway(lookInput);

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
