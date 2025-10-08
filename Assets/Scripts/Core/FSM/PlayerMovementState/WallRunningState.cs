using Core;
using UnityEngine;

public class WallRunningState : BaseState<PlayerMovementContext>
{
    public WallRunningState(PlayerMovementContext context) : base(context) { }

    public override void OnEnter()
    {
        Context.Controllable.SetMovementState(MovementState.WallRunning);
        Context.PlayerWallRun.StartWallRun();
        Debug.Log("Entered Wall Running State");
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
            Context.StateMachine.ChangeState(MovementState.Walking);
            return;
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

        Debug.Log("Exited Wall Running State");
    }

    private void ExitToAppropriateState()
    {
        // Decidir a qué estado ir basado en los inputs del jugador
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
        else if (!Context.PlayerWallRun.CanWallRun() && !Context.WantsToSprint)
        {
            // Si está en el aire, volver a sprinting (mantendrá el estado hasta aterrizar)
            Context.StateMachine.ChangeState(Context.WantsToSprint ? MovementState.Sprinting : MovementState.Walking);
        }
    }
}