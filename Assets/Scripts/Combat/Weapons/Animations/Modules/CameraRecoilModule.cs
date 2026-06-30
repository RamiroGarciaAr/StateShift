using UnityEngine;

/// <summary>
/// Links camera recoil to weapon recoil events for a unified procedural kick.
/// </summary>
[DefaultExecutionOrder(-20)]
public class CameraRecoilModule : MonoBehaviour
{
    [Header("Recoil Coupling")]
    [Tooltip("How much of the weapon's rotation recoil impulse is applied to the camera.")]
    [Range(0f, 2f)]
    [SerializeField]
    private float _recoilCouplingWeight = 0.5f;

    [Header("Spring Settings")]
    [Tooltip("The spring settings for the camera recoil.")]
    [SerializeField]
    private SpringVector3 _cameraRecoilSpring;

    private void OnEnable()
    {
        WeaponRecoilModule.OnRecoilImpulse += HandleRecoilImpulse;
    }

    private void OnDisable()
    {
        WeaponRecoilModule.OnRecoilImpulse -= HandleRecoilImpulse;
    }

    private void LateUpdate()
    {
        _cameraRecoilSpring.Update(Time.deltaTime);

        Vector3 val = _cameraRecoilSpring.Value;
        if (!float.IsNaN(val.x) && !float.IsNaN(val.y) && !float.IsNaN(val.z))
        {
            transform.localRotation = Quaternion.Euler(val);
        }
    }

    private void HandleRecoilImpulse(Vector3 rotImpulse, Vector3 posImpulse)
    {
        // Apply scaled rotation impulse to the camera
        _cameraRecoilSpring.AddImpulse(rotImpulse * _recoilCouplingWeight);
    }
}
