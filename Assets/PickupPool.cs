using UnityEngine;
using UnityEngine.Pool;

public class PickupPool : MonoBehaviour
{
    [SerializeField]
    private AudioPool audioPool; // wired in scene; passed through to pickups

    [SerializeField, Tooltip("Layers treated as ground for drop placement")]
    private LayerMask _groundMask;

    [SerializeField, Tooltip("How far up from the corpse to start the ground check")]
    private float _castStartHeight = 1f;

    [SerializeField, Tooltip("Lift off the ground so the pickup doesn't clip into it")]
    private float _groundOffset = 0.3f;

    [SerializeField]
    private PickupTrigger _prefab;

    [SerializeField]
    private int _defaultCapacity = 5,
        _maxCapacity = 20;
    private ObjectPool<PickupTrigger> _pool;

    private void Awake()
    {
        if (_prefab == null)
        {
            Debug.LogError($"[PickupPool] No prefab on {name}", this);
            enabled = false;
            return;
        }
        _pool = new ObjectPool<PickupTrigger>(
            createFunc: () =>
            {
                var p = Instantiate(_prefab);
                p.SetReleaseCallback(Release); // ← the wire: pickup learns how to return
                p.GetComponent<IPickupEffect>()?.SetAudioPool(audioPool);
                return p;
            },
            actionOnGet: p => p.gameObject.SetActive(true),
            actionOnRelease: p => p.gameObject.SetActive(false),
            actionOnDestroy: p => Destroy(p.gameObject),
            collectionCheck: false,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxCapacity
        );
    }

    public PickupTrigger Spawn(Vector3 origin)
    {
        Vector3 spawnPos = origin;

        // Cast down from slightly above the origin to find the floor.
        Vector3 castStart = origin + Vector3.up * _castStartHeight;
        if (Physics.Raycast(castStart, Vector3.down, out RaycastHit hit, 20f, _groundMask))
            spawnPos = hit.point + Vector3.up * _groundOffset;
        // else: no ground found — fall back to the origin as given

        var pickup = _pool.Get();
        pickup.transform.position = spawnPos;
        return pickup;
    }

    private void Release(PickupTrigger p) => _pool.Release(p);
}
