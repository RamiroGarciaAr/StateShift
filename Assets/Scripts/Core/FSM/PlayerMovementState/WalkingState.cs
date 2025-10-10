using Core;

public class WalkingState : BaseState<PlayerMovementContext>
{
    public WalkingState(PlayerMovementContext context) : base(context) {}

    public override void OnEnter()
    {
        Context.Controllable.SetMovementState(MovementState.Walking);
        Context.PlayerCrouch.SetCrouching(false);
    }

    public override void OnUpdate()
    {
        if (Context.WantsToSprint && !Context.WantsToCrouch)
            {
                Context.StateMachine.ChangeState(MovementState.Sprinting);
                return;
            }

            // Transición a Crouch
            if (Context.WantsToCrouch)
            {
                Context.StateMachine.ChangeState(MovementState.Crouching);
                return;
            }
    }
}
