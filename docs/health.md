# Health System

## Damage Flow

```
caller: target.TakeDamage(new DamageInfo(baseDamage, DamageType, hitPoint))
  │
  ├─ [guard] if !IsAlive → return
  │
  ├─ ApplyDamageModifiers(damageInfo)
  │    for each IDamageModifier (sorted by Priority ascending):
  │      damage = modifier.ModifyDamage(damage, damageInfo)
  │    damageInfo.FinalDamage = result
  │
  ├─ for each non-depleted HealthChunk (index 0 → N):
  │    multiplier    = DamageMatrixSO.GetMultiplier(damageType, chunk.HealthType)
  │    overflow      = chunk.ApplyDamage(remaining * multiplier)
  │    remaining     = overflow
  │    if chunk depleted → OnChunkDepleted(i)
  │
  ├─ OnHealthChanged(HealthChangeEventArgs)
  └─ if !IsAlive → OnDeath()
```

> **Bug:** `remaining` is reassigned the raw overflow from `ApplyDamage`, but the multiplier for the *next* chunk was already baked into the previous call. The multiplier of the depleted chunk effectively leaks into subsequent chunks. See [known-issues.md](known-issues.md).

---

## Damage Type × Health Type Matrix

Defined in `DamageMatrixSO`. Default values:

| | Kinetic | Fire | Plasma | Energy |
|---|---|---|---|---|
| **Flesh** | 1.0× | 1.5× | 1.0× | 0.5× |
| **Exo** | 0.75× | 1.0× | 1.0× | 1.5× |
| **Shield** | 1.0× | 0.5× | 2.0× | 1.0× |
| **Player** | 1.0× | 1.0× | 1.0× | 1.0× |

---

## BaseHealth

**File:** `Assets/Scripts/Health/Components/BaseHealth.cs`
Namespace: `Health`
Abstract — subclassed by `PlayerHealth` and `EnemyHealth`.

### Properties

| Property | Description |
|----------|-------------|
| `CurrentHealth` | Sum of `CurrentHealth` across all chunks |
| `MaxHealth` | Sum of `MaxHealth` across all chunks |
| `HealthNormalize` | `CurrentHealth / MaxHealth` (0–1) |
| `IsAlive` | `CurrentHealth > 0` |
| `CanHeal` | Virtual; `IsAlive && CurrentHealth < MaxHealth` |

### Events

| Event | Signature | When |
|-------|-----------|------|
| `OnHealthChanged` | `Action<HealthChangeEventArgs>` | After every damage or heal |
| `OnDeath` | `Action` | When health reaches 0 |
| `OnChunkDepleted` | `Action<int>` | When a chunk is fully drained (arg = chunk index) |

### Methods

| Method | Description |
|--------|-------------|
| `TakeDamage(DamageInfo)` | Apply damage with modifier + chunk flow |
| `RegisterDamageModifier(IDamageModifier)` | Add modifier to the chain |
| `UnregisterDamageModifier(IDamageModifier)` | Remove modifier |
| `Heal(float)` | Abstract — subclass defines where healing is applied |

---

## PlayerHealth

**File:** `Assets/Scripts/Health/Components/PlayerHealth.cs`
Config: `PlayerHealthConfigSO`

### Structure

- 1 **main chunk** (default 100 HP, type `Player`)
- N **side chunks** (default 2 × 20 HP each, type `Player`)

Side chunks act as shields. The main chunk is the last to deplete.

### Properties

| Property | Description |
|----------|-------------|
| `MainChunkHealth01` | Normalized health of the first (main) chunk |
| `ActiveSideChunks` | Count of non-depleted side chunks |
| `TotalSideChunks` | Total configured side chunks |
| `HasLostChunks` | `true` if any side chunk is depleted |

### Methods

| Method | Description |
|--------|-------------|
| `Heal(float)` | Heals main chunk only |
| `RestoreSideChunk()` | Restore the first depleted side chunk |
| `CanRestoreSideChunk()` | Returns `true` if a depleted side chunk exists |

### Events

| Event | When |
|-------|------|
| `OnSideChunkRestored(int index)` | After `RestoreSideChunk()` succeeds |

---

## EnemyHealth

**File:** `Assets/Scripts/Health/Components/EnemyHealth.cs`
Config: `EnemyHealthConfigSO`

Chunks are defined entirely in the ScriptableObject; each can have a different `HealthType` (Flesh/Exo/Shield) enabling type-based vulnerability design.

### Properties

| Property | Description |
|----------|-------------|
| `Chunks` | `IReadOnlyList<HealthChunk>` — all configured chunks |
| `CurrentChunkIndex` | Index of the first non-depleted chunk |

### Methods

| Method | Description |
|--------|-------------|
| `Heal(float)` | Heals the current (first non-depleted) chunk |
| `GetChunkHealthNormalized(int index)` | Get 0–1 health of a specific chunk |

---

## HealthChunk

**File:** `Assets/Scripts/Health/HealthChunk.cs`
Serializable class representing one health segment.

| Member | Description |
|--------|-------------|
| `CurrentHealth` | Current HP |
| `MaxHealth` | Maximum HP |
| `HealthType` | `Player`, `Flesh`, `Exo`, or `Shield` |
| `IsDepleted` | `CurrentHealth <= 0` |
| `HealthNormalized` | 0–1 |
| `ApplyDamage(float)` | Reduce health; returns overflow damage |
| `Heal(float)` | Restore health up to max |
| `Restore()` | Full restore |

---

## PlayerAdrenaline

**File:** `Assets/Scripts/Health/Components/PlayerAdrenaline.cs`
Implements: `IDamageModifier`

Reduces incoming damage based on the player's current momentum. Higher momentum = more adrenaline = less damage taken.

### Configuration

| Parameter | Default | Description |
|-----------|---------|-------------|
| `maxLevel` | 4 | Maximum adrenaline level |
| `damageReductionPerLevel` | 0.10 | Damage reduction per level (10% each) |
| `levelThresholds` | 0.25/0.5/0.75/1.0 | Momentum01 thresholds per level |

At max level (4), damage is reduced by 40%.

### Properties

| Property | Description |
|----------|-------------|
| `CurrentLevel` | Active adrenaline level (0–4) |
| `DamageReduction` | `CurrentLevel × damageReductionPerLevel` |
| `LevelNormalized` | `CurrentLevel / MaxLevel` |
| `Priority` | 100 (applied after most other modifiers) |

### Events

| Event | When |
|-------|------|
| `OnLevelChanged(int)` | Adrenaline level changed |

### IDamageModifier

```csharp
float ModifyDamage(float baseDamage, DamageInfo) =>
    baseDamage * (1f - DamageReduction)
```

---

## ScriptableObjects

### DamageMatrixSO

**File:** `Assets/Scripts/ScriptableObjects/Health/DamageMatrixSO.cs`
Menu: `Health/Damage Matrix`

Defines a `DamageMultiplierRow` per `HealthType`. Each row stores a multiplier for every `DamageType`.

```csharp
float GetMultiplier(DamageType damageType, HealthType healthType)
```

`Player` health type always returns 1.0× (no weakness/resistance).

---

### PlayerHealthConfigSO

**File:** `Assets/Scripts/ScriptableObjects/Health/PlayerHealthConfigSO.cs`
Menu: `Health/Player Health Config`

| Field | Default | Description |
|-------|---------|-------------|
| `mainChunkHealth` | 100 | HP of the main chunk |
| `sideChunkHealth` | 20 | HP per side chunk |
| `sideChunkCount` | 2 | Number of side chunks |

---

### EnemyHealthConfigSO

**File:** `Assets/Scripts/ScriptableObjects/Health/EnemyHealthConfigSO.cs`
Menu: `Health/Enemy Health Config`

Contains a list of `HealthChunkData` structs, each defining `maxHealth` and `healthType`. Assign in Inspector to configure enemy chunk layouts.

---

## Interfaces

### IHealth

```csharp
float CurrentHealth { get; }
float MaxHealth { get; }
float HealthNormalize { get; }
bool IsAlive { get; }
event Action<HealthChangeEventArgs> OnHealthChanged;
event Action OnDeath;
```

### IDamagable

```csharp
bool IsAlive { get; }
void TakeDamage(DamageInfo damageInfo);
```

### IHealable

```csharp
bool CanHeal { get; }
void Heal(float amount);
```

### IDamageModifier

```csharp
int Priority { get; }          // applied in ascending order
float ModifyDamage(float baseDamage, DamageInfo damageInfo);
```

---

## Data Classes

### DamageInfo

```csharp
float BaseDamage    // read-only, set at construction
float FinalDamage   // set by ApplyDamageModifiers
DamageType DamageType
Vector3 HitPoint
```

### HealthChangeEventArgs

```csharp
float PreviousHealth
float CurrentHealth
float MaxHealth
float DamageDealt
int   ChunkIdx
bool  ChunkDepleted
```

---

## Enums

### DamageType

| Value | Effective Against |
|-------|-------------------|
| `Kinetic` | All types (neutral) |
| `Fire` | Flesh (1.5×) |
| `Plasma` | Shield (2.0×) |
| `Energy` | Exo (1.5×) |

### HealthType

| Value | Weak To |
|-------|---------|
| `Player` | Nothing (1.0× all) |
| `Flesh` | Fire |
| `Exo` | Energy |
| `Shield` | Plasma |
