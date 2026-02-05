using UnityEngine;

[CreateAssetMenu(menuName = "Health/Player Health Config")]
public class PlayerHealthConfigSO : ScriptableObject
{
    [Header("Main Chunk")]
    [SerializeField] private float mainChunkHealth = 100f;

    [Header("Side Chunks")]
    [SerializeField] private float sideChunkHealth = 20f;
    [SerializeField] private int sideChunkCount = 2;

    public float MainChunkHealth => mainChunkHealth;
    public float SideChunkHealth => sideChunkHealth;
    public int SideChunkCount => sideChunkCount;
    public float TotalMaxHealth => mainChunkHealth + (sideChunkHealth * sideChunkCount);
}
