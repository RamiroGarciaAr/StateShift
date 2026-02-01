using Core.Commands;
using Core.Strategies.Weapons;
using UnityEngine;

namespace Entities.Controllers
{
    [RequireComponent(typeof(ITargetFinder))]
    public class TurretController : MonoBehaviour
    {
        [SerializeField] private Transform _turretHead;
        [SerializeField] private Gun _gun;
        [SerializeField] private float _rotationSpeed = 20f;
        [SerializeField] private float _firingAngleTolerance = 5f;

        private ITargetFinder _targetFinder;

        private ShootCommand _shootCommand;
        private ReloadCommand _reloadCommand;

        private Vector3 _targetDirection;
        private bool _targetVisible = false;

        void Awake()
        {
            _targetFinder = GetComponent<ITargetFinder>();
            _targetDirection = _turretHead.forward;

            _shootCommand = new ShootCommand(_gun);
            _reloadCommand = new ReloadCommand(_gun);
        }

        void Update()
        {
            FindTarget();
            RotateToTarget();
            FireIfLookingAtTarget();
            ReloadIfMagazineEmpty();
        }

        private void FindTarget()
        {
            if (_targetFinder.TryFindTarget(_turretHead.position, out Vector3 target))
            {
                _targetDirection = target - _turretHead.position;
                _targetVisible = true;
            }
            else
            {
                _targetVisible = false;
            }
        }

        private void RotateToTarget()
        {
            Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);

            float angle = Vector3.Angle(_turretHead.forward, _targetDirection);
            float interpolationRatio = _rotationSpeed / angle * Time.deltaTime;

            Quaternion interpolatedRotation = Quaternion.Lerp(_turretHead.rotation, targetRotation, interpolationRatio);

            RotateCommand command = new(_turretHead, interpolatedRotation);
            CommandQueueManager.Instance.EnqueueCommand(command);
        }

        private void FireIfLookingAtTarget()
        {
            float angle = Vector3.Angle(_turretHead.forward, _targetDirection);

            if (_targetVisible && angle < _firingAngleTolerance)
            {
                CommandQueueManager.Instance.EnqueueCommand(_shootCommand);
            }
        }

        private void ReloadIfMagazineEmpty()
        {
            if (_gun.AmmoOnMagazine == 0 && _gun.AmmoOnReserve > 0)
            {
                CommandQueueManager.Instance.EnqueueCommand(_reloadCommand);
            }
        }
    }
}