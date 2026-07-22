using System.Collections.Generic;
using UnityEngine;

public enum PatrolMode
{
    Loop, // 0,1,2,0,1,2...
    PingPong, // 0,1,2,1,0,1,2...
    Once, // 0,1,2 then hold on the last point
}

public class PatrolRoute : MonoBehaviour
{
    [SerializeField, Tooltip("How the route is traversed.")]
    private PatrolMode _mode = PatrolMode.Loop;

    [Header("Gizmos")]
    [SerializeField]
    private Color _gizmoColor = new(0.2f, 0.8f, 1f, 1f);

    [SerializeField]
    private float _pointRadius = 0.25f;

    // Child transforms ARE the waypoints, in hierarchy order. No manual list to maintain.
    private readonly List<Transform> _points = new();

    public PatrolMode Mode => _mode;
    public int Count
    {
        get
        {
            CollectPoints();
            return _points.Count;
        }
    }

    private void Awake()
    {
        CollectPoints();
    }

    // Rebuilds the point list from direct children. Cheap; called on access so the
    // route stays correct even if children are added/removed at edit time.
    private void CollectPoints()
    {
        _points.Clear();
        for (int i = 0; i < transform.childCount; i++)
            _points.Add(transform.GetChild(i));
    }

    public Vector3 GetPoint(int index)
    {
        CollectPoints();
        if (_points.Count == 0)
            return transform.position; // no points: fall back to the route origin
        index = Mathf.Clamp(index, 0, _points.Count - 1);
        return _points[index].position;
    }

    // Given the current index and travel direction, returns the next index.
    // 'direction' is only meaningful for PingPong and is updated by ref (the BOT owns it,
    // so two bots can walk one route independently).
    public int NextIndex(int current, ref int direction)
    {
        CollectPoints();
        int count = _points.Count;
        if (count <= 1)
            return 0;

        switch (_mode)
        {
            case PatrolMode.Loop:
                return (current + 1) % count;

            case PatrolMode.Once:
                return Mathf.Min(current + 1, count - 1); // clamp: bot holds at the end

            case PatrolMode.PingPong:
                int next = current + direction;
                if (next >= count || next < 0)
                {
                    direction = -direction; // bounce off the end
                    next = current + direction;
                }
                return next;

            default:
                return current;
        }
    }

    // True once a bot on this route should stop advancing (Once mode, reached the end).
    public bool IsTerminal(int index)
    {
        return _mode == PatrolMode.Once && index >= Count - 1;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw straight from children so the path updates live as you move points,
        // without relying on the runtime-collected list.
        int count = transform.childCount;
        if (count == 0)
            return;

        Gizmos.color = _gizmoColor;

        for (int i = 0; i < count; i++)
        {
            Vector3 p = transform.GetChild(i).position;
            Gizmos.DrawSphere(p, _pointRadius);
            UnityEditor.Handles.Label(p + Vector3.up * (_pointRadius + 0.2f), i.ToString());

            // Connect to the next point per mode.
            if (i < count - 1)
                Gizmos.DrawLine(p, transform.GetChild(i + 1).position);
        }

        // Closing line back to start, only for Loop (shows it's a cycle).
        if (_mode == PatrolMode.Loop && count > 1)
            Gizmos.DrawLine(transform.GetChild(count - 1).position, transform.GetChild(0).position);
    }
#endif
}
