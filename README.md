# StateShift

A PC first-person movement game built in Unity. Features a hierarchical FSM-driven movement system with momentum, wall-running, grappling, dashing, and a multi-chunk health system with type-based damage scaling.

## Features

- **Movement FSM** — composite state machine with grounded sub-states (walk, sprint, crouch, slide), wall-running, dashing, grappling, and in-air states
- **Momentum system** — builds from sprinting/sliding/downhill movement; decays exponentially; influences speed and adrenaline
- **Multi-chunk health** — sequential health chunks with type-based damage multipliers (Kinetic/Fire/Plasma/Energy vs. Flesh/Exo/Shield)
- **AI tick manager** — round-robin agent tick distribution across frames
- **Event system** — global C# Action events for pause, game-over, restart, and exit

## Documentation

| Doc | Contents |
|-----|----------|
| [Architecture](docs/architecture.md) | System overview, data flow, design patterns |
| [FSM & Movement States](docs/fsm.md) | StateMachine, BaseState, all player states |
| [Player Movement Components](docs/player-movement.md) | PlayerController and all movement components |
| [Health System](docs/health.md) | Health chunks, damage matrix, adrenaline modifier |
| [AI System](docs/ai.md) | AITickManager, AIMemory, TestEnemy |
| [Managers & Core](docs/managers.md) | Singleton, EventsManager, GameManager, SceneLoader |
| [Known Issues](docs/known-issues.md) | Bugs, performance issues, and incomplete code |