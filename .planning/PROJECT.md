# StateShift

## What This Is

A singleplayer movement shooter FPS built in Unity, targeting Titanfall 2 pilot gameplay feel with Apex Legends-level gunplay polish. The immediate goal is a polished vertical slice — one level that proves the movement and combat loop feel exceptional. The architecture is designed from day one to scale into a full singleplayer campaign.

## Core Value

Movement must feel exceptional first — every traversal action (wall-run, slide-hop, bunny hop) must be snappy, momentum-preserving, and deeply satisfying before anything else ships.

## Requirements

### Validated

<!-- Shipped and confirmed valuable. -->

- ✓ Generic FSM with 5 outer movement states (Grounded, InAir, WallRunning, Dashing, Grappling) — existing
- ✓ Momentum system scaling speed from sprinting, sliding, downhill travel, and post-dash — existing
- ✓ PlayerAdrenaline damage reduction driven by momentum — existing
- ✓ Cinemachine camera with dynamic FOV and wall-run camera tilt — existing
- ✓ Unity New Input System integration — existing
- ✓ WeaponBase / WeaponInventory with Pistol and Shotgun — existing
- ✓ DamageMatrixSO for typed damage multipliers — existing
- ✓ Multi-chunk health system with IDamageModifier pipeline — existing
- ✓ Speed lines HUD + weapon ammo HUD (weap_Name, stored_mag, stored_reserve) — existing
- ✓ Singleton service layer + EventsManager — existing
- ✓ Scene loading with loading screen intermediary — existing
- ✓ AITickManager round-robin scheduling — existing

### Active

<!-- Current scope. Building toward these for the vertical slice milestone. -->

**Movement System (Audit → Rebuild/Fix)**
- [ ] Full movement system audit — classify each component as keep, fix, or replace before touching code
- [ ] Fix all P0 movement bugs: grapple lockout, GrapplingState immediate exit, CanGrapple race condition
- [ ] Extract `PlayerMovementBrain` — PlayerInput must only read hardware and post events; FSM construction and state wiring move out
- [ ] Bunny hop / air strafing — momentum-preserving directional jumps (TF2 AB-chaining feel)
- [ ] Slide-hop chain — slide into jump preserves and boosts speed; chainable

**Weapon System (Redesign)**
- [ ] `IWeapon` interface — `Fire()` and `Reload()` only; a weapon does not care what bullet it fires
- [ ] `WeaponBase` abstract class — implements `IWeapon`, fed by `WeaponDataSO`, decoupled from bullet type
- [ ] `IFireMode` — separate fire mode strategies (semi, auto, burst) injected via `WeaponDataSO`
- [ ] Reload system extracted to a dedicated class — `WeaponDataSO.ReloadTime` enforced, reload lockout applied
- [ ] Fire rate enforced — `RoundsPerMinute` timer in `WeaponBase`, no uncapped fire
- [ ] Ammo system corrected — reload respects reserve count, no negative reserves
- [ ] Weapons: Pistol (mag-fed), Sniper (mag-fed, ricochet bullet), Shotgun (shell-fed, multi-pellet spread)
- [ ] Generic `ObjectPool<T>` — bullets use pooled instances, pool uses Generics not `IWeapon`
- [ ] `BulletSystem` — bullets are physical entities with distinct behaviours (ricochet, penetration, etc.)
- [ ] `WeaponPickup` — implements `IInteractable.Interact()`, world-space pickup
- [ ] Enemies share the same `IWeapon` / `WeaponBase` system as the player

**Code Quality (Enforced Per Phase)**
- [ ] Zero hardcoded values — every balance value exposed via ScriptableObjects
- [ ] Zero known bugs at phase completion — each phase ships clean
- [ ] All code in English — no Spanish identifiers or comments
- [ ] `ExitToAppropriateState()` de-duplicated into shared base method
- [ ] Dead code removed — orphaned strategies, `CommandInvoker`, `NavMeshTester` empty class
- [ ] Testing code gated behind `#if UNITY_EDITOR` or moved to `Editor/`

**Rendering**
- [ ] URP migration — upgrade from Built-in Render Pipeline

**Game Feel (Vertical Slice Polish)**
- [ ] Hit feedback — projectile impact VFX, audio punch, screen shake (Apex-style)
- [ ] Responsive input — sub-frame input latency, no movement lag feel
- [ ] Camera work — FOV changes on speed bursts, landing squash, wall-run tilt polished
- [ ] Audio — movement sounds (slide, land, wall-run) and weapon sounds that punch

### Out of Scope

<!-- Explicit boundaries for the vertical slice milestone. -->

- Multiplayer / networking — not the target; adds massive scope and architecture burden
- Titan gameplay (rodeo, titan combat) — full campaign concern, not vertical slice
- Stim tactical ability — deferred to post-vertical-slice
- Campaign narrative, cutscenes, VO — vertical slice proves feel, not story
- Mobile / VR / console platforms — Windows PC standalone only
- Level editor or modding support — not planned
- Leaderboards / analytics — no live service infra this milestone

## Context

**Existing codebase:** Unity 2022.3.62f2 LTS, Built-in Render Pipeline (migrating to URP). ~105 C# scripts. Component-based MonoBehaviour architecture with custom generic FSM, Singleton service locator, ScriptableObject data layer. Cinemachine, Unity New Input System, TextMesh Pro, ProBuilder already integrated.

**Known P0 bugs to resolve in movement audit phase:**
1. `PlayerGrapple.CancelGrapple()` missing `CancelInvoke` → permanent grapple lockout
2. `GrapplingState.OnUpdate()` exits immediately because `_isGrappling` is false during the launch delay
3. `BaseHealth.TakeDamage()` double-applies damage type multiplier across chunk boundaries
4. `CanGrapple` race during delay window allows double-grapple queue

**Known structural debt to resolve:**
- `PlayerInput` owns the FSM (violates separation of concerns) — extract to `PlayerMovementBrain`
- `ExitToAppropriateState()` copy-pasted in DashingState, WallRunningState, GrapplingState
- `PlayerJumpStrategy` fully implemented but not connected — either wire it or delete it
- Dead code: `CommandInvoker`, `JumpCommand`, `MoveCommand`, `IMovementStrategy` + implementations
- `AudioManager` is a placeholder — no event wiring, hardcoded clip, no singleton

**Feel references:** Titanfall 2 pilot traversal as the primary target. Apex Legends for gunplay refinement and hit feedback fidelity.

**Weapon system intent (from design diagram):**
Two independent systems run simultaneously — the weapon system and the bullet system. A weapon fires and reloads; it does not care what projectile it spawns. `WeaponData` feeds the weapon and specifies which `IFireMode` and which bullet type. The bullet system manages physical projectiles via a generic `ObjectPool<T>`. Reload logic lives in its own class. Enemies use the exact same `IWeapon` / `WeaponBase` hierarchy as the player.

## Constraints

- **Tech stack**: Unity 2022.3 LTS — no engine upgrade this milestone
- **Render pipeline**: Migrating to URP — all shaders and post-processing must be URP-compatible after migration
- **Platform**: Windows PC Standalone only — no cross-platform build constraints
- **Code quality**: Zero hardcoded values (all balance via SOs), zero known bugs per phase, English-only identifiers and comments, strict folder/naming conventions enforced
- **Architecture**: New systems must respect separation of concerns — input, state, movement, and weapon systems are independently testable units

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Audit movement system before modifying | Avoids introducing new bugs into code with 4 active P0s; need full picture before committing to fix vs rebuild | — Pending |
| URP upgrade | Built-in pipeline blocks modern shader graph and limits visual fidelity ceiling; upgrade cost worth it now before more content is authored | — Pending |
| Enemies share IWeapon / WeaponBase | Eliminates a parallel combat system for AI; all weapon tuning applies equally to player and enemy guns | — Pending |
| Reload system extracted to its own class | Reload behaviour (timing, lockout, shell-fed vs mag-fed) varies per weapon type; keeping it separate makes new weapons trivial to add | — Pending |
| Generic ObjectPool\<T\> required | Non-generic pool would require casting and type checks per bullet type; generics enforce type safety and make adding new bullet types zero-cost | — Pending |
| Vertical slice first, campaign later | Proves feel and architecture before committing to full campaign scope; all systems designed to scale up | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd:transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd:complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-03-26 after initialization*
