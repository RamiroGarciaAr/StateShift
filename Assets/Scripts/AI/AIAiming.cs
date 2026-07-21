using UnityEngine;

public class AIAiming : MonoBehaviour
{
    [SerializeField]
    private float turnSpeed = 60f;

    private Vector3 _target;
    private bool _hasTarget;

    public void SetTarget(Vector3 point)
    {
        _target = point;
        _hasTarget = true;
    }

    private void Update()
    {
        if (_hasTarget)
        {
            Vector3 toTarget = _target - transform.position;

            RotateAIHorizontal(toTarget);
        }
    }

    private void RotateAIHorizontal(Vector3 target)
    {
        target.y = 0f;

        if (target.sqrMagnitude < 0.001f)
            return;

        Quaternion desired = Quaternion.LookRotation(target);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desired,
            turnSpeed * Time.deltaTime
        );
    }
}
