// SUSPICIOUS — knows something happened, not where the player is now.
// Step 4 will add go-to-last-known + scan; for now it just times out.
public class SuspiciousState : AlertState
{
    private float _searchTimer;

    public SuspiciousState(AIBrain brain)
        : base(brain) { }

    public float ElapsedSearch => _searchTimer;

    public override void OnEnter()
    {
        Context.Memory.SetAlertLevel(AIMemory.AlertLevel.Suspicious);
        _searchTimer = 0f;
        // Step 4: set nav destination to LastKnownPlayerPosition, then scan.
    }

    public override void OnUpdate()
    {
        // Sliced tick delta — NOT Time.deltaTime. This is why the tick-delta fix existed.
        _searchTimer += Context.TickDelta;
    }
}
