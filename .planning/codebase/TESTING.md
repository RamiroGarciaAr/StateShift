# Testing Patterns

**Analysis Date:** 2026-03-26

## Test Framework

**Runner:** None. No automated test framework is present.
- No NUnit, Unity Test Framework (UTF), or any other test runner is configured
- No `EditMode` or `PlayMode` test assemblies exist
- No `.asmdef` files scoped to testing were found
- `Assets/Scripts/Testing/` is a regular runtime folder, not an Editor or test assembly

**Assertion Library:** None — `Debug.Log` + manual `Mathf.Approximately` comparisons are used in place of assertions.

**Run Commands:**
```
N/A — no test runner configured
```

## Test File Organization

**There are two separate testing locations with different purposes:**

### 1. `Assets/Scripts/Testing/` — Runtime Test Scene Components
These are `MonoBehaviour` scripts designed to be placed in a Unity scene for manual in-editor testing. They are compiled into production builds.

- `Assets/Scripts/Testing/BulletTestSpawner.cs` — spawns a bullet prefab on Space key press; uses `Input.GetKeyDown` (legacy Input Manager, not the project's New Input System)
- `Assets/Scripts/Testing/DamageTestTarget.cs` — stub `IDamagable` that logs hit data to console; `IsAlive` always returns `true`
- `Assets/Scripts/Testing/NavMeshTester.cs` — empty class, no implementation
- `Assets/Scripts/Testing/SpectatorCamera.cs` — fully functional noclip spectator camera for observing gameplay; includes `#if UNITY_EDITOR` gizmo; uses New Input System correctly

### 2. `Assets/Scripts/Health/Tests/HealthSystemTest.cs` — ContextMenu-Driven Health Test
A `MonoBehaviour` that exposes test operations as Unity Inspector `[ContextMenu]` entries. Placed on a GameObject in a test scene and triggered manually from the Inspector right-click menu.
- Location: `Assets/Scripts/Health/Tests/HealthSystemTest.cs`
- Also compiled into production builds (not Editor-only)

## Test Structure

**No describe/it blocks or test suites exist.** The only structured testing is the `[ContextMenu]` pattern in `HealthSystemTest`:

```csharp
// Trigger from Inspector right-click menu, in numbered order
[ContextMenu("1. Test Damage Matrix")]
public void TestDamageMatrix() { ... }

[ContextMenu("2. Create Test Enemy")]
public void CreateTestEnemy() { ... }

[ContextMenu("3. Damage Test Enemy")]
public void DamageTestEnemy() { ... }

[ContextMenu("4. Create Test Player")]
public void CreateTestPlayer() { ... }

[ContextMenu("5. Damage Test Player")]
public void DamageTestPlayer() { ... }

[ContextMenu("6. Restore Player Side Chunk")]
public void RestorePlayerSideChunk() { ... }

[ContextMenu("7. Cleanup Test Objects")]
public void CleanupTestObjects() { ... }
```

**Assertion pattern** (manual, no framework):
```csharp
private void AssertMultiplier(DamageType damage, HealthType health, float expected, string desc)
{
    float actual = damageMatrix.GetMultiplier(damage, health);
    bool pass = Mathf.Approximately(actual, expected);
    string result = pass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>";
    Debug.Log($"{result}: {desc} (expected {expected}, got {actual})");
}
```
Results are read visually from the Unity Console. No assertion throws on failure.

## Mocking

**Framework:** None.

**Pattern used:** Reflection to set private serialized fields on MonoBehaviours before testing:
```csharp
// From HealthSystemTest.cs — sets private [SerializeField] field for test setup
var configField = typeof(EnemyHealth).GetField("healthConfig",
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
configField?.SetValue(_testEnemy, enemyConfig);
```

**Limitations:**
- Field names are strings — rename refactors silently break test setup
- `SendMessage("Awake", ...)` is used to re-initialize after reflection injection, which is fragile
- No interface-based substitution or dependency injection in gameplay code

## Fixtures and Factories

**Test data:** ScriptableObject assets assigned in the Inspector at test time:
- `DamageMatrixSO` — assigned to `HealthSystemTest._damageMatrix`
- `EnemyHealthConfigSO` — assigned to `HealthSystemTest._enemyConfig`
- `PlayerHealthConfigSO` — assigned to `HealthSystemTest._playerConfig`

**Object creation:** `new GameObject(...)` + `AddComponent<T>()` used directly in test methods:
```csharp
var enemyGO = new GameObject("TestEnemy");
_testEnemy = enemyGO.AddComponent<EnemyHealth>();
```

Cleanup is manual via `[ContextMenu("7. Cleanup Test Objects")]` calling `DestroyImmediate`.

## Coverage

**Requirements:** None enforced.

**Covered by any form of testing:**
- `BaseHealth.TakeDamage` flow — via `HealthSystemTest` context menu
- `DamageMatrixSO.GetMultiplier` — via `TestDamageMatrix()` with manual `AssertMultiplier` calls
- `PlayerHealth.RestoreSideChunk` / `CanRestoreSideChunk` — via context menu
- `EnemyHealth` chunk depletion and death events — via context menu
- Projectile hit detection — via `BulletTestSpawner` in-scene manual test

**Not covered by any test:**
- `StateMachine<T>` state transitions — no tests; only testable via running the game
- All player movement states (`DashingState`, `WallRunningState`, `GrapplingState`, etc.)
- `PlayerGrapple`, `PlayerDash`, `PlayerWallRun`, `PlayerSlide`, `PlayerCrouch`, `PlayerJumper`
- `PlayerInput` context update logic and input-to-state wiring
- `WeaponBase.TryShoot` / `TryReload` ammo arithmetic
- `WeaponInventory` cycling logic
- `Singleton<T>` duplicate detection
- `EventsManager` event dispatch
- `GameManager` scene/timescale side effects
- `AITickManager` round-robin tick distribution
- All platform environment scripts

## Test Types

**Unit Tests:** Not implemented. No test runner, no isolated unit tests.

**Integration Tests:** Not implemented in any formal sense.

**Manual In-Editor Tests:** The only testing approach. Two patterns:
1. `[ContextMenu]` methods on a MonoBehaviour placed in a test scene (health system only)
2. Scene-placed `MonoBehaviour` components in `Assets/Scripts/Testing/` (combat and movement observation)

**E2E Tests:** Not used.

## Critical Issues with Current Test Setup

**Tests ship in production builds.**
All files in `Assets/Scripts/Testing/` and `Assets/Scripts/Health/Tests/HealthSystemTest.cs` are in regular `Assets/Scripts/` subdirectories with no assembly definition scoping. They compile into every build. There is no `Editor/` isolation, no `[assembly: InternalsVisibleTo(...)]` test assembly, and no platform compilation guards (`#if UNITY_EDITOR`) on the test MonoBehaviours themselves (only `SpectatorCamera` uses `#if UNITY_EDITOR` for its gizmo, not for the class itself).

**`NavMeshTester.cs` is an empty stub** at `Assets/Scripts/Testing/NavMeshTester.cs`. It compiles but does nothing.

**`BulletTestSpawner.cs` uses legacy Input** (`Input.GetKeyDown(KeyCode.Space)`) while the rest of the project uses Unity's New Input System. This inconsistency means it will not respect input system rebindings.

**Test assertions do not throw** — a failing `AssertMultiplier` call logs a red console message but does not halt execution or mark anything as definitively failed. Failures can be missed if the Console window is not open.

## Recommended Testing Approach for New Code

Until a proper test framework is adopted, follow the existing `[ContextMenu]` pattern for new systems that need manual validation. If adding a Unity Test Framework (`com.unity.test-framework`) package:

- Place EditMode tests in `Assets/Tests/EditMode/`
- Place PlayMode tests in `Assets/Tests/PlayMode/`
- Create dedicated `.asmdef` files for each, referencing production assembly definitions
- Gate test-only MonoBehaviours with `#if UNITY_EDITOR` or move them to `Editor/` folders to exclude from builds

---

*Testing analysis: 2026-03-26*
