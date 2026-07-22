// ALERTED — actively fighting. This is what the bot already does today:
// aim at the last-known position; firing gates on live LOS in the brain.
public class AlertedState : AlertState
{
    public AlertedState(AIBrain brain)
        : base(brain) { }

    public override void OnEnter()
    {
        Context.Memory.SetAlertLevel(AIMemory.AlertLevel.Alerted);
    }

    public override void OnUpdate()
    {
        Context.Aiming.SetTarget(Context.Memory.LastKnownPlayerPosition);
    }
}
