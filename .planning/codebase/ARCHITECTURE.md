# Architecture

**Analysis Date:** 2026-03-26

## Pattern Overview

**Overall:** Component-based MonoBehaviour architecture with a layered FSM, singleton service locator, and data-driven ScriptableObject configuration.

**Key Characteristics:**
- All gameplay entities are Unity GameObjects with composed MonoBehaviour components — no pure ECS or DI framework
- Player movement is driven by a hierarchical two-level FSM: an outer machine for locomotion mode and an inner machine for grounded sub-states
- Game-wide services (events, audio, AI scheduling, scene loading) are accessed via `Singleton<T>` instances
- ScriptableObjects carry tuning data; runtime mutable state lives in plain C# classes or component fields
- Combat damage flows through a typed pipeline: `DamageInfo` → `IDamageModifier` chain → `DamageMatrixSO` multiplier → `HealthChunk` absorption

---

## Core Systems

### 1. Finite State Machine

**Files:**
- `Assets/Scripts/Core/FSM/IState.cs` — interface: `OnEnter`, `OnUpdate`, `OnFixedUpdate`, `OnExit`
- `Assets/Scripts/Core/FSM/BaseState.cs` — abstract generic `BaseState<TContext>`, stores typed context, provides `Log()`
- `Assets/Scripts/Core/FSM/StateMachine.cs` — generic `StateMachine<TState> where TState : Enum`; dictionary of `IState`, exposes `Initialize`, `ChangeState`, `Update`, `FixedUpdate`, `ExitCurrentState`, `Clear`

**Instantiation Pattern:**
```csharp
_stateMachine = new StateMachine<MovementState>();
_stateMachine.RegisterState(MovementState.Grounded, new GroundedState(_context));
_stateMachine.Initialize(MovementState.Grounded);
```

**Hierarchy (Player):**
- Outer machine — 5 states: `Grounded`, `InAir`, `WallRunning`, `Dashing`, `Grappling`
- `GroundedState` owns a second inner `StateMachine<MovementState>` — 4 sub-states: `Walking`, `Sprinting`, `Crouching`, `Sliding`
- Both machines share the same `PlayerMovementContext` instance
- The inner machine reference is stored in `context.GroundedStateMachine` so sub-states can trigger their own transitions

### 2. Player Movement Context

**File:** `Assets/Scripts/Player/Movement/MovementState.cs`

`PlayerMovementContext` is a plain C# class (not a MonoBehaviour). It is constructed once in `PlayerInput.InitializeStateMachine()` and passed by reference to every state constructor. All states read from and write to it rather than fetching components directly.

**Contains:**
- Component references: `PlayerMovement`, `PlayerCrouch`, `PlayerSlide`, `PlayerDash`, `PlayerWallRun`, `PlayerGrapple`, `Rigidbody`, `IControllable`
- Both state machine references: `StateMachine`, `GroundedStateMachine`
- Per-frame input booleans: `WantsToSprint`, `WantsToCrouch`, `WantsToJump`, `WantsToGrapple`, `WantsToDash`
- Input vectors: `MovementInput`, `DashInputDirection`

### 3. Singleton Service Layer

**Files:**
- `Assets/Scripts/Core/Singleton/Singleton.cs` — `Singleton<T> : MonoBehaviour`; enforces one instance via `Awake`, optional `DontDestroyOnLoad` via `_persistAcrossScenes`

**Active singletons:**
| Class | File | Purpose |
|---|---|---|
| `EventsManager` | `Assets/Scripts/Managers/EventsManager.cs` | Global C# `Action` events |
| `GameManager` | `Assets/Scripts/Managers/GameManager.cs` | Pause/GameOver/restart logic |
| `AITickManager` | `Assets/Scripts/AI/AITickManager.cs` (also `Managers/`) | Round-robin AI tick scheduling |
| `AudioManager` | `Assets/Scripts/Managers/AudioManager.cs` | Sound playback by name (not a Singleton subclass) |

### 4. Event System

**File:** `Assets/Scripts/Managers/EventsManager.cs`

Four global events on `EventsManager.Instance`:
- `OnGamePause(bool isPaused)` — listened to by `GameManager`, `AITickManager`
- `OnGameOver()` — stops time, pauses AI
- `OnGameExit()` — loads main menu via `SceneLoader`
- `OnGameRestart()` — loads game scene via `SceneLoader`

**Weapon events** are `static event Action` on the component class, not routed through `EventsManager`:
- `PlayerInput.OnShoot`, `PlayerInput.OnChangeWeapon`, `PlayerInput.OnReload`
- `WeaponBase.OnAmmoChanged(int magazine, int reserves)`
- `WeaponInventory.OnWeaponChanged(string weaponName)`

### 5. Health System

**Files:**
- `Assets/Scripts/Health/Components/BaseHealth.cs` — abstract, holds `List<HealthChunk>` + `List<IDamageModifier>`
- `Assets/Scripts/Health/Components/PlayerHealth.cs` — 1 main chunk + N side chunks, all `HealthType.Player`
- `Assets/Scripts/Health/Components/EnemyHealth.cs` — chunk list driven by `EnemyHealthConfigSO`
- `Assets/Scripts/Health/HealthChunk.cs` — plain C# class, holds `float _currentHealth`, returns overflow on `ApplyDamage`
- `Assets/Scripts/Data_Scripts/Health/DamageMatrixSO.cs` — lookup table mapping `(DamageType, HealthType)` → float multiplier

**Damage pipeline:**
1. Caller constructs `DamageInfo(baseDamage, damageType, hitPoint)`
2. `BaseHealth.TakeDamage(DamageInfo)` runs `IDamageModifier` chain (sorted by `Priority`) → produces `FinalDamage`
3. Iterates `healthChunks` in order; per chunk applies `DamageMatrixSO.GetMultiplier(damageType, chunk.HealthType)` then calls `chunk.ApplyDamage(remainingDamage * multiplier)`
4. Fires `OnHealthChanged(HealthChangeEventArgs)`, `OnChunkDepleted(int)`, or `OnDeath`

**Known bug:** The multiplier is applied to `remainingDamage` before passing into `ApplyDamage`, but `ApplyDamage` returns the un-multiplied overflow. Overflow damage to the next chunk is therefore not re-multiplied, causing damage type scaling to leak incorrectly across chunk boundaries.

**Damage modifier example — `PlayerAdrenaline`:**
- `Assets/Scripts/Health/Components/PlayerAdrenaline.cs`
- Implements `IDamageModifier` (`Priority = 100`)
- Reads `PlayerMovement.Momentum01` each `Update` to derive 0–4 adrenaline level
- Reduces `baseDamage` by `level * 10%` (up to 40% reduction at max momentum)

**Damage types:** `Kinetic`, `Fire`, `Plasma`, `Energy` (defined in `Assets/Scripts/Health/Types/DamageType.cs`)
**Health types:** `Player`, `Flesh`, `Exo`, `Shield` (defined in `Assets/Scripts/Health/Types/HealthType.cs`)

### 6. Input System

**File:** `Assets/Scripts/Player/PlayerInput.cs`

`PlayerInput : Controller` owns the Unity Input System `PlayerInput` component. In `Update` it:
1. Reads raw input from named `InputAction` bindings (`"Movement"`, `"Jump"`, `"Sprint"`, etc.)
2. Converts `Vector2` movement to camera-relative world-space direction via `CalculateCameraRelativeDirection()`
3. Writes results into `PlayerMovementContext` via `UpdateContext()`
4. Calls `_stateMachine.Update()` — states pull from context, not from input directly
5. Forwards movement and jump directly to `IControllable` (double-dispatch, noted as a TODO)
6. Fires `static event Action` for weapon actions (`OnShoot`, `OnChangeWeapon`, `OnReload`)

**Known issue:** `Camera.main` is called every `Update` frame rather than cached.

### 7. Combat / Weapon System

**Files:**
- `Assets/Scripts/Combat/Weapons/WeaponBase.cs` — abstract `MonoBehaviour`, holds `WeaponDataSO`, manages ammo counters, fires `OnAmmoChanged`
- `Assets/Scripts/Combat/Weapons/Pistol.cs`, `Shotgun.cs` — concrete implementations
- `Assets/Scripts/Combat/Weapons/WeaponInventory.cs` — cycle through `List<WeaponBase>`, activates/deactivates GameObjects, fires `OnWeaponChanged`
- `Assets/Scripts/Combat/ProjectileBase.cs` — manual simulation (no Rigidbody); per-frame `Physics.Raycast`, deactivates on hit or lifetime expiry
- `Assets/Scripts/Data_Scripts/Combat/WeaponDataSO.cs` — all tuning values including damage falloff `AnimationCurve`

**Damage dealing path:**
```
PlayerInput.OnShoot
  → WeaponBase.TryShoot()
  → WeaponBase.Shoot()            // implemented in Pistol/Shotgun
  → ProjectileBase.Initialise()   // spawned at muzzle
  → ProjectileBase.TryDealDamage()
  → IDamagable.TakeDamage(DamageInfo)
```

### 8. AI System

**Files:**
- `Assets/Scripts/AI/ITickable.cs` — `void OnTick(float deltaTime)` + `bool IsTickable`
- `Assets/Scripts/AI/AITickManager.cs` — singleton, round-robin list of `ITickable`, configurable `_ticksPerFrame` (1–5)
- `Assets/Scripts/AI/TestEnemy.cs` — `ITickable` + Unity `NavMeshAgent`; registers in `Awake`, unregisters in `OnDestroy`
- `Assets/Scripts/AI/AIMemory.cs` — per-agent memory: `AlertLevel` enum, `LastKnownPlayerPosition`, `CanSeePlayer`
- `Assets/Scripts/AI/AIPerception.cs` — empty (1 line stub)

Agents do NOT tick themselves every frame. Only the agents selected by `AITickManager` in a given frame execute `OnTick`. `IsTickable` allows individual agents to temporarily opt out (e.g. dead enemies return `false`).

### 9. Scene Management

**Files:**
- `Assets/Scripts/Managers/SceneLoader.cs` — static class; always routes through a `LoadingScene` intermediary
- `Assets/Scripts/Managers/LoadingManager.cs` — runs async load from `LoadingScene`

**Flow:** Any system calls `SceneLoader.OpenLoadingScene(targetScene)` → `LoadingScene` loads → `LoadingManager` calls `SceneLoader.LoadNextAsync()` → actual scene loads → `SceneLoader.CloseLoadingScene()`.

### 10. Momentum System

**File:** `Assets/Scripts/Player/Movement/PlayerMovement.cs`

`_momentum` (0–`maxMomentum`, default cap 0.8) is a float on `PlayerMovement` that scales `CurrentSpeed` by `(1 + _momentum)`. Sources: sprinting, sliding, downhill travel, post-dash. Decay: exponential half-life when unfueled. `PlayerAdrenaline` reads `Momentum01` to gate damage reduction.

---

## Data Flow

### Player Input → Physics Frame

```
Update():
  PlayerInput.Update()
    → CalculateCameraRelativeDirection()    // raw input → world-space Vector2
    → UpdateContext()                        // write to PlayerMovementContext
    → _stateMachine.Update()                 // outer state: transition checks
      → GroundedState.OnUpdate()
        → _innerMachine.Update()             // sub-state: Walking/Sprinting/etc.
    → IControllable.Move(direction)          // writes _rawMoveDir on PlayerMovement

FixedUpdate():
  PlayerMovement.FixedUpdate()
    → GroundChecker.CheckGround()
    → AccumulateAndDecayMomentum()
    → SmoothInput()
    → UpdateSlopeDrag()
    → UpdateMovement()                       // applies velocity to Rigidbody
    → ClearInput()
  _stateMachine.FixedUpdate()               // physics ops inside states
```

### Damage → Death

```
Projectile hits collider
  → IDamagable.TakeDamage(DamageInfo)
  → IDamageModifier chain (PlayerAdrenaline etc.)
  → DamageMatrixSO.GetMultiplier(damageType, healthType)
  → HealthChunk.ApplyDamage(modified damage)
  → BaseHealth events: OnHealthChanged / OnChunkDepleted / OnDeath
  → UI or game logic subscribes to these events
```

### Game State Events

```
Any system → EventsManager.Instance.ActionGameOver()
  → GameManager.GameOverListener() → Time.timeScale = 0
  → AITickManager.HandleGameOver() → _isPaused = true
  → UI (GameOverMenu) subscribes to OnGameOver directly
```

---

## Key Abstractions

**`IState` / `BaseState<TContext>`:**
- Every player movement state inherits `BaseState<PlayerMovementContext>`
- States must not store mutable runtime state of their own; they read/write via `Context`
- Files: `Assets/Scripts/Player/Movement/States/`

**`IControllable` / `IMovable` / `IJumpable`:**
- `IControllable` composes `IMovable` (`.Move(Vector2)`, `.SetMovementState()`) and `IJumpable` (`.Jump()`, `.SetHoldingJump()`)
- `PlayerMovement` implements this; `PlayerInput` holds a reference and drives it
- Files: `Assets/Scripts/Player/Interfaces/`

**`IDamagable`:**
- `TakeDamage(DamageInfo)` + `bool IsAlive`
- All damageable entities implement this; `ProjectileBase` uses `GetComponentInParent<IDamagable>()`
- File: `Assets/Scripts/Health/IDamagable.cs`

**`IDamageModifier`:**
- `float ModifyDamage(float baseDamage, DamageInfo damageInfo)` + `int Priority`
- Registered on `BaseHealth` at runtime; supports stacking modifiers (armor, adrenaline, etc.)
- File: `Assets/Scripts/Health/Interfaces/IDamageModifier.cs`

**`ITickable`:**
- AI agents implement this to participate in `AITickManager` round-robin
- File: `Assets/Scripts/AI/ITickable.cs`

---

## Error Handling

**Strategy:** `Debug.LogError` / `Debug.LogWarning` in `Awake`/`Start` for missing required references. No exception throwing at runtime. Missing references generally cause silent no-ops (null guards with early returns).

**Patterns:**
- `StateMachine.ChangeState` logs error and returns without crashing if state is unregistered
- `PlayerInput` logs error if `IControllable` is null, then checks `if (Controllable == null) return` each frame
- `BaseHealth.TakeDamage` guards on `!IsAlive`
- `AITickManager` guards on `EventsManager.Instance == null` before subscribing

---

## Cross-Cutting Concerns

**Logging:** `Debug.Log/LogWarning/LogError` directly. `BaseState` provides a `Log(string)` helper that prefixes with the state class name. No structured logging or log level filtering.

**Validation:** Inspector `[SerializeField]` fields with `Debug.LogError` in `Awake` if null. No automated null-check framework.

**Pause Handling:** `EventsManager.OnGamePause(bool)` → `GameManager` sets `Time.timeScale`; `AITickManager` sets its own `_isPaused` flag. UI reads pause state through this event.

**Orphaned Scaffolding:**
- `Assets/Scripts/Core/Command/` — `CommandInvoker`, `JumpCommand`, `MoveCommand` all throw `NotImplementedException`; not wired into anything
- `Assets/Scripts/Player/Movement/Strategies/` — `IMovementStrategy`, `PlayerMovementStrategy`, `IJumpStrategy`, `PlayerJumpStrategy` exist but are not called from the active movement pipeline; leftover from an earlier refactor
- `Assets/Scripts/AI/AIPerception.cs` — empty stub (1 line)

---

*Architecture analysis: 2026-03-26*
