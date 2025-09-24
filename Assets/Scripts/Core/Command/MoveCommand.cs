using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class MoveCommand : ICommand
{
    private readonly IMovable movable;
    public Vector3 moveDir;
    public float maxSpeed;

    public MoveCommand(IMovable movable)
    {
        this.movable = movable;
    } 
    public void Execute()
    {
        movable.Move(moveDir, maxSpeed);
    }

    public void Undo()
    {
        throw new System.NotImplementedException();
    }
}
