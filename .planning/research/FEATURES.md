# Feature Research

**Domain:** Movement shooter FPS (Titanfall 2 pilot-style / Apex Legends gunplay)
**Researched:** 2026-03-26
**Confidence:** HIGH (movement mechanics verified against multiple developer sources and player documentation; gunplay feel sourced from design analysis and official developer interviews)

---

## Feature Landscape

### Table Stakes (Users Expect These)

Features that any movement shooter must have. Missing any of these and the game immediately feels unfinished or wrong.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Responsive ground movement (no input lag) | Any FPS must have sub-frame response to WASD; delay reads as broken | LOW | Physics update must run in FixedUpdate but input must be sampled in Update — mismatching causes perceived lag |
| Jump with consistent arc | Universal FPS expectation; inconsistent height destroys platforming | LOW | Gravity scale and jump force must be tuned together; asymmetric fall gravity (faster fall-down) feels better than symmetric |
| Sprint | Every modern FPS has sprint; absence feels like missing a gear | LOW | Already exists in codebase |
| Crouch / slide | Movement shooters require crouch-to-slide as entry to chain mechanics | MEDIUM | Slide must preserve momentum from sprint; must not feel like "you just became shorter" |
| Aim-down-sights (ADS) | Every weapon in a modern FPS needs an ADS mode | MEDIUM | ADS must slightly reduce movement speed and FOV; missing this breaks weapon parity expectations |
| Reload with timing enforced | A weapon that reloads instantly or with no lockout feels like a toy | LOW | Already active in project requirements; ReloadTime from WeaponDataSO |
| Fire rate cap (rounds per minute) | Uncapped fire rate immediately reveals the weapon as not real | LOW | Already active; RoundsPerMinute timer in WeaponBase |
| Ammo count HUD | Player always needs to know mag size and reserves | LOW | Already shipped (weap_Name, stored_mag, stored_reserve HUD) |
| Hit marker / visual shot confirmation | Without feedback that shots landed, combat feels random | LOW | A simple crosshair flash or color change; see hit feedback section below |
| Enemy death reaction | An enemy that just falls over without any hit acknowledgement feels wrong | MEDIUM | Ragdoll OR death animation with some brief physics impulse |
| Basic weapon switch | Switching between at least 2 weapons is expected | LOW | WeaponInventory exists; ensure switching has a brief lockout to prevent spam |
| Wall run (initiate, hold, exit) | This is the genre-defining feature; absence makes it a generic FPS | HIGH | Must detect wall proximity, apply lateral gravity override, give speed boost on initiation |
| Double jump | Standard in all post-2012 movement shooters; expected in this genre | LOW | Jump jets / double jump is already in FSM (InAir state) |
| Grapple hook | TF2 set this as expected in the sub-genre; already in codebase | HIGH | 4 active P0 bugs must be resolved before this counts as "shipped" |
| Audio for every movement action | Silence on slide, wall-run, or land kills immersion immediately | MEDIUM | Each FSM state transition should trigger a sound; land needs impact weight |
| Weapon audio with punch | Weak gunshot sounds make every weapon feel like a toy | MEDIUM | Audio must layer: mechanical click, propellant crack, tail-off |

---

### Differentiators (Titanfall 2 / Competitive Advantage)

Features that define the TF2 pilot movement feel. These are why the game exists — they must be exceptional, not merely present.

#### Movement Differentiators

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Bunny hop / slide-hop chain | Speed preservation across hops is the defining skill ceiling of the sub-genre; what separates TF2 from Apex or CoD | HIGH | Physics: only the velocity component projected onto acceleration direction is capped — full velocity is preserved. Requires 1-frame friction window on ground contact that must be jumped within. See bunny hop physics below |
| Air strafing | Directional control mid-air without losing speed; combines mouse-turn with strafe key | MEDIUM | Mouse delta must apply lateral acceleration only when strafe key held; `air_accelerate` parameter must be tuned lower than `accelerate` to preserve existing speed |
| Wall-run speed boost on initiation | Wall runs that don't accelerate are just "running on a wall" — no incentive to seek them | MEDIUM | Speed must increase over wall-run duration, then naturally decay, creating a "sine wave" incentive to chain before peak drops |
| Wall-jump speed preservation and boost | Jumping off a wall must preserve horizontal velocity and add a small kick | MEDIUM | Additive impulse on wall-jump, not replacement; direction is based on wall normal + player look direction blend |
| Wall-to-wall chain (no repeat rule) | Players cannot re-run the same wall until grounded; forces creative routing | LOW | Boolean flag on wall surface ID or surface normal; clear on ground landing |
| Slide-to-hop (slope speed multiplier) | Sliding downhill or transitioning to jump preserves and multiplies speed | MEDIUM | Velocity at slide-jump moment transferred fully to jump arc; gravity well on slide entry; no artificial speed limit during this window |
| Momentum-preserving dash | Dash adds to existing velocity vector, not replaces it | MEDIUM | Already in architecture (`PlayerAdrenaline` momentum system exists); ensure dash direction is a velocity addend |
| Grapple swing momentum | Grapple is a physics swing, not a teleport — looking below the anchor point builds speed through the arc | HIGH | Requires spring/pendulum physics on the grapple line; jump-cancel at arc peak transfers all accumulated velocity; this is TF2's highest-skill movement tool |
| Jump buffering (input tolerance) | Pressing jump just before landing still triggers the jump; makes hop chains forgiving | LOW | Store last jump input timestamp; if timestamp is within ~100ms of next ground contact, trigger jump immediately on land |
| Coyote time (edge leniency) | Jumping just after stepping off a ledge still registers; prevents frustrating misses | LOW | Maintain a "recently grounded" timer (~80-120ms); allow jump if timer has not expired even if technically airborne |
| Always-sprinting option | Removes the "hold shift" tax; movement should feel like default fast, not fast as an opt-in | LOW | Input config; replaces sprint-hold with sprint-toggle or auto-sprint |

#### Weapon Differentiators

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Physical bullet entities (not hitscan) | Projectile bullets can be dodged in a fast movement game; creates a skill loop absent in hitscan games; enables ricochet, penetration, and visual bullet trails | MEDIUM | Already in requirements: `BulletSystem` with `ObjectPool<T>`. Projectile velocity must be high enough that close-range feels instant but long-range requires lead |
| Ricochet bullet behavior | Single mechanic that makes sniping around corners a unique skill expression; differentiates Sniper from all other weapon archetypes | MEDIUM | Ray-reflect on surface normal at impact; reduce velocity per bounce; already named as Sniper bullet behavior in requirements |
| Shell-fed shotgun reload (per-shell) | Interrupting a partial reload retains shells already loaded; enables tactical shot-counting that mag-fed weapons lack | MEDIUM | Reload loop inserts one shell per cycle, not one mag; can be interrupted and resumed |
| Multi-pellet shotgun spread | Pellet spread that changes with movement speed creates a skill: slow down to tighten spread | LOW | Spread angle fed from WeaponDataSO; scale spread multiplier with player velocity magnitude |
| IFireMode injection (semi/auto/burst) | Allows weapon personality to be changed via data, not code; future weapons are zero-code | MEDIUM | Already in requirements: `IFireMode` strategies injected via `WeaponDataSO` |
| Enemy weapons share IWeapon hierarchy | Player and enemy guns balance identically; player can pick up enemy weapon and feel its character immediately | MEDIUM | Already a key decision in PROJECT.md; design implication: tuning one weapon tunes it for both sides |
| Reload animation cancel at proper frame | Player can cancel reload when mag clicks in — before the "slap" animation completes; reduces reload dead time for skilled players | MEDIUM | Track reload state machine phases: inserting, seated, complete; allow cancel after "seated" |
| Weapon pickup / swap from world | Finding a better gun on the ground and swapping mid-fight is a core loop in the reference games | LOW | `WeaponPickup` implementing `IInteractable.Interact()` already in requirements |

#### Game Feel Differentiators

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Dynamic FOV on speed bursts | Camera FOV widening during high-speed movement is the primary perceptual signal that you are fast; without it players don't feel speed | LOW | Already exists in Cinemachine setup; tune max/min FOV deltas and smoothing speed |
| Momentum-driven damage reduction (PlayerAdrenaline) | Being fast is directly rewarded with survivability; makes movement the defense mechanic, not cover | LOW | Already shipped in codebase |
| Layered hit feedback (marker + VFX + audio + screenshake) | Each hit must be communicated across 4 channels simultaneously; any missing layer weakens the punch | MEDIUM | See hit feedback system below |
| Wall-run camera tilt | Camera rolling 8-12 degrees into the wall is the visual signal that wall-run is active; extremely cheap, extremely impactful | LOW | Already in Cinemachine setup; polish the tilt curve |
| Landing squash (camera dip) | A brief downward camera dip on landing sells the weight of the fall; absent = floaty | LOW | Cinemachine impulse or procedural camera offset on FSM Grounded state enter |
| Speed lines / directional blur HUD | Visual speed feedback on HUD (already shipped); communicates velocity without needing external reference | LOW | Already shipped |
| Slide dust / VFX | Particle burst on slide initiation; particle trail during slide | LOW | Particle system triggered on slide state enter |

---

### Anti-Features (Things That Kill Movement Shooter Feel)

These are commonly requested or easy-to-accidentally-implement features that destroy the feel of a movement shooter.

| Feature | Why Requested | Why It Kills Feel | Alternative |
|---------|---------------|-------------------|-------------|
| Hard speed cap on all movement | "Prevents exploits" / "feels fair" | Removing the speed ceiling is the entire point of TF2 movement; a hard cap means bunny hopping does nothing; players immediately feel the wall | Cap only the base ground speed; let momentum-preserving techniques accumulate freely above it |
| High air friction / drag | "Realistic" / "prevents floaty" | High air drag kills air strafing and hop chains by bleeding off accumulated speed; the whole loop collapses | Set air drag to near-zero; only apply drag to components of velocity not aligned with current acceleration |
| Symmetrical jump arc (same gravity up and down) | "Physics accuracy" | Floating at jump apex feels wrong; feels like Quake with water gravity | Apply higher gravity multiplier on descending arc (velocity.y < 0); makes jump feel snappy and controlled |
| Global screen shake on all hits | "Juice / impact" | Screen shake during a high-speed movement game causes immediate motion sickness and actively disrupts aim; Titanfall uses restrained, low-magnitude impulse | Use short-duration, low-magnitude camera impulse only on heavy hits; use hit marker and audio as primary feedback |
| Damage-interrupt on hit | "Reactive" / "feel powerful" | Staggering player movement on damage received destroys the movement loop; in TF2 you are supposed to be able to move even when taking fire | Use health chunk depletion, screen flash, and audio for hit feedback; never interrupt locomotion velocity |
| Bullet spread / bloom on moving shots | "Realism" / "skill check" | Spread punishes the movement shooter's core loop — you must always be moving; if moving makes you miss, the game penalizes its own design | Apply spread based on ADS state only, not movement state; or omit bloom entirely from non-shotgun weapons |
| Long reload lockout with no cancel | "Commitment" / "realism" | Fast gameplay requires an exit from reload; players who start a reload mid-fight need to be able to swap weapons or cancel after a minimum threshold | Allow weapon swap to interrupt reload; allow reload cancel after mag is seated phase |
| Weapon-specific wall run disable | "Prevents OP gunplay while moving" | Any restriction on what you can do during wall run signals to the player that wall running is a niche option, not the primary locomotion | Player can always fire, ADS, and reload while wall running; balance through spread/recoil increases, not lockouts |
| Hitscan-only bullets | "Networking simplicity" | Eliminates the skill expression of leading a moving target and removes the dodge-bullet interaction that high-speed movement enables; also eliminates ricochet | Physical projectiles with high velocity; close-range feels instant, long-range requires lead; network reconciliation handles latency |
| Static enemy HP bars | "Clarity" | Visual HP bars on enemies in a fast movement game add visual clutter that fight a fast-paced readout; they belong in slower tactical games | Use hit marker color (white = hit, orange = armor, red = crit/kill) + audio pitch shift to signal damage tier |
| NavMesh-only enemies in vertical spaces | "Easy AI pathing" | Enemies that can only navigate flat surfaces feel pathetic against a player who constantly uses height; they become static targets | AI must use wall-run paths or off-mesh links; enemies must be capable of vertical pursuit or have intentional design reason for ground-only movement |

---

## Feature Dependencies

```
Bunny Hop / Slide-Hop Chain
    └──requires──> Jump Buffering (forgiving input window)
    └──requires──> Near-zero air friction (velocity preservation)
    └──requires──> Slide (with speed preservation on exit)
                       └──requires──> Sprint (entry condition)
                       └──requires──> Momentum system (already exists)

Air Strafing
    └──requires──> Near-zero air friction
    └──requires──> Air acceleration parameter tuned below ground acceleration
    └──enhances──> Bunny Hop (direction control during hops)

Wall Run
    └──requires──> Wall detection system
    └──requires──> Lateral gravity override while on wall
    └──enhances──> Bunny Hop (wall-run is the primary speed-build tool)
    └──requires──> Wall-run camera tilt (otherwise indistinguishable from running near a wall)

Wall Run Chain
    └──requires──> Wall Run
    └──requires──> Wall-to-wall no-repeat rule (forces routing skill)
    └──requires──> Wall-jump speed preservation

Grapple Swing Momentum
    └──requires──> Grapple (attach + retract — bugs fixed)
    └──requires──> Spring/pendulum physics on grapple line
    └──enhances──> Bunny Hop (grapple can initiate hop chains)

Physical Bullet System
    └──requires──> ObjectPool<T> (performance)
    └──requires──> BulletBase entity with velocity, collision, lifetime
    └──enables──> Ricochet (Sniper bullet subtype)
    └──enables──> Penetration (future bullet subtype)
    └──enables──> Multi-pellet Shotgun (N pooled pellets per shot)

Reload Animation Cancel
    └──requires──> Reload state machine (inserting / seated / complete phases)
    └──requires──> Weapon switch to interrupt reload

Shell-Fed Shotgun Reload
    └──requires──> Reload state machine
    └──conflicts──> Mag-fed reload (different loop structure — must branch in WeaponBase or subclass)

Dynamic FOV
    └──requires──> Momentum value available to camera system
    └──enhances──> Speed feel (primary perceptual signal of velocity)

Layered Hit Feedback
    └──requires──> Hit marker (HUD)
    └──requires──> Impact VFX (particle system at hit point)
    └──requires──> Audio punch (hit sound at point of impact)
    └──requires──> Camera impulse (low-magnitude, short-duration screenshake)
    └──enhances──> Weapon feel (each layer multiplies perceived impact)

Enemy Weapons (IWeapon shared hierarchy)
    └──requires──> IWeapon / WeaponBase redesign complete
    └──requires──> WeaponDataSO driving all balance values
    └──enables──> Enemy weapon drops / player pickup
```

### Dependency Notes

- **Bunny hop requires near-zero air friction:** This is the single most commonly broken implementation detail. Unity's default physics applies drag (`Rigidbody.drag`) which destroys hop chains. Drag must be zero or managed manually in the movement FSM.
- **Physical bullets require ObjectPool before anything else:** Spawning `Instantiate()` per bullet at 600 RPM is a performance cliff. Pool must exist before BulletSystem is wired up.
- **Shell-fed reload conflicts with mag-fed:** These are fundamentally different loop structures. The Shotgun weapon class must branch or subclass rather than share the same reload coroutine as the Pistol/Sniper.
- **Wall-run chain rule (no-repeat) is a feel dependency, not just a rule:** Without it, players can exploit infinite wall-run on one wall. With it, level design becomes meaningful — wall placement controls pilot routing.
- **Jump buffering is a prerequisite for hop chains feeling good:** Without a 100ms input buffer, hop chains require frame-perfect timing which destroys accessibility and fun at normal game speed.

---

## MVP Definition

The vertical slice must prove movement and combat feel. Every feature below is necessary for that proof.

### Launch With (v1 — Vertical Slice)

- [ ] Bunny hop / slide-hop chain with correct physics (zero drag, velocity-projection cap) — proves movement loop
- [ ] Air strafing with directional control — proves traversal skill ceiling
- [ ] Wall run with speed boost on initiation and exit kick — proves vertical traversal loop
- [ ] Wall-to-wall chain (no-repeat rule) — required for wall run to feel intentional
- [ ] Jump buffering + coyote time — makes all three above feel good rather than frustrating
- [ ] Grapple with swing momentum (bugs fixed first) — highest-skill movement tool; vertical slice must demonstrate it
- [ ] Dynamic FOV on speed — primary perceptual signal that movement is working
- [ ] Physical bullet system with ObjectPool — required before any weapon can feel correct
- [ ] Pistol (semi-auto, mag-fed) — baseline weapon; proves IFireMode + WeaponBase architecture
- [ ] Shotgun (shell-fed, multi-pellet) — proves reload state machine and spread system
- [ ] Sniper (mag-fed, ricochet bullet) — proves bullet behavior variants in BulletSystem
- [ ] Layered hit feedback (marker + VFX + audio + camera impulse) — without this, shooting feels wrong regardless of weapon design
- [ ] Weapon pickup (IInteractable) — closes the "find weapon mid-fight" loop
- [ ] Audio: movement states (slide, land, wall-run) + weapon shots — table stakes; anything missing reads as prototype

### Add After Validation (v1.x)

- [ ] Reload animation cancel at "seated" phase — adds skill expression; defer until core reload loop is confirmed stable
- [ ] Spread increase based on ADS state — balance tuning; needs playtesting data to set correctly
- [ ] PlayerAdrenaline damage reduction tuning — exists but may need rebalancing once hop chains are properly implemented
- [ ] Enemy weapons sharing IWeapon hierarchy in AI behavior — requires AI combat to be connected; separate phase concern

### Future Consideration (v2+)

- [ ] Stim tactical (speed + regen boost) — explicitly out of scope for vertical slice per PROJECT.md
- [ ] Phase Shift — adds architectural complexity (second physics space); defer
- [ ] Tap strafing (momentum-sharp mid-air directional change via rapid forward key taps) — ultra-high-skill ceiling mechanic; add after base air strafing is shipped and confirmed
- [ ] Titan gameplay — out of scope entirely for vertical slice
- [ ] Style meter (ULTRAKILL-style) — interesting differentiator but adds UI/scoring system complexity; vertical slice does not need it to prove feel

---

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| Jump buffering + coyote time | HIGH | LOW | P1 |
| Near-zero air friction (bunny hop prerequisite) | HIGH | LOW | P1 |
| Slide-hop chain with velocity preservation | HIGH | MEDIUM | P1 |
| Air strafing | HIGH | MEDIUM | P1 |
| Wall run speed boost + exit kick | HIGH | MEDIUM | P1 |
| Grapple swing momentum (bugs fixed) | HIGH | HIGH | P1 |
| Dynamic FOV on speed | HIGH | LOW | P1 |
| Physical bullet ObjectPool | HIGH | MEDIUM | P1 |
| Layered hit feedback (4 channels) | HIGH | MEDIUM | P1 |
| Pistol (semi-auto) | HIGH | LOW | P1 |
| Shotgun (shell-fed, pellet spread) | HIGH | MEDIUM | P1 |
| Sniper (ricochet bullet) | HIGH | MEDIUM | P1 |
| Weapon pickup (IInteractable) | MEDIUM | LOW | P1 |
| Movement audio (slide, land, wall-run) | HIGH | MEDIUM | P1 |
| Wall-to-wall no-repeat rule | MEDIUM | LOW | P1 |
| Landing squash (camera dip) | MEDIUM | LOW | P2 |
| Reload cancel at seated phase | MEDIUM | MEDIUM | P2 |
| ADS spread scaling | MEDIUM | LOW | P2 |
| Always-sprinting option | LOW | LOW | P2 |
| Enemy IWeapon integration in AI | MEDIUM | MEDIUM | P2 |
| Shell-fed shotgun interrupt/resume | MEDIUM | MEDIUM | P2 |
| Stim tactical | MEDIUM | MEDIUM | P3 |
| Style meter | LOW | HIGH | P3 |
| Tap strafing | LOW | MEDIUM | P3 |

---

## Competitor Feature Analysis

| Feature | Titanfall 2 | Apex Legends | Quake / CPMA | Our Approach |
|---------|-------------|--------------|--------------|--------------|
| Bunny hop | Yes — slide-hop chains; no speed cap | Limited; slide-hop exists but capped | Yes — strafe jumping; no speed cap; requires forward+strafe sync | Unrestricted; TF2-style; velocity projection cap only |
| Air strafing | Yes — mouse-turn + strafe key | Limited | Yes — CPMA air control; forward key also works | TF2-style; mouse delta + strafe key; no forward-key air accel |
| Wall run | Yes — core feature, speed builds over time | No (vaulting only) | No | TF2-style; lateral gravity override; speed increase over duration |
| Grapple | Yes — Grapple tactical; swing + momentum | Yes — Pathfinder passive, not player-universal | No | TF2-style; momentum swing; player-universal |
| Bullet type | Mix: some hitscan, some projectile | Mix: mostly hitscan | Mix: plasma = projectile, rail = hitscan | Physical projectiles for all weapons; high velocity for close-range feel |
| Recoil system | Per-weapon recoil patterns | Deterministic patterns (learnable) | Low recoil, raw aim | Deterministic patterns via WeaponDataSO; learnable, not random |
| Reload cancel | Yes — cancel on mag seat | Yes | Not applicable (fast TTK, less relevant) | Reload state machine with cancel threshold |
| FOV scaling | Yes — increases on high speed | Limited | No native, but FOV is fixed high | Dynamic FOV driven by momentum magnitude |
| Hit feedback | Hit marker + flash | Hit marker + color tiers (white/orange/red) + audio | Screen flash | 4-layer: marker + VFX + audio + camera impulse |
| Style system | No | No | No | Not in vertical slice; future consideration |

---

## Hit Feedback System Detail

This merits explicit documentation because every layer must be present for the feedback to feel correct.

**Layer 1 — Hit Marker (HUD)**
Crosshair briefly changes color or flashes when a projectile connects. White = regular hit. Red = kill. Audio-only variant for headshots acceptable. This is the fastest feedback signal — appears instantly on hit registration.

**Layer 2 — Impact VFX (World Space)**
Particle burst at bullet impact point. Must include: bullet hole decal, debris particles (material-specific if possible), and muzzle flash on firing. VFX lifetime should be short (0.3-0.5s) to avoid visual clutter in fast movement.

**Layer 3 — Audio Punch**
Impact sound at hit point. Two sounds needed: muzzle crack (at weapon) and impact thud (at target). If the target is armored, a different material sound (metallic clank vs flesh thud) conveys armor state without UI.

**Layer 4 — Camera Impulse**
Not screen shake. A single low-magnitude (0.03-0.08 units) Cinemachine impulse lasting 0.1-0.15s on receiving damage from enemy hits. Do NOT apply this to outgoing hits — only to incoming damage. Applying it to outgoing hits causes motion sickness during rapid fire.

**Anti-pattern:** Screen shake on every outgoing bullet. This is cited by multiple sources as a motion-sickness trigger in fast movement games and actively disrupts aim. Use camera impulse on incoming hits only; use animation recoil (weapon model kick-back) for outgoing shots.

---

## Bunny Hop Physics Implementation Notes

This is the most commonly broken feature in Unity movement shooter implementations and is worth documenting the correct physics model.

**The rule (from Quake III source):** When adding acceleration, only check the component of current velocity projected onto the desired acceleration direction. If `dot(velocity, wishDir) + accelerationMagnitude > maxSpeed`, reduce the acceleration but do not touch the total velocity vector.

**Why this preserves speed:** Air strafing works by keeping `wishDir` perpendicular to current velocity. The projected component is near zero, so full acceleration is always added. This continuously nudges velocity direction toward wish direction without ever hitting the speed clamp.

**The ground friction window:** There is a 1-frame window when hitting the ground where friction has not yet been applied. Jumping within this window preserves all air velocity. In Unity: detect ground contact in `OnCollisionEnter`; apply friction in `FixedUpdate`; jump input sampled in `Update`. The ordering ensures the jump-before-friction window exists naturally. Jump buffering (100ms window) makes this accessible rather than frame-perfect.

**Key tuning parameters (expose all via SO):**
- `groundAccelerate` — how fast ground movement accelerates
- `airAccelerate` — how fast air movement accelerates (lower = harder air strafing; higher = more floaty control)
- `maxGroundSpeed` — the projection cap for ground movement
- `friction` — ground drag coefficient (Quake range: 1-5; set to near-zero for hop chains to work)

---

## Sources

- [Titanfall 2: How Design Informs Speed — Medium](https://medium.com/@abhishekiyer_25378/titanfall-2-how-design-informs-speed-f14998d7f470) — MEDIUM confidence (analysis article, not official)
- [Designer Interview: Getting Titanfall's Controls Just Right — Game Developer](https://www.gamedeveloper.com/design/designer-interview-getting-i-titanfall-i-s-controls-just-right) — HIGH confidence (official developer interview)
- [Bunnyhopping from the Programmer's Perspective — adrianb.io](https://adrianb.io/2015/02/14/bunnyhop.html) — HIGH confidence (technical implementation analysis, matches Quake III source)
- [Steam Guide: Titanfall 2 Advanced Movement](https://steamcommunity.com/sharedfiles/filedetails/?id=2141300408) — MEDIUM confidence (community documentation)
- [Slide Hop — Titanfall Wiki](https://titanfall.fandom.com/wiki/Slide_Hop) — MEDIUM confidence (community wiki, matches in-game behavior)
- [Titanfall 2 Pilot Tacticals — Official Wiki](https://titanfall2.fandom.com/wiki/Pilot_Tacticals) — MEDIUM confidence (community wiki)
- [ULTRAKILL Style System — ultrakillgame.com](https://ultrakillgame.com/style/) — HIGH confidence (official site)
- [ULTRAKILL Parry — Official Wiki](https://ultrakill.wiki.gg/wiki/Parrying) — MEDIUM confidence (community wiki)
- [A UX Analysis of FPS Damage Indicators — Medium](https://medium.com/@jasper.stephenson/a-ux-analysis-of-first-person-shooter-damage-indicators-59ac9d41caf8) — MEDIUM confidence (design analysis)
- [Squeezing More Juice Out of Your Game Design — Game Developer](https://www.gamedeveloper.com/design/squeezing-more-juice-out-of-your-game-design-) — HIGH confidence (industry publication)
- [Hitscan vs Projectile — Aiming.Pro](https://aiming.pro/hit-scan-projectiles-fps) — MEDIUM confidence (analysis article)
- [Apex Legends Recoil Patterns — Dexerto](https://www.dexerto.com/apex-legends/updated-recoil-patterns-for-all-weapons-in-apex-legends-1354210/) — HIGH confidence (verified against game)

---

*Feature research for: movement shooter FPS (Titanfall 2 pilot / Apex gunplay)*
*Researched: 2026-03-26*
