# CLAUDE.md: BBQ King

Guidance for Claude Code when working in this repository. Read this file and `design.md` at the start of every session.

## What this project is

BBQ King is a cosy, single-player 2D pixel art game for PC (Steam), built in Unity 6 with C#. The player inherits a late relative's house, restores its overgrown American backyard, hosts barbecues, and later runs a food truck and enters a barbecue championship.

The developer knows C# but is new to Unity. Explain Unity-specific steps clearly and comment code so it can be followed.

## Source of truth

- `design.md` is the game design document. It defines what to build, the systems, starting values, art rules and technical decisions.
- Do not change a design decision on your own. If something in `design.md` seems wrong, unclear or impossible, stop and ask, then suggest the change.
- If a design decision changes during a session, tell the developer so they can update the design document (the master copy lives outside this repo and `design.md` is regenerated from it). Do not edit `design.md` yourself unless asked.
- The current build target is **Act 1** (section 4 of `design.md`). Do not build Act 2 or Act 3 features unless asked.

## Repository layout

```
/                     Repository root
├── CLAUDE.md         This file
├── design.md         Game design document (read only unless asked)
├── PROGRESS.md       Build log and milestone status (keep updated)
└── BBQKing/          Unity project
    └── Assets/_Project/
        ├── Art/          Sprites, tiles, close-up art (placeholders until replaced)
        ├── Audio/        Sound effects and music
        ├── Data/         ScriptableObject assets (recipes, items, jobs, events, shop stock)
        ├── Input/        Input System actions asset
        ├── Prefabs/
        ├── Scenes/       Title, Backyard, Town, CloseUp_Cleaning, CloseUp_Cooking
        ├── Scripts/
        │   ├── Core/         Game state, events bus, weekend and clock
        │   ├── Player/       Movement and interaction
        │   ├── Restoration/  Tile clearing and repairs
        │   ├── Cleaning/     Close-up grime scrubbing
        │   ├── Cooking/      Grill, smoker, heat, food states
        │   ├── Fuel/
        │   ├── Guests/       Satisfaction and ratings
        │   ├── Economy/      Money, reputation, shops
        │   ├── Saving/
        │   └── UI/
        └── Tests/        EditMode and PlayMode tests
```

The earlier `BBQ-Prototype-2D` project is a reference. Reuse its movement and cleaning scripts where they fit, moving them into the structure above.

If the repository does not yet match this layout, propose the setup first and wait for approval before creating it.

## Technical rules

- Unity 6, 2D project, Universal Render Pipeline with the 2D renderer.
- Pixel Perfect Camera, reference resolution 960x540 (chosen after testing; keep it easy to switch to 640x360 or 1280x720 for testing), 32 pixels per unit, integer scaling.
- Sprite import settings: Filter Mode Point, Compression None, Pixels Per Unit 32.
- Tilemap for ground, overgrowth and fences; interactable objects as prefabs.
- Input System package; all actions in one input asset. Keyboard and mouse first, but keep controller support possible.
- No gameplay numbers in scripts. Prices, temperatures, timings, portions and thresholds live in ScriptableObjects under `Data/`.
- Systems communicate through events where practical, not tight references.
- Saving uses JSON in `Application.persistentDataPath`.
- Temperatures are stored in °F internally; display converts to °C if the player chooses.

## Coding style

- C# with clear names; one class per file; namespaces per system (for example `BBQKing.Cooking`).
- Comment the why, not just the what, and explain Unity concepts the first time they appear.
- Prefer small, readable scripts over clever ones.
- Add EditMode tests for pure logic (satisfaction scoring, portions, reputation, money) where practical.

## Language

Use UK English in comments, documentation, commit messages and in-game text, except where the American setting calls for American terms in the game itself (backyard, block party, yard sale, hardware store, °F).

## How to work

1. At the start of a session, read `CLAUDE.md`, `design.md` and `PROGRESS.md`, then state which milestone you are on and what you plan to do.
2. Work in small, testable steps. After each step, explain how to test it in the Unity Editor.
3. You cannot click around in the Unity Editor. When a step needs manual Editor work (assigning references, creating assets from menus, setting up scenes), list it clearly as numbered steps for the developer.
4. Ask before adding third-party packages or asset store assets.
5. Commit after each completed step with a clear message.
6. At the end of a session, update `PROGRESS.md`: what was done, what is next, any problems or questions for the developer.

## Act 1 milestones

Build in this order. Each milestone should be playable before moving on.

| Milestone | Goal |
| --- | --- |
| M1 Foundations | Project setup, folder structure, pixel-perfect camera, backyard tilemap with placeholder tiles, player movement and interaction |
| M2 Restoration | Tile clearing with tools; Job 1 (clear the patio) playable; jobs board |
| M3 Cleaning | Close-up scrubbing scene; Job 2 (clean the kettle grill) playable; 95% completion rule |
| M4 First cookout | Event brief, menu planner, shops, prep, kettle grill cooking, guest satisfaction, event summary; Event 1 playable end to end |
| M5 Economy and saving | Money, reputation, family fund safety net, autosave and manual save |
| M6 Smoker | Jobs 3 to 5, offset smoker, wood fuel and wet wood, check-ins and fast-forward |
| M7 Act 1 events | Events 2 to 4, recipe cards, drizzle and gazebo, block party finale and food truck unlock |
| M8 Polish and playtest | Settings, accessibility options, audio hooks, bug fixing, a playtest build |

## Art rules (for placeholders and imports)

- Everything is on a 32x32 grid; larger items in multiples of 32.
- Three-quarter top-down view, light from the top left.
- Placeholders should be simple shapes in the right size and colour, clearly named so they can be replaced one for one.
- Never use assets copied from other games.
