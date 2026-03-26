# Codebase Concerns

**Analysis Date:** 2026-03-26

---

## P0 — Active Bugs (Break Gameplay)

**Grapple Invoke not cancelled → permanent lockout:**
- Issue: `PlayerGrapple.TryStartGrapple()` calls `Invoke(nameof(StartGrapple), grappleDelayTime)` but `CancelGrapple()` only calls `EndGrapple()` — it never calls `CancelInvoke()`. If grapple is cancelled during the delay window, `StartGrapple()` fires anyway after the delay, setting `_isGrappling = true` with no active grapple. `CanGrapple` then returns false permanently because `!_isGrappling` is never satisfied.
- File: `Assets/Scripts/Player/Movement/PlayerGrapple.cs` lines 72, 116–122
- Fix: Add `CancelInvoke(nameof(StartGrapple))` at the top of `CancelGrapple()`.

**GrapplingState exits immediately on enter:**
- Issue: `GrapplingState.OnUpdate()` checks `!Context.PlayerGrapple.IsGrappling` and exits if true. `IsGrappling` is only set to true inside `StartGrapple()`, which fires after `grappleDelayTime` (0.1s). The state is entered before the delay expires, so the first `OnUpdate()` call always exits immediately, transitioning the machine away before the grapple activates.
- File: `Assets/Scripts/Player/Movement/States/GrapplingState.cs` lines 16–20
- Fix: Track that a grapple launch is pending (e.g. check `_cooldownTimer <= 0` is still ticking down, or expose a `IsPending` flag on `PlayerGrapple`).

**Damage overflow multiplier corrupts subsequent chunks:**
- Issue: In `BaseHealth.TakeDamage()`, `remainingDamage` passed to `healthChunks[i].ApplyDamage()` is already multiplied by the chunk's type multiplier. The overflow returned from `ApplyDamage()` is the remainder of the already-multiplied value. That inflated remainder is then multiplied *again* by the next chunk's multiplier on the next loop iteration.
- File: `Assets/Scripts/Health/Components/BaseHealth.cs` lines 44–58
- Impact: Any multi-chunk enemy hit with a damage-type that has a multiplier >1 takes more damage than intended on every chunk after the first breach.
- Fix: Compute `remainingDamage = healthChunks[i].ApplyDamage(remainingDamage) * multiplier` with the multiplier applied only to the absorbed portion, or apply the multiplier before calling `ApplyDamage` and return raw overflow.

**CanGrapple race allows double-grapple during delay:**
- Issue: `CanGrapple` returns `_cooldownTimer <= 0 && !_isGrappling`. During `grappleDelayTime`, `_isGrappling` is still false and the cooldown has not started (it starts in `EndGrapple()`). A second `TryStartGrapple()` call during this window succeeds, queuing a second `Invoke(StartGrapple)` on top of the first.
- File: `Assets/Scripts/Player/Movement/PlayerGrapple.cs` line 32, line 63–77
- Fix: Introduce a `_isPendingLaunch` flag set in `TryStartGrapple()` and cleared in `StartGrapple()` / `CancelGrapple()`. Include it in the `CanGrapple` guard.

---

## P1 — Significant Tech Debt (Causes Future Pain)

**PlayerInput owns the state machine (architectural violation):**
- Issue: `PlayerInput` constructs and owns the `StateMachine<MovementState>` and all state instances. The class itself has two self-annotated TODOs marking this as wrong. Input handling, state wiring, and camera-relative direction calculation all live in one 180-line class that is explicitly described as "awful" in the source comment.
- File: `Assets/Scripts/Player/PlayerInput.cs` lines 8, 64, 140
- Impact: Cannot reuse the FSM independently, cannot test states without a fully wired input component, breaks separation of concerns.
- Fix: Extract state machine construction to a dedicated `PlayerMovementBrain` or `PlayerController` component. `PlayerInput` should only read hardware and post flags/events.

**StateMachine.Clear() does not reset `_currentStateType`:**
- Issue: `Clear()` sets `_currentState = null` and clears the dictionary but leaves `_currentStateType` at its last value. Any consumer reading `CurrentStateType` after `Clear()` gets a stale enum value.
- File: `Assets/Scripts/Core/FSM/StateMachine.cs` lines 81–86
- Fix: Add `_currentStateType = default;` inside `Clear()`.

**ExitToAppropriateState() copy-pasted across three states:**
- Issue: Identical private method exists in `DashingState`, `WallRunningState`, and `GrapplingState` — each checks `IsGrounded` and transitions to Grounded or InAir. Any logic change must be made in three places.
- Files: `Assets/Scripts/Player/Movement/States/DashingState.cs` line 46, `Assets/Scripts/Player/Movement/States/WallRunningState.cs` line 75, `Assets/Scripts/Player/Movement/States/GrapplingState.cs` line 49
- Fix: Move to `BaseState<PlayerMovementContext>` as a protected method, or to a static helper.

**Dead sliding branch in CalculateHorizontalMovement:**
- Issue: `UpdateMovement()` has an early return at line 228 for `MovementState.Sliding` that returns before reaching `CalculateHorizontalMovement()`. Inside `CalculateHorizontalMovement()` there is a branch for `MovementState.Sliding && IsGrounded` (lines 280–288) that can never be reached.
- File: `Assets/Scripts/Player/Movement/PlayerMovement.cs` lines 215–233, 280–288
- Impact: The slide velocity lerp logic at line 288 is effectively dead code, likely a refactor artefact. Maintenance risk.

**Orphaned strategy interfaces never connected:**
- Issue: `IMovementStrategy`, `IJumpStrategy`, `IJump` and their implementations (`PlayerMovementStrategy`, `PlayerJumpStrategy`) exist in `Assets/Scripts/Player/Movement/Strategies/` but are never referenced by `PlayerMovement`, `PlayerJumper`, or any consumer. `PlayerJumper` implements its own inline jump logic. `PlayerMovementStrategy.ApplyMovement()` uses a completely different velocity model than `PlayerMovement.CalculateHorizontalMovement()`.
- Files: `Assets/Scripts/Player/Movement/Strategies/` (all files)
- Impact: Dead code in production. Confusing for future devs: two parallel movement systems exist, only one is active.
- Fix: Delete the unused strategy files or wire them in and delete the inline duplicates.

**CommandInvoker / JumpCommand / MoveCommand are unused scaffolding:**
- Issue: `CommandInvoker` is a static undo-stack, `JumpCommand.Undo()` and `MoveCommand.Undo()` both throw `NotImplementedException`. Neither command is ever invoked anywhere in the codebase.
- Files: `Assets/Scripts/Core/Command/CommandInvoker.cs`, `Assets/Scripts/Core/Command/JumpCommand.cs`, `Assets/Scripts/Core/Command/MoveCommand.cs`
- Impact: Dead code in production build. The `NotImplementedException` in Undo paths is a runtime crash risk if anyone calls `CommandInvoker.UndoCommand()`.
- Fix: Either implement the command pattern fully or delete these files until the feature is scoped.

**Weapon reload does not respect ammo reserves correctly:**
- Issue: `WeaponBase.TryReload()` always sets `currentAmmoOnMagazine = weaponData.MagazineSize` regardless of how much reserve ammo is available. If `currentAmmoOnReserves < MagazineSize`, the player still gets a full magazine. `currentAmmoOnReserves` also goes negative and is clamped post-hoc rather than being checked before reloading.
- File: `Assets/Scripts/Combat/Weapons/WeaponBase.cs` lines 58–65
- Impact: Player can reload to full magazine from 1 reserve bullet. Reserve tracking is misleading.

**Shotgun fires a single projectile:**
- Issue: `Shotgun.Shoot()` instantiates one `ProjectileBase`, identical to `Pistol.Shoot()`. There is no pellet spread, no multiple projectile spawning. The class is effectively a `Pistol` reskin.
- File: `Assets/Scripts/Combat/Weapons/Shotgun.cs`

---

## P2 — Performance Issues

**`Camera.main` called every Update frame (multiple locations):**
- Issue: `PlayerInput.CalculateCameraRelativeDirection()` calls `Camera.main` every frame. `PlayerWallRun.Awake()` caches it, but `PlayerGrapple.Awake()` also caches it — the pattern is inconsistent. `PlayerInput` does not cache.
- Files: `Assets/Scripts/Player/PlayerInput.cs` line 143, `Assets/Scripts/Player/Movement/PlayerGrapple.cs` line 40 (cached, correct), `Assets/Scripts/Player/Movement/PlayerWallRun.cs` line 52 (cached, correct)
- Fix: Cache `Camera.main` in `Awake()` inside `PlayerInput` and use the cached reference.

**`Debug.DrawLine` / `Debug.DrawRay` called every Update in production builds:**
- Issue: `GroundChecker.CheckGround()` calls `Debug.DrawLine` unconditionally every FixedUpdate. `PlayerWallRun.CheckForWall()` calls `Debug.DrawRay` unconditionally every Update. These calls are stripped in production *only* if wrapped in `#if UNITY_EDITOR` or `Debug.isDebugBuild` guards.
- Files: `Assets/Scripts/Player/Movement/GroundChecker.cs` lines 95, 115; `Assets/Scripts/Player/Movement/PlayerWallRun.cs` lines 100–103
- Impact: Minor CPU cost per frame, noise in performance profiler.

**`PlayerAdrenaline` calls `_playerMovement.Momentum01` every Update:**
- Issue: `UpdateAdrenalineLevel()` is called in `Update()` and reads `Momentum01` every frame to recalculate adrenaline level. The result only changes when `_momentum` crosses a threshold. Should use an event or dirty flag.
- File: `Assets/Scripts/Health/Components/PlayerAdrenaline.cs` lines 47–50

**`UIAnimationManager.OnDisable` listener removal is broken:**
- Issue: In `OnDisable`, `RemoveListener` is called with a new lambda `() => SetTrigger(trigger)`. This is a different delegate instance than the one added in `OnEnable`, so it will never match and the listener is never actually removed — causing both a memory leak and duplicate invocations if the object is disabled and re-enabled.
- File: `Assets/Scripts/Managers/UIAnimationManager.cs` lines 38–45
- Fix: Cache the delegates per mapping or use `button.onClick.RemoveAllListeners()`.

---

## P3 — Code Quality and Maintainability

**WallRunningState exits when sprint is released (likely unintentional):**
- Issue: Line 58 in `WallRunningState.OnUpdate()` checks `!Context.WantsToSprint` and stops the wall run if sprint is not held. Wall running is typically expected to continue as long as the player is touching a wall and has speed, not contingent on holding sprint. The crouch-cancel check on the same line may be the intended guard; the sprint check is likely an oversight.
- File: `Assets/Scripts/Player/Movement/States/WallRunningState.cs` line 58

**`AudioManager` is not a Singleton and plays a hardcoded clip on Start:**
- Issue: `AudioManager` does not extend `Singleton<T>`. It plays `"Flight"` unconditionally in `Start()` — there is no scene context check. The loop line is commented out (`//s.source.loop = s.loop`), so no sounds loop. Linear array search via `Array.Find` on every `Play()` call is O(n).
- File: `Assets/Scripts/Managers/AudioManager.cs` lines 10–18, 21–33
- Impact: Audio system is effectively a placeholder. Not wired to any game events.

**`GameManager.GameOverListener` only unsubscribes two of four events:**
- Issue: `GameOverListener()` unsubscribes `OnGamePause` and `OnGameOver` but leaves `OnGameExit` and `OnGameRestart` subscribed after game over. If a restart or exit is triggered after game over, the listeners fire against a potentially invalid game state (TimeScale = 0, scene may be mid-transition).
- File: `Assets/Scripts/Managers/GameManager.cs` lines 25–31

**`SceneLoader._nextScene` is static and not reset between loads:**
- Issue: `SceneLoader._nextScene` is a static string. If `LoadNextAsync()` is called without first calling `OpenLoadingScene()`, it will attempt to load whatever the last-used scene name was (or null). No null guard exists on `_nextScene` in `LoadNextAsync()`.
- File: `Assets/Scripts/Managers/SceneLoader.cs` lines 5–16

**`WaypointController` uses legacy `Input.GetMouseButtonDown` mixed with New Input System:**
- Issue: `WaypointController.HandleDestinationInput()` uses `Input.GetMouseButtonDown(0)` (old Input Manager), while all other input in the project uses Unity's New Input System. This creates an inconsistent input layer and breaks if the old Input Manager is disabled.
- File: `Assets/Scripts/AI/WaypointController.cs` line 45

**`AIMemory` stores player visibility state but `CanSeePlayer` is never updated:**
- Issue: `AIMemory.CanSeePlayer` is declared as a public property initialized to `false` but there is no setter and no method that mutates it. `LastSeenPlayerTime` and `LastKnownPlayerPosition` are similarly declared but never written. `AIPerception.cs` is an empty file (1 line). The memory component is structurally complete but functionally disconnected.
- Files: `Assets/Scripts/AI/AIMemory.cs`, `Assets/Scripts/AI/AIPerception.cs`

**`GroundChecker` imports `Unity.VisualScripting` without using it:**
- Issue: `using Unity.VisualScripting;` appears at the top of `GroundChecker.cs` with no usage in the file. Indicates a leftover import.
- File: `Assets/Scripts/Player/Movement/GroundChecker.cs` line 3

**`PlayerMovement` imports `Unity.VisualScripting` without using it:**
- Issue: Same unused import present in `PlayerMovement.cs`.
- File: `Assets/Scripts/Player/Movement/PlayerMovement.cs` line 5

**Mixed Spanish/English in identifiers and comments:**
- Issue: Spanish-language comments and error strings appear throughout: `GroundedState` ("Salir si ya no está corriendo"), `StateMachine` ("Estado inicial X no registrado"), `PlayerMovement`, `PlayerWallRun`. No enforced convention exists.
- Impact: Inconsistent language makes the codebase harder to onboard into for English-only contributors.

---

## P4 — Test Coverage and Build Hygiene

**Testing code compiles into production builds:**
- Issue: `Assets/Scripts/Testing/` contains `BulletTestSpawner`, `DamageTestTarget`, `NavMeshTester`, and `SpectatorCamera`. This directory is NOT under an `Editor/` folder, so all four scripts compile into the player build. `BulletTestSpawner` uses legacy `Input.GetKeyDown` which depends on the old Input Manager. `DamageTestTarget` always reports `IsAlive = true` regardless of damage received, which would silently fail any damage test placed in a live scene.
- Files: `Assets/Scripts/Testing/BulletTestSpawner.cs`, `Assets/Scripts/Testing/DamageTestTarget.cs`, `Assets/Scripts/Testing/NavMeshTester.cs`, `Assets/Scripts/Testing/SpectatorCamera.cs`
- Fix: Move to `Assets/Scripts/Testing/Editor/` or wrap with `#if UNITY_EDITOR`.

**`NavMeshTester` is an empty class:**
- Issue: The file contains only a class declaration with no body. Compiles to a no-op MonoBehaviour.
- File: `Assets/Scripts/Testing/NavMeshTester.cs`

**`HealthSystemTest` uses reflection to set private fields:**
- Issue: `HealthSystemTest.CreateTestEnemy()` and `CreateTestPlayer()` use `System.Reflection` to inject serialized fields that cannot be set via constructor. If field names change during refactoring, the reflection calls silently fail (field returns null) and `Awake()` logs an error rather than throwing, making failures subtle.
- File: `Assets/Scripts/Health/Tests/HealthSystemTest.cs` lines 75–81
- Note: This file is in `Assets/Scripts/Health/Tests/` (not `Testing/`), but is still not under an `Editor/` folder and compiles into production.

**`PlayerJumpStrategy` (`IJumpStrategy`) is fully implemented but not connected:**
- Issue: `PlayerJumpStrategy` has a complete variable-height jump with coyote time, jump buffering, fall gravity multipliers, and jump cut — but `PlayerJumper` does not reference it. `PlayerJumper` has its own simpler jump implementation. The strategy exists as dead production code.
- Files: `Assets/Scripts/Player/Movement/Strategies/PlayerJumpStrategy.cs`, `Assets/Scripts/Player/Movement/PlayerJumper.cs`

**No automated tests exist for any gameplay systems:**
- Issue: No NUnit or Unity Test Framework test files exist outside of the manual in-editor `HealthSystemTest` (context-menu driven). Critical paths — damage overflow, grapple state transitions, FSM state changes — have no regression coverage.
- Risk: The P0 bugs above (damage multiplier overflow, grapple lockout) are not caught by any automated check.

---

## P5 — Missing Features / Incomplete Systems

**Fall damage not implemented:**
- Issue: `PlayerLandingEvent` has a `FallVelocity` property with an `@TODO: fall dmg ??` comment. `GroundChecker` exposes `LastAirVelocityY` specifically to support fall damage calculation. The data pipeline is wired; the damage application is not.
- Files: `Assets/Scripts/Player/Events/PlayerLandingEvent.cs` line 8, `Assets/Scripts/Player/Movement/GroundChecker.cs` line 28

**Hit VFX / decals not implemented:**
- Issue: `ProjectileBase.TryDealDamage()` has a `// TODO: Use hit.normal for decals, VFX, hit direction effects` comment. No impact feedback exists beyond damage application.
- File: `Assets/Scripts/Combat/ProjectileBase.cs` line 65

**Ammo inventory not tied to a player inventory system:**
- Issue: `WeaponBase` hardcodes `currentAmmoOnReserves = weaponData.TotalAmmo` with a TODO marking it as temporary. There is no inventory abstraction — each weapon tracks its own reserves, meaning switching weapons does not share an ammo pool and ammo pickup (`Ammo_Box` prefab referenced in recent commits) would need direct per-weapon wiring.
- File: `Assets/Scripts/Combat/Weapons/WeaponBase.cs` line 23

**AI is prototype-only (TestEnemy + WaypointController):**
- Issue: The only AI agent is `TestEnemy`, which navigates to click destinations. `AIMemory` has all perception state fields but no write paths. `AIPerception` is an empty class. No actual enemy behaviour (attack, patrol, alert response) is implemented.
- Files: `Assets/Scripts/AI/TestEnemy.cs`, `Assets/Scripts/AI/AIMemory.cs`, `Assets/Scripts/AI/AIPerception.cs`

**Reload animation and timing not enforced:**
- Issue: `WeaponBase.TryReload()` immediately restores ammo. `WeaponDataSO.ReloadTime` is exposed but never read by `TryReload()`. `Pistol.Reload()` and `Shotgun.Reload()` both just print a debug log. Reload is effectively instant with no lockout.
- Files: `Assets/Scripts/Combat/Weapons/WeaponBase.cs`, `Assets/Scripts/Combat/Weapons/Pistol.cs`, `Assets/Scripts/Combat/Weapons/Shotgun.cs`

**Fire rate (`roundsPerMinute`) not enforced:**
- Issue: `WeaponDataSO` exposes `RoundsPerMinute` and `SecondsBetweenShots`, but `WeaponBase.TryShoot()` fires every time the shoot event fires with no rate-of-fire timer. `PlayerInput` fires `OnShoot` every frame `WasPressedThisFrame()` returns true, which is per-keypress, but a held mouse button could still fire faster than intended depending on event dispatch.
- Files: `Assets/Scripts/Combat/Weapons/WeaponBase.cs`, `Assets/Scripts/Data_Scripts/Combat/WeaponDataSO.cs`

---

*Concerns audit: 2026-03-26*
