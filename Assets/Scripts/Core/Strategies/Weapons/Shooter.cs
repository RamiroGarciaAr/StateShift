using Commands;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Strategies.Weapons
{
    public class Shooter : MonoBehaviour
    {
        [SerializeField] private Gun _gun;

        [SerializeField] InputActionReference _shootAction;
        [SerializeField] InputActionReference _reloadAction;

        private ShootCommand _shootCommand;
        private ReloadCommand _reloadCommand;

        private void Start()
        {
            _shootAction.action.performed += OnShoot;
            _reloadAction.action.performed += OnReload;

            _shootCommand = new ShootCommand(_gun);
            _reloadCommand = new ReloadCommand(_gun);
        }

        private void OnShoot(InputAction.CallbackContext context)
        {
            CommandQueueManager.Instance.EnqueueCommand(_shootCommand);
        }

        private void OnReload(InputAction.CallbackContext context)
        {
            CommandQueueManager.Instance.EnqueueCommand(_reloadCommand);
        }
    }
}