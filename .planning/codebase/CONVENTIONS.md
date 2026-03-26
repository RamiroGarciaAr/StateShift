# Coding Conventions

**Analysis Date:** 2026-03-26

## Naming Patterns

**Classes:**
- PascalCase throughout: `BaseHealth`, `PlayerMovementContext`, `WeaponInventory`
- MonoBehaviour components use descriptive compound names: `PlayerCrouch`, `PlayerWallRun`, `GroundChecker`
- Abstract base classes prefixed with `Base`: `BaseHealth`, `BaseState<T>`, `WeaponBase`
- State classes suffixed with `State`: `DashingState`, `WallRunningState`, `GrapplingState`
- ScriptableObjects suffixed with `SO`: `WeaponDataSO`, `DamageMatrixSO`, `PlayerHealthConfigSO`
- Singletons inherit `Singleton<T>` and are named by responsibility: `EventsManager`, `GameManager`, `AITickManager`
- Interfaces prefixed with `I`: `IWeapon`, `IDamagable`, `ITickable`, `IControllable`
- Manager scripts end with `Manager`: `EventsManager`, `AudioManager`, `AITickManager`

**Methods:**
- PascalCase for all public and protected methods: `TakeDamage`, `InitializeChunks`, `ExitToAppropriateState`
- Private methods also PascalCase: `CalculateHorizontalMovement`, `UpdateContext`, `ApplyFinalVelocity`
- Boolean-returning methods use "Is/Has/Can/Was" prefixes: `IsGrounded`, `HasWall`, `CanGrapple`, `WasGrounded`
- Unity callbacks in standard form: `Awake`, `Start`, `Update`, `FixedUpdate`, `OnEnable`, `OnDisable`
- Try-pattern used for operations that may fail: `TryStartGrapple()`, `TryStartDash()`, `TryStartSlide()`
- Unity ContextMenu methods use numbered prefix for ordering: `"1. Test Damage Matrix"`, `"2. Create Test Enemy"`

**Fields and Properties:**
- Private serialized fields: `camelCase` with `[SerializeField]` — `private float baseSpeed`, `private int currentAmmoOnMagazine`
- Private non-serialized backing fields: underscore prefix `_camelCase` — `_rb`, `_currentState`, `_stateMachine`
- Public properties: PascalCase expression-body getters — `public bool IsGrounded => _groundChecker.IsGrounded`
- Constants: ALL_CAPS_SNAKE_CASE — `SCROLL_SPEED_STEP`, `PITCH_CLAMP_DEGREES`, `SPEED_MIN`
- Static events: PascalCase with `On` prefix — `OnAmmoChanged`, `OnWeaponChanged`, `OnShoot`
- Instance events: PascalCase with `On` prefix — `OnHealthChanged`, `OnDeath`, `OnChunkDepleted`

**Known Casing Violations:**
- `BaseHealth.isAlive` was the historical incorrect form — current code has `IsAlive` (corrected in `BaseHealth`)
- `AIMemory.LastSeenPleyerTime` — typo in property name ("Pleyer" instead of "Player"); field exists in `Assets/Scripts/AI/AIMemory.cs` but the current file shows the corrected `LastSeenPlayerTime`
- `WeaponBase`: private fields `currentAmmoOnMagazine`, `currentAmmoOnReserves` lack underscore prefix, inconsistent with the `_camelCase` convention used elsewhere

**Enums:**
- PascalCase for type name and all values: `MovementState.Grounded`, `AlertLevel.Unaware`, `DamageType.Kinetic`
- Nested enums placed inside their owner class: `AIMemory.AlertLevel`

**Files:**
- One class per file, filename matches class name: `BaseHealth.cs`, `DashingState.cs`
- No enforced file name casing inconsistency observed

**Directories:**
- PascalCase, descriptive compound names: `FSM/`, `Data_Scripts/`, `Health/`, `Combat/`
- Underscore allowed in directory names: `Data_Scripts/`, `Audio_Scripts/`
- Subdirectory nesting used to group related types: `Health/Components/`, `Health/Data/`, `Health/Interfaces/`, `Health/Types/`

## Code Style

**Formatting:**
- No `.editorconfig`, `.prettierrc`, or `omnisharp.json` detected in project root
- Allman-style braces: opening brace on same line as declaration (K&R style observed in most files)
- Mixed indentation: some files use 4-space indent, others use tab indent (no enforced standard)
- Blank lines used liberally between method bodies

**Access Modifiers:**
- `private` always explicit — no implicit private fields
- `protected` used for fields shared with subclasses: `healthChunks`, `damageModifiers` in `BaseHealth`
- `public` properties expose private state via expression-body getters — backing field mutation stays private

**Inspector Exposure:**
- `[SerializeField] private` is the standard pattern for inspector-visible fields
- `[Header("...")]` used to group related inspector fields — consistent across all components
- `[Tooltip("...")]` used selectively on less-obvious fields
- `[RequireComponent(typeof(...))]` used on components with hard dependencies: `PlayerInput`, `PlayerMovement`
- `[Range(0,1)]` used for normalized float fields: `movementSmoothing`

**Regions:**
- `#region`/`#endregion` used in `PlayerMovement.cs` (`Fields`, `Properties`, `Unity Methods`)
- Not used consistently across other files — treat as optional

**Null Checks:**
- Null-conditional `?.Invoke()` used for all event invocations: `OnDeath?.Invoke()`, `OnAmmoChanged?.Invoke(...)`
- Null-conditional used on optional component lookups before method calls
- Defensive null guard at top of Unity callbacks: `if (Controllable == null) return;`

## Namespaces

Usage is partial and inconsistent:
- `Health` namespace: all health system types (`BaseHealth`, `PlayerHealth`, `DamageInfo`, `HealthChunk`, etc.)
- `Entities.Controllers` namespace: `Controller`, `PlayerInput`
- `Strategies` namespace: `IControllable`, `IController`, `IJumpable`, `IMovable`
- `Core` namespace: `MovementState` enum, `PlayerMovementContext`
- `Core.Events` namespace: `PlayerJumpingEvent`, `PlayerLandingEvent`
- `Core.Utils` namespace: `Spring`

Most gameplay classes (states, managers, weapons, AI) are in the **global namespace** (no namespace declaration). This is inconsistent with the namespaced subsystems above.

## Import Organization

No enforced ordering. Observed pattern:
1. `System.*` / `System.Collections.*`
2. `UnityEngine.*`
3. `UnityEngine.InputSystem`
4. Local namespace usings (e.g., `using Health;`, `using Core;`, `using Strategies;`)

## Comments

**Language mixing — no enforced convention:**
- Spanish comments dominant in player movement state files: `// Salir si ya no está corriendo en la pared`, `// Manejar salto desde la pared`
- English comments dominant in newer/refactored files: `SpectatorCamera`, `WeaponBase`, `ProjectileBase`
- Mixed within single files: `WallRunningState.cs` has Spanish section comments and one English comment (`// Transition to Grapple`)
- Use English for all new code — do not add Spanish comments to new files

**XML Documentation:**
- `<summary>` blocks used selectively on public-facing classes and utility methods: `SpectatorCamera`, `WeaponDataSO.GetDamageMultiplierAtDistance`, `HealthSystemTest`
- Not required on private methods or MonoBehaviour lifecycle callbacks
- Not used consistently — treat as encouraged but optional

**TODO Comments:**
- Present and self-critical in `PlayerInput.cs`: architecture dissatisfaction documented inline
- Other TODOs in `WeaponBase.cs`, `ProjectileBase.cs`, `WeaponDataSO.cs`, `PlayerLandingEvent.cs`
- No FIXME or HACK markers found

**Debug Logging:**
- `Debug.Log` used extensively throughout gameplay code for development feedback
- Logs use `[ClassName]` prefix brackets: `[SpectatorCamera]`, `[AIMemory]`, `[HIT]`
- `BaseState<T>.Log(string)` helper available for state classes, but not consistently used — raw `Debug.Log` also called directly

## Component Architecture

**MonoBehaviour composition over inheritance:**
- Player entity is composed of many small MonoBehaviour components: `PlayerMovement`, `PlayerCrouch`, `PlayerSlide`, `PlayerWallRun`, `PlayerDash`, `PlayerGrapple`, `PlayerJumper`, `GroundChecker`
- Components communicate via the shared `PlayerMovementContext` object, not direct references between components

**Event-driven decoupling:**
- Static C# `Action` events used for cross-object communication: `WeaponBase.OnAmmoChanged`, `PlayerInput.OnShoot`, `WeaponInventory.OnWeaponChanged`
- Instance events on components for lifecycle callbacks: `BaseHealth.OnDeath`, `BaseHealth.OnHealthChanged`
- Subscribe in `OnEnable`/`Start`, unsubscribe in `OnDisable`/`OnDestroy` — `WeaponBase` follows this correctly; `GameManager` subscribes in `Start` without a symmetric `OnDestroy`

**Singleton access:**
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

`ExitToAppropriateState()` is duplicated identically in three state files:
- `Assets/Scripts/Player/Movement/States/DashingState.cs`
- `Assets/Scripts/Player/Movement/States/WallRunningState.cs`
- `Assets/Scripts/Player/Movement/States/GrapplingState.cs`

All three contain the exact same 4-line implementation. New states requiring this logic should not copy it again — this is a candidate for extraction to `BaseState<PlayerMovementContext>` as a protected helper.

---

*Convention analysis: 2026-03-26*
