# Project Research Summary

**Project:** StateShift
**Domain:** Singleplayer movement shooter FPS (Titanfall 2 pilot feel, Apex gunplay)
**Researched:** 2026-03-26
**Confidence:** MEDIUM-HIGH — stack and architecture HIGH confidence from official docs and verified reference implementations; features MEDIUM-HIGH from developer interviews and community documentation; pitfalls HIGH from direct codebase analysis.

---

## Executive Summary

StateShift is a singleplayer PC FPS built around high-speed momentum movement (Titanfall 2 pilot style) with physical projectile combat. The game concept is proven — the genre has a clear feature fingerprint, well-documented physics models, and strong architectural precedents. The recommended approach treats the project as two sequential proof-of-concepts: first make movement feel correct (bunny hop chains, wall-run speed buildup, grapple swing momentum), then make combat feel correct (physical bullets, layered hit feedback, three distinct weapon archetypes). Neither phase is ready to start today without first resolving four P0 bugs and extracting the FSM from `PlayerInput` — both of which are architectural prerequisites that block correct behavior in every system that follows.

The stack requires one significant infrastructure change this milestone: URP migration from the Built-In render pipeline. This is a well-scoped, one-time operation that should run in an isolated branch before any feature work begins. Everything else in the stack stays as-is — Input System 1.14.2, Cinemachine 2.10.x (do not upgrade to 3.x), Unity 2022.3 LTS. The weapon system requires a full redesign using Strategy + Template Method patterns to support three weapon archetypes (pistol, shotgun, sniper) sharing the same `IWeapon` interface with enemies. The existing Singleton/EventsManager/FSM infrastructure is sound and carries forward.

The primary risk is building new mechanics on top of unresolved state corruption. The grapple lockout P0, the `GrapplingState` immediate-exit P0, the health overflow bug, and the input architecture violation are all load-bearing problems — each one will reproduce itself in new movement mechanics if not fixed first. Scope discipline is the second risk: the codebase has five incomplete subsystems that create gravity wells for "quick fix" detours. The vertical slice exit criteria must be binary, and everything outside movement + combat + three weapons is explicitly deferred.

---

## Key Findings

### Recommended Stack

The engine stays at Unity 2022.3 LTS with no upgrade this milestone. The single required infrastructure change is installing URP 14.x (bundled with 2022.3) and removing the legacy Post-Processing Stack v2 package, which is silently incompatible with URP. The Render Pipeline Converter automates standard material conversion; the `CrosshairShader.shader` requires a manual HLSL rewrite.

All other packages remain at current versions. Cinemachine must stay at 2.10.x — Cinemachine 3 has no automated code migration path and three existing custom camera scripts (`CameraController.cs`, `DynamicFOV.cs`, `WallRunCamaraEffects.cs`) would require full rewrites for zero functional gain this milestone. The physics architecture stays on dynamic Rigidbody with manual velocity control — CharacterController cannot expose velocity as a mutable vector and is therefore incompatible with Quake-style air acceleration. See `STACK.md` for full package manifest.

**Core technologies:**
- **URP 14.x:** Render pipeline replacement — Shader Graph support, Volume post-processing, SRP Batcher; Built-In pipeline has no upgrade path and blocks visual ceiling
- **Unity Input System 1.14.2 (keep):** Dynamic Update mode for lowest latency; discrete events (jump/dash/grapple) read via `WasPressedThisFrame` in `Update` and passed as flags into `FixedUpdate` via context
- **Rigidbody + custom velocity:** Momentum-preserving movement requires direct `rb.linearVelocity` manipulation; CharacterController hides velocity and prevents air strafing
- **60 Hz fixed timestep (0.01666f):** Physics step matches render frame at 60 FPS; eliminates perceptible physics/visual stutter
- **Cinemachine 2.10.x (keep, do not upgrade):** FOV dynamics, camera tilt, and camera shake all working; CM3 API is a breaking change with no migration tool
- **UnityEngine.Pool.ObjectPool\<T\> (built-in):** Bullet pooling at 600 RPM requires zero-allocation steady state; built into engine, no install required
- **AudioManager (needs implementation):** Currently a placeholder; needs to become a proper `Singleton<T>` wired to `EventsManager` events — no FMOD/Wwise needed for vertical slice

### Expected Features

The vertical slice must prove two things: movement feels like Titanfall 2, and combat feels like Apex Legends gunplay. Every feature in the v1 list is necessary to make that proof. Features marked v1.x and v2+ are explicitly deferred.

**Must have (table stakes — game reads as unfinished without these):**
- Bunny hop / slide-hop chain with velocity-projection physics (zero Rigidbody drag)
- Air strafing with near-zero air friction and tunable `airAccelerate` parameter
- Wall run: lateral gravity override, speed boost on initiation, camera tilt, exit kick
- Wall-to-wall chain with no-repeat rule
- Jump buffering (100ms window) + coyote time (80-120ms) — prerequisite for hop chains feeling accessible
- Grapple with swing momentum (all 4 P0 bugs fixed first)
- Dynamic FOV on speed bursts (already exists, needs tuning)
- Physical bullet system with `ObjectPool<T>` before any weapon ships
- Layered hit feedback: hit marker + impact VFX + audio punch + camera impulse (all 4 layers simultaneously)
- Pistol (semi-auto, mag-fed), Shotgun (shell-fed, multi-pellet spread), Sniper (mag-fed, ricochet bullet)
- Weapon pickup via `IInteractable`
- Movement audio for every state transition (slide, land, wall-run)

**Should have (differentiators that define the feel):**
- Reload animation cancel at "seated" phase (not at start)
- ADS spread scaling (spread based on ADS state, not movement state — never penalize movement)
- Always-sprinting option (remove shift-hold tax)
- Enemy weapons sharing `IWeapon` hierarchy (balance parity, weapon drops)
- Shell-fed shotgun interrupt/resume reload

**Anti-features to avoid (these kill movement shooter feel):**
- Hard speed cap on all movement — cap only base ground speed, let momentum techniques accumulate freely above
- High air friction / drag — destroys hop chains; set to zero or near-zero in air
- Symmetrical jump arc — apply higher gravity multiplier on descent
- Screen shake on outgoing shots — motion sickness during rapid fire; use camera impulse on incoming damage only
- Damage-interrupt on hit — never interrupt locomotion velocity
- Bullet spread/bloom on moving shots — penalizes the core design loop

**Defer (v2+):**
- Stim tactical (speed + regen boost)
- Phase Shift
- Tap strafing
- Titan gameplay
- Style meter (ULTRAKILL-style)

### Architecture Approach

The architecture is layered in five tiers: Input Layer (`PlayerInput` emits `InputFrame` struct) → Brain/Logic Layer (`PlayerMovementBrain` owns FSM and writes `PlayerMovementContext`) → Movement FSM Layer (outer state machine for locomotion mode, inner for grounded sub-states) → Physics Layer (`PlayerMovement` applies Quake-model acceleration to Rigidbody, `BulletSystem` manages pooled projectiles) → Service Layer (singletons: `EventsManager`, `GameManager`, `AITickManager`, `AudioManager`). The weapon system sits in the Physics/Logic boundary: `WeaponHolder` owns `IWeapon`, shared between player and enemy; `WeaponBase` uses Template Method with injected `IFireMode` strategies; `BulletBase` self-releases to pool on impact or lifetime expiry.

The most important architectural constraint: no FSM state zeros horizontal velocity on exit by default. Velocity carries across every state transition unless explicitly killed by a named method (`PlayerMovement.KillHorizontalVelocity()`). This is the physical mechanism that enables bunny hop, slide-hop, and wall-run exit speed. Violating this at any state boundary silently breaks the momentum loop.

**Major components:**
1. `PlayerInput` — hardware-only; reads bindings, emits `InputFrame` struct; owns zero FSM logic
2. `PlayerMovementBrain` — constructs FSM, wires states, reads `InputFrame`, writes `PlayerMovementContext`; extracted from current `PlayerInput` (P1 debt item)
3. `PlayerMovement` — Rigidbody velocity control; implements Quake projection-based air acceleration; applies asymmetric gravity multiplier on descent
4. `WeaponHolder` — owns `IWeapon` reference; shared by player and enemy via identical `Fire()`/`Reload()` interface
5. `WeaponBase` (abstract) — Template Method: enforces fire rate timer, reload lockout, ammo checks; delegates firing cadence to `IFireMode` strategy SO (stateless logic) + per-instance `FireModeState` struct
6. `BulletSystem` + `BulletPool<T>` — pool registry per bullet prefab type; `BulletBase` calls `ReturnToPool()` delegate on hit; zero allocations in steady state
7. `EnemyShootingController` — AI decision layer only; calls `WeaponHolder.Fire()` — contains no weapon logic
8. `ReloadHandler` — extracted coroutine managing inserting/seated/complete phases; enables reload cancel at seated phase

### Critical Pitfalls

1. **Building new mechanics on top of P0 bugs** — The grapple lockout (`CancelInvoke` missing), `GrapplingState` immediate exit, health overflow, and CanGrapple race all share the same root cause: mutable state set without a cleanup path. Fix all four before touching any new movement feature or the bugs reproduce in every new mechanic. Verify fix with specific playtest cases before proceeding.

2. **URP migration breaks all custom shaders and the post-processing stack** — `CGPROGRAM` shaders do not render at all under URP; they require manual conversion. PPv2 package is silently incompatible (no errors, effects simply stop working). Cinemachine post-processing extension must swap from `CinemachinePostProcessing` to `CinemachineVolumeSettings`. Run migration in an isolated branch and verify Cinemachine FOV/tilt blending in play mode before merging.

3. **Bunny hop velocity reset caused by ground friction timing** — Ground friction applied the frame of landing zeros horizontal momentum before jump input processes. Prevention requires: jump buffer window checked before friction deceleration in `FixedUpdate`, not after; no speed cap applied during the jump window; Input System in Dynamic Update mode (not Fixed Update) so jump presses aren't dropped between steps.

4. **PlayerInput owning the FSM blocks every subsequent change** — Cannot test movement states without full input stack; adding new movement features deepens the coupling. Extract `PlayerMovementBrain` first, before any feature work. This is the prerequisite that gates everything else.

5. **Weapon inheritance growing to contain conditional branching** — Adding a third weapon type by modifying `WeaponBase` is the failure mode. `IFireMode` strategies must use concrete `ScriptableObject` subclasses (not generic base classes — Unity 2022 LTS does not serialize generic SO subclasses in the Inspector). `WeaponBase` must not contain `if (weaponType == Shotgun)` conditionals. Enemy uses same `IWeapon` interface with `InfiniteAmmo` as a `WeaponDataSO` field, not a separate class.

---

## Implications for Roadmap

Architecture research provides explicit build-order sequencing with hard dependency rationale. The 6-phase order below is derived from that dependency graph and validated against the pitfall map. Do not reorder phases 1-3 — they have blocking dependencies.

### Phase 1: Input / Brain Separation + P0 Bug Fixes

**Rationale:** Every subsequent phase is blocked by two problems that must be resolved first. The `PlayerInput`-owns-FSM violation means movement states cannot be modified or tested without full input wiring — any new movement feature added now compounds the cost of the eventual extract. The four P0 bugs (grapple lockout, GrapplingState immediate exit, health overflow, CanGrapple race) have a shared root cause (async state without cleanup path) that will reproduce in new mechanics if not resolved.
**Delivers:** `PlayerInput` reads hardware only and emits `InputFrame` struct. `PlayerMovementBrain` owns FSM construction, state wiring, and context writes. All four P0 bugs fixed and verified. `StateMachine.Clear()` reset fixed. `ExitToAppropriateState()` de-duplicated into protected base method. Dead code deleted (`CommandInvoker`, orphaned strategy files).
**Addresses (from FEATURES.md):** Grapple (prerequisite: bugs fixed before grapple counts as shipped)
**Avoids (from PITFALLS.md):** Pitfall 1 (P0 state corruption compounding), Pitfall 7 (input architecture blocking testing)
**Research flag:** No deeper research needed — pattern is well-documented, codebase violations are clearly identified.

---

### Phase 2: URP Migration

**Rationale:** URP migration affects every shader, every material, and the post-processing stack. Running it in isolation before movement/weapon work begins means no in-progress systems are disrupted by the pipeline switch. Running it after would require verifying all newly added VFX and shaders twice. The migration is bounded and reversible only while it runs on an isolated branch.
**Delivers:** URP 14.x installed. PPv2 package removed. `CrosshairShader.shader` manually rewritten in HLSL. All ProBuilder materials batch-converted. Cinemachine extension swapped to `CinemachineVolumeSettings`. URP Pipeline Asset configured for PC FPS (Forward rendering, 4x MSAA, 2 shadow cascades). FOV and camera tilt blending verified in play mode.
**Uses (from STACK.md):** URP 14.x, Shader Graph 14.x; verify `com.coffee.ui-particle` URP compatibility
**Avoids (from PITFALLS.md):** Pitfall 2 (URP breaks shaders and post-processing silently)
**Research flag:** No deeper research needed — official URP upgrade guide covers all required steps. Manual shader conversion steps fully documented in STACK.md.

---

### Phase 3: Movement System Rebuild

**Rationale:** Movement feel is the game's core design proof. A broken movement base makes everything built on top of it feel wrong regardless of quality. Bunny hop, air strafing, slide-hop, wall-run chain, grapple swing, jump buffer, and coyote time must all work correctly and feel like TF2 before combat iteration begins. All physics tuning parameters must be exposed via `PlayerDataSO` — never hardcoded.
**Delivers:** Quake projection-based air acceleration (`ApplyAirAcceleration` with `dot(velocity, wishDir)` projection model). Zero Rigidbody drag with per-state custom drag. Asymmetric gravity multiplier on descent. 60 Hz fixed timestep. Jump buffering (100ms) and coyote time (80-120ms) in `PlayerMovementContext`. Slide-hop velocity-additive jump impulse in `SlidingState.OnExit()`. Wall-run speed boost over duration + exit kick. Wall-to-wall no-repeat rule. Grapple spring/pendulum physics with swing momentum. `Camera.main` cached in `Awake`. `OnGUI` debug removed. Rigidbody set to Interpolate + Continuous Collision Detection + rotation frozen + `useGravity = false`.
**Addresses (from FEATURES.md):** Bunny hop/slide-hop chain, air strafing, wall run + chain, grapple swing momentum, jump buffering + coyote time, dynamic FOV tuning
**Avoids (from PITFALLS.md):** Pitfall 3 (floaty Rigidbody defaults), Pitfall 4 (bunny hop velocity reset on friction timing)
**Research flag:** Quake air acceleration math is fully documented (adrianb.io + CPMPlayer.cs reference). Tuning pass will need playtesting — may need a short research-phase for wall-run physics feel parameters.

---

### Phase 4: Weapon System Redesign

**Rationale:** `IWeapon` / `IFireMode` interface design must be locked before `BulletBase` and the pool are built — bullet types depend on weapon hierarchy. Weapon interfaces must also be stable before `WeaponHolder` is wired to both player and enemy. The interface boundary (`IWeapon` as the only surface `WeaponHolder` and `EnemyShootingController` touch) must be defined here, not retrofitted after concrete weapons exist.
**Delivers:** `IWeapon` interface + `WeaponBase` abstract MonoBehaviour (Template Method). `WeaponHolder` shared component (player + enemy). `IFireMode` as stateless ScriptableObject strategies with concrete non-generic subclasses (`SemiAutoFireMode`, `FullAutoFireMode`, `BurstFireMode`). `FireModeState` struct owned by `WeaponBase`. `ReloadHandler` extracted coroutine (inserting/seated/complete phases). Fire rate timer enforced via `RoundsPerMinute`. Reload respects reserve count with no negative reserves. `WeaponDataSO` extended with `FireMode`, `BulletPrefab`, `DamageType`, `InfiniteAmmo`.
**Implements (from ARCHITECTURE.md):** Pattern 3 (IWeapon/WeaponBase/IFireMode), Pattern 5 (shared weapon system for player + enemy)
**Avoids (from PITFALLS.md):** Pitfall 5 (weapon inheritance brittleness + generic SO serialization failure)
**Research flag:** No deeper research needed — Strategy + Template Method patterns are well-established; Unity SO serialization constraint is documented.

---

### Phase 5: Bullet System + ObjectPool + Three Weapons

**Rationale:** `ObjectPool<T>` requires stable bullet types (Phase 4). Concrete weapon implementations (`Pistol`, `Shotgun`, `Sniper`) require stable `WeaponBase` and pool infrastructure. The pool must exist before any weapon fires its first bullet — Instantiate/Destroy at 600 RPM causes GC spikes mid-fight.
**Delivers:** `BulletBase` abstract component (movement, hit detection, self-release). `StandardBullet`, `RicochetBullet` (Sniper), `ShotgunPellet` concrete implementations. `BulletPool<T>` wrapping `UnityEngine.Pool.ObjectPool<T>`. `BulletSystem` registry keyed by bullet prefab type. `Pistol` (semi-auto, mag-fed). `Shotgun` (shell-fed, multi-pellet ExecuteFire, per-shell reload loop, interruptible at seated). `Sniper` (mag-fed, ricochet bullet via surface normal reflection). `WeaponPickup` implementing `IInteractable`. Layered hit feedback: hit marker color change, impact VFX particle burst, audio punch, CinemachineImpulseSource on incoming damage only.
**Addresses (from FEATURES.md):** Physical bullet system, ricochet behavior, shell-fed shotgun reload, multi-pellet spread, layered hit feedback (all 4 channels), weapon pickup
**Avoids (from PITFALLS.md):** Pitfall 5 (ObjectPool replaces Instantiate/Destroy allocation pattern), Pitfall 6 (game feel requires all 4 feedback layers simultaneously — not screen shake alone)
**Research flag:** Ricochet bullet physics (surface normal reflection + velocity decay per bounce) may benefit from a targeted research session. Shotgun shell-fed interrupt/resume reload state machine is non-trivial.

---

### Phase 6: Enemy Weapon Integration + AI Perception

**Rationale:** Enemy combat requires the full weapon system (Phases 4+5) to be stable. AI perception (line-of-sight, cone check) requires the scene to be in a playable movement state (Phases 1-3) to validate sight-line behavior against a moving player. Enemy weapons using the same `IWeapon` interface as the player is an architectural requirement — not an optional feature — for balance parity.
**Delivers:** `EnemyShootingController` decision layer calling `WeaponHolder.Fire()`. `AIPerception` with line-of-sight ray cast and cone check. `AIMemory` write paths for `CanSeePlayer` and `LastKnownPlayerPosition` (fixes existing `LastSeenPleyerTime` typo). Enemy configured with own `WeaponDataSO` assets. Enemy weapon drops enabling player pickup.
**Implements (from ARCHITECTURE.md):** Pattern 5 (Enemy calls WeaponHolder — zero weapon code duplication)
**Research flag:** May need a targeted research session for AI sight-line tuning in vertical spaces (see FEATURES.md anti-pattern: NavMesh-only enemies in vertical spaces).

---

### Phase 7: Polish — Audio, VFX, URP Visual Pass

**Rationale:** Game feel polish (hit stop, weapon recoil patterns, audio/visual timing alignment, Cinemachine first-person camera configuration) requires all mechanics to be working first. Polish applied before mechanics are stable gets invalidated by subsequent changes. This is a dedicated pass, not tasks scattered across earlier phases.
**Delivers:** `AudioManager` proper `Singleton<T>` implementation with `EventsManager` event wiring. Movement audio for every FSM state transition (slide initiation, landing impact, wall-run, wall-jump). Weapon audio layering (muzzle crack + impact thud + tail-off). Slide dust particle burst. Landing squash (CinemachineImpulse on Grounded state enter). First-person camera body set to `Do Nothing` (zero Cinemachine damping on gameplay view). `Debug.DrawLine` and `OnGUI` calls gated with `#if UNITY_EDITOR`. Testing scripts in `Testing/` folder gated or moved to `Editor/` folder.
**Addresses (from FEATURES.md):** Movement audio, weapon audio, slide dust VFX, landing squash, wall-run camera tilt polish, speed lines HUD
**Avoids (from PITFALLS.md):** Pitfall 6 (game feel requires simultaneous layering — dedicated phase, not scattered additions); Cinemachine damping on FPS view causing perceived input lag

---

### Phase Ordering Rationale

- **Phases 1-2 are both prerequisites with no features.** Phase 1 (P0 fixes + Brain extraction) must precede all movement work or bugs reproduce in new mechanics. Phase 2 (URP) must run in isolation on a clean branch before any new VFX or shaders are authored.
- **Phase 3 before Phase 4** because movement feel validates the game concept independently. A broken weapon on correct movement is debuggable. Any weapon iteration on broken movement produces false signals. The P0 grapple bugs also affect FSM behavior during weapon tests — resolving them first means weapon testing in Phase 4-5 starts on clean state machine behavior.
- **Phase 4 before Phase 5** because interface design must be locked before pool and bullet types are built against it. Pool keying requires knowing bullet prefab types; bullet hierarchy requires knowing WeaponBase's ExecuteFire contract.
- **Phase 5 before Phase 6** because enemy weapons require a stable weapon system and pooled bullet types.
- **Phase 7 last** because polish gets invalidated by mechanics changes. Game feel pass as a dedicated final phase is the only approach that produces lasting results.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 3 (Movement Rebuild):** Wall-run feel tuning (speed curve over duration, lateral gravity override magnitude) has no single authoritative source — will need iterative playtesting parameters. Targeted research session recommended for wall-run physics specifically.
- **Phase 5 (Bullets):** Ricochet bullet (surface normal reflection with velocity decay per bounce) and shotgun shell-fed interrupt/resume reload are non-trivial implementations. Recommend research-phase before implementing.
- **Phase 6 (Enemy AI):** Enemy navigation in vertical spaces (off-mesh links for wall-run paths, or intentional design decision to be ground-only) needs design decision before AI phase begins.

Phases with standard patterns (skip research-phase):
- **Phase 1 (Input/Brain separation):** Clear refactor with defined target architecture. No unknowns.
- **Phase 2 (URP migration):** Official Unity upgrade guide covers all steps. STACK.md documents every manual action required.
- **Phase 4 (Weapon interfaces):** Strategy + Template Method patterns are textbook. SO serialization constraint documented.
- **Phase 7 (Polish):** Standard polish checklist. Cinemachine configuration well-documented.

---

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | All package versions verified against Unity 2022.3 LTS official docs. Cinemachine 3 deferral rationale verified against official CM3 upgrade guide. |
| Features | MEDIUM-HIGH | Movement physics (bunny hop, air strafing) verified against Quake III source and developer interviews. TF2 feel targets sourced from official developer interviews. Some community wiki sources for specific mechanic behavior. |
| Architecture | HIGH | Movement math from verified adrianb.io analysis matching Quake III source. Weapon patterns from established Unity community + Game Programming Patterns references. Pool API from official Unity docs. |
| Pitfalls | HIGH | P0 bugs identified via direct codebase analysis — no inference. URP migration pitfalls from official Unity blog and docs. Physics timing pitfalls from official Input System timing docs. |

**Overall confidence:** HIGH for sequencing and architecture decisions; MEDIUM for exact feel-tuning parameter values (gravity multiplier, air acceleration, wall-run speed curve) which require playtesting to finalize.

### Gaps to Address

- **Gravity and physics feel parameters:** Research identifies correct architecture (asymmetric gravity multiplier, per-state drag, zero Rigidbody drag) but exact numeric values (`-30` to `-40 m/s²` gravity, `airAccelerate` range) must be validated through a playtesting pass. Expose all via `PlayerDataSO` and plan a dedicated tuning session after Phase 3 mechanics are implemented.
- **Wall-run feel curve:** Speed buildup over wall-run duration and the decay curve determining when the player is incentivized to chain vs. stay have no single authoritative source. Plan iterative tuning; treat TF2 reference footage as the target.
- **`com.coffee.ui-particle` URP compatibility:** STACK.md flags this as "verify post-migration." Add explicit URP compatibility check as a Phase 2 exit criterion.
- **Testing folder build exclusion:** Scripts in `Assets/Scripts/Testing/` compile into production builds. This needs resolution in Phase 1 or Phase 7 — either `#if UNITY_EDITOR` guards or relocation to `Editor/` folder. Not blocking but is a known production build risk.

---

## Sources

### Primary (HIGH confidence)
- Unity URP 14 Official Docs — shader upgrade guide, Render Pipeline Converter, performance configuration
- Cinemachine 3 Upgrade Guide (official) — deferral rationale (no automated code migration)
- Unity Input System 1.14.2 Official Docs — timing, latency, Dynamic vs Fixed Update mode
- Unity ObjectPool API (official) — pool pattern, create/get/release/destroy callbacks
- Titanfall 2 Designer Interview (Game Developer) — control design rationale, movement feel intent
- Bunnyhopping from the Programmer's Perspective (adrianb.io) — Quake III air acceleration algorithm, friction window analysis
- Direct codebase analysis (`CONCERNS.md`, `PlayerInput.cs`, weapon scripts) — P0 bug classification

### Secondary (MEDIUM confidence)
- Quake3 CPM movement reference implementation (WiggleWizard/quake3-movement-unity3d) — Rigidbody vs CharacterController decision
- Unity Discussions: SO-based gun system, Shooter class for player + enemy — architecture pattern validation
- Titanfall 2: How Design Informs Speed (Medium) — movement design analysis
- Steam Guide: Titanfall 2 Advanced Movement — mechanic documentation
- Titanfall 2 Pilot Tacticals / Slide Hop (Community Wikis) — mechanic behavior documentation
- Unity Blog: Migrating Built-In shaders to URP — migration gotcha documentation
- KinematicSoup: Timesteps and smooth motion in Unity — fixed timestep rationale

### Tertiary (MEDIUM-LOW confidence, needs validation)
- Titanfall 2 feel analysis articles — community analysis, not official; used to confirm movement tuning targets but numeric values need playtesting
- Apex Legends recoil patterns (Dexerto) — reference for deterministic recoil patterns; verified against game behavior

---

*Research completed: 2026-03-26*
*Ready for roadmap: yes*
