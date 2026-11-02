using BBQKing.Player;
using UnityEditor;
using UnityEngine;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// Draws the placeholder art in code and saves it as PNG files under Art/Placeholder.
    ///
    /// Each file is named after an item in the design.md section 8 content list, so it can be
    /// replaced one for one: overwrite the PNG with your own drawing of the same size and every
    /// tile, prefab and scene that uses it updates automatically. Existing files are never
    /// overwritten, so replaced art is safe when the builder runs again.
    ///
    /// The style follows the art rules: 32x32 grid, light from the top left, dark coloured
    /// outlines rather than pure black.
    /// </summary>
    public static class PlaceholderArt
    {
        const string Root = "Assets/_Project/Art/Placeholder/";

        // ----- Ground and wall tiles (32 x 32, centre pivot)
        public const string GrassLong = Root + "Tiles/ground_grass_long.png";
        public const string GrassCut = Root + "Tiles/ground_grass_cut.png";
        public const string PatioClean = Root + "Tiles/ground_patio_clean.png";
        public const string PatioWeedy = Root + "Tiles/ground_patio_weedy.png";
        public const string Dirt = Root + "Tiles/ground_dirt.png";
        public const string Path = Root + "Tiles/ground_path.png";
        public const string FenceIntact = Root + "Tiles/fence_intact.png";
        public const string FenceBroken = Root + "Tiles/fence_broken.png";
        public const string HouseWall = Root + "Tiles/wall_house.png";

        // ----- Overgrowth and rubbish (32 x 32, centre pivot, transparent so the ground shows)
        public const string WeedSmall = Root + "Overgrowth/weed_small.png";
        public const string WeedMedium = Root + "Overgrowth/weed_medium.png";
        public const string WeedLarge = Root + "Overgrowth/weed_large.png";
        public const string TrashBag = Root + "Overgrowth/trash_bag.png";
        public const string RubbishCans = Root + "Overgrowth/rubbish_cans.png";
        public const string RubbishPotBroken = Root + "Overgrowth/rubbish_pot_broken.png";

        // ----- UI frames (9-sliced)
        public const string UiPanelDark = Root + "UI/ui_panel_dark.png";
        public const string UiPanelPaper = Root + "UI/ui_panel_paper.png";
        public const string UiSlot = Root + "UI/ui_slot.png";
        public const string UiSlotSelected = Root + "UI/ui_slot_selected.png";
        public const string TileHighlight = Root + "UI/ui_tile_highlight.png";   // 32 x 32, drawn in the world

        // ----- Tool icons (32 x 32, centre pivot), named after the section 8 tools list
        public const string ToolGloves = Root + "Tools/tool_gloves.png";
        public const string ToolTrashBag = Root + "Tools/tool_trash_bag.png";
        public const string ToolWireBrush = Root + "Tools/tool_wire_brush.png";

        // ----- Equipment in the backyard (32 x 32, bottom-centre pivot), named after the section 8 list
        public const string KettleGrillDirty = Root + "Equipment/kettle_grill_dirty.png";
        public const string KettleGrillClean = Root + "Equipment/kettle_grill_clean.png";

        // ----- Close-ups (128 x 128, centre pivot, drawn at 3 times size in the close-up scene).
        // The grime layer is the same size; the game wipes it away, so it is imported readable.
        public const string CloseUpKettleGrate = Root + "CloseUps/closeup_kettle_grill_grate.png";
        public const string CloseUpKettleGrateGrime = Root + "CloseUps/closeup_kettle_grill_grate_grime.png"; // top section
        public const string CloseUpKettleBowlGrime = Root + "CloseUps/closeup_kettle_grill_bowl_grime.png";   // bottom section

        // ----- Effects (32 x 32, centre pivot)
        public const int SparkleFrames = 4;

        /// <summary>For example Effects/sparkle_0.png: the cleaning sparkle, played in order.</summary>
        public static string Sparkle(int frame) => Root + $"Effects/sparkle_{frame}.png";

        // ----- Objects (32 x 32; letter centre pivot, door bottom-centre pivot)
        public const string Letter = Root + "Objects/letter.png";
        public const string BackDoor = Root + "Objects/house_back_door.png";

        // ----- Player walk frames (32 x 32, bottom-centre pivot so the feet sit on the position)
        public const int WalkFrames = 4;

        /// <summary>For example Characters/player_walk_down_0.png. Frame 0 is also the standing pose.</summary>
        public static string PlayerFrame(Facing facing, int frame) =>
            Root + $"Characters/player_walk_{facing.ToString().ToLowerInvariant()}_{frame}.png";

        // A small warm palette shared by all placeholders.
        static readonly Color32 Outline = PixelCanvas.Hex("#2b1d14");
        static readonly Color32 GrassDark = PixelCanvas.Hex("#3d6e2d");
        static readonly Color32 GrassMid = PixelCanvas.Hex("#4f8a3a");
        static readonly Color32 GrassLight = PixelCanvas.Hex("#6aa84a");
        static readonly Color32 GrassPale = PixelCanvas.Hex("#86bf5c");
        static readonly Color32 SlabMid = PixelCanvas.Hex("#c9b79c");
        static readonly Color32 SlabLight = PixelCanvas.Hex("#ddd0b8");
        static readonly Color32 SlabDark = PixelCanvas.Hex("#a8957a");
        static readonly Color32 Grout = PixelCanvas.Hex("#8f7d66");
        static readonly Color32 DirtMid = PixelCanvas.Hex("#8a6440");
        static readonly Color32 DirtDark = PixelCanvas.Hex("#6e4e31");
        static readonly Color32 DirtLight = PixelCanvas.Hex("#a07a52");
        static readonly Color32 GravelMid = PixelCanvas.Hex("#b5a58c");
        static readonly Color32 GravelDark = PixelCanvas.Hex("#9a8a72");
        static readonly Color32 GravelLight = PixelCanvas.Hex("#d0c4ae");
        static readonly Color32 WoodMid = PixelCanvas.Hex("#9c6b3e");
        static readonly Color32 WoodLight = PixelCanvas.Hex("#b8854f");
        static readonly Color32 WoodDark = PixelCanvas.Hex("#5a3a22");
        static readonly Color32 SidingMid = PixelCanvas.Hex("#d8cfc0");
        static readonly Color32 SidingShadow = PixelCanvas.Hex("#b0a693");
        static readonly Color32 SidingLight = PixelCanvas.Hex("#ebe4d8");
        static readonly Color32 Paper = PixelCanvas.Hex("#f2ead8");
        static readonly Color32 PanelDark = PixelCanvas.Hex("#3a2a20");
        static readonly Color32 PanelEdge = PixelCanvas.Hex("#6e5440");

        /// <summary>Writes every missing placeholder file.</summary>
        public static void GenerateAll()
        {
            Tile(GrassLong, DrawGrassLong);
            Tile(GrassCut, DrawGrassCut);
            Tile(PatioClean, () => DrawPatio(false));
            Tile(PatioWeedy, () => DrawPatio(true));
            Tile(Dirt, DrawDirt);
            Tile(Path, DrawPath);
            Tile(FenceIntact, () => DrawFence(false));
            Tile(FenceBroken, () => DrawFence(true));
            Tile(HouseWall, DrawHouseWall);

            Tile(WeedSmall, () => DrawWeed(0));
            Tile(WeedMedium, () => DrawWeed(1));
            Tile(WeedLarge, () => DrawWeed(2));
            Tile(TrashBag, DrawTrashBagOnGround);
            Tile(RubbishCans, DrawRubbishCans);
            Tile(RubbishPotBroken, DrawRubbishPotBroken);

            Frame(UiPanelDark, PanelDark, PanelEdge);
            Frame(UiPanelPaper, Paper, Grout);
            Frame(UiSlot, PanelDark, PanelEdge);
            Frame(UiSlotSelected, PanelEdge, Brass);
            Tile(TileHighlight, DrawTileHighlight);

            Tile(ToolGloves, DrawGloves);
            Tile(ToolTrashBag, DrawTrashBag);
            Tile(ToolWireBrush, DrawWireBrush);

            Tile(KettleGrillDirty, () => DrawKettleGrill(true), SpriteAlignment.BottomCenter);
            Tile(KettleGrillClean, () => DrawKettleGrill(false), SpriteAlignment.BottomCenter);

            Tile(CloseUpKettleGrate, DrawKettleGrate);
            GrimeImage(CloseUpKettleGrateGrime, DrawKettleGrateGrime);
            GrimeImage(CloseUpKettleBowlGrime, DrawKettleBowlGrime);

            for (int frame = 0; frame < SparkleFrames; frame++)
            {
                int f = frame; // each drawing needs its own copy of the frame number
                Tile(Sparkle(f), () => DrawSparkle(f));
            }

            Tile(Letter, DrawLetter);
            // The door's pivot is its bottom edge, where it meets the ground, so it Y-sorts in front
            // of the wall tile it hangs on but behind the player standing below it.
            Tile(BackDoor, DrawBackDoor, SpriteAlignment.BottomCenter);

            foreach (Facing facing in System.Enum.GetValues(typeof(Facing)))
                for (int frame = 0; frame < WalkFrames; frame++)
                {
                    string path = PlayerFrame(facing, frame);
                    if (PlaceholderAssetWriter.SavePngIfMissing(path, DrawPlayer(facing, frame)))
                        PlaceholderAssetWriter.ConfigureSprite(path, SpriteAlignment.BottomCenter);
                }
        }

        /// <summary>A grime layer: imported readable, because the game wipes it away pixel by pixel.</summary>
        static void GrimeImage(string path, System.Func<PixelCanvas> draw)
        {
            if (!PlaceholderAssetWriter.SavePngIfMissing(path, draw())) return;
            PlaceholderAssetWriter.ConfigureSprite(path, SpriteAlignment.Center);
            PlaceholderAssetWriter.MakeReadable(path);
        }

        static void Tile(string path, System.Func<PixelCanvas> draw, SpriteAlignment pivot = SpriteAlignment.Center)
        {
            if (PlaceholderAssetWriter.SavePngIfMissing(path, draw()))
                PlaceholderAssetWriter.ConfigureSprite(path, pivot);
        }

        /// <summary>A 12x12 UI frame with a 4 pixel border, stretched with 9-slicing.</summary>
        static void Frame(string path, Color32 fill, Color32 edge)
        {
            var c = new PixelCanvas(12, 12);
            c.FillRect(1, 1, 10, 10, fill);
            c.Outline(1, 1, 10, 10, edge);
            c.Outline(0, 0, 12, 12, Outline);
            // Clip the corners for a softer, rounded look.
            c.Set(0, 0, default); c.Set(11, 0, default); c.Set(0, 11, default); c.Set(11, 11, default);
            if (PlaceholderAssetWriter.SavePngIfMissing(path, c))
                PlaceholderAssetWriter.ConfigureSprite(path, SpriteAlignment.Center, new Vector4(4, 4, 4, 4));
        }

        // A repeatable "random" number from a position, so the art is the same every time it is drawn.
        static int Hash(int x, int y, int seed) => (int)(PlaceholderHash.Mix(x, y, seed) & 0x7fffffff);

        static PixelCanvas DrawGrassLong()
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(GrassMid);
            // Tufts: short vertical blades, darker at the base and lighter at the tip.
            for (int i = 0; i < 26; i++)
            {
                int x = Hash(i, 1, 7) % 32, y = Hash(i, 2, 7) % 29;
                c.Set(x, y, GrassDark);
                c.Set(x, y + 1, GrassMid);
                c.Set(x, y + 2, GrassLight);
                if (i % 3 == 0) c.Set(x + 1, y + 3, GrassPale);
            }
            return c;
        }

        static PixelCanvas DrawGrassCut()
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(GrassLight);
            // Mower stripes and a few specks.
            for (int y = 0; y < 32; y++)
                if ((y / 8) % 2 == 0)
                    for (int x = 0; x < 32; x++) c.Set(x, y, GrassMid);
            for (int i = 0; i < 12; i++) c.Set(Hash(i, 3, 5) % 32, Hash(i, 4, 5) % 32, GrassPale);
            return c;
        }

        static PixelCanvas DrawPatio(bool weedy)
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(Grout);
            // Four 15x15 slabs with a one-pixel grout line, lit from the top left.
            for (int sy = 0; sy < 2; sy++)
                for (int sx = 0; sx < 2; sx++)
                {
                    int x0 = sx * 16, y0 = sy * 16;
                    c.FillRect(x0, y0 + 1, 15, 15, SlabMid);
                    for (int i = 0; i < 15; i++) { c.Set(x0 + i, y0 + 15, SlabLight); c.Set(x0, y0 + 1 + i, SlabLight); }
                    for (int i = 1; i < 15; i++) { c.Set(x0 + i, y0 + 1, SlabDark); c.Set(x0 + 14, y0 + i, SlabDark); }
                }

            if (weedy)
            {
                // Weeds pushing up through the grout lines.
                int[,] weeds = { { 15, 4 }, { 15, 22 }, { 6, 15 }, { 25, 15 }, { 15, 15 } };
                for (int w = 0; w < weeds.GetLength(0); w++)
                {
                    int x = weeds[w, 0], y = weeds[w, 1];
                    c.Set(x, y, GrassDark); c.Set(x, y + 1, GrassMid); c.Set(x - 1, y + 2, GrassLight);
                    c.Set(x + 1, y + 2, GrassLight); c.Set(x + 1, y + 1, GrassMid);
                }
            }
            return c;
        }

        static PixelCanvas DrawDirt()
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(DirtMid);
            for (int i = 0; i < 30; i++) c.Set(Hash(i, 5, 3) % 32, Hash(i, 6, 3) % 32, i % 2 == 0 ? DirtDark : DirtLight);
            return c;
        }

        static PixelCanvas DrawPath()
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(GravelMid);
            // Small stones: a light top-left pixel and a dark bottom-right pixel.
            for (int i = 0; i < 22; i++)
            {
                int x = Hash(i, 7, 9) % 31, y = 1 + Hash(i, 8, 9) % 31;
                c.Set(x, y, GravelLight);
                c.Set(x + 1, y - 1, GravelDark);
            }
            return c;
        }

        static PixelCanvas DrawFence(bool broken)
        {
            // Transparent background, so the ground tile shows between the pickets.
            var c = new PixelCanvas(32, 32);

            // Two horizontal rails behind the pickets.
            c.FillRect(0, 9, 32, 3, WoodDark);
            c.FillRect(0, 21, 32, 3, WoodDark);

            // Four pickets, 6 pixels wide with a 2 pixel gap, and pointed tops.
            for (int p = 0; p < 4; p++)
            {
                if (broken && (p == 1 || p == 2)) continue; // missing pickets
                int x0 = p * 8 + 1;
                Picket(c, x0, 0, 28);
            }

            if (broken)
            {
                // A loose picket lying at the bottom of the gap.
                c.FillRect(9, 1, 13, 4, WoodMid);
                c.Outline(9, 1, 13, 4, WoodDark);
                for (int i = 10; i < 21; i++) c.Set(i, 3, WoodLight);
            }
            return c;
        }

        static void Picket(PixelCanvas c, int x0, int y0, int height)
        {
            c.FillRect(x0, y0, 6, height, WoodMid);
            for (int j = y0; j < y0 + height; j++) { c.Set(x0, j, WoodLight); c.Set(x0 + 5, j, WoodDark); }
            // Pointed top.
            c.FillRect(x0 + 1, y0 + height, 4, 1, WoodMid);
            c.FillRect(x0 + 2, y0 + height + 1, 2, 1, WoodLight);
            c.Outline(x0, y0, 6, height, WoodDark);
        }

        // ------------------------------------------------------------------ player

        static readonly Color32 Skin = PixelCanvas.Hex("#e8b48a");
        static readonly Color32 SkinShade = PixelCanvas.Hex("#c98f68");
        static readonly Color32 Hair = PixelCanvas.Hex("#5a3a22");
        static readonly Color32 Shirt = PixelCanvas.Hex("#c94f3b");
        static readonly Color32 ShirtLight = PixelCanvas.Hex("#e0705a");
        static readonly Color32 ShirtShade = PixelCanvas.Hex("#9c3a2b");
        static readonly Color32 Trousers = PixelCanvas.Hex("#3b4f7a");
        static readonly Color32 TrousersShade = PixelCanvas.Hex("#2c3b5c");
        static readonly Color32 Shoes = PixelCanvas.Hex("#3a2a20");
        static readonly Color32 Eye = PixelCanvas.Hex("#2b1d14");

        /// <summary>
        /// A simple person: head, shirt, trousers and shoes, about 24 pixels tall.
        /// Walk frames: 0 and 2 are the passing poses, 1 and 3 are steps with the body raised a pixel.
        /// </summary>
        static PixelCanvas DrawPlayer(Facing facing, int frame)
        {
            var c = new PixelCanvas(32, 32);
            bool stepping = frame % 2 == 1;
            int bob = stepping ? 1 : 0;          // body rises on the stepping frames
            bool firstLegForward = frame == 1;   // frame 3 swaps legs

            if (facing == Facing.Down || facing == Facing.Up)
            {
                // Legs: on a step, one foot lifts by a pixel.
                int leftLift = stepping && firstLegForward ? 1 : 0;
                int rightLift = stepping && !firstLegForward ? 1 : 0;
                Leg(c, 12, 1 + leftLift, 3, 6 + bob - leftLift, Trousers);
                Leg(c, 17, 1 + rightLift, 3, 6 + bob - rightLift, TrousersShade);

                // Body and arms (the arms swing opposite to the legs).
                int y0 = 7 + bob;
                c.FillRect(11, y0, 10, 9, Shirt);
                for (int j = y0; j < y0 + 9; j++) { c.Set(11, j, ShirtLight); c.Set(20, j, ShirtShade); }
                int armSwing = stepping ? (firstLegForward ? 1 : -1) : 0;
                c.FillRect(9, y0 + 2 + armSwing, 2, 6, Shirt);
                c.FillRect(21, y0 + 2 - armSwing, 2, 6, ShirtShade);
                c.Set(9, y0 + 1 + armSwing, Skin); c.Set(10, y0 + 1 + armSwing, Skin);
                c.Set(21, y0 + 1 - armSwing, SkinShade); c.Set(22, y0 + 1 - armSwing, SkinShade);

                // Head: face from the front, all hair from behind.
                int hy = y0 + 9;
                c.FillRect(11, hy, 10, 9, facing == Facing.Down ? Skin : Hair);
                c.FillRect(11, hy + 6, 10, 3, Hair);
                if (facing == Facing.Down)
                {
                    c.Set(13, hy + 3, Eye); c.Set(18, hy + 3, Eye);
                    for (int j = hy; j < hy + 6; j++) c.Set(20, j, SkinShade);
                }
            }
            else
            {
                // Drawn facing left; the right-facing frames are a mirror image.
                int frontStep = stepping ? (firstLegForward ? -2 : 1) : 0;
                Leg(c, 14 + frontStep, 1, 3, 6 + bob, Trousers);
                Leg(c, 15 - frontStep, 1, 3, 6 + bob, TrousersShade);
                c.FillRect(12 + frontStep, 1, 3, 1, Shoes); // toe pointing left

                int y0 = 7 + bob;
                c.FillRect(12, y0, 8, 9, Shirt);
                for (int j = y0; j < y0 + 9; j++) { c.Set(12, j, ShirtLight); c.Set(19, j, ShirtShade); }
                int armSwing = stepping ? (firstLegForward ? -1 : 1) : 0;
                c.FillRect(15 + armSwing, y0 + 1, 2, 6, ShirtShade);
                c.Set(15 + armSwing, y0, Skin);

                int hy = y0 + 9;
                c.FillRect(11, hy, 9, 9, Skin);
                c.FillRect(14, hy + 2, 6, 7, Hair);   // hair at the back of the head
                c.FillRect(11, hy + 6, 9, 3, Hair);
                c.Set(12, hy + 3, Eye);
                c.Set(10, hy + 2, Skin);              // nose

                if (facing == Facing.Right) c = Mirror(c);
            }

            AddOutline(c, Outline);
            return c;
        }

        static void Leg(PixelCanvas c, int x, int y, int width, int height, Color32 colour)
        {
            c.FillRect(x, y, width, height, colour);
            c.FillRect(x, y - 1, width, 1, Shoes);
        }

        static PixelCanvas Mirror(PixelCanvas source)
        {
            var m = new PixelCanvas(source.Width, source.Height);
            for (int y = 0; y < source.Height; y++)
                for (int x = 0; x < source.Width; x++)
                    m.Set(source.Width - 1 - x, y, source.Get(x, y));
            return m;
        }

        /// <summary>Draws a one-pixel dark outline around every shape, as the art style guide asks.</summary>
        static void AddOutline(PixelCanvas c, Color32 colour)
        {
            var solid = new bool[c.Width * c.Height];
            for (int i = 0; i < solid.Length; i++) solid[i] = c.Pixels[i].a > 0;

            bool IsSolid(int x, int y) => x >= 0 && y >= 0 && x < c.Width && y < c.Height && solid[y * c.Width + x];

            for (int y = 0; y < c.Height; y++)
                for (int x = 0; x < c.Width; x++)
                    if (!IsSolid(x, y) && (IsSolid(x - 1, y) || IsSolid(x + 1, y) || IsSolid(x, y - 1) || IsSolid(x, y + 1)))
                        c.Set(x, y, colour);
        }

        // ------------------------------------------------------------------ objects

        static readonly Color32 SealRed = PixelCanvas.Hex("#b83a2e");
        static readonly Color32 DoorRed = PixelCanvas.Hex("#8c3b2f");
        static readonly Color32 DoorRedLight = PixelCanvas.Hex("#a84a3c");
        static readonly Color32 DoorRedDark = PixelCanvas.Hex("#6b2a21");
        static readonly Color32 Glass = PixelCanvas.Hex("#9cc4d4");
        static readonly Color32 GlassLight = PixelCanvas.Hex("#d4ecf2");
        static readonly Color32 Brass = PixelCanvas.Hex("#e0c060");
        static readonly Color32 Step = PixelCanvas.Hex("#9a9184");

        /// <summary>The relative's letter: a cream envelope with a red seal, lying on the ground.</summary>
        static PixelCanvas DrawLetter()
        {
            var c = new PixelCanvas(32, 32);
            int x0 = 7, y0 = 9, w = 18, h = 12;
            c.FillRect(x0, y0, w, h, Paper);
            // The flap: a V from the top corners to the middle.
            for (int i = 0; i < w / 2; i++)
            {
                c.Set(x0 + i, y0 + h - 1 - i / 2, Grout);
                c.Set(x0 + w - 1 - i, y0 + h - 1 - i / 2, Grout);
            }
            c.FillRect(x0 + w / 2 - 1, y0 + h - 6, 2, 2, SealRed);
            for (int i = 1; i < w - 1; i++) c.Set(x0 + i, y0, SlabDark); // shadow along the bottom edge
            AddOutline(c, Grout);
            return c;
        }

        /// <summary>The back door, drawn over a house wall tile: red panelled door with a small window.</summary>
        static PixelCanvas DrawBackDoor()
        {
            var c = new PixelCanvas(32, 32);
            c.FillRect(5, 2, 22, 30, WoodDark);          // frame
            c.FillRect(7, 2, 18, 28, DoorRed);           // door
            for (int j = 2; j < 30; j++) { c.Set(7, j, DoorRedLight); c.Set(24, j, DoorRedDark); }
            c.FillRect(10, 19, 12, 8, WoodDark);         // window frame
            c.FillRect(11, 20, 4, 3, Glass); c.FillRect(17, 20, 4, 3, Glass);
            c.FillRect(11, 24, 4, 2, Glass); c.FillRect(17, 24, 4, 2, Glass);
            c.Set(11, 22, GlassLight); c.Set(17, 22, GlassLight); c.Set(11, 25, GlassLight); c.Set(17, 25, GlassLight);
            c.Outline(10, 5, 12, 11, DoorRedDark);       // lower panel
            c.FillRect(21, 13, 2, 2, Brass);             // handle
            c.FillRect(3, 0, 26, 2, Step);               // step
            for (int i = 3; i < 29; i++) c.Set(i, 1, SidingLight);
            return c;
        }

        // ------------------------------------------------------------------ overgrowth and rubbish

        static readonly Color32 FlowerYellow = PixelCanvas.Hex("#f2c94c");
        static readonly Color32 CanRed = PixelCanvas.Hex("#c0392b");
        static readonly Color32 CanSilver = PixelCanvas.Hex("#b8bec4");
        static readonly Color32 CanSilverDark = PixelCanvas.Hex("#868d94");
        static readonly Color32 Terracotta = PixelCanvas.Hex("#c06a3e");
        static readonly Color32 TerracottaDark = PixelCanvas.Hex("#8f4a2a");
        static readonly Color32 TerracottaLight = PixelCanvas.Hex("#d98a5e");

        /// <summary>
        /// A weed in three sizes (0 small, 1 medium, 2 large), drawn like a dandelion: a flat
        /// rosette of jagged leaves spreading out over the slab, seen from the three-quarter view.
        /// Bigger weeds take more pulls, so each size is clearly bigger: the medium one adds a few
        /// grass blades and the large one a yellow flower on a stalk.
        /// </summary>
        static PixelCanvas DrawWeed(int size)
        {
            var c = new PixelCanvas(32, 32);
            var centre = new Vector2(16f, 12f);

            // Leaf directions in degrees (0 = right, 90 = away from the viewer).
            float[][] angles =
            {
                new[] { 15f, 100f, 165f, 215f, 325f },
                new[] { 5f, 60f, 115f, 170f, 210f, 270f, 330f },
                new[] { 0f, 45f, 90f, 135f, 180f, 215f, 255f, 295f, 335f },
            };
            float length = 6f + size * 3f;     // 6, 9 or 12 pixels
            float width = 1.8f + size * 0.6f;  // half-width at the widest point

            // Draw the leaves pointing away first, so the ones pointing towards the viewer overlap them.
            var order = (float[])angles[size].Clone();
            System.Array.Sort(order, (a, b) => Mathf.Sin(b * Mathf.Deg2Rad).CompareTo(Mathf.Sin(a * Mathf.Deg2Rad)));
            foreach (float angle in order) Leaf(c, centre, angle, length, width);

            if (size >= 1)
            {
                // A few grass blades poking up through the leaves.
                foreach (int x in new[] { 13, 18, 20 })
                    for (int j = 0; j < 4 + size; j++) c.Set(x + (j > 3 ? 1 : 0), 13 + j, j < 2 ? GrassMid : GrassPale);
            }

            if (size == 2)
            {
                // A dandelion flower on a slightly bent stalk.
                for (int j = 0; j < 10; j++) c.Set(16 + (j > 6 ? 1 : 0), 13 + j, GrassDark);
                c.FillRect(15, 23, 5, 3, FlowerYellow);
                c.FillRect(16, 26, 3, 1, FlowerYellow);
                c.Set(17, 24, Brass);
            }

            AddOutline(c, Outline);
            return c;
        }

        /// <summary>
        /// One jagged leaf lying on the ground, from the centre outwards. Up-and-down distances are
        /// squashed to suggest the three-quarter view; the edge facing the top-left light is paler.
        /// </summary>
        static void Leaf(PixelCanvas c, Vector2 centre, float angleDegrees, float length, float halfWidth)
        {
            const float squash = 0.55f;
            float a = angleDegrees * Mathf.Deg2Rad;
            var along = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * squash);
            var across = new Vector2(-Mathf.Sin(a), Mathf.Cos(a) * squash).normalized;
            bool acrossFacesLight = Vector2.Dot(across, new Vector2(-1f, 1f)) > 0f;

            for (float t = 0.5f; t <= length; t += 0.5f)
            {
                // Widest in the middle, pointed at the tip, with notches along the edge like a dandelion leaf.
                float w = halfWidth * Mathf.Sin(Mathf.PI * Mathf.Min(1f, t / length * 1.15f));
                if ((int)(t * 2f) % 5 == 0) w *= 0.45f;

                for (float s = -w; s <= w; s += 0.5f)
                {
                    var p = centre + along * t + across * s;
                    Color32 colour;
                    if (Mathf.Abs(s) < 0.5f && t > 1.5f) colour = GrassDark;          // the middle vein
                    else if ((s > 0f) == acrossFacesLight) colour = GrassLight;       // the lit half
                    else colour = GrassMid;
                    c.Set(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), colour);
                }
            }
        }

        /// <summary>A black trash bag slumped on its side.</summary>
        static PixelCanvas DrawTrashBagOnGround()
        {
            var c = new PixelCanvas(32, 32);
            c.FillRect(7, 7, 16, 11, BagMid);
            c.FillRect(9, 18, 12, 2, BagMid);
            c.FillRect(23, 10, 3, 4, BagDark);                   // tied neck
            c.FillRect(26, 9, 2, 2, BagMid); c.FillRect(26, 13, 2, 2, BagMid);
            for (int i = 9; i < 20; i++) c.Set(i, 17, BagLight); // shine along the top
            for (int i = 8; i < 23; i++) c.Set(i, 7, BagDark);
            AddOutline(c, Outline);
            return c;
        }

        /// <summary>Two dropped drink cans, one red and one silver.</summary>
        static PixelCanvas DrawRubbishCans()
        {
            var c = new PixelCanvas(32, 32);
            // A red can lying down.
            c.FillRect(6, 10, 11, 6, CanRed);
            c.FillRect(17, 10, 2, 6, CanSilver);
            for (int i = 6; i < 17; i++) c.Set(i, 15, PixelCanvas.Hex("#e0604f"));
            // A crushed silver can standing up.
            c.FillRect(21, 9, 6, 10, CanSilver);
            for (int j = 9; j < 19; j++) c.Set(26, j, CanSilverDark);
            c.Set(22, 13, CanSilverDark); c.Set(23, 14, CanSilverDark); c.Set(24, 13, CanSilverDark);
            AddOutline(c, Outline);
            return c;
        }

        /// <summary>A cracked terracotta plant pot tipped on its side, soil spilling out, with a broken piece nearby.</summary>
        static PixelCanvas DrawRubbishPotBroken()
        {
            var c = new PixelCanvas(32, 32);
            // The pot lies with its base on the left and its open mouth on the right, widening towards the mouth.
            for (int x = 6; x <= 18; x++)
            {
                int half = 4 + (x - 6) / 5;                      // 4 at the base, 6 at the mouth
                for (int y = 14 - half; y <= 14 + half; y++) c.Set(x, y, Terracotta);
                c.Set(x, 14 + half, TerracottaLight);            // lit along the top
                c.Set(x, 14 - half, TerracottaDark);             // shaded underneath
            }
            c.FillRect(19, 7, 3, 15, TerracottaLight);           // the thick rim round the mouth
            for (int y = 7; y < 22; y++) c.Set(21, y, TerracottaDark);
            c.FillRect(17, 10, 2, 9, DirtDark);                  // dark soil just inside the mouth
            // Soil spilling out onto the ground.
            c.FillRect(22, 8, 4, 4, DirtMid);
            c.FillRect(26, 8, 3, 2, DirtMid);
            c.Set(23, 11, DirtLight); c.Set(27, 9, DirtLight); c.Set(24, 8, DirtDark);
            // A crack across the pot.
            int[] crackY = { 18, 17, 17, 16, 15, 15, 14 };
            for (int i = 0; i < crackY.Length; i++) c.Set(9 + i, crackY[i], TerracottaDark);
            // A curved broken piece in front.
            c.FillRect(9, 4, 5, 2, Terracotta);
            c.Set(8, 5, Terracotta); c.Set(14, 5, Terracotta);
            for (int i = 9; i < 14; i++) c.Set(i, 5, TerracottaLight);
            AddOutline(c, Outline);
            return c;
        }

        // ------------------------------------------------------------------ UI in the world

        /// <summary>Corner brackets and a faint wash marking the tile a tool will work on.</summary>
        static PixelCanvas DrawTileHighlight()
        {
            var c = new PixelCanvas(32, 32);
            c.FillRect(1, 1, 30, 30, new Color32(0xff, 0xf4, 0xd6, 0x30));
            var edge = PixelCanvas.Hex("#fff4d6");
            const int len = 8;
            foreach (int x in new[] { 0, 30 })
                foreach (int y in new[] { 0, 30 })
                {
                    int hx = x == 0 ? 0 : 32 - len, vy = y == 0 ? 0 : 32 - len;
                    c.FillRect(hx, y, len, 2, edge);   // horizontal arm
                    c.FillRect(x, vy, 2, len, edge);   // vertical arm
                }
            return c;
        }

        // ------------------------------------------------------------------ tools

        static readonly Color32 GloveMid = PixelCanvas.Hex("#d9a441");
        static readonly Color32 GloveLight = PixelCanvas.Hex("#f0c868");
        static readonly Color32 GloveDark = PixelCanvas.Hex("#a8782a");
        static readonly Color32 BagMid = PixelCanvas.Hex("#3c4048");
        static readonly Color32 BagLight = PixelCanvas.Hex("#5e646e");
        static readonly Color32 BagDark = PixelCanvas.Hex("#262930");

        /// <summary>A yellow work glove, fingers up, with a darker cuff.</summary>
        static PixelCanvas DrawGloves()
        {
            var c = new PixelCanvas(32, 32);
            c.FillRect(10, 4, 12, 5, GloveDark);                 // cuff
            c.FillRect(10, 9, 12, 9, GloveMid);                  // palm
            for (int f = 0; f < 4; f++) c.FillRect(10 + f * 3, 18, 2, 6 + (f == 1 || f == 2 ? 2 : 0), GloveMid); // fingers
            c.FillRect(22, 10, 3, 6, GloveMid);                  // thumb
            for (int j = 9; j < 18; j++) c.Set(10, j, GloveLight); // lit from the top left
            for (int f = 0; f < 4; f++) c.Set(10 + f * 3, 22, GloveLight);
            AddOutline(c, Outline);
            return c;
        }

        /// <summary>A full black trash bag with a tied top.</summary>
        static PixelCanvas DrawTrashBag()
        {
            var c = new PixelCanvas(32, 32);
            c.FillRect(8, 3, 16, 14, BagMid);                    // body
            c.FillRect(10, 17, 12, 3, BagMid);                   // shoulders
            c.FillRect(14, 20, 4, 3, BagDark);                   // neck
            c.FillRect(12, 23, 3, 3, BagMid); c.FillRect(17, 23, 3, 3, BagMid); // tie ears
            for (int j = 5; j < 16; j++) c.Set(10, j, BagLight); // shine on the top left
            c.Set(11, 16, BagLight); c.Set(12, 17, BagLight);
            for (int i = 9; i < 23; i++) c.Set(i, 3, BagDark);   // shadow along the bottom
            AddOutline(c, Outline);
            return c;
        }

        /// <summary>A wire brush lying on its side: wooden handle on the left, bristles underneath the head.</summary>
        static PixelCanvas DrawWireBrush()
        {
            var c = new PixelCanvas(32, 32);
            c.FillRect(3, 15, 14, 4, WoodMid);                   // handle
            for (int i = 3; i < 17; i++) c.Set(i, 18, WoodLight); // lit from the top
            c.FillRect(16, 13, 13, 7, WoodDark);                 // head
            for (int i = 16; i < 29; i++) c.Set(i, 19, WoodMid);
            for (int i = 17; i < 29; i += 2)                     // steel bristles
                c.FillRect(i, 8, 1, 5, i % 4 == 1 ? CanSilver : CanSilverDark);
            AddOutline(c, Outline);
            return c;
        }

        // ------------------------------------------------------------------ equipment

        static readonly Color32 KettleBlack = PixelCanvas.Hex("#2f3236");
        static readonly Color32 KettleMid = PixelCanvas.Hex("#454a51");
        static readonly Color32 KettleLight = PixelCanvas.Hex("#6c737c");
        static readonly Color32 Charcoal = PixelCanvas.Hex("#1e1f22");
        static readonly Color32 Ash = PixelCanvas.Hex("#5e5a55");
        static readonly Color32 GrateLight = PixelCanvas.Hex("#d6dade");
        static readonly Color32 Rust = PixelCanvas.Hex("#8a4b2a");
        static readonly Color32 RustLight = PixelCanvas.Hex("#a8643a");
        static readonly Color32 GrimeBrown = PixelCanvas.Hex("#4a3524");
        static readonly Color32 GrimeBlack = PixelCanvas.Hex("#2a211b");

        static readonly Color32 KettleTop = PixelCanvas.Hex("#5a6068");

        /// <summary>
        /// The relative's kettle grill in the three-quarter view (design.md 7.1): the top of the
        /// lid is seen from above as a wide oval, with the wooden handle and the vent on it, and
        /// below it the front of the bowl curves down to three legs. Lit from the top left. The
        /// dirty one has rust and old grease on it.
        /// </summary>
        static PixelCanvas DrawKettleGrill(bool dirty)
        {
            var c = new PixelCanvas(32, 32);
            // Three legs splaying out, the back one shorter because it stands further away.
            c.FillRect(8, 1, 2, 9, KettleBlack);
            c.FillRect(22, 1, 2, 9, KettleBlack);
            c.FillRect(15, 3, 2, 6, KettleBlack);
            c.FillEllipse(16f, 6f, 6f, 1.5f, Ash);               // ash catcher between the legs

            // The kettle's outline: a squashed sphere, wider than it is tall from this angle.
            c.FillEllipse(16f, 16f, 11f, 9f, KettleMid);
            // The front of the bowl darkens towards the bottom, away from the light.
            for (int y = 7; y < 12; y++)
                for (int x = 0; x < 32; x++)
                    if (c.Get(x, y).Equals(KettleMid)) c.Set(x, y, KettleBlack);

            // The top of the lid, seen from above.
            c.FillEllipse(16f, 19.5f, 10f, 5f, KettleTop);
            c.FillEllipse(12f, 21.5f, 3f, 1.2f, KettleLight);     // shine on its top left

            // Where the lid meets the bowl: an arc curving down at the front, as a ring seen
            // from above does. A lighter pixel above it catches the lid's edge.
            for (int x = 6; x <= 26; x++)
            {
                float t = (x + 0.5f - 16f) / 11f;
                int y = Mathf.RoundToInt(15.5f - 3f * Mathf.Sqrt(Mathf.Max(0f, 1f - t * t)));
                c.Set(x, y, KettleBlack);
                if (x < 16) c.Set(x, y + 1, KettleLight);
            }

            c.FillRect(13, 19, 6, 1, KettleBlack);                // shadow under the handle
            c.FillRect(13, 20, 6, 2, WoodMid);                    // handle, lying across the lid
            c.FillRect(13, 21, 6, 1, WoodLight);
            c.FillRect(20, 22, 2, 1, KettleBlack);                // lid vent

            if (dirty)
            {
                // Scattered rust and grease spots, the same every build.
                for (int y = 7; y < 26; y++)
                    for (int x = 4; x < 28; x++)
                    {
                        var p = c.Get(x, y);
                        if (p.a == 0 || (p.r == WoodMid.r && p.g == WoodMid.g)) continue;
                        int roll = Hash(x, y, 31) % 100;
                        if (roll < 10) c.Set(x, y, Rust);
                        else if (roll < 14) c.Set(x, y, RustLight);
                        else if (roll < 24) c.Set(x, y, GrimeBrown);
                    }
            }
            AddOutline(c, Outline);
            return c;
        }

        // The close-up grill's shape in the three-quarter view, shared by the grate and its grime
        // so they line up: a round kettle seen from above and in front, so its circles become
        // ovals two thirds as tall as they are wide, with the front of the bowl showing below.
        const int CloseUpSize = 128;
        const float GrillCentreX = 64f, GrillCentreY = 78f;     // centre of the rim's oval
        const float RimRadiusX = 63f, RimRadiusY = 42f;         // outside of the rim
        const float BowlRadiusX = 58f, BowlRadiusY = 38.5f;     // inside the bowl, where grime sits
        const float GrateRadiusX = 54f, GrateRadiusY = 36f;     // the cooking grate
        const float BodyRadiusY = 74f;                          // how far the bowl's front curves down

        /// <summary>
        /// True for pixels on the outside of the bowl below the rim: the lower half of a tall oval,
        /// where it shows under the rim. This is the close-up's "bottom" section.
        /// </summary>
        static bool IsBowlFront(int x, int y) =>
            y + 0.5f < GrillCentreY
            && OvalDistance(x, y, RimRadiusX, BodyRadiusY) <= 1f
            && OvalDistance(x, y, RimRadiusX, RimRadiusY) > 1f;

        /// <summary>How far a pixel is from the grill's centre, as a share of an oval's size (1 = on its edge).</summary>
        static float OvalDistance(int x, int y, float radiusX, float radiusY)
        {
            float dx = (x + 0.5f - GrillCentreX) / radiusX, dy = (y + 0.5f - GrillCentreY) / radiusY;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// The kettle grill in the close-up, in the same three-quarter view as everything else
        /// (design.md 7.2): the cooking grate and the black bowl's rim are seen from above as wide
        /// ovals, with charcoal and ash under the grate, and the front of the bowl curves down
        /// below. The grate's bars run from front to back. Lit from the top left. This is the
        /// clean look; the grime layer is drawn over it.
        /// </summary>
        static PixelCanvas DrawKettleGrate()
        {
            var c = new PixelCanvas(CloseUpSize, CloseUpSize);
            for (int y = 0; y < CloseUpSize; y++)
                for (int x = 0; x < CloseUpSize; x++)
                {
                    float dx = x + 0.5f - GrillCentreX;
                    bool rim = OvalDistance(x, y, RimRadiusX, RimRadiusY) <= 1f;

                    if (IsBowlFront(x, y))
                    {
                        // Lit on the left, darker on the right and around the curve at the bottom.
                        var colour = dx < -RimRadiusX * 0.55f ? KettleLight : KettleMid;
                        if (OvalDistance(x, y, RimRadiusX, BodyRadiusY) > 0.88f || dx > RimRadiusX * 0.6f) colour = KettleBlack;
                        c.Set(x, y, colour);
                    }
                    else if (rim)
                    {
                        float bowl = OvalDistance(x, y, BowlRadiusX, BowlRadiusY);
                        if (bowl > 1f)
                            // The rim: lit along its back left, where the light falls on it.
                            c.Set(x, y, y + 0.5f > GrillCentreY && dx < 0 ? KettleLight : KettleMid);
                        else
                            // Inside the bowl: charcoal with patches of ash and old coals.
                            c.Set(x, y, Hash(x / 2, y / 2, 41) % 100 < 18 ? Ash : Charcoal);
                    }
                }

            // The grate: a steel ring with bars running front to back, 6 pixels apart.
            for (int y = 0; y < CloseUpSize; y++)
                for (int x = 0; x < CloseUpSize; x++)
                {
                    float d = OvalDistance(x, y, GrateRadiusX, GrateRadiusY);
                    if (d > 1f) continue;
                    bool ring = d > 1f - 2.5f / GrateRadiusY;
                    int column = (x - 4) % 6;
                    if (ring) c.Set(x, y, y + 0.5f > GrillCentreY ? GrateLight : CanSilver);
                    else if (column == 0) c.Set(x, y, GrateLight);       // lit from the left
                    else if (column == 1) c.Set(x, y, CanSilverDark);    // shadow side of each bar
                }

            AddOutline(c, Outline);
            return c;
        }

        /// <summary>
        /// Years of grease, soot and rust over the inside of the bowl and the grate, in blotchy
        /// 4 x 4 pixel patches so it reads as grime rather than noise. Everything else, including
        /// the rim and the front of the bowl, is see-through, so it starts clean and does not count
        /// towards the progress meter.
        /// </summary>
        static PixelCanvas DrawKettleGrateGrime()
        {
            var c = new PixelCanvas(CloseUpSize, CloseUpSize);
            for (int y = 0; y < CloseUpSize; y++)
                for (int x = 0; x < CloseUpSize; x++)
                {
                    if (OvalDistance(x, y, BowlRadiusX, BowlRadiusY) > 1f) continue;

                    int patch = Hash(x / 4, y / 4, 51) % 100;
                    int speck = Hash(x, y, 52) % 100;
                    Color32 colour = patch < 45 ? GrimeBrown : patch < 80 ? GrimeBlack : Rust;
                    if (speck < 8) colour = RustLight;
                    // Mostly solid, with some thinner patches where a little steel shows through.
                    colour.a = (byte)(patch % 5 == 0 ? 190 : 245);
                    c.Set(x, y, colour);
                }
            return c;
        }

        /// <summary>
        /// Grease and soot on the outside of the bowl, below the rim: the close-up's bottom section.
        /// The patches are tall and thin, like old drips running down the kettle. Everything else is
        /// see-through, so it belongs to the top section or starts clean.
        /// </summary>
        static PixelCanvas DrawKettleBowlGrime()
        {
            var c = new PixelCanvas(CloseUpSize, CloseUpSize);
            for (int y = 0; y < CloseUpSize; y++)
                for (int x = 0; x < CloseUpSize; x++)
                {
                    if (!IsBowlFront(x, y)) continue;

                    int drip = Hash(x / 3, y / 9, 61) % 100;
                    int speck = Hash(x, y, 62) % 100;
                    Color32 colour = drip < 50 ? GrimeBlack : drip < 85 ? GrimeBrown : Rust;
                    if (speck < 6) colour = RustLight;
                    colour.a = (byte)(drip % 4 == 0 ? 180 : 240);
                    c.Set(x, y, colour);
                }
            return c;
        }

        // ------------------------------------------------------------------ effects

        static readonly Color32 SparkleWhite = PixelCanvas.Hex("#ffffff");
        static readonly Color32 SparkleYellow = PixelCanvas.Hex("#fff2a8");

        /// <summary>
        /// One frame of the cleaning sparkle: a four-pointed star that grows from a dot (frame 0)
        /// to full size (frame 2) and shrinks again (frame 3). White in the middle, pale gold
        /// towards the tips.
        /// </summary>
        static PixelCanvas DrawSparkle(int frame)
        {
            var c = new PixelCanvas(32, 32);
            int[] armLengths = { 2, 6, 11, 6 };
            int arm = armLengths[Mathf.Clamp(frame, 0, armLengths.Length - 1)];
            const int centre = 16;

            for (int i = -arm; i <= arm; i++)
            {
                int distance = Mathf.Abs(i);
                var colour = distance <= arm / 3 ? SparkleWhite : distance < arm ? SparkleYellow : Brass;
                c.Set(centre + i, centre, colour);
                c.Set(centre, centre + i, colour);
            }
            if (frame >= 1)
            {
                // A small cross in the middle makes the star look brighter, and short diagonals
                // on the biggest frame make it twinkle.
                c.FillRect(centre - 1, centre - 1, 3, 3, SparkleWhite);
                if (frame == 2)
                    for (int i = 2; i <= 4; i++)
                    {
                        c.Set(centre + i, centre + i, SparkleYellow); c.Set(centre - i, centre + i, SparkleYellow);
                        c.Set(centre + i, centre - i, SparkleYellow); c.Set(centre - i, centre - i, SparkleYellow);
                    }
            }
            return c;
        }

        // ------------------------------------------------------------------ house

        static PixelCanvas DrawHouseWall()
        {
            var c = new PixelCanvas(32, 32);
            c.Fill(SidingMid);
            // Horizontal clapboard siding: 8 pixel boards with a shadow under each overlap.
            for (int y = 0; y < 32; y += 8)
                for (int x = 0; x < 32; x++)
                {
                    c.Set(x, y, SidingShadow);
                    c.Set(x, y + 7, SidingLight);
                }
            return c;
        }
    }
}
