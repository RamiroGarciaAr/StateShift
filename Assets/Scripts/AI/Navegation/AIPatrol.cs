using UnityEngine;

public class AIPatrol : MonoBehaviour
{
    [SerializeField, Tooltip("Default patrol route. Null = stationary guard (no patrol).")]
    private PatrolRoute _route;

    private int _currentIndex;
    private int _direction = 1; // PingPong travel direction; owned per-bot

    public bool HasRoute => _route != null && _route.Count > 0;

    public Vector3 CurrentPoint => HasRoute ? _route.GetPoint(_currentIndex) : transform.position;

    // Advance to the next waypoint per the route's mode.
    public void Advance()
    {
        if (!HasRoute)
            return;
        _currentIndex = _route.NextIndex(_currentIndex, ref _direction);
    }

    // True when a Once-mode route has reached its end and the bot should hold.
    public bool AtEnd => HasRoute && _route.IsTerminal(_currentIndex);

    // Expansion seam: an encounter trigger can swap the route later.
    // AIPatrol never needs to know about "multiple routes" — the decider calls this.
    public void SetRoute(PatrolRoute route)
    {
        _route = route;
        _currentIndex = 0;
        _direction = 1;
    }
}
