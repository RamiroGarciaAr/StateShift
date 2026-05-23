using System.ComponentModel.Design;
using UnityEngine;

[DefaultExecutionOrder(-20)] // Runs before the WeaponAnimationController to ensure the bob is applied before the animation updates
public class WeaponBobModule : MonoBehaviour
{
    [Header("Bob Settings")]
    [Tooltip("How much the weapon should bob based on the movement input")]
    [Range(0f, 10f)]
    [SerializeField]
    float bobFrequency = 6f;

    [Tooltip("How much the weapon should bob based on the movement input")]
    [Range(0f, 1f)]
    [SerializeField]
    float bobAmplitude = 0.03f;

    [Tooltip("How much the weapon should rotate based on the movement input")]
    [SerializeField]
    float bobRotationAmount = 1f;

    private void LateUpdate()
    {
        // Rotate the weapon based on the movement input
        float rotationZ = Mathf.Sin(Time.time * bobFrequency) * bobRotationAmount;
        transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    public void ApplyBob(Vector2 moveInput)
    {
        float bobX = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        float bobY = Mathf.Cos(Time.time * bobFrequency * 2f) * bobAmplitude;
    }
}
