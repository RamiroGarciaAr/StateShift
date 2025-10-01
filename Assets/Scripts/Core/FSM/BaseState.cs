using UnityEngine;

public abstract class BaseState<TContext> : IState where TContext : class
{
    protected TContext Context { get; private set; }

    protected BaseState(TContext context)
    {
        Context = context;
    }
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnFixedUpdate();
    public abstract void OnExit();
    protected void Log(string message)
    {
        Debug.Log($"[{GetType().Name}] {message}");
    }
}

