using UnityEngine;

[DefaultExecutionOrder(-20)] // Runs before the WeaponAnimationController to ensure the recoil is applied before the animation updates
public class WeaponRecoilModule : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField]
    private float recoilAmount = 10f;

    [SerializeField]
    private float recoilShakeAmount = 5f;

    [SerializeField]
    private float kickbackAmount = 0.1f;

    [SerializeField]
    private SpringVector3 recoilSpringRotation = new SpringVector3();

    [SerializeField]
    private SpringVector3 recoilSpringPosition = new SpringVector3();

    public Vector3 RotationValue => recoilSpringRotation.Value;
    public Vector3 PositionValue => recoilSpringPosition.Value;

    private void LateUpdate()
    {
        recoilSpringRotation.Update(Time.deltaTime);
        recoilSpringPosition.Update(Time.deltaTime);
    }

    public void ApplyRecoil()
    {
        recoilSpringRotation.AddImpulse(
            new Vector3(-recoilAmount, Random.Range(-recoilShakeAmount, recoilShakeAmount), 0f)
        );
        recoilSpringPosition.AddImpulse(new Vector3(0f, 0f, -kickbackAmount));
    }
}
