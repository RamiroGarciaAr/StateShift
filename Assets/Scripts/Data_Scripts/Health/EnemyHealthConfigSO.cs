using System.Collections.Generic;
using Health;
using UnityEngine;

[CreateAssetMenu(menuName = "Health/Enemy Health Config")]
public class EnemyHealthConfigSO : ScriptableObject
{
    [SerializeField]
    private List<HealthChunkData> chunks = new();

    [SerializeField]
    private List<HealthModifierData> healthModifier;

    public IReadOnlyList<HealthModifierData> HealthModifier => healthModifier;
    public IReadOnlyList<HealthChunkData> Chunks => chunks;
}

// Even though we could use a dictionary here I like the control that this stuct gives me
// So I will leave it like this
[System.Serializable]
public struct HealthModifierData
{
    [Range(0.1f, 10f)]
    public float modifier;
    public BodyPart bodyPart;
}
