# Technology Stack

**Analysis Date:** 2026-03-26

## Languages

**Primary:**
- C# — All gameplay, AI, UI, and system scripts (`Assets/Scripts/`, 105 `.cs` files)
- ShaderLab / HLSL — Custom crosshair shader (`Assets/Scripts/Shaders/UI/CrosshairShader.shader`)

## Runtime

**Environment:**
- Unity 2022.3.62f2 LTS (Long Term Support)
- Revision: 7670c08855a9

**Target Platform:**
- Windows PC Standalone (primary build target based on project structure and feature set)
- Default resolution: 1920x1080

## Frameworks

**Core Engine:**
- Unity 2022.3 LTS — MonoBehaviour-based architecture with custom singleton, FSM, and event layers

**Input:**
- Unity Input System `com.unity.inputsystem` v1.14.2
- Input map: `Assets/InputActions/PlayerMap.inputactions`
- Actions: Movement, Jump, Sprint, Crouch, Aim, Shoot, Reload, Pause, Interact, Dash, Grapple, ChangeWeapon

**Camera:**
- Cinemachine `com.unity.cinemachine` v2.10.5
- Used in: `Assets/Scripts/Player/Camera/CameraController.cs`, `DynamicFOV.cs`, `WallRunCamaraEffects.cs`

**UI:**
- Unity uGUI `com.unity.ugui` v1.0.0 — menus, HUD base layout
- TextMesh Pro `com.unity.textmeshpro` v3.0.9 — all in-game text rendering
- ParticleEffectForUGUI `com.coffee.ui-particle` v4.11.3 (git) — UI particle effects (speed lines: `Assets/Scripts/UI/HUD/SpeedLinesController.cs`)

**AI / Navigation:**
- Unity AI Navigation `com.unity.ai.navigation` v1.1.7 — NavMesh baking and runtime
- NavMeshAgent used in: `Assets/Scripts/AI/TestEnemy.cs`, `Assets/Scripts/Testing/NavMeshTester.cs`

**Rendering:**
- Render Pipeline: Built-in (no URP/HDRP — confirmed by `GraphicsSettings.asset` using legacy deferred/shadow shaders)
- Post Processing Stack `com.unity.postprocessing` v3.4.0 — imported but no script-level usage detected; likely applied via volume components in scenes

**Physics:**
- Unity Physics (3D) — `com.unity.modules.physics` — used across 23 scripts (Rigidbody, CharacterController, LayerMask, Raycast, SpringJoint)

**Animation:**
- Unity Animation `com.unity.modules.animation` — Animator referenced in: `WeaponDataSO.cs`, `UIAnimationManager.cs`, `DynamicFOV.cs`, `GrappleRope.cs`, `PlayerDash.cs`, `CrosshairBloomController.cs`

**Audio:**
- Unity Audio `com.unity.modules.audio` — custom `AudioManager` at `Assets/Scripts/Managers/AudioManager.cs` using `AudioSource` / `AudioClip` / `AudioMixer`

**Build / Level Design:**
- ProBuilder `com.unity.probuilder` v5.2.4 — geometry authoring tool, data in `Assets/Thirdparty/ProBuilder Data/`

**Timeline:**
- Unity Timeline `com.unity.timeline` v1.7.7 — package present, no script usage detected; likely used in scene animations

**Vector Graphics:**
- Unity Vector Graphics `com.unity.vectorgraphics` v2.0.0-preview.25 — imported, no direct script usage detected

**Visual Scripting:**
- Unity Visual Scripting `com.unity.visualscripting` v1.9.4 — namespace imported in `GroundChecker.cs` and `PlayerMovement.cs` (likely for `TypeOptions` attribute only, not for graph-based logic)

**Collaboration:**
- Unity Version Control (Collab Proxy) `com.unity.collab-proxy` v2.9.1

**Development Tools:**
- Unity Feature Development `com.unity.feature.development` v1.0.1 (meta-package)

## Key Dependencies

**Critical:**
- `com.unity.inputsystem` v1.14.2 — all player input, cannot remove
- `com.unity.cinemachine` v2.10.5 — camera system, cannot remove
- `com.unity.textmeshpro` v3.0.9 — all text rendering in UI
- `com.unity.ai.navigation` v1.1.7 — enemy pathfinding

**Infrastructure:**
- `com.unity.postprocessing` v3.4.0 — visual effects pipeline
- `com.coffee.ui-particle` v4.11.3 — UI particle effects (speed lines HUD)
- `com.unity.probuilder` v5.2.4 — scene geometry

## Data Configuration

**ScriptableObjects (runtime data):**
- `Assets/Scripts/Data_Scripts/Combat/WeaponDataSO.cs` — weapon stats
- `Assets/Scripts/Data_Scripts/Health/DamageMatrixSO.cs` — damage type multipliers
- `Assets/Scripts/Data_Scripts/Health/EnemyHealthConfigSO.cs` — enemy health config
- `Assets/Scripts/Data_Scripts/Health/PlayerHealthConfigSO.cs` — player health config
- Instantiated assets stored in `Assets/Data/`

**Scenes:**
- `Assets/Scenes/LoadingScene.unity`
- `Assets/Scenes/MainMenuScene.unity`
- `Assets/Scenes/Testing/EnemyTestScene.unity`
- `Assets/Scenes/Testing/PlayerTesting.unity`
- `Assets/Scenes/Testing/ShootingRange.unity`

## Platform Requirements

**Development:**
- Unity 2022.3.62f2 LTS
- Windows 11 (current dev environment)
- No .NET version file present — uses Unity's embedded Mono / IL2CPP

**Production:**
- Target: Windows PC Standalone
- Resolution default: 1920x1080
- No mobile, VR, or WebGL modules actively configured

---

*Stack analysis: 2026-03-26*
