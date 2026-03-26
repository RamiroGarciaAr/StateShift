using System.Collections.Generic;
using UnityEngine;
using Health;

[CreateAssetMenu(menuName = "Health/Enemy Health Config")]
public class EnemyHealthConfigSO : ScriptableObject
{
    [SerializeField] private List<HealthChunkData> chunks = new();

    public IReadOnlyList<HealthChunkData> Chunks => chunks;
}
