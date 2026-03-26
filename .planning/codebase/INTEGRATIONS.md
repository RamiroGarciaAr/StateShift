# External Integrations

**Analysis Date:** 2026-03-26

## Third-Party Asset Store Packages

All third-party assets are stored under `Assets/Thirdparty/`.

**Graphy - Ultimate Stats Monitor v3.0.5:**
- Publisher: Martin Pane (Tayx)
- Purpose: Runtime FPS, RAM, audio, and advanced stats overlay for development/debug builds
- Location: `Assets/Thirdparty/Graphy - Ultimate Stats Monitor/`
- Assembly definitions: `Tayx.Graphy.asmdef`, `Tayx.Graphy.Editor.asmdef`
- License: MIT
- Usage: Drag-and-drop prefab from `Assets/Thirdparty/Graphy - Ultimate Stats Monitor/Prefab/`; no custom script integration detected in `Assets/Scripts/`

**Gaskellgames FolderSystem + GgCore:**
- Publisher: Gaskellgames
- Purpose: Editor-only hierarchy folder organization tool (cosmetic folders in Unity's Hierarchy panel)
- Location: `Assets/Thirdparty/Gaskellgames/`
- Assembly definitions: `Gaskellgames.FolderSystem.asmdef`, `Gaskellgames.asmdef`
- Usage: Editor utility only — no runtime impact

**Ciathyza Gridbox Prototype Materials:**
- Publisher: Ciathyza
- Purpose: Prototype/greybox materials with grid patterns for level blockout
- Location: `Assets/Thirdparty/Ciathyza/Gridbox Prototype Materials/`
- Usage: Applied to ProBuilder geometry in test/prototype scenes

**Synty POLYGON Sci-Fi City:**
- Publisher: Synty Studios
- Purpose: Low-poly sci-fi city 3D art pack — buildings, props, characters, vehicles, environments, weapons
- Location: `Assets/Thirdparty/PolygonSciFiCity/`
- Contents: Models, materials, prefabs, textures, FX, scenes
- License: Synty commercial license (asset store)

**Synty POLYGON Sci-Fi Space:**
- Publisher: Synty Studios
- Purpose: Low-poly sci-fi space 3D art pack — buildings, props, characters, vehicles, environments, weapons
- Location: `Assets/Thirdparty/PolygonSciFiSpace/`
- Contents: Models, materials, prefabs, textures, FX, scenes (includes VR variant)
- License: Synty commercial license (asset store)

**TextMesh Pro (in Thirdparty):**
- Location: `Assets/Thirdparty/TextMesh Pro/`
- Note: This is a legacy copy of TMP assets. The active TMP package is `com.unity.textmeshpro` v3.0.9 from the Package Manager. The `Thirdparty` copy contains fonts, sprite assets, and style sheets used at runtime via `Assets/Thirdparty/TextMesh Pro/Resources/`.

## Unity Package Manager Integrations

**ParticleEffectForUGUI (`com.coffee.ui-particle`):**
- Source: GitHub (`https://github.com/mob-sakai/ParticleEffectForUGUI.git#4.11.3`)
- Purpose: Enables `ParticleSystem` rendering inside UI Canvas (used by `SpeedLinesController.cs`)
- Not from Unity Asset Store — direct git dependency in `Packages/manifest.json`

## Custom Shaders

**CrosshairShader:**
- File: `Assets/Scripts/Shaders/UI/CrosshairShader.shader`
- Type: Built-in pipeline ShaderLab / HLSL
- Purpose: Procedural crosshair with configurable inner/outer radius, dash count, outline, and transparency
- Material: `Assets/Scripts/Shaders/UI/Custom_CrosshairShader.mat`

## Audio

**AudioManager:**
- Implementation: `Assets/Scripts/Managers/AudioManager.cs`
- Approach: Custom lightweight manager. Array of `Sound` structs wired at Awake. Played by name string lookup.
- No third-party audio middleware (no FMOD, no Wwise)
- Audio assets: `Assets/Audio/SFX/`

## Analytics & Services

**Unity Analytics:** Module `com.unity.modules.unityanalytics` is present in manifest but no analytics calls detected in scripts. Not actively used.

**Version Control:** `com.unity.collab-proxy` v2.9.1 (Unity Version Control / Plastic SCM). Git is also used (repo confirmed on `feat/combat` branch).

## Networking & External APIs

- No networking packages detected
- No external API integrations (no REST clients, no cloud save, no leaderboards)
- No authentication services
- No analytics SDK calls found in `Assets/Scripts/`

## Save / Persistence

- No save system detected — no `PlayerPrefs`, `JsonUtility`, or file I/O calls found in `Assets/Scripts/`
- All runtime configuration flows through ScriptableObject assets in `Assets/Data/`

## CI/CD & Build

- No CI configuration files detected (no `.github/workflows/`, no `Jenkinsfile`, no `azure-pipelines.yml`)
- Build is manual from Unity Editor

---

*Integration audit: 2026-03-26*
