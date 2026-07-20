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

#if UNITY_EDITOR
    [Header("Gizmos")]
    [SerializeField, Tooltip("Draw the perception gizmos even when this object is not selected")]
    private bool alwaysDrawGizmos = false;

    [SerializeField, Tooltip("Fill color of the field-of-view cone")]
    private Color fovFillColor = new(1f, 0.85f, 0.2f, 0.12f);

    [SerializeField, Tooltip("Outline color of the field-of-view cone")]
    private Color fovOutlineColor = new(1f, 0.85f, 0.2f, 0.6f);

    private const float TargetMarkerRadius = 0.15f;
#endif

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

    private void OnDrawGizmos()
    {
        if (alwaysDrawGizmos)
            DrawPerceptionGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (!alwaysDrawGizmos)
            DrawPerceptionGizmos();
    }

    /// <summary>
    /// Draws the field-of-view cone, its range boundary, and the live line-of-sight test to the current target.
    /// </summary>
    private void DrawPerceptionGizmos()
    {
        if (eyeOrigin == null)
            return;

        Vector3 origin = eyeOrigin.position;
        Vector3 forward = eyeOrigin.forward;
        Vector3 up = eyeOrigin.up;
        Vector3 right = eyeOrigin.right;
        float halfAngle = detectionAngle * 0.5f;

        // Horizontal FOV slice.
        UnityEditor.Handles.color = fovFillColor;
        Vector3 horizontalStart = Quaternion.AngleAxis(-halfAngle, up) * forward;
        UnityEditor.Handles.DrawSolidArc(origin, up, horizontalStart, detectionAngle, detectionDistance);

        // Vertical FOV slice.
        Vector3 verticalStart = Quaternion.AngleAxis(-halfAngle, right) * forward;
        UnityEditor.Handles.DrawSolidArc(origin, right, verticalStart, detectionAngle, detectionDistance);

        // Range boundary and cone edges.
        UnityEditor.Handles.color = fovOutlineColor;
        UnityEditor.Handles.DrawWireArc(origin, up, horizontalStart, detectionAngle, detectionDistance);
        UnityEditor.Handles.DrawWireArc(origin, right, verticalStart, detectionAngle, detectionDistance);
        if (detectionAngle < 360f)
        {
            Vector3 horizontalEnd = Quaternion.AngleAxis(halfAngle, up) * forward;
            Vector3 verticalEnd = Quaternion.AngleAxis(halfAngle, right) * forward;
            UnityEditor.Handles.DrawLine(origin, origin + horizontalStart * detectionDistance);
            UnityEditor.Handles.DrawLine(origin, origin + horizontalEnd * detectionDistance);
            UnityEditor.Handles.DrawLine(origin, origin + verticalStart * detectionDistance);
            UnityEditor.Handles.DrawLine(origin, origin + verticalEnd * detectionDistance);
        }

        DrawLineOfSight(origin);
    }

    /// <summary>
    /// Draws the sightline to the current target, colored green when visible and red when out of range, out of cone, or occluded.
    /// </summary>
    private void DrawLineOfSight(Vector3 origin)
    {
        if (target == null)
            return;

        Vector3 aimPoint = target.position + Vector3.up * aimHeight;
        bool visible = Application.isPlaying ? CanDetectTarget(aimPoint) : EvaluateVisibility(origin, aimPoint);

        Gizmos.color = visible ? Color.green : Color.red;
        Gizmos.DrawLine(origin, aimPoint);
        Gizmos.DrawWireSphere(aimPoint, TargetMarkerRadius);
    }

    /// <summary>
    /// Editor-only mirror of the runtime detection test that recomputes the cached cone so previews stay accurate before play.
    /// </summary>
    private bool EvaluateVisibility(Vector3 origin, Vector3 aimPoint)
    {
        RecalculateCone();
        return CanDetectTarget(aimPoint);
    }
#endif

    private void RecalculateCone()
    {
        _cosHalfAngle = Mathf.Cos(Mathf.Deg2Rad * detectionAngle * 0.5f);
    }
}
