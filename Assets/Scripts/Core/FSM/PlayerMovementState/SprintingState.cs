using Core;

public class SprintingState : BaseState<PlayerMovementContext>
{
    public SprintingState(PlayerMovementContext context) : base(context){}

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
                    Context.StateMachine.ChangeState(MovementState.Sliding);
                }
                else
                {
                    // Si no tiene velocidad suficiente, ir directo a crouch
                    Context.StateMachine.ChangeState(MovementState.Crouching);
                }
                return;
            }

            // Transición a Walking
            if (!Context.WantsToSprint)
            {
                Context.StateMachine.ChangeState(MovementState.Walking);
                return;
            }
        }

}
