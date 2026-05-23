using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-20)]
public class CameraRecoilModule : MonoBehaviour
{
    [Header("Vertical Accumulation")]
    [Tooltip("The maximum vertical recoil multiplier that can be applied.")]
    [SerializeField]
    private float maxVerticalRecoilMult = 3f;

    [Tooltip("The rate at which vertical recoil accumulates.")]
    [SerializeField]
    private float verticalGrowthRate = 3f;

    [SerializeField]
    private float verticalKick = 2f;

    [Header("Horizontal Accumulation")]
    [Tooltip("The maximum horizontal recoil multiplier that can be applied.")]
    [SerializeField]
    private float maxHorizontalRecoilWindow = 1f;

    [Tooltip("The rate at which horizontal recoil accumulates.")]
    [SerializeField]
    private float horizontalGrowthRate = 2f;

    [Header("Shot Tracking")]
    [Tooltip("The number of shots required to reach maximum recoil.")]
    [SerializeField]
    private int shotsToMaxRecoil = 10;

    [Tooltip("The time after which recoil starts to reset if no shots are fired.")]
    [SerializeField]
    private float recoilResetTime = 0.3f;

    [Tooltip("The spring settings for the camera recoil.")]
    [SerializeField]
    private SpringVector3 cameraRecoilSpring;

    private int _shotsFired;
    private float _resetTimer;

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
        _resetTimer -= Time.deltaTime;
        if (_resetTimer <= 0f)
        {
            _shotsFired = 0; // Reset shots fired when the timer runs out
        }
        // Update the camera recoil spring each frame
        cameraRecoilSpring.Update(Time.deltaTime);
        // Apply the recoil offset to the camera's local rotation
        transform.localRotation = Quaternion.Euler(cameraRecoilSpring.Value);
    }

    private void HandleWeaponFired()
    {
        _shotsFired = Mathf.Min(_shotsFired + 1, shotsToMaxRecoil); // Increment shots fired, but cap it at shotsToMaxRecoil
        _resetTimer = recoilResetTime; // We start the reset timer whenever a shot is fired

        float t = _shotsFired / (float)shotsToMaxRecoil; // Calculate the normalized time based on shots fired
        float vMult = 1f + (maxVerticalRecoilMult - 1f) * (1 - Mathf.Exp(-verticalGrowthRate * t));
        float hMult = maxHorizontalRecoilWindow * (1 - Mathf.Exp(-horizontalGrowthRate * t));

        Vector3 kick = new Vector3(-verticalKick * vMult, Random.Range(-hMult, hMult), 0f);
        cameraRecoilSpring.AddImpulse(kick);
    }
}
