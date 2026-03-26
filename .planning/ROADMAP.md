# Roadmap: StateShift

## Overview

This milestone scopes to movement only. The goal is to get movement feeling right before anything else ships. That means fixing the broken architectural foundation first (four P0 bugs + input/FSM violation), then rebuilding the movement mechanics from the ground up (bunny hop, slide-hop, jump buffer, coyote time, wall-run speed curve), and finally layering on the audio-visual feedback that makes movement feel alive (movement audio, dynamic FOV, camera work).

Weapon system, bullet system, enemy integration, and URP migration are deferred to a future milestone. They will not be touched until movement is polished and signed off.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [ ] **Phase 1: Input / Brain Separation + P0 Fixes** - Extract PlayerMovementBrain, fix all four P0 bugs, and purge dead code
- [ ] **Phase 2: Movement System Rebuild** - Bunny hop, slide-hop, jump buffering, coyote time, wall-run speed curve, and physics configuration
- [ ] **Phase 3: Movement Feel Polish** - Movement audio, dynamic FOV, UIAnimationManager correctness

## Phase Details

### Phase 1: Input / Brain Separation + P0 Fixes
**Goal**: PlayerInput reads hardware only and the FSM runs cleanly — no state corruption, no architectural coupling blocking future work
**Depends on**: Nothing (first phase)
**Requirements**: ARCH-01, ARCH-02, ARCH-03, ARCH-04, ARCH-05, ARCH-06, ARCH-07, ARCH-08, ARCH-09, ARCH-10, MOVE-08, MOVE-09
**Success Criteria** (what must be TRUE):
  1. PlayerInput contains no FSM construction, state wiring, or movement context writes — only hardware reads and InputFrame emission
  2. Grapple can be cancelled at any point without causing CanGrapple = false lockout, and a second TryStartGrapple during the delay window does not queue a second grapple
  3. StateMachine.Clear() resets _currentStateType to default, and ExitToAppropriateState() exists once in a shared base class
  4. Dead code files (CommandInvoker, JumpCommand, MoveCommand, orphaned strategy interfaces) are deleted from the project
  5. All testing scripts are absent from production builds — either gated with #if UNITY_EDITOR or moved to an Editor folder
**Plans**: 5 plans

Plans:
- [ ] 01-01-PLAN.md — Extract PlayerMovementBrain from PlayerInput (ARCH-01)
- [ ] 01-02-PLAN.md — Fix three grapple P0 bugs: lockout, double-grapple, immediate exit (ARCH-02, MOVE-08, MOVE-09)
- [ ] 01-03-PLAN.md — Fix StateMachine.Clear() and consolidate ExitToAppropriateState (ARCH-03, ARCH-04)
- [ ] 01-04-PLAN.md — Fix damage overflow, delete dead code, gate testing scripts (ARCH-02, ARCH-07, ARCH-08)
- [ ] 01-05-PLAN.md — Cache Camera.main, gate debug draws, replace Spanish, verify ARCH-05 (ARCH-05, ARCH-06, ARCH-09, ARCH-10)

### Phase 2: Movement System Rebuild
**Goal**: Player can chain movement techniques (bunny hop, slide-hop, wall-run) with momentum-preserving physics that feel like Titanfall 2
**Depends on**: Phase 1
**Requirements**: MOVE-01, MOVE-02, MOVE-03, MOVE-04, MOVE-05, MOVE-06, MOVE-07, MOVE-10, MOVE-11, MOVE-12
**Success Criteria** (what must be TRUE):
  1. Player can chain bunny hops — each jump preserves horizontal speed and directional air strafing increases speed (no hard velocity cap accumulating above base ground speed)
  2. Sliding into a jump boosts exit speed, and the jump fires on the first grounded frame when input was buffered within 100ms before landing
  3. Player can jump within 100ms of walking off a ledge without consuming a jump (coyote time), and wall run does not stop when sprint input is released
  4. Wall run speed follows a rise-then-decay curve that incentivises chaining the next move
  5. All movement parameters (air acceleration, max air speed, gravity multiplier, fall gravity multiplier) are tunable via PlayerMovementDataSO with no hardcoded magic numbers in gameplay code
**Plans**: TBD

### Phase 3: Movement Feel Polish
**Goal**: Every movement action has audio-visual feedback that makes traversal feel as responsive as the Titanfall 2 reference — no weapon feedback in this phase
**Depends on**: Phase 2
**Requirements**: FEEL-03, FEEL-04, FEEL-05
**Success Criteria** (what must be TRUE):
  1. Slide start, landing, and wall-run start each play a distinct audio event with no missing or doubled triggers
  2. Dynamic FOV scales up during speed bursts (post-slide, post-dash, bunny hop chain) and returns smoothly to base FOV with no popping or discontinuity
  3. UIAnimationManager removes all listeners on OnDisable — re-enabling the component does not produce duplicate invocations
**Plans**: TBD
**UI hint**: yes

## Future Milestone: Combat + Rendering

The following phases are deferred. They depend on polished movement being signed off first and will be scoped into a separate milestone.

### Future Phase: URP Migration
**Goal**: Project runs on URP 14.x with all shaders rendering correctly and post-processing working via URP Volumes
**Deferred requirements**: REND-01, REND-02, REND-03, REND-04
**Reason for deferral**: URP migration is a high-risk pipeline change with no movement dependency. Deferring keeps the movement milestone clean and unblocked.

### Future Phase: Weapon System Redesign
**Goal**: A clean weapon interface architecture exists that both player and enemy can share — fire rate enforced, reload locked out correctly, ammo reserves respected
**Deferred requirements**: WEAP-01, WEAP-02, WEAP-03, WEAP-04, WEAP-05, WEAP-06, WEAP-07, PICK-01, PICK-02
**Reason for deferral**: Weapon architecture redesign is independent of movement quality. It begins only after movement is signed off.

### Future Phase: Bullet System + Three Weapons
**Goal**: Three distinct weapon archetypes fire pooled physical bullets with correct per-weapon behaviour
**Deferred requirements**: WEAP-08, WEAP-09, WEAP-10, BULL-01, BULL-02, BULL-03, BULL-04, BULL-05, BULL-06
**Reason for deferral**: Depends on Weapon System Redesign phase.

### Future Phase: Enemy Weapon Integration
**Goal**: Enemies shoot using the same IWeapon / WeaponBase hierarchy as the player
**Deferred requirements**: WEAP-11
**Reason for deferral**: Depends on Bullet System phase.

### Future Phase: Combat Feel Polish
**Goal**: Every combat action has layered audio-visual feedback — hit feedback, camera impulse on damage
**Deferred requirements**: FEEL-01, FEEL-02
**Reason for deferral**: FEEL-01 and FEEL-02 are weapon-dependent (hit marker, impact VFX, audio on enemy hit, damage impulse). They cannot be implemented until the bullet and damage pipeline is complete.

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Input / Brain Separation + P0 Fixes | 0/5 | Not started | - |
| 2. Movement System Rebuild | 0/TBD | Not started | - |
| 3. Movement Feel Polish | 0/TBD | Not started | - |
