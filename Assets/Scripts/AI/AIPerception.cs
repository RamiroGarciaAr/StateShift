using UnityEngine;

public class AIPerception : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Tooltip("Eye position")]
    private Transform eyeOrigin;

    [SerializeField, Tooltip("Represents what the AI can't see pass")]
    private LayerMask occludedMask;

    [SerializeField, Range(1f, 360f), Tooltip("Amount of degrees that the AI can see")]
    private float detectionAngle = 360f;

    [SerializeField, Tooltip("Max distance that the AI can see")]
    private float detectionDistance = 30f;

    [SerializeField]
    private Transform target;

    private float _cosHalfAngle;

    //TODO: This is not...gr8 so i should find later a better way to set the offset without coupleing ut
    //"Trigger: player gains crouch, or enemies need to target specific hitboxes."
    [SerializeField]
    private float aimHeight = 1f; // the player height is 2f

    private void Awake()
    {
        RecalculateCone();

        if (eyeOrigin == null)
        {
            Debug.LogError($"[AIPerception] No eyeOrigin assigned on {gameObject.name}", this);
            enabled = false;
        }
    }

    public PerceptionResult Sample()
    {
        if (target == null)
            return PerceptionResult.Miss;

        Vector3 aimPoint = target.position + Vector3.up * aimHeight;
        return CanDetectTarget(aimPoint)
            ? new PerceptionResult(true, aimPoint)
            : PerceptionResult.Miss;
    }

    private bool CanDetectTarget(Vector3 aimPoint)
    {
        float distance = Vector3.Distance(eyeOrigin.position, aimPoint);
        Vector3 dirToTarget = aimPoint - eyeOrigin.position;
        dirToTarget.Normalize();
        float dot = Vector3.Dot(eyeOrigin.forward, dirToTarget);
        if (distance > detectionDistance || dot < _cosHalfAngle)
            return false;

        //check if the view is interrupted
        if (Physics.Raycast(eyeOrigin.position, dirToTarget, distance, occludedMask))
            return false;
        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RecalculateCone();
    }
#endif

    private void RecalculateCone()
    {
        _cosHalfAngle = Mathf.Cos(Mathf.Deg2Rad * detectionAngle * 0.5f);
    }
}
