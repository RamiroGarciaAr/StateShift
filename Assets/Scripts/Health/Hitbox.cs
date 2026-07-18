using Health;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    [SerializeField]
    private BodyPart bodyPart;
    private IDamageable _target;

    public BodyPart BodyPart => bodyPart;
    public IDamageable Damageable => _target;

    void Awake()
    {
        _target = GetComponentInParent<IDamageable>();
        if (_target == null)
            Debug.LogError("[Hitbox] No IDamageable found in parents of:", this); // maybe we should mention the parent too...just saying
    }
}
