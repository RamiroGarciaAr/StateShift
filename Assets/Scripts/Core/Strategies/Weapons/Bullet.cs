using Commands;
using Strategies.Health;
using UnityEngine;

namespace Strategies.Weapons
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class Bullet : MonoBehaviour, IBullet
    {
        public BulletProperties Properties => _properties;
        [SerializeField] private BulletProperties _properties;

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

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IBullet>(out var _))
            {
                return;
            }

            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                ICommand command = new DamageCommand(damageable, Properties.Damage);
                CommandQueueManager.Instance.EnqueueCommand(command);
            }

            gameObject.SetActive(false);
        }
    }
}