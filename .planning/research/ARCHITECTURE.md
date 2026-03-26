# Architecture Research

**Domain:** Singleplayer movement shooter FPS (Unity, PC)
**Researched:** 2026-03-26
**Confidence:** HIGH (movement math), HIGH (weapon patterns), HIGH (pooling), MEDIUM (input separation)

---

## System Overview

```
┌──────────────────────────────────────────────────────────────────────┐
│                          INPUT LAYER                                  │
│  ┌──────────────────┐         ┌──────────────────────────────────┐   │
│  │  PlayerInput     │         │  EnemyShootingController         │   │
│  │  (hardware only) │         │  (AI decision → IShooter call)   │   │
│  └────────┬─────────┘         └──────────────┬───────────────────┘   │
│           │ InputFrame (struct)               │ Fire() / Reload()     │
└───────────┼──────────────────────────────────┼───────────────────────┘
            │                                  │
┌───────────┼──────────────────────────────────┼───────────────────────┐
│                       BRAIN / LOGIC LAYER                             │
│  ┌────────▼──────────────────────┐    ┌──────▼──────────────────┐    │
│  │  PlayerMovementBrain          │    │  WeaponHolder            │    │
│  │  - Constructs FSM             │    │  - Owns IWeapon ref      │    │
│  │  - Wires states               │    │  - Calls Fire/Reload     │    │
│  │  - Reads InputFrame           │    │  - Shared by player+AI   │    │
│  │  - Writes PlayerMovCtx        │    └──────────────────────────┘    │
│  └────────┬──────────────────────┘                                    │
└───────────┼───────────────────────────────────────────────────────────┘
            │
┌───────────┼───────────────────────────────────────────────────────────┐
│                     MOVEMENT FSM LAYER                                 │
│  ┌─────────────────────────────────────────────────────────────────┐  │
│  │  StateMachine<MovementState>  (outer)                            │  │
│  │  ┌──────────┐ ┌──────────┐ ┌────────────┐ ┌────────┐ ┌──────┐  │  │
│  │  │ InAir    │ │ Grounded │ │ WallRunning│ │Dashing │ │Grappl│  │  │
│  │  └──────────┘ └────┬─────┘ └────────────┘ └────────┘ └──────┘  │  │
│  │               ┌────▼──────────────────────────────────────────┐ │  │
│  │               │ StateMachine<MovementState> (inner / Grounded) │ │  │
│  │               │ Walking │ Sprinting │ Crouching │ Sliding      │ │  │
│  │               └──────────────────────────────────────────────-─┘ │  │
│  └─────────────────────────────────────────────────────────────────┘  │
│                              │ reads/writes                            │
│  ┌───────────────────────────▼─────────────────────────────────────┐  │
│  │               PlayerMovementContext (shared plain C# object)     │  │
│  └──────────────────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────────────────┘
            │ velocity commands
┌───────────┼───────────────────────────────────────────────────────────┐
│                     PHYSICS LAYER                                      │
│  ┌─────────────────────┐   ┌──────────────────────────────────────┐   │
│  │  PlayerMovement     │   │  BulletSystem                        │   │
│  │  - Rigidbody vel    │   │  - ObjectPool<BulletBase>            │   │
│  │  - Quake accel math │   │  - Per-bullet IFireBehaviour         │   │
│  │  - Momentum calc    │   └──────────────────────────────────────┘   │
│  └─────────────────────┘                                               │
└───────────────────────────────────────────────────────────────────────┘
            │ events
┌───────────┼───────────────────────────────────────────────────────────┐
│                     SERVICE LAYER (Singletons)                         │
│  EventsManager  │  GameManager  │  AITickManager  │  AudioManager     │
└───────────────────────────────────────────────────────────────────────┘
```

---

## Component Responsibilities

| Component | Responsibility | Communicates With |
|-----------|---------------|-------------------|
| `PlayerInput` | Read hardware, emit `InputFrame` per-tick | `PlayerMovementBrain` (write frame), `WeaponHolder` (fire/reload events) |
| `PlayerMovementBrain` | Construct FSM, write `PlayerMovementContext`, call `_stateMachine.Update()` | `PlayerInput` (consumes frame), FSM states (via context) |
| `StateMachine<MovementState>` | State lifecycle, transition logic | `PlayerMovementContext` (shared), `PlayerMovement` (velocity commands) |
| `PlayerMovement` | Rigidbody velocity control, momentum accumulation, Quake-style accel | Rigidbody, FSM states (receive velocity directives) |
| `WeaponHolder` | Owns active `IWeapon`, routes Fire/Reload calls, manages `WeaponInventory` | `IWeapon` implementations, `BulletSystem`, HUD events |
| `WeaponBase` | Implements `IWeapon`, enforces fire rate + reload lockout, reads `WeaponDataSO` | `IFireMode` (strategy), `BulletSystem` (spawn request), `ReloadHandler` |
| `IFireMode` | Encapsulates semi/auto/burst firing cadence | `WeaponBase` (called on trigger) |
| `ReloadHandler` | Coroutine-based reload with timing + lockout | `WeaponBase` (callback on complete) |
| `BulletSystem` | Manages `ObjectPool<BulletBase>` per bullet type, spawns and recycles | `WeaponBase` (spawn call), `IDamagable` targets |
| `BulletBase` | Physical projectile: movement, hit detection, self-release to pool | `BulletSystem` pool (Release on hit/lifetime) |
| `EnemyShootingController` | AI decision layer: aim, fire timing, burst logic | `WeaponHolder` (same interface as player) |
| `AITickManager` | Round-robin tick scheduling for all `ITickable` agents | `ITickable` agents |

---

## Architectural Patterns

### Pattern 1: InputFrame Struct — Decouple Hardware from FSM

**What:** `PlayerInput` writes all per-frame inputs into an `InputFrame` value struct. `PlayerMovementBrain` reads the struct and writes it into `PlayerMovementContext`. No FSM state ever touches `UnityEngine.InputSystem` directly.

**When to use:** Always, for every input-consuming system. This is the fix for the P1 debt item where `PlayerInput` constructs and owns the FSM.

**Trade-offs:** One extra struct allocation per frame (negligible — value type, stack-allocated). Gains: FSM is independently testable, `PlayerInput` can be swapped for a replay system or AI driver without touching state logic.

**Example:**
```csharp
// PlayerInput.cs — hardware layer only
public struct InputFrame
{
    public Vector2 MoveInput;
    public Vector3 WorldMoveDirection;  // camera-relative, pre-computed here
    public bool WantsJump;
    public bool WantsSprint;
    public bool WantsCrouch;
    public bool WantsDash;
    public bool WantsGrapple;
    public bool WantsShoot;
    public bool WantsReload;
}

// PlayerInput.cs
private void Update()
{
    _frame = BuildFrame();                 // reads hardware
    _brain.ConsumeFrame(_frame);           // brain owns FSM, not this class
}
```

```csharp
// PlayerMovementBrain.cs — owns the FSM
public void ConsumeFrame(InputFrame frame)
{
    _context.WantsToJump    = frame.WantsJump;
    _context.WantsToSprint  = frame.WantsSprint;
    _context.MovementInput  = frame.MoveInput;
    // ... etc
    _stateMachine.Update();
}
```

---

### Pattern 2: Quake Acceleration Model for Bunny Hop / Air Strafing

**What:** Replace Unity's direct velocity assignment in `PlayerMovement` with projection-based acceleration. The key: when airborne, add acceleration only up to the projection of current velocity onto the wish direction — not a hard speed cap on total magnitude. This is the mathematical foundation of TF2/Source-style bunny hopping.

**When to use:** In the `InAirState.OnFixedUpdate()` and the `PlayerMovement.ApplyAirAcceleration()` method. Grounded movement uses normal friction + speed capping.

**Trade-offs:** Requires understanding the math or the implementation diverges silently. Feels distinctly different from Unity's default `CharacterController` physics — intentionally so.

**Algorithm (verified from adrianb.io source analysis):**
```csharp
// PlayerMovement.cs
private void ApplyAirAcceleration(Vector3 wishDir, float wishSpeed, float airAccel)
{
    float currentSpeed = Vector3.Dot(_rb.linearVelocity, wishDir);  // projection
    float addSpeed = wishSpeed - currentSpeed;

    if (addSpeed <= 0) return;

    float accelSpeed = airAccel * wishSpeed * Time.fixedDeltaTime;
    if (accelSpeed > addSpeed) accelSpeed = addSpeed;

    _rb.linearVelocity += accelSpeed * wishDir;
    // Total speed can exceed wishSpeed — this is intentional.
    // It enables bunny hopping and air strafing.
}
```

**Velocity preservation across state transitions:** The critical rule is that no state's `OnExit()` zeroes the horizontal velocity. States must explicitly choose to zero it (slide cancel, wall-run dismount) rather than it being the default. `PlayerMovementContext` should carry a `PreserveVelocityOnExit` flag that states set before transitioning.

**Slide-hop:** `SlidingState.OnExit()` fires a velocity-additive jump impulse if `WantsJump` is true at exit. The impulse is added on top of current velocity, not as a replacement. This is what creates the speed-boosting chain.

**Input buffering:** Store `_jumpBufferTimer` and `_coyoteTimer` as floats on `PlayerMovementContext`. Jump buffer: set timer when jump pressed (e.g. 0.15s window). Coyote: set timer on ground-leave. Both checked in `InAirState.OnEnter()` and grounded jump logic. This is the correct location — buffer state belongs in context, not in `PlayerInput`.

---

### Pattern 3: IWeapon / WeaponBase / IFireMode — Strategy + Template Method

**What:** `IWeapon` is a minimal interface (`Fire()`, `Reload()`, `bool CanFire`, `WeaponDataSO Data`). `WeaponBase` is an abstract MonoBehaviour implementing it with Template Method: enforces rate-of-fire timer, reload lockout, ammo checks, then calls `abstract void ExecuteFire()`. Concrete weapons override `ExecuteFire()` only. `IFireMode` is a separate ScriptableObject strategy injected via `WeaponDataSO` that controls cadence (when to trigger `ExecuteFire` in auto, burst, semi modes).

**Why two abstractions (`IWeapon` + `WeaponBase`) instead of one:**  `IWeapon` allows `WeaponHolder` and `EnemyShootingController` to operate on weapons without MonoBehaviour coupling. `WeaponBase` handles the Unity lifecycle (Awake, coroutines) that enemies and player both need. You can mock `IWeapon` in tests without a scene.

**Trade-offs:** `IFireMode` as a ScriptableObject cannot hold per-instance fire state (you'd share state across all weapons using the same SO asset). Solve by making `IFireMode` return a `FireModeState` object on `Init()` that `WeaponBase` owns — the SO is stateless logic only.

**Example:**
```csharp
// Interfaces
public interface IWeapon
{
    bool CanFire { get; }
    void Fire();
    void Reload();
    WeaponDataSO Data { get; }
}

public interface IFireMode
{
    // Returns true when WeaponBase should call ExecuteFire().
    // Called every frame while trigger is held.
    bool ShouldFire(ref FireModeState state, float deltaTime);
}

// WeaponBase.cs (abstract MonoBehaviour)
public abstract class WeaponBase : MonoBehaviour, IWeapon
{
    [SerializeField] protected WeaponDataSO _data;
    private IFireMode _fireMode;        // resolved from _data.FireMode
    private FireModeState _fireModeState;
    private ReloadHandler _reloadHandler;

    public bool CanFire => !_reloadHandler.IsReloading && _currentMag > 0;

    public void Fire()
    {
        if (!CanFire) return;
        if (_fireMode.ShouldFire(ref _fireModeState, Time.deltaTime))
            ExecuteFire();
    }

    protected abstract void ExecuteFire();  // pistol, shotgun, sniper override this

    public void Reload() => _reloadHandler.TryReload(_currentMag, _currentReserves, _data);
}
```

```csharp
// WeaponDataSO.cs
[CreateAssetMenu]
public class WeaponDataSO : ScriptableObject
{
    public int    MagazineSize;
    public int    TotalAmmo;
    public float  RoundsPerMinute;
    public float  ReloadTime;
    public float  Damage;
    public AnimationCurve DamageFalloff;
    public IFireMode      FireMode;       // assigned as SO reference in Inspector
    public GameObject     BulletPrefab;   // type identity for pool lookup
    public DamageType     DamageType;
}
```

---

### Pattern 4: Generic ObjectPool\<T\> — Wrapper Over UnityEngine.Pool

**What:** Use Unity's built-in `UnityEngine.Pool.ObjectPool<T>` (available Unity 2021 LTS+, confirmed available in 2022.3 LTS) rather than rolling a custom collection. Wrap it in a typed `BulletPool<T>` MonoBehaviour that manages the parent transform and prefab reference. `BulletBase` holds a reference to its own pool and calls `Release()` on hit or lifetime expiry.

**Why not a custom generic class:** `UnityEngine.Pool.ObjectPool<T>` has no MonoBehaviour constraint — it works with any type. It handles the create/get/release/destroy callbacks, collection-check double-release detection, and capacity management. Building a custom pool duplicates this with no gain.

**Why not `where T : MonoBehaviour`:** The `MonoBehaviour` constraint forces subclassing, prevents using the pool for plain C# objects (e.g., particle effect data). Use `where T : Component` or unconstrained + `createFunc` delegate. The factory pattern (createFunc) removes the constraint requirement.

**Example:**
```csharp
// BulletPool.cs — one pool per bullet type, registered with BulletSystem
public class BulletPool<T> : MonoBehaviour where T : BulletBase
{
    [SerializeField] private T _prefab;
    private ObjectPool<T> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<T>(
            createFunc:       () => Instantiate(_prefab, transform),
            actionOnGet:      b  => b.gameObject.SetActive(true),
            actionOnRelease:  b  => b.gameObject.SetActive(false),
            actionOnDestroy:  b  => Destroy(b.gameObject),
            collectionCheck:  true,
            defaultCapacity:  20,
            maxSize:          100
        );
    }

    public T Get() => _pool.Get();
    public void Release(T bullet) => _pool.Release(bullet);
}

// BulletBase.cs — self-releasing
public abstract class BulletBase : MonoBehaviour
{
    private Action<BulletBase> _releaseToPool;

    public void Init(Vector3 pos, Quaternion rot, Action<BulletBase> releaseFunc)
    {
        transform.SetPositionAndRotation(pos, rot);
        _releaseToPool = releaseFunc;
        // reset per-instance state here
    }

    protected void ReturnToPool() => _releaseToPool?.Invoke(this);
}
```

---

### Pattern 5: Shared Weapon System for Player and Enemy

**What:** `WeaponHolder` is a standalone MonoBehaviour that owns an `IWeapon` reference and exposes `Fire()` / `Reload()` / `SwapWeapon()`. Both `PlayerMovementBrain` (via weapon action events from `PlayerInput`) and `EnemyShootingController` (via AI logic) call `WeaponHolder`. The weapon itself has zero knowledge of whether its holder is a player or an enemy.

**Key separation:** The AI layer (`EnemyShootingController`) makes *decisions* (aim, when to fire, burst patterns). It then calls the same `WeaponHolder.Fire()` that the player calls. The weapon applies the same fire rate, ammo, and reload rules regardless of caller. Enemy balance is tuned through `WeaponDataSO` values — not through separate weapon code.

**Trade-offs:** Enemies use physics-simulated projectiles just like the player. This is correct for a movement shooter where enemy bullets you dodge are part of the design. It would be wrong for a game needing hitscan-only AI. This codebase uses manual projectile simulation already (`ProjectileBase`), so the pattern fits.

**Example:**
```csharp
// EnemyShootingController.cs
public class EnemyShootingController : MonoBehaviour, ITickable
{
    [SerializeField] private WeaponHolder _weaponHolder;  // same component as player uses
    private AIMemory _memory;

    public void OnTick(float deltaTime)
    {
        if (!_memory.CanSeePlayer) return;
        if (IsAimingAtPlayer())
            _weaponHolder.Fire();    // identical call to player's input path
    }
}
```

---

## Data Flow

### Input → Movement Physics

```
Hardware (keyboard/mouse)
    ↓ every Update frame
PlayerInput.BuildFrame()
    → InputFrame struct (WantsJump, WorldMoveDirection, ...)
    ↓
PlayerMovementBrain.ConsumeFrame(InputFrame)
    → writes PlayerMovementContext fields
    → _stateMachine.Update()  (transition checks, per-state Update logic)
    ↓
PlayerMovement.FixedUpdate()  (physics frame)
    → reads _rawMoveDir set by context
    → ApplyGroundAcceleration() or ApplyAirAcceleration()  (Quake model)
    → AccumulateAndDecayMomentum()
    → Rigidbody.linearVelocity = computed velocity
```

### Weapon Fire → Damage

```
PlayerInput detects shoot action
    → fires event: OnShootPressed
    → WeaponHolder.Fire()
        → WeaponBase.Fire()
            → IFireMode.ShouldFire()  (cadence gate)
            → ExecuteFire()           (concrete weapon override)
                → BulletSystem.Spawn(bulletPrefab, muzzlePos, muzzleRot)
                    → BulletPool<T>.Get()
                    → BulletBase.Init(pos, rot, releaseFunc)
    ↓ (projectile travels)
BulletBase.OnTriggerEnter / Physics.Raycast hit
    → IDamagable.TakeDamage(new DamageInfo(damage, damageType, hitPoint))
    → IDamageModifier chain (PlayerAdrenaline, armor, etc.)
    → DamageMatrixSO.GetMultiplier(damageType, healthType)
    → HealthChunk.ApplyDamage(modified)
    → BaseHealth events: OnHealthChanged / OnDeath
    → BulletBase.ReturnToPool()
```

### Enemy Weapon → Same Damage Path

```
EnemyShootingController.OnTick()
    → WeaponHolder.Fire()             ← identical entry point as player
        → [same path as above]
```

### Bunny Hop Chain

```
SlidingState: player holds jump
    → OnExit(): fires velocity-additive jump impulse
    → transitions to InAirState (preserves horizontal velocity)

InAirState.OnFixedUpdate()
    → PlayerMovement.ApplyAirAcceleration()
        → projects current velocity onto wish direction
        → adds accel only up to projection deficit (never hard-caps total speed)
    → momentum continues to decay/accumulate

InAirState: player lands
    → GroundedState.OnEnter(): does NOT zero horizontal velocity
    → 1-frame no-friction window: player can immediately trigger next slide or sprint
    → speed preserved, chain continues
```

---

## Folder Structure

```
Assets/Scripts/
├── Core/
│   ├── FSM/                    # IState, BaseState<T>, StateMachine<T> — keep as-is
│   ├── Singleton/              # Singleton<T> — keep as-is
│   └── Interfaces/             # IDamagable, IInteractable, IShooter (new)
│
├── Player/
│   ├── Input/
│   │   ├── PlayerInput.cs      # hardware only — reads bindings, emits InputFrame
│   │   └── InputFrame.cs       # value struct (new)
│   ├── Brain/
│   │   └── PlayerMovementBrain.cs  # FSM construction, context wiring (new — extracted from PlayerInput)
│   ├── Movement/
│   │   ├── PlayerMovement.cs   # Rigidbody, Quake accel, momentum
│   │   ├── PlayerMovementContext.cs
│   │   ├── PlayerJumper.cs
│   │   ├── PlayerCrouch.cs
│   │   ├── PlayerSlide.cs
│   │   ├── PlayerDash.cs
│   │   ├── PlayerWallRun.cs
│   │   ├── PlayerGrapple.cs
│   │   └── GroundChecker.cs
│   └── States/                 # all outer + inner FSM states
│
├── Combat/
│   ├── Weapons/
│   │   ├── IWeapon.cs          # interface: Fire, Reload, CanFire, Data
│   │   ├── WeaponBase.cs       # abstract MonoBehaviour (Template Method)
│   │   ├── WeaponHolder.cs     # owns IWeapon, shared by player + enemy
│   │   ├── WeaponInventory.cs
│   │   ├── ReloadHandler.cs    # extracted reload coroutine + lockout (new)
│   │   ├── FireModes/
│   │   │   ├── IFireMode.cs    # ShouldFire() interface
│   │   │   ├── SemiAutoFireMode.cs   # ScriptableObject
│   │   │   ├── FullAutoFireMode.cs   # ScriptableObject
│   │   │   └── BurstFireMode.cs      # ScriptableObject
│   │   └── Concrete/
│   │       ├── Pistol.cs
│   │       ├── Sniper.cs
│   │       └── Shotgun.cs      # multi-pellet ExecuteFire override
│   │
│   ├── Bullets/
│   │   ├── BulletBase.cs       # abstract: Move(), HitDetect(), ReturnToPool()
│   │   ├── StandardBullet.cs
│   │   ├── RicochetBullet.cs   # Sniper
│   │   └── ShotgunPellet.cs
│   │
│   ├── Pooling/
│   │   ├── BulletPool.cs       # wraps UnityEngine.Pool.ObjectPool<T>
│   │   └── BulletSystem.cs     # registry of pools by bullet prefab type
│   │
│   └── Pickup/
│       └── WeaponPickup.cs     # IInteractable.Interact() → swap weapon
│
├── Health/                     # keep existing structure, fix overflow bug
│
├── AI/
│   ├── EnemyShootingController.cs  # decision layer → calls WeaponHolder.Fire()
│   ├── AIMemory.cs
│   ├── AIPerception.cs         # implement line-of-sight, cone check
│   └── AITickManager.cs
│
├── Data_Scripts/
│   ├── Combat/
│   │   └── WeaponDataSO.cs     # add FireMode ref, BulletPrefab, DamageType
│   └── Health/
│       └── DamageMatrixSO.cs
│
└── Managers/
    ├── EventsManager.cs
    ├── GameManager.cs
    ├── AudioManager.cs         # needs full implementation
    └── SceneLoader.cs
```

---

## Anti-Patterns

### Anti-Pattern 1: State Owns the FSM

**What people do:** `PlayerInput` constructs `StateMachine<MovementState>`, registers all states, and calls `_stateMachine.Update()`. This is the current codebase.

**Why it's wrong:** Input is hardware-facing; FSM is logic-facing. Coupling them means you cannot unit-test state transitions without a fully wired input component, cannot replay input without the input system, and cannot swap input providers (e.g., AI takeover, replay, tutorial scripting).

**Do this instead:** `PlayerMovementBrain` owns and constructs the FSM. `PlayerInput` calls `_brain.ConsumeFrame(frame)` after building the `InputFrame`. This is exactly the `PlayerController` → `PlayerMovementBrain` split required in the P1 debt list.

---

### Anti-Pattern 2: Velocity Zeroing on State Exit

**What people do:** `InAirState.OnExit()` sets `rb.linearVelocity = Vector3.zero` or similar. Common because states feel "cleaner" if they reset everything on leave.

**Why it's wrong:** Destroys momentum chain. Bunny hop, slide-hop, and wall-run exit speed are all lost. The player decelerates to zero on every state boundary instead of preserving and building speed.

**Do this instead:** No state zeros velocity on exit by default. States that legitimately need to kill speed (grapple cancel, wall-run into wall) do so explicitly via a named method (`PlayerMovement.KillHorizontalVelocity()`). Everywhere else, velocity carries across transitions.

---

### Anti-Pattern 3: Weapon Duplicated for Enemy

**What people do:** `EnemyWeapon.cs` duplicates fire rate, reload, and ammo logic from `WeaponBase.cs` with minor tweaks for AI (e.g., no ammo limit, infinite reload). Balance changes must be applied in two places.

**Why it's wrong:** Enemy weapon balance diverges from player weapon balance silently. Any bug fix to `WeaponBase` must be manually mirrored.

**Do this instead:** Enemies use the same `WeaponBase` subclasses as the player, configured via the same `WeaponDataSO`. Enemy "unlimited ammo" is a `WeaponDataSO` field (`InfiniteAmmo: bool`), not a separate class. `EnemyShootingController` calls `WeaponHolder.Fire()` — it does not contain weapon logic.

---

### Anti-Pattern 4: ScriptableObject Storing Mutable Per-Instance State

**What people do:** `IFireMode` is a ScriptableObject that holds `_lastFireTime` or `_burstCount` as instance fields. Since SOs are shared assets, every weapon referencing the same fire mode SO shares the same state — all pistols fire simultaneously, burst count is shared, etc.

**Why it's wrong:** ScriptableObjects are not per-instance. Fields on an SO are shared across all users of that asset at runtime.

**Do this instead:** `IFireMode` ScriptableObjects contain stateless logic only. `WeaponBase` owns a `FireModeState` struct (plain C# value type) that holds `lastFireTime`, `burstShotsRemaining`, etc. The `IFireMode.ShouldFire(ref FireModeState state, float dt)` signature takes the state by ref — the SO reads and mutates only the passed struct.

---

### Anti-Pattern 5: ObjectPool Using `Destroy` + `Instantiate` Per Bullet

**What people do:** `Pistol.Shoot()` calls `Instantiate(bulletPrefab)` and the bullet calls `Destroy(gameObject)` on hit. Fast to write.

**Why it's wrong:** Every `Instantiate` allocates heap memory. Every `Destroy` queues a GC event. At 600 RPM pistol fire this is ~10 allocs/second minimum — spikes GC mid-fight, causing frame hitches at exactly the moment the game must feel responsive.

**Do this instead:** `BulletBase.ReturnToPool()` calls the pool's `Release()` delegate. The pool sets the GameObject inactive and stores it in the free list. Zero allocations in steady-state fire. `UnityEngine.Pool.ObjectPool<T>` (available in Unity 2022.3 LTS) handles this with built-in double-release detection.

---

## Build Order (Dependency Sequencing)

The systems have hard dependencies that dictate phase order:

```
Phase 1: Input / Brain Separation
│  Extract PlayerMovementBrain from PlayerInput
│  Define InputFrame struct
│  PlayerInput becomes hardware-only
│
│  Rationale: Every other system change is blocked by the P1 architectural
│  violation. FSM bugs are harder to diagnose when input and state are coupled.
│  This must ship clean before touching movement or weapon logic.
│
├─► Phase 2: Movement Audit + Bugs + Bunny Hop
│     Fix P0 grapple bugs (CancelInvoke, IsPending flag)
│     Fix StateMachine.Clear() reset
│     De-duplicate ExitToAppropriateState()
│     Implement Quake air acceleration model
│     Add jump buffer + coyote time to PlayerMovementContext
│     Implement slide-hop velocity preservation
│     Delete dead strategy files + CommandInvoker
│
│  Rationale: Movement must feel correct before weapon iteration begins.
│  A broken movement system makes combat feel wrong even if weapons are perfect.
│  P0 bugs must be resolved before any playtesting validates anything.
│
├─► Phase 3: Weapon System Redesign
│     Define IWeapon, IFireMode interfaces
│     Implement WeaponBase with Template Method (fire rate, reload lockout)
│     Extract ReloadHandler
│     Implement SemiAuto, FullAuto, Burst fire modes as SOs
│     Fix ammo math (reserve check before reload, no negative reserves)
│     Implement Sniper (ricochet bullet type)
│     Fix Shotgun (multi-pellet ExecuteFire)
│
│  Rationale: Interface design must be locked before ObjectPool is built.
│  BulletBase needs to know its pool type. WeaponDataSO shape must be stable
│  before concrete weapons reference it.
│
├─► Phase 4: Bullet System + ObjectPool
│     Implement BulletBase (abstract movement + hit detection + self-release)
│     Implement StandardBullet, RicochetBullet, ShotgunPellet
│     Wrap UnityEngine.Pool.ObjectPool<T> in BulletPool<T>
│     Implement BulletSystem registry
│     Replace Instantiate/Destroy calls with pool Get/Release
│
│  Rationale: Pool requires stable bullet types (Phase 3). Pool design
│  finalizes once bullet hierarchy is known.
│
├─► Phase 5: Enemy Weapon Integration
│     Implement WeaponHolder (shared player + enemy component)
│     Implement EnemyShootingController (AI decision layer)
│     Implement AIPerception (line-of-sight, cone check)
│     Connect AIMemory write paths (CanSeePlayer, LastKnownPlayerPosition)
│     Enemy uses same WeaponBase subclasses with own WeaponDataSO assets
│
│  Rationale: Enemy weapons require the full weapon system (Phase 3+4) to
│  be stable. AI perception requires the scene to be in a playable state
│  (Phase 1+2 movement working) to validate sight-line behavior.
│
└─► Phase 6: Polish (URP, VFX, Audio)
      URP migration (shaders, post-processing)
      Hit VFX via projectile impact normals (TODO already in ProjectileBase)
      AudioManager proper Singleton implementation + event wiring
      Camera polish (FOV burst, landing squash)
```

**Movement audit before weapon redesign** is the correct sequencing because:
- Movement feel validates the game concept independently. A broken weapon on a great-feeling movement base is debuggable. A great weapon on a broken movement base makes nothing feel right.
- The P0 grapple bugs affect state machine behavior. Weapon tests that fire during a grapple could produce false results if the FSM is misbehaving.
- `PlayerMovementBrain` extraction (Phase 1) changes how weapon input events are dispatched. Weapon system work after this extraction starts in the correct architecture.

---

## Integration Boundaries

| Boundary | Communication Pattern | Notes |
|----------|-----------------------|-------|
| `PlayerInput` ↔ `PlayerMovementBrain` | `ConsumeFrame(InputFrame)` method call | `PlayerInput` references `Brain`; `Brain` does not reference `PlayerInput` |
| `PlayerMovementBrain` ↔ FSM | Direct call: `_stateMachine.Update()` | Brain owns FSM instance |
| FSM states ↔ `PlayerMovement` | Via `PlayerMovementContext` — states write directives, `PlayerMovement.FixedUpdate()` reads them | Decouples state logic from physics timing |
| `WeaponHolder` ↔ `IWeapon` | Interface only — no concrete type reference at holder level | Enables weapon swap and enemy reuse |
| `WeaponBase` ↔ `BulletSystem` | `BulletSystem.Spawn(prefab, pos, rot)` method call | `WeaponBase` does not know pool internals |
| `BulletBase` ↔ `BulletPool<T>` | `Action<BulletBase>` delegate passed at `Init()` | Bullet releases itself; pool does not poll |
| `EnemyShootingController` ↔ `WeaponHolder` | Same `WeaponHolder.Fire()` call as player | Zero weapon code duplication |
| `BaseHealth` ↔ `IDamageModifier` | `List<IDamageModifier>`, sorted by `Priority`, applied in order | Keep existing; fix overflow bug only |
| Any system ↔ services | `EventsManager.Instance.OnX` | Singletons — keep existing pattern |

---

## Sources

- Quake/Source bunny hop algorithm: [Bunnyhopping from the Programmer's Perspective](https://adrianb.io/2015/02/14/bunnyhop.html)
- Q3-style FPS controller reference: [atil/fpscontroller](https://github.com/atil/fpscontroller)
- Unity ScriptableObject weapon pattern: [Unity Discussions — SO-based gun system](https://discussions.unity.com/t/tutorial-make-a-scriptableobject-based-gun-system-from-scratch/896847)
- Strategy pattern + ScriptableObjects: [DEV Community — Strategy Pattern with SOs](https://dev.to/eriksk/implementing-the-strategy-design-pattern-using-scriptable-objects-in-unity-292i)
- Unity built-in ObjectPool API: [Unity Scripting API — ObjectPool](https://docs.unity3d.com/ScriptReference/Pool.ObjectPool_1.html)
- Type-safe Unity pool: [Game Developer — Type-Safe Object Pool for Unity](https://www.gamedeveloper.com/programming/type-safe-object-pool-for-unity)
- Shared shooter interface for player + enemy: [Unity Discussions — Shooter class for player and enemy](https://discussions.unity.com/t/proper-way-of-making-a-shooter-class-both-player-and-enemy-use/816152)
- Component pattern / input separation: [Game Programming Patterns — Component](https://gameprogrammingpatterns.com/component.html)
- Input separation (codebase): `Assets/Scripts/Player/PlayerInput.cs` — self-annotated TODOs confirming the architectural violation

---

*Architecture research for: StateShift — Unity singleplayer movement shooter FPS*
*Researched: 2026-03-26*
