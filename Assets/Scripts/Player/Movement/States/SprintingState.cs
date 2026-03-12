using UnityEngine;
using Core;

public class SprintingState : BaseState<PlayerMovementContext>
{
    public SprintingState(PlayerMovementContext context) : base(context) { }

    public override void OnEnter()
    {
        Context.Controllable.SetMovementState(MovementState.Sprinting);
    }

    public override void OnUpdate()
    {
        // Transición a Slide cuando presiona crouch mientras corre
        if (Context.WantsToCrouch)
        {
            bool slideStarted = Context.PlayerSlide.TryStartSlide();

            if (slideStarted)
            {
                Context.GroundedStateMachine.ChangeState(MovementState.Sliding);
            }
            else
            {
                // Si no tiene velocidad suficiente, ir directo a crouch
                Context.GroundedStateMachine.ChangeState(MovementState.Crouching);
            }
            return;
        }
        // Transition to Grapple
        if (Context.WantsToGrapple && Context.PlayerGrapple.CanGrapple)
        {
            bool grappleStarted = Context.PlayerGrapple.TryStartGrapple();
            if (grappleStarted)
            {
                Context.StateMachine.ChangeState(MovementState.Grappling);
                return;
            }
        }
        // Transición a WallRunning cuando está en el aire y tiene una pared
        if (!Context.PlayerMovement.IsGrounded)
        {
            if (Context.PlayerWallRun.CanWallRun())
            {
                Context.StateMachine.ChangeState(MovementState.WallRunning);
                return;
            }
        }

        // Transición a Walking
        if (!Context.WantsToSprint)
        {
            Context.GroundedStateMachine.ChangeState(MovementState.Walking);
            return;
        }
        //Transicion a Dash
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

