using Flyweight.Stats;
using UnityEngine;

namespace Core.Strategies.Weapons
{
    [RequireComponent(typeof(Rigidbody))]
    public abstract class Projectile : MonoBehaviour, IProjectile
    {
        public ProjectileProperties Properties => _properties;
        [SerializeField] private ProjectileProperties _properties;

        private Rigidbody _rigidbody;
        private float _timeRemaining;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _rigidbody.useGravity = Properties.UseGravity;
        }

        private void OnEnable()
        {
            _rigidbody.velocity = transform.forward * Properties.Velocity;
            _timeRemaining = Properties.FlightTime;
        }

        private void Update()
        {
            if (_timeRemaining > 0)
            {
                _timeRemaining -= Time.deltaTime;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            OnProjectileCollision(collision);

            gameObject.SetActive(false);
        }

        public abstract void OnProjectileCollision(Collision other);
        public abstract void OnProjectileStop();
    }
}