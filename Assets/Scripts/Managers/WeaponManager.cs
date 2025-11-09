using System;
using System.Collections.Generic;
using Commands;
using Strategies.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    public class WeaponManager : MonoBehaviour
    {
        public static WeaponManager Instance { get; private set; }

        public IGun EquippedGun => _guns[EquippedIndex];

        private int EquippedIndex { get; set; } = 0;
        private int NextIndex => (EquippedIndex + 1) % _guns.Count;
        private int PreviousIndex => EquippedIndex == 0 ? _guns.Count - 1 : EquippedIndex - 1;

        [SerializeField] private List<Gun> _guns;

        [SerializeField] private InputActionReference _shootAction;
        [SerializeField] private InputActionReference _reloadAction;
        [SerializeField] private InputActionReference _switchNextAction;
        [SerializeField] private InputActionReference _switchPreviousAction;

        private SwitchGunCommand _switchNextCommand;
        private SwitchGunCommand _switchPreviousCommand;
        private ShootCommand _shootCommand;
        private ReloadCommand _reloadCommand;

        public event Action<IGun> OnEquipped;
        public event Action<IGun> OnShot;
        public event Action<IGun> OnReloaded;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }

            Instance = this;
        }

        void Start()
        {
            _shootAction.action.started += OnShoot;
            _reloadAction.action.started += OnReload;
            _switchNextAction.action.performed += OnSwitchNext;
            _switchPreviousAction.action.performed += OnSwitchPrevious;

            _guns.ForEach((gun) => gun.UnEquip());

            _guns[EquippedIndex].Equip();

            CreateCommands();
            AttachListeners();
        }

        private void AttachListeners()
        {
            _guns[EquippedIndex].OnShot += AfterShoot;
            _guns[EquippedIndex].OnReloaded += AfterReload;
        }

        private void DetachListeners()
        {
            _guns[EquippedIndex].OnShot -= AfterShoot;
            _guns[EquippedIndex].OnReloaded -= AfterReload;
        }

        private void CreateCommands()
        {
            IGun nextGun = _guns[NextIndex];
            IGun previousGun = _guns[PreviousIndex];

            _switchNextCommand = new SwitchGunCommand(nextGun, EquippedGun, AfterSwitchNext);
            _switchPreviousCommand = new SwitchGunCommand(previousGun, EquippedGun, AfterSwitchPrevious);

            _shootCommand = new ShootCommand(EquippedGun);
            _reloadCommand = new ReloadCommand(EquippedGun);
        }

        private void AfterSwitchNext()
        {
            DetachListeners();

            EquippedIndex = NextIndex;

            CreateCommands();
            AttachListeners();

            OnEquipped?.Invoke(_guns[EquippedIndex]);
        }

        private void AfterSwitchPrevious()
        {
            DetachListeners();

            EquippedIndex = PreviousIndex;

            CreateCommands();
            AttachListeners();

            OnEquipped?.Invoke(_guns[EquippedIndex]);
        }

        private void AfterShoot()
        {
            OnShot?.Invoke(_guns[EquippedIndex]);
        }

        private void AfterReload()
        {
            OnReloaded?.Invoke(_guns[EquippedIndex]);
        }

        private void OnSwitchNext(InputAction.CallbackContext context)
        {
            CommandQueueManager.Instance.EnqueueCommand(_switchNextCommand);
        }

        private void OnSwitchPrevious(InputAction.CallbackContext context)
        {
            CommandQueueManager.Instance.EnqueueCommand(_switchPreviousCommand);
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