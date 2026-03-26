# Technology Stack

**Project:** StateShift
**Milestone:** Movement + Weapon system rebuild, URP migration
**Researched:** 2026-03-26
**Confidence:** MEDIUM — Unity package versions verified via official docs; physics architecture from community consensus + reference implementations; Cinemachine 3 migration risk verified against official changelog.

---

## Engine Constraint

Unity 2022.3 LTS (currently 2022.3.62f2). No engine upgrade this milestone. All package versions below are the correct major version series for 2022.3 (SRP core is locked to the 14.x.x series in this Unity version).

---

## Recommended Stack

### Render Pipeline

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Universal Render Pipeline | `com.unity.render-pipelines.universal` 14.x (bundled with 2022.3) | Replace Built-in pipeline | Shader Graph support, URP Volume post-processing, SRP Batcher, modern VFX Graph compatibility. Built-in pipeline has no upgrade path for Shader Graph and blocks visual fidelity ceiling. |
| Shader Graph | `com.unity.shadergraph` 14.x (bundled with URP) | Material authoring after migration | Auto-included with URP. Use for any new materials. Crosshair shader must be manually rewritten. |

**Do not** install the legacy `com.unity.postprocessing` (PPv2) alongside URP — they are architecturally incompatible. URP has its own Volume-based post-processing baked in. PPv2 volumes are silently ignored at runtime under URP.

---

### Input

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Unity Input System | `com.unity.inputsystem` 1.14.2 (already installed) | All player input | Already integrated. v1.14.x is the current stable series for 2022.3 LTS. Do not downgrade. |

**Update mode:** Set to **Dynamic Update** (Process Events in Dynamic Update) in the Input System settings asset. This is the lowest-latency configuration. Reading input in `Update()` under Dynamic Update gives you the latest values at the start of every frame. Do not use Fixed Update mode for this game — it introduces a full fixed timestep of lag on every frame where the physics and render rates diverge.

**Discrete events pattern:** Jump, dash, grapple are discrete `WasPressedThisFrame` checks. These must be read in `Update()` and passed as boolean flags into `FixedUpdate()` via the `PlayerMovementContext`. Reading `WasPressedThisFrame` inside `FixedUpdate()` when the Input System is in Dynamic Update mode causes missed presses. The existing architecture already uses this flag-passing pattern via `PlayerMovementContext.WantsToJump` etc. — keep it.

**Camera-relative direction:** `Camera.main` must be cached once in `Awake()`. The current `PlayerController` calls it every `Update` frame. This is a known regression in the codebase that must be fixed as part of the `PlayerMovementBrain` extraction.

---

### Physics / Movement Architecture

| Technology | Configuration | Why |
|------------|---------------|-----|
| Unity 3D Physics (Rigidbody) | Keep — already in use | Momentum-preserving movement (bunny hop, slide-hop, grapple physics) requires velocity manipulation. `Rigidbody.velocity` is set directly; `AddForce` is only used for impulse cases. CharacterController does not expose velocity as a mutable vector and cannot implement Quake/TF2-style air acceleration without fighting the abstraction. |
| Fixed Timestep | **0.01666f (60 Hz)** | Default is 0.02 (50 Hz). At 60 Hz physics, velocity changes applied in `FixedUpdate()` resolve once per rendered frame at 60 FPS, eliminating the perceptible "stuttery" gap between physics and visual state. Double the CPU cost of 50 Hz — acceptable for a PC-only singleplayer game. Set in Project Settings > Time. |

**Rigidbody configuration for movement:**
- `useGravity = false` — gravity applied manually in `FixedUpdate()` for custom fall curves and coyote time control
- `interpolation = Rigidbody.Interpolation.Interpolate` — visual smoothing between physics steps; critical at 60 Hz physics / 144 Hz render
- `collisionDetection = CollisionDetectionMode.Continuous` — prevents tunneling at high momentum values
- `constraints` — freeze rotation on all axes; handle rotation via transform directly in camera/look code
- `drag = 0` — apply custom drag per-state rather than global Rigidbody drag; global drag fights momentum-preserving mechanics

**Quake/TF2 air strafing implementation pattern (for bunny hop):**

The standard approach used by every Quake-derivative Unity port (CPMPlayer.cs reference: github.com/WiggleWizard/quake3-movement-unity3d) is:
1. Compute `wishdir` — the normalized desired movement direction from input
2. Compute `wishspeed` — `wishdir.magnitude * moveSpeed`
3. In air: cap `currentspeed = dot(velocity, wishdir)`, only add acceleration when `currentspeed < wishspeed`
4. Never zero out horizontal velocity on landing — carry it forward
5. Apply friction **only** on ground, and **skip friction** on the frame the player jumps (this is the bhop preservation mechanic)

The existing `PlayerMovement` + `PlayerMomentum` system already approximates this but implements it through a momentum scalar rather than direct velocity projection. The audit phase will determine whether to refactor toward the velocity-projection model or tune the existing scalar approach to the same feel.

**Do not use CharacterController** for this project. CharacterController wraps velocity inside the component and prevents the direct velocity manipulation required for air strafing. All Quake-movement reference implementations that achieve authentic feel use Rigidbody or a custom kinematic solution.

---

### Camera

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Cinemachine | `com.unity.cinemachine` **2.10.x** (keep current — do not upgrade to 3.x) | FPS camera, FOV dynamics, wall-run tilt | Cinemachine 3 is a breaking API change with no automated code migration path. The project has custom scripting against Cinemachine 2 API in `CameraController.cs`, `DynamicFOV.cs`, and `WallRunCamaraEffects.cs`. Upgrading to CM3 this milestone adds migration risk with zero functional benefit for the current feature set. |

**What Cinemachine 2 handles well for this project:**
- `CinemachineFreeLook` or `CinemachineVirtualCamera` with `Aim: Composer` for mouse look
- `CinemachineBasicMultiChannelPerlin` for procedural camera shake (hit feedback, landing impact)
- FOV manipulation via `m_Lens.FieldOfView` driven by momentum — already implemented in `DynamicFOV.cs`
- Camera roll via `Dutch` property — already used in `WallRunCamaraEffects.cs`

**Camera shake for hit feedback:** Use `CinemachineImpulseSource` + `CinemachineImpulseListener` (both available in CM 2.x). This is the correct CM-native way to trigger screen shake from weapon fire and impacts. Do not implement shake via manual camera position offset — it fights the Cinemachine system.

**Cinemachine 3 deferral rationale (confidence: HIGH):** The official CM3 upgrade guide states "there is currently no automated way to migrate code" and "the Unity API updater can't take care of all the API changes." Given the existing custom camera code, this is a non-trivial migration. No CM3 feature is required by the current milestone scope.

---

### Object Pooling

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| `UnityEngine.Pool.ObjectPool<T>` | Built into Unity 2021+ (available in 2022.3) | Bullet pooling, VFX pooling | Already in the engine — no package install required. Type-safe generic API. Handles create/get/release/destroy callbacks cleanly. The project's design spec requires `ObjectPool<T>` with generics; this is the exact interface. |

**API pattern:**

```csharp
private ObjectPool<BulletBase> _pool = new ObjectPool<BulletBase>(
    createFunc:      () => Instantiate(_bulletPrefab),
    actionOnGet:     b => b.gameObject.SetActive(true),
    actionOnRelease: b => b.gameObject.SetActive(false),
    actionOnDestroy: b => Destroy(b.gameObject),
    collectionCheck: false,   // disable in release builds — no GC overhead
    defaultCapacity: 20,
    maxSize:         100
);
```

**Do not** wrap `UnityEngine.Pool` in another generic layer. The project spec calls for `ObjectPool<T>` — that is exactly what `UnityEngine.Pool.ObjectPool<T>` is. Writing a custom pool on top is unnecessary abstraction.

**Important editor caveat:** `GenericPool<T>` (the static variant) uses static variables that persist across Play Mode sessions when Domain Reload is disabled. Use the instance-based `ObjectPool<T>` (shown above) to avoid stale pooled objects between editor runs.

---

### UI

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| TextMesh Pro | `com.unity.textmeshpro` 3.0.9 (keep) | All in-game text | No reason to change. TMP works identically under URP. |
| Unity uGUI | `com.unity.ugui` 1.0.0 (keep) | HUD, menus | Render pipeline-agnostic. No changes required on URP migration. |
| ParticleEffectForUGUI | `com.coffee.ui-particle` 4.11.3 (verify post-URP) | Speed lines HUD | This package renders particles in UI space via custom render passes. After URP migration, verify the package's URP compatibility. v4.11.x has documented URP support but must be confirmed in-editor after pipeline switch. |

---

### AI / Navigation

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Unity AI Navigation | `com.unity.ai.navigation` 1.1.7 (keep) | NavMesh enemy pathfinding | No changes required. Navigation is render-pipeline-agnostic. |

---

### Audio

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Unity Audio (built-in) | Built-in (keep) | Movement and weapon sounds | `AudioManager` is currently a placeholder with no event wiring. For this milestone it needs to become a proper singleton using the `Singleton<T>` base class and receive events from `EventsManager`. No external audio middleware (FMOD, Wwise) is needed for a vertical slice. |

**Do not adopt FMOD or Wwise this milestone.** They add integration complexity and require per-platform native plugins. Unity's built-in `AudioMixer` with `AudioMixerGroup` routing (SFX, Music, UI) is sufficient for the vertical slice feature set.

---

## URP Migration: Full Breakdown

### What the Render Pipeline Converter automates (confidence: HIGH)

Accessed via `Window > Rendering > Render Pipeline Converter`, selecting `Built-In to URP`:

- Standard material → `Universal Render Pipeline/Lit`
- Mobile/Legacy materials → `Universal Render Pipeline/Simple Lit`
- Particle materials → `Universal Render Pipeline/Particles/Lit`
- PostProcessing Stack v2 volumes/profiles/layers → URP Volume components
- Rendering settings from Built-in → URP Pipeline Asset equivalents
- Animation clips referencing converted materials

### What you must do manually

| Item | Action Required |
|------|-----------------|
| `CrosshairShader.shader` (ShaderLab/HLSL) | Full manual rewrite. Replace `CGPROGRAM`/`ENDCG` with `HLSLPROGRAM`/`ENDHLSL`. Change includes to `Core.hlsl`. Add `"RenderPipeline" = "UniversalPipeline"` tag. Wrap properties in `CBUFFER_START(UnityPerMaterial)`. Replace `sampler2D` with `TEXTURE2D()`/`SAMPLER()` macros. Replace `tex2D()` with `SAMPLE_TEXTURE2D()`. Replace `fixed4` with `half4`. |
| Surface Shaders | URP has no Surface Shader support. Must be rewritten as custom HLSL or recreated in Shader Graph. Check if any `.shader` files use `#pragma surface`. |
| `com.unity.postprocessing` package | Remove after migration. URP Volume replaces it entirely. PPv2 is silently incompatible — it does not error, it simply does nothing under URP. |
| Camera components | Remove legacy `PostProcessLayer` component from the Camera GameObject. Add `Volume` component to a global Volume GameObject. Configure Volume Profile with URP override effects (Bloom, Color Grading, Vignette, Depth of Field). |
| Lighting | URP uses its own lighting model. Baked lightmaps transfer but real-time light settings may need re-tuning. Check `Rendering > Lighting` settings after migration. |

### Post-migration URP Pipeline Asset configuration (for PC/FPS)

Recommended settings in the URP Pipeline Asset after migration:

```
Rendering:
  Rendering Path:        Forward   (Forward+ only needed for many dynamic lights — not this game)
  Depth Texture:         Enabled   (required for soft particles, depth effects)
  Opaque Texture:        Disabled  (not needed — no screen-space refraction)

Shadows:
  Max Distance:          50-100    (tune to level scale; smaller = better GPU perf)
  Cascade Count:         2         (4 cascades is overkill for small FPS levels)

Post Processing:
  Grading Mode:          High Dynamic Range

Quality:
  Anti Aliasing (MSAA):  4x        (native MSAA for PC standalone target)
```

---

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| Render Pipeline | URP 14 | HDRP | HDRP targets high-end visuals with significant configuration overhead. URP is the correct choice for a fast-paced movement game where performance matters more than ray-traced lighting. |
| Physics controller | Rigidbody + custom velocity | CharacterController | CharacterController hides velocity, preventing Quake-style air acceleration. Every credible Quake/TF2 movement implementation uses Rigidbody or kinematic. |
| Physics controller | Rigidbody + custom velocity | Kinematic Rigidbody | Kinematic requires fully manual collision resolution via `Physics.ComputePenetration`. More control but more implementation work. Standard dynamic Rigidbody with locked rotation is sufficient and integrates with existing collision callbacks. |
| Object Pooling | `UnityEngine.Pool.ObjectPool<T>` | Custom `ObjectPool<T>` | Built-in is type-safe, already ships with the engine, and matches the spec exactly. No reason to write a custom implementation. |
| Camera | Cinemachine 2.10.x | Cinemachine 3.1.x | Breaking API change with no code migration tool. Three existing custom scripts would require manual rewrite with no functional benefit this milestone. |
| Input mode | Dynamic Update | Fixed Update | Dynamic Update is explicitly documented as lower latency. Fixed Update input mode introduces lag equal to one fixed timestep when the render rate exceeds the physics rate. |
| Audio | Unity built-in AudioMixer | FMOD / Wwise | Middleware adds integration scope, native plugin management, and platform licensing overhead. Not justified for a vertical slice. |

---

## Installation — Packages to Add/Remove at Migration

```bash
# Add via Package Manager
com.unity.render-pipelines.universal   (14.x — install via Package Manager > Unity Registry)

# Remove after URP migration confirms stable
com.unity.postprocessing               (3.4.0 — incompatible with URP, remove to avoid confusion)

# Keep — no changes
com.unity.inputsystem                  (1.14.2)
com.unity.cinemachine                  (2.10.5)
com.unity.textmeshpro                  (3.0.9)
com.unity.ai.navigation                (1.1.7)
com.unity.probuilder                   (5.2.4)

# Verify post-migration
com.coffee.ui-particle                 (4.11.3 — confirm URP render pass compatibility)
```

---

## Sources

- Unity URP 14 Official Docs: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/upgrading-your-shaders.html
- URP Custom Shader Upgrade Guide: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/urp-shaders/birp-urp-custom-shader-upgrade-guide.html
- URP Performance Configuration: https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@14.0/manual/configure-for-better-performance.html
- Cinemachine 3 Upgrade Guide (why not to migrate): https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineUpgradeFrom2.html
- Input System Timing & Latency: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/timing-and-latency.html
- Input System Mixed Timing: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/timing-mixed-scenarios.html
- UnityEngine.Pool API: https://docs.unity3d.com/2021.1/Documentation/ScriptReference/Pool.ObjectPool_1.html
- Quake3 CPM movement reference (Rigidbody vs CC decision): https://github.com/WiggleWizard/quake3-movement-unity3d
- Unity Object Pooling Guide: https://unity.com/how-to/use-object-pooling-boost-performance-c-scripts-unity
