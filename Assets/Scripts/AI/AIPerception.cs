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

    private bool _isValid;

    // Number of line segments used to tessellate the detection cone/disc gizmo.
    private const int Segments = 48;

    private void Awake()
    {
        RecalculateCone();
        _isValid = eyeOrigin != null;
        if (!_isValid)
        {
            Debug.LogError($"[AIPerception] No eyeOrigin assigned on {gameObject.name}", this);
            enabled = false;
            return;
        }
    }

    public PerceptionResult Sample()
    {
        if (!_isValid || target == null)
            return PerceptionResult.Miss;

        Vector3 aimPoint = target.position + Vector3.up * aimHeight;
        bool detected = CanDetectTarget(aimPoint);

#if UNITY_EDITOR
        Debug.DrawLine(eyeOrigin.position, aimPoint, detected ? Color.green : Color.red);
#endif

        return detected
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

    /// <summary>
    /// Renders the editor-only detection cone from the eye origin so designers can tune
    /// detectionAngle and detectionDistance directly in the Scene view. Draws a wire disc
    /// for a full 360 degree field of view, or a swept arc with boundary edge rays otherwise.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (eyeOrigin == null)
            return;

        Vector3 origin = eyeOrigin.position;
        Vector3 forward = eyeOrigin.forward;
        Vector3 up = eyeOrigin.up;

        Gizmos.color = Color.yellow;

        if (detectionAngle >= 360f)
        {
            Vector3 previous = origin + forward * detectionDistance;
            for (int i = 1; i <= Segments; i++)
            {
                float angle = 360f * i / Segments;
                Vector3 current = origin + (Quaternion.AngleAxis(angle, up) * forward) * detectionDistance;
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
            return;
        }

        float half = detectionAngle * 0.5f;

        Vector3 leftEdge = origin + (Quaternion.AngleAxis(-half, up) * forward) * detectionDistance;
        Vector3 rightEdge = origin + (Quaternion.AngleAxis(half, up) * forward) * detectionDistance;
        Gizmos.DrawLine(origin, leftEdge);
        Gizmos.DrawLine(origin, rightEdge);

        Vector3 previousArc = leftEdge;
        for (int i = 1; i <= Segments; i++)
        {
            float angle = -half + detectionAngle * i / Segments;
            Vector3 currentArc = origin + (Quaternion.AngleAxis(angle, up) * forward) * detectionDistance;
            Gizmos.DrawLine(previousArc, currentArc);
            previousArc = currentArc;
        }
    }
#endif

    private void RecalculateCone()
    {
        _cosHalfAngle = Mathf.Cos(Mathf.Deg2Rad * detectionAngle * 0.5f);
    }
}
