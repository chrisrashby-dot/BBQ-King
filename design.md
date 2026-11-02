# BBQ King: Game Design Document

Oct 31, 2026 · @Chris

## 1. Overview

BBQ King is a cosy, single-player cooking and restoration game. You inherit a late relative's house, bring its overgrown American backyard back to life, host barbecues for the neighbourhood, then take your skills on the road in a food truck and all the way to a barbecue championship.

This document is the working specification for the game. It is the source of truth for design decisions; the build follows it, and it is updated whenever a decision changes.

| Item | Detail |
| --- | --- |
| Working title | BBQ King |
| Genre | Cosy simulation: restoration, planning and cooking |
| Platform | PC (Steam) |
| Players | Single-player only |
| Engine | Unity 6, 2D, C# |
| Visual style | 2D pixel art, 32x32 sprites, three-quarter top-down view (Space Station 13 style) |
| Setting | An American backyard and neighbourhood, with American barbecue culture |
| Session length | Built for 30-minute sessions; supports longer play |
| Player fantasy | From overgrown backyard to champion pitmaster, at your own pace |

**Key decisions:**

| Decision | Choice |
| --- | --- |
| Story | You inherit a late relative's house; restoring their backyard is where the game begins |
| Cooking realism | Light realism: real techniques such as low and slow smoking, resting meat and managing heat, with forgiving timings and recoverable mistakes |
| Progression | Money and reputation: money pays for shopping, fuel, repairs and upgrades; reputation unlocks bigger events, the food truck and championship entry |
| Tone | Relaxing and cosy with a little challenge; never chaotic |

## 2. Design pillars

Every feature must support these four pillars; if it breaks one, it needs rethinking.

1. **Cosy, never chaotic.** Calm pacing, pleasant feedback and recoverable mistakes. Tension comes from planning and craft, not frantic multitasking.
2. **Satisfying in 30 minutes.** Every session ends with visible progress: a cleared patch, a clean grill, a finished menu or a served meal.
3. **A little bit of challenge.** Choices matter. Poor planning or fire management lowers the result, but never ends the game.
4. **Every stage matters.** Restoration, planning and hosting are equally important and feed into each other. The game gets more interesting as it goes, not less.

## 3. Core loop and game structure

### 3.1 Core loop

Each event follows the same seven steps, and its rewards fund and unlock the next.

1. **Restore:** clear, clean or repair part of the space or equipment.
2. **Plan:** read the event brief (guests, occasion, budget) and choose a menu and cooking style.
3. **Shop:** buy meat, sides, drinks and fuel within the budget.
4. **Prep:** rub, marinate, chop and set up the backyard for guests.
5. **Cook:** light and manage the fire, control temperature and time each dish.
6. **Host:** serve guests, respond to requests and keep everyone happy.
7. **Reward:** earn money, reputation and guest feedback, which unlock the next job and bigger events.

The stages link deliberately: restoration unlocks cooking options (cleaning the smoker makes low and slow possible), planning shows up on the day (a forgotten item means improvising), and hosting funds the next job.

### 3.2 Acts

| Act | Setting | Restoration focus | Cooking and hosting focus | New skills |
| --- | --- | --- | --- | --- |
| 1. The backyard | The inherited house and its overgrown backyard | Weeds, rubbish, patio, the relative's old grill and smoker | Family, neighbours, a children's birthday party | Grilling basics, fire lighting, menu planning, first low and slow cook |
| 2. The food truck | Markets, festivals, private bookings | A tired second-hand food truck, inside and out | Serving the public, catering contracts, regulars | Cooking in a small space, menus that travel, reading a crowd |
| 3. The championship | Regional heats and a final | Competition rig and kit | Judged categories and turn-in boxes | Precision, consistency, presentation |

### 3.3 Story

You inherit a late relative's house. Restoring their neglected backyard gives a gentle reason to keep going, and over time you become the neighbourhood's go-to barbecue host. Friendly rivals share tips and turn up at your events rather than acting as villains. Players can return to the backyard at any time to host a relaxed family barbecue between food truck shifts or championship heats.

### 3.4 Money and reputation

- **Money** is earned from events (guests contribute, later paid bookings) and spent on ingredients, fuel, repairs, tools and upgrades.
- **Reputation** is earned from guest satisfaction and unlocks event types, the food truck (end of Act 1) and championship entry (Act 3).
- Money can run low but never to zero: a small "family fund" top-up prevents getting stuck, in keeping with the cosy pillar.

### 3.5 Avoiding a late-game slump

- Introduce new cooking methods across the acts (smoking, rotisserie, pizza oven) rather than all at the start.
- Let players hand off mastered jobs to a helper, such as weeding, so their time goes on fresh content.
- Regulars from the backyard years follow the food truck, so earlier relationships pay off later.

## 4. Act 1 design: The backyard

Act 1 is the first playable slice and the first build target. It takes the player from arriving at the inherited house to hosting a neighbourhood block party, and ends by unlocking the food truck. Everything in this section is a first proposal to build and then tune through playtesting.

### 4.1 Scope

| Item | Act 1 target |
| --- | --- |
| Play time | Roughly 60 to 90 minutes, across two to four 30-minute sessions |
| Locations | The backyard, the house kitchen (prep only), and three shops reached from a town map |
| Restoration jobs | 5 |
| Events | 4, ending with the block party |
| Recipes | 8 (5 mains, 3 sides) |
| Cooking equipment | The relative's kettle grill (charcoal) and offset smoker (wood) |
| Weather | Sunny, plus one light drizzle event |

**Out of scope for Act 1:** the food truck, the championship, gas grills, seasons, helpers, guest dialogue trees and advanced garden decorating.

### 4.2 The backyard map

The backyard is a single tile map of roughly 40 by 25 tiles, split into areas that open up as jobs are completed.

| Area | Starting state | Unlocked by | What it adds |
| --- | --- | --- | --- |
| Back door and patio | Weeds through the slabs, rubbish, a grimy kettle grill | Available from the start | Grilling station and seating for 4 |
| Lawn | Long grass, broken fence panels | Job 3 | Seating for 8 more guests, kids' corner |
| Smoker corner | Overgrown, rusted offset smoker under a tarp | Job 4 | Low and slow cooking |
| Shed and woodpile | Cluttered shed, wet scattered wood | Job 5 | Tool storage, dry wood supply |
| Kitchen (inside) | Usable but dusty | Available from the start | Prep bench, fridge, recipe book |

### 4.3 Act 1 flow

1. **Arrival:** the player arrives at the house with a letter from the relative and the keys. A short walk around the backyard teaches movement and interaction.
2. **Job 1, clear the patio:** pull weeds and bag rubbish, tile by tile. Teaches clearing.
3. **Job 2, clean the kettle grill:** a close-up scrubbing scene. Teaches cleaning tools and the progress meter.
4. **Event 1, a family cookout:** 4 guests, burgers and hot dogs on the kettle grill with charcoal. Teaches the full loop: brief, plan, shop, prep, cook, serve, feedback.
5. **Job 3, clear the lawn and fix the fence:** mowing, raking and replacing fence panels.
6. **Event 2, the neighbours' welcome:** 8 guests, adds chicken thighs and grilled corn, and the first guest preference (one vegetarian).
7. **Job 4, restore the offset smoker:** a multi-stage job: remove the tarp, scrape rust, clean the firebox and replace the door seal.
8. **Job 5, sort the shed and stack the woodpile:** stack wood under cover so it dries; wet wood smokes badly.
9. **Event 3, a children's birthday party:** 10 guests, half of them children; the first low and slow cook (pork ribs) running alongside quick grilling for the kids. A light drizzle tests the gazebo.
10. **Event 4, the block party (Act 1 finale):** 16 guests; an overnight brisket on the smoker plus grill favourites. Hitting the reputation target unlocks a listing for a second-hand food truck, ending Act 1.

### 4.4 The relative's recipe cards

The relative's handwritten recipe cards are found as areas are cleared (one in the kitchen at the start, one per completed job). Each card unlocks a recipe and adds a line of personal history, which carries the story without cutscenes.

### 4.5 Act 1 recipes

| Recipe | Type | Equipment | Unlocked | Done when (light realism) |
| --- | --- | --- | --- | --- |
| Burgers | Main | Kettle grill, direct heat | Start | Internal 160°F, the USDA minimum for ground meat |
| Hot dogs | Main | Kettle grill, direct heat | Start | Browned and heated through |
| Chicken thighs | Main | Kettle grill, two-zone | Job 2 card | Internal 165°F, the USDA minimum for poultry |
| Pork ribs | Main | Offset smoker, low and slow | Job 4 card | Tender window; target temperature to be confirmed |
| Brisket | Main | Offset smoker, overnight | Job 5 card | Tender window plus rest; target to be confirmed |
| Grilled corn | Side | Kettle grill | Job 1 card | Charred and tender |
| Coleslaw | Side | Kitchen prep | Start | No cooking |
| Potato salad | Side | Kitchen prep | Job 3 card | No cooking |

Safety temperatures follow the [USDA FSIS Safe Minimum Internal Temperature Chart](https://www.fsis.usda.gov/food-safety/safe-food-handling-and-preparation/food-safety-basics/safe-temperature-chart). Rib and brisket finish targets need a reliable barbecue source before they are fixed.

### 4.6 Act 1 success criteria

- A new player can play from arrival to the block party without getting stuck.
- Every 30-minute session ends with at least one visible win.
- Players learn fire control through feedback (smoke colour, thermometer, food states), not long text tutorials.
- Playtesters say they want to see the food truck.

## 5. Systems specification

This section defines how each system works. All numbers are starting values to tune in playtesting, and should live in data files rather than code (see section 9).

### 5.1 Time and days

- The game runs in **weekends**. Each weekend has one main event or one main job, and the jobs board shows what is available.
- Outside events there is no clock: jobs, planning and shopping take as long as the player likes.
- On event days a clock runs from guest arrival to the end of the party. Starting value: 1 real second equals 1 in-game minute.
- Long cooks (ribs, brisket) use **check-ins**: the player tends the fire, then can fast-forward to the next check-in. Missing a check-in drifts the temperature but never ruins the cook outright.

### 5.2 Restoration

- **Clearing:** tiles in the world map have an overgrowth or rubbish state. The player uses a tool on a tile to clear it in one to three actions, depending on the tile and tool. A clearing job is complete when every tile in its area is cleared; the 95% rule is only for close-up cleaning. Only tiles next to the player (diagonals included) or the tile they stand on can be worked on. Pointing at a weed or piece of trash highlights it and picks the tool it needs automatically.
- **Cleaning:** objects (grill, smoker, patio furniture) open a close-up scene. A grime layer covers the object; holding the mouse button with a tool erases grime under a circular brush. The job is complete at 95% clean, so players never hunt for the last pixel.
- **Repairing:** some jobs need parts (fence panels, a door seal), bought at the hardware store.
- **Tools:** gloves, trash bags, rake, shears, scraper, wire brush and hose in Act 1. Gloves and trash bags are separate tools: gloves pull weeds, the trash bag picks up trash. Upgrades increase brush size and speed.

### 5.3 Event briefs and menu planning

Each event brief lists the occasion, guest count (adults and children), budget suggestion, preferences (for example vegetarian, mild food for children) and any equipment needed.

- **Portions:** 1 main portion per adult and 0.5 per child, plus at least 2 sides.
- **Variety bonus:** 2 or more different mains raise satisfaction.
- **Preferences:** an unmet preference lowers that guest's satisfaction but does not fail the event.

### 5.4 Shopping and money

- Three shops in Act 1: **butcher** (meat), **supermarket** (sides, buns, drinks, cake), **hardware store** (charcoal, wood, tools, parts).
- Shops are menu screens reached from a town map; no walking around town in Act 1.
- Better-quality ingredients cost more and raise food quality.
- Money comes from events: guests chip in for family events, and later events pay a fee.
- **Safety net:** if money falls below the cost of a basic event, a small family fund top-up appears, so the player can never get stuck.

### 5.5 Fuel

| Fuel | Used in | Behaviour | Notes |
| --- | --- | --- | --- |
| Charcoal | Kettle grill | Steady, predictable heat; lit with a chimney starter | Bought in bags at the hardware store |
| Wood | Offset smoker | Adds smoke flavour; needs regular top-ups at check-ins | Must be stored dry; wet wood gives bitter white smoke and a quality penalty |
| Gas | Not in Act 1 | Easy, fast heat with little flavour | Arrives with the food truck in Act 2 |

### 5.6 Cooking

- **Heat control:** the kettle grill has a vent slider and two zones (direct and indirect). The smoker has a firebox vent and a chimney damper.
- **Thermometer:** shows cooker temperature and, for meat, internal temperature. Units default to °F with a °C option in settings.
- **Food states:** Raw, Cooking, Perfect, Overdone, Burnt. The Perfect window is generous on Relaxed difficulty and tighter on Pitmaster.
- **Smoke cues:** thin blue smoke means good; thick white smoke means the fire needs attention.
- **Resting:** large cuts gain a quality bonus if rested before serving.
- **Turning and moving food:** click to flip, drag between zones.

### 5.7 Hosting and ratings

- Each guest has a satisfaction score from 0 to 100, built from food quality (50%), preferences met (20%), variety (15%) and atmosphere such as seating and decoration (15%).
- Waiting is forgiving: guests chat and play while waiting and only lose a little satisfaction if a dish is very late.
- The event ends with a summary screen: a 1 to 5 star rating, money earned, reputation earned and guest comments.

### 5.8 Reputation

- Reputation earned per event equals the star rating multiplied by guest count.
- Act 1 thresholds unlock the next event; the block party sets the threshold that unlocks the food truck.

### 5.9 Difficulty

- **Relaxed:** generous cooking windows, cheaper prices, no weather penalties.
- **Pitmaster:** tighter windows, real budgets, weather affects fire and fuel.

### 5.10 Saving

Autosave after each loop step and at the end of each weekend. Manual save anywhere except during a cook.

## 6. Controls and UI

### 6.1 Controls (keyboard and mouse)

| Action | Input |
| --- | --- |
| Move | WASD or arrow keys |
| Interact, pick up, open close-up | E |
| Use tool, scrub, flip food | Left mouse button (hold to scrub) |
| Cancel, put down | Right mouse button |
| Select tool | Automatic when pointing at something to clear; or 1 to 6, or mouse wheel |
| Jobs board and journal | Tab |
| Fast-forward to next check-in | Space (only during long cooks) |
| Pause menu, leave close-up | Escape |

Controller support is a later goal; the input system should be set up so it can be added without rewriting.

### 6.2 Screens

| Screen | Purpose |
| --- | --- |
| Title | New game, continue, settings, quit |
| Backyard (main view) | Walk, clear, interact; HUD shows money, reputation, weekend and current job |
| Close-up cleaning | Scrub an object; progress meter, tool selector |
| Jobs board | Available jobs and the next event |
| Event brief | Occasion, guests, preferences, budget suggestion |
| Menu planner | Choose mains and sides; portion check against guest count |
| Town map and shops | Pick a shop; buy from a list with a running total |
| Prep | Rub, marinate, chop in the kitchen |
| Cooking view | Zoomed grill or smoker: vents, zones, thermometer, food states |
| Event summary | Stars, money, reputation, guest comments |
| Recipe book | Unlocked recipe cards and the relative's notes |
| Settings | Volume, °F or °C, difficulty, hold or toggle to scrub, text size |

### 6.3 Accessibility basics

Hold-or-toggle scrubbing, adjustable text size, colour cues always paired with an icon or text (for example food states), and no time-critical inputs outside event cooking.

## 7. Art and audio specification

### 7.1 Art direction

A warm, cosy 2D pixel art look. The main visual reference is Space Station 13: 32x32 sprites in a three-quarter top-down view, with a wide view of spaces full of objects to interact with. Stardew Valley and Unpacking are secondary references for the walkable world and charming, recognisable objects.

| Art direction | Decision |
| --- | --- |
| View | Three-quarter top-down: objects show their top and one front face; walkable player character |
| Grid | 32x32 pixels for tiles, objects and characters; larger items in multiples of 32 |
| Characters | 32x32, four directions (up, down, left, right), walk cycle of 4 frames |
| Close-ups | Zoomed-in scenes for scrubbing and cooking, in the same pixel style at a larger scale |
| Resolution | 960x540 reference resolution, showing about 30 x 17 tiles for a wide, Space Station 13 style view; scaled up crisply with a pixel-perfect camera |
| Assets | All original; no copied assets from Space Station 13 or any other game |

### 7.2 Art style guide

1. **Fixed palette:** one limited colour palette for every sprite, with warm summer tones for food, fire and the backyard.
2. **One outline style:** dark coloured outlines rather than pure black, applied everywhere.
3. **One light direction:** light from the top left, so shading is consistent.
4. **Consistent perspective:** every object uses the same three-quarter angle.
5. **Readable at a glance:** each object is recognisable at 32x32, especially food states and dirty versus clean surfaces.
6. **Test in context:** check every new sprite against an in-game screenshot before finalising it.

### 7.3 Mood and audio

- **Visuals:** soft lighting, golden-hour skies, clear contrast between grimy and clean, readable food states.
- **Tone:** gentle humour and warmth; friendly guests and rivals.
- **Sound effects:** sizzling, crackling wood, scrubbing, weeds pulled, trash bag rustle, a satisfying chime at 100% clean, guests chatting.
- **Ambience:** birdsong, distant lawnmowers and neighbourhood sounds; crickets in the evening.
- **Music:** relaxed acoustic, with a slightly livelier track during events.

## 8. Act 1 content list

Everything Act 1 needs, so art and build work can be tracked. Placeholders are fine until each item is drawn.

| Category | Items |
| --- | --- |
| Ground tiles | Grass (long and cut), patio slabs (clean, weedy), dirt, path, fence (intact, broken), house wall and back door |
| Overgrowth and rubbish | Weeds (3 variants), brambles, long grass, trash bags, cans, broken pots, leaves |
| Equipment | Kettle grill (dirty, clean), offset smoker (rusted, restored), chimney starter, thermometer, gazebo |
| Furniture | Patio table, chairs, picnic bench, cool box, kids' paddling pool, bunting, string lights |
| Shed and wood | Shed (cluttered, tidy), woodpile (wet, dry, stacked), axe block |
| Tools | Gloves, trash bag, rake, shears, mower, scraper, wire brush, hose |
| Food | Burger, hot dog, chicken thigh, pork ribs, brisket (each in raw, cooking, perfect, overdone, burnt), corn, coleslaw, potato salad, buns, birthday cake |
| Characters | Player (4 directions, walk, interact), 6 guest bases with palette swaps, 2 neighbours, children (2 bases) |
| Close-ups | Kettle grill grate, smoker body, smoker firebox (each with a grime layer) |
| UI | HUD icons (money, reputation, weekend), tool bar, jobs board, recipe cards, shop list, thermometer dial, star rating |
| Audio | Sizzle, fire crackle, scrub, weed pull, trash bag, clean chime, guest chatter, footsteps (grass and patio), menu clicks, 2 to 3 music tracks |

## 9. Technical specification

| Item | Decision |
| --- | --- |
| Engine | Unity 6, 2D project, Universal Render Pipeline with the 2D renderer |
| Language | C# |
| Camera | Pixel Perfect Camera at 960x540, 32 pixels per unit, integer scaling only: 2x on 1080p, 4x on 4K, black borders on 1440p |
| Sprite import | Point (no filter), no compression, pixels per unit 32 |
| World | Unity Tilemap for ground, overgrowth and fences; objects as prefabs |
| Input | Unity's Input System package, with actions defined in one input asset |
| Data | ScriptableObjects for recipes, ingredients, tools, jobs, events and shop stock, so values can be tuned without code changes |
| Saving | JSON save files written to the player's persistent data folder |
| Version control | Git, with a Unity .gitignore and Git LFS for art and audio |
| Build tool | Claude Code, guided by CLAUDE.md and design.md in the project root |
| Licence | Unity Personal is free up to US$200,000 in revenue and funding; Unity Pro is required above that, at US$2,310 per seat per year in 2026 ([Unity pricing](https://unity.com/products/pricing-updates)) |

### 9.1 Code principles

- Each system (clearing, cleaning, cooking, fuel, guests, economy, saving) lives in its own folder and scripts, talking to others through events rather than direct references where practical.
- No gameplay numbers hard-coded in scripts; they belong in ScriptableObject data.
- Placeholder art must be swappable by replacing sprites, without code changes.

### 9.2 Scenes

Title, Backyard (includes the kitchen), Town (shop screens), and close-up scenes for cleaning and cooking loaded additively over the backyard.

## 10. Decision log

Newest decisions at the bottom. When a decision changes, add a new row rather than editing the old one.

| Date | Decision | Reason |
| --- | --- | --- |
| 31 Oct 2026 | Cosy pace, 30-minute sessions, a little challenge, no chaos | Core player experience |
| 31 Oct 2026 | Progression from backyard to food truck to championship | Keeps the game growing rather than repeating |
| 31 Oct 2026 | PC (Steam) first | Target platform |
| 31 Oct 2026 | Unity with C# | Existing C# experience; asset store; strong 2D tools |
| 31 Oct 2026 | 2D pixel art over 3D low-poly | Built a "Clean the grill" prototype in both; 2D felt achievable and looked cute even with placeholder art |
| 31 Oct 2026 | Space Station 13 style 32x32 sprites as the main visual reference | Chunky, readable and achievable for a solo developer |
| 31 Oct 2026 | Working title BBQ King (was Barbecue Simulator) | Barbecue Simulator was already used by other projects |
| 31 Oct 2026 | American backyard setting | Fits American barbecue culture, smokers and cook-offs |
| 31 Oct 2026 | Light realism in cooking | Real techniques, forgiving timings |
| 31 Oct 2026 | Money and reputation | Two reasons to host well; money for spending, reputation for unlocks |
| 31 Oct 2026 | Story: inheriting a late relative's house | Gentle motivation and a reason the backyard is neglected |
| 31 Oct 2026 | Strictly single-player | Simpler build; suits the cosy pace |
| 31 Oct 2026 | Pitch document became this design document | Moving into the specification phase |
| 31 Oct 2026 | 960x540 reference resolution instead of 640x360 (confirmed by testing) | Space Station 13 shows a wide area with small-looking sprites; 960x540 gives a wider view and scales cleanly on 1080p and 4K |
| 1 Nov 2026 | Gloves and trash bags are separate tools; "bin bag" renamed "trash bag" | Each tool clears one kind of thing; American term for the setting |
| 1 Nov 2026 | A clearing job is complete when every tile in its area is cleared | Developer's choice for M2; clearing is counted tile by tile, and the 95% rule stays for close-up cleaning |
| 2 Nov 2026 | Tools work only on tiles next to the player, and the right tool is picked automatically when pointing at something to clear | From the M2 play-test: a highlight running ahead of the player felt wrong, and picking tools by hand was tedious |
| 2 Nov 2026 | "Rubbish bags" in the overgrowth list renamed "trash bags" | Consistent with the trash bag tool and the American setting |

## 11. Open questions

- [ ] Review and confirm the Act 1 proposal in section 4: flow, jobs, events and recipes.
- [ ] Confirm or adjust the starting values in section 5 after the first playtest.
- [x] Choose the reference resolution by trying 640x360, 960x540 and 1280x720 in the 2D prototype.
- [ ] Find a reliable barbecue source for rib and brisket finish temperatures.
- [ ] Who was the relative (grandparent, aunt or uncle), and what was their name?
- [ ] Design the player character: fixed or customisable?
- [ ] Choose the colour palette and draw the kettle grill as the first real sprite.
- [ ] Final name: BBQ King is the working title; check for clashes before release.

## Appendix: market landscape and name check

A web search on 31 October 2026 found barbecue games and cleaning games, but none combining restoration, planning and cosy hosting into one progression. The search was not exhaustive.

| Game | What it is | How BBQ King differs |
| --- | --- | --- |
| [BBQ Simulator: The Squad](https://store.steampowered.com/app/1755350) | Chaotic co-op barbecue sandbox for up to 8 players; rated Very Positive from 883 Steam reviews at the time of search | Single-player, cosy, with restoration and planning |
| [Cooking Simulator: BBQ DLC](https://store.epicgames.com/p/cooking-simulator-bbq-c789a9?lang=nl) | Expansion adding grills and outdoor cooking to a realistic cooking sim | Backyard and event focus rather than a restaurant |
| [Oversmoked Outlaws](https://www.vgdb.co/games/oversmoked-outlaws) | Cosy and survival barbecue game with adjustable danger | No survival layer; grounded, everyday setting |
| [BBQ pitmaster tycoon (mobile)](https://play.google.com/store/apps/details?id=bbq.pit) | Food truck and restaurant business game with catering contracts | Hands-on cooking and restoration, not a tycoon |
| [Pool Cleaning Simulator](https://www.pcspecialist.ie/forums/threads/pool-cleaning-simulator.94492/) | PowerWash Simulator style pool cleaning | Shows appetite for cleaning sims; BBQ King adds a cooking payoff |

**Name check:** a search on 31 October 2026 found two older casual games with similar names: [BBQ King](https://apps.microsoft.com/detail/9wzdncrfj3ln), a Microsoft Store grilling business game, and [Cooking BBQ King](https://apps.apple.com/us/app/-/id6443548498), an iOS time management game. Neither appeared to be on Steam, but the name should be checked again before release.
