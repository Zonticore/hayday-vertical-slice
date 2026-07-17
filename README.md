# Hay Day Vertical Slice

A mobile-first recreation of the core *Hay Day* farming loop, built in Unity for the Voidpet Game Developer Challenge.

**Completed in eight working hours.** I focused on shipping one coherent resource economy rather than a larger collection of disconnected features.

## Submission

| Deliverable | Link |
| --- | --- |
| Windows build | [Download ZIP](./Builds/HaydayVerticalSliceWindowsBuild.zip) |
| Android build | [Download ZIP](./Builds/HaydayVerticalSliceAndroidBuild.zip) |
| Visual showcase | [Watch video](https://drive.google.com/file/d/1nBnzh0W3yKT3IhUaZd32rH2Gz33YGKo5/view?usp=sharing) |
| Technical showcase | [Watch video](https://drive.google.com/file/d/1OcYPaWEeg5JR-cjK_ifpdV2qra0rLQCJ/view?usp=sharing) |
| AI session transcript | [View or download transcript](./Codex_AI_Transcript.txt) |

> **Mobile side note:** [This short phone recording](https://drive.google.com/file/d/1SgJtUh-AbegRFLofV16Ya-0u7Jr4pgbM/view?usp=sharing) shows the Android build running and being played on a physical device.

### Build instructions

- **Windows:** Extract the ZIP and run `HayDayVerticalSlice.exe`. Keep the executable beside its data folder and DLLs.
- **Android:** Extract the ZIP, transfer the APK to an Android device, allow installation from that file source if prompted, and install it.

## The playable loop

1. Harvest mature wheat by dragging the sickle across farm patches.
2. Replant wheat and wait for it to grow.
3. Build a feed mill and turn wheat into chicken feed.
4. Buy chickens from the store and drag them into the coop.
5. Feed chickens individually; only fed chickens produce eggs.
6. Collect eggs and complete wheat or egg orders.
7. Earn coins and XP, level up, and expand the farm.

```text
Wheat -> Chicken Feed -> Fed Chickens -> Eggs -> Orders -> Coins + XP
   |                                                        |
   +-------------------- Barn / Silo -----------------------+
```

## What I built

### Farming and production

- Six starting patches with mature wheat
- Drag-to-harvest sickle and drag-to-plant interactions
- Seed use, crop growth timers, and speed-up support
- Placeable farm patches and feed mill
- Timed chicken-feed production and collection

### Chickens

- Chickens purchased by dragging them from the store into the coop
- One feed consumed per chicken, with egg output based on how many were fed
- Idle and producing chicken visuals
- Empty/filled trough and egg-ready states
- Animated egg collection into barn storage

### World, economy, and UI

- Isometric grid with placement validation and multi-tile footprints
- Store categories for animals and buildings
- Drag buildings out of the store and place them in the world
- Separate barn and silo inventories with independent capacities
- Random wheat and egg request-board orders with variable rewards
- Item, coin, and XP fly-to-HUD animations
- Bouncing coin/XP feedback, XP thresholds, and leveling
- Touch/mouse camera panning plus pinch and wheel zoom

## Controls

- **Pan:** drag an empty part of the world
- **Zoom:** pinch on mobile or use the mouse wheel
- **Interact:** tap/click a tile or building
- **Harvest or plant:** drag the selected context action across patches
- **Place a building:** drag it from the store into the world
- **Buy a chicken:** drag it from the Animals store tab onto the coop
- **Close UI:** tap/click outside the open screen

## Scope and decisions

I started by playing *Hay Day* and identifying the smallest set of systems that communicates its identity. The essential experience was the dependency chain between farming, production, animals, storage, orders, and progression, not any single crop or building.

I therefore prioritized one complete loop. The bakery, visitors, neighbor farms, roadside shop, storage upgrades, and broader crop variety were investigated but intentionally left outside the eight-hour slice.

## Architecture

### Data-driven tile factories

```text
TileDefinitionSO
    -> TileFactorySO
        -> base TileInstance
            -> reusable state, visual, interaction, and production components
```

`TileDefinitionSO` owns a tile's identity, sprite, footprint, category, price, and factory. Each focused `TileFactorySO` creates the shared tile foundation and composes only the behavior it needs. Farm patches, coops, request boards, feed mills, terrain, and storage buildings all use the same placement pipeline without one large inheritance hierarchy.

New content can normally be added through data, a focused factory, and reusable components instead of modifying the grid or placement systems.

### ScriptableObject-authored content

ScriptableObjects define items, tiles, recipes, store inventory, prices, order pools, reward ranges, context actions, tools, the starting layout, and initial player progression. This keeps balancing and content selection out of gameplay code.

Interactive objects also contribute actions to one shared context-menu system. Planting, harvesting, feeding, collecting, production, and speed-ups remain small and reusable. State and visuals are separated for farm patches, production buildings, and chicken coops.

## AI-first development

I used Codex directly in the Unity project and ChatGPT image generation for the visual assets. I established the scope and architecture, decomposed the work into focused systems, gave the agents constraints, and reviewed the output through playtesting, compilation, and Git diffs.

My workflow was:

1. Analyze the reference game and rank its core loops.
2. Define the grid, factory, context-action, and ScriptableObject architecture.
3. Have Codex implement bounded pieces directly in the project.
4. Generate and import visual assets with ChatGPT.
5. Play, inspect, reject weak output, and iterate.
6. Commit throughout development so changes remained reviewable.

AI output was treated as a draft. Human-directed corrections included replacing hard-coded data with authored assets, fixing unsuitable button-based drag interactions, correcting image transparency failures, changing chicken feeding to per-animal production, and separating coop, trough, chicken, and egg visual states.

The modular design also works well with coding agents: responsibilities stay small enough to reason about within limited context while remaining maintainable for conventional team development.

## Running from source

1. Install Unity **6000.4.0f1**.
2. Clone this repository and open it through Unity Hub.
3. Open `Assets/Scenes/Main.unity` and enter Play Mode.

The project uses Unity's Input System and Universal Render Pipeline 2D.

## What I would do next

- Audio, haptics, particles, and additional animation polish
- Save/load and offline production progress
- A guided first-time-user experience
- More crops, recipes, animals, orders, and progression unlocks
- Storage upgrades and clearer inventory inspection
- Automated tests for inventory, placement, and production transactions

## Disclaimer

This is a non-commercial technical exercise inspired by *Hay Day*. *Hay Day* is owned by Supercell. No assets were extracted from the original game; the visual assets in this project were generated specifically for this challenge.

---

Created by **Jayben Bushnell** for the Voidpet Game Developer Challenge.
