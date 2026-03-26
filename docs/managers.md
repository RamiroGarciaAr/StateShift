# Managers & Core

## Singleton\<T\>

**File:** `Assets/Scripts/Core/Singleton/Singleton.cs`

Generic MonoBehaviour singleton base. All global managers inherit from this.

```csharp
public class MyManager : Singleton<MyManager> { }

// Access from anywhere:
MyManager.Instance.DoSomething();
```

### Behavior

| Scenario | Result |
|----------|--------|
| First instance `Awake` | Assigns `Instance`; optionally calls `DontDestroyOnLoad` |
| Duplicate instance `Awake` | `Destroy(gameObject)` — logs a warning |
| Instance destroyed | `Instance` cleared to null |

### Inspector Field

| Field | Description |
|-------|-------------|
| `_persistAcrossScenes` | If `true`, calls `DontDestroyOnLoad` on the GameObject |

---

## EventsManager

**File:** `Assets/Scripts/Managers/EventsManager.cs`
Extends: `Singleton<EventsManager>`

Central event bus for game-state broadcasts. Other systems subscribe to these events rather than calling each other directly.

### Events

| Event | Signature | Meaning |
|-------|-----------|---------|
| `OnGamePause` | `Action<bool>` | `true` = paused, `false` = resumed |
| `OnGameOver` | `Action` | Player died |
| `OnGameExit` | `Action` | Exit to main menu |
| `OnGameRestart` | `Action` | Restart current level |

### Usage Pattern

```csharp
// Subscribe
EventsManager.Instance.OnGamePause += HandlePause;

// Invoke
EventsManager.Instance.ActionGamePause(true);

// Unsubscribe (important — do this in OnDisable/OnDestroy)
EventsManager.Instance.OnGamePause -= HandlePause;
```

### Invoke Methods

| Method | Fires |
|--------|-------|
| `ActionGamePause(bool isPaused)` | `OnGamePause` |
| `ActionGameOver()` | `OnGameOver` |
| `ActionGameExit()` | `OnGameExit` |
| `ActionGameRestart()` | `OnGameRestart` |

---

## GameManager

**File:** `Assets/Scripts/Managers/GameManager.cs`
Extends: `Singleton<GameManager>`

Reacts to `EventsManager` events to implement game-state side effects.

### Event → Action Mapping

| Event | Action |
|-------|--------|
| `OnGamePause(true)` | `Time.timeScale = 0` |
| `OnGamePause(false)` | `Time.timeScale = 1` |
| `OnGameOver` | `Time.timeScale = 0`; unsubscribes from all events |
| `OnGameExit` | Calls `SceneLoader.OpenLoadingScene("MainMenuScene")` |
| `OnGameRestart` | Calls `SceneLoader.OpenLoadingScene("SampleScene")` |

> After `OnGameOver`, `GameManager` unsubscribes from all events. To restart from a game-over state, the restart flow must go through a new scene load.

---

## SceneLoader

**File:** `Assets/Scripts/Managers/SceneLoader.cs`
Static class (no MonoBehaviour).

Handles scene transitions with an intermediate loading screen.

### Three-Step Flow

```
1. SceneLoader.OpenLoadingScene("TargetSceneName")
   ├─ Saves scene name to static _nextScene
   └─ Loads "LoadingScene" additively or directly

2. LoadingScene starts → LoadingManager.Update() shows progress bar
   └─ Calls SceneLoader.LoadNextAsync()
        └─ AsyncOperation loads _nextScene

3. On load complete → SceneLoader.CloseLoadingScene()
   └─ Unloads "LoadingScene"
```

### Methods

| Method | Description |
|--------|-------------|
| `OpenLoadingScene(string nextSceneName)` | Begin transition; saves target scene name |
| `LoadNextAsync()` | Start async load of saved scene (call from LoadingScene) |
| `CloseLoadingScene()` | Unload loading scene after target is ready |

---

## LoadingManager

**File:** `Assets/Scripts/Managers/LoadingManager.cs`

Attached to a GameObject in the loading scene. Updates a progress bar UI while `SceneLoader.LoadNextAsync()` is running.

| Inspector Field | Type | Description |
|----------------|------|-------------|
| `_progressBar` | `Slider` | UI slider displaying load progress |
| `_progressText` | `TextMeshProUGUI` | Percentage text label |

---

## AudioManager

**File:** `Assets/Scripts/Managers/AudioManager.cs`

Simple array-based audio manager. Not a singleton.

### Setup

Add a `Sound[]` array in the Inspector. Each entry:

| Field | Description |
|-------|-------------|
| `Name` | String key used to play the sound |
| `clip` | `AudioClip` |
| `loop` | Whether to loop |
| `volume` | 0–1 |
| `pitch` | 0.1–3 |

`Awake` creates an `AudioSource` component for each sound and configures it.

### Usage

```csharp
audioManager.Play("Flight");    // play by name
```

`Start` automatically plays the sound named `"Flight"`.

> This is a minimal implementation suitable for small projects. It does not support 3D spatial audio, pooling, or dynamic mixing.

---

## UIAnimationManager

**File:** `Assets/Scripts/Managers/UIAnimationManager.cs`

Drives a UI `Animator` via trigger strings, typically for menu/overlay state machines.

### Inspector Fields

| Field | Description |
|-------|-------------|
| `animator` | The Animator to control |
| `triggerMappings` | List of `TriggerMapping` (button → trigger name pairs) |
| `escTriggerName` | Trigger to fire on Escape key |
| `isEscAllowed` | Whether Escape input is active |
| `buttonsToReset` | Buttons whose color/state should reset on trigger activation |

### Methods

| Method | Description |
|--------|-------------|
| `ActivateTrigger(string)` | Set animator trigger + reset all registered buttons |
| `SetTrigger(string)` | Set animator trigger without resetting buttons |

### Behavior

- On `OnEnable`, registers click listeners on all mapped buttons
- Each button click calls `ActivateTrigger` with its mapped trigger name
- On `Update`, checks for Escape key and fires `escTriggerName` if `isEscAllowed`
- On `OnDisable`, unregisters all listeners
