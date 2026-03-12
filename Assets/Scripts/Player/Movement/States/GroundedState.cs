using Core;

public class GroundedState : BaseState<PlayerMovementContext>
{
    private readonly StateMachine<MovementState> _innerMachine;

    public MovementState CurrentSubState => _innerMachine.CurrentStateType;

    public GroundedState(PlayerMovementContext context) : base(context)
    {
        _innerMachine = new StateMachine<MovementState>();
        _innerMachine.RegisterState(MovementState.Walking, new WalkingState(context));
        _innerMachine.RegisterState(MovementState.Sprinting, new SprintingState(context));
        _innerMachine.RegisterState(MovementState.Crouching, new CrouchingState(context));
        _innerMachine.RegisterState(MovementState.Sliding, new SlidingState(context));

        context.GroundedStateMachine = _innerMachine;
    }

    public override void OnEnter()
    {
        _innerMachine.ChangeState(PickInitialSubState());
    }

    public override void OnUpdate()
    {
        if (Context.WantsToGrapple && Context.PlayerGrapple.CanGrapple)
        {
            if (Context.PlayerGrapple.TryStartGrapple())
            {
                Context.StateMachine.ChangeState(MovementState.Grappling);
                return;
            }
        }

        if (Context.WantsToDash)
        {
            if (Context.PlayerDash.TryStartDash(Context.DashInputDirection))
            {
                Context.StateMachine.ChangeState(MovementState.Dashing);
                Context.WantsToDash = false;
                return;
            }
        }

        _innerMachine.Update();
    }

    public override void OnFixedUpdate()
    {
        _innerMachine.FixedUpdate();
    }

    public override void OnExit()
    {
        _innerMachine.ExitCurrentState();
    }

    private MovementState PickInitialSubState()
    {
        if (Context.WantsToSprint) return MovementState.Sprinting;
        if (Context.WantsToCrouch) return MovementState.Crouching;
        return MovementState.Walking;
    }
}
