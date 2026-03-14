# Known Issues

Issues are grouped by severity. Each entry includes location, description, impact, and a suggested fix.

---

## Critical Bugs

These cause broken or permanently incorrect behavior at runtime.

---

### 1. Grapple permanent lockout after cancel during delay

**File:** `Assets/Scripts/Player/Movement/PlayerGrapple.cs:72` and `PlayerGrapple.cs:116`

**Description:**
`TryStartGrapple` schedules `StartGrapple` via `Invoke(nameof(StartGrapple), grappleDelayTime)` (line 72). `CancelGrapple()` (line 116) only calls `EndGrapple()` if `_isGrappling` is already true — but during the 0.1 s delay, `_isGrappling` is still `false`. The pending `Invoke` is never cancelled.

**Impact:**
If the player cancels the grapple during the delay window (e.g., by releasing the button or pressing jump), `StartGrapple` fires 0.1 s later anyway, setting `_isGrappling = true` permanently with no active physics. The component is then deadlocked — `CanGrapple` returns `false` forever because `!_isGrappling` is false but no grapple is actually in progress.

**Fix:**
```csharp
public void CancelGrapple()
{
    CancelInvoke(nameof(StartGrapple));   // ← add this
    if (_isGrappling)
    {
        EndGrapple();
    }
}
```

---

### 2. GrapplingState exits immediately after entry

**File:** `Assets/Scripts/Player/Movement/States/GrapplingState.cs:17`
**Related:** `PlayerGrapple.cs:31`

**Description:**
`GrapplingState.OnUpdate()` checks `!Context.PlayerGrapple.IsGrappling` on line 17 and exits if true. However, `IsGrappling` is the `_isGrappling` field, which is only set to `true` inside `StartGrapple()` — which fires 0.1 s *after* `TryStartGrapple()` returns. The state machine transitions to `GrapplingState` immediately after `TryStartGrapple()`, so `IsGrappling` is still `false` on the first (and likely all subsequent) `OnUpdate` ticks before the invoke fires.

**Impact:**
`GrapplingState` exits on the very first frame it is entered, transitioning back to `Grounded` or `InAir`. The grapple pull physics (from `StartGrapple`) then fire 0.1 s later while the player is in a different state, causing the lockout from Issue #1.

**Fix:**
Introduce a pending/active distinction in `PlayerGrapple`:
```csharp
public bool IsPendingOrGrappling => _isPending || _isGrappling;
```
Check `IsPendingOrGrappling` in `GrapplingState.OnUpdate()` instead of `IsGrappling`.

---

## Important Bugs

These cause incorrect behavior but don't permanently break a system.

---

### 3. Damage overflow multiplier leaks into subsequent chunks

**File:** `Assets/Scripts/Health/Components/BaseHealth.cs:52`

**Description:**
Inside `TakeDamage`, the loop applies `remainingDamage * multiplier` to each chunk and then assigns `overflow` back to `remainingDamage`:

```csharp
remainingDamage = healthChunks[i].ApplyDamage(remainingDamage * multiplier);
```

`ApplyDamage` returns the raw unabsorbed damage (already multiplied). When this overflow flows to the next chunk, its own multiplier is applied on top — effectively stacking multipliers across chunk boundaries.

**Example:** Fire damage (1.5×) against a Flesh chunk overflows into an Exo chunk. The overflow was already scaled by 1.5×; the Exo chunk then applies its own 1.0× multiplier to the pre-scaled value, which is correct by accident in this case. However if the overflow is large and the next chunk has a different multiplier, the applied damage is wrong.

**Fix:**
Pass the raw overflow (before multiplier) to the next chunk:
```csharp
float scaled = remainingDamage * multiplier;
remainingDamage = healthChunks[i].ApplyDamage(scaled);
// remainingDamage is now un-multiplied overflow from ApplyDamage
```
`HealthChunk.ApplyDamage` should return the excess above `MaxHealth` in the same unit as input, without re-applying the multiplier.

---

### 4. `CanGrapple` race condition during delay window

**File:** `Assets/Scripts/Player/Movement/PlayerGrapple.cs:32`

**Description:**
`CanGrapple` is defined as `_cooldownTimer <= 0 && !_isGrappling`. During the 0.1 s delay after `TryStartGrapple()`, `_isGrappling` is still `false`, so `CanGrapple` remains `true`. Another system (or double-tap) can call `TryStartGrapple()` a second time, scheduling a second `Invoke`.

**Impact:**
Two `StartGrapple` invocations fire in quick succession, potentially with different `_grapplePoint` values. The second call overwrites `_isGrappling` state and `_grappleDirection`, creating erratic pull behavior.

**Fix:**
Add a `_isPending` flag set to `true` in `TryStartGrapple` and cleared in `StartGrapple` / `CancelGrapple`. Update `CanGrapple` to also check `!_isPending`.

---

### 5. Wall-running exits when sprint key is released

**File:** `Assets/Scripts/Player/Movement/States/WallRunningState.cs:58`

**Description:**
The condition on line 58 is `Context.WantsToCrouch || !Context.WantsToSprint`. The `!WantsToSprint` part cancels wall-running whenever the player releases the sprint key, even if they are actively holding a wall.

**Impact:**
Players cannot wall-run without holding sprint. This may be intentional design, but it is not documented and could surprise players who release sprint mid-run.

**Fix (if unintentional):** Remove `|| !Context.WantsToSprint` from the condition, or make sprint-to-exit a named/configurable option.

---

### 6. `StateMachine.Clear()` does not reset `_currentStateType`

**File:** `Assets/Scripts/Core/FSM/StateMachine.cs:81`

**Description:**
`Clear()` exits the current state, clears the states dictionary, and nulls `_currentState`, but leaves `_currentStateType` at its last value.

**Impact:**
After `Clear()`, `CurrentStateType` returns a stale enum value. Any code that checks `IsInState()` or `CurrentStateType` after clearing will see incorrect results.

**Fix:**
```csharp
public void Clear()
{
    _currentState?.OnExit();
    states.Clear();
    _currentState = null;
    _currentStateType = default;   // ← add this
}
```

---

## Performance Issues

---

### 7. `Camera.main` called every frame without caching

**File:** `Assets/Scripts/Player/PlayerController.cs:120`

**Description:**
`CalculateCameraRelativeDirection()` calls `Camera.main` every `Update()`. `Camera.main` iterates all GameObjects to find the camera-tagged object — it is not a cached property.

**Fix:**
Cache in `Awake` or `Start`:
```csharp
private Camera _mainCamera;

void Awake() {
    _mainCamera = Camera.main;
}
```

---

### 8. `OnGUI()` debug rendering left in production code

**File:** `Assets/Scripts/Player/PlayerController.cs:157`

**Description:**
`PlayerController` has a full `OnGUI()` implementation (lines 157–196) that renders movement state, dash charges, momentum, and grapple cooldown every frame. `OnGUI` is one of the most expensive Unity callbacks — it allocates per call and runs multiple times per frame during repaint/layout passes.

**Fix:**
Wrap in a compile conditional or remove before shipping:
```csharp
#if UNITY_EDITOR || DEVELOPMENT_BUILD
private void OnGUI() { ... }
#endif
```

---

## Code Quality Issues

---

### 9. Dead sliding branch in `CalculateHorizontalMovement`

**File:** `Assets/Scripts/Player/Movement/PlayerMovement.cs` (approximate — inside `UpdateMovement`)

**Description:**
`UpdateMovement()` returns early when the movement state is `Sliding`, bypassing all subsequent horizontal movement logic. There is a slide-specific code path after the early return that was intended to handle sliding physics but is never reached.

**Impact:**
No runtime error, but the dead code implies the slide physics are either handled elsewhere (in `PlayerSlide`) or were partially migrated and the old path was not cleaned up.

**Fix:**
Remove the dead code path or move it above the early return if it should execute.

---

### 10. Incomplete `CommandInvoker` / Command pattern scaffolding

**Files:**
- `Assets/Scripts/Core/Command/CommandInvoker.cs`
- `Assets/Scripts/Core/Command/JumpCommand.cs`
- `Assets/Scripts/Core/Command/MoveCommand.cs`

**Description:**
`JumpCommand.Undo()` and `MoveCommand.Undo()` both throw `NotImplementedException`. These classes are never instantiated or used anywhere in the codebase. The orphaned interfaces in `Core/Strategies/Movement/` are also part of this unfinished refactor.

**Fix:**
Either complete the command pattern refactor or delete the scaffolding to reduce confusion.

---

### 11. `ExitToAppropriateState()` copy-pasted in three states

**Files:**
- `Assets/Scripts/Player/Movement/States/DashingState.cs`
- `Assets/Scripts/Player/Movement/States/WallRunningState.cs`
- `Assets/Scripts/Player/Movement/States/GrapplingState.cs`

**Description:**
All three states contain an identical private method:
```csharp
private void ExitToAppropriateState()
{
    if (Context.PlayerMovement.IsGrounded)
        Context.StateMachine.ChangeState(MovementState.Grounded);
    else
        Context.StateMachine.ChangeState(MovementState.InAir);
}
```

**Fix:**
Add the method to `BaseState<PlayerMovementContext>` as a `protected` helper, or extract a shared base class for airborne states.

---

### 12. Mixed Spanish/English code comments

**Affected files:** `WallRunningState.cs`, `PlayerController.cs`, `MovementState.cs`, and others throughout

**Description:**
Comments are inconsistently written in Spanish and English. No convention is enforced.

**Fix:**
Standardize on one language (likely English for broader collaborator access) and update comments incrementally.

---

### 13. `AIMemory.LastSeenPleyerTime` typo

**File:** `Assets/Scripts/AI/AIMemory.cs`

**Description:**
The backing field (or property) for last-seen player timestamp is spelled `LastSeenPleyerTime` ("Pleyer" instead of "Player"). This does not affect runtime behavior but will surface in serialized data, stack traces, and any external tools that reference it.

**Fix:** Rename to `LastSeenPlayerTime`.

---

### 14. Testing code in production build path

**File:** `Assets/Scripts/Testing/` (not in an Editor-only folder)

**Description:**
Test scripts (`SpectatorCamera`, `NavMeshTester`, `HealthSystemTest`, `WaypointController`) are in `Assets/Scripts/Testing/` rather than `Assets/Editor/`. They will be compiled into production builds.

**Fix:**
Move test-only MonoBehaviours to `Assets/Editor/` or wrap them in `#if UNITY_EDITOR` guards. Alternatively, use Assembly Definitions with editor-only references.
