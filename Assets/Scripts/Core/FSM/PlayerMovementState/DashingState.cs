using Core;
using UnityEngine;

public class DashingState : BaseState<PlayerMovementContext>
{
    public DashingState(PlayerMovementContext context) : base(context) { }

    public override void OnEnter()
    {
        Context.Controllable.SetMovementState(MovementState.Dashing);
        
        // Dash is already started by the transition state
        // This state just manages behavior during the dash
    }

    public override void OnUpdate()
    {
        // Exit dash when it's complete
        if (!Context.PlayerDash.IsDashing)
        {
            ExitToAppropriateState();
            return;
        }
    }

    public override void OnExit()
    {
        // Ensure dash is properly ended
        if (Context.PlayerDash.IsDashing)
        {
            Context.PlayerDash.CancelDash();
        }
    }

    private void ExitToAppropriateState()
    {
        // Decide next state based on player conditions
        if (Context.PlayerMovement.IsGrounded)
        {
            if (Context.WantsToSprint)
            {
                Context.StateMachine.ChangeState(MovementState.Sprinting);
            }
            else if (Context.WantsToCrouch)
            {
                Context.StateMachine.ChangeState(MovementState.Crouching);
            }
            else
            {
                Context.StateMachine.ChangeState(MovementState.Walking);
            }
        }
        else
        {
            // In air after dash
            Context.StateMachine.ChangeState(Context.WantsToSprint ? MovementState.Sprinting : MovementState.Walking);
        }
    }
}