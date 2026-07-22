using System.Collections.Generic;
using UnityEngine;

public class AITickManager : Singleton<AITickManager>
{
    [Header("Tick Settings")]
    [Tooltip(
        "The number of AI ticks to process per frame. "
            + "Lower values can improve performance but may result in less responsive AI behavior."
    )]
    [SerializeField, Range(1, 5)]
    private int _ticksPerFrame = 1;

    private readonly List<ITickable> _agents = new();
    private readonly List<float> _lastTickTime = new(); // parallel to _agents, same index

    private readonly List<ITickable> _pendingAdd = new();
    private readonly List<ITickable> _pendingRemove = new();

    private int _currentIndex = 0;
    private bool _isPaused = false;

    private void Start()
    {
        if (EventsManager.Instance == null)
        {
            Debug.LogWarning(
                "[AITickManager] EventsManager not found. Pause/GameOver integration disabled."
            );
            return;
        }

        EventsManager.Instance.OnGamePause += HandleGamePause;
        EventsManager.Instance.OnGameOver += HandleGameOver;
    }

    protected override void OnDestroy()
    {
        if (EventsManager.Instance == null)
            return;

        EventsManager.Instance.OnGamePause -= HandleGamePause;
        EventsManager.Instance.OnGameOver -= HandleGameOver;
    }

    private void Update()
    {
        if (_isPaused)
            return;

        DrainPending();

        if (_agents.Count == 0)
            return;

        _currentIndex %= _agents.Count; // drain may have stranded the cursor past the end

        int ticked = 0;
        int inspected = 0;
        while (ticked < _ticksPerFrame && inspected < _agents.Count)
        {
            int idx = _currentIndex;
            ITickable agent = _agents[idx];
            _currentIndex = (_currentIndex + 1) % _agents.Count;
            inspected++;

            if (agent.IsTickable)
            {
                // True elapsed time since THIS agent was last ticked — not one frame.
                float now = Time.time;
                float agentDelta = now - _lastTickTime[idx];
                _lastTickTime[idx] = now;

                agent.OnTick(agentDelta);
                ticked++;
            }
        }
    }

    public void RegisterAgent(ITickable agent)
    {
        if (_agents.Contains(agent) || _pendingAdd.Contains(agent))
            return;

        _pendingAdd.Add(agent);
    }

    public void UnregisterAgent(ITickable agent)
    {
        _pendingAdd.Remove(agent);
        if (!_pendingRemove.Contains(agent))
            _pendingRemove.Add(agent);
    }

    private void DrainPending()
    {
        foreach (var agent in _pendingRemove)
        {
            int idx = _agents.IndexOf(agent);
            if (idx < 0)
                continue;

            if (idx < _currentIndex)
                _currentIndex--;

            _agents.RemoveAt(idx);
            _lastTickTime.RemoveAt(idx); // keep the parallel list aligned
        }

        foreach (var agent in _pendingAdd)
        {
            _agents.Add(agent);
            _lastTickTime.Add(Time.time); // init to NOW, not 0 — avoids a first-tick spike
        }

        _pendingAdd.Clear();
        _pendingRemove.Clear();
    }

    private void HandleGamePause(bool isPaused)
    {
        _isPaused = isPaused;
    }

    private void HandleGameOver()
    {
        _isPaused = true;
    }

    [ContextMenu("Log Registered Agents")]
    private void LogRegisteredAgents()
    {
        Debug.Log($"[AITickManager] Registered Agents ({_agents.Count}):");
        foreach (var agent in _agents)
        {
            Debug.Log($" - {agent}");
        }
    }
}
