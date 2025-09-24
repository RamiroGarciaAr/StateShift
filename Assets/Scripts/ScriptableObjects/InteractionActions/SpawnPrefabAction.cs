using UnityEngine;

[CreateAssetMenu(menuName="Interactions/Spawn Prefab")]
public class SpawnPrefabAction : InteractionAction {
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawnPoint;
    public override void Execute(GameObject instigator) {
        if (!prefab) return;
        var p = spawnPoint ? spawnPoint : instigator.transform;
        Instantiate(prefab, p.position, p.rotation);
    }
}