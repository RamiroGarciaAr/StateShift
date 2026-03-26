<!-- GSD:project-start source:PROJECT.md -->
## Project

**StateShift**

A singleplayer movement shooter FPS built in Unity, targeting Titanfall 2 pilot gameplay feel with Apex Legends-level gunplay polish. The immediate goal is a polished vertical slice — one level that proves the movement and combat loop feel exceptional. The architecture is designed from day one to scale into a full singleplayer campaign.

**Core Value:** Movement must feel exceptional first — every traversal action (wall-run, slide-hop, bunny hop) must be snappy, momentum-preserving, and deeply satisfying before anything else ships.

### Constraints

- **Tech stack**: Unity 2022.3 LTS — no engine upgrade this milestone
- **Render pipeline**: Migrating to URP — all shaders and post-processing must be URP-compatible after migration
- **Platform**: Windows PC Standalone only — no cross-platform build constraints
- **Code quality**: Zero hardcoded values (all balance via SOs), zero known bugs per phase, English-only identifiers and comments, strict folder/naming conventions enforced
- **Architecture**: New systems must respect separation of concerns — input, state, movement, and weapon systems are independently testable units
<!-- GSD:project-end -->

<!-- GSD:stack-start source:codebase/STACK.md -->
## Technology Stack

## Languages
- C# — All gameplay, AI, UI, and system scripts (`Assets/Scripts/`, 105 `.cs` files)
- ShaderLab / HLSL — Custom crosshair shader (`Assets/Scripts/Shaders/UI/CrosshairShader.shader`)
## Runtime
- Unity 2022.3.62f2 LTS (Long Term Support)
- Revision: 7670c08855a9
- Windows PC Standalone (primary build target based on project structure and feature set)
- Default resolution: 1920x1080
## Frameworks
- Unity 2022.3 LTS — MonoBehaviour-based architecture with custom singleton, FSM, and event layers
- Unity Input System `com.unity.inputsystem` v1.14.2
- Input map: `Assets/InputActions/PlayerMap.inputactions`
- Actions: Movement, Jump, Sprint, Crouch, Aim, Shoot, Reload, Pause, Interact, Dash, Grapple, ChangeWeapon
- Cinemachine `com.unity.cinemachine` v2.10.5
- Used in: `Assets/Scripts/Player/Camera/CameraController.cs`, `DynamicFOV.cs`, `WallRunCamaraEffects.cs`
- Unity uGUI `com.unity.ugui` v1.0.0 — menus, HUD base layout
- TextMesh Pro `com.unity.textmeshpro` v3.0.9 — all in-game text rendering
- ParticleEffectForUGUI `com.coffee.ui-particle` v4.11.3 (git) — UI particle effects (speed lines: `Assets/Scripts/UI/HUD/SpeedLinesController.cs`)
- Unity AI Navigation `com.unity.ai.navigation` v1.1.7 — NavMesh baking and runtime
- NavMeshAgent used in: `Assets/Scripts/AI/TestEnemy.cs`, `Assets/Scripts/Testing/NavMeshTester.cs`
- Render Pipeline: Built-in (no URP/HDRP — confirmed by `GraphicsSettings.asset` using legacy deferred/shadow shaders)
- Post Processing Stack `com.unity.postprocessing` v3.4.0 — imported but no script-level usage detected; likely applied via volume components in scenes
- Unity Physics (3D) — `com.unity.modules.physics` — used across 23 scripts (Rigidbody, CharacterController, LayerMask, Raycast, SpringJoint)
- Unity Animation `com.unity.modules.animation` — Animator referenced in: `WeaponDataSO.cs`, `UIAnimationManager.cs`, `DynamicFOV.cs`, `GrappleRope.cs`, `PlayerDash.cs`, `CrosshairBloomController.cs`
- Unity Audio `com.unity.modules.audio` — custom `AudioManager` at `Assets/Scripts/Managers/AudioManager.cs` using `AudioSource` / `AudioClip` / `AudioMixer`
- ProBuilder `com.unity.probuilder` v5.2.4 — geometry authoring tool, data in `Assets/Thirdparty/ProBuilder Data/`
- Unity Timeline `com.unity.timeline` v1.7.7 — package present, no script usage detected; likely used in scene animations
- Unity Vector Graphics `com.unity.vectorgraphics` v2.0.0-preview.25 — imported, no direct script usage detected
- Unity Visual Scripting `com.unity.visualscripting` v1.9.4 — namespace imported in `GroundChecker.cs` and `PlayerMovement.cs` (likely for `TypeOptions` attribute only, not for graph-based logic)
- Unity Version Control (Collab Proxy) `com.unity.collab-proxy` v2.9.1
- Unity Feature Development `com.unity.feature.development` v1.0.1 (meta-package)
## Key Dependencies
- `com.unity.inputsystem` v1.14.2 — all player input, cannot remove
- `com.unity.cinemachine` v2.10.5 — camera system, cannot remove
- `com.unity.textmeshpro` v3.0.9 — all text rendering in UI
- `com.unity.ai.navigation` v1.1.7 — enemy pathfinding
- `com.unity.postprocessing` v3.4.0 — visual effects pipeline
- `com.coffee.ui-particle` v4.11.3 — UI particle effects (speed lines HUD)
- `com.unity.probuilder` v5.2.4 — scene geometry
## Data Configuration
- `Assets/Scripts/Data_Scripts/Combat/WeaponDataSO.cs` — weapon stats
- `Assets/Scripts/Data_Scripts/Health/DamageMatrixSO.cs` — damage type multipliers
- `Assets/Scripts/Data_Scripts/Health/EnemyHealthConfigSO.cs` — enemy health config
- `Assets/Scripts/Data_Scripts/Health/PlayerHealthConfigSO.cs` — player health config
- Instantiated assets stored in `Assets/Data/`
- `Assets/Scenes/LoadingScene.unity`
- `Assets/Scenes/MainMenuScene.unity`
- `Assets/Scenes/Testing/EnemyTestScene.unity`
- `Assets/Scenes/Testing/PlayerTesting.unity`
- `Assets/Scenes/Testing/ShootingRange.unity`
## Platform Requirements
- Unity 2022.3.62f2 LTS
- Windows 11 (current dev environment)
- No .NET version file present — uses Unity's embedded Mono / IL2CPP
- Target: Windows PC Standalone
- Resolution default: 1920x1080
- No mobile, VR, or WebGL modules actively configured
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

## Naming Patterns
- PascalCase throughout: `BaseHealth`, `PlayerMovementContext`, `WeaponInventory`
- MonoBehaviour components use descriptive compound names: `PlayerCrouch`, `PlayerWallRun`, `GroundChecker`
- Abstract base classes prefixed with `Base`: `BaseHealth`, `BaseState<T>`, `WeaponBase`
- State classes suffixed with `State`: `DashingState`, `WallRunningState`, `GrapplingState`
- ScriptableObjects suffixed with `SO`: `WeaponDataSO`, `DamageMatrixSO`, `PlayerHealthConfigSO`
- Singletons inherit `Singleton<T>` and are named by responsibility: `EventsManager`, `GameManager`, `AITickManager`
- Interfaces prefixed with `I`: `IWeapon`, `IDamagable`, `ITickable`, `IControllable`
- Manager scripts end with `Manager`: `EventsManager`, `AudioManager`, `AITickManager`
- PascalCase for all public and protected methods: `TakeDamage`, `InitializeChunks`, `ExitToAppropriateState`
- Private methods also PascalCase: `CalculateHorizontalMovement`, `UpdateContext`, `ApplyFinalVelocity`
- Boolean-returning methods use "Is/Has/Can/Was" prefixes: `IsGrounded`, `HasWall`, `CanGrapple`, `WasGrounded`
- Unity callbacks in standard form: `Awake`, `Start`, `Update`, `FixedUpdate`, `OnEnable`, `OnDisable`
- Try-pattern used for operations that may fail: `TryStartGrapple()`, `TryStartDash()`, `TryStartSlide()`
- Unity ContextMenu methods use numbered prefix for ordering: `"1. Test Damage Matrix"`, `"2. Create Test Enemy"`
- Private serialized fields: `camelCase` with `[SerializeField]` — `private float baseSpeed`, `private int currentAmmoOnMagazine`
- Private non-serialized backing fields: underscore prefix `_camelCase` — `_rb`, `_currentState`, `_stateMachine`
- Public properties: PascalCase expression-body getters — `public bool IsGrounded => _groundChecker.IsGrounded`
- Constants: ALL_CAPS_SNAKE_CASE — `SCROLL_SPEED_STEP`, `PITCH_CLAMP_DEGREES`, `SPEED_MIN`
- Static events: PascalCase with `On` prefix — `OnAmmoChanged`, `OnWeaponChanged`, `OnShoot`
- Instance events: PascalCase with `On` prefix — `OnHealthChanged`, `OnDeath`, `OnChunkDepleted`
- `BaseHealth.isAlive` was the historical incorrect form — current code has `IsAlive` (corrected in `BaseHealth`)
- `AIMemory.LastSeenPleyerTime` — typo in property name ("Pleyer" instead of "Player"); field exists in `Assets/Scripts/AI/AIMemory.cs` but the current file shows the corrected `LastSeenPlayerTime`
- `WeaponBase`: private fields `currentAmmoOnMagazine`, `currentAmmoOnReserves` lack underscore prefix, inconsistent with the `_camelCase` convention used elsewhere
- PascalCase for type name and all values: `MovementState.Grounded`, `AlertLevel.Unaware`, `DamageType.Kinetic`
- Nested enums placed inside their owner class: `AIMemory.AlertLevel`
- One class per file, filename matches class name: `BaseHealth.cs`, `DashingState.cs`
- No enforced file name casing inconsistency observed
- PascalCase, descriptive compound names: `FSM/`, `Data_Scripts/`, `Health/`, `Combat/`
- Underscore allowed in directory names: `Data_Scripts/`, `Audio_Scripts/`
- Subdirectory nesting used to group related types: `Health/Components/`, `Health/Data/`, `Health/Interfaces/`, `Health/Types/`
## Code Style
- No `.editorconfig`, `.prettierrc`, or `omnisharp.json` detected in project root
- Allman-style braces: opening brace on same line as declaration (K&R style observed in most files)
- Mixed indentation: some files use 4-space indent, others use tab indent (no enforced standard)
- Blank lines used liberally between method bodies
- `private` always explicit — no implicit private fields
- `protected` used for fields shared with subclasses: `healthChunks`, `damageModifiers` in `BaseHealth`
- `public` properties expose private state via expression-body getters — backing field mutation stays private
- `[SerializeField] private` is the standard pattern for inspector-visible fields
- `[Header("...")]` used to group related inspector fields — consistent across all components
- `[Tooltip("...")]` used selectively on less-obvious fields
- `[RequireComponent(typeof(...))]` used on components with hard dependencies: `PlayerInput`, `PlayerMovement`
- `[Range(0,1)]` used for normalized float fields: `movementSmoothing`
- `#region`/`#endregion` used in `PlayerMovement.cs` (`Fields`, `Properties`, `Unity Methods`)
- Not used consistently across other files — treat as optional
- Null-conditional `?.Invoke()` used for all event invocations: `OnDeath?.Invoke()`, `OnAmmoChanged?.Invoke(...)`
- Null-conditional used on optional component lookups before method calls
- Defensive null guard at top of Unity callbacks: `if (Controllable == null) return;`
## Namespaces
- `Health` namespace: all health system types (`BaseHealth`, `PlayerHealth`, `DamageInfo`, `HealthChunk`, etc.)
- `Entities.Controllers` namespace: `Controller`, `PlayerInput`
- `Strategies` namespace: `IControllable`, `IController`, `IJumpable`, `IMovable`
- `Core` namespace: `MovementState` enum, `PlayerMovementContext`
- `Core.Events` namespace: `PlayerJumpingEvent`, `PlayerLandingEvent`
- `Core.Utils` namespace: `Spring`
## Import Organization
## Comments
- Spanish comments dominant in player movement state files: `// Salir si ya no está corriendo en la pared`, `// Manejar salto desde la pared`
- English comments dominant in newer/refactored files: `SpectatorCamera`, `WeaponBase`, `ProjectileBase`
- Mixed within single files: `WallRunningState.cs` has Spanish section comments and one English comment (`// Transition to Grapple`)
- Use English for all new code — do not add Spanish comments to new files
- `<summary>` blocks used selectively on public-facing classes and utility methods: `SpectatorCamera`, `WeaponDataSO.GetDamageMultiplierAtDistance`, `HealthSystemTest`
- Not required on private methods or MonoBehaviour lifecycle callbacks
- Not used consistently — treat as encouraged but optional
- Present and self-critical in `PlayerInput.cs`: architecture dissatisfaction documented inline
- Other TODOs in `WeaponBase.cs`, `ProjectileBase.cs`, `WeaponDataSO.cs`, `PlayerLandingEvent.cs`
- No FIXME or HACK markers found
- `Debug.Log` used extensively throughout gameplay code for development feedback
- Logs use `[ClassName]` prefix brackets: `[SpectatorCamera]`, `[AIMemory]`, `[HIT]`
- `BaseState<T>.Log(string)` helper available for state classes, but not consistently used — raw `Debug.Log` also called directly
## Component Architecture
- Player entity is composed of many small MonoBehaviour components: `PlayerMovement`, `PlayerCrouch`, `PlayerSlide`, `PlayerWallRun`, `PlayerDash`, `PlayerGrapple`, `PlayerJumper`, `GroundChecker`
- Components communicate via the shared `PlayerMovementContext` object, not direct references between components
- Static C# `Action` events used for cross-object communication: `WeaponBase.OnAmmoChanged`, `PlayerInput.OnShoot`, `WeaponInventory.OnWeaponChanged`
- Instance events on components for lifecycle callbacks: `BaseHealth.OnDeath`, `BaseHealth.OnHealthChanged`
- Subscribe in `OnEnable`/`Start`, unsubscribe in `OnDisable`/`OnDestroy` — `WeaponBase` follows this correctly; `GameManager` subscribes in `Start` without a symmetric `OnDestroy`
- Always accessed via `TypeName.Instance`: `EventsManager.Instance.OnGamePause`, `AITickManager.Instance`
- Never cached into local fields — accessed at call site each time
## Error Handling
- `Debug.LogError` used for programmer errors (missing required references)
- Guard clauses with early returns: `if (!IsAlive) return;`, `if (damageMatrix == null) return;`
- No exceptions thrown in gameplay code (except `JumpCommand.Undo()` which throws `NotImplementedException`)
- `SendMessageOptions.DontRequireReceiver` used in test code to safely call lifecycle methods via reflection
## Function Design
- Methods kept short; logic split into named private helpers: `CalculateHorizontalMovement`, `CalculateVerticalMovement`, `ApplyFinalVelocity`
- Expression-body (`=>`) used for single-expression methods and properties: `public string GetWeaponName() => weaponData.WeaponName`
- Large files (`PlayerMovement.cs` at 340 lines) use `#region` blocks to aid navigation
- No methods observed with more than ~30 lines of executable logic
## Copy-Paste Patterns
- `Assets/Scripts/Player/Movement/States/DashingState.cs`
- `Assets/Scripts/Player/Movement/States/WallRunningState.cs`
- `Assets/Scripts/Player/Movement/States/GrapplingState.cs`
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

## Pattern Overview
- All gameplay entities are Unity GameObjects with composed MonoBehaviour components — no pure ECS or DI framework
- Player movement is driven by a hierarchical two-level FSM: an outer machine for locomotion mode and an inner machine for grounded sub-states
- Game-wide services (events, audio, AI scheduling, scene loading) are accessed via `Singleton<T>` instances
- ScriptableObjects carry tuning data; runtime mutable state lives in plain C# classes or component fields
- Combat damage flows through a typed pipeline: `DamageInfo` → `IDamageModifier` chain → `DamageMatrixSO` multiplier → `HealthChunk` absorption
## Core Systems
### 1. Finite State Machine
- `Assets/Scripts/Core/FSM/IState.cs` — interface: `OnEnter`, `OnUpdate`, `OnFixedUpdate`, `OnExit`
- `Assets/Scripts/Core/FSM/BaseState.cs` — abstract generic `BaseState<TContext>`, stores typed context, provides `Log()`
- `Assets/Scripts/Core/FSM/StateMachine.cs` — generic `StateMachine<TState> where TState : Enum`; dictionary of `IState`, exposes `Initialize`, `ChangeState`, `Update`, `FixedUpdate`, `ExitCurrentState`, `Clear`
```csharp
```
- Outer machine — 5 states: `Grounded`, `InAir`, `WallRunning`, `Dashing`, `Grappling`
- `GroundedState` owns a second inner `StateMachine<MovementState>` — 4 sub-states: `Walking`, `Sprinting`, `Crouching`, `Sliding`
- Both machines share the same `PlayerMovementContext` instance
- The inner machine reference is stored in `context.GroundedStateMachine` so sub-states can trigger their own transitions
### 2. Player Movement Context
- Component references: `PlayerMovement`, `PlayerCrouch`, `PlayerSlide`, `PlayerDash`, `PlayerWallRun`, `PlayerGrapple`, `Rigidbody`, `IControllable`
- Both state machine references: `StateMachine`, `GroundedStateMachine`
- Per-frame input booleans: `WantsToSprint`, `WantsToCrouch`, `WantsToJump`, `WantsToGrapple`, `WantsToDash`
- Input vectors: `MovementInput`, `DashInputDirection`
### 3. Singleton Service Layer
- `Assets/Scripts/Core/Singleton/Singleton.cs` — `Singleton<T> : MonoBehaviour`; enforces one instance via `Awake`, optional `DontDestroyOnLoad` via `_persistAcrossScenes`
| Class | File | Purpose |
|---|---|---|
| `EventsManager` | `Assets/Scripts/Managers/EventsManager.cs` | Global C# `Action` events |
| `GameManager` | `Assets/Scripts/Managers/GameManager.cs` | Pause/GameOver/restart logic |
| `AITickManager` | `Assets/Scripts/AI/AITickManager.cs` (also `Managers/`) | Round-robin AI tick scheduling |
| `AudioManager` | `Assets/Scripts/Managers/AudioManager.cs` | Sound playback by name (not a Singleton subclass) |
### 4. Event System
- `OnGamePause(bool isPaused)` — listened to by `GameManager`, `AITickManager`
- `OnGameOver()` — stops time, pauses AI
- `OnGameExit()` — loads main menu via `SceneLoader`
- `OnGameRestart()` — loads game scene via `SceneLoader`
- `PlayerInput.OnShoot`, `PlayerInput.OnChangeWeapon`, `PlayerInput.OnReload`
- `WeaponBase.OnAmmoChanged(int magazine, int reserves)`
- `WeaponInventory.OnWeaponChanged(string weaponName)`
### 5. Health System
- `Assets/Scripts/Health/Components/BaseHealth.cs` — abstract, holds `List<HealthChunk>` + `List<IDamageModifier>`
- `Assets/Scripts/Health/Components/PlayerHealth.cs` — 1 main chunk + N side chunks, all `HealthType.Player`
- `Assets/Scripts/Health/Components/EnemyHealth.cs` — chunk list driven by `EnemyHealthConfigSO`
- `Assets/Scripts/Health/HealthChunk.cs` — plain C# class, holds `float _currentHealth`, returns overflow on `ApplyDamage`
- `Assets/Scripts/Data_Scripts/Health/DamageMatrixSO.cs` — lookup table mapping `(DamageType, HealthType)` → float multiplier
- `Assets/Scripts/Health/Components/PlayerAdrenaline.cs`
- Implements `IDamageModifier` (`Priority = 100`)
- Reads `PlayerMovement.Momentum01` each `Update` to derive 0–4 adrenaline level
- Reduces `baseDamage` by `level * 10%` (up to 40% reduction at max momentum)
### 6. Input System
### 7. Combat / Weapon System
- `Assets/Scripts/Combat/Weapons/WeaponBase.cs` — abstract `MonoBehaviour`, holds `WeaponDataSO`, manages ammo counters, fires `OnAmmoChanged`
- `Assets/Scripts/Combat/Weapons/Pistol.cs`, `Shotgun.cs` — concrete implementations
- `Assets/Scripts/Combat/Weapons/WeaponInventory.cs` — cycle through `List<WeaponBase>`, activates/deactivates GameObjects, fires `OnWeaponChanged`
- `Assets/Scripts/Combat/ProjectileBase.cs` — manual simulation (no Rigidbody); per-frame `Physics.Raycast`, deactivates on hit or lifetime expiry
- `Assets/Scripts/Data_Scripts/Combat/WeaponDataSO.cs` — all tuning values including damage falloff `AnimationCurve`
```
```
### 8. AI System
- `Assets/Scripts/AI/ITickable.cs` — `void OnTick(float deltaTime)` + `bool IsTickable`
- `Assets/Scripts/AI/AITickManager.cs` — singleton, round-robin list of `ITickable`, configurable `_ticksPerFrame` (1–5)
- `Assets/Scripts/AI/TestEnemy.cs` — `ITickable` + Unity `NavMeshAgent`; registers in `Awake`, unregisters in `OnDestroy`
- `Assets/Scripts/AI/AIMemory.cs` — per-agent memory: `AlertLevel` enum, `LastKnownPlayerPosition`, `CanSeePlayer`
- `Assets/Scripts/AI/AIPerception.cs` — empty (1 line stub)
### 9. Scene Management
- `Assets/Scripts/Managers/SceneLoader.cs` — static class; always routes through a `LoadingScene` intermediary
- `Assets/Scripts/Managers/LoadingManager.cs` — runs async load from `LoadingScene`
### 10. Momentum System
## Data Flow
### Player Input → Physics Frame
```
```
### Damage → Death
```
```
### Game State Events
```
```
## Key Abstractions
- Every player movement state inherits `BaseState<PlayerMovementContext>`
- States must not store mutable runtime state of their own; they read/write via `Context`
- Files: `Assets/Scripts/Player/Movement/States/`
- `IControllable` composes `IMovable` (`.Move(Vector2)`, `.SetMovementState()`) and `IJumpable` (`.Jump()`, `.SetHoldingJump()`)
- `PlayerMovement` implements this; `PlayerInput` holds a reference and drives it
- Files: `Assets/Scripts/Player/Interfaces/`
- `TakeDamage(DamageInfo)` + `bool IsAlive`
- All damageable entities implement this; `ProjectileBase` uses `GetComponentInParent<IDamagable>()`
- File: `Assets/Scripts/Health/IDamagable.cs`
- `float ModifyDamage(float baseDamage, DamageInfo damageInfo)` + `int Priority`
- Registered on `BaseHealth` at runtime; supports stacking modifiers (armor, adrenaline, etc.)
- File: `Assets/Scripts/Health/Interfaces/IDamageModifier.cs`
- AI agents implement this to participate in `AITickManager` round-robin
- File: `Assets/Scripts/AI/ITickable.cs`
## Error Handling
- `StateMachine.ChangeState` logs error and returns without crashing if state is unregistered
- `PlayerInput` logs error if `IControllable` is null, then checks `if (Controllable == null) return` each frame
- `BaseHealth.TakeDamage` guards on `!IsAlive`
- `AITickManager` guards on `EventsManager.Instance == null` before subscribing
## Cross-Cutting Concerns
- `Assets/Scripts/Core/Command/` — `CommandInvoker`, `JumpCommand`, `MoveCommand` all throw `NotImplementedException`; not wired into anything
- `Assets/Scripts/Player/Movement/Strategies/` — `IMovementStrategy`, `PlayerMovementStrategy`, `IJumpStrategy`, `PlayerJumpStrategy` exist but are not called from the active movement pipeline; leftover from an earlier refactor
- `Assets/Scripts/AI/AIPerception.cs` — empty stub (1 line)
<!-- GSD:architecture-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd:quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd:debug` for investigation and bug fixing
- `/gsd:execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd:profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
