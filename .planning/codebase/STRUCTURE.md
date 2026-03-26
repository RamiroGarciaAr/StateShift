# Codebase Structure

**Analysis Date:** 2026-03-26

## Directory Layout

```
Assets/Scripts/
├── AI/                         # AI tick scheduling, perception, memory, test agent
├── Audio_Scripts/
│   └── SFX/                    # Sound data class
├── Combat/
│   ├── Interfaces/             # IWeapon
│   └── Weapons/                # WeaponBase, Pistol, Shotgun, WeaponInventory
├── Core/
│   ├── Command/                # UNUSED scaffolding — CommandInvoker, JumpCommand, MoveCommand
│   ├── FSM/                    # Generic StateMachine<T>, BaseState<T>, IState
│   └── Singleton/              # Singleton<T> base class
├── Data_Scripts/
│   ├── Combat/                 # WeaponDataSO
│   └── Health/                 # DamageMatrixSO, PlayerHealthConfigSO, EnemyHealthConfigSO
├── Environment/
│   └── Platforms/              # Moving platform system (PlatformMover, PlatformRider, etc.)
├── Health/
│   ├── Components/             # BaseHealth, PlayerHealth, EnemyHealth, PlayerAdrenaline
│   ├── Data/                   # DamageInfo, HealthChangedEventArgs, HealthChunkData
│   ├── Interfaces/             # IDamageModifier, IHealable, IHealth
│   ├── Tests/
│   │   └── Editor/             # HealthSystemTest (Editor-only test runner)
│   └── Types/                  # DamageType enum, HealthType enum
├── Interactables/              # IInteractable interface
├── Managers/                   # EventsManager, GameManager, AudioManager, SceneLoader,
│   │                           #   LoadingManager, UIAnimationManager
│   └── AITickManager.cs        # (lives in Managers/, also in AI/)
├── Player/
│   ├── Camera/                 # CameraController, CameraMouseMovement, DynamicFOV, WallRunCameraEffects
│   ├── Events/                 # PlayerJumpingEvent, PlayerLandingEvent (data classes)
│   ├── Interfaces/             # IControllable, IController, IJumpable, IMovable
│   ├── Movement/
│   │   ├── States/             # All outer+inner FSM state classes
│   │   └── Strategies/         # UNUSED partial-refactor strategy interfaces+implementations
│   └── PlayerInput.cs          # Unity Input System reader + FSM host
├── Shaders/
│   └── UI/                     # UI shader helpers
├── Testing/                    # In-build test scripts (NOT Editor folder)
│   └── BulletTestSpawner, DamageTestTarget, NavMeshTester, SpectatorCamera
├── UI/
│   ├── HUD/                    # CrosshairBloomController, SpeedLinesController, WeaponHUDController
│   ├── Menus/                  # GameOverMenu, MainMenu, PauseMenu
│   └── Shared/                 # ButtonTextColorChanger, TriggerMapping
└── Utilities/                  # (directory exists, no .cs files at analysis time)
```

---

## Directory Purposes

**`Core/FSM/`:**
- Purpose: The generic FSM engine. No game-specific code lives here.
- Key files: `StateMachine.cs`, `BaseState.cs`, `IState.cs`
- Rule: Do not add player-specific logic. These classes are reusable by any system.

**`Core/Singleton/`:**
- Purpose: `Singleton<T>` base class only.
- Key file: `Singleton.cs`

**`Core/Command/`:**
- Purpose: Unused scaffolding. `CommandInvoker`, `JumpCommand`, `MoveCommand` all throw `NotImplementedException`.
- Status: Dead code — do not extend until the system is designed.

**`Player/Movement/States/`:**
- Purpose: Concrete FSM state implementations for the player locomotion machine.
- Contains: `GroundedState`, `InAirState`, `WallRunningState`, `DashingState`, `GrapplingState`, `WalkingState`, `SprintingState`, `CrouchingState`, `SlidingState`
- Convention: Every file is a single state class inheriting `BaseState<PlayerMovementContext>`.
- All transitions are self-contained inside `OnUpdate()` of each state — call `Context.StateMachine.ChangeState()` directly.

**`Player/Movement/Strategies/`:**
- Purpose: Partial refactor remnants. `IMovementStrategy`, `PlayerMovementStrategy`, `IJumpStrategy`, `PlayerJumpStrategy` exist but are not called by the active movement pipeline (`PlayerMovement.cs`).
- Status: Not used. Do not add new strategy implementations without first wiring up the interface.

**`Player/Movement/` (root level files):**
- `MovementState.cs` — `MovementState` enum + `PlayerMovementContext` class (both in `namespace Core`)
- `PlayerMovement.cs` — `IControllable` implementation; owns Rigidbody velocity calculations and momentum
- `GroundChecker.cs` — spherecast ground detection, slope analysis, landing/leaving-ground events
- `PlayerJumper.cs` — jump force, gravity, hold-jump variable height
- `PlayerCrouch.cs`, `PlayerSlide.cs`, `PlayerDash.cs`, `PlayerWallRun.cs`, `PlayerGrapple.cs` — one component per movement ability

**`Health/`:**
- Purpose: Self-contained health system with its own interfaces, types, and data classes.
- `namespace Health` is used throughout this directory.
- `IDamagable.cs` sits at root of `Health/` (not in `Interfaces/`) — inconsistency to note.
- `HealthChunk.cs` sits at root of `Health/` (not in `Components/`) — inconsistency to note.
- Tests are in `Health/Tests/Editor/` — the only true editor-only tests in the project.

**`Data_Scripts/`:**
- Purpose: All ScriptableObject definitions. Tuning data only; no MonoBehaviour logic.
- Sub-divided by domain: `Combat/`, `Health/`.
- Convention: All SOs use `[CreateAssetMenu]` and end in `SO` suffix.

**`Managers/`:**
- Purpose: Scene-persistent or scene-scoped singleton services.
- `AITickManager.cs` is physically in this directory (not `AI/`), despite also being referenced from `AI/`.
- `SceneLoader.cs` is a `static class`, not a Singleton — accessed as a utility anywhere.
- `AudioManager.cs` is a plain `MonoBehaviour`, not a `Singleton<T>` subclass — must be placed in scene manually.

**`AI/`:**
- Purpose: AI infrastructure and agent implementations.
- `AIMemory.cs` and `AIPerception.cs` are per-agent components (attached to enemy GameObjects).
- `TestEnemy.cs` is a prototype agent using Unity `NavMeshAgent`.

**`Combat/`:**
- Purpose: Weapon implementations and projectile simulation.
- `Combat/Interfaces/IWeapon.cs` defines the weapon contract.
- `ProjectileBase.cs` lives in `Combat/` root (not a subdirectory) — manually-simulated, no Rigidbody.

**`UI/HUD/`:**
- Purpose: In-game overlay controllers.
- `WeaponHUDController` subscribes to `WeaponBase.OnAmmoChanged` and `WeaponInventory.OnWeaponChanged` — static events on the component classes.

**`Testing/`:**
- Purpose: In-build (non-Editor) debug and test scripts.
- NOT in an `Editor/` folder, so these compile into production builds.

**`Environment/Platforms/`:**
- Purpose: Moving platform system. `PlatformMover` moves the platform and calls `IPlatformRider` callbacks on riders. `PlatformRider` implements `IPlatformRider` and uses `Rigidbody.MovePosition/MoveRotation`.

---

## Key File Locations

**FSM Engine:**
- `Assets/Scripts/Core/FSM/StateMachine.cs`
- `Assets/Scripts/Core/FSM/BaseState.cs`
- `Assets/Scripts/Core/FSM/IState.cs`

**Player FSM entry point (FSM construction):**
- `Assets/Scripts/Player/PlayerInput.cs` — `InitializeStateMachine()` builds context and registers all states

**Player movement context (shared state):**
- `Assets/Scripts/Player/Movement/MovementState.cs` — `PlayerMovementContext` class + `MovementState` enum

**All outer FSM states:**
- `Assets/Scripts/Player/Movement/States/GroundedState.cs`
- `Assets/Scripts/Player/Movement/States/InAirState.cs`
- `Assets/Scripts/Player/Movement/States/WallRunningState.cs`
- `Assets/Scripts/Player/Movement/States/DashingState.cs`
- `Assets/Scripts/Player/Movement/States/GrapplingState.cs`

**All inner (grounded sub) FSM states:**
- `Assets/Scripts/Player/Movement/States/WalkingState.cs`
- `Assets/Scripts/Player/Movement/States/SprintingState.cs`
- `Assets/Scripts/Player/Movement/States/CrouchingState.cs`
- `Assets/Scripts/Player/Movement/States/SlidingState.cs`

**Physics / velocity:**
- `Assets/Scripts/Player/Movement/PlayerMovement.cs` — implements `IControllable`, handles Rigidbody velocity
- `Assets/Scripts/Player/Movement/GroundChecker.cs` — ground detection and slope analysis
- `Assets/Scripts/Player/Movement/PlayerJumper.cs` — jump request and gravity

**Ability components:**
- `Assets/Scripts/Player/Movement/PlayerDash.cs`
- `Assets/Scripts/Player/Movement/PlayerGrapple.cs`
- `Assets/Scripts/Player/Movement/PlayerWallRun.cs`
- `Assets/Scripts/Player/Movement/PlayerCrouch.cs`
- `Assets/Scripts/Player/Movement/PlayerSlide.cs`

**Health system:**
- `Assets/Scripts/Health/Components/BaseHealth.cs`
- `Assets/Scripts/Health/Components/PlayerHealth.cs`
- `Assets/Scripts/Health/Components/EnemyHealth.cs`
- `Assets/Scripts/Health/HealthChunk.cs`
- `Assets/Scripts/Health/IDamagable.cs`
- `Assets/Scripts/Data_Scripts/Health/DamageMatrixSO.cs`

**Global services:**
- `Assets/Scripts/Managers/EventsManager.cs`
- `Assets/Scripts/Managers/GameManager.cs`
- `Assets/Scripts/Managers/SceneLoader.cs`
- `Assets/Scripts/AI/AITickManager.cs`

**Combat:**
- `Assets/Scripts/Combat/Weapons/WeaponBase.cs`
- `Assets/Scripts/Combat/Weapons/WeaponInventory.cs`
- `Assets/Scripts/Combat/ProjectileBase.cs`
- `Assets/Scripts/Data_Scripts/Combat/WeaponDataSO.cs`

---

## Naming Conventions

**Files:**
- MonoBehaviours: `PascalCase`, descriptive noun or noun-phrase — `PlayerMovement.cs`, `GroundChecker.cs`
- Interfaces: `I` prefix — `IControllable.cs`, `IDamagable.cs`
- ScriptableObjects: `SO` suffix — `WeaponDataSO.cs`, `DamageMatrixSO.cs`
- States: `[StateName]State.cs` — `WalkingState.cs`, `GrapplingState.cs`
- Enums: PascalCase, typically own file — `MovementState.cs`, `DamageType.cs`

**Namespaces in use:**
- `Core` — `MovementState` enum, `PlayerMovementContext`
- `Core.Events` — referenced in imports but no files found defining it (may be unused import)
- `Entities.Controllers` — `Controller`, `PlayerInput`
- `Health` — all health system classes
- `Strategies` — `IControllable`, `IMovable`, `IJumpable`
- Most gameplay MonoBehaviours are in the global namespace (no `namespace` declaration)

**Directories:**
- Organized by domain/feature, not by type (not a flat `Components/` or `Scripts/` dump)
- `Data_Scripts/` for pure SO data; runtime component logic stays in the domain folder

---

## Where to Add New Code

**New movement state (e.g. swimming, climbing):**
1. Add the enum value to `MovementState` in `Assets/Scripts/Player/Movement/MovementState.cs`
2. Create `[StateName]State.cs` in `Assets/Scripts/Player/Movement/States/`
3. Inherit `BaseState<PlayerMovementContext>`, implement `OnEnter`, `OnUpdate`, `OnExit`
4. Register in `PlayerInput.InitializeStateMachine()` in `Assets/Scripts/Player/PlayerInput.cs`
5. Add transition logic in the state(s) that should exit to the new state

**New outer state vs. new inner (grounded sub) state:**
- Outer: register on `_stateMachine` in `InitializeStateMachine()`
- Inner grounded sub-state: register on `_innerMachine` inside `GroundedState`'s constructor in `Assets/Scripts/Player/Movement/States/GroundedState.cs`

**New ability component (e.g. dodge roll):**
1. Create `PlayerDodge.cs` in `Assets/Scripts/Player/Movement/`
2. Add `[RequireComponent(typeof(PlayerDodge))]` to `PlayerInput`
3. Add `public PlayerDodge PlayerDodge` to `PlayerMovementContext`
4. Populate it in `PlayerInput.InitializeStateMachine()`

**New enemy type:**
1. Create the MonoBehaviour in `Assets/Scripts/AI/`
2. Implement `ITickable` and register with `AITickManager.Instance` in `Awake`
3. Add `AIMemory` component for perception state
4. Drive movement via `UnityEngine.AI.NavMeshAgent`

**New weapon:**
1. Create `[WeaponName].cs` in `Assets/Scripts/Combat/Weapons/` inheriting `WeaponBase`
2. Implement `Shoot()` and `Reload()`
3. Create a `WeaponDataSO` asset via `Assets > Create > Combat > Weapon Data`
4. Add to the player's `WeaponInventory` component in the scene

**New ScriptableObject data type:**
- Place in `Assets/Scripts/Data_Scripts/[Domain]/`
- End class name with `SO`, use `[CreateAssetMenu]`

**New manager / service:**
- Inherit `Singleton<T>` and place in `Assets/Scripts/Managers/`
- Subscribe to `EventsManager` events in `Start`, unsubscribe in `OnDestroy`

**New HUD element:**
- Place controller script in `Assets/Scripts/UI/HUD/`
- Subscribe to `static event Action` on the relevant component class, or subscribe to `EventsManager`

---

## Special Directories

**`Assets/Scripts/Testing/`:**
- Purpose: In-build debug helpers (bullet spawner, damage target, NavMesh tester, spectator camera)
- Generated: No
- Committed: Yes
- Note: These are NOT in an `Editor/` folder, so they ship in production builds. Move to `Editor/` or gate behind `#if UNITY_EDITOR` if that is undesirable.

**`Assets/Scripts/Health/Tests/Editor/`:**
- Purpose: Editor-only unit tests for the health system
- Generated: No
- Committed: Yes
- Note: The only proper Editor-folder test location in the project.

**`Assets/Scripts/Core/Command/`:**
- Purpose: Command pattern scaffolding
- Status: All methods throw `NotImplementedException`. Not wired to any system.
- Committed: Yes — treat as dead code until intentionally activated.

---

*Structure analysis: 2026-03-26*
