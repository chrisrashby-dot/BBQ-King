# BBQ King: Progress log

Claude Code updates this file at the end of every session. Newest entry at the top.

## Milestone status

| Milestone | Status |
| --- | --- |
| M1 Foundations | Done (play-tested 1 Nov 2026) |
| M2 Restoration | Built; play-tested once (2 Nov 2026), targeting changed |
| M3 Cleaning | Not started |
| M4 First cookout | Not started |
| M5 Economy and saving | Not started |
| M6 Smoker | Not started |
| M7 Act 1 events | Not started |
| M8 Polish and playtest | Not started |

## Session log

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
- **Gloves and trash bag are two separate tools:** gloves pull weeds, the trash bag picks up trash. This changes design.md section 5.2 (see questions below).
- **"Trash bag", not "bin bag":** the American term, for the setting.
- **Targeting:** changed after the play-test on 2 Nov 2026, at the developer's request: with a mouse, clicks only work on the tiles touching the player's tile (diagonals included) and the highlight only shows while the pointer is over one. The highlight no longer runs ahead of the player. A gamepad still uses the faced tile.
- **Job complete:** when every tile in the job is cleared (the 95% rule stays for close-up scrubbing).
- **Weekend counter:** waits for M4.

## Questions for the developer

- **Design changes to record (1 Nov 2026):** please update the master design document:
  - Section 5.2 Tools: gloves and trash bags as two separate tools ("gloves, trash bags, rake, shears, scraper, wire brush and hose").
  - Section 8 Tools: "bin bag" becomes "trash bag". (Section 8 also says "rubbish bags" under Overgrowth and rubbish; in-game this reads as trash, but the placeholder files keep the section 8 names, such as `rubbish_bag.png`, so they still match one for one. Say if you would like those renamed too.)
- **M2 starting values to tune (design.md does not set them):** small, medium and large weeds take 1, 2 and 3 pulls; each piece of trash takes 1; 45% of patio slabs start weedy and 15% of the rest have trash; tool reach 1 tile around the player's tile; holding the button uses the tool every 0.3 seconds. All are in data assets (see the README) except the two shares, which are in `Editor/BackyardLayout.cs`.
- **Jobs and events:** each job currently needs only the job before it. In design.md section 4.3, events come between some jobs (for example Event 1 before Job 3); that link arrives with events in M4.
- **Job 1 completion text:** "Patio cleared!" in `Data/Messages/Job1Done` is marked as placeholder text. Job 1 also unlocks the grilled corn recipe card (section 4.5); recipe cards arrive in M7.
- **Reference resolution (answered 1 Nov 2026):** 960x540 chosen after testing. design.md and CLAUDE.md are both updated.
- **Relative's name and the letter text:** still open in design.md section 11. The letter in `Data/Messages/RelativeLetter` has clearly marked placeholder text.
- **Starting values for the player:** walking speed 3.5 tiles a second, walk animation 8 frames a second, reach 0.5 tiles ahead with a 0.6 tile radius. design.md does not set these; they are in `PlayerConfig` to tune.
- **Small additions beyond the plan, for your information (no design change):**
  - An `Editor` folder and a `Tests/PlayMode` folder inside `Assets/_Project`. CLAUDE.md does not list `Editor`, but Unity needs editor-only scripts kept apart.
  - A **UI** input map alongside the Player map, so the player stands still while a letter is open.
  - Broken fence panels still block the player in M1, so nobody can walk off the map. Job 3 replaces them in M2 or later.
