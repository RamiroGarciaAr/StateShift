using System.Collections.Generic;
using UnityEngine;

public class AITickManager : Singleton<AITickManager>
{
    [Header("Tick Settings")]
    [Tooltip("The number of AI ticks to process per frame. " +
             "Lower values can improve performance but may result in less responsive AI behavior.")]

    [SerializeField][Range(1, 5)] private int _ticksPerFrame = 1;

    private readonly List<ITickable> _agents = new List<ITickable>();
    private int _currentIndex = 0;
    private bool _isPaused = false;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {

        if (EventsManager.Instance == null)
        {
            Debug.LogWarning("[AITickManager] EventsManager not found Pause/GameOver integration disabled.");
            return;
        }

        EventsManager.Instance.OnGamePause += HandleGamePause;
        EventsManager.Instance.OnGameOver += HandleGameOver;
    }

    protected override void OnDestroy()
    {
        if (EventsManager.Instance == null) return; // Check if EventsManager still exists before unsubscribing

        EventsManager.Instance.OnGamePause -= HandleGamePause;
        EventsManager.Instance.OnGameOver -= HandleGameOver;
    }

    private void Update()
    {
        if (_isPaused || _agents.Count == 0) return;
    
        for (int i = 0; i < _ticksPerFrame; i++)
        {
            TickNextAgent();
        }
    }

    private void TickNextAgent()
    {
        if (_agents.Count == 0) return;

        _currentIndex = _currentIndex % _agents.Count; // Ensure index is within bounds

        ITickable agent = _agents[_currentIndex];

        if (agent.IsTickable) agent.OnTick(Time.deltaTime);

        _currentIndex++;
    }

    public void RegisterAgent(ITickable agent)
    {
        if (!_agents.Contains(agent))
        {
            _agents.Add(agent);
        }
    }

    public void UnregisterAgent(ITickable agent)
    {
        int index = _agents.IndexOf(agent);

        if (index < 0) return;

        if (index < _currentIndex)
        {
            _currentIndex--; // Adjust current index if the removed agent is before it
        }
        _agents.RemoveAt(index);
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
        Debug.Log($"$[AI Tick Manager] Registered Agents ({_agents.Count}):");
        foreach (var agent in _agents)
        {
            Debug.Log($" - {agent}");
        }
    }
}
