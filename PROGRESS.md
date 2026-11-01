# BBQ King: Progress log

Claude Code updates this file at the end of every session. Newest entry at the top.

## Milestone status

| Milestone | Status |
| --- | --- |
| M1 Foundations | Built; waiting for a hand play-test in the Editor |
| M2 Restoration | Not started |
| M3 Cleaning | Not started |
| M4 First cookout | Not started |
| M5 Economy and saving | Not started |
| M6 Smoker | Not started |
| M7 Act 1 events | Not started |
| M8 Polish and playtest | Not started |

## Session log

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

Next: play-test M1 in the Editor (see the README), choose the reference resolution, then start **M2 Restoration** (tile clearing with tools, Job 1, the jobs board).

## Questions for the developer

- **Reference resolution:** after trying the three options in `DisplayConfig`, which do you prefer? 960x540 is the default for now. If you confirm it, the "(proposed)" in design.md sections 7.1 and 9 can be dropped.
- **Relative's name and the letter text:** still open in design.md section 11. The letter in `Data/Messages/RelativeLetter` has clearly marked placeholder text.
- **Starting values for the player:** walking speed 3.5 tiles a second, walk animation 8 frames a second, reach 0.5 tiles ahead with a 0.6 tile radius. design.md does not set these; they are in `PlayerConfig` to tune.
- **Small additions beyond the plan, for your information (no design change):**
  - An `Editor` folder and a `Tests/PlayMode` folder inside `Assets/_Project`. CLAUDE.md does not list `Editor`, but Unity needs editor-only scripts kept apart.
  - A **UI** input map alongside the Player map, so the player stands still while a letter is open.
  - Broken fence panels still block the player in M1, so nobody can walk off the map. Job 3 replaces them in M2 or later.
