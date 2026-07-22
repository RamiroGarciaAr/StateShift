using UnityEngine;

public class AIMemory : MonoBehaviour
{
#if UNITY_EDITOR
    const float GizmoHeightOffset = 1.5f;
    const float GizmoSphereRadius = 0.2f;
    const float LastKnownGizmoRadius = 0.35f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = CurrentAlertLevel switch
        {
            AlertLevel.Unaware => Color.green,
            AlertLevel.Suspicious => Color.yellow,
            AlertLevel.Alerted => Color.red,
            _ => Color.white,
        };

        Vector3 indicatorPosition;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            // No mesh to measure against; place the indicator above the pivot.
            indicatorPosition = transform.position + Vector3.up * GizmoHeightOffset;
        }
        else
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            // Horizontally centered on the mesh, vertically just above its top,
            // so the indicator is pivot-independent for any NPC.
            indicatorPosition = new Vector3(
                bounds.center.x,
                bounds.max.y + GizmoHeightOffset,
                bounds.center.z
            );
        }

        Gizmos.DrawSphere(indicatorPosition, GizmoSphereRadius);

        if (LastSeenPlayerTime < 0f)
            return; // never seen anything

        Gizmos.color = CanSeePlayer ? Color.green : Color.red;
        Gizmos.DrawWireSphere(LastKnownPlayerPosition, LastKnownGizmoRadius);
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
    public Vector3 LastKnownVelocity { get; private set; }

    public float LastSeenPlayerTime { get; private set; } = -1f;
    public bool CanSeePlayer { get; private set; } = false;

    public void SetAlertLevel(AlertLevel newLevel)
    {
        if (CurrentAlertLevel == newLevel)
            return;

        CurrentAlertLevel = newLevel;
        Debug.Log($"[AIMemory] Alert level changed to {CurrentAlertLevel} for {gameObject.name}");
    }

    public void SeedSuspicion(Vector3 approxPlayerPos)
    {
        LastKnownPlayerPosition = approxPlayerPos;
        LastSeenPlayerTime = Time.time;
        // deliberately not touching CanSeePlayer (suspect, don't see) or LastKnownVelocity
    }

    public void Report(in PerceptionResult perceptionResult)
    {
        bool wasVisible = CanSeePlayer;
        bool seen = perceptionResult.IsVisible;
        CanSeePlayer = seen;

        if (seen)
        {
            // Derive velocity from two consecutive sightings — compute BEFORE overwriting.
            if (wasVisible)
            {
                float dt = Time.time - LastSeenPlayerTime;
                if (dt > 0.0001f)
                    LastKnownVelocity = (perceptionResult.Position - LastKnownPlayerPosition) / dt;
            }
            LastKnownPlayerPosition = perceptionResult.Position;
            LastSeenPlayerTime = Time.time;
        }
    }
}
