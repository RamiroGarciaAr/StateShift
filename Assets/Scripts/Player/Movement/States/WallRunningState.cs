using Core;
using UnityEngine;

public class WallRunningState : BaseState<PlayerMovementContext>
{
    public WallRunningState(PlayerMovementContext context) : base(context) { }

    public override void OnEnter()
    {
        Context.Controllable.SetMovementState(MovementState.WallRunning);
        Context.PlayerWallRun.StartWallRun();
    }

    public override void OnUpdate()
    {
        // Salir si ya no está corriendo en la pared
        if (!Context.PlayerWallRun.IsWallRunning)
        {
            ExitToAppropriateState();
            return;
        }

        // Salir si toca el suelo
        if (Context.PlayerMovement.IsGrounded)
        {
            Context.PlayerWallRun.StopWallRun();
            Context.StateMachine.ChangeState(MovementState.Grounded);
            return;
        }
        // Transition to Grapple
        if (Context.WantsToGrapple && Context.PlayerGrapple.CanGrapple)
        {
            Context.PlayerWallRun.StopWallRun(); // Stop wallrun primero
            bool grappleStarted = Context.PlayerGrapple.TryStartGrapple();
            if (grappleStarted)
            {
                Context.StateMachine.ChangeState(MovementState.Grappling);
                return;
            }
        }
        // Salir si ya no hay pared
        if (!Context.PlayerWallRun.HasWall)
        {
            Context.PlayerWallRun.StopWallRun();
            ExitToAppropriateState();
            return;
        }

        // Manejar salto desde la pared
        if (Context.WantsToJump)
        {
            Context.PlayerWallRun.WallJump();
            ExitToAppropriateState();
            return;
        }

        // Salir si el jugador intenta agacharse (cancelar wallrun)
        if (Context.WantsToCrouch || !Context.WantsToSprint)
        {
            Context.PlayerWallRun.StopWallRun();
            ExitToAppropriateState();
            return;
        }
    }

    public override void OnExit()
    {
        // Asegurar que el wallrun termine limpiamente
        if (Context.PlayerWallRun.IsWallRunning)
        {
            Context.PlayerWallRun.StopWallRun();
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