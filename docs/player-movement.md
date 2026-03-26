# Player Movement Components

## PlayerController

**File:** `Assets/Scripts/Player/PlayerController.cs`
Namespace: `Entities.Controllers`

The top-level orchestrator. Reads Unity Input System, populates `PlayerMovementContext`, and ticks the `StateMachine<MovementState>`.

### Input Actions (mapped from "Player" action map)

| Action | Flag Set | Read Mode |
|--------|---------|-----------|
| Movement | `MovementInput`, `DashInputDirection` | `ReadValue<Vector2>()` |
| Jump | `WantsToJump` | `WasPressedThisFrame()` |
| Sprint | `WantsToSprint` | `IsPressed()` |
| Crouch | `WantsToCrouch` | `IsPressed()` |
| Dash | `WantsToDash` | `WasPressedThisFrame()` |
| Grapple | `WantsToGrapple` | `IsPressed()` |

### Per-Frame Flow

```
Update():
  1. Read raw input Vector2
  2. CalculateCameraRelativeDirection()  ← rotates input by Camera.main yaw
  3. UpdateContext()                     ← write all flags to context
  4. _stateMachine.Update()             ← tick active state
  5. Controllable.Move(direction)       ← always applied
  6. HandleJump()                       ← calls Jump() + SetHoldingJump()

FixedUpdate():
  _stateMachine.FixedUpdate()
```

### FSM Initialization (`InitializeStateMachine`)

Creates `PlayerMovementContext`, populates all component references, creates `StateMachine<MovementState>`, registers all five outer states, and calls `Initialize(MovementState.Grounded)`.

> **Performance note:** `Camera.main` is accessed every frame in `CalculateCameraRelativeDirection` (line 120) without caching. See [known-issues.md](known-issues.md).

---

## PlayerMovement

**File:** `Assets/Scripts/Player/Movement/PlayerMovement.cs`
Implements: `IControllable` (= `IMovable` + `IJumpable`)

Core horizontal and vertical movement, momentum accumulation, and slope handling. Owns `PlayerJumper` and `GroundChecker`.

### Key Properties

| Property | Type | Description |
|----------|------|-------------|
| `Speed` | `float` | Configured base speed |
| `CurrentSpeed` | `float` | Effective speed (base × momentum factor) |
| `IsGrounded` | `bool` | Forwarded from `GroundChecker` |
| `IsJumping` | `bool` | Currently moving upward after a jump |
| `IsHoldingJump` | `bool` | Jump button held (for variable height) |
| `Momentum01` | `float` | Normalized momentum 0–1 (capped at `maxMomentum = 0.8`) |
| `GroundRigidbody` | `Rigidbody` | Rigidbody of ground surface (moving platforms) |
| `GroundNormal` | `Vector3` | Surface normal at ground contact |
| `GroundPoint` | `Vector3` | World contact point |
| `GroundVelocity` | `Vector3` | Velocity of the ground surface |
| `IsOnSlope` | `bool` | Standing on any slope |
| `IsOnWalkableSlope` | `bool` | Slope angle within walkable limit |
| `SlopeAngle` | `float` | Current slope angle in degrees |
| `SlopeDir` | `Vector3` | Down-slope direction vector |

### Key Methods

| Method | Description |
|--------|-------------|
| `Move(Vector2 direction)` | Set desired XZ movement direction |
| `Jump()` | Request a jump (queued, executed next FixedUpdate) |
| `SetHoldingJump(bool)` | Forward jump-hold state to `PlayerJumper` |
| `SetMovementState(MovementState)` | Notify movement of active state (affects speed/behavior) |
| `AddMomentum(float)` | Add to momentum accumulator (capped at `maxMomentum`) |
| `AddSlideMomentumTick()` | Per-frame momentum gain during slides |

### Momentum System

- Accumulates from: sprinting, sliding, downhill movement
- Decay: exponential (configurable half-life) when not fueled
- Effect: increases `CurrentSpeed` up to 80% above base (`maxMomentum = 0.8`)
- Read by `PlayerAdrenaline` for damage reduction scaling

---

## PlayerJumper

**File:** `Assets/Scripts/Player/Movement/PlayerJumper.cs`
Owned by `PlayerMovement`.

### Key Methods

| Method | Description |
|--------|-------------|
| `RequestJump()` | Queue a jump; only accepted if grounded |
| `ProcessJump()` | Execute queued jump on FixedUpdate |
| `ApplyGravity(float verticalVelocity)` | Apply gravity with variable-height modifier |
| `SetHoldingJump(bool)` | Track whether jump button is held |

### Variable-Height Jump

- While holding jump and moving upward: reduced gravity multiplier
- On release or peak: full (higher) gravity applied
- Produces natural feel without changing jump force

---

## GroundChecker

**File:** `Assets/Scripts/Player/Movement/GroundChecker.cs`
Owned by `PlayerMovement`.

Performs a downward raycast from the base of the capsule every frame.

### Key Properties

| Property | Description |
|----------|-------------|
| `IsGrounded` / `WasGrounded` | Current and previous-frame ground state |
| `GroundNormal` | Surface normal |
| `GroundPoint` | World contact point |
| `GroundRigidbody` | Rigidbody of ground (moving platforms) |
| `GroundVelocity` | Ground surface velocity |
| `IsOnSlope` / `IsOnWalkableSlope` | Slope detection |
| `SlopeAngle` / `SlopeDir` | Slope angle and down-slope direction |
| `LegHeight` | Adjusts raycast length |

### Events

| Event | When Fired |
|-------|-----------|
| `OnLanded` | Transition from airborne → grounded |
| `OnLeftGround` | Transition from grounded → airborne |
| `OnEnteredSlope` | First frame on a slope |
| `OnExitedSlope` | Left slope |

---

## PlayerCrouch

**File:** `Assets/Scripts/Player/Movement/PlayerCrouch.cs`
Requires: `CapsuleCollider`

Handles smooth crouch/stand collider transitions and camera height.

### Key Methods

| Method | Description |
|--------|-------------|
| `SetCrouching(bool)` | Start or end crouch; smoothly scales collider height |
| `CanStandUp()` | Sphere cast upward to check for ceiling |

---

## PlayerSlide

**File:** `Assets/Scripts/Player/Movement/PlayerSlide.cs`

High-speed ground slide with momentum feedback.

### Key Methods

| Method | Description |
|--------|-------------|
| `TryStartSlide()` | Starts slide if `CurrentSpeed >= slideSpeedThreshold` (6 m/s) |
| `EndSlide()` | Stop slide; awards momentum bonus |

### Mechanics

| Parameter | Default | Description |
|-----------|---------|-------------|
| `slideSpeedThreshold` | 6 m/s | Minimum speed to initiate |
| `slideBoost` | 3 | Additive velocity burst on start |
| `slideMaxDuration` | 1 s | Slide ends automatically after this time |
| Slope acceleration | — | Extra force applied downhill during slide |
| Exit momentum gain | — | Momentum awarded when slide ends naturally |

---

## PlayerDash

**File:** `Assets/Scripts/Player/Movement/PlayerDash.cs`

Directional dash with charge system and cooldown.

### Key Properties

| Property | Description |
|----------|-------------|
| `IsDashing` | Currently in an active dash |
| `CanDash` | Has charges and not currently dashing |
| `CurrentCharges` / `MaxCharges` | Charge count (default max: 2) |
| `CooldownProgress` | 0–1 normalized cooldown for UI |
| `ChargeRecoveryProgress` | 0–1 normalized charge recovery for UI |

### Key Methods

| Method | Description |
|--------|-------------|
| `TryStartDash(Vector2 inputDir)` | Consume a charge and launch dash |
| `CancelDash()` | Abort the active dash |
| `RefillCharges()` | Reset charges to max |

### Mechanics

- Direction: input-relative to camera yaw
- Momentum retained after dash: 85% (configurable)
- Force curve: ease-in/out (AnimationCurve)
- Charges recover over time independently of cooldown

---

## PlayerGrapple

**File:** `Assets/Scripts/Player/Movement/PlayerGrapple.cs`

Raycast-based grapple-to-point with pull physics.

### Key Properties

| Property | Description |
|----------|-------------|
| `IsGrappling` | Pull physics are active |
| `CanGrapple` | Cooldown elapsed and not currently grappling |
| `GrapplePoint` | World-space target position |
| `CooldownProgress` | 0–1 normalized cooldown for UI |

### Key Methods

| Method | Description |
|--------|-------------|
| `TryStartGrapple()` | Raycast from camera forward; schedules `StartGrapple` via `Invoke` after `grappleDelayTime` |
| `CancelGrapple()` | Abort if `_isGrappling` is true; does **not** cancel the pending `Invoke` |

### Mechanics

| Parameter | Default | Description |
|-----------|---------|-------------|
| `maxGrappleDistance` | 50 m | Raycast range |
| `grappleDelayTime` | 0.1 s | Delay before pull starts (visual windup) |
| `velocityRetention` | 0.8 | Fraction of current velocity kept on grapple start |
| `minDistanceToTarget` | 2 m | Distance at which grapple auto-ends |
| `momentumGain` | 0.25 | Momentum awarded on grapple completion |

> **Critical bugs in this component:** see [known-issues.md](known-issues.md) — the pending `Invoke` is never cancelled, and `GrapplingState` exits immediately because `IsGrappling` is false during the delay.

---

## PlayerWallRun

**File:** `Assets/Scripts/Player/Movement/PlayerWallRun.cs`

Wall detection and wall-run physics including wall jumping.

### Key Properties

| Property | Description |
|----------|-------------|
| `IsWallRunning` | Currently attached to a wall |
| `HasWall` | Wall detected on left or right |
| `IsWallLeft` / `IsWallRight` | Which side the wall is on |

### Key Methods

| Method | Description |
|--------|-------------|
| `CanWallRun()` | Returns true if speed, wall, non-grounded, and cooldown conditions are met |
| `StartWallRun()` | Disable gravity, apply wall-stick force, set initial boost |
| `StopWallRun()` | Re-enable gravity, award momentum, start cooldown |
| `WallJump()` | Apply upward + away-from-wall impulse |

### Mechanics

| Parameter | Default | Description |
|-----------|---------|-------------|
| Max wall-run time | 2 s | Auto-exits after this duration |
| `gravityCounterForce` | — | Upward force countering gravity during run |
| Wall-stick force | — | Force toward wall to maintain contact |
| Wall jump side force | — | Away-from-wall horizontal impulse |
| Re-entry cooldown | — | Delay before same wall can be used again |
