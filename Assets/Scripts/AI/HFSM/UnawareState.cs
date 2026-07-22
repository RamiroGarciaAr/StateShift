using UnityEngine;

// UNAWARE — hasn't noticed the player. Walks its patrol route (if it has one).
public class UnawareState : AlertState
{
    public UnawareState(AIBrain brain)
        : base(brain) { }

    public override void OnEnter()
    {
        Context.Memory.SetAlertLevel(AIMemory.AlertLevel.Unaware);

        // Set the FIRST destination on enter — do NOT wait for OnUpdate to notice
        // the bot is idle, or a fresh bot reports "arrived" on frame 1 and skips its route.
        if (Context.Patrol != null && Context.Patrol.HasRoute)
            Context.NavMove.SetDestination(Context.Patrol.CurrentPoint);
    }

    public override void OnUpdate()
    {
        var patrol = Context.Patrol;
        if (patrol == null || !patrol.HasRoute)
            return; // stationary guard: no route, stand still

        // Arrived at the current point -> advance and head to the next.
        if (Context.NavMove.IsIdle)
        {
            if (patrol.AtEnd)
                return; // Once mode, reached the end: hold here

            patrol.Advance();
            Context.NavMove.SetDestination(patrol.CurrentPoint);
        }
    }

    public override void OnExit()
    {
        // Leaving patrol (spotted the player, took damage): stop moving so the
        // combat/search state starts from a clean stop, not mid-stride.
        Context.NavMove.Stop();
    }
}
