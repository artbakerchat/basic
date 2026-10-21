---
doc: prd
status: approved
---

# Beeplex — Product Requirements Document

An ambient real-time bee foraging simulator where players guide a bee through gaze-directed scent beacons to collect standard nectar and capture rare Dream Drops during a strict real-world 3:00 AM nocturnal event.
Source: `scope.md > The Unique Kernel` and `scope.md > The Core Loop`.

## The Core Journey
1. **Arrival**: The player opens Beeplex. The scene displays an ambient flower field, the hive, the autonomous bee, and the top-level Unity Canvas HUD with the real-time clock running.
2. **Gaze & Scent Beacon**: The player hovers/focuses their gaze on a blooming flower patch. An invisible "scent beacon" is placed at that flower.
3. **Autonomous Navigation**: The bee detects the scent beacon, smoothly navigates through dynamic pathfinding, and begins hovering over the target blossom.
4. **Harvesting**: The bee locks onto the flower for 2–3 seconds to siphon nectar. The golden `Nectar_Slider` fills (e.g., from 0/20 towards 20/20).
5. **Hive Return & Deposit**:
   - *Autonomous*: When the bee hits max capacity (20/20), it automatically breaks away and flies back to the hive to deposit its load.
   - *Manual Recall*: The player can look directly at the hive at any point to cast a recall beacon, summoning the bee home early.
6. **Hive Score Update**: Once the bee unloads at the hive, `StandardHoney_Text` increments. The nectar slider resets to 0/20, and the bee idles, awaiting the next beacon.
7. **The 3:00 AM Dream Bloom Window**:
   - When local system time triggers the 3:00 AM event, the scene shifts for exactly 45 seconds of real-world time.
   - World lighting dims to deep midnight blue.
   - `DreamEvent_Banner` illuminates with `✧ DREAM BLOOM ACTIVE ✧`.
   - Special dream blooms uncurl, pulsating with neon/cyan bioluminescent 2D light and releasing floating spore particles.
   - Siphoning a dream bloom awards Dream Drops, deposited to update `DreamNectar_Text`.
   - At the 45-second mark, dream blooms close, lighting restores, the banner disables, and standard foraging resumes.

## Screens and Layout

The application consists of a single unified game screen in Unity featuring the play area and the overlay Canvas HUD:

### 1. Game World (2D Top-Down / Isometric View)
- **Central Hive**: The home base where the bee originates and deposits harvested nectar.
- **Flower Field**: Multiple standard flower patches distributed across the foraging area.
- **Dream Bloom Nodes**: Special dormant nocturnal flora that activate exclusively during the 3:00 AM window.
- **The Bee**: The player's autonomous agent that flits, pathfinds, hovers, and carries nectar.

### 2. Canvas HUD
Source: `scope.md > Visual Identity & HUD`.
- **Top Center — `Clock_Text`**: Displays the active local system clock formatted as `hh:mm AM/PM` (e.g., `03:15 AM`).
- **Sub-Header Banner — `DreamEvent_Banner`**: TextMeshPro banner displaying `✧ DREAM BLOOM ACTIVE ✧`. Default state: Disabled (`gameObject.SetActive(false)`). Enabled exclusively during the 45-second Dream Bloom window.
- **Top Left / Bottom Center — `Nectar_Slider`**:
  - Slider with a rich golden-yellow fill area.
  - `Nectar_Label` TextMeshPro overlay: `"[Current] / [Max] Nectar"` (e.g., `"15 / 20 Nectar"`).
- **Top Right — `HiveScore_Panel`**:
  - `StandardHoney_Text`: `"Honey: [Count]"` (e.g., `"Honey: 120"`).
  - `DreamNectar_Text`: `"Dream Drops: [Count]"` (e.g., `"Dream Drops: 3"`).

## Look and Feel
- **Visual Style**: Calming, nocturnal, atmospheric, and organic.
- **Palette**:
  - Deep midnight blue / dark obsidian background (`#0b0f19` to `#111827`).
  - Warm honey gold for standard nectar, slider fill, and daytime accents (`#f59e0b` / `#fbbf24`).
  - Pulsating neon cyan bioluminescence for dream blooms and dream spores (`#38bdf8` / `#06b6d4`).
  - Soft off-white and emerald green for text readouts and hive success indicators.
- **Lighting & VFX**:
  - Global 2D lighting that smoothly dims to deep midnight blue during the 3:00 AM event.
  - Point Light 2D attached to active dream blooms creating a gentle pulsating halo.
  - Rising particle system emitting tiny, glowing dream-spores from active blooms.
- **Typography**: Clean, crisp sans-serif via TextMeshPro with high-contrast legibility against dark nature backdrops.

## Features and Behavior

### Feature 1: Gaze-Directed Scent Beacon & Pathfinding
Source: `scope.md > The Unique Kernel`.
- Looking at/hovering the cursor over any interactive flower patch casts an invisible scent beacon on that position.
- The bee senses the beacon and immediately begins dynamic pathfinding toward it.
- **Retargeting Mid-Flight**: If the player shifts gaze to a different flower while the bee is still traveling, the bee immediately recalculates its course toward the new scent beacon.
- **Retargeting Mid-Harvest**: If the bee is already actively locked onto a flower and siphoning nectar when the player looks away, it will finish harvesting that current flower first, then depart toward the newly targeted beacon.
- **Idle State**: If no beacon is cast or the player gazes into empty terrain, the bee completes its last valid task and hovers in place.

#### Acceptance Criteria
- [ ] Hovering over a flower patch visibly updates the bee's trajectory toward that flower.
- [ ] Gaze shifts while in flight result in immediate, smooth steering toward the new target.
- [ ] Gaze shifts during active harvest do not cancel the current siphoning animation or drop yield; the bee departs only after the 2–3 second harvest finishes.

### Feature 2: Nectar Harvesting & Capacity Management
Source: `scope.md > The Core Loop`.
- Arriving at a flower initiates an automatic 2–3 second harvesting hover.
- Each standard harvest yields +5 Nectar (configurable), filling the `Nectar_Slider` proportionally up to its maximum capacity of 20.
- `Nectar_Label` updates dynamically to reflect numeric progress (`0 / 20` -> `5 / 20` -> `20 / 20`).
- Once capacity reaches 20 / 20, the bee ceases foraging and transitions immediately to the **Hive Return** behavior.

#### Acceptance Criteria
- [ ] Hovering over a valid flower increases nectar by the designated increment over 2–3 seconds.
- [ ] `Nectar_Slider` fill amount and `Nectar_Label` text match the bee's current internal payload.
- [ ] The bee rejects further flower harvesting once current nectar equals maximum capacity (20).

### Feature 3: Hive Return & Resource Deposition
Source: `scope.md > The POC Boundary`.
- **Autonomous Return**: When nectar hits 20/20, the bee automatically prioritizes flying straight back to the hive without requiring player input.
- **Manual Early Recall**: The player can look directly at the hive to cast a scent beacon on it, summoning the bee home early regardless of its current nectar amount.
- **Unloading**: Upon touching the hive:
  - All carried standard nectar is converted into Standard Honey at a 1:1 ratio.
  - Any carried dream nectar is converted into Dream Drops.
  - `StandardHoney_Text` and `DreamNectar_Text` increment with a brief punchy visual feedback effect.
  - The bee's carried nectar empties to 0/20, and the `Nectar_Slider` resets to zero.

#### Acceptance Criteria
- [ ] Bee automatically turns toward the hive the moment nectar hits 20/20.
- [ ] Looking at the hive with partial nectar (e.g. 5/20) successfully recalls the bee.
- [ ] Carried nectar is cleared upon hive arrival and scores are updated accurately.

### Feature 4: Real-Time 3:00 AM Dream Bloom Event
Source: `scope.md > The Unique Kernel` and `scope.md > What "Working" Looks Like`.
- The system clock is continuously evaluated against local time.
- At 3:00 AM sharp, the Dream Bloom event triggers:
  - The window stays active for exactly **45 seconds of real-world time** (representing in-game 3:00 AM – 3:30 AM).
  - World lighting darkens to deep midnight blue.
  - `DreamEvent_Banner` ("✧ DREAM BLOOM ACTIVE ✧") activates.
  - Dream blooms activate with uncurling petals, pulsating cyan Point Light 2D, and rising spore particles.
  - The bee can harvest dream blooms, collecting special dream nectar.
  - Returning dream nectar to the hive increments `DreamNectar_Text` ("Dream Drops: X").
- At the expiration of the 45-second window:
  - Dream blooms close and extinguish their lights/particles.
  - `DreamEvent_Banner` deactivates.
  - Ambient lighting smoothly interpolates back to standard illumination.
  - Any dream drop being actively siphoned at the cutoff moment is completed.

#### Acceptance Criteria
- [ ] Entering 3:00 AM activates the banner, lighting change, and bloom particle/light VFX for exactly 45 seconds.
- [ ] Siphoning dream blooms during this window yields dream nectar and deposits Dream Drops to the hive counter.
- [ ] After 45 seconds, the banner deactivates and world lighting returns to standard levels.

## States and Boundaries

- **State 1: First Run / Fresh Arrival**
  - Bee is perched at the hive.
  - Slider is at 0 / 20.
  - Scores: Honey = 0, Dream Drops = 0.
  - Clock shows current live system time.
  - Dream banner is disabled.
- **State 2: Active Foraging (Standard)**
  - Player gazing at flower patches.
  - Bee pathfinding or hovering 2-3 seconds per flower.
  - Slider filling up in increments of 5.
- **State 3: Laden / Return Flight**
  - Nectar slider is at 20 / 20.
  - Bee ignores flower scent beacons and heads straight to the hive.
- **State 4: Dream Bloom Active (45s Window)**
  - Clock is in the 3:00 AM window.
  - Deep blue atmosphere, cyan lights, and rising dream spores.
  - Dream banner visible.
- **State 5: Empty Gaze / Non-Interactive Target**
  - Player looks at open ground, rocks, or non-forageable terrain.
  - No new beacon is created; the bee finishes its active command or idles calmly.

## Product Decisions
- **Gaze-directed Scent Beacons vs. Direct Flight**: Player casts beacons via gaze/cursor rather than steering the bee with WASD or gamepad sticks, creating a serene, indirect caretaker relationship.
- **Autonomous vs. Manual Hive Return**: Hybrid approach—fully autonomous when full (20/20) to prevent wasted time, but supports voluntary manual recall by looking at the hive.
- **Harvest Priority during Retargeting**: The bee respects ongoing siphoning (2-3s) before turning toward a new gaze beacon, avoiding erratic jitter when the player moves their eyes quickly across the screen.
- **45-Second Real-Time Event Window**: Real-world 3:00 AM arrival creates genuine mystique; 45-second duration provides a punchy, high-focus gameplay window that fits cleanly into video capture and demo requirements.

## What We're Building
- Complete Unity scene containing the Hive, standard flower patches, and dream bloom nodes.
- Canvas HUD: `Clock_Text`, `DreamEvent_Banner`, `Nectar_Slider`, and `HiveScore_Panel`.
- Scent beacon raycasting and dynamic bee navigation system.
- Harvesting mechanics with 2–3s hover time and 20-unit capacity.
- Hive deposit scoring logic for Standard Honey and Dream Drops.
- Real-time clock evaluator triggering the 45-second midnight event with cyan lights and spore VFX.

## Deferred From the POC
- Hive comb building or honey-spending skill trees (deferred to focus on foraging game feel).
- Multi-bee swarm coordination (single bee proves the beacon and nocturnal mechanic).
- Nighttime predator encounters or weather hazards (deferred to preserve peaceful simulation tone).

## Non-Goals
- Manual arcade dogfighting or steering controls.
- Speed-up debug sliders in the user-facing HUD (game maintains authentic real-time clock integrity).
- Persistent multiplayer database or network synchronization.

## Open Questions
- None blocking specification. All core loops, timings, behaviors, and visual cues are resolved.
