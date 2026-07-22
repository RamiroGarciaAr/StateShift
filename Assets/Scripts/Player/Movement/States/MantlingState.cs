using Core;

public class MantlingState : BaseState<PlayerMovementContext>
{
    public MantlingState(PlayerMovementContext context) : base(context) { }

    public override void OnEnter()
    {
        Context.Controllable.SetMovementState(MovementState.Mantling);

        // The mantle is already started by the transition owner (GroundedState/InAirState/GrapplingState).
        // This state only broadcasts the movement state and waits for the scripted motion to finish.
    }

    public override void OnUpdate()
    {
        // PlayerMantle drives its own trajectory in FixedUpdate; poll for completion here.
        if (!Context.PlayerMantle.IsMantling)
        {
            ExitToAppropriateState();
        }
    }

    public override void OnExit()
    {
        // Safety: if we are transitioning out while still mantling (interrupt), abort cleanly.
        if (Context.PlayerMantle.IsMantling)
        {
            Context.PlayerMantle.CancelMantle();
        }
    }

    private void ExitToAppropriateState()
    {
        if (Context.PlayerMovement.IsGrounded)
            Context.StateMachine.ChangeState(MovementState.Grounded);
        else
            Context.StateMachine.ChangeState(MovementState.InAir);
    }
}
