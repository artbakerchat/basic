---
doc: spec
status: approved
---

# Beeplex — Technical Specification

## How This Works, In Plain Language
Beeplex is a lightweight, offline 2D simulation built in Unity with C#. 

Instead of moving the bee using arrow keys or controllers, the player moves their mouse cursor over the meadow. An invisible ray shoots from the camera into the 2D world to detect when the cursor rests over a flower patch or the hive, placing an invisible **Scent Beacon**. 

The bee is an autonomous agent driven by a simple finite state machine:
- When it senses a scent beacon at a flower, it accelerates smoothly toward it using physics-free **Vector2 steering**, adding a subtle sine-wave wobble so it floats and bobs like a real insect.
- Upon reaching the blossom, it locks on for 2–3 seconds to harvest nectar (+5 per cycle), updating a golden UI slider.
- When full (20/20 nectar) or when the player gazes back at the hive, the bee flies straight to the hive to unload its nectar into the colony's honey stores.

A background time manager monitors the computer's live local clock (`System.DateTime.Now`). When the real-world clock hits 3:00 AM, it triggers a 45-second event: the global 2D lighting dims to deep midnight blue, a banner flashes `✧ DREAM BLOOM ACTIVE ✧`, and special dream flowers activate pulsating cyan Point Light 2D halos and floating spore particles. Foraging during this window grants rare Dream Drops.

## The Core Journey Through the System
PRD ref: `prd.md > The Core Journey`.

1. **Player launches scene** → `GameManager` and `HUDManager` initialize; `HUDManager` binds to `System.DateTime.Now` to update `Clock_Text` every second. Scores initialize to 0.
2. **Player hovers cursor over Flower** → `ScentBeaconController` translates mouse position via `Camera.main.ScreenToWorldPoint()`, detects a 2D collider on `FlowerPatch`, and notifies `BeeAgent`.
3. **Bee navigates toward Flower** → `BeeAgent` transitions from `Idle` to `FlyingToBeacon`. Its movement loop calculates a normalized steering direction vector toward the beacon, applying a perpendicular sine-wave offset (`Mathf.Sin(Time.time * frequency) * amplitude`) for organic flight hover.
4. **Harvesting cycle** → When distance drops below target threshold (0.2 units), `BeeAgent` transitions to `Harvesting`. A 2.5-second coroutine runs; mid-flight gaze changes are ignored during this lock-on window. Upon completion, `currentNectar += 5`, updating `Nectar_Slider`.
5. **Hive return** → If `currentNectar >= 20` or if `ScentBeaconController` raycasts the Hive collider, `BeeAgent` switches to `ReturningToHive`.
6. **Colony deposit** → On reaching the Hive collider, `HiveBase` converts carried standard nectar into `standardHoney` and dream nectar into `dreamDrops`. `HUDManager` updates the score panels; `currentNectar` resets to 0.
7. **3:00 AM Event** → `TimeEventManager` checks local time. When 3:00 AM is reached, `TimeEventManager` triggers `OnDreamBloomStart`:
   - `Light2D` global light intensity dims to 0.25 (midnight blue).
   - `DreamEvent_Banner` enables.
   - `DreamBloom` components turn on their `Light2D` (cyan) and `ParticleSystem` (dream spores).
   - Timer counts down 45 seconds of real-world time before reverting world state.

## Stack
- **Game Engine**: Unity 2022.3 LTS or Unity 6 (2D template).
  - *Rationale*: Native support for 2D lighting, TextMeshPro, sprite animation, and particle systems out of the box.
  - *Doc*: [Unity Manual](https://docs.unity3d.com/Manual/index.html)
- **Programming Language**: C# (.NET Standard 2.1).
  - *Rationale*: Type-safe, expressive scripting language standard across Unity.
- **UI Framework**: Unity UI (uGUI) + TextMeshPro (`com.unity.textmeshpro`).
  - *Rationale*: Crisp sub-pixel font rendering for the HUD clock, banner, and score trackers.
  - *Doc*: [TextMeshPro Documentation](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/index.html)
- **Lighting & Rendering**: Universal Render Pipeline (URP) 2D Lighting (`UnityEngine.Rendering.Universal.Light2D`).
  - *Rationale*: Enables atmospheric day/night transitions and cyan bioluminescent flower halos without complex custom shaders.
  - *Doc*: [2D Lights in URP](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest/index.html?subfolder=/manual/2D/2D-Lights.html)
- **VFX**: Unity Built-in Particle System (`UnityEngine.ParticleSystem`).
  - *Rationale*: Lightweight, zero-dependency spore emission rising from active dream blooms.

## Where It Runs and How Someone Tries It
- **Runtime**: Local standalone desktop application (Windows, macOS, or Linux) or directly inside the Unity Editor.
- **System Requirements**: Any computer capable of running Unity 2022.3+ or running a standalone executable build. No internet connection or external API keys required.
- **How to Run**:
  1. Open the project in Unity Editor.
  2. Load scene: `Assets/Scenes/MainMeadow.unity`.
  3. Press the **Play** button at the top of the editor.
- **How to Record Demo**:
  - Use OBS Studio, Windows Xbox Game Bar (`Win + G`), or macOS QuickTime Screen Recording to record a 60–90 second clip demonstrating cursor scent guidance, harvesting, hive deposit, and the 3:00 AM Dream Bloom window.

## Look and Feel
Implements `prd.md > Look and Feel`.
- **Palette**: Deep obsidian backgrounds (`#0b0f19`), honey amber UI accents (`#f59e0b` / `#fbbf24`), and luminous cyan bioluminescence (`#38bdf8`).
- **Typography**: Clean, geometric sans-serif (LiberationSans or Inter SDF via TextMeshPro).
- **Audio/Vibe**: Serene, atmospheric, quiet night-time nature simulation.

## Components

### ScentBeaconController
Implements `prd.md > Feature 1: Gaze-Directed Scent Beacon & Pathfinding`.
- Uses `Camera.main.ScreenToWorldPoint(Input.mousePosition)` to cast a 2D raycast against layer masks (`FlowerLayer`, `HiveLayer`).
- When a valid target is hit, records target world position and target type.
- Dispatches event `OnBeaconPlaced(Vector2 position, TargetType type)`.
- If mouse hovers over empty ground, ignores raycast and allows bee to persist existing task.

### BeeAgent
Implements `prd.md > Feature 1`, `prd.md > Feature 2`, and `prd.md > Feature 3`.
- Manages finite state machine:
  - `Idle`: Hovers gently in place with small sine-wave bob.
  - `FlyingToBeacon`: Calculates steering direction `(targetPos - transform.position).normalized * flySpeed`. Adds perpendicular organic oscillation `perpVector * Mathf.Sin(Time.time * hoverFreq) * hoverAmp`.
  - `Harvesting`: Locks on target flower position for 2.5 seconds. Retargeting requests received during this state are queued until harvest finishes.
  - `ReturningToHive`: Navigates directly to hive target position.
- Inventory tracking: `currentNectar` (0 to 20). Automatically sets state to `ReturningToHive` when `currentNectar >= 20`.

### FlowerPatch
Implements `prd.md > Feature 2: Nectar Harvesting & Capacity Management` and `prd.md > Feature 4`.
- Attached to flower GameObjects with a 2D BoxCollider2D / CircleCollider2D.
- Properties:
  - `bool isDreamBloom`: Distinguishes standard flowers from nocturnal bloom nodes.
  - `int nectarYield = 5`.
  - `bool isDepleted`: Temporarily disabled while recovering.
  - `Light2D bloomLight`: Point Light 2D component (cyan color for dream blooms).
  - `ParticleSystem sporeParticles`: Rising glowing particle system.
- Methods:
  - `HarvestNectar()`: Returns nectar amount and starts brief cooldown.
  - `SetDreamActive(bool active)`: Enables/disables bloom light and particle emission.

### HiveBase
Implements `prd.md > Feature 3: Hive Return & Resource Deposition`.
- Sits at the center of the meadow with a 2D trigger collider.
- Holds persistent colony score: `int standardHoney` and `int dreamDrops`.
- On `BeeAgent` arrival:
  - Empties bee's carried nectar.
  - Increments `standardHoney += beePayload.standardNectar`.
  - Increments `dreamDrops += beePayload.dreamNectar`.
  - Fires `OnHiveInventoryChanged(standardHoney, dreamDrops)`.

### TimeEventManager
Implements `prd.md > Feature 4: Real-Time 3:00 AM Dream Bloom Event`.
- In `Update()`, reads `System.DateTime.Now`.
- Checks condition: `DateTime.Now.Hour == 3` (or debug trigger for testing).
- When event begins:
  - Sets `isEventActive = true`.
  - Starts 45.0f second countdown timer.
  - Dims global URP `Light2D` to midnight blue (`#0f172a`, intensity 0.25).
  - Activates all `DreamBloom` nodes.
  - Informs `HUDManager` to enable `DreamEvent_Banner`.
- When 45.0f seconds elapse:
  - Restores global lighting smoothly over 2 seconds.
  - Closes dream blooms.
  - Disables `DreamEvent_Banner`.

### HUDManager
Implements `prd.md > Canvas HUD` and `prd.md > Acceptance Criteria`.
- References UI elements:
  - `TextMeshProUGUI clockText`
  - `GameObject dreamBanner`
  - `Slider nectarSlider`
  - `TextMeshProUGUI nectarLabel`
  - `TextMeshProUGUI honeyText`
  - `TextMeshProUGUI dreamText`
- Updates clock display string: `DateTime.Now.ToString("hh:mm tt")`.
- Updates slider value and `"X / 20 Nectar"` text.
- Formats honey count: `"Honey: " + honeyCount` and `"Dream Drops: " + dreamDrops`.

## Data Model

In-memory data structures:

```csharp
public enum BeeState 
{
    Idle,
    FlyingToBeacon,
    Harvesting,
    ReturningToHive
}

public struct NectarPayload 
{
    public int standardNectar; // 0 to 20
    public int dreamNectar;    // 0 to 20
    public int Total => standardNectar + dreamNectar;
}

public class ColonyData 
{
    public int StandardHoney { get; set; } = 0;
    public int DreamDrops { get; set; } = 0;
}
```

*Lifecycle*: Fresh instance on each game launch; resets to 0 as approved for a clean testing slate.

## File Structure

```
basic/
├── Assets/
│   ├── Scenes/
│   │   └── MainMeadow.unity          # Primary playable scene with 2D environment & Canvas
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameManager.cs         # Initializes managers and wires event delegates
│   │   │   └── TimeEventManager.cs    # Evaluates system clock and controls 45s event cycle
│   │   ├── Bee/
│   │   │   ├── BeeAgent.cs            # State machine, vector steering, and payload storage
│   │   │   └── ScentBeaconController.cs # Cursor raycasting and beacon dispatching
│   │   ├── Environment/
│   │   │   ├── FlowerPatch.cs         # Nectar dispensing, dream node logic, lights & VFX
│   │   │   └── HiveBase.cs            # Hive drop-off point and colony resource store
│   │   └── UI/
│   │       └── HUDManager.cs          # Updates Canvas HUD, clock text, slider, and banner
│   ├── Prefabs/
│   │   ├── Bee.prefab                 # Bee sprite, audio, and BeeAgent component
│   │   ├── StandardFlower.prefab      # Yellow/pink flower sprite with FlowerPatch
│   │   ├── DreamBloom.prefab          # Cyan nocturnal bloom with Light2D and spore emitter
│   │   └── Hive.prefab                # Central honeycomb base with trigger collider
│   └── Sprites/
│       ├── Environment/               # Meadows, tiles, foliage sprites
│       └── UI/                        # Golden slider fills and panel borders
├── devpost/
│   ├── learner-profile.md
│   ├── scope.md
│   ├── scope.html
│   ├── prd.md
│   ├── prd.html
│   ├── spec.md
│   └── spec.html
└── public.md
```

## External Services and Dependencies
- **No external cloud APIs, auth providers, or database servers.**
- Completely offline, self-contained Unity architecture.
- Free, open, and instant to build and run on any workstation.

## Important Failure Modes
1. **Mouse cursor hovers outside game window or over non-target terrain**:
   - *Fallback*: Raycast returns `null`. `ScentBeaconController` discards the input without altering the bee's current destination or state.
2. **System clock jumps or changes during session**:
   - *Fallback*: `TimeEventManager` checks `DateTime.Now` per tick without accumulating drift; a 45-second timer runs via delta time once triggered.
3. **Bee overshoots destination at high steering speeds**:
   - *Fallback*: Distance check includes an arrival radius (0.2f units). Once within radius, velocity smoothly zeroes out and transitions to `Harvesting` or `Unloading`.

## What Was Simplified and Why
- **Vector2 Steering vs. 2D NavMesh Baking**: Vector2 steering with sine-wave hovering was chosen over Unity NavMesh. This eliminates external package requirements (`NavMeshComponents`), avoids baking steps, and delivers a much more organic, bobbing bee-flight feel.
- **In-Memory Session Scores vs. PlayerPrefs / Cloud DB**: Colony scores reset to 0 upon launch to provide a clean, predictable testing environment for hackathon judges and demo capture.
- **Mouse Cursor Raycast vs. Eye-Tracking Hardware**: Mouse cursor gaze raycasting provides an accessible, universal control scheme that runs identically on all computers and records cleanly on video.

## Decisions and Open Issues
- **Decisions**:
  - Steering: Vector2 steering + sine-wave hovering wobble accepted.
  - Input: Mouse cursor raycast via `Camera.main.ScreenToWorldPoint` accepted.
  - Persistence: Session-based reset to 0 on launch accepted.
- **One Genuine Uncertainty Clarified**:
  - The learner evaluated whether NavMesh pathfinding was necessary for the bee. Clarified that open-meadow flower foraging does not have maze obstacles; lightweight Vector2 steering is zero-dependency, simpler to maintain, and provides an authentic floating insect feel.
- **Open Issues**:
  - None blocking implementation. Everything is mapped to concrete C# classes and Unity components.
