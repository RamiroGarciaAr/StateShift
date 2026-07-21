using UnityEngine;

public class AIAiming : MonoBehaviour
{
    [SerializeField]
    private float turnSpeed = 60f; // deg/s

    [SerializeField]
    private float pitchSpeed = 90f; // deg/s

    private Vector3 _target;
    private bool _hasTarget;

    [SerializeField]
    private Transform pitchPivot;

    [Range(-90, 0), SerializeField]
    private float minPitch = -15f;

    [Range(0, 90), SerializeField]
    private float maxPitch = 30f;

    private void Awake()
    {
        if (pitchPivot == null)
        {
            Debug.LogError($"[AIAiming] No pitch pivot assigned on: {gameObject.name}", this);
            enabled = false;
            return;
        }
    }

    public void SetTarget(Vector3 point)
    {
        _target = point;
        _hasTarget = true;
    }

    private void Update()
    {
        if (!_hasTarget)
            return;

        Vector3 toTarget = _target - transform.position;
        Vector3 horizontalVector = new Vector3(toTarget.x, 0, toTarget.z);
        RotateAIYaw(horizontalVector);
        RotateAIPitch(horizontalVector);
    }

    private void RotateAIPitch(Vector3 horizontalTarget)
    {
        float dy = _target.y - pitchPivot.position.y;
        float elevation = Mathf.Atan2(dy, horizontalTarget.magnitude) * Mathf.Rad2Deg;

        float clamped = Mathf.Clamp(elevation, minPitch, maxPitch);

        Quaternion desiredLocal = Quaternion.Euler(-clamped, 0, 0);
        pitchPivot.localRotation = Quaternion.RotateTowards(
            pitchPivot.localRotation,
            desiredLocal,
            pitchSpeed * Time.deltaTime
        );
    }

    private void RotateAIYaw(Vector3 horizontalTarget)
    {
        if (horizontalTarget.sqrMagnitude < 0.001f)
            return;

        Quaternion desired = Quaternion.LookRotation(horizontalTarget);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            desired,
            turnSpeed * Time.deltaTime
        );
    }
}
