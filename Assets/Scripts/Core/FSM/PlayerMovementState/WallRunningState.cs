using Core;
using UnityEngine;

public class WallRunningState : BaseState<PlayerMovementContext>
{
    public WallRunningState(PlayerMovementContext context) : base(context) { }

    public override void OnEnter()
    {
        Context.Controllable.SetMovementState(MovementState.WallRunning);
        Debug.Log("Entered Wall Running State");
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnExit()
    {}
}