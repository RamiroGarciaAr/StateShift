using Entities.Controllers;
using UnityEngine;

/// <summary>
///  Knows all the Pivots and The events
///  Will delegate to the modules
/// </summary>
[DefaultExecutionOrder(-10)] // Ensure this runs after other scripts that might trigger the events
public class WeaponAnimationController : MonoBehaviour
{
    [Header("Pivots")]
    [SerializeField]
    private Transform swayPivot;

    [SerializeField]
    private Transform recoilPivot;

    [SerializeField]
    private Transform bobPivot;

    [Header("Modules")]
    [SerializeField]
    private WeaponSwayModule swayModule;

    [SerializeField]
    private WeaponRecoilModule recoilModule;

    [SerializeField]
    private WeaponBobModule bobModule;

    [Header("Animator")]
    [SerializeField]
    private Animator animator;

    private void OnEnable()
    {
        PlayerInput.OnLook += HandleLook;
        WeaponBase.OnShoot += HandleShoot;
        PlayerInput.OnMove += HandleMove;
        WeaponBase.OnReloadAnimation += HandleReload;
    }

    private void OnDisable()
    {
        PlayerInput.OnLook -= HandleLook;
        WeaponBase.OnShoot -= HandleShoot;
        PlayerInput.OnMove -= HandleMove;
        WeaponBase.OnReloadAnimation -= HandleReload;
    }

    private void LateUpdate()
    {
        /*
            Recoil Pivot changes but...maybe we can change it so the module also changes the local position?
            I guess it would be more efficient to only change the local rotation and not the position but...we will see
        */
        recoilPivot.localRotation = Quaternion.Euler(recoilModule.RotationValue);
        recoilPivot.localPosition = recoilModule.PositionValue;

        swayPivot.localRotation = Quaternion.Euler(swayModule.RotationValue);
        swayPivot.localPosition = swayModule.PositionValue;

        bobPivot.localRotation = Quaternion.Euler(bobModule.RotationValue);
        bobPivot.localPosition = bobModule.PositionValue;
    }

    private void HandleLook(Vector2 lookInput) => swayModule.ApplySway(lookInput);

    private void HandleShoot() => recoilModule.ApplyRecoil();

    private void HandleMove(Vector2 moveInput) => bobModule.ApplyBob(moveInput);

    private void HandleReload(float animationSpeed)
    {
        animator.speed = animationSpeed;
        animator.SetTrigger("Reload");
    }
}
