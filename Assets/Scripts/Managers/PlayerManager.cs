using Core.Commands;
using Core.Strategies.Health;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(EquipmentManager), typeof(PlayerHealth))]
    public sealed class PlayerManager : MonoBehaviour
    {
        public EquipmentManager Equipment { get; private set; }
        public PlayerHealth PlayerHealth { get; private set; }

        private PlayerInputActions _playerInput;

        private void Awake()
        {
            _playerInput = new PlayerInputActions();

            Equipment = GetComponent<EquipmentManager>();
            PlayerHealth = GetComponent<PlayerHealth>();
        }

        private void OnEnable()
        {
            _playerInput.Player.Enable();
        }

        private void Start()
        {
            GameManager.Instance.SetPlayerInstance(this);

            _playerInput.Player.Shoot.started += OnShoot;
            _playerInput.Player.Reload.started += OnReload;
            _playerInput.Player.NextWeapon.started += OnSwitchNextGun;
            _playerInput.Player.PreviousWeapon.started += OnSwitchPreviousGun;
        }

        private void OnShoot(InputAction.CallbackContext ctx)
        {
            ShootCommand command = new(Equipment.EquippedGun);
            CommandQueueManager.Instance.EnqueueCommand(command);
        }

        private void OnReload(InputAction.CallbackContext ctx)
        {
            ReloadCommand command = new(Equipment.EquippedGun);
            CommandQueueManager.Instance.EnqueueCommand(command);
        }

        private void OnSwitchNextGun(InputAction.CallbackContext ctx)
        {
            SwitchNextGunCommand command = new(Equipment);
            CommandQueueManager.Instance.EnqueueCommand(command);
        }

        private void OnSwitchPreviousGun(InputAction.CallbackContext ctx)
        {
            SwitchPreviousGunCommand command = new(Equipment);
            CommandQueueManager.Instance.EnqueueCommand(command);
        }
    }
}