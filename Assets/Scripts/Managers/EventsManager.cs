using System;
using UnityEngine;

public class EventsManager : MonoBehaviour
{
    public static EventsManager Instance { get; private set; }

    public event Action<bool> OnGamePause;
    public event Action OnGameOver;
    public event Action OnGameExit;
    public event Action OnGameRestart;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }

        Instance = this;
    }

    public void ActionGamePause(bool isPaused) => OnGamePause?.Invoke(isPaused);
    public void ActionGameOver() => OnGameOver?.Invoke();
    public void ActionGameExit() => OnGameExit?.Invoke();
    public void ActionGameRestart() => OnGameRestart?.Invoke();
}