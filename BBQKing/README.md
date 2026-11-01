# BBQ King: Unity project

The game itself. Design decisions live in `../design.md`, working rules in `../CLAUDE.md` and the build log in `../PROGRESS.md`.

- **Engine:** Unity 6.6 (6000.6.7f1), 2D, Universal Render Pipeline (URP) with the 2D Renderer
- **Input:** Input System package, all actions in `Assets/_Project/Input/BBQKingControls.inputactions`
- **Milestone:** M2 Restoration (tools, clearing weeds and trash, Job 1 and the jobs board), built on M1 Foundations

## Opening and running

1. Open **Unity Hub**, click **Add > Add project from disk** and choose this `BBQKing` folder.
2. Check the Editor version is **6000.6.7f1**, then open it. The first open takes a few minutes.
3. In the **Project** window, open `Assets/_Project/Scenes/Backyard.unity`.
4. In the **Game** view, set the resolution drop-down to **1920x1080** (see "Comparing resolutions").
5. Press **Play**.

### Controls

| Action | Key |
| --- | --- |
| Walk | WASD or arrow keys (gamepad left stick or d-pad) |
| Use something nearby | E, when a prompt shows at the bottom of the screen (gamepad A) |
| Choose a tool | 1 to 6, or the mouse wheel (gamepad shoulder buttons) |
| Use the tool | Left mouse button; hold to keep going (gamepad right trigger) |
| Jobs board | Tab (gamepad Select) |
| Close a letter, note or the jobs board | E, Escape, Tab or right mouse button (gamepad B) |

The tool works on the highlighted tile: the tile under the mouse if it is within reach (1.5 tiles of the player's feet), otherwise the tile the player is facing.

### Playing Job 1 (Clear the patio)

1. Read the letter on the patio if you like, then press **Tab** to see the jobs board.
2. With the **gloves** (key 1), pull the weeds growing through the slabs. Small weeds take 1 pull, medium 2 and large (with a yellow flower) 3. Weedy slabs turn clean.
3. With the **trash bag** (key 2), pick up the trash bags, cans and broken pots (1 each).
4. The wrong tool shows a hint such as "Try the gloves." above the toolbar.
5. The top-left corner counts down what is left. When it reaches zero, "Patio cleared!" appears and the next job (Clean the kettle grill) becomes current. That job is built in M3.

The pause and fast-forward actions in design.md section 6.1 are already in the input asset, but nothing uses them until later milestones.

## Comparing resolutions

The game uses a **960x540** reference resolution, chosen after testing on 1 Nov 2026. 640x360 and 1280x720 stay available for comparison and testing. To switch:

1. In the Project window, select `Assets/_Project/Data/Settings/DisplayConfig`.
2. In the Inspector, change **Reference Resolution**. This works during Play mode, so you can walk around and compare.

Changes to this asset are **kept** when you press Stop, so set it back to 960 x 540 before you finish.

| Reference | Tiles visible | On a 1920x1080 window | On 2560x1440 |
| --- | --- | --- | --- |
| 640x360 | 20 x 11 | x3, fills the screen | x4, fills the screen |
| 960x540 (chosen) | 30 x 17 | x2, fills the screen | x2, black border |
| 1280x720 | 40 x 22 | x1, black border | x2, fills the screen |

The game only ever scales by whole numbers so pixels stay sharp. When the window is not an exact multiple, you get a border instead of blurry pixels.

## Tuning without code

All gameplay values live in assets under `Assets/_Project/Data/`, not in scripts. Select one and edit it in the Inspector. Changes to assets are kept after Play mode, unlike changes to objects in the scene.

| Asset | What it controls |
| --- | --- |
| `Data/Player/PlayerConfig` | Walking speed (3.5 tiles a second), walk animation speed, how far the player can reach to use things, tool reach (1.5 tiles), how far ahead the faced tile is (0.6 tiles), and the time between uses while the button is held (0.3 seconds) |
| `Data/Characters/PlayerSprites` | The walk frames for each direction |
| `Data/Messages/RelativeLetter`, `BackDoor`, `Job1Done` | The text shown for the letter, the door and finishing Job 1 |
| `Data/Settings/DisplayConfig` | Reference resolution |
| `Data/Tools/Gloves`, `TrashBag` | Each tool's name and toolbar icon |
| `Data/Tools/BackyardTools` | Which tools are on the toolbar, in slot order (up to 6) |
| `Data/Restoration/Clearables/*` | For each kind of weed or trash: its tile, the tool that clears it, how many uses it takes (1 to 3) and the ground left behind |
| `Data/Restoration/ClearingConfig` | The list of clearables, the hint texts and how much a weed shrinks with each pull |
| `Data/Jobs/Job1_ClearThePatio` and the others | Each job's title, description, the jobs it needs first, its clearing area and its completion message |
| `Data/Jobs/JobBook` | Which jobs exist, in jobs board order |

The starting values in PlayerConfig and the clearables are first guesses, because design.md does not set them. Tune them by feel.

Wording on the HUD and jobs board ("To do", "Locked", "{0} left") is set on the **Job HUD** and **Jobs Board** objects in the scene, and the hint timing on the **Hint** object. Rebuilding the scene resets those, so change them in the builder if you want to keep them.

The letter text is a clearly marked placeholder, because the relative's name is still an open question in design.md.

## How it is organised

```
Assets/_Project/
  Art/Placeholder/   Generated PNGs, named after the design.md section 8 content list
  Art/Tiles/         Tile assets (each points at a PNG)
  Data/              ScriptableObject assets: settings, player, messages, event channels, input reader,
                     tools, clearables, jobs
  Editor/            Scene builders (BackyardSceneBuilder, RestorationBuilder), placeholder art
                     generator, pixel-art import settings, shared builder helpers
  Input/             BBQKingControls.inputactions (all controls)
  Prefabs/           Letter, BackDoor
  Scenes/            Backyard.unity
  Scripts/
    Core/            InputReader, DisplayConfig, camera scripts, event channels, MessageData
    Player/          Movement, walk animation, facing rules, interaction, tool belt, tool aiming and use
    Restoration/     Tools, clearables, the clearing system, jobs and the job tracker
    UI/              Canvas scaling, prompt, message panel, toolbar, hint, job HUD, jobs board
    (Cleaning, Cooking, Fuel, Guests, Economy, Saving: empty until later milestones)
  Tests/EditMode/    Pure logic tests (facing, walk cycle, tool slots, tool aiming, clearing, jobs)
  Tests/PlayMode/    The Backyard scene with pretend keyboard and mouse presses
```

### How the pieces talk to each other

- **Input:** scripts never read the keyboard directly. They all share the `InputReader` asset, which turns `BBQKingControls` into simple values (`Move`) and events (`InteractPressed`). It has two action maps: **Player** for walking about, and **UI** while a panel is open, so opening a letter stops the player walking off.
- **Events:** systems post to **event channel** assets in `Data/Events` instead of holding references to each other. For example, `PlayerInteractor` posts the prompt text to `InteractionPromptChannel`, and `InteractionPromptUI` listens. Neither knows the other exists, so either can be swapped out.
- **Interactables:** anything with a component implementing `IInteractable` and a Collider2D can be used with E. `MessageInteractable` is the simplest kind: it posts a `MessageData` to `ShowMessageChannel`, and `MessagePanelUI` shows it.
- **Tools and clearing:** the player's `ToolBelt` posts the selected slot to `SelectedToolSlotChannel` (the toolbar listens). `ToolUser` picks the target tile and posts each use to `ToolUseChannel`. The `ClearingSystem` (on the **Restoration** object) hears it, checks the tool against the tile's `ClearableData`, clears the tile and posts to `TileClearedChannel`; wrong tools post a hint to `HintChannel`.
- **Jobs:** the `JobBook` asset holds every job and how far along it is, shared by everyone who needs it. The `JobTracker` counts each clearing job's tiles at the start, counts down on `TileClearedChannel`, and shows the completion message. The job HUD and jobs board redraw whenever the job book changes. Progress is not saved yet (M5).

## Rebuilding the scene

The scene, tiles, placeholder art, prefabs and data assets are created by `Assets/_Project/Editor/BackyardSceneBuilder.cs`, with the M2 parts (tools, clearing, jobs, toolbar, HUD, jobs board) in `RestorationBuilder.cs`. Run it from the menu bar with **BBQ King > Build Backyard Scene**.

- It **replaces** `Backyard.unity` (it asks first), so hand edits to the scene are lost.
- It **never overwrites** existing PNGs, data assets or prefabs, so replaced art and tuned values are safe.
- The map layout is in `Editor/BackyardLayout.cs`, including how many patio slabs start weedy (45%) and how many of the rest have trash (15%). The weeds and trash are placed the same way every time.

You only need it if the scene gets into a muddle, or after changing the layout in code. You can also paint tiles by hand with **Window > 2D > Tile Palette**.

Fence tiles are only pictures. The fence colliders are separate boxes on the **Walls and Fences** object: the side fences block their whole tile, while the fences across the top and bottom block only a strip at the bottom of the tile, so the player can step behind them. A fence you paint by hand will not block the player unless you add a Box Collider 2D for it (set its **Composite Operation** to **Merge**).

## Replacing the placeholder art

Overwrite a PNG in `Assets/_Project/Art/Placeholder/` with your own file of the **same name and size**. The import settings and every tile, prefab and scene that uses it stay as they are.

| Files | Size (px) | Pivot | Notes |
| --- | --- | --- | --- |
| `Tiles/ground_*.png`, `wall_house.png` | 32 x 32 | Centre | One tile each |
| `Tiles/fence_intact.png`, `fence_broken.png` | 32 x 32 | Centre | Transparent background so the grass shows through |
| `Characters/player_walk_<direction>_<0-3>.png` | 32 x 32 | Bottom centre | 4 walk frames per direction; frame 0 is also the standing pose |
| `Objects/letter.png` | 32 x 32 | Centre | |
| `Objects/house_back_door.png` | 32 x 32 | Bottom centre | Transparent around the door so the wall shows through. The bottom pivot makes it sort in front of the wall and behind the player |
| `Overgrowth/weed_small.png`, `weed_medium.png`, `weed_large.png` | 32 x 32 | Centre | Transparent so the slab shows. Bigger weeds take more pulls, so make the sizes easy to tell apart |
| `Overgrowth/rubbish_bag.png`, `rubbish_cans.png`, `rubbish_pot_broken.png` | 32 x 32 | Centre | Transparent so the slab shows |
| `Tools/tool_gloves.png`, `tool_trash_bag.png` | 32 x 32 | Centre | Toolbar icons |
| `UI/ui_panel_dark.png`, `ui_panel_paper.png`, `ui_slot.png`, `ui_slot_selected.png` | 12 x 12 | Centre | 9-sliced, 4 px border (set in the Sprite Editor) |
| `UI/ui_tile_highlight.png` | 32 x 32 | Centre | Drawn over the target tile in the world; keep the middle mostly see-through |

New images dropped anywhere under `Assets/_Project/Art/` get the pixel-art import settings automatically (`Editor/PixelArtImportSettings.cs`).

### Sprite import settings

If you ever set them by hand: select the image, set these in the Inspector, then click **Apply**.

| Setting | Value | Why |
| --- | --- | --- |
| Texture Type | Sprite (2D and UI) | |
| Pixels Per Unit | **32** | 32 pixels = 1 tile. Must match the Pixel Perfect Camera |
| Mesh Type | Full Rect | Avoids edge artefacts |
| Filter Mode | **Point (no filter)** | Stops Unity blurring pixels |
| Compression | **None** | Compression smears colours |
| Generate Mip Maps | Off | Mipmaps blur pixel art |

## Running the tests

1. Open **Window > General > Test Runner**.
2. On the **EditMode** tab, click **Run All**. These check the facing and walk-cycle rules, toolbar slots, tool aiming, clearing counts and job statuses (36 tests).
3. On the **PlayMode** tab, click **Run All**. These load the Backyard and press keys and mouse buttons on a pretend keyboard and mouse (11 tests):
   - walking, the letter, the back door, fences (including standing behind the bottom fence) and the resolution switch;
   - pulling a weed with the gloves, and the hint for the wrong tool;
   - the job HUD count, the jobs board on Tab, and finishing Job 1.

## Git and Git LFS

The repository root is the folder above this one. PNG, WAV, OGG, PSD, Aseprite and font files are stored with **Git LFS** (see `../.gitattributes`), so install Git LFS (`git lfs install`) on any machine you clone to. The two art-style prototypes stay on disk beside this project but are not in Git.

## Steps to do by hand in the Unity Editor

1. **Add the project in Unity Hub** (see "Opening and running").
2. **Open the scene** `Assets/_Project/Scenes/Backyard.unity`. Unity may open an empty "Untitled" scene the first time.
3. **Set the Game view** to 1920x1080. If it is missing, click **+** at the bottom of the resolution drop-down and add a *Fixed Resolution* of 1920 x 1080.
4. If Unity offers to **import TMP Essentials**, you can close the dialog. The UI uses the built-in Text component for now.

The text uses Unity's built-in font, which is not a pixel font. Picking a pixel font is a later art task.
