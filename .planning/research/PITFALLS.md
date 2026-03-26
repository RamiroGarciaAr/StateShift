# Pitfalls Research

**Domain:** Unity 2022.3 LTS movement shooter FPS (Titanfall 2 feel target)
**Researched:** 2026-03-26
**Confidence:** HIGH (URP migration, Rigidbody physics) / MEDIUM (game feel, weapon architecture) / HIGH (P0 bug patterns from direct codebase analysis)

---

## Critical Pitfalls

### Pitfall 1: Deferred Bug Fixing Causes Compounding State Corruption

**What goes wrong:**
The four existing P0 bugs are not isolated failures — they are symptoms of a shared pattern: mutable state that can be set without a corresponding cleanup path. The grapple lockout (`_isGrappling = true` with no reset path) and the CanGrapple race (cooldown not started during delay window) share the same root cause: a state transition that is not atomic. When you add bunny hop, slide-hop, and wall-run on top of a movement FSM that already has these races, the probability of cross-state contamination multiplies. A wall-run that starts during a grapple delay, or a bunny hop that fires during a dash cooldown, will reproduce the same class of bug.

**Why it happens:**
Async operations (Invoke, coroutines, delay timers) modify state after the calling context has moved on. When the state machine transitions away and back, or when input fires a second time during the delay window, the deferred callback fires against stale context.

**How to avoid:**
Fix all four P0s before touching any movement feature. Treat each fix as requiring: (1) an `_isPending` guard that blocks re-entry during any delay window, (2) a `CancelInvoke` / coroutine stop paired with every `Invoke` / `StartCoroutine`, and (3) a state machine `OnExit` that cancels all pending operations in the component. Do not ship new movement mechanics on top of unresolved state corruption.

**Warning signs:**
- Movement action that "sometimes doesn't work" on the second or third use
- Player stuck in a state they cannot exit (locked in air, locked out of a mechanic)
- Any `Invoke(nameof(...), delay)` call without a paired `CancelInvoke` in the cancel path
- A state's `OnEnter` checking a flag that is set in a deferred callback

**Phase to address:** Movement System Audit (before any new movement work)

---

### Pitfall 2: URP Migration Breaks All Built-In Shaders and the Post-Processing Stack Silently or Visually

**What goes wrong:**
Every material that uses a Built-in Render Pipeline shader turns pink immediately after installing URP — including Unity's own standard shaders if not upgraded. The legacy Post-Processing Stack v2 package is fully incompatible with URP; it must be removed and replaced with URP's native Volume framework. Custom shaders written with `CGPROGRAM`/`ENDCG` blocks will not render at all — they require manual conversion to `HLSLPROGRAM`/`ENDHLSL` with updated include paths and SRP Batcher-compatible CBUFFER declarations.

Cinemachine's post-processing integration also changes: the `CinemachinePostProcessing` extension (v2 stack) is replaced by `CinemachineVolumeSettings` in URP. Any camera-blended post-processing effects (the FOV/tilt camera system in StateShift) will silently stop blending unless the extension is swapped.

**Why it happens:**
URP is a completely separate render pipeline — it does not fall back gracefully on Built-in shaders. Developers assume Unity's upgrade wizard handles everything. The wizard handles Unity's own standard materials only; custom shaders and third-party packages require manual work.

**How to avoid:**
Before clicking "Install URP": (1) inventory every shader in the project (search for `.shader` files and identify which use `CGPROGRAM`), (2) identify all third-party packages and check their URP compatibility matrix, (3) run the upgrade in a dedicated git branch. After installing URP, run `Edit > Rendering > Render Pipeline Converter` for standard materials, then manually address each custom shader. Test the Cinemachine virtual camera blend and FOV transitions immediately after — they are easy to miss until a gameplay scenario exercises them.

**Warning signs:**
- Any pink material in Scene or Game view after migration
- Post-processing effects (motion blur, vignette, depth of field) no longer visible
- Cinemachine camera blends not applying expected post-processing profile
- Console errors referencing `Shader "Standard"` or `Graphics.Blit` with null source

**Phase to address:** URP Migration phase (dedicated isolated branch, before any new content)

---

### Pitfall 3: Rigidbody Movement Feels Floaty Because of Default Physics Parameters, Not Code Logic

**What goes wrong:**
Unity's default gravity (-9.81 m/s²) and default Rigidbody drag produce movement that feels like the player is walking on the moon. The jump arc is too high and lingers too long. Slides decelerate gracefully instead of snapping. When developers try to fix this by tweaking force magnitudes, they end up with velocity values that are arbitrary and non-reproducible, and the underlying physics parameters remain wrong.

The second failure mode: all physics forces applied in `Update()` instead of `FixedUpdate()` cause frame-rate-dependent movement. At 144 Hz the player moves faster and jumps higher than at 60 Hz. This is not theoretical — the existing `PlayerMovement` code must be audited for any velocity manipulation outside `FixedUpdate`.

**Why it happens:**
Unity's default physics settings target realistic simulation. Movement shooters require exaggerated, game-feel-tuned gravity (typically 2-4x real gravity while falling). The disconnect between Update-based input reading and FixedUpdate-based physics application is a structural issue that compounds over time.

**How to avoid:**
Set `Physics.gravity` to approximately -30 to -40 m/s² (tune via `PlayerDataSO`, not hardcoded). Apply an additional gravity multiplier to the Rigidbody when the player is falling (negative Y velocity) to tighten the fall arc — this is the technique Titanfall 2 uses for snappy landings. All `rb.AddForce`, `rb.velocity` assignments, and `rb.MovePosition` calls must live in `FixedUpdate`. Input reading stays in `Update` or input callbacks; the resulting intent vectors are stored in fields read by `FixedUpdate`. Set Rigidbody Interpolation to `Interpolate` (not None) to eliminate camera jitter caused by physics/render desync.

**Warning signs:**
- Jump arc feels like a parabola over 2+ seconds
- Player slides for a noticeable distance after releasing directional input
- Movement feels different at 60 Hz vs 120 Hz (frame-rate dependency)
- Camera jitter when moving (Rigidbody Interpolation is None)

**Phase to address:** Movement System Audit / Rebuild

---

### Pitfall 4: Bunny Hop Velocity Preservation Broken by Ground Friction Timing

**What goes wrong:**
The classic bunny hop implementation bug: ground friction is applied the frame the player lands, zeroing out horizontal momentum before the jump input is processed in the same or next frame. The result is a hop system that technically works but resets to zero speed on every landing. The player can "bunny hop" but gains no speed and preserves no direction.

The secondary failure: velocity cap is applied on grounding. If the player slides at 800 units/s and jumps, but `PlayerMovement` clamps velocity to `maxSpeed` the moment `IsGrounded` becomes true, the slide-hop chain is broken. The cap must not apply during the jump input window.

The third failure: jump input is read in `Update` but the jump force is applied in `FixedUpdate`. If the player taps jump at the exact frame boundary, the input is consumed in `Update` before `FixedUpdate` runs, and the jump fires correctly. But if the project uses `Process Events in Fixed Update` mode on the Input System, the jump event fires in `FixedUpdate` — meaning a jump pressed between two FixedUpdate calls is silently dropped.

**Why it happens:**
Momentum physics and input timing interact at sub-frame granularity. Developers implement each piece correctly in isolation but do not account for the ordering: friction → grounding check → jump input → force application, all happening in the same physics step.

**How to avoid:**
Use a "jump buffer" window (100-150ms): store the timestamp of the last jump input. In `FixedUpdate`, if `timeSinceJumpPressed < jumpBufferWindow && IsGrounded`, consume the buffer and apply the jump impulse. Use a "coyote time" window (80-120ms): preserve `IsGrounded = true` for a short window after leaving a ledge, allowing jumps to register at ledge edges. Do not apply ground friction the same `FixedUpdate` frame that a jump is executed. Specifically: check jump buffer before applying friction deceleration, not after. Expose `frictionDecelerationTime`, `jumpBufferWindow`, and `coyoteTime` as fields on `PlayerDataSO` — never hardcode.

**Warning signs:**
- Slide-hop chain resets speed to zero on every landing
- Jump "sometimes doesn't register" near ledge edges
- Bunny hopping faster at high frame rates
- Air strafing causes speed loss instead of speed maintenance (air acceleration cap too low)

**Phase to address:** Movement System Rebuild (after audit, during bunny hop feature implementation)

---

### Pitfall 5: Weapon System Brittle Due to Inheritance Overuse and Missing Interface Boundary

**What goes wrong:**
The dominant failure in FPS weapon systems: adding a new weapon type requires modifying a base class, which breaks every existing weapon. This happens when `WeaponBase` grows to contain conditional behaviour ("if shotgun, spread; if sniper, zoom; if auto, hold fire logic") rather than delegating to injected strategies. The second failure: `WeaponBase` directly instantiates bullet types, coupling the weapon to a specific projectile. Adding a non-projectile weapon (beam weapon, melee) requires refactoring the base class.

In the existing StateShift codebase this is already present: `Shotgun.Shoot()` is a reskin of `Pistol.Shoot()` with no spread, and `TryReload()` fills the magazine unconditionally regardless of reserve count. Both indicate the weapon base class is doing work it shouldn't.

Generic ScriptableObjects have a known Unity 2022 LTS issue: `ScriptableObject` subclasses with generic type parameters are not serializable in the Unity Editor without workarounds (the `GenericUnityObjects` package or concrete non-generic subclasses). The planned `IFireMode` strategy pattern injected via `WeaponDataSO` must use non-generic `ScriptableObject` base types or concrete serializable subclasses.

**Why it happens:**
Inheritance is the natural object-oriented extension point. Developers add behaviour to the base class because it's fast and avoids creating new files. The system degrades incrementally until adding a new weapon type requires tracing through 5 levels of inheritance and conditional branches.

**How to avoid:**
Enforce the interface boundary: `IWeapon.Fire()` and `IWeapon.Reload()` — no other public members. `WeaponBase` implements `IWeapon` and delegates everything to injected components: `IFireMode` (semi/auto/burst strategy), `IReloadStrategy` (mag-fed vs shell-fed), `ObjectPool<T>` (bullet instantiation). `WeaponBase` itself must not know what bullet it fires — it calls `_bulletSpawner.Spawn(transform.forward)` and the spawner handles the type. Enemies using `IWeapon` directly means `WeaponBase` cannot contain player-specific code (aim assist, crosshair callbacks).

For `IFireMode` as a `ScriptableObject`: use concrete subclasses (`SemiAutoFireMode : ScriptableObject`, `FullAutoFireMode : ScriptableObject`) — do not use generic base classes. Unity 2022 LTS serializes concrete `ScriptableObject` subclasses correctly.

**Warning signs:**
- Adding a third weapon requires changing `WeaponBase`
- `WeaponBase` contains `if (weaponData.type == WeaponType.Shotgun)` conditionals
- `Pistol.Reload()` and `Shotgun.Reload()` have different logic that should be in an injected strategy
- Fire rate or reload time not respected (already the case in current codebase)

**Phase to address:** Weapon System Redesign phase

---

### Pitfall 6: Game Feel Separating Factor Is Feedback Layering, Not Individual Effects

**What goes wrong:**
Developers add screen shake. It helps a little. They add hit particles. It helps a little. The game still feels indie because AAA feel comes from layered simultaneous feedback, not from any single effect. The differences between Apex Legends gunplay and a typical indie FPS:

1. **Hit stop**: 2-4 frames of time dilation or frozen animation on confirmed hit. Apex and Titanfall 2 both use this. Unity developers almost never implement it because it requires pausing the target's animation/physics independently while the game continues.
2. **Weapon weight**: The camera dips slightly on every shot (recoil pattern), not just screen shake. The dip follows a deterministic pattern per weapon — players learn it and control it (like Apex's recoil patterns).
3. **Sound timing**: Audio is mixed so the impact sound arrives at the exact frame the visual impact occurs. Frame-misaligned audio makes hits feel fake even if the visual is correct.
4. **Camera interpolation speed**: AAA games use faster camera follow speeds and lookahead. Indie games use the Cinemachine default damping which feels sluggish. For an FPS where the camera IS the player's eye, zero camera damping is correct — Cinemachine's `Body: Do Nothing` and `Aim: Do Nothing` (or hard lock to transform) is correct for the first-person view.

**Why it happens:**
Each individual effect seems complete when implemented in isolation. The layering interaction only becomes apparent when playtesting the full combat scenario. Individual effects are easy to implement; the layering requires a deliberate game feel pass as a dedicated phase.

**How to avoid:**
Treat game feel as a dedicated phase, not a feature list to check off during system development. Schedule a "polish pass" after all mechanics are functional. Keep a reference video of Titanfall 2 or Apex combat and compare side by side after each pass. For Cinemachine specifically: on the first-person view camera, set the camera body to `Do Nothing` (hard-parented to head transform). Only use Cinemachine damping for the shoulder camera or cinematic cameras — never for the gameplay view.

**Warning signs:**
- Weapons "work" but shooting feels unimpactful
- Screen shake is the only feedback on hit
- Audio and visual impact don't coincide (check AudioSource `PlayOneShot` timing vs VFX instantiation)
- Cinemachine default damping active on first-person camera (introduces camera lag)

**Phase to address:** Game Feel / Vertical Slice Polish phase (dedicated, after all mechanics are working)

---

### Pitfall 7: Input Architecture Violates Separation of Concerns and Blocks Testing

**What goes wrong:**
The existing `PlayerInput` class owns the `StateMachine<MovementState>` and constructs all state instances. This is already flagged as a P1 in CONCERNS.md. The downstream consequence: every test of movement state logic requires a fully constructed `PlayerInput` component with a connected `PlayerInput` Unity component, a camera, and all movement sub-components. States cannot be tested in isolation.

When bunny hop and slide-hop are added to this architecture, the input class grows further and the coupling worsens. The eventual refactor becomes more expensive with every new feature added before it is done.

**Why it happens:**
The input handling class is the first place movement logic appears when prototyping. It's fast to wire up but creates an architectural anchor that becomes harder to move as the system grows.

**How to avoid:**
The `PlayerMovementBrain` extraction must happen before new movement features are implemented — not after. The split: `PlayerInput` reads hardware and posts flags/events (one class, no FSM). `PlayerMovementBrain` constructs the FSM, creates state instances, owns the context object, and decides state transitions based on input flags. States themselves call movement components. This hierarchy is independently testable at each layer.

**Warning signs:**
- `PlayerInput.cs` exceeds 200 lines (currently ~180 and growing)
- Adding a new movement state requires editing `PlayerInput`
- Cannot instantiate a movement state in a unit test without a `PlayerInput` component
- Spanish-language TODOs in `PlayerInput` source marking it as "awful" (already present)

**Phase to address:** Movement System Audit (first action, before any feature work)

---

### Pitfall 8: Scope Creep During Vertical Slice Through Feature Curiosity

**What goes wrong:**
The vertical slice target is explicitly one level that proves movement and combat feel. Scope creep in this context looks like: adding an inventory system before the weapon system works; building a damage number UI before hit feedback is solid; prototyping Titan mechanics "just to see how it feels"; adding audio events to the AudioManager placeholder instead of leaving it for a dedicated audio phase.

The specific risk for StateShift: the codebase already has 5 systems in various states of incompletion (AudioManager placeholder, AIPerception empty class, ammo inventory TODO, fire rate not enforced, shotgun not implemented). Each of these is a scope gravity well — they attract "quick fixes" that each take a day but collectively derail the core movement/combat loop proof.

**Why it happens:**
Incomplete systems create anxiety. Developers feel they should "clean up" each loose end as they pass through. In a vertical slice, the correct answer is often to delete or stub incomplete systems rather than complete them — they are not part of the proof.

**How to avoid:**
Define the vertical slice exit criteria in binary terms: what must be true, what must be demonstrable. Then create an explicit "do not touch" list of systems outside that scope. For StateShift: `AudioManager`, `AIPerception`, fall damage, ammo inventory abstraction, and Titan mechanics are all out of scope until the movement + combat loop is demonstrated. Use `// VERTICAL SLICE: OUT OF SCOPE` comments as a forcing function — any PR that removes such a comment requires explicit sign-off.

**Warning signs:**
- Working on a system not in the current phase's requirements
- "While I'm in here" changes to out-of-scope systems
- Phase taking 2x longer than estimated (scope has already crept)
- Any Titan-related code appearing before the vertical slice is complete

**Phase to address:** All phases — scope discipline is a process constraint, not a one-time fix

---

## Technical Debt Patterns

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Copy-paste `ExitToAppropriateState()` across states | Fast to prototype new states | Three-way divergence when exit logic changes; already has 3 copies | Never — move to protected base method before adding a 4th state |
| `PlayerInput` owns FSM | Everything in one file, easy to prototype | Cannot test states without full input stack; blocks extraction of bunny hop logic | Never at this stage — extract before new movement features |
| `Invoke(nameof(X), delay)` without paired `CancelInvoke` | Simple delay implementation | Permanent state corruption on cancel (the current grapple lockout P0) | Never for state transitions — always pair with `CancelInvoke` |
| Hardcoded balance values in `WeaponBase` (e.g. `TotalAmmo = 90`) | Fast iteration on feel | Cannot tune without recompile; cannot expose to designers | Never — always route through `WeaponDataSO` |
| `Testing/` scripts outside `Editor/` folder | Scripts accessible in play mode | All test code compiles into production build, runtime crashes possible | Never — always gate with `#if UNITY_EDITOR` or move to `Editor/` folder |
| Generic ScriptableObject base classes | Elegant typing | Unity 2022 LTS does not serialize generic `ScriptableObject` subclasses in the Editor | Never for data assets — use concrete subclasses |
| `Debug.DrawLine` unconditionally in `FixedUpdate`/`Update` | Easy visual debugging | CPU cost every frame in production; profiler noise | Acceptable in dev builds only — gate with `#if UNITY_EDITOR` |

---

## Integration Gotchas

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| URP + Cinemachine | Using `CinemachinePostProcessing` extension (v2 stack compatible only) after URP migration | Use `CinemachineVolumeSettings` extension with URP Volume profiles |
| URP + custom shaders | Assuming Unity's Render Pipeline Converter handles custom `CGPROGRAM` shaders | Custom shaders require manual rewrite: `CGPROGRAM` → `HLSLPROGRAM`, update includes, add `"RenderPipeline" = "UniversalPipeline"` tag |
| URP + Built-in Post-Processing Stack v2 | Leaving PPv2 package installed after URP migration | Remove PPv2 package; URP post-processing is built-in via Volume framework |
| New Input System + FixedUpdate physics | Reading `WasPressedThisFrame()` inside `FixedUpdate` — misses inputs between fixed steps | Read input in `Update` callback or event; store flags in fields consumed by `FixedUpdate` |
| New Input System + jump buffering | No buffer window — missed inputs if jump pressed between FixedUpdate calls | Store `_jumpPressedTime = Time.time` on input event; check `(Time.time - _jumpPressedTime) < bufferWindow` in `FixedUpdate` |
| Rigidbody + Cinemachine first-person camera | Using Cinemachine body damping on the first-person view camera | Set body to `Do Nothing` for FPS view — damping introduces camera lag that reads as input latency |
| ProBuilder + URP | ProBuilder meshes use Standard shader by default — turn pink after URP migration | Batch-convert ProBuilder materials using Render Pipeline Converter after installing URP |

---

## Performance Traps

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| `Camera.main` called every `Update` frame | Slight per-frame cost; profiler shows `Camera.FindMainCamera` allocations | Cache in `Awake()`: `_camera = Camera.main` | Already broken in `PlayerInput` — fix in movement audit |
| `UIAnimationManager` lambda listener never removed | Memory leak; duplicate event firings on second enable cycle | Cache delegates as fields; use `RemoveAllListeners()` or named method references | Already broken in current codebase |
| `PlayerAdrenaline` polling every `Update` | Unnecessary computation every frame for a value that rarely changes | Use event-driven dirty flag — subscribe to momentum change events | Scales poorly once more adrenaline consumers are added |
| `Debug.DrawLine/Ray` in `GroundChecker` and `PlayerWallRun` unconditionally | Profiler noise, minor CPU in production | Wrap in `#if UNITY_EDITOR` | Every production build currently |
| Object instantiation for bullets without pooling | GC pressure during sustained fire; frame spikes on rapid-fire weapons | `ObjectPool<T>` — already in scope for the weapon redesign | Noticeable at shotgun pellet count (8-12 pellets per shot) |

---

## "Looks Done But Isn't" Checklist

- [ ] **Grapple system:** Looks done, but `CancelGrapple()` is missing `CancelInvoke` — verify the lockout P0 is fixed by testing: grapple, cancel during delay, attempt to grapple again
- [ ] **Weapon reload:** Looks done (ammo refills), but reserve count not respected and `ReloadTime` not enforced — verify reload from 1 reserve bullet gives only 1 round in magazine
- [ ] **Fire rate:** Looks done (weapon fires), but `RoundsPerMinute` timer not checked — verify rapid mouse clicks cannot fire faster than RPM allows
- [ ] **Shotgun:** Looks like a weapon, but fires a single projectile — verify multiple pellets with spread exist before calling it complete
- [ ] **URP migration:** Looks done (no pink materials), but Cinemachine post-processing blend may be silently broken — verify FOV changes and camera blend effects still work in play mode
- [ ] **Bunny hop:** Movement chain works, but test at exactly 60 Hz and 144 Hz — speed gain should be identical at both frame rates
- [ ] **PlayerMovementBrain extraction:** Looks done (new class exists), but verify `PlayerInput` no longer constructs or references `StateMachine<MovementState>` — search for `new StateMachine` in `PlayerInput.cs`
- [ ] **Health system damage overflow:** Looks done (damage applies), but test multi-chunk enemy with a 2x damage type — verify second chunk takes the correct raw overflow, not the multiplied overflow
- [ ] **Testing code exclusion from build:** Scripts in `Testing/` folder compile, but verify they are gated — check Build Settings > include/exclude and look for `#if UNITY_EDITOR` guards

---

## Recovery Strategies

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| P0 bugs discovered after new features built on top | HIGH | Rollback new feature branch, fix P0 in isolation on audit branch, rebase feature branch — retroactive fix with new code on top is higher risk |
| URP migration breaks scene content | MEDIUM | Keep migration on isolated branch; revert to Built-in branch if migration scope exceeds estimate; do not commit URP migration to main branch until all materials confirmed working |
| Weapon system needs base class refactor after 3rd weapon | HIGH | Treat as full redesign, not patch — extract `IFireMode` and `IReloadStrategy` retroactively; plan for 2-3 days per weapon to reconnect to new interface |
| Bunny hop is frame-rate dependent in shipped build | MEDIUM | Audit all velocity manipulation for `Time.deltaTime` scaling; move all physics to `FixedUpdate`; budget 1 day to audit + test |
| Scope creep detected mid-phase | LOW | Cut the out-of-scope item immediately — no partial completion; stub it with a `// DEFERRED: [reason]` comment and file an issue |
| Cinemachine post-processing silently broken after URP | LOW | Install `CinemachineVolumeSettings` extension, remove `CinemachinePostProcessing` component, reassign URP Volume profile |

---

## Pitfall-to-Phase Mapping

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| P0 bug state corruption pattern | Phase 1: Movement System Audit | All four P0 bugs fixed and verified with targeted playtest cases before any new movement code |
| PlayerInput owns FSM | Phase 1: Movement System Audit | `PlayerInput.cs` contains zero `StateMachine` references; `PlayerMovementBrain` constructs all states |
| URP migration breaks shaders and post-processing | Phase 2: URP Migration (isolated) | Zero pink materials in scene; Cinemachine FOV blend works; no PPv2 package in manifest |
| Rigidbody floaty feel | Phase 3: Movement Rebuild | Gravity multiplier in `PlayerDataSO`; fall arc matches Titanfall 2 reference; Rigidbody Interpolation set to Interpolate |
| Bunny hop velocity reset on land | Phase 3: Movement Rebuild | Slide-hop at max speed; land and immediately jump; speed retained and increased; identical result at 60 and 144 Hz |
| Jump input dropped between FixedUpdate calls | Phase 3: Movement Rebuild | Jump buffer window implemented and tested: tap jump just before landing from any height, jump must register |
| Weapon system brittle inheritance | Phase 4: Weapon System Redesign | Adding a 4th weapon type requires zero changes to `WeaponBase`; enemy uses same `IWeapon` interface |
| Generic ScriptableObject serialization | Phase 4: Weapon System Redesign | `IFireMode` implementations are concrete `ScriptableObject` subclasses, visible and editable in Unity Inspector |
| Fire rate / reload not enforced | Phase 4: Weapon System Redesign | `RoundsPerMinute` timer blocks fire; reload respects reserve count; `ReloadTime` enforced with lockout |
| Game feel feedback layering | Phase 5: Polish / Vertical Slice | Hit stop implemented; weapon recoil pattern deterministic; audio/visual impact coincide on frame timing |
| Cinemachine damping on FPS view | Phase 5: Polish / Vertical Slice | First-person camera body set to `Do Nothing`; no perceptible input lag on camera |
| Scope creep | All phases | Phase exit criteria defined in binary terms; each phase ships with zero known bugs |

---

## Sources

- Unity official docs: [Migrate from Built-In to URP workflow](https://docs.unity3d.com/6000.5/Documentation/Manual/urp/migrating-from-birp-workflow.html)
- Unity official docs: [Upgrade custom shaders for URP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/urp-shaders/birp-urp-custom-shader-upgrade-guide.html)
- Unity official docs: [Rigidbody Interpolation](https://docs.unity3d.com/Manual/rigidbody-interpolation.html)
- Unity official docs: [Input System timing and latency](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.13/manual/timing-and-latency.html)
- Unity official docs: [Optimize for fixed-timestep scenarios](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.12/manual/timing-optimize-fixed-update.html)
- Steam guide: [Air strafing and bunnyhopping mechanics](https://steamcommunity.com/sharedfiles/filedetails/?id=184184420)
- Steam guide: [TF2 movement — velocity cap and friction simulation](https://steamcommunity.com/sharedfiles/filedetails/?id=2982238486)
- Community analysis: [How Titanfall 2 made movement feel incredible](https://codegeekology.com/how-titanfall-2-made-its-movement-mechanics-feel-incredible/)
- Unity Blog: [Migrating Built-In shaders to URP](https://blog.unity.com/engine-platform/migrating-built-in-shaders-to-the-universal-render-pipeline)
- Unity Blog: [Advanced URP migration guide](https://blog.unity.com/engine-platform/move-on-over-to-the-universal-render-pipeline-with-our-advanced-guide)
- KinematicSoup: [Timesteps and smooth motion in Unity](https://kinematicsoup.com/news/2016/8/9/rrypp5tkubynjwxhxjzd42s3o034o8)
- Terresquall Blog: [Fix jittery camera with Rigidbody Interpolate](https://blog.terresquall.com/2021/12/fix-jittery-camera-movement-in-unity-with-rigidbody-interpolate/)
- Direct codebase analysis: `CONCERNS.md` (2026-03-26) — P0 through P5 bug classification

---
*Pitfalls research for: Unity 2022.3 LTS movement shooter FPS (StateShift)*
*Researched: 2026-03-26*
