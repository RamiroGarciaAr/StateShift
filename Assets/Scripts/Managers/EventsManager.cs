using System;
using UnityEngine;

public class EventsManager : Singleton<EventsManager>
{

    public event Action<bool> OnGamePause;
    public event Action OnGameOver;
    public event Action OnGameExit;
    public event Action OnGameRestart;

    protected override void Awake()
    {
        base.Awake();
    }

    public void ActionGamePause(bool isPaused) => OnGamePause?.Invoke(isPaused);
    public void ActionGameOver() => OnGameOver?.Invoke();
    public void ActionGameExit() => OnGameExit?.Invoke();
    public void ActionGameRestart() => OnGameRestart?.Invoke();
}