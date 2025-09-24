public sealed class JumpCommand : ICommand
{
    private readonly IJump jumpable;

    public JumpCommand(IJump jumpable)
    {
        this.jumpable = jumpable;
    }

    public void Execute() => jumpable.Jump();

    public void Undo()
    {
        throw new System.NotImplementedException();
    }
}