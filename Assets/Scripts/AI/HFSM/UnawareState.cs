using UnityEngine;

// UNAWARE — hasn't noticed the player.
// Step 3 will add patrol here; for now the state only sets the alert level.
public class UnawareState : AlertState
{
    public UnawareState(AIBrain brain)
        : base(brain) { }

    public override void OnEnter()
    {
        Context.Memory.SetAlertLevel(AIMemory.AlertLevel.Unaware);
    }

    public override void OnUpdate()
    {
        // Step 3: patrol route movement.
    }
}
