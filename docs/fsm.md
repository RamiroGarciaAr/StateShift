# FSM & Movement States

## StateMachine\<TState\>

**File:** `Assets/Scripts/Core/FSM/StateMachine.cs`
Generic state machine keyed by any `Enum` type.

### API

| Member | Description |
|--------|-------------|
| `CurrentState` | Active `IState` instance |
| `CurrentStateType` | Active state enum value |
| `Initialize(TState)` | Enter the first state (calls `OnEnter`) |
| `Update()` | Delegate to `CurrentState.OnUpdate()` |
| `FixedUpdate()` | Delegate to `CurrentState.OnFixedUpdate()` |
| `RegisterState(TState, IState)` | Add a state to the dictionary |
| `ChangeState(TState)` | Exit current → enter new (OnExit → OnEnter) |
| `IsInState(TState)` | Returns `true` if currently in that state |
| `GetState(TState)` | Retrieve a registered state instance |
| `ExitCurrentState()` | Exit current state without transitioning (used by composite states) |
| `Clear()` | Exit current, clear all registered states |

> **Known issue:** `Clear()` does not reset `_currentStateType`, leaving it stale after clear. See [known-issues.md](known-issues.md).

---

## IState

**File:** `Assets/Scripts/Core/FSM/IState.cs`

```
OnEnter()        ← called once on state entry
  │
  ▼  (every frame)
OnUpdate()       ← logic, transitions
OnFixedUpdate()  ← physics
  │
  ▼  (on exit)
OnExit()         ← cleanup
```

---

## BaseState\<TContext\>

**File:** `Assets/Scripts/Core/FSM/BaseState.cs`
Abstract base for all concrete states.

| Member | Description |
|--------|-------------|
| `Context` | Protected reference to the shared context object |
| `OnEnter()` | Abstract — must implement |
| `OnUpdate()` | Abstract — must implement |
| `OnFixedUpdate()` | Virtual — empty default |
| `OnExit()` | Virtual — empty default |
| `Log(string)` | Debug log prefixed with the state class name |

---

## MovementState Enum

**File:** `Assets/Scripts/Player/Movement/MovementState.cs`
Namespace: `Core`

| Value | Kind | Description |
|-------|------|-------------|
| `Grounded` | Outer | Composite state — delegates to inner FSM |
| `WallRunning` | Outer | Player is running on a wall |
| `Dashing` | Outer | Active dash in progress |
| `Grappling` | Outer | Active grapple pull in progress |
| `InAir` | Outer | Airborne (no wall, no grapple) |
| `Walking` | Inner (Grounded) | Default grounded movement |
| `Sprinting` | Inner (Grounded) | Held sprint key |
| `Crouching` | Inner (Grounded) | Held crouch, low speed |
| `Sliding` | Inner (Grounded) | High-speed crouch slide |

---

## PlayerMovementContext

**File:** `Assets/Scripts/Player/Movement/MovementState.cs`
Mutable plain class. All movement states share one instance created by `PlayerController`.

### Component References

| Property | Type | Purpose |
|----------|------|---------|
| `Controllable` | `IControllable` | Core movement + jump interface |
| `PlayerMovement` | `PlayerMovement` | Horizontal movement, momentum, ground state |
| `PlayerCrouch` | `PlayerCrouch` | Crouch/stand transitions |
| `PlayerSlide` | `PlayerSlide` | Slide logic |
| `PlayerDash` | `PlayerDash` | Dash charges and execution |
| `PlayerGrapple` | `PlayerGrapple` | Grapple targeting and pull |
| `PlayerWallRun` | `PlayerWallRun` | Wall detection and wall-run physics |
| `Rigidbody` | `Rigidbody` | Direct physics access |

### State Machines

| Property | Type | Purpose |
|----------|------|---------|
| `StateMachine` | `StateMachine<MovementState>` | Outer FSM (Grounded/InAir/etc.) |
| `GroundedStateMachine` | `StateMachine<MovementState>` | Inner FSM owned by `GroundedState` |

### Input Flags (set every frame by `PlayerController.UpdateContext`)

| Property | Type | Source |
|----------|------|--------|
| `MovementInput` | `Vector2` | Camera-relative WASD |
| `DashInputDirection` | `Vector2` | Raw WASD (preserved for dash direction) |
| `WantsToSprint` | `bool` | Sprint held (`IsPressed`) |
| `WantsToCrouch` | `bool` | Crouch held (`IsPressed`) |
| `WantsToJump` | `bool` | Jump pressed this frame (`WasPressedThisFrame`) |
| `WantsToDash` | `bool` | Dash pressed this frame |
| `WantsToGrapple` | `bool` | Grapple held (`IsPressed`) |

---

## Movement States

### GroundedState (Composite)

**File:** `Assets/Scripts/Player/Movement/States/GroundedState.cs`

Creates and owns an inner `StateMachine<MovementState>` for Walking/Sprinting/Crouching/Sliding. Writes `GroundedStateMachine` back to the context.

**OnEnter:** Initializes inner FSM to `Walking` (or `Crouching` if crouch held).
**OnUpdate:** Checks outer transitions first; if none, ticks inner FSM.
**OnExit:** Calls `ExitCurrentState()` on inner FSM.

| Condition | Transition |
|-----------|-----------|
| `!IsGrounded` | → `InAir` |
| `WantsToGrapple && CanGrapple && TryStartGrapple()` | → `Grappling` |
| `WantsToDash && CanDash && TryStartDash()` | → `Dashing` |

**`CurrentSubState`** — exposes the inner FSM's `CurrentStateType`.

---

### InAirState

**File:** `Assets/Scripts/Player/Movement/States/InAirState.cs`

**OnEnter:** Sets movement state to `InAir`.
**OnUpdate:** Checks transitions.

| Condition | Transition |
|-----------|-----------|
| `IsGrounded` | → `Grounded` |
| `PlayerWallRun.CanWallRun()` | → `WallRunning` |
| `WantsToGrapple && CanGrapple && TryStartGrapple()` | → `Grappling` |
| `WantsToDash && CanDash && TryStartDash()` | → `Dashing` |

---

### WallRunningState

**File:** `Assets/Scripts/Player/Movement/States/WallRunningState.cs`

**OnEnter:** Calls `PlayerWallRun.StartWallRun()`.
**OnExit:** Calls `StopWallRun()` if still running.

| Condition | Transition |
|-----------|-----------|
| `!IsWallRunning` | → `Grounded` or `InAir` |
| `IsGrounded` | → `Grounded` |
| `WantsToGrapple && CanGrapple` | → `Grappling` |
| `!HasWall` | → `Grounded` or `InAir` |
| `WantsToJump` | Wall jump → `Grounded` or `InAir` |
| `WantsToCrouch \|\| !WantsToSprint` | → `Grounded` or `InAir` |

> **Note:** The `!WantsToSprint` condition (line 58) exits wall-running when sprint is released. This may be unintentional — see [known-issues.md](known-issues.md).

---

### DashingState

**File:** `Assets/Scripts/Player/Movement/States/DashingState.cs`

The **previous state** calls `TryStartDash()` before transitioning here. `DashingState` only monitors for completion.

**OnUpdate:**

| Condition | Transition |
|-----------|-----------|
| `!PlayerDash.IsDashing && IsGrounded` | → `Grounded` |
| `!PlayerDash.IsDashing && !IsGrounded` | → `InAir` |
| `WantsToGrapple && CanGrapple && TryStartGrapple()` | → `Grappling` |

**OnExit:** Calls `CancelDash()` if still dashing.

---

### GrapplingState

**File:** `Assets/Scripts/Player/Movement/States/GrapplingState.cs`

The **previous state** calls `TryStartGrapple()` before transitioning here.

**OnUpdate:**

| Condition | Transition |
|-----------|-----------|
| `!PlayerGrapple.IsGrappling` | → `Grounded` or `InAir` |
| `WantsToJump` | Cancel grapple → `Grounded` or `InAir` |
| `!WantsToGrapple` | Cancel grapple → `Grounded` or `InAir` |

**OnExit:** Calls `CancelGrapple()` if still grappling.

> **Critical bug:** `IsGrappling` is `false` during the 0.1 s delay in `TryStartGrapple()`, causing `GrapplingState` to exit immediately on the first `OnUpdate()` tick. See [known-issues.md](known-issues.md).

---

### Grounded Sub-States

All located in `Assets/Scripts/Player/Movement/States/`.

| State | File | Key Behavior |
|-------|------|-------------|
| `WalkingState` | `WalkingState.cs` | Base speed, transitions to Sprinting/Crouching/Sliding |
| `SprintingState` | `SprintingState.cs` | Increased speed, adds momentum each frame |
| `CrouchingState` | `CrouchingState.cs` | Reduced speed, calls `SetCrouching(true)` |
| `SlidingState` | `SlidingState.cs` | Calls `TryStartSlide()`; exits when slide ends or crouch released |

These states are managed exclusively by `GroundedState`'s inner machine. They never appear as `CurrentStateType` on the outer `StateMachine`.
