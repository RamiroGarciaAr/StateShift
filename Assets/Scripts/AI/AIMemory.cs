using UnityEngine;

public class AIMemory : MonoBehaviour
{
#if UNITY_EDITOR
    const float GizmoHeightOffset = 2f;
    const float GizmoSphereRadius = 0.2f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = CurrentAlertLevel switch
        {
            AlertLevel.Unaware => Color.green,
            AlertLevel.Suspicious => Color.yellow,
            AlertLevel.Alerted => Color.red,
            _ => Color.white,
        };
        Vector3 indicatorPosition = transform.position + Vector3.up * GizmoHeightOffset;

        Gizmos.DrawSphere(indicatorPosition, GizmoSphereRadius);

        if (LastSeenPlayerTime < 0f)
            return; // never seen anything

        Gizmos.color = CanSeePlayer ? Color.green : Color.red;
        Gizmos.DrawLine(indicatorPosition, LastKnownPlayerPosition);
    }
#endif

    public enum AlertLevel
    {
        Unaware,
        Suspicious,
        Alerted,
    }

    public AlertLevel CurrentAlertLevel { get; private set; } = AlertLevel.Unaware;
    public Vector3 LastKnownPlayerPosition { get; private set; }
    public float LastSeenPlayerTime { get; private set; } = -1f;
    public bool CanSeePlayer { get; private set; } = false;

    public void SetAlertLevel(AlertLevel newLevel)
    {
        if (CurrentAlertLevel == newLevel)
            return;

        CurrentAlertLevel = newLevel;
        Debug.Log($"[AIMemory] Alert level changed to {CurrentAlertLevel} for {gameObject.name}");
    }

    public void Report(in PerceptionResult perceptionResult)
    {
        bool seen = perceptionResult.IsVisible;
        CanSeePlayer = seen;
        if (seen)
        {
            LastKnownPlayerPosition = perceptionResult.Position;
            LastSeenPlayerTime = Time.time;
        }
    }
}
