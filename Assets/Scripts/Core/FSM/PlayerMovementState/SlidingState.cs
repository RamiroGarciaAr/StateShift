using Core;

public class SlidingState : BaseState<PlayerMovementContext>
    {
        public SlidingState(PlayerMovementContext context) : base(context) { }

        public override void OnEnter()
        {
            Context.Controllable.SetMovementState(MovementState.Sliding);
            Context.PlayerCrouch.SetCrouching(true);
        }

        public override void OnUpdate()
        {
            // Si termina el slide, ir a crouch si sigue presionado, o a walking
            if (!Context.PlayerSlide.IsSliding)
            {
                if (Context.WantsToCrouch)
                {
                    Context.StateMachine.ChangeState(MovementState.Crouching);
                }
                else
                {
                    Context.StateMachine.ChangeState(MovementState.Walking);
                }
                return;
            }

            // Cancelar slide si suelta el botón de crouch
            if (!Context.WantsToCrouch)
            {
                Context.PlayerSlide.CancelSlide();
                Context.StateMachine.ChangeState(MovementState.Walking);
                return;
            }

            // Cancelar slide si salta
            if (Context.WantsToJump)
            {
                Context.PlayerSlide.CancelSlide();
                Context.StateMachine.ChangeState(MovementState.Walking);
                return;
            }
        }
        public override void OnExit()
        {
            Context.PlayerSlide.EndSlide();
        }
    }
