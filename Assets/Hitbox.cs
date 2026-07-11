using Health;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hitbox : MonoBehaviour
{
    [SerializeField]
    private BodyPart bodyPart;
    private IDamageable _target;

    void Awake() => _target = GetComponentInParent<IDamageable>();
}
