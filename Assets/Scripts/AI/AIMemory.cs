
using UnityEngine;

public class AIMemory : MonoBehaviour
{
    public enum AlertLevel
    {
        Unaware,
        Suspicious,
        Alerted
    }

    public AlertLevel CurrentAlertLevel { get; private set; } = AlertLevel.Unaware;
    public Vector3 LastKnownPlayerPosition { get; private set; }
    public float LastSeenPlayerTime { get; private set; } = -1f;
    public bool CanSeePlayer { get; private set; } = false;

    public void SetAlertLevel(AlertLevel newLevel)
    {
        if (CurrentAlertLevel == newLevel) return;
        
        CurrentAlertLevel = newLevel;
        // Debug.Log($"[AIMemory]Alert level changed to {CurrentAlertLevel} for {gameObject.name}");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = CurrentAlertLevel switch
        {
            AlertLevel.Unaware => Color.green,
            AlertLevel.Suspicious => Color.yellow,
            AlertLevel.Alerted => Color.red,
            _ => Color.white
        };
        Vector3 indicatorPosition = transform.position + Vector3.up * 2f;

        Gizmos.DrawSphere(indicatorPosition, 0.2f);

        if (CurrentAlertLevel != AlertLevel.Unaware)
        {
            Gizmos.DrawLine(indicatorPosition, LastKnownPlayerPosition);
        }
    }

}
