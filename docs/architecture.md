# Architecture Overview

## System Map

```
┌─────────────────────────────────────────────────────────────────┐
│                        PlayerController                         │
│  (reads Unity Input System, owns StateMachine + Context)        │
└───────────────────────┬─────────────────────────────────────────┘
                        │ creates / ticks
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│               StateMachine<MovementState>  (outer)              │
│  Grounded ──► GroundedState (composite)                         │
│                  └─ StateMachine<MovementState> (inner)         │
│                       Walking / Sprinting / Crouching / Sliding │
│  WallRunning, Dashing, Grappling, InAir                         │
└───────────────────────┬─────────────────────────────────────────┘
                        │ reads / writes
                        ▼
┌─────────────────────────────────────────────────────────────────┐
│                   PlayerMovementContext                         │
│  component refs: PlayerMovement, PlayerDash, PlayerGrapple,     │
│                  PlayerWallRun, PlayerCrouch, PlayerSlide        │
│  input flags:    WantsToSprint/Crouch/Jump/Dash/Grapple         │
│  state machines: StateMachine (outer), GroundedStateMachine     │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────┐
│           EventsManager              │
│  OnGamePause / OnGameOver /          │
│  OnGameExit / OnGameRestart          │
└────┬──────────────────┬──────────────┘
     │                  │
     ▼                  ▼
GameManager        AITickManager
(timeScale,        (pauses AI tick
 scene load)        on pause/over)

┌──────────────────────────────────────┐
│            Health System             │
│  DamageInfo ──► ApplyModifiers       │
│               ──► per-chunk damage   │
│               ──► OnHealthChanged    │
│               ──► OnDeath            │
└──────────────────────────────────────┘
```

---

## Player Input → Movement Flow

1. `PlayerController.Update()` reads Unity Input System action values.
2. Camera-relative direction is computed from `Camera.main` forward/right.
3. `UpdateContext()` writes all input flags into `PlayerMovementContext`.
4. `_stateMachine.Update()` ticks the active state's `OnUpdate()`.
5. Each state reads the context and calls methods on movement components
   (`TryStartDash()`, `TryStartGrapple()`, `StartWallRun()`, etc.) or triggers
   state transitions via `Context.StateMachine.ChangeState(...)`.
6. `Controllable.Move(direction)` and `Controllable.Jump()` are called directly
   on `PlayerMovement` (via `IControllable`) every frame regardless of state.
7. `FixedUpdate()` follows the same pattern through `_stateMachine.FixedUpdate()`.

---

## Health Damage Flow

```
caller: target.TakeDamage(new DamageInfo(damage, DamageType.Fire, hitPoint))
  │
  ├─ ApplyDamageModifiers(damageInfo)
  │    sorted by IDamageModifier.Priority (ascending)
  │    e.g. PlayerAdrenaline reduces damage by up to 40%
  │
  ├─ for each non-depleted HealthChunk (front to back):
  │    multiplier = DamageMatrixSO.GetMultiplier(damageType, chunk.HealthType)
  │    overflow   = chunk.ApplyDamage(remaining * multiplier)
  │    remaining  = overflow            ← NOTE: multiplier applied per-chunk (bug)
  │    if chunk just depleted → OnChunkDepleted(i)
  │
  ├─ OnHealthChanged(HealthChangeEventArgs)
  └─ if !IsAlive → OnDeath()
```

---

## AI Tick Distribution

```
Each frame:
  AITickManager.Update()
    └─ TickNextAgent()
         agents[_currentIndex].OnTick(deltaTime)
         _currentIndex = (_currentIndex + 1) % agents.Count
```

Up to `_ticksPerFrame` (1–5) agents are ticked per frame. With many enemies,
not every agent updates every frame — this smooths CPU cost at the expense of
tick frequency per agent.

---

## Design Patterns

| Pattern | Where Used | Notes |
|---------|-----------|-------|
| Hierarchical FSM | `StateMachine<T>` + `GroundedState` inner machine | `GroundedState` owns and delegates to an inner `StateMachine<MovementState>` for sub-states |
| Shared Context | `PlayerMovementContext` | Single mutable object passed to all states; states read input flags and call component methods |
| Composite State | `GroundedState` | Outer state that manages an inner FSM; uses `ExitCurrentState()` on outer machine before outer transitions |
| Observer / Events | `EventsManager`, `BaseHealth` | C# `Action` delegates for loose coupling between systems |
| Singleton | `Singleton<T>` base class | Global managers (EventsManager, AITickManager); optional `DontDestroyOnLoad` |
| Round-Robin Tick | `AITickManager` | Distributes AI updates across frames to flatten CPU spikes |
| ScriptableObject Config | `DamageMatrixSO`, `PlayerHealthConfigSO`, `EnemyHealthConfigSO` | Keeps balance data out of code; inspector-editable |
| Damage Modifier Chain | `IDamageModifier` + `BaseHealth` | Ordered list of modifier components (e.g., adrenaline) applied before damage hits chunks |
| Command (incomplete) | `CommandInvoker`, `JumpCommand`, `MoveCommand` | Scaffolding only — `Undo()` throws `NotImplementedException`; currently unused |

---

## Key Dependencies

```
PlayerController
  ├─ PlayerMovement        (IControllable, core physics)
  ├─ PlayerJumper          (owned by PlayerMovement)
  ├─ GroundChecker         (owned by PlayerMovement)
  ├─ PlayerCrouch
  ├─ PlayerSlide
  ├─ PlayerDash
  ├─ PlayerGrapple
  └─ PlayerWallRun

BaseHealth
  └─ DamageMatrixSO        (damage type × health type multipliers)

PlayerHealth / EnemyHealth
  └─ PlayerHealthConfigSO / EnemyHealthConfigSO

PlayerAdrenaline
  ├─ PlayerHealth          (subscribes to OnHealthChanged)
  └─ PlayerMovement        (reads Momentum01)

EventsManager ──► GameManager
             ──► AITickManager
```

---

## File Locations

| System | Path |
|--------|------|
| FSM core | `Assets/Scripts/Core/FSM/` |
| Movement states | `Assets/Scripts/Player/Movement/States/` |
| Context + enum | `Assets/Scripts/Player/Movement/MovementState.cs` |
| Movement components | `Assets/Scripts/Player/Movement/` |
| Player controller | `Assets/Scripts/Player/PlayerController.cs` |
| Health system | `Assets/Scripts/Health/` |
| AI | `Assets/Scripts/AI/` |
| Managers | `Assets/Scripts/Managers/` |
| Singleton base | `Assets/Scripts/Core/Singleton/Singleton.cs` |
| ScriptableObjects | `Assets/Scripts/ScriptableObjects/` |
