using UnityEngine;
using UnityEngine.AI;

public class SuspiciousState : AlertState
{
    private enum Phase
    {
        GoToPoint,
        Scanning,
    }

    private float _searchTimer;
    private Phase _phase;
    private float _scanBaseYaw;
    private float _sweepPhase;
    private float _scanTimer;

    private Vector3 _searchCenter; // Tier 2: expansion pivots around here
    private float _searchRadius;

    // TODO: promote to serialized Metrics on the brain for the tuning pass.
    private const float LeadTime = 0.5f;
    private const float ScanAngle = 60f;
    private const float SweepSpeed = 2f;
    private const float SampleRadius = 3f;
    private const float ScanDuration = 2f; // seconds spent scanning before expanding
    private const float ExpandStep = 4f; // how much wider each expansion ring is
    private const int ExpandTries = 6; // angles to try when finding a ring point

    public SuspiciousState(AIBrain brain)
        : base(brain) { }

    public float ElapsedSearch => _searchTimer;

    public override void OnEnter()
    {
        Context.Memory.SetAlertLevel(AIMemory.AlertLevel.Suspicious);
        _searchTimer = 0f;
        _sweepPhase = 0f;
        _searchRadius = 0f;
        _phase = Phase.GoToPoint;

        // Tier 1: project along last-known velocity.
        Vector3 projected =
            Context.Memory.LastKnownPlayerPosition + Context.Memory.LastKnownVelocity * LeadTime;

        if (NavMesh.SamplePosition(projected, out NavMeshHit hit, SampleRadius, NavMesh.AllAreas))
            projected = hit.position;
        else
            projected = Context.Memory.LastKnownPlayerPosition;

        _searchCenter = projected;
        Context.NavMove.SetDestination(projected);
    }

    public override void OnUpdate()
    {
        _searchTimer += Context.TickDelta;

        switch (_phase)
        {
            case Phase.GoToPoint:
                if (Context.NavMove.IsIdle)
                {
                    _phase = Phase.Scanning;
                    _scanBaseYaw = Context.transform.eulerAngles.y;
                    _sweepPhase = 0f;
                    _scanTimer = 0f;
                }
                break;

            case Phase.Scanning:
                // Sweep the cone.
                _sweepPhase += Context.TickDelta * SweepSpeed;
                float offset = Mathf.Sin(_sweepPhase) * ScanAngle;
                Quaternion rot = Quaternion.Euler(0f, _scanBaseYaw + offset, 0f);
                Vector3 lookPoint = Context.transform.position + rot * Vector3.forward * 10f;
                Context.Aiming.SetTarget(lookPoint);

                // Tier 2: scanned long enough here and found nothing — expand.
                _scanTimer += Context.TickDelta;
                if (_scanTimer >= ScanDuration)
                    ExpandSearch();
                break;
        }
    }

    // Pick a wider point around the search center, biased toward where the player fled.
    private void ExpandSearch()
    {
        _searchRadius += ExpandStep;

        // Bias the first probe toward last-known velocity, then fan out.
        Vector3 fleeDir = Context.Memory.LastKnownVelocity;
        float baseAngle =
            fleeDir.sqrMagnitude > 0.01f
                ? Mathf.Atan2(fleeDir.x, fleeDir.z) * Mathf.Rad2Deg
                : Random.Range(0f, 360f);

        for (int i = 0; i < ExpandTries; i++)
        {
            // Alternate outward from the flee direction: 0, +60, -60, +120, -120...
            float ang = baseAngle + ((i + 1) / 2) * 60f * (i % 2 == 0 ? 1 : -1);
            float rad = ang * Mathf.Deg2Rad;
            Vector3 probe =
                _searchCenter + new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * _searchRadius;

            if (NavMesh.SamplePosition(probe, out NavMeshHit hit, SampleRadius, NavMesh.AllAreas))
            {
                Context.NavMove.SetDestination(hit.position);
                _phase = Phase.GoToPoint;
                return;
            }
        }

        // No valid ring point found (walls all around) — just keep scanning here.
        // The brain's give-up timer will end the search regardless.
        _scanTimer = 0f;
    }
}
