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

        // ----- UI frames (9-sliced)
        public const string UiPanelDark = Root + "UI/ui_panel_dark.png";
        public const string UiPanelPaper = Root + "UI/ui_panel_paper.png";

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

            Frame(UiPanelDark, PanelDark, PanelEdge);
            Frame(UiPanelPaper, Paper, Grout);

            foreach (Facing facing in System.Enum.GetValues(typeof(Facing)))
                for (int frame = 0; frame < WalkFrames; frame++)
                {
                    string path = PlayerFrame(facing, frame);
                    if (PlaceholderAssetWriter.SavePngIfMissing(path, DrawPlayer(facing, frame)))
                        PlaceholderAssetWriter.ConfigureSprite(path, SpriteAlignment.BottomCenter);
                }
        }

        static void Tile(string path, System.Func<PixelCanvas> draw)
        {
            if (PlaceholderAssetWriter.SavePngIfMissing(path, draw()))
                PlaceholderAssetWriter.ConfigureSprite(path, SpriteAlignment.Center);
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
