
using Core;
using UnityEngine;

public class CrouchingState : BaseState<PlayerMovementContext>
{
    public CrouchingState(PlayerMovementContext context) : base(context) { }

    public override void OnEnter()
    {
        Context.Controllable.SetMovementState(MovementState.Crouching);
        Context.PlayerCrouch.SetCrouching(true);
    }

    public override void OnUpdate()
    {
        // Salir del crouch
        if (!Context.WantsToCrouch)
        {
            // Decidir a qué estado ir
            if (Context.WantsToSprint && Context.PlayerCrouch.CanStandUp())
            {
                Context.StateMachine.ChangeState(MovementState.Sprinting);
            }
            else if (Context.PlayerCrouch.CanStandUp())
            {
                Context.StateMachine.ChangeState(MovementState.Walking);
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
    }

    public override void OnExit()
    {
        Context.PlayerCrouch.SetCrouching(false);
    }
}