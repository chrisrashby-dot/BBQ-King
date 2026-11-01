# BBQ King: Unity project

The game itself. Design decisions live in `../design.md`, working rules in `../CLAUDE.md` and the build log in `../PROGRESS.md`.

- **Engine:** Unity 6.6 (6000.6.7f1), 2D, Universal Render Pipeline (URP) with the 2D Renderer
- **Input:** Input System package, all actions in `Assets/_Project/Input/BBQKingControls.inputactions`
- **Milestone:** M1 Foundations (backyard, camera, player movement and interaction)

## Opening and running

1. Open **Unity Hub**, click **Add > Add project from disk** and choose this `BBQKing` folder.
2. Check the Editor version is **6000.6.7f1**, then open it. The first open takes a few minutes.
3. In the **Project** window, open `Assets/_Project/Scenes/Backyard.unity`.
4. In the **Game** view, set the resolution drop-down to **1920x1080** (see "Comparing resolutions").
5. Press **Play**.

### Controls in M1

| Action | Key |
| --- | --- |
| Walk | WASD or arrow keys (gamepad left stick or d-pad) |
| Use something nearby | E, when a prompt shows at the bottom of the screen (gamepad A) |
| Close a letter or note | E, Escape or right mouse button (gamepad B) |

Try reading the letter on the patio and using the back door. The other actions in design.md section 6.1 (tools, journal, pause, fast-forward) are already in the input asset, but nothing uses them until later milestones.

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
| `Data/Player/PlayerConfig` | Walking speed (3.5 tiles a second), walk animation speed, how far the player can reach to use things |
| `Data/Characters/PlayerSprites` | The walk frames for each direction |
| `Data/Messages/RelativeLetter`, `BackDoor` | The text shown when you use the letter and the door |
| `Data/Settings/DisplayConfig` | Reference resolution |

The starting values in PlayerConfig are first guesses, because design.md does not set them. Tune them by feel.

The letter text is a clearly marked placeholder, because the relative's name is still an open question in design.md.

## How it is organised

```
Assets/_Project/
  Art/Placeholder/   Generated PNGs, named after the design.md section 8 content list
  Art/Tiles/         Tile assets (each points at a PNG)
  Data/              ScriptableObject assets: settings, player, messages, event channels, input reader
  Editor/            Scene builder, placeholder art generator, pixel-art import settings
  Input/             BBQKingControls.inputactions (all controls)
  Prefabs/           Letter, BackDoor
  Scenes/            Backyard.unity
  Scripts/
    Core/            InputReader, DisplayConfig, camera scripts, event channels, MessageData
    Player/          Movement, walk animation, facing rules, interaction
    UI/              Pixel-perfect canvas scaling, interaction prompt, message panel
    (Restoration, Cleaning, Cooking, Fuel, Guests, Economy, Saving: empty until later milestones)
  Tests/EditMode/    Pure logic tests (facing, walk cycle)
  Tests/PlayMode/    Walkthrough of the Backyard scene
```

### How the pieces talk to each other

- **Input:** scripts never read the keyboard directly. They all share the `InputReader` asset, which turns `BBQKingControls` into simple values (`Move`) and events (`InteractPressed`). It has two action maps: **Player** for walking about, and **UI** while a panel is open, so opening a letter stops the player walking off.
- **Events:** systems post to **event channel** assets in `Data/Events` instead of holding references to each other. For example, `PlayerInteractor` posts the prompt text to `InteractionPromptChannel`, and `InteractionPromptUI` listens. Neither knows the other exists, so either can be swapped out.
- **Interactables:** anything with a component implementing `IInteractable` and a Collider2D can be used with E. `MessageInteractable` is the simplest kind: it posts a `MessageData` to `ShowMessageChannel`, and `MessagePanelUI` shows it.

## Rebuilding the scene

The scene, tiles, placeholder art, prefabs and data assets are created by `Assets/_Project/Editor/BackyardSceneBuilder.cs`. Run it from the menu bar with **BBQ King > Build Backyard Scene**.

- It **replaces** `Backyard.unity` (it asks first), so hand edits to the scene are lost.
- It **never overwrites** existing PNGs, data assets or prefabs, so replaced art and tuned values are safe.
- The map layout is in `Editor/BackyardLayout.cs`.

You only need it if the scene gets into a muddle, or after changing the layout in code. You can also paint tiles by hand with **Window > 2D > Tile Palette**.

## Replacing the placeholder art

Overwrite a PNG in `Assets/_Project/Art/Placeholder/` with your own file of the **same name and size**. The import settings and every tile, prefab and scene that uses it stay as they are.

| Files | Size (px) | Pivot | Notes |
| --- | --- | --- | --- |
| `Tiles/ground_*.png`, `wall_house.png` | 32 x 32 | Centre | One tile each |
| `Tiles/fence_intact.png`, `fence_broken.png` | 32 x 32 | Centre | Transparent background so the grass shows through |
| `Characters/player_walk_<direction>_<0-3>.png` | 32 x 32 | Bottom centre | 4 walk frames per direction; frame 0 is also the standing pose |
| `Objects/letter.png` | 32 x 32 | Centre | |
| `Objects/house_back_door.png` | 32 x 32 | Bottom centre | Transparent around the door so the wall shows through. The bottom pivot makes it sort in front of the wall and behind the player |
| `UI/ui_panel_dark.png`, `ui_panel_paper.png` | 12 x 12 | Centre | 9-sliced, 4 px border (set in the Sprite Editor) |

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
2. On the **EditMode** tab, click **Run All**. These check the facing and walk-cycle rules (14 tests).
3. On the **PlayMode** tab, click **Run All**. These load the Backyard and press keys on a pretend keyboard to check walking, the letter, the back door, fences and the resolution switch (5 tests).

## Git and Git LFS

The repository root is the folder above this one. PNG, WAV, OGG, PSD, Aseprite and font files are stored with **Git LFS** (see `../.gitattributes`), so install Git LFS (`git lfs install`) on any machine you clone to. The two art-style prototypes stay on disk beside this project but are not in Git.

## Steps to do by hand in the Unity Editor

1. **Add the project in Unity Hub** (see "Opening and running").
2. **Open the scene** `Assets/_Project/Scenes/Backyard.unity`. Unity may open an empty "Untitled" scene the first time.
3. **Set the Game view** to 1920x1080. If it is missing, click **+** at the bottom of the resolution drop-down and add a *Fixed Resolution* of 1920 x 1080.
4. If Unity offers to **import TMP Essentials**, you can close the dialog. The UI uses the built-in Text component for now.

The text uses Unity's built-in font, which is not a pixel font. Picking a pixel font is a later art task.
