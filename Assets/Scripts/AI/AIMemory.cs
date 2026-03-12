
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
    
}
