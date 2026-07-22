using UnityEngine;

// Coordinates 4 legs into a diagonal-pair gait.
// Pair A = FL + BR, Pair B = FR + BL. Only one pair steps at a time, so at least
// two feet are always planted (stable). A pair steps together when it's that pair's
// turn AND a leg in it wants to step.
public class SpiderGait : MonoBehaviour
{
    [Header("Diagonal pairs")]
    [SerializeField]
    private IKFootStepper _legFL;

    [SerializeField]
    private IKFootStepper _legBR; // pair A with FL

    [SerializeField]
    private IKFootStepper _legFR;

    [SerializeField]
    private IKFootStepper _legBL; // pair B with FR

    private IKFootStepper[] _pairA;
    private IKFootStepper[] _pairB;

    [Header("Shared leg tuning (applied to all 4)")]
    [SerializeField]
    private float _stepThreshold = 0.4f;

    [SerializeField]
    private float _overshoot = 0.3f;

    [SerializeField]
    private float _stepDuration = 0.15f;

    [SerializeField]
    private float _stepHeight = 0.3f;

    private void Awake()
    {
        if (_legFL == null || _legBR == null || _legFR == null || _legBL == null)
        {
            Debug.LogError($"[SpiderGait] Missing leg refs on {name}", this);
            enabled = false;
            return;
        }
        _pairA = new[] { _legFL, _legBR };
        _pairB = new[] { _legFR, _legBL };

        ApplyConfigToAll();
    }

    private void ApplyConfigToAll()
    {
        foreach (var leg in new[] { _legFL, _legBR, _legFR, _legBL })
            leg.Configure(_stepThreshold, _overshoot, _stepDuration, _stepHeight);
    }

    private void LateUpdate()
    {
        // A pair may step only if the OTHER pair is fully planted.
        bool aStepping = AnyStepping(_pairA);
        bool bStepping = AnyStepping(_pairB);

        if (!bStepping && AnyWants(_pairA))
            StepPair(_pairA);
        else if (!aStepping && AnyWants(_pairB))
            StepPair(_pairB);
    }

    private bool AnyStepping(IKFootStepper[] pair)
    {
        return pair[0].IsStepping || pair[1].IsStepping;
    }

    private bool AnyWants(IKFootStepper[] pair)
    {
        return pair[0].WantsToStep || pair[1].WantsToStep;
    }

    // Step both legs of a pair together (diagonal legs move as one).
    private void StepPair(IKFootStepper[] pair)
    {
        foreach (var leg in pair)
            if (!leg.IsStepping)
                leg.BeginStep();
    }
}
