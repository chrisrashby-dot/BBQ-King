# BBQ King: Progress log

Claude Code updates this file at the end of every session. Newest entry at the top.

## Milestone status

| Milestone | Status |
| --- | --- |
| M1 Foundations | Done (play-tested 1 Nov 2026) |
| M2 Restoration | Done (play-tested 2 Nov 2026) |
| M3 Cleaning | Built 2 Nov 2026; not yet play-tested by hand |
| M4 First cookout | Not started |
| M5 Economy and saving | Not started |
| M6 Smoker | Not started |
| M7 Act 1 events | Not started |
| M8 Polish and playtest | Not started |

## Session log

### 2 Nov 2026: new laptop, M2 signed off

- Moved to a new laptop. The repository was cloned fresh, with Git LFS and Unity 6000.6.7f1 at the same path as before.
- Batch-mode tests on the new laptop: 38 EditMode and 14 PlayMode tests pass, with no compiler errors or warnings.
- The developer play-tested the M2 changes by hand (tools only next to the player, the highlight only over a weed or trash, the automatic tool choice with the growing toolbar slot). They all work, so **M2 is done**.
- **Code review fixes** (one commit, all nine findings from a review of `Assets/_Project`):
  1. Using a gamepad now hands aiming back to the faced tile. Before, once the mouse had moved, the gamepad never aimed again, and this carried over between Play sessions.
  2. A clearing job that has nothing left when it unlocks now finishes straight away, instead of blocking every job after it.
  3. Panels now call `InputReader.OpenPanel` and `ClosePanel`, which count open panels, so walking only comes back when the last one closes. M3 builds on this: the input reader now also knows when a close-up is open.
  4. Weeds and trash in a locked job's area are no longer highlighted and no longer switch the tool; clicking one still shows "That can wait for now." This only matters once a later job has a clearing area.
  5. The patio weed and trash shares moved from `BackyardLayout.cs` into `ClearingConfig` (CLAUDE.md: no gameplay numbers in scripts).
  6. The hint and toolbar pulse timings moved from scene objects into a new `Data/Settings/UiConfig` asset, so rebuilding the scene keeps them.
  7. The interaction prompt is only rebuilt when it changes, not every frame.
  8. The jobs board only rebuilds its list while it is open.
  9. An empty toolbar no longer marks slot 1 as selected.
- New tests: an empty job finishing when it unlocks, an empty toolbar (EditMode); stacked panels, gamepad aiming (PlayMode). One existing PlayMode test relied on the mouse position left over from the test before and now moves the mouse first. 40 EditMode and 16 PlayMode tests pass in batch mode.

### 2 Nov 2026: M3 Cleaning

The developer agreed the four M3 answers (recorded in design.md section 5.2 and its decision log): the wire brush is Job 2's only tool; leaving a close-up keeps its progress for the rest of the play (saving it waits for M5); the grate close-up is 128 x 128 pixels drawn 3 times bigger; and the cleaning code is written fresh, because the `BBQ-Prototype-2D` project is not on this laptop (it was never in Git).

Done (one commit per step):

1. **Kettle grill:** the relative's grill stands on the patio at tile (19, 19), which is kept free of weeds and trash. It is solid at its feet and Y-sorted with the player. Before Job 1 is done, E shows "It needs a good scrub, but the patio comes first."; afterwards E asks for its close-up. Data in `Data/Cleaning/KettleGrill`; new placeholder art for the grill (dirty and clean), the wire brush icon, and the close-up grate with its grime layer.
2. **Close-up scene:** `CloseUp_Cleaning` loads on top of the backyard (design.md 9.2) with its own camera, which draws over the backyard camera, and its own UI (title, the wire brush in a tool slot, help line). The backyard HUD and toolbar hide while it is open. A new **CloseUp** input map (scrub, aim, leave); the input reader keeps one map on at a time. Escape leaves. The builder now makes both scenes and puts both in the build.
3. **Scrubbing:** holding the left mouse button wipes grime away under a round brush. The grime is a `GrimeMask` built from the grime image's opacity and drawn through a texture made at run time. Brush size and speed are in `Data/Cleaning/CleaningConfig`. Leaving keeps the progress.
4. **Meter and the 95% rule:** a meter under the title ("37% clean"). At 95% the rest wipes away, "Spotless!" shows for 1.5 seconds, the close-up closes, Job 2 is done, "Grill cleaned!" shows and the grill on the patio looks clean.
5. **Tests and docs:** EditMode tests for the grime maths and the 95% rule; PlayMode tests for the grill, opening and leaving the close-up, scrubbing and keeping progress, and scrubbing to finished. README updated.

Checked in batch mode: everything compiles with no errors or warnings from our code; 48 EditMode and 22 PlayMode tests pass; screenshots of the grill on the patio and the close-up part way through (with the meter) look as expected. **Not yet checked by hand in the Editor.**

Later the same day, at the developer's request: both grill placeholders were drawn side-on or straight down, which broke the three-quarter view rule (design.md 7.1 and 7.2). They are redrawn: the patio grill shows the top of its lid with the handle on it, and the close-up shows the grate as a wide oval with the front of the bowl below. The grime layer follows the new shape. Same file names, sizes and GUIDs; all tests still pass.

Later again, at the developer's request (design.md 5.2 and decision log updated): **sections and rewards.** The kettle grill is cleaned in two sections, each with its own meter: the **top** (grate and inside of the bowl) and the **bottom** (outside of the bowl). Each section finishes at 95% with a sparkle over it and a chime; when both are done, a bigger sparkle with a white flash and a triple chime plays, then the close-up closes. New: `CleaningSection`, `SectionedGrime` (the rules, with EditMode tests), `GrimeLayer` (one section's picture), `CloseUpEffects` (sparkles, flash, chimes), a bottom grime layer, 4 sparkle frames and 2 placeholder chimes made in code (`Editor/PlaceholderAudio.cs`, WAV files in `Audio/Placeholder`). The existing kettle grill and cleaning config assets were given the new sections and chimes by the builder. 51 EditMode and 22 PlayMode tests pass; screenshots show the two meters and the final flash with sparkles. **Not yet checked by hand in the Editor, and the chimes have not been listened to** (batch mode has no speakers).

Next: play-test M3 in the Editor (README, "Playing Job 2"), then **M4 First cookout** (event brief, menu planner, shops, prep, kettle grill cooking, guest satisfaction, event summary).

### 1 Nov 2026: M2 Restoration

Done (one commit per step):

1. **Tools and toolbar:** gloves and trash bag as tool assets (`Data/Tools`), in slot order in `BackyardTools`. Keys 1 to 6 or the mouse wheel pick a tool; the toolbar along the bottom shows the icons, the selected slot and the tool's name. The builder's shared helpers moved into `BuilderUtil` and `UiFactory`, and the M2 parts into `RestorationBuilder`.
2. **Aiming:** a new **Aim** action (mouse position). The tool works on the tile under the mouse if it is within 1.5 tiles of the player's feet, otherwise on the tile the player faces; a highlight marks it. Tab now also closes panels.
3. **Clearing:** weeds in three sizes (1, 2 or 3 pulls with the gloves) on every weedy patio slab, and trash (bag, cans, broken pot: one grab with the trash bag) on some clean slabs. Holding the button repeats every 0.3 seconds. Big weeds shrink as they come loose, weedy slabs turn clean, and the wrong tool shows a hint ("Try the gloves.").
4. **Job 1:** the five Act 1 jobs as data, each needing the one before, in a `JobBook`. Job 1 covers the patio (42 weeds and bits of trash); the HUD in the top-left counts down, and clearing the last one shows "Patio cleared!" and moves on to Job 2. Tiles in a locked job's area cannot be cleared yet.
5. **Jobs board:** Tab opens a list of every job with its status (done, to do with tiles left and the description, or locked).
6. **Tests and docs:** EditMode tests for tool slots, aiming, clearing counts and job statuses; PlayMode tests for pulling a weed, the wrong-tool hint, the HUD count, the jobs board and finishing Job 1. README updated.

Checked in batch mode: everything compiles with no errors or warnings from our code; 36 EditMode and 11 PlayMode tests pass; screenshots of the toolbar, highlight, clearing, HUD, jobs board and "Patio cleared!" look as expected. **Not yet checked by hand in the Editor.**

Next: play-test M2 in the Editor (README, "Playing Job 1"), then **M3 Cleaning** (close-up scrubbing scene, Job 2 the kettle grill, 95% rule).

### 1 Nov 2026: M1 Foundations

Done (one commit per step):

1. **Repository and project:** Git repository at the root with a Unity `.gitignore` and Git LFS for art, audio and fonts. New `BBQKing` Unity project (URP 2D) using the 2D prototype's packages and settings, the `Assets/_Project` layout from CLAUDE.md, and assembly definitions for game code, editor tools and tests. The prototypes and `pitch.md.old` stay on disk but are ignored by Git.
2. **Input:** `BBQKingControls.inputactions` with every action from design.md section 6.1 (Player map) plus a UI map, keyboard and mouse first with gamepad bindings alongside. An `InputReader` asset turns it into simple values and events.
3. **Camera and resolution:** Pixel Perfect Camera at 32 pixels per unit with whole-number scaling. The reference resolution is set in `Data/Settings/DisplayConfig` (960x540 by default, with 640x360 and 1280x720 one click away, even during Play mode). The camera follows the player and stays inside the map. Y-sorting is on.
4. **Backyard tilemap:** 40 x 25 tiles laid out from section 4.2 (house wall and back door, weedy patio, long-grass lawn, paths, shed yard, smoker corner, fences with broken panels). Ground, Overgrowth (empty until M2) and Walls and Fences layers; walls and fences block the player. Placeholder tiles are named after the section 8 content list.
5. **Player:** movement with a Rigidbody2D, a 4-frame walk cycle in four directions, and values in `Data/Player/PlayerConfig`. EditMode tests for the facing and walk-cycle rules.
6. **Interaction:** press E near an object; an on-screen prompt; event channels so the player and UI never reference each other; a message panel that freezes the player while open. Test objects: the relative's letter and the back door, both prefabs.
7. **Docs and tests:** `BBQKing/README.md`, and a PlayMode walkthrough test of the Backyard.

Checked in batch mode: everything compiles with no errors or warnings from our code; 14 EditMode and 5 PlayMode tests pass; screenshots at all three resolutions look as expected. **Not yet checked by hand in the Editor.**

Later the same day: fixed the back door drawing over the player (Y-sorting), and the developer chose **960x540** as the reference resolution. All tests also pass in the Editor. Hand play-test: walking speed, camera and text readability are fine. The player could not get behind the bottom fence because each fence tile blocked its whole square; fences across the screen now block only a strip at their base (side fences still block the whole tile), with a new PlayMode test.

M1 play-tested by hand and marked done.

Next: **M2 Restoration** (tile clearing with tools, Job 1, the jobs board).

M2 decisions from the developer (1 Nov 2026):
- **Gloves and trash bag are two separate tools:** gloves pull weeds, the trash bag picks up trash. Recorded in design.md section 5.2.
- **"Trash bag", not "bin bag":** the American term, for the setting.
- **Targeting:** changed after the play-test on 2 Nov 2026, at the developer's request: with a mouse, clicks only work on the tiles touching the player's tile (diagonals included) and the highlight only shows while the pointer is over one. The highlight no longer runs ahead of the player. A gamepad still uses the faced tile. Later the same day, also at the developer's request: the highlight only shows over a weed or piece of trash, the right tool is picked automatically when the player points at one, and its toolbar slot briefly grows (size and time are in `Data/Settings/UiConfig` since 2 Nov 2026). Choosing by hand with 1 to 6 or the wheel still works.
- **Job complete:** when every tile in the job is cleared (the 95% rule stays for close-up scrubbing).
- **Weekend counter:** waits for M4.

## Questions for the developer

- **M3 starting values to tune (design.md does not set them):** brush radius 6 pixels of close-up art (18 screen pixels at 960 x 540), scrub speed 3 (a fully grimy spot clears in a third of a second), and a 1.5 second pause on "Spotless!" before the close-up closes. All are in `Data/Cleaning/CleaningConfig`.
- **Job 2 completion text:** "Grill cleaned!" in `Data/Messages/Job2Done` is marked as placeholder text. Job 2 also unlocks the chicken thighs recipe card (design.md 4.5); recipe cards arrive in M7.
- **After Job 2:** the jobs board now shows Job 3 as available, because jobs only need the job before them. In design.md 4.3, Event 1 (the family cookout) comes between Job 2 and Job 3; that link arrives with events in M4.
- **Gamepad in close-ups:** a gamepad can open and leave the close-up, but cannot move the brush yet. Controller support is a later goal (design.md 6.1).
- **Sound:** the section and triple chimes are in (placeholder bell tones made in code; replace `Audio/Placeholder/clean_chime*.wav` with real sounds of the same names). The scrub sound from design.md 7.3 and the other audio hooks are still planned for M8.
- **Celebration starting values to tune:** 8 sparkles at 1.5 times size for a section, 24 at 3 times for the whole grill, 0.5 seconds between the last section's sparkle and the big one, a 60% white flash fading over 0.4 seconds. All in `Data/Cleaning/CleaningConfig`.
- **The 2D prototype:** `BBQ-Prototype-2D` is not on this laptop. If you want its scripts or art compared with the new cleaning code, copy it next to the repository.

- **design.md is the master design document (2 Nov 2026):** the developer retired the separate master copy. The M2 design changes (separate gloves and trash bag, "trash bag" wording, tools only next to the player, automatic tool choice, the clearing completion rule) are in design.md with decision log rows, so nothing needs copying. CLAUDE.md is updated to match.
- **M2 starting values to tune (design.md does not set them):** small, medium and large weeds take 1, 2 and 3 pulls; each piece of trash takes 1; 45% of patio slabs start weedy and 15% of the rest have trash; tool reach 1 tile around the player's tile; holding the button uses the tool every 0.3 seconds. All are in data assets (see the README); the two shares moved into `ClearingConfig` on 2 Nov 2026.
- **Jobs and events:** each job currently needs only the job before it. In design.md section 4.3, events come between some jobs (for example Event 1 before Job 3); that link arrives with events in M4.
- **Job 1 completion text:** "Patio cleared!" in `Data/Messages/Job1Done` is marked as placeholder text. Job 1 also unlocks the grilled corn recipe card (section 4.5); recipe cards arrive in M7.
- **Reference resolution (answered 1 Nov 2026):** 960x540 chosen after testing. design.md and CLAUDE.md are both updated.
- **Relative's name and the letter text:** still open in design.md section 11. The letter in `Data/Messages/RelativeLetter` has clearly marked placeholder text.
- **Starting values for the player:** walking speed 3.5 tiles a second, walk animation 8 frames a second, reach 0.5 tiles ahead with a 0.6 tile radius. design.md does not set these; they are in `PlayerConfig` to tune.
- **Small additions beyond the plan, for your information (no design change):**
  - An `Editor` folder and a `Tests/PlayMode` folder inside `Assets/_Project`. CLAUDE.md does not list `Editor`, but Unity needs editor-only scripts kept apart.
  - A **UI** input map alongside the Player map, so the player stands still while a letter is open.
  - Broken fence panels still block the player in M1, so nobody can walk off the map. Job 3 replaces them in M2 or later.
