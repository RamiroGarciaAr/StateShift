using Core;
using UnityEngine;

public class WalkingState : BaseState<PlayerMovementContext>
{
    public WalkingState(PlayerMovementContext context) : base(context) { }

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

        // Transicion a Crouch
        if (Context.WantsToCrouch)
        {
            Context.StateMachine.ChangeState(MovementState.Crouching);
            return;
        }
        // Transicion a Dash
        if (Context.WantsToDash)
        {
            bool dashStarted = Context.PlayerDash.TryStartDash(Context.DashInputDirection);
            
            if (dashStarted)
            {
                Context.StateMachine.ChangeState(MovementState.Dashing);
                Context.WantsToDash = false;
            }
            return;
        }

    }
}
