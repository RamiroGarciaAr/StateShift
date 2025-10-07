using Core;


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
                if (Context.WantsToSprint)
                {
                    Context.StateMachine.ChangeState(MovementState.Sprinting);
                }
                else
                {
                    Context.StateMachine.ChangeState(MovementState.Walking);
                }
                return;
            }
        }

        public override void OnExit()
        {
            Context.PlayerCrouch.SetCrouching(false);
        }
    }