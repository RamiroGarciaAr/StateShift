using System.Collections.Generic;
using Core;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponStanceConfigSO", menuName = "Animations/Stance Config")]
public class WeaponStanceConfigSO : ScriptableObject
{
    [Header("Stance Configs List")]
    [SerializeField]
    List<StanceEntry> _stanceList = new List<StanceEntry>();

    [Header("Fallback SOD Pose Values")]
    [SerializeField]
    private float _fallbackPoseF;

    [SerializeField]
    private float _fallbackPoseZ;

    [SerializeField]
    private float _fallbackPoseR;

    public float FallbackPoseF => _fallbackPoseF;

    public float FallbackPoseZ => _fallbackPoseZ;

    public float FallbackPoseR => _fallbackPoseR;

    /// <summary>
    /// Returns the stance config matching the given movement state,
    /// or a default stance built from the fallback pose values.
    /// </summary>
    public WeaponStanceConfig GetStanceConfig(MovementState movementState)
    {
        foreach (var stance in _stanceList)
        {
            if (stance.movementState == movementState)
                return stance.stanceConfig;
        }
        // This is fine for now BUT we should consider caching this
        return new WeaponStanceConfig(
            Vector3.zero,
            Vector3.zero,
            _fallbackPoseF,
            _fallbackPoseZ,
            _fallbackPoseR
        );
    }
}
