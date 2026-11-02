# BBQ King: Unity project

The game itself. Design decisions live in `../design.md`, working rules in `../CLAUDE.md` and the build log in `../PROGRESS.md`.

- **Engine:** Unity 6.6 (6000.6.7f1), 2D, Universal Render Pipeline (URP) with the 2D Renderer
- **Input:** Input System package, all actions in `Assets/_Project/Input/BBQKingControls.inputactions`
- **Milestone:** M3 Cleaning (the kettle grill close-up and Job 2), built on M2 Restoration and M1 Foundations

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
| Choose a tool | Automatic when pointing at a weed or trash; or 1 to 6, or the mouse wheel (gamepad shoulder buttons) |
| Use the tool | Left mouse button; hold to keep going (gamepad right trigger) |
| Jobs board | Tab (gamepad Select) |
| Close a letter, note or the jobs board | E, Escape, Tab or right mouse button (gamepad B) |
| Scrub in a close-up | Hold the left mouse button and move the mouse over the grime |
| Leave a close-up | Escape (gamepad B) |

Point the mouse at a weed or piece of trash next to the player (diagonals included, or on the tile they stand on): a highlight appears over it, the right tool is picked for you and its toolbar slot briefly grows so you can see the change. Clicking works on that tile. Empty tiles get no highlight, and neither do weeds and trash in the area of a job that is still locked (clicking one says "That can wait for now."). Clicking further away does nothing, so walk up to what you want to clear. You can still pick a tool by hand while pointing; it stays until you point at a different tile. With a gamepad, the same happens for the tile the player is facing.

### Playing Job 1 (Clear the patio)

1. Read the letter on the patio if you like, then press **Tab** to see the jobs board.
2. With the **gloves** (key 1), pull the weeds growing through the slabs. Small weeds take 1 pull, medium 2 and large (with a yellow flower) 3. Weedy slabs turn clean.
3. With the **trash bag** (key 2), pick up the trash bags, cans and broken pots (1 each).
4. The wrong tool shows a hint such as "Try the gloves." above the toolbar.
5. The top-left corner counts down what is left. When it reaches zero, "Patio cleared!" appears and the next job (Clean the kettle grill) becomes current.

The kettle grill's tile on the patio never has a weed or trash on it.

### Playing Job 2 (Clean the kettle grill)

1. Walk up to the kettle grill on the patio (to the right of the letter). Before Job 1 is done, **E** only says the patio comes first.
2. Once Job 1 is done, press **E** at the grill. The close-up opens over the backyard, with the grill grate covered in grime and the **wire brush** in hand.
3. Hold the **left mouse button** and move over the grime to scrub it away. The meter under the title shows how clean the grill is.
4. Press **Escape** to leave part way; the grime you scrubbed stays scrubbed when you come back (until you stop playing).
5. At **95% clean** the rest wipes away by itself, "Spotless!" shows, and the close-up closes after a moment. "Grill cleaned!" appears, Job 2 is done, and the grill on the patio looks clean.

With a gamepad you can open and leave the close-up, but there is no way to move the brush yet; that comes with controller support later.

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
| `Data/Player/PlayerConfig` | Walking speed (3.5 tiles a second), walk animation speed, how far the player can reach to use things, tool reach (1 tile around the player's tile), how far ahead the faced tile is with a gamepad (0.6 tiles), and the time between uses while the button is held (0.3 seconds) |
| `Data/Characters/PlayerSprites` | The walk frames for each direction |
| `Data/Messages/RelativeLetter`, `BackDoor`, `Job1Done`, `Job2Done` | The text shown for the letter, the door and finishing Jobs 1 and 2 |
| `Data/Settings/DisplayConfig` | Reference resolution |
| `Data/Settings/UiConfig` | How long hints show (2 seconds) and fade (0.5 seconds), and how much (1.3 times) and how long (0.25 seconds) a toolbar slot grows when the tool changes |
| `Data/Tools/Gloves`, `TrashBag`, `WireBrush` | Each tool's name and toolbar icon (the wire brush is used in the close-up) |
| `Data/Tools/BackyardTools` | Which tools are on the toolbar, in slot order (up to 6) |
| `Data/Restoration/Clearables/*` | For each kind of weed or trash: its tile, the tool that clears it, how many uses it takes (1 to 3) and the ground left behind |
| `Data/Restoration/ClearingConfig` | The list of clearables, the hint texts, how much a weed shrinks with each pull, and how many patio slabs start weedy (45%) and how many of the rest have trash (15%). The last two only take effect when the scene is rebuilt |
| `Data/Jobs/Job1_ClearThePatio` and the others | Each job's title, description, the jobs it needs first, its clearing area and its completion message |
| `Data/Jobs/JobBook` | Which jobs exist, in jobs board order |
| `Data/Cleaning/CleaningConfig` | The brush's radius (6 pixels of close-up art), how fast it scrubs (3, so a fully grimy spot clears in a third of a second), the finishing line (95%, from design.md 5.2) and the pause before the close-up closes (1.5 seconds) |
| `Data/Cleaning/KettleGrill` | The kettle grill's job (Job 2), tool (wire brush), close-up art and grime layer, and its prompts and "not yet" hint |

The starting values in PlayerConfig, the clearables and the cleaning brush are first guesses, because design.md does not set them. Tune them by feel.

Wording on the HUD and jobs board ("To do", "Locked", "{0} left") is set on the **Job HUD** and **Jobs Board** objects in the scene, and the close-up meter's wording ("{0}% clean", "Spotless!") on the **Close-up** object in `CloseUp_Cleaning`. Rebuilding the scene resets those, so change them in the builder if you want to keep them.

The letter text is a clearly marked placeholder, because the relative's name is still an open question in design.md.

## How it is organised

```
Assets/_Project/
  Art/Placeholder/   Generated PNGs, named after the design.md section 8 content list
  Art/Tiles/         Tile assets (each points at a PNG)
  Data/              ScriptableObject assets: settings, player, messages, event channels, input reader,
                     tools, clearables, jobs, cleaning
  Editor/            Scene builders (BackyardSceneBuilder, RestorationBuilder, CleaningBuilder),
                     placeholder art generator, pixel-art import settings, shared builder helpers
  Input/             BBQKingControls.inputactions (all controls)
  Prefabs/           Letter, BackDoor, KettleGrill
  Scenes/            Backyard.unity, CloseUp_Cleaning.unity (opened on top of the backyard)
  Scripts/
    Core/            InputReader, DisplayConfig, camera scripts, event channels, MessageData
    Player/          Movement, walk animation, facing rules, interaction, tool belt, tool aiming and use
    Restoration/     Tools, clearables, the clearing system, jobs and the job tracker
    Cleaning/        Cleanable objects, the close-up loader and session, grime and scrubbing
    UI/              Canvas scaling, prompt, message panel, toolbar, hint, job HUD, jobs board
    (Cooking, Fuel, Guests, Economy, Saving: empty until later milestones)
  Tests/EditMode/    Pure logic tests (facing, walk cycle, tool slots, tool aiming, clearing, jobs, grime)
  Tests/PlayMode/    The Backyard scene with pretend keyboard and mouse presses
```

### How the pieces talk to each other

- **Input:** scripts never read the keyboard directly. They all share the `InputReader` asset, which turns `BBQKingControls` into simple values (`Move`) and events (`InteractPressed`). It has three action maps: **Player** for walking about, **UI** while a panel is open, so opening a letter stops the player walking off, and **CloseUp** while a close-up is open (scrub, aim and leave). Only one is on at a time: a panel wins over a close-up, which wins over walking. Panels call `OpenPanel` and `ClosePanel`, which count how many are open, so walking only comes back when the last one closes. Moving the mouse makes it the aiming device; using a gamepad hands aiming back to the tile the player faces.
- **Events:** systems post to **event channel** assets in `Data/Events` instead of holding references to each other. For example, `PlayerInteractor` posts the prompt text to `InteractionPromptChannel`, and `InteractionPromptUI` listens. Neither knows the other exists, so either can be swapped out.
- **Interactables:** anything with a component implementing `IInteractable` and a Collider2D can be used with E. `MessageInteractable` is the simplest kind: it posts a `MessageData` to `ShowMessageChannel`, and `MessagePanelUI` shows it.
- **Tools and clearing:** the player's `ToolBelt` posts the selected slot to `SelectedToolSlotChannel` (the toolbar listens). `ToolUser` picks the target tile and posts each use to `ToolUseChannel`. The `ClearingSystem` (on the **Restoration** object) hears it, checks the tool against the tile's `ClearableData`, clears the tile and posts to `TileClearedChannel`; wrong tools post a hint to `HintChannel`.
- **Jobs:** the `JobBook` asset holds every job and how far along it is, shared by everyone who needs it. The `JobTracker` counts each clearing job's tiles at the start, counts down on `TileClearedChannel`, and shows the completion message. The job HUD and jobs board redraw whenever the job book changes. Progress is not saved yet (M5).
- **Cleaning close-ups:** the kettle grill's `CleanableObject` asks for its close-up on the `CloseUpSession` asset. The `CloseUpLoader` (on the **Cleaning** object in the backyard) hears it, hides the backyard UI, switches the controls to the CloseUp map and loads `CloseUp_Cleaning` **additively**, on top of the backyard. That scene's own camera draws over the backyard camera; its art sits far away at (1000, 1000) so neither camera sees the other's objects. `CleaningCloseUp` turns the grime image into a `GrimeMask`, scrubs it under the mouse and shows the meter. At 95% it asks the session to close as finished; the loader unloads the scene, gives walking back and marks the job done, which shows its completion message. The session keeps each object's grime, so leaving part way loses nothing.

## Rebuilding the scene

The scene, tiles, placeholder art, prefabs and data assets are created by `Assets/_Project/Editor/BackyardSceneBuilder.cs`, with the M2 parts (tools, clearing, jobs, toolbar, HUD, jobs board) in `RestorationBuilder.cs` and the M3 parts (the kettle grill, the close-up loader and the `CloseUp_Cleaning` scene) in `CleaningBuilder.cs`. Run it from the menu bar with **BBQ King > Build Backyard Scene**.

- It **replaces** `Backyard.unity` and `CloseUp_Cleaning.unity` (it asks first), so hand edits to the scenes are lost. It also puts both scenes in the build (File > Build Profiles, Scene List), which the close-up needs in order to load.
- It **never overwrites** existing PNGs, data assets or prefabs, so replaced art and tuned values are safe.
- The map layout is in `Editor/BackyardLayout.cs`. How many patio slabs start weedy or with trash is in `Data/Restoration/ClearingConfig`. The weeds and trash are placed the same way every time.

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
| `Overgrowth/trash_bag.png`, `rubbish_cans.png`, `rubbish_pot_broken.png` | 32 x 32 | Centre | Transparent so the slab shows |
| `Tools/tool_gloves.png`, `tool_trash_bag.png`, `tool_wire_brush.png` | 32 x 32 | Centre | Toolbar icons |
| `Equipment/kettle_grill_dirty.png`, `kettle_grill_clean.png` | 32 x 32 | Bottom centre | The grill on the patio, before and after Job 2 |
| `CloseUps/closeup_kettle_grill_grate.png` | 128 x 128 | Centre | The clean grill seen from above in the close-up, drawn 3 times bigger |
| `CloseUps/closeup_kettle_grill_grate_grime.png` | 128 x 128 | Centre | The grime over it. See-through parts start clean and do not count towards the meter. **Read/Write** must stay on in its import settings, because the game wipes it pixel by pixel |
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
2. On the **EditMode** tab, click **Run All**. These check the facing and walk-cycle rules, toolbar slots, tool aiming, clearing counts, job statuses and the grime maths, including the 95% rule (48 tests).
3. On the **PlayMode** tab, click **Run All**. These load the Backyard and press keys and mouse buttons on a pretend keyboard and mouse (22 tests):
   - walking, the letter, the back door, fences (including standing behind the bottom fence) and the resolution switch;
   - pulling a weed, nothing happening out of reach, the highlight only over things to clear, the trash bag being picked automatically, and the hint for a wrong tool chosen by hand;
   - the job HUD count, the jobs board on Tab, and finishing Job 1;
   - stacked panels and handing aiming to a gamepad;
   - the kettle grill blocking the player, its "not yet" hint, opening and leaving the close-up, scrubbing and keeping progress, and scrubbing to 95% to finish Job 2.

## Git and Git LFS

The repository root is the folder above this one. PNG, WAV, OGG, PSD, Aseprite and font files are stored with **Git LFS** (see `../.gitattributes`), so install Git LFS (`git lfs install`) on any machine you clone to. The two art-style prototypes stay on disk beside this project but are not in Git.

## Steps to do by hand in the Unity Editor

1. **Add the project in Unity Hub** (see "Opening and running").
2. **Open the scene** `Assets/_Project/Scenes/Backyard.unity`. Unity may open an empty "Untitled" scene the first time.
3. **Set the Game view** to 1920x1080. If it is missing, click **+** at the bottom of the resolution drop-down and add a *Fixed Resolution* of 1920 x 1080.
4. If Unity offers to **import TMP Essentials**, you can close the dialog. The UI uses the built-in Text component for now.

The text uses Unity's built-in font, which is not a pixel font. Picking a pixel font is a later art task.
