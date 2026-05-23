using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class CameraRecoilModule : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField]
    private float verticalKick = 2f;

    [SerializeField]
    private float horizontalKick = 0.5f;

    [Tooltip("The spring settings for the camera recoil.")]
    [SerializeField]
    private SpringVector3 cameraRecoilSpring;

    void OnEnable()
    {
        WeaponBase.OnShoot += HandleWeaponFired;
    }

    void OnDisable()
    {
        WeaponBase.OnShoot -= HandleWeaponFired;
    }

    private void LateUpdate()
    {
        // Update the camera recoil spring each frame
        cameraRecoilSpring.Update(Time.deltaTime);
        // Apply the recoil offset to the camera's local rotation
        transform.localRotation = Quaternion.Euler(cameraRecoilSpring.Value);
    }

    private void HandleWeaponFired()
    {
        // Apply recoil to the camera
        Vector3 recoilKick = new Vector3(
            -verticalKick,
            Random.Range(-horizontalKick, horizontalKick),
            0
        );
        cameraRecoilSpring.AddImpulse(recoilKick);
    }
}
