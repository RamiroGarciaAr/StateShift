# AI System

## AITickManager

**File:** `Assets/Scripts/AI/AITickManager.cs`
Extends: `Singleton<AITickManager>`

Distributes AI updates across frames using a round-robin scheduler. Instead of ticking all agents every frame, it ticks a fixed number per frame, smoothing CPU cost as enemy count grows.

### Configuration

| Field | Range | Description |
|-------|-------|-------------|
| `_ticksPerFrame` | 1–5 | How many agents are ticked per frame |

### API

| Method | Description |
|--------|-------------|
| `RegisterAgent(ITickable agent)` | Add agent to the tick list |
| `UnregisterAgent(ITickable agent)` | Remove agent; adjusts `_currentIndex` if needed |
| `TickNextAgent()` | Tick one agent and advance the round-robin index |

### Pause Integration

Subscribes to `EventsManager.OnGamePause` and `EventsManager.OnGameOver`. When either fires, `_isPaused` is set and no agents are ticked until unpaused.

### Tick Frequency Per Agent

With N agents and `_ticksPerFrame = T`, each agent is ticked approximately every `N/T` frames. At 60 fps with 10 agents and 1 tick/frame, each agent ticks every ~10 frames (~6 Hz). Increase `_ticksPerFrame` for more responsive AI at higher CPU cost.

> **Context menu:** `LogRegisteredAgents()` prints the current agent list to the console.

---

## ITickable

**File:** `Assets/Scripts/AI/ITickable.cs`

Interface for objects that receive periodic AI updates.

```csharp
bool IsTickable { get; }              // if false, agent is skipped this tick
void OnTick(float deltaTime);         // called once per tick
```

`AITickManager` checks `IsTickable` before calling `OnTick`. Agents can use this to self-disable (e.g., dead enemies return `false`).

---

## AIMemory

**File:** `Assets/Scripts/AI/AIMemory.cs`

Tracks an AI agent's awareness state and last-known player information.

### AlertLevel Enum

| Level | Meaning |
|-------|---------|
| `Unaware` | No knowledge of player |
| `Suspicious` | Something triggered interest, searching |
| `Alerted` | Has confirmed player position |

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentAlertLevel` | `AlertLevel` | Active awareness state |
| `LastKnownPlayerPosition` | `Vector3` | Last confirmed or suspected player position |
| `LastSeenPlayerTime` | `float` | `Time.time` of last player sighting |
| `CanSeePlayer` | `bool` | Currently has line-of-sight to player |

### Methods

| Method | Description |
|--------|-------------|
| `SetAlertLevel(AlertLevel)` | Transition to new alert level |

### Debug Visualization

`OnDrawGizmos` renders a sphere at the agent's position colored by alert level:
- Green = Unaware
- Yellow = Suspicious
- Red = Alerted

A line is also drawn from the agent to `LastKnownPlayerPosition`.

> **Typo:** The backing field is named `LastSeenPleyerTime` ("Pleyer"). This doesn't affect runtime behavior but surfaces in serialized data.

---

## AIPerception

**File:** `Assets/Scripts/AI/AIPerception.cs`

Stub class — currently empty. Intended to handle line-of-sight and detection logic that would update `AIMemory`.

---

## TestEnemy

**File:** `Assets/Scripts/AI/TestEnemy.cs`
Implements: `ITickable`
Requires: `NavMeshAgent`

A prototype enemy that navigates to waypoints set by `WaypointController`. Used to validate the AI tick system and NavMesh setup.

### Behavior

On each `OnTick`:
1. Check if `WaypointController.HasDestination`
2. If a destination exists, set `NavMeshAgent.destination`
3. If the agent has arrived (within 0.5 m), clear the destination

`IsTickable` returns `_isAlive`. Dead enemies are skipped by the tick manager.

### Registration

`TestEnemy` calls `AITickManager.Instance.RegisterAgent(this)` on `Start` and `UnregisterAgent(this)` on `OnDestroy`.

---

## WaypointController

**File:** `Assets/Scripts/AI/WaypointController.cs`

Simple click-to-move waypoint setter used in combination with `TestEnemy` during prototyping.

### Static Interface

| Member | Description |
|--------|-------------|
| `CurrentDestination` | Static `Vector3` — current target position |
| `HasDestination` | `true` after a destination is set |

### Behavior

On mouse click: raycasts against the `Ground` layer from the camera. If a hit is found, sets `CurrentDestination` and `HasDestination = true`.

This is a debug/testing utility and is not part of the production AI design.
