---
doc: scope
status: approved
---

# Beeplex

A real-time ambient bee foraging simulator where players guide bees via scent beacons and stay up for a rare 3:00 AM nocturnal bloom.

## The Unique Kernel
The player doesn't manually fly the bee; looking at a flower patch casts an invisible "scent beacon" that causes the bee to dynamically pathfind and hover to harvest nectar. This loop is tied to a strict real-world clock where an elusive "Dream Bloom" event activates exclusively at 3:00 AM real time, allowing the harvest of rare Dream Drops.

## Who It's For
Players and simulation fans who appreciate ambient, atmospheric creature games and the high-commitment mystique of real-world scheduled events—those willing to be active in the dead of night to catch a fleeting digital phenomenon.

## The Core Loop
1. The player looks at a flower patch, casting an invisible scent beacon into the world.
2. The bee dynamically pathfinds toward the beacon and hovers over the blossoms, collecting nectar into its inventory (`Nectar_Slider` fills).
3. The player guides the bee back to the hive (or the bee returns when laden) to deposit the harvest.
4. The hive stores the harvest (`StandardHoney_Text` increments; during the 3:00 AM event, `DreamNectar_Text` / Dream Drops increment).
5. The player tracks the real-time clock, preparing for the 3:00 AM Dream Bloom window.

## Inspiration & Identity
* **Atmosphere**: Peaceful, immersive, and nocturnal.
* **Visual Identity & HUD**:
  * Top Center: `Clock_Text` displaying current real-world time (e.g., "03:15 AM").
  * Event Banner: `DreamEvent_Banner` reading `✧ DREAM BLOOM ACTIVE ✧` (disabled by default, active during the 3 AM window).
  * Capacity: `Nectar_Slider` with a golden yellow fill area and clear numeric label (e.g., "15 / 20 Nectar").
  * Score: `HiveScore_Panel` tracking `StandardHoney_Text` ("Honey: 120") and `DreamNectar_Text` ("Dream Drops: 3").

## Why This Matters to the Learner
To create a simulation that bridges real-world circadian time with autonomous creature behavior—turning a late-night waking hour into a rewarding, atmospheric discovery moment.

## What "Working" Looks Like
Running the Unity project opens a scene with the defined Canvas HUD. Gaze/cursor placement over flowers casts scent beacons that draw the bee through dynamic pathfinding. The bee hovers, gathers nectar, fills the golden slider, and unloads at the hive to increment honey. The clock continuously tracks local system time, and when the real-world clock enters 3:00 AM, the "✧ DREAM BLOOM ACTIVE ✧" banner illuminates and harvesting delivers Dream Drops to the hive.

## The POC Boundary
* **Now (In Scope)**:
  * Unity Canvas HUD with `Clock_Text`, `DreamEvent_Banner`, `Nectar_Slider`, and `HiveScore_Panel`.
  * Scent beacon casting on flower gaze/target.
  * Autonomous bee navigation/pathfinding to the beacon and hovering behavior.
  * Nectar collection logic with capacity limits.
  * Hive deposit mechanic updating honey and dream drop counts.
  * System clock real-time check triggering the 3:00 AM Dream Bloom event.
* **Later**:
  * Hive building upgrades and spending collected honey.
  * Multiple bee types, hive swarms, or customizable bee workers.
  * Dynamic weather, wind resistance, and predator hazards.
  * Rich ambient nocturnal audio and sound effects.
* **Explicitly Cut**:
  * Accelerated time slider / simulated time skip (cut in favor of the learner's strict real-time 3:00 AM design).
  * Direct manual arcade flight controls (cut in favor of the autonomous scent beacon system).
  * Multiplayer hive sharing and leaderboards.
