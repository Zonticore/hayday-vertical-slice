# Hay Day Vertical Slice

A mobile-first recreation of the core *Hay Day* farming loop, built in Unity for the Voidpet Game Developer Challenge.

**Completed in eight working hours.** The goal was not to reproduce the entire game, but to ship a coherent miniature experience: grow resources, process them, care for animals, fulfill orders, and reinvest rewards into the farm.

## Submission

| Deliverable | Link |
| --- | --- |
| Playable build | **TODO — add build link** |
| Gameplay video | **TODO — add video link** |
| AI session transcript | **TODO — add transcript link** |

## The playable loop

1. Harvest mature wheat by dragging the sickle across farm patches.
2. Replant wheat and wait for it to grow.
3. Build a feed mill and turn wheat into chicken feed.
4. Buy chickens from the store and drag them into the chicken coop.
5. Feed chickens individually. Only fed chickens produce eggs, so the output reflects the resources invested.
6. Collect eggs and deliver wheat or egg orders through the request board.
7. Earn coins and XP, level up, and use the economy to expand the farm.

This creates a compact but connected economy:

```text
Wheat -> Chicken Feed -> Fed Chickens -> Eggs -> Orders -> Coins + XP
   |                                                        |
   +-------------------- Barn / Silo -----------------------+
```

## Features

### Farming and production

- Six starting farm patches with mature wheat
- Drag-to-harvest sickle interaction
- Crop planting, seed use, growth timers, and speed-up support
- Placeable farm patches and feed mill
- Feed-mill recipe that converts wheat into chicken feed
- Timed production with working, ready, and collection states

### Chickens

- Chickens purchased by dragging them from the store into the coop
- Individual feeding: one feed is consumed per chicken
- Production output based on the number of chickens actually fed
- Idle and producing chicken visuals
- Empty and filled trough states
- Egg-ready state and animated egg collection

### Building and world interaction

- Isometric grid with placement validation and multi-tile footprints
- Drag buildings out of the store and place them directly into the world
- Building costs deducted only after valid placement
- Camera panning, mouse-wheel zoom, and mobile pinch zoom
- Contextual actions positioned beside the selected world object
- Farm interaction remains modular across patches, production buildings, animals, and the request board

### Economy, progression, and UI

- Separate barn and silo inventories with independent capacities
- World-space barn and silo buildings plus temporary HUD destination displays
- Harvested items fly to the appropriate storage display before being awarded
- Harvest XP stars fly from the world to the progression bar
- Completed-order coins and XP fly from the request board to their HUD displays
- Coin and XP displays update and bounce when rewards are received
- XP thresholds and leveling
- Store sections for animals and buildings
- Request board with randomly selected wheat and egg orders
- Variable coin and XP rewards authored per order

## Controls

The project supports touch and mouse input.

- **Pan:** drag an empty part of the world
- **Zoom:** pinch on mobile or use the mouse wheel
- **Interact:** tap or click a tile/building
- **Harvest or plant:** press and drag the appropriate context action across farm patches
- **Place a building:** open the store, select Buildings, and drag an item out into the world
- **Buy a chicken:** open the Animals section and drag a chicken onto the coop
- **Close UI:** tap or click outside the open screen

## Running from source

1. Install Unity **6000.4.0f1** through Unity Hub.
2. Clone this repository.
3. Add the repository folder as a Unity project.
4. Open `Assets/Scenes/Main.unity`.
5. Enter Play Mode.

The project uses Unity's Input System and Universal Render Pipeline 2D.

## Scope and product decisions

I began by playing *Hay Day* and identifying the smallest set of systems that communicates its identity. The essential experience was not any single crop or building; it was the dependency chain between farming, production, animals, storage, orders, and progression.

I deliberately prioritized one complete loop over a larger collection of disconnected features. Systems such as the bakery, visitors, neighbor farms, roadside shop, storage upgrades, and broader crop variety were investigated but left outside the eight-hour slice.

The result is designed to feel expandable while still being demonstrable as a finished loop.

## Architecture

### Data-driven tile factories

World content is split between definitions and behavior:

```text
TileDefinitionSO
    -> selects a TileFactorySO
        -> creates the base TileInstance
            -> composes reusable interaction, state, visual, and production components
```

`TileDefinitionSO` owns identity, sprite, category, footprint, price, and factory selection. Each `TileFactorySO` builds the shared tile foundation and attaches only the behavior needed by that tile. Farm patches, chicken coops, request boards, feed mills, terrain, and storage buildings therefore share the same placement pipeline without becoming one large inheritance hierarchy.

This follows the Open/Closed Principle in a practical way: new content can normally be added through a definition, a focused factory, and reusable components rather than editing the core grid or placement systems.

### Reusable context actions

Interactive objects provide actions to a shared context-menu system. Planting, harvesting, feeding, collecting, producing, and speeding up are authored as small actions instead of being hard-coded into the UI controller.

Actions return their actual success state. This lets an option remain visible when useful—for example, attempting to feed a chicken without feed—without consuming resources or presenting a failed action as successful.

### ScriptableObject-authored content

ScriptableObjects are used for:

- Item definitions and storage destinations
- Tile definitions and factories
- Production recipes
- Store inventory and prices
- Request-board order pools and reward ranges
- Context actions and tools
- Tutorial/startup layout
- Starting player inventory, currency, capacity, and progression

This keeps balancing and content selection out of gameplay code and makes the slice easier to extend without introducing more hard-coded branches.

### Focused runtime systems

- `GridService` owns coordinate conversion and occupancy.
- `TileBuildService` validates and executes construction.
- `BuildingPlacementController` owns drag-to-place previews and purchases.
- `UserModel` owns currency, progression, barn storage, and silo storage.
- `UISystem` coordinates modal screens and outside-click dismissal.
- State and visual components remain separate for farm patches, production buildings, and chicken coops.

## AI-first development process

I used Codex aggressively inside the Unity project and ChatGPT image generation for the visual assets. My role was to establish the product scope and architecture, decompose work into bounded systems, give the agents constraints, and continuously evaluate the output.

The workflow was:

1. Play and analyze the reference game.
2. Rank the core loops and choose the eight-hour scope.
3. Define the grid, factory, context-action, and ScriptableObject architecture.
4. Ask Codex to implement focused pieces directly in the project.
5. Compile, play, inspect diffs, and iterate after each substantial change.
6. Generate visual assets with ChatGPT, import them, reject weak results, and regenerate or reconfigure them where needed.
7. Commit to GitHub throughout development so every agent-authored change remained reviewable.

AI output was treated as a draft, not as an authority. Examples of human-directed corrections included replacing hard-coded gameplay data with authored assets, fixing drag interactions that did not work with standard buttons, correcting generated transparency problems, changing chicken feeding from all-at-once to per-animal production, and separating the coop, trough, chicken, and egg visual states.

This approach let the agents work quickly while the codebase retained small responsibilities and explicit data ownership—both useful for agent context limits and for conventional team development.

## What I would do next

With another development pass, I would prioritize:

- Device testing and mobile performance profiling
- Audio, haptics, particles, and stronger transition polish
- A guided first-time-user experience
- Save/load and production progress across sessions
- More crops, recipes, animals, orders, and progression unlocks
- Storage upgrades and clearer inventory inspection
- Additional animation frames and interaction feedback
- Automated tests around inventory transactions, placement, and timed production

## Project structure

```text
Assets/
  Database/          ScriptableObject-authored items, recipes, orders, tools, and tiles
  Scenes/Main.unity  Playable vertical slice
  Scripts/
    ChickenCoop/     Per-animal feeding, production state, and visuals
    FarmPatch/       Crop state, growth, harvesting, and rewards
    Interaction/     Context actions, menus, and pointer interaction
    Production/      Shared timed-production behavior
    Tiles/           Grid, factories, placement, definitions, and runtime instances
    UI/              HUD, store, request board, and reward animations
    User/            Inventory, storage, currency, XP, and starting configuration
```

## Disclaimer

This is a non-commercial technical exercise inspired by *Hay Day*. *Hay Day* is owned by Supercell. The project does not contain assets extracted from the original game; its visual assets were generated specifically for this challenge and integrated during development.

---

Created by **Jayben Bushnell** for the Voidpet Game Developer Challenge.
