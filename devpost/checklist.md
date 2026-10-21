---
doc: checklist
status: approved
---

# Build Checklist

Build mode: fast

## Slices

- [x] **1. Project scaffolding, gaze scent beacon, and autonomous bee flight**
  Becomes usable: The Unity scene loads the meadow environment, central hive, and flower patches. Hovering the mouse cursor over flowers casts an invisible scent beacon, and the bee smoothly steers and hovers over the targeted flower with organic sine-wave flight.
  Why now: Proves the core project architecture, 2D raycasting, and the unique kernel (autonomous scent guidance) in the very first step.
  PRD ref: `prd.md > Feature 1: Gaze-Directed Scent Beacon & Pathfinding`, `prd.md > Screens and Layout`
  Spec ref: `spec.md > ScentBeaconController`, `spec.md > BeeAgent`, `spec.md > File Structure`
  Build: Scaffold project directory structure; implement `ScentBeaconController.cs` (mouse raycasting) and `BeeAgent.cs` (FSM, Vector2 steering, and sine-wave hover); configure meadow scene with colliders.
  Verify (mechanical): Compile C# scripts with zero errors; run unit/syntax verification to validate raycast logic and bee steering calculations toward beacon coordinates.
  Learner check: Open the scene in Unity, move your mouse over different flower patches, and verify the bee steers and hovers smoothly over the targeted flower.
  Commit: `Scaffold meadow and implement gaze scent beacon with autonomous bee flight`

- [x] **2. Nectar harvesting, golden capacity slider, and hive honey deposit**
  Becomes usable: When the bee arrives at a flower, it locks on for 2.5 seconds to harvest (+5 nectar), dynamically updating the golden `Nectar_Slider`. Reaching 20/20 max capacity (or looking at the hive) sends the bee to the hive to deposit honey, resetting the slider and incrementing the honey score.
  Why now: Closes the full core gameplay loop (forage -> fill -> deposit -> score) before layering on nocturnal lighting and timed events.
  PRD ref: `prd.md > Feature 2: Nectar Harvesting & Capacity Management`, `prd.md > Feature 3: Hive Return & Resource Deposition`
  Spec ref: `spec.md > FlowerPatch`, `spec.md > HiveBase`, `spec.md > HUDManager`, `spec.md > BeeAgent`
  Build: Implement `FlowerPatch.cs` (harvest coroutine, cooldown), `HiveBase.cs` (deposit trigger, score store), extend `BeeAgent.cs` (capacity limit, retargeting rules), and wire `HUDManager.cs` (`Nectar_Slider`, `StandardHoney_Text`).
  Verify (mechanical): Compile scripts with zero errors; verify that harvest calls increment payload, reach capacity lockout at 20, transition to return state, and deposit into hive counter.
  Learner check: Direct the bee to flowers, watch the golden nectar slider fill up to 20/20, see the bee return to the hive, and observe Standard Honey incrementing.
  Commit: `Implement nectar harvesting, UI capacity slider, and hive honey deposits`

- [x] **3. Real-time clock, midnight lighting transition, and 3:00 AM Dream Bloom event**
  Becomes usable: The Canvas clock continuously reflects local system time. When the 3:00 AM window arrives, world lighting dims to midnight blue, the `✧ DREAM BLOOM ACTIVE ✧` banner illuminates, dream blooms glow cyan with floating spores for 45 seconds, and siphoning them deposits rare Dream Drops into the hive score.
  Why now: Completes the signature nocturnal event, fulfills all acceptance criteria, and readies the application for recording the demo video.
  PRD ref: `prd.md > Feature 4: Real-Time 3:00 AM Dream Bloom Event`, `prd.md > Look and Feel`
  Spec ref: `spec.md > TimeEventManager`, `spec.md > HUDManager`, `spec.md > FlowerPatch`
  Build: Implement `TimeEventManager.cs` with `System.DateTime.Now` evaluator and 45s timer; wire global URP `Light2D` dimming, Point Light 2D, and dream spore `ParticleSystem` on `DreamBloom` nodes; update `HUDManager.cs` to show banner and track `DreamNectar_Text`.
  Verify (mechanical): Compile scripts with zero errors; verify time event trigger, 45-second countdown transition, and Dream Drop deposit score update.
  Learner check: Observe the clock, witness the midnight blue lighting transition and bioluminescent cyan blooms, and harvest a Dream Drop.
  Commit: `Add real-time clock, midnight lighting transition, and 3 AM Dream Bloom event`

## Hands-on Checkpoints

- [x] Early usable behavior explored — Slice 2 boundary (full foraging and hive deposit loop)
- [ ] Final kick-the-tires exploration and feedback completed

## Final Review

- [ ] Final review complete — feedback resolved and learner confirms ready to ship

## Code Tour and App Map

- [ ] Learning activity complete — guided route, focused alternative, prior practice connected, or brief recap
- [ ] Optional edit and transfer reflection addressed — offered/declined/already covered/not applicable as appropriate
- [ ] `devpost/app-map.html` generated from finished code, checked, and shown, including a project-grounded practice to reuse

Activity and evidence: [what actually happened; real document/test/code references; unfinished work if interrupted]
Route and stops: [actual paths and symbols; guided stops completed, or reference-only route]
Edit outcome: [tried/kept/reverted/declined/not applicable; verification if changed]
Reflection: [offered/answered/declined/already covered — personal answer belongs only in the ignored profile]
Activity mode: [live app and editor, explicit static fallback, focused alternative, prior practice, or recap]

## Revisions
